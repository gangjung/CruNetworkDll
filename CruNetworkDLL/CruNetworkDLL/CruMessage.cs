using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    // MessageHandler가 필요할거 같다.

    /// <summary>
    /// 서버와 클라이언트가 주고받는 메시지 정보.
    /// byte -> 정보
    /// 정보 -> byte
    /// 헤드와 바디를 가지고 있으며, 명령 종류를 가지고 있다.
    /// 
    /// 데이터를 입력할 때, 타입이랑 데이터 사이즈 같은지 잘 확인해야한다.
    /// 
    /// 해당 토큰은 데이터를 처리...하기보단, 데이터를 들고있어야 맞다.
    ///  -> 통신에서 사용되는 메시지는 여기까지하고, 메시지를 각 함수로 전달할 때, 따로 메시지를 변화시켜 해당 큐에 저장할 것.
    ///     이 과정은 중앙이 아니라 해당 Receive thread에서 진행되는 것이므로 각각 클라이언트가 처리하는게 맞음.
    /// </summary>
    public class CruMessage
    {
        // Property
        public short Type { get { return _type; } }
        public object[] Datas { get { return _datas; } }
        
        private short _type;
        private object[] _datas;
        
        // 비어있는 Message를 만들 때, 어떻게 해줘야 할까?
        public CruMessage()
        {
            _type = 0;
            _datas = null;
        }

        public CruMessage(short type, object[] datas)
        {
            _type = type;
            _datas = datas;
        }
        public CruMessage(CruNetworkProtocol.MSG_TYPE type, object[] datas)
        {
            _type = (short)type;
            _datas = datas;
        }

        public void Initialize(short type, object[] datas)
        {
            _type = type;
            _datas = datas;
        }
    }
}
