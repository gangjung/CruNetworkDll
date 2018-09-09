using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    public static partial class CruNetworkProtocol
    {
        public class PlayerData
        {
            // 미정
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static PlayerData()
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
        public class CustomPlayerData
        {
            // 미정
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static CustomPlayerData()
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
        public class PlayerLocation
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static int HORI_SIZE { get { return _HORI_SIZE; } }
            public static int VERTI_SIZE { get { return _VERTI_SIZE; } }

            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short;
            private static int _HORI_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT); // float;
            private static int _VERTI_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT); // float;

            static PlayerLocation()
            {
                sizeList = new int[] { _TOTAL_SIZE, _CLIENTNUM_SIZE, _HORI_SIZE, _VERTI_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.FLOAT, DATA_TYPE.FLOAT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        

        // Movement(2)
        public class Move
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static int HORI_SIZE { get { return _HORI_SIZE; } }
            public static int VERTI_SIZE { get { return _VERTI_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short
            private static int _HORI_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT);    // float
            private static int _VERTI_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT);   // float

            static Move()
            {
                sizeList = new int[] { _TOTAL_SIZE, _CLIENTNUM_SIZE, _HORI_SIZE, _VERTI_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.FLOAT, DATA_TYPE.FLOAT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
        public class Look
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int CLIENTNUM_SIZE { get { return _CLIENTNUM_SIZE; } }
            public static int ANGLE_SIZE { get { return _ANGLE_SIZE; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;
            public static int Count { get { return sizeList.Length; } }

            // 일단 공격할 때만 그 쪽 방향을 처다보니, 쓸 필요 없으려나...?
            private static int _TOTAL_SIZE = 0;
            private static int _CLIENTNUM_SIZE = GetDataTypeSize(DATA_TYPE.SHORT); // short
            private static int _ANGLE_SIZE = GetDataTypeSize(DATA_TYPE.FLOAT);   // float

            static Look()
            {
                sizeList = new int[] { _TOTAL_SIZE, _CLIENTNUM_SIZE, _ANGLE_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.SHORT, DATA_TYPE.FLOAT };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
    }
}
