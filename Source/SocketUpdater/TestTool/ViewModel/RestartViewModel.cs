using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using DevExpress.Mvvm;
using Microsoft.Win32;
using UpdaterClient.Utility;
using UpdaterShare.Utility;

namespace TestTool.ViewModel
{
    public class RestartViewModel:BindableBase
    {
        public ICommand RestartCmd { get; }
        private readonly string _latestVersion;
        private bool _isNow = true;

        public bool IsNow
        {
            get { return _isNow; }
            set
            {
                SetProperty(ref _isNow, value, () => IsNow);
            }
        }
        public RestartViewModel(string latestVersion)
        {
            _latestVersion = latestVersion;
            RestartCmd = new DelegateCommand(RestartRevit);
        }

        private void RestartRevit()
        {
            //Write xml (Temp\BIMProductUpdate\updateinfo.xml)
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode header = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(header);
            XmlElement rootNode = xmlDoc.CreateElement("UpdateInfo");

            var mainFolder = Path.Combine(Path.GetTempPath(), "BIMProductUpdate");
            var updateInfoFile = Path.Combine(mainFolder, "updateinfo.xml");
            var registryPath = @"SOFTWARE\BIMProduct\BIMProduct2018\Revit2016";
            RegistryKey rsg = Registry.LocalMachine.OpenSubKey(registryPath, false);
            if (rsg != null)
            {
                foreach (var kv in rsg.GetValueNames())
                {
                    rootNode.AppendChild(XmlUtils.InsertNode(xmlDoc, "Node", kv,
                        RegistryUtils.ReadRegistryInfo(Registry.LocalMachine, registryPath, kv)));
                }
                rootNode.AppendChild(XmlUtils.InsertNode(xmlDoc, "Node", "LatestVersion", _latestVersion));
                rootNode.AppendChild(XmlUtils.InsertNode(xmlDoc, "Node", "Status", IsNow ? "Now" : "Auto"));
                xmlDoc.AppendChild(rootNode);
                xmlDoc.Save(updateInfoFile);

                var updateExeFolderPath = AppDomain.CurrentDomain.BaseDirectory.Replace("TestTool", "UpdaterRestart");
                if (!IsNow)
                {
                    BootUtils.CreateLnk("Restart", mainFolder, Path.Combine(updateExeFolderPath, "Restart.vbs"));
                }

                //Start Process  (UpdaterRestart.exe) 
                string restartExePath = Path.Combine(updateExeFolderPath, "UpdaterRestart.exe");
                Process.Start(restartExePath);
            }
            else
            {
                MessageBox.Show("Not Found Registry Info!");
            }
        }
    }
}
