using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace CruNetworkDLL
{
    /// <summary>
    /// 클라이언트 Send, Receive 동기? 비동기?
    /// 내 생각에는 클라이언트는 동기적으로 처리해야 맞는 것 같다.
    /// [Send] Send -> sendCompleteEvent
    /// [Receive] Receive -> receiveCompleteEvent
    /// </summary>
    class ClientData_Async
    {
        public Socket ClientSocket { get { return _client; } set { if (_client == null) _client = value; } }

        // 클라이언트마다 개인적으로 가지는 이벤트. 필요할까?
        public delegate void OnSendCompleteEvent();
        public OnSendCompleteEvent sendCompleteEvent;
        public delegate void OnReceiveCompleteEvent();
        public OnReceiveCompleteEvent receiveCompleteEvent;

        // 클라이언트마다 공통적으로 실행되는 이벤트
        public delegate void OnSendCompleteCommonEvent(CruMessage[] datas);
        public OnSendCompleteCommonEvent sendCompleteCommonEvent;
        public delegate void OnReceiveCompleteCommonEvent(CruMessage[] datas);
        public OnReceiveCompleteCommonEvent receiveCompleteCommonEvent;

        private Socket _client;
        private byte[] _sendBuffer;
        private byte[] _receiveBuffer;
        private CruMessageQueue _sendQueue;
        private CruMessageTransfer _transfer;   // 데이터 치환기.
        // Thread
        private Thread _sending;
        private Thread _receiving;
        // 비동기일 떄
        private SocketAsyncEventArgs _receiveEvent;
        private SocketAsyncEventArgs _sendEvent;
        // Thread 관리
        private AutoResetEvent _receiveEventControl;
        private AutoResetEvent _sendEventControl;

        public ClientData_Async(Socket socket, SocketAsyncEventArgs receiveEvent, SocketAsyncEventArgs sendEvent)
        {
            _client = socket;
            _sendBuffer = new byte[1024];
            _receiveBuffer = new byte[1024];

            _receiveEvent = receiveEvent;
            _sendEvent = sendEvent;

            _sendQueue = new CruMessageQueue(10);

            _transfer = new CruMessageTransfer();
            _receiveEventControl = new AutoResetEvent(false);
            _sendEventControl = new AutoResetEvent(false);
        }

        public void Initialize(Socket client, SocketAsyncEventArgs receiveEvent, SocketAsyncEventArgs sendEvent)
        {
            _client = client;
            _receiveEvent = receiveEvent;
            _sendEvent = sendEvent;
            //_buffer.SetValue(buffer, 0);
            Array.Clear(_sendBuffer, 0,1024);
            Array.Clear(_receiveBuffer, 0, 1024);
            _sendQueue.Clear();

            _receiveEventControl.Reset();
            _sendEventControl.Reset();
        }

        public void StartDataTransfer()
        {
            _sending = new Thread(StartSending);
            _receiving = new Thread(StartReceiving);
            _sending.Start();
            _receiving.Start();
        }

        // 보내는건 데이터가 있을 때만 보낸다.
        // 
        public void StartSending()
        {
            byte[] buffer = new byte[1024];
            int size = 0;

            while(true)
            {
                // 데이터를 보내는 건, 중앙에서 관리해줘야 한다.
                // 일정 단위로 데이터를 보냄. 그게 프레임 단위로 보낼 수도 있고.
                // 중앙 서버에서 프레임별로 풀어주는걸로 하자.
                _sendEventControl.WaitOne();

                //size = _transfer.MsgsToByte(_sendQueue.Dequeue(), buffer);
                Send(buffer, size);
            }
        }

        public void StartReceiving()
        {
            CruMessage[] receiveMsg = new CruMessage[50];

            // 데이터를 받는 것은 언제나 열려있어야 한다.
            // 데이터를 받아서 중앙에서 처리해줄 수 있도록 해야함.
            while (true)
            {
                Receive();
            }
        }

        // SendData가 들어오면 Thread를 실행시켜주자.
        public void PushSendData(CruMessage[] msg)
        {
            if (msg == null)
                throw new ArgumentNullException("parameter is null");

            lock (_sendQueue)
            {
                _sendQueue.Enqueue(msg);
            }

            _sendEventControl.Set();
        }

        public void PushSendDatas(CruMessage[] msgs)
        {
            if (msgs == null)
                throw new ArgumentNullException("parameter is null");

            lock (_sendQueue)
            {
                _sendQueue.Enqueue(msgs);
            }
        }

        // CruMessageTransfer로 변환된 msg를 전송한다.
        // 밖에서 중앙에서 따로 Send할 이유가 있나?
        // 잠시... 중앙에서 Thread관리하면 따로 내가 Thread만들어 줄 필요가 없지 않나???? 그렇네?
        public bool Send(byte[] msg, int size)
        {
            try
            {
                _client.Send(msg, size, SocketFlags.None);
            }catch(SystemException e)
            {
                Console.WriteLine(e);
                return false;
            }

            sendCompleteCommonEvent(null);

            return true;
        }
        // [비동기] Send
        public bool SendAsync(byte[] msg, int size)
        {
            bool pending = false;

            _sendEvent.SetBuffer(msg, 0, 1024);

            try
            {
                pending = _client.SendAsync(_sendEvent);
            }catch(SystemException e)
            {
                Console.WriteLine(e);

                return false;
            }

            if(pending == false)
            {
                ProcessSend(null, _sendEvent);
            }

            return true;
        }
        // [비동기] Send 후 실행
        public void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
            {
                Console.WriteLine("Send 실패");
                return;
            }

            if (sendCompleteCommonEvent != null)
                sendCompleteCommonEvent(null);
        }

        // 일단 한 번에 여러 명령이 올 수 있다는 가정하에 작성.
        // Receive 받고, 함수를 통해 정보를 다른 곳으로 보내줘야한다.
        // Receive된 정보를 바꾸는 걸 비동기화로 바꾸면 좋을 듯.
        public bool Receive()
        {
            int size;
            CruMessage[] temp = null;
            try
            {
                size = _client.Receive(_receiveBuffer, 1024, SocketFlags.None);
                //temp = _transfer.ByteToMsgs(_receiveBuffer, 0, size);
            }
            catch(SystemException e)
            {
                Console.WriteLine(e);
                return false;
            }

            receiveCompleteCommonEvent(temp);

            return true;
        }
        // [비동기] Receive
        public bool ReceiveAsync(CruMessage[] result)
        {
            bool pending = false;

            try
            {
                pending = _client.ReceiveAsync(_receiveEvent);
            }
            catch (SystemException e)
            {
                Console.WriteLine(e);

                return false;
            }

            if (pending == false)
            {
                ProcessReceive(null, _receiveEvent);
            }

            return true;
        }
        // [비동기] Receive 후 실행
        public void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Console.WriteLine("[Client]Success to Receive");

                    if (receiveCompleteCommonEvent != null)
                        receiveCompleteCommonEvent(null);
                }
            }
        }

        public void Dispose()
        {
            _client.Close();
            _client = null;
            Array.Clear(_sendBuffer, 0, 1024);
            Array.Clear(_receiveBuffer, 0, 1024);
            _receiveEvent.Dispose();
            _sendEvent.Dispose();

        }
    }
}
