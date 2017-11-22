using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UpdaterShare.Model;

namespace UpdaterShare.Utility
{
    public static class PacketUtils
    {
        #region Tag Length
        /// <summary>
        /// Packet Start_Tag Length
        /// </summary>
        public static readonly int Packet_Start_Length = PacketDef.Packet_Start.Length;
        /// <summary>
        /// Packet Version_Tag Length
        /// </summary>
        public static readonly int Packet_Version_Length = PacketDef.Packet_Version.Length;
        /// <summary>
        /// Packet Length_Tag Length
        /// </summary>
        public static readonly int Packet_LengthTag_Length = 4;
        /// <summary>
        /// Packet Number_Tag Length
        /// </summary>
        public static readonly int Packet_PacketNumber_Length = 4;
        /// <summary>
        /// Packet Crc16_Tag Length
        /// </summary>
        public static readonly int Packet_Crc16Code_Length = 2;
        #endregion

        #region Tag Convert To Byte[]
        /// <summary>
        /// Client Requst File Tag
        /// </summary>
        /// <returns></returns>
        public static byte[] ClientRequestFileTag()
        {
            return PacketDef.Packet_Start.Concat(PacketDef.Packet_Version).Concat(PacketDef.Packet_Client_RequestFile).ToArray();
        }


        /// <summary>
        /// Server Response File Tag
        /// </summary>
        /// <returns></returns>
        public static byte[] ServerResponseFileTag()
        {
            return PacketDef.Packet_Start.Concat(PacketDef.Packet_Version).Concat(PacketDef.Packet_Server_ResponseFile).ToArray();
        }


        /// <summary>
        /// Client Send Find Udpate File Tag
        /// </summary>
        /// <returns></returns>
        public static byte[] ClientFindFileInfoTag()
        {
            return PacketDef.Packet_Start.Concat(PacketDef.Packet_Version).Concat(PacketDef.Packet_Client_FindUpdateFile).ToArray();
        }


        /// <summary>
        /// Server Response Has Found Update File Tag
        /// </summary>
        /// <returns></returns>
        public static byte[] ServerFoundFileInfoTag()
        {
            return PacketDef.Packet_Start.Concat(PacketDef.Packet_Version).Concat(PacketDef.Packet_Server_FoundUpdateFile).ToArray();
        }


        /// <summary>
        /// Client Send HeartBeat Tag
        /// </summary>
        /// <returns></returns>
        public static byte[] ClientRequestHeartBeatTag()
        {
            return PacketDef.Packet_Start.Concat(PacketDef.Packet_Version).Concat(PacketDef.Packet_Client_RequestHeartBeat).ToArray();
        }


        /// <summary>
        /// Server Response HeartBeat Tag
        /// </summary>
        /// <returns></returns>
        public static byte[] ServerResponseHeartBeatTag()
        {
            return PacketDef.Packet_Start.Concat(PacketDef.Packet_Version).Concat(PacketDef.Packet_Server_ResponseHeartBeat).ToArray();
        }
        #endregion



        /// <summary>
        /// Packet Data
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <param name="packetNumber"></param>
        /// <returns></returns>
        public static byte[] PacketData(byte[] tag, byte[] data, byte[] packetNumber = null)
        {
            // A Packet = tag(start_tag + version_tag + request/response_tag) + length_tag + data + crc16_tag         

            int startTagLength = tag.Length;
            int dataLenght = 0;
            if (data != null)
            {
                if (packetNumber == null)
                {
                    dataLenght = data.Length;
                }
                else
                {
                    dataLenght = packetNumber.Length + data.Length;
                    data = packetNumber.Concat(data).ToArray();
                }
            }

            int crcLength = Packet_Crc16Code_Length;
            byte[] packetLengthWithoutself = BitConverter.GetBytes(startTagLength + dataLenght + crcLength);
            byte[] packetLength = BitConverter.GetBytes(startTagLength + packetLengthWithoutself.Length + dataLenght + crcLength);
            byte[] packetWithoutcrc = data == null
                                      ? tag.Concat(packetLength).ToArray()
                                      : tag.Concat(packetLength).Concat(data).ToArray();
            byte[] crcCode = GetPacketCrc(packetWithoutcrc);
            byte[] packetInfo = packetWithoutcrc.Concat(crcCode).ToArray();
            return packetInfo;
        }



        /// <summary>
        /// Get Crc16 Code From Packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static byte[] GetPacketCrc(byte[] packet)
        {
            return BitConverter.GetBytes(Crc16Utils.GetCrc16Code(packet));
        }



