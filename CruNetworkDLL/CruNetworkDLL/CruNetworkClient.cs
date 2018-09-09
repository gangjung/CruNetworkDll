using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CruNetworkDLL
{
    public class CruNetworkClient
    {
        //public ClientData Client { get => _client;}

        //// 어떤 데이터를 보냈는지가 필요한가?
        //public delegate void OnSendCompleteEvent(CruMessage[] datas);
        //public OnSendCompleteEvent onSendCompleteEvent;
        //public delegate void OnReceiveCompleteEvent(CruMessage[] datas);
        //public OnReceiveCompleteEvent onReceiveCompleteEvent;

        //private Socket _client;
        //private string _ip;
        //private int _port;

        //private Thread _sending;
        //private Thread _receiving;

        //private AutoResetEvent _receiveEventControl;
        //private AutoResetEvent _sendEventControl;

        public int ClientNum { get { return _client.ClientNum; } }

        public delegate void OnSendCompleteEvent(object datas);
        public OnSendCompleteEvent onSendCompleteEvent;
        public delegate void OnReceiveCompleteEvent(CruMessage[] datas, int clientNum);
        public OnReceiveCompleteEvent onReceiveCompleteEvent;

        private ClientData _client;
        private string _ip;
        private int _port;

        private CruMessageQueue _receiveQueue;
        private CruMessageTransfer _messageTransfer;
        
        public CruNetworkClient(string address = null, int port = 0)
        {
            _ip = address;
            _port = port;

            _receiveQueue = new CruMessageQueue(100);
            _messageTransfer = new CruMessageTransfer();

            Init();
        }

        private void Init()
        {
            onReceiveCompleteEvent += PushToReceiveQueue;
        }

        public Socket Connect(string address, int port)
        {
            IPEndPoint iPEndPoint;

            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (address == "localhost")
                iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            else
                iPEndPoint = new IPEndPoint(IPAddress.Parse(_ip), port);

            try
            {
                client.Connect(iPEndPoint);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                client = null;
            }

            return client;
        }

        // 연결되었다면 token값 할당. 아니라면 token == null
        public int ConnectToLogin(string id, string pw, out string token)
        {
            // todo
            // id, pw를 로그인서버로 검증받아서 token을 받는다.
            // 결과로 온 연결 실패 값에 따라 다른 명령 처리할 것.
            byte[] buffer = new byte[1024];
            int datasize = 0;
            int result = 0;
            token = null;

            CruMessage msg = new CruMessage(CruNetworkProtocol.MSG_TYPE.LOGIN_REQUEST, new object[2] { (string)id, (string)pw });

            // 로그인 요청할 데이터 세팅
            datasize = _messageTransfer.SingleMsgToSendData(msg, buffer, 0, buffer.Length);
            //byte[] buffer = _messageTransfer.DatasToByte(CruNetworkProtocol.MSG_TYPE.LOGIN_REQUEST, datas);
            
            // 로그인 서버 포트로 연결할 것
            Socket client = Connect("localhost", 8080);

            if (client == null)
                return -1;

            // 로그인 정보 전달.
            client.Send(buffer, 0, datasize, SocketFlags.None);
            Array.Clear(buffer, 0, buffer.Length);
            datasize = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            CruMessage[] receivemsg = _messageTransfer.ReceiveByteToMsgs(buffer, datasize);
            
            result = (short)receivemsg[0].Datas[0];

            if (result == 0)
                token = (string)receivemsg[0].Datas[1];

            return result;
        }

        // Login -> Lobi 인지, Game -> Lobi 인지 구분해서 처리해야할 것이다!
        public int ConnectToLobi(string id, string token)
        {
            /* 순서 */
            /* 먼저 아이디와 토큰정보를 받아서  유저 체크를 한다.
             제대로 인증되면, 그 때, ClientData를 통해 주기적으로 통신(Thread 실행)을 받는다. */
            byte[] buffer = new byte[1024];
            int result = 0;
            int datasize = 0;
            CruMessage msg = new CruMessage(CruNetworkProtocol.MSG_TYPE.CHECK_USER, new object[2] { id, token });

            datasize = _messageTransfer.SingleMsgToSendData(msg, buffer, 0, buffer.Length);

            // 로그인 서버 포트로 연결할 것
            Socket client = Connect("localhost", 8080);

            // 로그인 정보 전달.
            client.Send(buffer, 0, datasize, SocketFlags.None);
            Array.Clear(buffer, 0, buffer.Length);
            datasize = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            CruMessage[] receivemsg = _messageTransfer.ReceiveByteToMsgs(buffer, datasize);

            // datas = _messageTransfer.
            result = (short)receivemsg[0].Datas[0];

            // 제대로 연결되었다면
            if (result == 0)
            {
                // 제대로 연결되었기에 ClientData 할당. 아닌가...? 그냥 새로 하나 만들까?
                _client = new ClientData(client, (short)receivemsg[0].Datas[1]);
                // 제대로 사용하려면, 미리 Event를 할당해놓아야 한다.
                _client.sendCompleteCommonEvent = ProcessSend;
                _client.receiveCompleteCommonEvent = ProcessReceive;

                _client.StartDataTransmit();
            }
            else
            {
                // 제대로 연결이 되지 않았을 때 처리.
            }

            return result;
        }

        public int ConnectToGame(string id, string token)
        {
            /* 순서 */
            /* 먼저 아이디와 토큰정보를 받아서  유저 체크를 한다.
             제대로 인증되면, 그 때, ClientData를 통해 주기적으로 통신(Thread 실행)을 받는다. */
            byte[] buffer = new byte[1024];
            int result = 0;
            int datasize = 0;
            CruMessage msg = new CruMessage(CruNetworkProtocol.MSG_TYPE.CHECK_USER, new object[2] { id, token });

            datasize = _messageTransfer.ExtractSendDatas(msg, buffer, 0);

            // 로그인 서버 포트로 연결할 것
            Socket client = Connect("localhost", 0);

            // 로그인 정보 전달.
            client.Send(buffer, 0, datasize, SocketFlags.None);
            Array.Clear(buffer, 0, buffer.Length);
            datasize = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            msg = _messageTransfer.ExtractDatas(CruNetworkProtocol.MSG_TYPE.CHECK_USER, buffer, 0, datasize);

            // datas = _messageTransfer.
            result = (int)msg.Datas[0];

            // 제대로 연결되었다면
            if (result == 1)
            {
                // 제대로 연결되었기에 ClientData 할당. 아닌가...? 그냥 새로 하나 만들까?
                _client = new ClientData(client, (int)msg.Datas[1]);
                // 제대로 사용하려면, 미리 Event를 할당해놓아야 한다.
                _client.sendCompleteCommonEvent = ProcessSend;
                _client.receiveCompleteCommonEvent = ProcessReceive;

                _client.StartDataTransmit();
            }
            else
            {
                // 제대로 연결이 되지 않았을 때 처리.
            }

            return result;
        }

        public void PushMessage(CruMessage msg)
        {
            _client.PushUnicastData(msg);
        }

        public void StartSendData()
        {
            _client.StartSendData();
        }

        /// <summary>
        /// 1. 연결이 끊겼을 때
        /// 2. 다른 서버로 이동할 때
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void ReConnect(string address, int port)
        {
            CLIENT_STATE state = _client.State;

            if (_client != null)
                _client.Dispose();

            _client.ClientSocket = null;
            _ip = address;
            _port = port;

            // 상태에 따른 Network 재연결 처리.
            switch (state)
            {
                case CLIENT_STATE.LOGIN:
                    break;

                case CLIENT_STATE.LOBI:
                    break;

                case CLIENT_STATE.PLAYING:
                    break;
            }
        }

        private void PushToReceiveQueue(CruMessage[] msg, int clientNum)
        {
            if (msg == null)
                throw new ArgumentNullException("parameter is null");

            lock (_receiveQueue)
            {
                _receiveQueue.Enqueue(msg);
            }
        }
        public CruMessage[] GetReceiveMsg()
        {
            lock (_receiveQueue)
            {
                if (_receiveQueue.Count == 0)
                    return null;

                return _receiveQueue.Dequeue();
            }
        }

        private void ProcessSend(object datas)
        {
            if (onSendCompleteEvent == null)
                return;

            onSendCompleteEvent(datas);
        }
        private void ProcessReceive(CruMessage[] datas, int clientNum)
        {
            if (onReceiveCompleteEvent == null)
                return;

            onReceiveCompleteEvent(datas, clientNum);
        }

        public void DisConnect()
        {
            _client.Dispose();
        }
    }
}
