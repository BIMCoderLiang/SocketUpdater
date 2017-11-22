using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace UpdaterRestart.Utility
{
    public static class ReplaceFilesUtils
    {
        /// <summary>
        /// Replace Old Files With New Ones
        /// See ...\ExampleFile\config.xml
        /// </summary>
        /// <param name="installLocation"></param>
        /// <param name="unzipFolder"></param>
        /// <returns></returns>
        public static bool CoverFiles(string installLocation, string unzipFolder)
        {
            if (string.IsNullOrEmpty(installLocation) || string.IsNullOrEmpty(unzipFolder)) return false;
            if (!Directory.Exists(unzipFolder)) return false;
            try
            {
                string[] xmlFiles = Directory.GetFiles(unzipFolder, "*.xml");
                var configFile = xmlFiles.FirstOrDefault(x => x.Contains("config.xml"));
                if (configFile != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(configFile);
                    XmlNode xmlRootNode = xmlDoc.SelectSingleNode("UpdateLocation");
                    if (xmlRootNode != null)
                    {
                        XmlNodeList xnl = xmlRootNode.ChildNodes;
                        if (xnl.Count > 0)
                        {
                            foreach (XmlNode xn in xnl)
                            {
                                XmlElement xe = (XmlElement) xn;
                                string dirName = xe.GetAttribute("FilePath");
                                string fileName = xn.InnerText;

                                string mainDir = dirName.Substring(0, dirName.IndexOf('\\'));
                                string restDir = dirName.Substring(dirName.IndexOf('\\') + 1);
                                mainDir = GetFolderPath(mainDir, installLocation);

                                string updateDir = Path.Combine(mainDir, restDir);
                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    string sourceFileName = Path.Combine(unzipFolder, fileName);
                                    string destFileName = Path.Combine(updateDir, fileName);
                                    if (!Directory.Exists(updateDir))
                                    {
                                        Directory.CreateDirectory(updateDir);
                                    }
                                    File.Copy(sourceFileName, destFileName, true);
                                }
                                else
                                {
                                    //if InnerText is null, delete this file or folder
                                    string deleteFileOrFolderPath = updateDir;
                                    if (File.Exists(deleteFileOrFolderPath))
                                    {
                                        File.Delete(deleteFileOrFolderPath);
                                    }
                                    else if (Directory.Exists(deleteFileOrFolderPath))
                                    {
                                        Directory.Delete(deleteFileOrFolderPath,true);
                                    }
                                }                              
                            }
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Get Special Folder From config.xml
        /// </summary>
        /// <param name="mainFolderName"></param>
        /// <param name="installLocation"></param>
        /// <returns></returns>
        private static string GetFolderPath(string mainFolderName, string installLocation)
        {
            switch (mainFolderName)
            {
                //C:\Windows\Fonts
                case "Fonts":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

                //C:\Program Files                                
                case "ProgramFiles":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                //C:\Program Files(x86)
                case "ProgramFilesX86":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                //C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup
                case "Startup":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                // ProgramData
                case "CommonApplicationData":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                // C:\Users\<user>\AppData\Local
                case "LocalApplicationData":
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // Default Install Location
                default:
                    return installLocation;
            }
        }
    }
}
