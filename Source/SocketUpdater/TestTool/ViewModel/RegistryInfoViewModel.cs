using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Microsoft.Win32;
using TestTool.Model;
using UpdaterClient.Utility;

namespace TestTool.ViewModel
{
    public class RegistryInfoViewModel : BindableBase
    {
        public ObservableCollection<RegistryInfoModel> RegistryInfos { get; set; }
        public ICommand GenerateCmd { get; }
        public ICommand DeleteCmd { get; }
        public RegistryInfoViewModel()
        {
            RegistryInfos = new ObservableCollection<RegistryInfoModel>();
            InitData();

            GenerateCmd = new DelegateCommand(GenerateRegistryInfo);
            DeleteCmd = new DelegateCommand(DeleteRegistryInfo);
        }

        private void InitData()
        {
            RegistryInfos.Clear();
            RegistryInfoModel r1 = new RegistryInfoModel() { RKey = "InstallationDateTime", RValue = DateTime.Now.ToString()};
            RegistryInfoModel r2 = new RegistryInfoModel() { RKey = "InstallationLocation", RValue = @"C:\Program Files\BIMProduct\BIMProductUpdate For Revit 2016\" };
            RegistryInfoModel r3 = new RegistryInfoModel() { RKey = "ProductName", RValue = "BIMProduct" };
            RegistryInfoModel r4 = new RegistryInfoModel() { RKey = "ProductNameInEng", RValue = "BIMProduct2018" };
            RegistryInfoModel r5 = new RegistryInfoModel() { RKey = "ProductVersion", RValue = "18.1.4.0" };
            RegistryInfoModel r6 = new RegistryInfoModel() { RKey = "RevitVersion", RValue = "Revit2016" };
            RegistryInfos.Add(r1);
            RegistryInfos.Add(r2);
            RegistryInfos.Add(r3);
            RegistryInfos.Add(r4);
            RegistryInfos.Add(r5);
            RegistryInfos.Add(r6);
        }

        private void GenerateRegistryInfo()
        {
            try
            {
                var registryPath = @"SOFTWARE\BIMProduct\BIMProduct2018\Revit2016";
                var localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                foreach (var r in RegistryInfos)
                {
                    RegistryUtils.CreateRegistryInfo(localMachineRegistry, registryPath, r.RKey, r.RValue);
                }
                MessageBox.Show("Create Registry Info Success!");
            }
            catch
            {
                MessageBox.Show("Create Registry Info Failed!");
            }
            
        }

        private void DeleteRegistryInfo()
        {
            var localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            var result = RegistryUtils.DeleteRegistryInfo(localMachineRegistry, @"SOFTWARE\BIMProduct");
            MessageBox.Show(result ? "Delete Registry Info Success!" : "Delete Registry Info Failed!");
        }
    }
}
