using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    public static partial class CruNetworkProtocol
    {
        public class ItemData
        {
            // 상점에서 파는 물건은 그냥 클라이언트에 정보를 저장할까?
            // 아니면 정보를 받아오는 걸로?
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short;

            static ItemData()
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
        // Request(2) - 아이템 사고 팔고 등등, Player가 요청하는 것
        public class ItemBuy
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static ItemBuy()
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
    }
}
