using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CruNetworkDLL
{
    /// <summary>
    ///  + 클라이언트에게 보낼 Event를 설정하시려면, ClientManager를 가져와 그 곳에 Event를 설정하세요.
    /// </summary>
    public class CruNetworkServer
    {
        // Property
        public Socket Listener { get { return _listener; } set { _listener = value; } }
        public int Port { get { return _port; } set { _port = value; } }
        public int WaitCapacity { get { return _waitCapacity; } set { _waitCapacity = value; } }
        public bool IsContinueAccept { set { _isContinueAccept = value; } }

        public ClientManager GetClientManager() { return _clientManager; }
        public string Ip { get { return _ip; } set { _ip = value; } }

        // Client가 성공적으로 연결되었을 때, 실행되는 공통 delegate event
        public delegate void OnClientConnectComplete(object token);
        private OnClientConnectComplete clientConnectCompleteEvent;
        
        // 서버가 처리하는 event
        public delegate void OnAcceptComplete(object sender, SocketAsyncEventArgs e);
        private OnAcceptComplete acceptCompleteEvent;
        public delegate void OnReceiveComplete(object sender, SocketAsyncEventArgs e);
        private OnReceiveComplete receiveCompleteEvent;
        public delegate void OnSendComplete(object sender, SocketAsyncEventArgs e);
        private OnSendComplete sendCompleteEvent;

        private Socket _listener;
        private string _ip;
        private int _port;
        private int _waitCapacity;

        private ClientManager _clientManager;   // 전체적인 클라이언트 관리

        // 통신 이벤트
        private SocketAsyncEventArgs _acceptEvent;
        private SocketAsyncEventArgs _receiveEvent;
        private SocketAsyncEventArgs _sendEvent;

        private AutoResetEvent _acceptEventControl;

        private bool _isContinueAccept; // 계속해서 Accept할거냐

        public CruNetworkServer(string address, int port, int waitCapacity, bool isContinueAccept, int clientCapacity, int emptyPoolCnt)
        {
            _ip = address;
            _port = port;
            _waitCapacity = waitCapacity;
            _isContinueAccept = isContinueAccept;

            isContinueAccept = false;
            
            Init(clientCapacity, emptyPoolCnt);
        }

        private void Init(int clientCapacity, int emptyPoolCnt)
        {
            _clientManager = new ClientManager(clientCapacity, emptyPoolCnt);

            _acceptEvent = new SocketAsyncEventArgs();
            _receiveEvent = new SocketAsyncEventArgs();
            _sendEvent = new SocketAsyncEventArgs();

            _acceptEventControl = new AutoResetEvent(false);

            _acceptEvent.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessAccept);
        }

        public void Start()
        {
            IPEndPoint iPEndPoint;

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (_ip == "localhost")
                iPEndPoint = new IPEndPoint(IPAddress.Any, _port);
            else
                iPEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);

            try
            {
                _listener.Bind(iPEndPoint);
                _listener.Listen(_waitCapacity);

                Thread acceptThread = new Thread(StartAccept);
                acceptThread.Start();
            }
            catch (SystemException e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartAccept()
        {
            bool pending;

            while (_isContinueAccept)
            {
                _acceptEvent.AcceptSocket = null;
                pending = false;

                Console.WriteLine("연결을 기다리는 중");

                try
                {
                    pending = _listener.AcceptAsync(_acceptEvent);
                }
                catch (SystemException e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                _acceptEventControl.WaitOne();

                if (pending == false)
                {
                    ProcessAccept(null, _acceptEvent);
                }
                    
            }
        }

        // 범용성을 위해, 내부적으로 어떻게 처리할건지는 밖에서 추가해주는 것으로 결정.
        private void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success) {
                Console.WriteLine("[Server]Success to Accept");

                // 새로운 클라이언트 연결.
                // 흠... 이부분을 자동으로 연결해줘야 하나... 아니면 내가 알아서 추가할 수 있도록 해야하나...
                //_clientManager.CreateNewClient(e.AcceptSocket, null, null);

                if(acceptCompleteEvent != null)
                    acceptCompleteEvent(null, e);
            }

            _acceptEventControl.Set();
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if(e.LastOperation == SocketAsyncOperation.Receive)
            {
                if(e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    Console.WriteLine("Success to Receive");

                    //if (receiveCompleteEvent != null)
                    //    receiveCompleteEvent(null, e);
                }
            }
        }

        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError != SocketError.Success || e.BytesTransferred <= 0){
                Console.WriteLine("Send 실패");
                return;
            }

            //if (sendCompleteEvent != null)
            //    sendCompleteEvent(null, e);

            Console.WriteLine("Send 성공");
        }

        // Add Event
        public bool AddAcceptEvent(OnAcceptComplete eventvalue)
        {
            if (eventvalue == null)
                return false;

            //_acceptEvent.Completed += eventvalue;
            acceptCompleteEvent += eventvalue;

            return true;
        }
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
        public bool RemoveAccepEvent(OnAcceptComplete eventvalue)
        {
            if (eventvalue == null)
                return false;

            acceptCompleteEvent -= eventvalue;

            return true;
        }
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

        public void ShowClientCount()
        {
            Console.WriteLine(_clientManager.ClientCount);
        }
    }
}
