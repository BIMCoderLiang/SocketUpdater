using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Microsoft.Win32;
using TestTool.Model;
using UpdaterClient.Utility;
using UpdaterShare.Model;

namespace TestTool.ViewModel
{
    public class ClientRequestInfoViewModel:BindableBase
    {
        public ObservableCollection<ClientInfoModel> RequestInfos { get; set; }
        public ObservableCollection<ClientInfoModel> ReceiveInfos { get; set; }
        public ICommand ConnectCmd { get; }

        public ClientBasicInfo ClientInfo { get; set; }
        public DownloadFileInfo DownloadInfo { get; set; }

        public ClientRequestInfoViewModel()
        {
            RequestInfos = new ObservableCollection<ClientInfoModel>();
            ReceiveInfos = new ObservableCollection<ClientInfoModel>();
            InitData();

            ConnectCmd = new DelegateCommand(ConnectServer);
        }


        private void InitData()
        {
            RequestInfos.Clear();
            ReceiveInfos.Clear();

            var registryPath = @"SOFTWARE\BIMProduct\BIMProduct2018\Revit2016";
            var localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);

            ClientInfoModel r1 = new ClientInfoModel()
            {
                RName = "ProductNameInEng",
                RValue = RegistryUtils.ReadRegistryInfo(localMachineRegistry, registryPath, "ProductNameInEng")
            };

            ClientInfoModel r2 = new ClientInfoModel()
            {
                RName = "RevitVersion",
                RValue = RegistryUtils.ReadRegistryInfo(localMachineRegistry, registryPath, "RevitVersion")
            };


            ClientInfoModel r3 = new ClientInfoModel()
            {
                RName = "ProductVersion",
                RValue = RegistryUtils.ReadRegistryInfo(localMachineRegistry, registryPath, "ProductVersion")
            };

            RequestInfos.Add(r1);
            RequestInfos.Add(r2);
            RequestInfos.Add(r3);

        }

        private void ConnectServer()
        {
            try
            {
                ClientBasicInfo cb = new ClientBasicInfo()
                {
                    ProductName = RequestInfos.FirstOrDefault(x => string.Equals(x.RName,"ProductNameInEng", StringComparison.OrdinalIgnoreCase))?.RValue,
                    RevitVersion = RequestInfos.FirstOrDefault(x => string.Equals(x.RName, "RevitVersion", StringComparison.OrdinalIgnoreCase))?.RValue,
                    CurrentProductVersion = RequestInfos.FirstOrDefault(x => string.Equals(x.RName,"ProductVersion", StringComparison.OrdinalIgnoreCase))?.RValue
                };
                ClientInfo = cb;
                DownloadFileInfo df = new DownloadFileInfo();

                var result = RequestInfoUtils.RequestDownloadFileInfo(cb, "http://localhost:55756/", "GetFileInfo", "GetInfo", ref df);
                DownloadInfo = df;
                if (result)
                {                 
                    ClientInfoModel r1 = new ClientInfoModel()
                    {
                        RName = "LatestProductVersion",
                        RValue = df.LatestProductVersion
                    };
                    ClientInfoModel r2 = new ClientInfoModel()
                    {
                        RName = "DownloadFileMd5",
                        RValue = df.DownloadFileMd5
                    };
                    ClientInfoModel r3 = new ClientInfoModel()
                    {
                        RName = "DownloadFileTotalSize",
                        RValue = df.DownloadFileTotalSize.ToString()
                    };
                    ReceiveInfos.Add(r1);
                    ReceiveInfos.Add(r2);
                    ReceiveInfos.Add(r3);
                }
                else
                {
                    MessageBox.Show("Get DownLoad File Failed!");
                }
            }
            catch 
            {
                MessageBox.Show("Get DownLoad File Failed!");
            }          
        }
    }
}
