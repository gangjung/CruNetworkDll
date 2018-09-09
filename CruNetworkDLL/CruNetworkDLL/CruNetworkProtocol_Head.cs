using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    public static partial class CruNetworkProtocol
    {
        /* Common */
        // Message SIze(2)     
        public class Head
        {
            public static int HEADSIZE_DATACOUNT { get { return _HEADSIZE_DATACOUNT; } }
            public static int HEADSIZE_TYPE { get { return _HEADSIZE_TYPE; } }
            public static int HEADSIZE_STRING { get { return _HEADSIZE_STRING; } }
            public static int Count { get { return sizeList.Length; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;

            private static int _HEADSIZE_DATACOUNT = GetDataTypeSize(DATA_TYPE.SHORT); // short
            private static int _HEADSIZE_TYPE = GetDataTypeSize(DATA_TYPE.SHORT);    // short
            private static int _HEADSIZE_STRING = GetDataTypeSize(DATA_TYPE.SHORT); // short

            static Head()
            {
                sizeList = new int[] { _HEADSIZE_DATACOUNT, _HEADSIZE_TYPE, _HEADSIZE_STRING };
                typeList = new DATA_TYPE[] { DATA_TYPE.SHORT, DATA_TYPE.SHORT, DATA_TYPE.SHORT };
            }
        }
        
        public class NormalData
        {
            public static int TOTAL_SIZE { get { return _TOTAL_SIZE; } }
            public static int BODY_SIZE { get { return _BODY_SIZE; } }
            public static int Count { get { return sizeList.Length; } }
            public static readonly int[] sizeList;
            public static readonly DATA_TYPE[] typeList;

            private static int _TOTAL_SIZE = 0;
            private static int _BODY_SIZE = GetDataTypeSize(DATA_TYPE.STRING);

            static NormalData()
            {
                sizeList = new int[] { _TOTAL_SIZE, _BODY_SIZE };
                typeList = new DATA_TYPE[] { DATA_TYPE.INT, DATA_TYPE.STRING };

                for (int i = 1; i < sizeList.Length; ++i)
                {
                    _TOTAL_SIZE += sizeList[i];
                    sizeList[0] += sizeList[i];
                }
            }
        }
    }
}
