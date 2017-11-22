using System.Net.Sockets;

namespace UpdaterShare.Model
{
    public class ComObject
    {
        public Socket WorkSocket = null;
        // Why the BufferSize is Set 1024*80
        // https://stackoverflow.com/questions/1540658/net-asynchronous-stream-read-write
        public const int BufferSize = 1024 * 80;
        public byte[] Buffer = new byte[BufferSize];
        public int PacketNumber;
    }
}
