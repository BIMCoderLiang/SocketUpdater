using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using TestTool.Model;
using UpdaterClient;
using UpdaterShare.Model;

namespace TestTool.ViewModel
{
    public class ClientDownLoadViewModel:BindableBase
    {
        public ObservableCollection<LinkInfo> LinkInfos { get; set; }
        public ICommand DownloadCmd { get; }

        private ClientBasicInfo ClientInfo { get; }
        private DownloadFileInfo DownloadInfo { get; }
        public ClientDownLoadViewModel(ClientBasicInfo cb, DownloadFileInfo dl)
        {
            ClientInfo = cb;
            DownloadInfo = dl;

            LinkInfos = new ObservableCollection<LinkInfo>();
            InitData();    

            DownloadCmd = new DelegateCommand(StartDownload);
            
        }

        private void InitData()
        {
            var mainFolder = Path.Combine(Path.GetTempPath(), "BIMProductUpdate");
            LinkInfos.Clear();
            LinkInfo li1 = new LinkInfo() {LName = "IpString", LValue= "127.0.0.1"};
            LinkInfo li2 = new LinkInfo() { LName = "Port", LValue = "8885" };
            LinkInfo li3 = new LinkInfo() { LName = "LocalSavePath", LValue = Path.Combine(mainFolder, "downLoad") };
            LinkInfo li4 = new LinkInfo() { LName = "TempDirPath", LValue = Path.Combine(mainFolder, "temp") };
            LinkInfos.Add(li1);
            LinkInfos.Add(li2);
            LinkInfos.Add(li3);
            LinkInfos.Add(li4);
        }

        private void StartDownload()
        {
            try
            {
                ClientLinkInfo clInfo = new ClientLinkInfo()
                {
                    IpString = LinkInfos.FirstOrDefault(x => string.Equals(x.LName,"IpString",StringComparison.OrdinalIgnoreCase))?.LValue,
                    Port = Convert.ToInt32(LinkInfos.FirstOrDefault(x => string.Equals(x.LName, "Port", StringComparison.OrdinalIgnoreCase))?.LValue)
                };

                var localSavePath = LinkInfos.FirstOrDefault(x => string.Equals(x.LName,"LocalSavePath", StringComparison.OrdinalIgnoreCase))?.LValue;
                var tempDirPath = LinkInfos.FirstOrDefault(x => string.Equals(x.LName,"TempDirPath", StringComparison.OrdinalIgnoreCase))?.LValue;

                var clientSocket = new ClientSocket();
                var result = clientSocket.StartClient(ClientInfo, DownloadInfo,clInfo, localSavePath, tempDirPath);
                if (result)
                {
                    MessageBox.Show("DownLoad File Success!");
                }
                else
                {
                    MessageBox.Show("DownLoad File Failed!");
                }
            }
            catch 
            {
                MessageBox.Show("DownLoad File Failed!");
            }       
        }
    }
}
