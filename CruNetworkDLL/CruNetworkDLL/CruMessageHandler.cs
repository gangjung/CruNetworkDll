using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    /// <summary>
    /// 들어온 메시지를 type에 따라 실행시킨다.
    /// 음... 메시지를 종류별로 구분해서, 순서대로 명령을 실행시킬까??
    /// 
    /// Q.데이터가 오면, 받은 thread에서 처리해줘야할까, 메인이 처리해줘야 할까...?
    /// 음...너무 메인에게 몰아넣으면, 메인thread에서 처리해줘야할 일이 많아지기에 느려진다.각 thread에서 처리해주는거보다 느려질 수 있다.그냥 각 thread에서 처리해주는 걸로하자.
    ///  -> receive가 엄청 막힐 수 있다... 각 명령 처리 thread에서 넣어주고, 메인에서 매 프레임마다 처리해줄 수 있게 해야할까?
    ///  -> 일단 해당 명령을 처리하면, 그것에 대한 결과를 
    ///  
    /// ClientManager에서 만들어서, client생성 시, 해당 명령을 전달해주면 된다.
    /// 
    /// Network단에서 사용하는게 아니라, Client에서 Handler를 불러온 뒤, 명령을 추가해서 ClientManager의 ClientEvent에 추가해주면 된다.
    ///  -> NetworkManager.AddClientEvent(Handler로 만들어진 event);
    /// </summary>
    public class CruMessageHandler
    {
        // void -> CruMessage?
        // delegate로 만들어서 데이터를 받도록 처리하자.
        // 먼저 전송받은 데이터에 들어있는 정보를 먼저 parameter로 받고, 처리과정에서 필요한 정보를 뒤의 parameter로 넣는다.
        // => 공격, 피격 같은건 알아서 명령 넣고 처리하면 되지만, 아이템 구입 등은 하나의 처리를 할 때, 다른 처리를 할 수 없도록 막아두는 것도 좋은 방법이다. 로직이 꼬일 수 있기 때문!
        //    그렇기에 의도적으로 지연을 둬서 명령이 올 때까지 아무것도 못하게 처리하자!
        //    receive thread에서 명령 처리하도록 하자!
        // 대부분 msg들이 누구한테서 왔는지 처리해줘야 한다.
        public delegate void OnLoginRequestEvent(CruMessage msg, int clientNum);
        public delegate void OnLoginACKEvent(CruMessage msg, int clientNum);
        public delegate void OnCheckUserEvent(CruMessage msg, int clientNum);
        public delegate void OnCheckUserACKEvent(CruMessage msg, int clientNum);
        public delegate void OnPlayerDataEvent(CruMessage msg, int clientNum);
        public delegate void OnCustomPlayerDataEvent(CruMessage msg, int clientNum);
        public delegate void OnPlayerLocation(CruMessage msg, int clientNum);
        public delegate void OnItemDataEvent(CruMessage msg, int clientNum);
        public delegate void OnMoveEvent(CruMessage msg, int clientNum);
        public delegate void OnLookEvent(CruMessage msg, int clientNum);
        public delegate void OnItemBuyEvent(CruMessage msg, int clientNum);
        public delegate void OnRequestPlayersLocation(CruMessage msg, int clientNum);
        public delegate void OnResultGetEXPEvent(CruMessage msg, int clientNum);
        public delegate void OnResultLevelUpEvent(CruMessage msg, int clientNum);
        public delegate void OnResultAttackEvent(CruMessage msg, int clientNum);

        public OnLoginRequestEvent onLoginRequestEvent;
        public OnLoginACKEvent onLoginACKEvent;
        public OnCheckUserEvent onCheckUserEvent;
        public OnCheckUserACKEvent onCheckUserACKEvent;
        public OnPlayerDataEvent onPlayerDataEvent;
        public OnCustomPlayerDataEvent onCustomPlayerDataEvent;
        public OnPlayerLocation onPlayerLocation;
        public OnItemDataEvent onItemDataEvent;
        public OnMoveEvent onMoveEvent;
        public OnLookEvent onLookEvent;
        public OnItemBuyEvent onItemBuyEvent;
        public OnRequestPlayersLocation onRequestPlayersLocation;
        public OnResultGetEXPEvent onResultGetEXPEvent;
        public OnResultLevelUpEvent onResultLevelUpEvent;
        public OnResultAttackEvent onResultAttackEvent;

        public CruMessageHandler()
        {
            //// delegate 배열이라...
            //Delegate[] a = new Delegate[10];
            //a[3].DynamicInvoke();
        }

        // bool 형으로 만들어서 제대로 처리되었는지 확인해야할까...?
        public void MessageHandle(CruMessage msg, int clientNum)
        {
            CruNetworkProtocol.MSG_TYPE type = (CruNetworkProtocol.MSG_TYPE)msg.Type;
            
            switch (type)
            {
                case CruNetworkProtocol.MSG_TYPE.LOGIN_REQUEST:
                    onLoginRequestEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.LOGIN_ACK:
                    onLoginACKEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.CHECK_USER:
                    onCheckUserEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.CHECK_USER_ACK:
                    onCheckUserACKEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.PLAYER_DATA:
                    onPlayerDataEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.CUSTOM_PLAYER_DATA:
                    onCustomPlayerDataEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.PLAYER_LOCATION:
                    onPlayerLocation(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.ITEM_DATA:
                    onItemDataEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.MOVE:
                    onMoveEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.LOOK:
                    onLookEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.ITEM_BUY:
                    onItemBuyEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.REQUEST_PLAYERS_LOCATION:
                    onRequestPlayersLocation(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.RESULT_GETEXP:
                    onResultGetEXPEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.RESULT_LEVELUP:
                    onResultLevelUpEvent(msg, clientNum);
                    break;

                case CruNetworkProtocol.MSG_TYPE.RESULT_ATTACK:
                    onResultAttackEvent(msg, clientNum);
                    break;

                default:
                    throw new SystemException("해당 명령이 존재하지 않습니다.");
                    
            }
        }
        public void MessageHandle(CruMessage[] msgs, int clientNum)
        {
            CruNetworkProtocol.MSG_TYPE type;
            int count = msgs.Length;

            for (int i = 0; i < count; ++i)
            {
                MessageHandle(msgs[i], clientNum);
            }
        }

        /// <summary>
        /// Event Reset.
        /// </summary>
        public void ResetEvents()
        {
            onLoginRequestEvent = null;
            onLoginACKEvent = null;
            onCheckUserEvent = null;
            onCheckUserACKEvent = null;
            onPlayerDataEvent = null;
            onCustomPlayerDataEvent = null;
            onItemDataEvent = null;
            onMoveEvent = null;
            onLookEvent = null;
            onItemBuyEvent = null;
            onResultGetEXPEvent = null;
            onResultLevelUpEvent = null;
            onResultAttackEvent = null;
        }
    }
}
