using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using UpdaterClient.Utility;
using UpdaterRestart.Utility;
using UpdaterShare.Utility;

namespace UpdaterRestart
{
    public class ExecuteUpdate
    {
        static void Main(string[] args)
        {
            var mainFolder = Path.Combine(Path.GetTempPath(), "BIMProductUpdate");
            //Temp\BIMProductUpdate\updateinfo.xml
            var updateInfoFile = Path.Combine(mainFolder, "updateinfo.xml");
            if (!File.Exists(updateInfoFile)) return;

            //Temp\BIMProductUpdate\downLoad
            var zipFolder = Path.Combine(mainFolder, "downLoad");
            if (!Directory.Exists(zipFolder)) return;
            var zipFile = Directory.GetFiles(zipFolder).FirstOrDefault(x => x.Contains(".zip"));
            if (string.IsNullOrEmpty(zipFile)) return;

            //Temp\BIMProductUpdate\updateFiles
            var unzipFolder = Path.Combine(mainFolder, "updateFiles");
            if (!Directory.Exists(unzipFolder))
            {
                Directory.CreateDirectory(unzipFolder);
            }

            //Unzip
            using (FileStream zipReadStream = new FileStream(zipFile, FileMode.Open, FileAccess.Read))
            {
                ZipUtils.UnZip(zipReadStream, unzipFolder);
            }
            var unzipFilesFolder = Path.Combine(unzipFolder, Path.GetFileNameWithoutExtension(zipFile));
            if (!Directory.Exists(unzipFilesFolder)) return;
            var unzipFiles = Directory.GetFiles(unzipFilesFolder);
            foreach (var file in unzipFiles)
            {
                File.Copy(file, Path.Combine(unzipFolder, Path.GetFileName(file)),true);
            }
            Directory.Delete(unzipFilesFolder, true);



            //See ExampleFile/updateInfo.xml
            var installationLocation = XmlUtils.GetValueByKey(updateInfoFile, "UpdateInfo", "InstallationLocation");
            var revitVersion = XmlUtils.GetValueByKey(updateInfoFile, "UpdateInfo", "RevitVersion");
            var latestVersion = XmlUtils.GetValueByKey(updateInfoFile, "UpdateInfo", "LatestVersion");
            var status = XmlUtils.GetValueByKey(updateInfoFile, "UpdateInfo", "Status");

            var registryPath = $"SOFTWARE\\BIMProduct\\BIMProduct2018\\{revitVersion}";
            if (string.Equals(status, "Now", StringComparison.CurrentCultureIgnoreCase))
            {
                string revitExePath = string.Empty;
                var isKilled = ProcessUtils.KillProcess("Revit", ref revitExePath);
                if (!isKilled) return;
                var isCovered = ReplaceFilesUtils.CoverFiles(installationLocation, unzipFolder);
                if (!isCovered) return;
                try
                {
                    Directory.Delete(mainFolder);
                    RegistryUtils.WriteRegistryInfo(Registry.LocalMachine, registryPath, "ProductVersion", latestVersion);
                    RegistryUtils.WriteRegistryInfo(Registry.LocalMachine, registryPath, "InstallationDateTime",
                                                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    ProcessUtils.StartProcess(revitExePath);
                }
                catch
                {
                   
                }
            }

            else if (string.Equals(status, "Auto", StringComparison.CurrentCultureIgnoreCase))
            {
                var isCovered = ReplaceFilesUtils.CoverFiles(installationLocation, unzipFolder);
                if (!isCovered) return;
                try
                {
                    Directory.Delete(mainFolder,true);
                    RegistryUtils.WriteRegistryInfo(Registry.LocalMachine, registryPath, "ProductVersion", latestVersion);
                    RegistryUtils.WriteRegistryInfo(Registry.LocalMachine, registryPath, "InstallationDateTime",
                                                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup)+  "Restart.lnk");
                }
                catch
                {

                }
            }
        }
    }
}
