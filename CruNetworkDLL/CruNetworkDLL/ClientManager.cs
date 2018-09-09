using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace CruNetworkDLL
{
    /// <summary>
    /// 기능
    ///  + 클라이언트 생성, 삭제
    ///  + send, receive, disconnect 시, 진행될 이벤트를 설정가능.
    ///  + Client들로 받은 명령 데이터 저장.
    ///  + Client들로 보낼 broadcast data 저장
    ///  + Unicast data 각 클라이언트로 전달
    ///  + Broadcast 실행
    /// </summary>
    public class ClientManager
    {
        public int ClientCount { get { return _clientList.Count; } }

        // Client 공통된 통신 이벤트
        public delegate void OnReceiveComplete(CruMessage[] msgs, int clientNum);
        private OnReceiveComplete receiveCompleteEvent;
        public delegate void OnSendComplete(object datas);
        private OnSendComplete sendCompleteEvent;
        public delegate void OnDisConnect(int clientNum);
        private OnDisConnect disConnectEvent;

        private List<ClientData> _clientList;
        private ClientDataPool _clientPool;
        private CruMessageTransfer _messageTransfer;
        private CruMessageQueue _sendQueue; // broadcast를 통해 모든 클라리언트들에게 전송할 데이터
        private CruMessageQueue _receiveQueue;
        private byte[] _sendDataBuffer;
        private int _capacity;

        public ClientManager(int capacity, int emptyPoolCnt)
        {
            _capacity = capacity;

            _clientList = new List<ClientData>();
            _clientPool = new ClientDataPool(capacity);
            _messageTransfer = new CruMessageTransfer();
            _sendQueue = new CruMessageQueue(capacity);
            _receiveQueue = new CruMessageQueue(capacity);
            _sendDataBuffer = new byte[1024];
            _clientPool.SetEmptyData(emptyPoolCnt);   // Pool 미리 생성

            // Event 추가
            receiveCompleteEvent += PushReceiveData;
            disConnectEvent += DisConnectClient;
        }

        /// <summary>
        /// Client를 생성하기 전에 Event 등록을 먼저 해야합니다.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="receiveEvent"></param>
        /// <param name="sendEvent"></param>
        /// <returns></returns>
        public bool CreateNewClient(Socket client, SocketAsyncEventArgs receiveEvent, SocketAsyncEventArgs sendEvent)
        {
            try
            {
                ClientData temp = _clientPool.Pop();
                int clientNum = _clientList.Count;
                //temp.Initialize(client, receiveEvent, sendEvent);
                temp.Initialize(client, clientNum);
                temp.sendCompleteCommonEvent = ProcessSendCompleteEvent;
                temp.receiveCompleteCommonEvent = ProcessReceiveCompleteEvent;
                temp.disconnectEvent = ProcessDisConnectEvent;
                temp.StartDataTransmit();

                _clientList.Add(temp);
            }
            catch
            {
                Console.WriteLine("클라이언트 생성에 실패했습니다.");
                return false;
            }

            return true;
        }

        public void DisConnectClient(int clientNum)
        {
            new Thread(() => DisconnectClientThread(clientNum)).Start();
        }

        // 따로 thread로 만들어서 disconnect해주는 이유는, 소켓에러가 나는 부분이 send나 receive thread이기 때문에, Join으로 두 thread가 정상종료되었는지 확인할 수 없다. 그래서 따로 Thread를 만들어서 정상종료를 확인한다.
        public void DisconnectClientThread(int clientNum)
        {
            ClientData client = _clientList[clientNum];
            client.Dispose();
            _clientList.RemoveAt(clientNum);
            _clientPool.Push(client);
        }

        // 클라이언트틀로부터 받은 데이터 반환.
        public CruMessage[] GetReceiveMessages()
        {
            if (_receiveQueue.Count == 0)
                return null;

            lock (_receiveQueue)
            {
                return _receiveQueue.Dequeue();
            }
        }

        // 클라이언트로부터 온 메시지들을 저장하는 큐
        public void PushReceiveData(CruMessage msg, int clientNum)
        {
            if (msg == null)
                throw new ArgumentNullException("msgs is null");

            lock (_receiveQueue)
            {
                _receiveQueue.Enqueue(msg);
            }
        }
        public void PushReceiveData(CruMessage[] msgs, int clientNum)
        {
            if (msgs == null)
                throw new ArgumentNullException("msgs is null");

            lock (_receiveQueue)
            {
                _receiveQueue.Enqueue(msgs);
            }
        }

        // 클라이언트로 보낼 메시지 큐
        // 큐의 사이즈가 커질 수 있으니... 메시지 통신 사이즈를 늘려야하나?
        // 큐는... 리스트 같은거라 Enqueue랑 Dequeue 동기화 안해줘도 상관없지 않나?
        public void PushBroadcastData(CruMessage msg)
        {
            if (msg == null)
                throw new ArgumentNullException("parameter is null");

            lock (_sendQueue)
            {
                _sendQueue.Enqueue(msg);
            }
        }
        public void PushBroadcastData(CruMessage[] msgs)
        {
            if (msgs == null)
                throw new ArgumentNullException("parameter is null");

            lock (_sendQueue)
            {
                _sendQueue.Enqueue(msgs);
            }
        }

        public void PushUnicastDataToClient(CruMessage msg, int clientNum)
        {
            if (msg == null)
                throw new ArgumentNullException("parameter is null");

            lock (_clientList)
            {
                _clientList[clientNum].PushUnicastData(msg);
            }
        }
        public void PushUnicastDataToClient(CruMessage[] msgs, int clientNum)
        {
            if(msgs == null)
                throw new ArgumentNullException("parameter is null");

            lock (_clientList)
            {
                _clientList[clientNum].PushUnicastData(msgs);
            }
        }

        // 모든 클라이언트로 BroadCast~!~!
        public void BroadCast()
        {
            int msgCount = 0;
            int msgSize = 0;

            lock (_sendQueue)
            {
                msgCount = _sendQueue.Count;

                // 메시지가 있으면 변환해줘야함.
                if (msgCount != 0)
                    msgSize = _messageTransfer.MsgsToSendData(_sendQueue.Dequeue(), _sendDataBuffer, 0, _sendDataBuffer.Length);
                // msgSize = _messageTransfer.MsgsToByte(_sendQueue.Dequeue(), _sendDataBuffer);                    
            }

            lock (_clientList)
            {
                // 보낼 클라이언트가 있는지 확인.
                if (_clientList.Count == 0)
                    return;

                for (int i = 0; i < _clientList.Count; ++i)
                {
                    // 보낼 msg가있다면, 각 클라이언트마다 Broadcast msg 전달.
                    if(msgSize != 0)
                        _clientList[i].SetBroadcastData(_sendDataBuffer, msgSize, msgCount);

                    // Broadcast msg가 전달 되었으면, send 실행.
                    _clientList[i].StartSendData();
                }
            }

            // 데이터 보내고 초기화.
            Array.Clear(_sendDataBuffer, 0, _sendDataBuffer.Length);
        }

        // 다른 class로 delegate 복사가 안되므로 만든 것.
        // 모든 클라이언트가 공통으로 가지는 통신 Event.
        // 소켓 관련 이벤트를 Network.dll 밖에서 다룰 일이 없다. 그래서 제거.
        public void ProcessReceiveCompleteEvent(CruMessage[] msgs, int clientNum)
        {
            receiveCompleteEvent(msgs, clientNum);
        }
        public void ProcessSendCompleteEvent(object msgs)
        {
            sendCompleteEvent(msgs);
        }
        public void ProcessDisConnectEvent(int clientNum)
        {
            DisConnectClient(clientNum);
        }

        // Add Event
        public bool AddReceiveEvent(OnReceiveComplete eventvalue)
        {
            if (eventvalue == null)
                return false;

            receiveCompleteEvent += eventvalue;

            return true;
        }
        public bool AddSendEvent(OnSendComplete eventvalue)
        {
            if (eventvalue == null)
                return false;

            sendCompleteEvent += eventvalue;

            return true;
        }
        // Remove Event
        public bool RemoveReceiveEvent(OnReceiveComplete eventvalue)
        {
            if (eventvalue == null)
                return false;

            receiveCompleteEvent -= eventvalue;

            return true;
        }
        public bool RemoveSendEvent(OnSendComplete eventvalue)
        {
            if (eventvalue == null)
                return false;

            sendCompleteEvent -= eventvalue;

            return true;
        }
    }
}
