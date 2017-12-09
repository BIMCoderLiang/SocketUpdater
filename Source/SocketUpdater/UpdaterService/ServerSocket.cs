using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UpdaterShare.GlobalSetting;
using UpdaterShare.Model;
using UpdaterShare.Utility;

namespace UpdaterService
{  
    public static class ServerSocket
    {
        private static int _downloadChannelsCount;
        private static string _serverPath;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);

        public static void StartServer(int port, int backlog)
        {         
            _downloadChannelsCount = DownloadSetting.DownloadChannelsCount;
            try
            {
                IPAddress ipAddress = IPAddress.Any;
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(backlog);

                while (true)
                {
                    AllDone.Reset();
                    listener.BeginAccept(AcceptCallback, listener);
                    AllDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\RunLog.txt";
                File.AppendAllText(path, ex.Message);
            }
        }


        private static void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            ComObject state = new ComObject { WorkSocket = handler };
            handler.BeginReceive(state.Buffer, 0, ComObject.BufferSize, 0, FindUpdateFileCallback, state);
        }


        private static void FindUpdateFileCallback(IAsyncResult ar)
        {
            ComObject state = (ComObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                var receiveData = state.Buffer.Take(bytesRead).ToArray();
                var dataList = PacketUtils.SplitBytes(receiveData, PacketUtils.ClientFindFileInfoTag());
                if (dataList != null && dataList.Any())
                {
                    var request = PacketUtils.GetData(PacketUtils.ClientFindFileInfoTag(), dataList.FirstOrDefault());
                    string str = System.Text.Encoding.UTF8.GetString(request);
                    var infos = str.Split('_');
                    var productName = infos[0];
                    var revitVersion = infos[1];
                    var currentVersion = infos[2];

                    var updatefile = ServerFileUtils.GetLatestFilePath(productName, revitVersion, currentVersion);
                    if (string.IsNullOrEmpty(updatefile) || !File.Exists(updatefile)) return;
                    _serverPath = updatefile;
                    FoundUpdateFileResponse(handler);
                }
            }
        }


        private static void FoundUpdateFileResponse(Socket handler)
        {
            byte[] foundUpdateFileData = PacketUtils.PacketData(PacketUtils.ServerFoundFileInfoTag(),null);
            ComObject state = new ComObject { WorkSocket = handler };
            handler.BeginSend(foundUpdateFileData, 0, foundUpdateFileData.Length, 0, HasFoundUpdateFileCallback, state);
        }


        private static void HasFoundUpdateFileCallback(IAsyncResult ar)
        {
            ComObject state = (ComObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            handler.BeginReceive(state.Buffer, 0, ComObject.BufferSize, 0, ReadFilePositionRequestCallback, state);
        }


        private static void ReadFilePositionRequestCallback(IAsyncResult ar)
        {
            ComObject state = (ComObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                var receiveData = state.Buffer.Take(bytesRead).ToArray();
                var dataList = PacketUtils.SplitBytes(receiveData, PacketUtils.ClientRequestFileTag());
                if (dataList != null)
                {
                    foreach (var request in dataList)
                    {
                        if (PacketUtils.IsPacketComplete(request))
                        {
                            int startPosition = PacketUtils.GetRequestFileStartPosition(request); 
                            SendFileResponse(handler, startPosition);
                        }
                    }
                }
            }
        }

        private static void SendFileResponse(Socket handler, int startPosition)
        {
            var packetSize = PacketUtils.GetPacketSize(_serverPath, _downloadChannelsCount);
            if (packetSize != 0)
            {
                byte[] filedata = FileUtils.GetFile(_serverPath, startPosition, packetSize);
                byte[] packetNumber = BitConverter.GetBytes(startPosition/packetSize);
                if (filedata != null)
                {
                    byte[] segmentedFileResponseData = PacketUtils.PacketData(PacketUtils.ServerResponseFileTag(), filedata, packetNumber);
                    ComObject state = new ComObject {WorkSocket = handler};
                    handler.BeginSend(segmentedFileResponseData, 0, segmentedFileResponseData.Length, 0, SendFileResponseCallback, state);
                }
            }
            else
            {               
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }       
        }


        private static void SendFileResponseCallback(IAsyncResult ar)
        {
            try
            {
                ComObject state = (ComObject)ar.AsyncState;
                Socket handler = state.WorkSocket;
                handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();             
            }
            catch (Exception e)
            {

            }
        }
    }
}
