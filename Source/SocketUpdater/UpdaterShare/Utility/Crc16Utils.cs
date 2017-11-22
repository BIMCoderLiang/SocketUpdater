using System.Linq;

namespace UpdaterShare.Utility
{
    public static class Crc16Utils
    {
        /// <summary>
        /// Get Crc16 value From byte[]
        /// </summary>
        /// <param name="data">import data</param>
        /// <returns></returns>
        public static ushort GetCrc16Code(byte[] data)
        {
            ushort crc = 0;
            foreach (var bt in data)
            {
                crc ^= (ushort)(bt << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc;
        }


        /// <summary>
        /// Ckeck CrcCode of packet byte[]
        /// </summary>
        /// <param name="importData"></param>
        /// <returns></returns>
        public static bool CheckCrcCode(byte[] importData)
        {
            byte[] receivedata = importData;
            byte[] packetInfoWithoutCrc = receivedata.Take(receivedata.Length - PacketUtils.Packet_Crc16Code_Length).ToArray();
            byte[] localCalcCrc = PacketUtils.GetPacketCrc(packetInfoWithoutCrc);
            byte[] serverCrc = receivedata.Skip(receivedata.Length - PacketUtils.Packet_Crc16Code_Length).ToArray();
            var compareCrc = PacketUtils.BytesCompare(localCalcCrc, serverCrc);
            if (compareCrc == 0)
            {
                return true;
            }
            return false;
        }
    }
}
