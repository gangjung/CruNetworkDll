using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    public static partial class CruNetworkProtocol
    {
        /* Login Server */
        // Login(2) - Login은 Request와 ACK가 맞물려있다.
        public class LoginRequest
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int ID_SIZE { get { return _ID_SIZE; } }
            public static int PW_SIZE { get { return _PW_SIZE; } }
            public static int Count { get { return sizeList.Length; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;

            private static int _TOTAL_SIZE = 0;
            private static int _ID_SIZE = GetDataTypeSize(DATA_TYPE.STRING); // string
            private static int _PW_SIZE = GetDataTypeSize(DATA_TYPE.STRING); // string

            static LoginRequest()
            {
                sizeList = new int[] { _TOTAL_SIZE, _ID_SIZE, _PW_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.STRING, DATA_TYPE.STRING };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class LoginACK
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int ACK_SIZE { get { return _ACK_SIZE; } }
            public static int TOKEN_SIZE { get { return _TOKEN_SIZE; } }
            public static int PORTNUM_SIZE { get { return _PORTNUM_SIZE; } }
            public static int Count { get { return sizeList.Length; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;

            private static int _TOTAL_SIZE = 0;
            private static int _ACK_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short - 왜 실패했는지 확인해야 하기 때문 ㅇㅇ
            private static int _TOKEN_SIZE = GetDataTypeSize(DATA_TYPE.STRING);  // string
            private static int _PORTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static LoginACK()
            {
                sizeList = new int[] { _TOTAL_SIZE, _ACK_SIZE, _TOKEN_SIZE, _PORTNUM_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.STRING, DATA_TYPE.SHORT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
    }
}
