using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpdaterShare.GlobalSetting;
using UpdaterShare.Model;
using UpdaterShare.Utility;

namespace UpdaterClient
{
    public static class ClientSocket
    {
        private static int _downloadChannelsCount;     
        private static bool _isPacketsComplete;
        private static long _packSize;
        private static string _updateFileName;
        private static readonly ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        private static readonly Dictionary<Socket, byte[]> TempReceivePacketDict = new Dictionary<Socket, byte[]>();
        private static readonly Dictionary<int, byte[]> ResultPacketDict = new Dictionary<int, byte[]>();

        public static bool StartClient(ClientBasicInfo basicInfo, DownloadFileInfo dlInfo, ClientLinkInfo clInfo,
                                       string localSaveFolderPath, string tempFilesDir)
        {
            if (basicInfo == null || dlInfo == null || clInfo == null ||
                string.IsNullOrEmpty(localSaveFolderPath) || string.IsNullOrEmpty(tempFilesDir))
                return false;
            if (!Directory.Exists(localSaveFolderPath))
            {
                Directory.CreateDirectory(localSaveFolderPath);
            }
            if (!Directory.Exists(tempFilesDir))
            {
                Directory.CreateDirectory(tempFilesDir);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(tempFilesDir);
                di.Delete(true);
                Directory.CreateDirectory(tempFilesDir);
            }


            var updateFileName = $"{basicInfo.ProductName}_{basicInfo.RevitVersion}_{basicInfo.CurrentProductVersion}_{dlInfo.LatestProductVersion}";
            _updateFileName = updateFileName;

            var localSavePath = Path.Combine(localSaveFolderPath,$"{updateFileName}.zip");
            var downloadChannelsCount = DownloadSetting.DownloadChannelsCount;
            _downloadChannelsCount = downloadChannelsCount;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(clInfo.IpString);
                IPEndPoint remoteEp = new IPEndPoint(ipAddress, clInfo.Port);

                int packetCount = downloadChannelsCount;
                long packetSize = dlInfo.DownloadFileTotalSize / packetCount;
                _packSize = packetSize;


                var tasks = new Task[packetCount];
                for (int index = 0; index < packetCount; index++)
                {
                    int packetNumber = index;
                    var task = new Task(() =>
                    {                  
                        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        ComObject state = new ComObject { WorkSocket = client, PacketNumber = packetNumber };
                        client.BeginConnect(remoteEp, ConnectCallback, state);
                    });                  
                    tasks[packetNumber] = task;
                    task.Start();
                }
                Task.WaitAll(tasks);
                ReceiveDone.WaitOne();


                _isPacketsComplete = CheckPackets(TempReceivePacketDict, _downloadChannelsCount);
                CloseSockets(TempReceivePacketDict);
                if (_isPacketsComplete)
                {
                    GenerateTempFiles(ResultPacketDict, downloadChannelsCount, tempFilesDir);
                    FileUtils.CombineTempFiles(localSavePath, tempFilesDir);
                    return Md5Utils.IsMd5Equal(dlInfo.DownloadFileMd5, Md5Utils.GetFileMd5(localSavePath));
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        #region  Step1:Connection
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                ComObject state = (ComObject)ar.AsyncState;
                Socket client = state.WorkSocket;
                client.EndConnect(ar);
                FindUpdateFileInfo(client, state.PacketNumber);
            }
            catch
            {
                
            }
        }
        #endregion


        #region Step2:Find Update File According to ProductName and Latest Version
        private static void FindUpdateFileInfo(Socket client, int packetNumber)
        {
            ComObject state = new ComObject { WorkSocket = client, PacketNumber = packetNumber };
            byte[] byteData = PacketUtils.PacketData(PacketUtils.ClientFindFileInfoTag(), Encoding.UTF8.GetBytes(_updateFileName));
            client.BeginSend(byteData, 0, byteData.Length, 0, FindUpdateFileCallback, state);
        }
        private static void FindUpdateFileCallback(IAsyncResult ar)
        {
            try
            {
                ComObject state = (ComObject)ar.AsyncState;
                Socket client = state.WorkSocket;
                client.EndSend(ar);
                client.BeginReceive(state.Buffer, 0, ComObject.BufferSize, 0, FoundFileCallback, state);
            }
            catch
            {
                
            }
        }
        private static void FoundFileCallback(IAsyncResult ar)
        {
            try
            {
                ComObject state = (ComObject)ar.AsyncState;
                Socket client = state.WorkSocket;
                int packetNumber = state.PacketNumber;
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    var receiveData = state.Buffer.Take(bytesRead).ToArray();
                    var dataList = PacketUtils.SplitBytes(receiveData, PacketUtils.ServerFoundFileInfoTag());
                    if (dataList != null && dataList.Any())
                    {
                        SendFileStartPositionInfo(client, packetNumber, _packSize);
                    }
                }
            }
            catch
            {
                
            }
        }
        #endregion


