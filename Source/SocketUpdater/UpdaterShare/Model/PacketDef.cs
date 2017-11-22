
namespace UpdaterShare.Model
{
    public static class PacketDef
    {
        #region 数据包标识字节定义
        /// <summary>
        /// Packet Start Tag
        /// </summary>
        public static readonly byte[] Packet_Start = { 0xAA, 0x55 };
        /// <summary>
        /// Packet Version Tag
        /// </summary>
        public static readonly byte[] Packet_Version = { 0x01 };
        /// <summary>
        /// Client Request File Packet Tag
        /// </summary>
        public static readonly byte[] Packet_Client_RequestFile = { 0x01 };
        /// <summary>
        /// Server Response File Packet Tag
        /// </summary>
        public static readonly byte[] Packet_Server_ResponseFile = { 0x02 };
        /// <summary>
        /// Client Send Basic Info Tag
        /// </summary>
        public static readonly byte[] Packet_Client_FindUpdateFile = {0x03};
        /// <summary>
        /// Server Receivce Basic Into Tag
        /// </summary>
        public static readonly byte[] Packet_Server_FoundUpdateFile = {0x04};
        /// <summary>
        /// Client Send HeartBeat Tag
        /// </summary>
        public static readonly byte[] Packet_Client_RequestHeartBeat = { 0x05 };
        /// <summary>
        /// Server Response HeartBeat Tag
        /// </summary>
        public static readonly byte[] Packet_Server_ResponseHeartBeat = { 0x06 };
        #endregion
    }
}
