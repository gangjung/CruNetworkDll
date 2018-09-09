using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    /// <summary>
    /// BroadCast 메시지인지, UniCast인지 확인해야한다.
    /// 메시지 전달 방식 여러가지를 확인해야한다.
    /// 이건 변수로 사용하지 않을것. 그러니까 static으로 사용할 수 있도록 처리
    /// object[] 배열에 넣어서 바로 사용할 수 있도록 할 수 있지만, 
    /// 데이터 사이즈 부분, 나중에 파일로 Json 파일로 변환해서 읽을 수 있도록 할까?
    /// 
    /// 정해진 데이터를 쉽게 가져올 수 있는 방법에 뭐가 있을까?
    ///  - object[]에서 저장하면, 해당 데이터형으로 변환해줘야 한다. 데이터형을 모르면 무쓸모.
    ///  - Json으로 저장? Dictionary로하면 차피 object형을 쓰는건 같음.
    ///  
    /// Request는 ClientNum 정보가 필요하다.
    /// 
    /// 확장성을 고려해서, 새로운 프로토콜이 추가되면, 해당 프로토콜을 추가할 수 있는 함수가 필요하다. 생성자라던지 ㅇㅇ.
    ///  - static 생성자가 존재한다!!!
    /// </summary>
    public static partial class CruNetworkProtocol
    {
        private static int[] _totalDataSizeList;
        private static Dictionary<MSG_TYPE, int[]> _dataSizeList;
        private static Dictionary<MSG_TYPE, DATA_TYPE[]> _dataTypeList;

        // Type - type을 그냥 각 class에 저장해둘까...? enum말고?
        public enum MSG_TYPE
        {
            HEAD, NORMAL_DATA,
            LOGIN_REQUEST, LOGIN_ACK,
            CHECK_USER, CHECK_USER_ACK, PLAYER_DATA, CUSTOM_PLAYER_DATA, PLAYER_LOCATION, ITEM_DATA,
            MOVE, LOOK,
            ITEM_BUY, REQUEST_PLAYERS_LOCATION, RESULT_PLAYERS_LOCATION,
            RESULT_GETEXP, RESULT_LEVELUP, RESULT_ATTACK
        };
        
        // TypeCode를 쓰고싶었으나, float이 없는 관계로... 만들어서 씀.
        // 찾아보니 float -> TypeCode.Single 로 사용되고있더라. 
        // 여기에 값을 할당하고 싶지만, 같은 사이즈를 가지고 있는 변수들이 있어서 안됨...
        public enum DATA_TYPE { BOOLEAN , SHORT, INT, FLOAT, DOUBLE, STRING };

        // 정적 생성자.
        static CruNetworkProtocol()
        {
            _totalDataSizeList = new int[]{
                // Head
                Head.HEADSIZE_TYPE,
                NormalData.TOTAL_SIZE,
                // Login Server
                LoginRequest.TOTAL_SIZE,
                LoginACK.TOTAL_SIZE,
                // Game Server
                CheckUser.TOTAL_SIZE,
                CheckUserACK.TOTAL_SIZE,
                ResultGetEXP.TOTAL_SIZE,
                ResultLevelUp.TOTAL_SIZE,
                ResultAttack.TOTAL_SIZE,
                // Player
                PlayerData.TOTAL_SIZE,
                CustomPlayerData.TOTAL_SIZE,
                PlayerLocation.TOTAL_SIZE,
                RequestPlayersLocation.TOTAL_SIZE,
                ResultPlayersLocation.TOTAL_SIZE,
                Move.TOTAL_SIZE,
                Look.TOTAL_SIZE,
                // Item
                ItemData.TOTAL_SIZE,
                ItemBuy.TOTAL_SIZE
            };

            _dataSizeList = new Dictionary<MSG_TYPE, int[]>()
            {
                // Head
                { MSG_TYPE.HEAD, Head.sizeList },
                { MSG_TYPE.NORMAL_DATA, NormalData.sizeList },
                // Login Server
                { MSG_TYPE.LOGIN_REQUEST, LoginRequest.sizeList },
                { MSG_TYPE.LOGIN_ACK, LoginACK.sizeList },
                // Game Server
                { MSG_TYPE.CHECK_USER, CheckUser.sizeList },
                { MSG_TYPE.CHECK_USER_ACK, CheckUserACK.sizeList },
                { MSG_TYPE.RESULT_GETEXP, ResultGetEXP.sizeList },
                { MSG_TYPE.RESULT_LEVELUP, ResultLevelUp.sizeList },
                { MSG_TYPE.RESULT_ATTACK, ResultAttack.sizeList },
                // Player
                { MSG_TYPE.PLAYER_DATA, PlayerData.sizeList },
                { MSG_TYPE.CUSTOM_PLAYER_DATA, CustomPlayerData.sizeList },
                { MSG_TYPE.PLAYER_LOCATION, PlayerLocation.sizeList },
                { MSG_TYPE.REQUEST_PLAYERS_LOCATION, RequestPlayersLocation.sizeList },
                { MSG_TYPE.RESULT_PLAYERS_LOCATION, ResultPlayersLocation.sizeList },
                { MSG_TYPE.MOVE, Move.sizeList },
                { MSG_TYPE.LOOK, Look.sizeList },
                // Item
                { MSG_TYPE.ITEM_DATA, ItemData.sizeList },
                { MSG_TYPE.ITEM_BUY, ItemBuy.sizeList }
            };

            _dataTypeList = new Dictionary<MSG_TYPE, DATA_TYPE[]>()
            {
                // Head
                { MSG_TYPE.HEAD, Head.typeList },
                { MSG_TYPE.NORMAL_DATA, NormalData.typeList },
                // Login Server
                { MSG_TYPE.LOGIN_REQUEST, LoginRequest.typeList },
                { MSG_TYPE.LOGIN_ACK, LoginACK.typeList },
                // Game Server
                { MSG_TYPE.CHECK_USER, CheckUser.typeList },
                { MSG_TYPE.CHECK_USER_ACK, CheckUserACK.typeList },
                { MSG_TYPE.RESULT_GETEXP, ResultGetEXP.typeList },
                { MSG_TYPE.RESULT_LEVELUP, ResultLevelUp.typeList },
                { MSG_TYPE.RESULT_ATTACK, ResultAttack.typeList },
                // Player
                { MSG_TYPE.PLAYER_DATA, PlayerData.typeList },
                { MSG_TYPE.CUSTOM_PLAYER_DATA, CustomPlayerData.typeList },
                { MSG_TYPE.PLAYER_LOCATION, PlayerLocation.typeList },
                { MSG_TYPE.REQUEST_PLAYERS_LOCATION, RequestPlayersLocation.typeList },
                { MSG_TYPE.RESULT_PLAYERS_LOCATION, RequestPlayersLocation.typeList },
                { MSG_TYPE.MOVE, Move.typeList },
                { MSG_TYPE.LOOK, Look.typeList },
                // Item
                { MSG_TYPE.ITEM_DATA, ItemData.typeList },
                { MSG_TYPE.ITEM_BUY, ItemBuy.typeList }
            };
        }

        public static int GetDataTypeSize(DATA_TYPE type)
        {
            switch (type)
            {
                case DATA_TYPE.BOOLEAN:
                    return sizeof(bool);

                case DATA_TYPE.SHORT:
                    return sizeof(short);

                case DATA_TYPE.INT:
                    return sizeof(int);

                case DATA_TYPE.FLOAT:
                    return sizeof(float);

                case DATA_TYPE.DOUBLE:
                    return sizeof(double);

                case DATA_TYPE.STRING:
                    return 10;

                default:
                    return 0;
            }
        }

        // 각 메시지 타입에따라 전체 메시지 크기 확인
        public static int GetMessageTotalSIze(MSG_TYPE type)
        {
            return _totalDataSizeList[(int)type];
        }
        public static int GetMessageTotalSIze(short type)
        {
            return _totalDataSizeList[type];
        }

        // 각 메시지 타입에 따른 메시지 데이터 사이즈 정보 확인
        public static int[] GetMessageSizeList(MSG_TYPE type)
        {
            return _dataSizeList[type];
        }
        public static int[] GetMessageSizeList(short type)
        {
            return _dataSizeList[(MSG_TYPE)type];
        }

        // 각 메시지 타입에 따른 데이터 타입 정보 확인
        public static DATA_TYPE[] GetMessageTypeList(MSG_TYPE type)
        {
            return _dataTypeList[type];
        }
        public static DATA_TYPE[] GetMessageTypeList(short type)
        {
            return _dataTypeList[(MSG_TYPE)type];
        }
    }
}

