using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{   
    /// <summary>
    /// Message 데이터를 변환시킨다.
    /// Message Protocol에 따라서 Message byte를 만들어 줘야 하나? -> 명령에 따른 Message
    /// Message에 있는 데이터들을 object[]로 반환하는 걸 만들자.
    /// </summary>
    public class CruMessageTransfer
    {
        private int HEADSIZE_DATACOUNT;
        private int HEADSIZE_TYPE;

        public CruMessageTransfer()
        {
            int[] dataSizeList = CruNetworkProtocol.GetMessageSizeList(CruNetworkProtocol.MSG_TYPE.HEAD);
            HEADSIZE_DATACOUNT = dataSizeList[0];
            HEADSIZE_TYPE = dataSizeList[1];

            //Init();
        }

        private void Init()
        {
            
        }

        // 데이터 사이즈는 버퍼의 최종 사이즈보다 같거나 작다. 왜냐하면, 보낼 때, buffer사이즈보다 작은 사이즈를 보내기 때문!
        // 만약 데이터 사이즈와 버퍼 사이즈가 같다면... 그건 추가적으로 오는 데이터가 있을 수 있다는거니... 추가적인 작업이 있어야 함.
        public CruMessage[] ReceiveByteToMsgs(byte[] buffer, int receiveDatasize)
        {
            CruMessage[] result;
            CruMessage temp;
            int headsize;
            int transferdDataSize = 0;
            short dataCnt;
            short type;
            int idx = 0;

            // 데이터 수 확인
            headsize = CruNetworkProtocol.Head.HEADSIZE_DATACOUNT;
            dataCnt = BitConverter.ToInt16(buffer, 0);
            result = new CruMessage[dataCnt];
            idx += headsize;

            // 개별 데이터 확인
            headsize = CruNetworkProtocol.Head.HEADSIZE_TYPE;

            for(int i = 0; i<dataCnt; ++i)
            {
                // type 확인
                type = BitConverter.ToInt16(buffer, idx);
                idx += headsize;

                // data 확인
                switch (type)
                {
                    case (short)CruNetworkProtocol.MSG_TYPE.RESULT_PLAYERS_LOCATION:
                        temp = ExtractMutableDatas(type, buffer, idx, receiveDatasize, out transferdDataSize);
                        idx += transferdDataSize;
                        transferdDataSize = 0;
                        break;
                    default:
                        temp = ExtractDatas(type, buffer, idx, receiveDatasize);
                        idx += CruNetworkProtocol.GetMessageTotalSIze(type);
                        break;
                }
                
                result[i] = temp;
            }

            if (idx > receiveDatasize)
                return null;

            return result;
        }

        // CruMessage를 만드는 곳
        public CruMessage ExtractDatas(short type, byte[] buffer, int offset, int totalmsgssize)
        {
            CruMessage result;
            object[] datas;
            int[] datasizelist;
            CruNetworkProtocol.DATA_TYPE[] typelist;
            int bufferIdx = 0;
            int setDataSize = 0;

            datasizelist = CruNetworkProtocol.GetMessageSizeList(type);
            typelist = CruNetworkProtocol.GetMessageTypeList(type);

            datas = new object[datasizelist.Length - 1];

            for (int i = 1; i < datasizelist.Length; ++i)
            {
                // 실제 버퍼 입력받을 위치
                bufferIdx = offset + setDataSize;

                // 다음에 처리할 사이즈가 총 버퍼의 사이즈를 넘어간다면,
                if (bufferIdx + datasizelist[i] > totalmsgssize)
                    throw new SystemException("버퍼 사이즈를 넘겼습니다.");

                // 데이터 Get
                switch (typelist[i])
                {
                    case CruNetworkProtocol.DATA_TYPE.BOOLEAN:        
                        datas[i - 1] = BitConverter.ToBoolean(buffer, bufferIdx);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.SHORT:
                        datas[i - 1] = BitConverter.ToInt16(buffer, bufferIdx);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.INT:
                        datas[i - 1] = BitConverter.ToInt32(buffer, bufferIdx);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.FLOAT:
                        datas[i - 1] = BitConverter.ToSingle(buffer, bufferIdx);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.DOUBLE:
                        datas[i - 1] = BitConverter.ToDouble(buffer, bufferIdx);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.STRING:
                        {
                            int stringsize = BitConverter.ToInt16(buffer, bufferIdx);
                            datas[i - 1] = Encoding.ASCII.GetString(buffer, bufferIdx + CruNetworkProtocol.Head.HEADSIZE_STRING, stringsize);
                        }
                        break;

                    default:
                        break;
                }

                setDataSize += datasizelist[i];
            }

            // 변화한 데이터 사이즈가, 총량을 넘어갔을 때 ㅇㅇ
            if (setDataSize > datasizelist[0])
                return null;

            result = new CruMessage(type, datas);

            return result;
        }
        public CruMessage ExtractDatas(CruNetworkProtocol.MSG_TYPE type, byte[] buffer, int offset, int buffersize)
        {
            CruMessage result;
            object[] datas;
            int[] datasizelist;
            CruNetworkProtocol.DATA_TYPE[] typelist;
            int bufferIdx = 0;
            int setDataSize = 0;

            datasizelist = CruNetworkProtocol.GetMessageSizeList(type);
            typelist = CruNetworkProtocol.GetMessageTypeList(type);

            datas = new object[datasizelist.Length - 1];

            for (int i = 1; i < datasizelist.Length; ++i)
            {
                setDataSize = offset + bufferIdx;

                // 다음에 처리할 사이즈가 총 버퍼의 사이즈를 넘어간다면,
                if (setDataSize + datasizelist[i] > buffersize)
                    throw new SystemException("버퍼 사이즈를 넘겼습니다.");

                // 데이터 Get
                switch (typelist[i])
                {
                    case CruNetworkProtocol.DATA_TYPE.BOOLEAN:
                        datas[i - 1] = BitConverter.ToBoolean(buffer, setDataSize);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.SHORT:
                        datas[i - 1] = BitConverter.ToInt16(buffer, setDataSize);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.INT:
                        datas[i - 1] = BitConverter.ToInt32(buffer, setDataSize);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.FLOAT:
                        datas[i - 1] = BitConverter.ToSingle(buffer, setDataSize);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.DOUBLE:
                        datas[i - 1] = BitConverter.ToDouble(buffer, setDataSize);
                        break;

                        // string어디까지 읽을건지가 안적혀있구나... 이거... 그냥 앞에 데이터타입을 붙여야하나?
                    case CruNetworkProtocol.DATA_TYPE.STRING:
                        {
                            int stringsize = BitConverter.ToInt16(buffer, setDataSize);
                            datas[i - 1] = Encoding.ASCII.GetString(buffer, bufferIdx + CruNetworkProtocol.Head.HEADSIZE_STRING, stringsize);
                        }
                        break;

                    default:
                        break;
                }

                setDataSize += datasizelist[i];
            }

            // 변화한 데이터 사이즈가, 총량을 넘어갔을 때 ㅇㅇ
            if (setDataSize > datasizelist[0])
                return null;

            result = new CruMessage(type, datas);

            return result;
        }

        // 같은 데이터를 여러번 중첩하는 가변 데이터 처리.
        public CruMessage ExtractMutableDatas(short type, byte[] buffer, int offset, int totalmsgssize, out int transferedDataSize)
        {
            CruMessage result;
            object[] datas;
            int[] datasizelist;
            CruNetworkProtocol.DATA_TYPE[] typelist;
            int dataCount = 0;
            int inputDataCount = 0;
            int totalDataSize = 0;
            int bufferIdx = 0;
            int setDataSize = 0;

            datasizelist = CruNetworkProtocol.GetMessageSizeList(type);
            typelist = CruNetworkProtocol.GetMessageTypeList(type);

            dataCount = BitConverter.ToInt16(buffer, offset);
            setDataSize += datasizelist[1];

            // 데이터 사이즈 측정
            for (int i = 2; i < datasizelist.Length; ++i)
                totalDataSize += datasizelist[i];

            totalDataSize = (totalDataSize * dataCount) + datasizelist[1];

            // 데이터 사이즈만큼 byte배열 생성
            datas = new object[datasizelist.Length - 1];
            datas[inputDataCount++] = (short)dataCount;

            for (int i = 0; i < dataCount; ++i)
            {
                for(int j = 2; j<datasizelist.Length; ++j)
                {
                    // 실제 버퍼 입력받을 위치
                    bufferIdx = offset + setDataSize;

                    // 다음에 처리할 사이즈가 총 버퍼의 사이즈를 넘어간다면,
                    if (bufferIdx + datasizelist[j] > totalmsgssize)
                        throw new SystemException("버퍼 사이즈를 넘겼습니다.");

                    // 데이터 Get
                    switch (typelist[j])
                    {
                        case CruNetworkProtocol.DATA_TYPE.BOOLEAN:
                            datas[inputDataCount++] = BitConverter.ToBoolean(buffer, bufferIdx);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.SHORT:
                            datas[inputDataCount++] = BitConverter.ToInt16(buffer, bufferIdx);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.INT:
                            datas[inputDataCount++] = BitConverter.ToInt32(buffer, bufferIdx);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.FLOAT:
                            datas[inputDataCount++] = BitConverter.ToSingle(buffer, bufferIdx);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.DOUBLE:
                            datas[inputDataCount++] = BitConverter.ToDouble(buffer, bufferIdx);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.STRING:
                            {
                                int stringsize = BitConverter.ToInt16(buffer, bufferIdx);
                                datas[inputDataCount++] = Encoding.ASCII.GetString(buffer, bufferIdx + CruNetworkProtocol.Head.HEADSIZE_STRING, stringsize);
                            }
                            break;

                        default:
                            break;
                    }

                    setDataSize += datasizelist[j];
                }
            }

            // 변화한 데이터 사이즈가, 총량을 넘어갔을 때 ㅇㅇ
            if (setDataSize > totalDataSize)
            {
                transferedDataSize = 0;
                return null;
            }

            result = new CruMessage(type, datas);
            transferedDataSize = totalDataSize;

            return result;
        }




        // 메시지들을 클라이언트로 보낼 byte[]로 변환.
        public int SingleMsgToSendData(CruMessage msg, byte[] buffer, int offset, int buffersize)
        {
            byte[] temp;
            int datasize = 0;
            int msgSize = 0;
            int dataCnt = 1;

            // 보낼 데이터 수
            //datasize = CruNetworkProtocol.Head.HEADSIZE_DATACOUNT;
            temp = BitConverter.GetBytes((short)dataCnt);
            Array.Copy(temp, 0, buffer, offset, temp.Length);
            datasize += temp.Length;

            // 데이터 처리
            {
                // msg 추출
                switch (msg.Type)
                {
                    case (short)CruNetworkProtocol.MSG_TYPE.RESULT_PLAYERS_LOCATION:
                        msgSize = ExtractMutableSendDatas(msg, buffer, datasize);
                        break;
                    default:
                        msgSize = ExtractSendDatas(msg, buffer, datasize);
                        break;
                }

                if (msgSize == 0)
                    throw new SystemException("메시지를 변환하는 과정에서 오류가 발생했습니다.");

                // 추출된 byte 수 만큼 datasize 증가.
                datasize += msgSize;
            }
            if (datasize > buffersize)
                throw new SystemException("buffer size를 초과하였습니다.");

            return datasize;
        }
        public int MsgsToSendData(CruMessage[] msgs, byte[] buffer, int offset, int buffersize)
        {
            byte[] temp;
            int datasize = 0;
            int msgSize = 0;
            int dataCnt = msgs.Length;

            //datasize = CruNetworkProtocol.Head.HEADSIZE_DATACOUNT;
            temp = BitConverter.GetBytes((short)dataCnt);
            Array.Copy(temp, 0, buffer, offset, temp.Length);
            datasize += temp.Length;

            for(int i = 0; i<dataCnt; ++i)
            {
                // msg 추출
                switch (msgs[i].Type)
                {
                    case (short)CruNetworkProtocol.MSG_TYPE.RESULT_PLAYERS_LOCATION:
                        msgSize = ExtractMutableSendDatas(msgs[i], buffer, datasize);
                        break;
                    default:
                        msgSize = ExtractSendDatas(msgs[i], buffer, datasize);
                        break;
                }

                if (msgSize == 0)
                    throw new SystemException("메시지를 변환하는 과정에서 오류가 발생했습니다.");

                // 추출된 byte 수 만큼 datasize 증가.
                datasize += msgSize;
            }

            if (datasize > buffersize)
                throw new SystemException("buffer size를 초과하였습니다.");

            return datasize;
        }

        public int ExtractSendDatas(CruMessage msg, byte[] buffer, int offset)
        {
            byte[] temp;
            short type = msg.Type;
            object[] datas = msg.Datas;
            int headSize = 0;
            int dataSize = 0;
            int[] datasizelist;
            CruNetworkProtocol.DATA_TYPE[] typelist;

            headSize = CruNetworkProtocol.Head.HEADSIZE_TYPE;
            datasizelist = CruNetworkProtocol.GetMessageSizeList(type);
            typelist = CruNetworkProtocol.GetMessageTypeList(type);

            temp = BitConverter.GetBytes(type);
            Array.Copy(temp, 0, buffer, offset + dataSize, headSize);
            dataSize += headSize;

            for(int i = 0; i<msg.Datas.Length; ++i)
            {
                // 버퍼 사이즈 넘어가는지 체크
                if (offset + dataSize + datasizelist[i + 1] > buffer.Length)
                    throw new SystemException("버퍼 사이즈를 넘어갑니다.");

                switch (typelist[i + 1])
                {
                    case CruNetworkProtocol.DATA_TYPE.BOOLEAN:
                        temp = BitConverter.GetBytes((bool)datas[i]);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.SHORT:
                        temp = BitConverter.GetBytes((short)datas[i]);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.INT:
                        temp = BitConverter.GetBytes((int)datas[i]);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.FLOAT:
                        temp = BitConverter.GetBytes((float)datas[i]);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.DOUBLE:
                        temp = BitConverter.GetBytes((double)datas[i]);
                        break;

                    case CruNetworkProtocol.DATA_TYPE.STRING:
                        {
                            short stringsize;
                            temp = new byte[datasizelist[i + 1]];

                            if ((string)datas[i] == null)
                                stringsize = 0;
                            else
                                stringsize = (short)((string)datas[i]).Length;

                            // Set string size
                            Array.Copy(BitConverter.GetBytes(stringsize), temp, CruNetworkProtocol.Head.HEADSIZE_STRING);

                            // Set string data
                            if (datas[i] != null)
                                Encoding.ASCII.GetBytes((string)datas[i], 0, ((string)datas[i]).Length, temp, CruNetworkProtocol.Head.HEADSIZE_STRING);
                        }
                        break;

                    default:
                        break;
                }

                Array.Copy(temp, 0, buffer, offset + dataSize, datasizelist[i + 1]);
                dataSize += datasizelist[i + 1];
            }

            if (dataSize != datasizelist[0] + headSize)
                return 0;

            return dataSize;
        }

        public int ExtractMutableSendDatas(CruMessage msg, byte[] buffer, int offset)
        {
            /*
             일반적으로 가변데이터 형식은 똑같다. 
             type + data갯수 + (data 정보들) + (data 정보들) + ...
             */
            byte[] temp;
            short type = msg.Type;
            object[] datas = msg.Datas;
            int headSize = 0;
            int dataCnt = datas.Length;
            int dataSize = 0;
            int totalDataSize = 0;
            int[] datasizelist;
            CruNetworkProtocol.DATA_TYPE[] typelist;

            headSize = CruNetworkProtocol.Head.HEADSIZE_TYPE;
            datasizelist = CruNetworkProtocol.GetMessageSizeList(type);
            typelist = CruNetworkProtocol.GetMessageTypeList(type);

            // type 처리
            temp = BitConverter.GetBytes(type);
            Array.Copy(temp, 0, buffer, offset + dataSize, headSize);
            dataSize += headSize;

            // data count 처리
            temp = BitConverter.GetBytes((short)datas[0]);
            Array.Copy(temp, 0, buffer, offset + dataSize, datasizelist[1]);
            dataSize += datasizelist[1];

            // data 처리
            for (int i = 1; i < dataCnt; ++i)
            {
                for(int j = 2; j < datasizelist.Length; ++j)
                {
                    // 버퍼 사이즈 넘어가는지 체크
                    if (offset + dataSize + datasizelist[j] > buffer.Length)
                        throw new SystemException("버퍼 사이즈를 넘어갑니다.");

                    switch (typelist[j])
                    {
                        case CruNetworkProtocol.DATA_TYPE.BOOLEAN:
                            temp = BitConverter.GetBytes((bool)datas[i]);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.SHORT:
                            temp = BitConverter.GetBytes((short)datas[i]);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.INT:
                            temp = BitConverter.GetBytes((int)datas[i]);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.FLOAT:
                            temp = BitConverter.GetBytes((float)datas[i]);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.DOUBLE:
                            temp = BitConverter.GetBytes((double)datas[i]);
                            break;

                        case CruNetworkProtocol.DATA_TYPE.STRING:
                            {
                                short stringsize;
                                temp = new byte[datasizelist[j]];

                                if ((string)datas[i] == null)
                                    stringsize = 0;
                                else
                                    stringsize = (short)((string)datas[i]).Length;

                                // Set string size
                                Array.Copy(BitConverter.GetBytes(stringsize), temp, CruNetworkProtocol.Head.HEADSIZE_STRING);

                                // Set string data
                                if (datas[i] != null)
                                    Encoding.ASCII.GetBytes((string)datas[i], 0, ((string)datas[i]).Length, temp, CruNetworkProtocol.Head.HEADSIZE_STRING);
                            }
                            break;

                        default:
                            break;
                    }

                    Array.Copy(temp, 0, buffer, offset + dataSize, datasizelist[j]);
                    dataSize += datasizelist[j];
                }
            }

            // 총 데이터 사이즈 = 헤드사이즈 + 데이터묶음 수 사이즈 + ((데이터 묶음 사이즈) * 데이터 묶음 수)
            totalDataSize = headSize + datasizelist[1] + ((datasizelist[0] - datasizelist[1]) * (short)datas[0]);

            if (dataSize != totalDataSize)
                return 0;

            return dataSize;
        }
    }
}