        /// <summary>
        /// Byte[] Compare
        /// </summary>
        /// <param name="b1">first byte[]</param>
        /// <param name="b2">second byte[]</param>
        /// <param name="result">same return 0; first>second return negative; first > second return positive;</param>
        /// <returns></returns>
        [DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr memcmp(byte[] b1, byte[] b2, IntPtr result);
        public static int BytesCompare(byte[] b1, byte[] b2)
        {
            IntPtr retval = memcmp(b1, b2, new IntPtr(b1.Length));
            return retval.ToInt32();
        }



        /// <summary>
        /// Check Packet Receive Competely
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static bool IsPacketComplete(byte[] packet)
        {
            var receiveLength = packet.Length;
            var theoryLength = GetPacketLength(packet);
            if (receiveLength == theoryLength)
            {
                return true;
            }
            return false;
        }



        /// <summary>
        /// Get Packet Length
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static int GetPacketLength(byte[] packet)
        {
            return BitConverter.ToInt32(packet
                               .Take(ClientRequestFileTag().Length + Packet_LengthTag_Length)
                               .Skip(ClientRequestFileTag().Length).ToArray(), 0);
        }





        /// <summary>
        /// Client Get Packet Number From Response 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static int GetResponsePacketNumber(byte[] packet)
        {
            var tempData = packet.Skip(ClientRequestFileTag().Length + Packet_LengthTag_Length).ToArray();
            var resultData = tempData.Take(Packet_PacketNumber_Length).ToArray();
            var packetNumber = BitConverter.ToInt32(resultData, 0);
            return packetNumber;
        }


        /// <summary>
        /// Get Data Part in Packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static byte[] GetData(byte[] tag, byte[] packet)
        {
            if (BytesCompare(tag, ServerResponseFileTag()) == 0)
            {
                var tempData = packet.Skip(tag.Length + Packet_LengthTag_Length + Packet_PacketNumber_Length).ToArray();
                return tempData.Take(tempData.Length - Packet_Crc16Code_Length).ToArray();
            }
            else
            {
                var tempData = packet.Skip(tag.Length + Packet_LengthTag_Length).ToArray();
                return tempData.Take(tempData.Length - Packet_Crc16Code_Length).ToArray();
            }
        }


        /// <summary>
        /// Get Download Packet Size according to server file and channels count
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="downloadChannelsCount"></param>
        /// <returns></returns>
        public static int GetPacketSize(string filePath, int downloadChannelsCount)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                FileInfo severFileInfo = new FileInfo(filePath);
                return (int)(severFileInfo.Length / downloadChannelsCount);
            }
            return 0;
        }


        /// <summary>
        /// Server Get Request File Start Position 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static int GetRequestFileStartPosition(byte[] packet)
        {
            var tempData = packet.Skip(ClientRequestFileTag().Length + Packet_LengthTag_Length).ToArray();
            var resultData = tempData.Take(tempData.Length - Packet_Crc16Code_Length).ToArray();
            var start = BitConverter.ToInt32(resultData, 0);
            return start;
        }



        /// <summary>
        /// Split Packets According to Tag
        /// </summary>
        /// <param name="receivedata"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static List<byte[]> SplitBytes(byte[] receivedata, byte[] tag)
        {
            //match tag
            var packetStartIndexList = new List<int>();
            for (var i = 0; i < receivedata.Length; i++)
            {
                if (i == receivedata.Length - 3) break;
                packetStartIndexList.AddRange(tag.TakeWhile((t, j) => j != tag.Length - 3)
                                      .Where((t, j) => receivedata[i] == t &&
                                          receivedata[i + 1] == tag[j + 1] &&
                                          receivedata[i + 2] == tag[j + 2] &&
                                          receivedata[i + 3] == tag[j + 3])
                                      .Select(t => i));
            }


            switch (packetStartIndexList.Count)
            {
                //0 match
                case 0:
                    return null;

                //1 match
                case 1:
                    try
                    {
                        var index = packetStartIndexList.FirstOrDefault();
                        var packetLength = GetPacketLength(receivedata.Skip(index).ToArray());
                        return new List<byte[]> { receivedata.Skip(index).Take(packetLength).ToArray() };
                    }
                    catch
                    {
                        return null;
                    }

                //>1 matches
                default:
                    var result = new List<byte[]>();
                    var packLength = packetStartIndexList[1] - packetStartIndexList[0];
                    for (var index = 0; index < packetStartIndexList.Count; index++)
                    {
                        var packetIndex = packetStartIndexList[index];
                        var destbyte = new byte[packLength];

                        if (index == packetStartIndexList.LastOrDefault())
                        {
                            try
                            {
                                Array.Copy(receivedata, packetIndex, destbyte, 0, packLength);
                                result.Add(destbyte);
                            }
                            catch
                            {
                                break;
                            }
                        }
                        else
                        {

                            Array.Copy(receivedata, packetIndex, destbyte, 0, packLength);
                            result.Add(destbyte);
                        }
                    }
                    return result;
            }
        }
    }
}