        #region Step3:Request Packets of UpdateFile and Ready to Receive File
        private static void SendFileStartPositionInfo(Socket client, int packetNumber, long packSize)
        {
            ComObject state = new ComObject { WorkSocket = client };
            byte[] byteData = PacketUtils.PacketData(PacketUtils.ClientRequestFileTag(), BitConverter.GetBytes(packetNumber * packSize));
            client.BeginSend(byteData, 0, byteData.Length, 0, SendFileRequestCallback, state);
        }
        private static void SendFileRequestCallback(IAsyncResult ar)
        {
            try
            {
                ComObject state = (ComObject)ar.AsyncState;
                Socket client = state.WorkSocket;
                client.EndSend(ar);
                client.BeginReceive(state.Buffer, 0, ComObject.BufferSize, 0, ReceiveFileCallback, state);
            }
            catch
            {
                
            }
        }


        private static void ReceiveFileCallback(IAsyncResult ar)
        {
            try
            {
                ComObject state = (ComObject)ar.AsyncState;
                Socket client = state.WorkSocket;
                int bytesRead = client.EndReceive(ar);
                Console.WriteLine(bytesRead);

                if (bytesRead > 0)
                {
                    if (TempReceivePacketDict.ContainsKey(client))
                    {
                        TempReceivePacketDict[client] =
                            TempReceivePacketDict[client].Concat(state.Buffer.Take(bytesRead)).ToArray();
                    }
                    else
                    {
                        TempReceivePacketDict.Add(client, state.Buffer.Take(bytesRead).ToArray());
                    }
                    client.BeginReceive(state.Buffer, 0, ComObject.BufferSize, 0, ReceiveFileCallback, state);
                }
                else
                {
                    if (TempReceivePacketDict.Count == _downloadChannelsCount)
                    {
                        ReceiveDone.Set();
                    }                 
                }
            }
            catch
            {
                
            }
        }
        #endregion


        #region Step4:Check Packets
        private static bool CheckPackets(Dictionary<Socket, byte[]> dictionary, int downloadChannelsCount)
        {
            foreach (var socket in dictionary.Keys)
            {
                var response = dictionary[socket];
                var packets = PacketUtils.SplitBytes(response, PacketUtils.ServerResponseFileTag());
                if (packets != null && packets.Any())
                {
                    var result = packets.FirstOrDefault();
                    if (Crc16Utils.CheckCrcCode(result) && PacketUtils.IsPacketComplete(result))
                    {
                        var packetNumber = PacketUtils.GetResponsePacketNumber(result);
                        var data = PacketUtils.GetData(PacketUtils.ServerResponseFileTag(), result);
                        if (!ResultPacketDict.ContainsKey(packetNumber))
                        {
                            ResultPacketDict.Add(packetNumber, data);
                        }                     
                    }
                }
            }
            return ResultPacketDict.Count == downloadChannelsCount;
        }
        #endregion


        #region Step5:Close Sockets
        private static void CloseSockets(Dictionary<Socket, byte[]> tempReceivePacketDict)
        {
            foreach (var socket in tempReceivePacketDict.Keys)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }
        #endregion


        #region Step6:Generate Temp Flies
        private static void GenerateTempFiles(Dictionary<int, byte[]> dictionary, int downloadChannelsCount, string tempFilesDir)
        {
            dictionary = dictionary.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            foreach (var packetNumber in dictionary.Keys)
            {
                var data = ResultPacketDict[packetNumber];
                string tempfilepath = Path.Combine(tempFilesDir, $"{packetNumber}_{downloadChannelsCount}");
                FileUtils.GenerateTempFile(tempfilepath, data);
            }
        }
        #endregion
    }
}
