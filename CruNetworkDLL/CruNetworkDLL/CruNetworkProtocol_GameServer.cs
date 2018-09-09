using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    public static partial class CruNetworkProtocol
    {
        /* Game Server */
        // Data(5)
        public class CheckUser
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int ID_SIZE { get { return _ID_SIZE; } }
            public static int TOKEN_SIZE { get { return _TOKEN_SIZE; } }
            public static int Count { get { return sizeList.Length; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;

            private static int _TOTAL_SIZE = 0;
            private static int _ID_SIZE = GetDataTypeSize(DATA_TYPE.STRING); // string
            private static int _TOKEN_SIZE = GetDataTypeSize(DATA_TYPE.STRING);  // string

            static CheckUser()
            {
                sizeList = new int[] { _TOTAL_SIZE, _ID_SIZE, _TOKEN_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.STRING, DATA_TYPE.STRING };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class CheckUserACK
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int ACK { get { return _ACK; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _ACK = GetDataTypeSize(DATA_TYPE.SHORT);  // short - 왜 실패했는지 확인해야 하기 때문 ㅇㅇ
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static CheckUserACK()
            {
                sizeList = new int[] { _TOTAL_SIZE, _ACK, _CLIENTNUM_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.SHORT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class RequestPlayersLocation
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static RequestPlayersLocation()
            {
                sizeList = new int[] { _TOTAL_SIZE, _CLIENTNUM_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class ResultPlayersLocation
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENT_COUNT_SIZE { get { return _CLIENT_COUNT_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static int HORI_SIZE { get { return _HORI_SIZE; } }
            public static int VERTI_SIZE { get { return _VERTI_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENT_COUNT_SIZE = GetDataTypeSize(DATA_TYPE.SHORT);
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT);
            private static int _HORI_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT);
            private static int _VERTI_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT);

            static ResultPlayersLocation()
            {
                sizeList = new int[] { _TOTAL_SIZE, CLIENT_COUNT_SIZE, _CLIENTNUM_SIZE, _HORI_SIZE, _VERTI_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.SHORT, DATA_TYPE.FLOAT, DATA_TYPE.FLOAT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }

        // Result(3)
        public class ResultGetEXP
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static int EXP_AMOUNT_SIZE { get { return _EXP_AMOUNT_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            // 경험치 얼마나 얻었는지를 볼 때, 사용하려나?
            // 현재 경험치는 [Data] 카테고리에서 얻자.
            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT);   // short
            private static int _EXP_AMOUNT_SIZE = GetDataTypeSize(DATA_TYPE.INT);  // int;

            static ResultGetEXP()
            {
                sizeList = new int[] { _TOTAL_SIZE, _CLIENTNUM_SIZE, _EXP_AMOUNT_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.INT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class ResultLevelUp
        {
            // 레벨업을 하면 어떤 데이터를 줘야하나???
            // 플레이어 데이터를 다시 전달해줘야 하나???
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;

            static ResultLevelUp()
            {
                sizeList = new int[] { _TOTAL_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class ResultAttack
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int ATTACKER_SIZE { get { return _ATTACKER_SIZE; } }
            public static int VICTIM_SIZE { get { return _VICTIM_SIZE; } }
            public static int DAMAGE_SIZE { get { return _DAMAGE_SIZE; } }
            public static int TYPE_SIZE { get { return _TYPE_SIZE; } }
            public static int DIRECTION_SIZE { get { return _DIRECTION_SIZE; } }
            public static readonly int[] sizeList;
            // 공격에 대한 정보 미정
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            // 공격 결과를 전달하는건데, 누가 공격했는지가 필요할까??
            // 나중에 필요할듯. 공격 당한 방향이라던지 ㅇㅇ
            private static int _TOTAL_SIZE = 0;
            private static int _ATTACKER_SIZE = GetDataTypeSize(DATA_TYPE.SHORT);    // short;
            private static int _VICTIM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT);    // short;
            private static int _DAMAGE_SIZE = GetDataTypeSize(DATA_TYPE.INT);   // int;
            private static int _TYPE_SIZE = 0; // 어떤 방식의 공격이냐.
            private static int _DIRECTION_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT);    // float - 공격한 방향

            static ResultAttack()
            {
                sizeList = new int[] { _TOTAL_SIZE, _ATTACKER_SIZE, _VICTIM_SIZE, _DAMAGE_SIZE, _TYPE_SIZE, _DIRECTION_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.SHORT, DATA_TYPE.INT, 0, DATA_TYPE.FLOAT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
    }
}

