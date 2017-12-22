using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;

namespace UpdaterShare.Utility
{
    public static class ServerFileUtils
    {
        /// <summary>
        /// Get Latest File Path
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="revitVersion"></param>
        /// <param name="currentProductVersion"></param>
        /// <returns></returns>
        public static string GetLatestFilePath(string productName, string revitVersion, string currentProductVersion)
        {
            try
            {
                var mainFolderPath = Path.Combine(GetFilePathFromService("SocketService"),"ExampleFile");
                var updateFilesList = Directory.GetFiles(mainFolderPath);
                if (updateFilesList.Any())
                {
                    foreach (var file in updateFilesList)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var paras = fileName.Split('_');
                        if (paras[0] == productName && paras[1] == revitVersion && paras[2] == currentProductVersion)
                        {
                            return Path.GetFullPath(file);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        //ProductName_RevitVersion_OldProductVersion_NewProductVersion
        public static string GetLatestVersion(string fileName)
        {
            return fileName.Split('_').LastOrDefault();
        }


        /// <summary>
        /// Get Latest File From Windows Service by Registry
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetFilePathFromService(string serviceName)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                var socketService = services.FirstOrDefault(x => String.Equals(x.ServiceName, "SocketService"));
                if (socketService != null)
                {
                    var localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ?
                                                                       RegistryView.Registry64 : RegistryView.Registry32);
                    var key = localMachineRegistry.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);
                    if (key != null)
                    {
                        var serviceExePath = GetString(key.GetValue("ImagePath").ToString());
                        var folderPath = Path.GetDirectoryName(serviceExePath);
                        if (!String.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                        {
                            return folderPath;
                        }                    
                    }
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Remove "\"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string GetString(string str)
        {
            if (str.Contains("\""))
            {
                str = str.Substring(1, str.Length - 2);
            }
            return str;
        }
    }
}
