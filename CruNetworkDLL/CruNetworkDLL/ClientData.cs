using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace CruNetworkDLL
{
    public enum CLIENT_STATE { LOGIN, LOBI, PLAYING };

    /// <summary>
    /// Network 단계에서 사용되는 Client 정보. 
    /// 한 마디로 그냥 연결 통로다.
    /// 클라이언트 Send, Receive 동기? 비동기?
    /// 내 생각에는 클라이언트는 동기적으로 처리해야 맞는 것 같다.
    /// [Send] Send -> sendCompleteEvent
    /// [Receive] Receive -> receiveCompleteEvent
    /// </summary>
    public class ClientData
    {
        public Socket ClientSocket { get { return _client; } set { if (_client == null) _client = value; } }
        public CLIENT_STATE State { get { return _state; } set { _state = value; } }
        public bool IsConnect { get { return _isConnect; } }
        public int ClientNum { get { return _clientNum; } }

        // 클라이언트마다 공통적으로 실행되는 이벤트
        public delegate void OnSendCompleteCommonEvent(object datas);
        public OnSendCompleteCommonEvent sendCompleteCommonEvent;
        public delegate void OnReceiveCompleteCommonEvent(CruMessage[] datas, int clientNum);
        public OnReceiveCompleteCommonEvent receiveCompleteCommonEvent;
        public delegate void OnDisConnectEvent(int clientNum);
        public OnDisConnectEvent disconnectEvent;

        private Socket _client;
        private CLIENT_STATE _state;
        private int _clientNum;
        private CruMessageQueue _sendQueue;
        private CruMessageTransfer _transfer;   // 데이터 치환기.
        private byte[] _unicastBuffer;
        private byte[] _sendBuffer;
        private byte[] _receiveBuffer;
        private int _unicastDataSize;
        private int _sendDataSize;
        private int _sendDataCount;
        private int _receiveDataSize;
        private int _headSize;
        private bool _isConnect;
        // Thread
        private Thread _sending;
        private Thread _receiving;
        // Thread 관리
        private AutoResetEvent _receiveEventControl;
        private AutoResetEvent _sendEventControl;

        public ClientData(Socket socket, int clientNum)
        {
            _client = socket;
            _clientNum = clientNum;
            _unicastBuffer = new byte[1024];
            _sendBuffer = new byte[1024];
            _receiveBuffer = new byte[1024];
            _unicastDataSize = 0;
            _sendDataSize = 0;
            _sendDataCount = 0;
            _receiveDataSize = 0;
            _headSize = CruNetworkProtocol.Head.HEADSIZE_DATACOUNT;

            _isConnect = false;

            _sendQueue = new CruMessageQueue(10);

            _transfer = new CruMessageTransfer();
            _receiveEventControl = new AutoResetEvent(false);
            _sendEventControl = new AutoResetEvent(false);
        }

        public void Initialize(Socket client, int clientNum)
        {
            _client = client;
            _clientNum = clientNum;
            _isConnect = false;
            //_buffer.SetValue(buffer, 0);
            Array.Clear(_sendBuffer, 0, 1024);
            Array.Clear(_receiveBuffer, 0, 1024);
            _sendQueue.Clear();

            _receiveEventControl.Reset();
            _sendEventControl.Reset();
        }        

        // Thread는 같이 시작해서 같이 끝나야한다.
        public void StartDataTransmit()
        {
            _sending = new Thread(StartSending);
            _receiving = new Thread(StartReceiving);
            _isConnect = true;
            _sending.Start();
            _receiving.Start();
        }

        // 보내는건 데이터가 있을 때만 보낸다.
        // Thread말고 그냥 send 해주면 안되냐고 말할 수 있지만, 모든 클라이언트에 동시에 보내야 하기에, 각자 Thread로 처리해준다.
        public void StartSending()
        {
            Console.WriteLine("기다린다!!!");
            // 처음에는 기다림.
            _sendEventControl.WaitOne();
            Console.WriteLine("보낸다!!!");
            while (_isConnect)
            {
                Console.WriteLine("보낼 데이터!" + _sendDataCount);
                if (_sendDataCount != 0 || _sendQueue.Count != 0)
                {
                    SetUnicastData();
                    SetSendData();
                    Send(_sendBuffer, _sendDataSize);

                    // 버퍼 비우기
                    SendBufferClear();
                }
                
                // 데이터를 보내는 건, 중앙에서 관리해줘야 한다.
                // 일정 단위로 데이터를 보냄. 그게 프레임 단위로 보낼 수도 있고.
                // 중앙 서버에서 프레임별로 풀어주는걸로 하자.
                _sendEventControl.WaitOne();
            }

            Console.WriteLine("Send Thread 종료");
        }

        public void StartReceiving()
        {
            // 데이터를 받는 것은 언제나 열려있어야 한다.
            // 데이터를 받아서 중앙에서 처리해줄 수 있도록 해야함.
            while (_isConnect)
            {
                Receive();

                // 버퍼 비우기
                ReceiveBufferClear();
            }

            Console.WriteLine("Receive Thread 종료");
        }

        // 클라이언트 통신 재연결 때문에, Thread만 종료시키는게 따로 필요할 것 같다.
        // 연결을 끊었다면 true, 이미 연결이 끊겨있다면 false.
        private bool TerminateThread()
        {
            if (_isConnect == false)
                return false;

            _isConnect = false;
            _sendEventControl.Set();

            return true;
        }

        public void StartSendData() {
            _sendEventControl.Set();
        }

        // SendData가 들어오면 Thread를 실행시켜주자.
        // broadcast data와 unicast data를 통합해서 보내주자.
        private void SetSendData()
        {
            if (_sendDataCount == 0)
                return;

            byte[] dataCount = BitConverter.GetBytes((short)_sendDataCount);

            Array.Copy(dataCount, 0, _sendBuffer, 0, _headSize);
            _sendDataSize += _headSize;
        }
        
        // Unicast Data 설정. 해당 클라이언트만 받는 정보.
        // 보내기 직전에 실행된다.
        public void SetUnicastData()
        {
            int count = 0;

            lock (_sendQueue)
            {
                count = _sendQueue.Count;

                if (_sendQueue.Count == 0)
                    return;

                _unicastDataSize = _transfer.MsgsToSendData(_sendQueue.Dequeue(), _unicastBuffer, 0, _unicastBuffer.Length);
                //_unicastDataSize = _transfer.MsgsToByte(_sendQueue.Dequeue(), _unicastBuffer);
            }

            Array.Copy(_unicastBuffer, _headSize, _sendBuffer, _sendDataSize + _headSize, _unicastDataSize - _headSize);
            _sendDataSize += _unicastDataSize - _headSize;
            _sendDataCount += count;
        }

        // Broad Data 설정. 모든 클라이언트가 받는 정보.
        public void SetBroadcastData(byte[] broadcastData, int size, int count)
        {
            if (broadcastData == null)
                throw new ArgumentNullException("parameter is null");

            // Count부분 빼고 복사하기.
            Array.Copy(broadcastData, _headSize, _sendBuffer, _sendDataSize + _headSize, size - _headSize);
            _sendDataSize += size - _headSize;
            _sendDataCount += count;
        }

        public void PushUnicastData(CruMessage msg)
        {
            if (msg == null)
                throw new ArgumentNullException("parameter is null");

            lock (_sendQueue)
            {
                _sendQueue.Enqueue(msg);
            }
        }
        public void PushUnicastData(CruMessage[] msgs)
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
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
                // disconnectEvent(_clientNum);
                return false;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);

                if (TerminateThread() == true)
                    disconnectEvent(_clientNum);

                return false;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e);

                if (TerminateThread() == true)
                    disconnectEvent(_clientNum);

                return false;
            }

            sendCompleteCommonEvent(_clientNum);

            return true;
        }

        // 일단 한 번에 여러 명령이 올 수 있다는 가정하에 작성.
        // Receive 받고, 함수를 통해 정보를 다른 곳으로 보내줘야한다.
        // Receive된 정보를 바꾸는 걸 비동기화로 바꾸면 좋을 듯.
        // Receive된 정보를 해당 쓰레드 말고 중앙으로 줘서 처리할 수 있지만, 그러면 중앙에서 모든 Client에서 온 정보를 처리해야하기에 각각 Thread에서 처리해주는게 더 빠르다.
        public bool Receive()
        {
            CruMessage[] temp;
            try
            {
                _receiveDataSize = _client.Receive(_receiveBuffer, 1024, SocketFlags.None);

                if (_receiveDataSize == 0)
                {
                    throw new SocketException();
                }

                // temp = _transfer.ByteToMsgs(_receiveBuffer, 0, _receiveDataSize);
                temp = _transfer.ReceiveByteToMsgs(_receiveBuffer, _receiveDataSize);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);

                if (TerminateThread() == true)
                {
                    if(disconnectEvent != null)
                        disconnectEvent(_clientNum);
                }

                return false;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e);

                if (TerminateThread() == true)
                {
                    if (disconnectEvent != null)
                        disconnectEvent(_clientNum);
                }

                return false;
            }

            receiveCompleteCommonEvent(temp, _clientNum);

            return true;
        }

        private void SendBufferClear()
        {
            Array.Clear(_unicastBuffer, 0, _unicastBuffer.Length);
            Array.Clear(_sendBuffer, 0, _sendBuffer.Length);

            _unicastDataSize = 0;
            _sendDataSize = 0;
            _sendDataCount = 0;
        }

        private void ReceiveBufferClear()
        {
            Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
            _receiveDataSize = 0;
        }

        public void Dispose()
        {
            // socket이 null이면 이미 dispose가 한 번 실행됐다는 소리.
            if (_client == null)
                return;

            lock (_client)
            {
                _client.Close();
                _client = null;
            }

            TerminateThread();

            _sending.Join();
            _receiving.Join();

            Array.Clear(_sendBuffer, 0, 1024);
            Array.Clear(_receiveBuffer, 0, 1024);

            Console.WriteLine("클라이언트 Dispose");
        }
    }
}
