using System;
using System.IO;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace UpdaterClient.Utility
{
    public static class BootUtils
    {
        /// <summary>
        /// Create lnk for Restart.vbs in Startup Folder
        /// </summary>
        /// <param name="lnkNameWithoutExt"></param>
        /// <param name="lnkTempDirPath">Some dirs can't create lnk!</param>
        /// <param name="vbScriptPath">path must with extenstion!</param>
        /// <returns></returns>
        public static bool CreateLnk(string lnkNameWithoutExt, string lnkTempDirPath, string vbScriptPath)
        {
            if (string.IsNullOrEmpty(lnkNameWithoutExt) || string.IsNullOrEmpty(lnkTempDirPath) ||
                string.IsNullOrEmpty(vbScriptPath)) return false;
            if (!Directory.Exists(lnkTempDirPath))
            {
                Directory.CreateDirectory(lnkTempDirPath);
            }
            try
            {
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + $"\\{lnkNameWithoutExt}.lnk"))
                {
                    var shell = new WshShell();
                    var shortcut = (IWshShortcut)shell.CreateShortcut(Path.Combine(lnkTempDirPath, $"{lnkNameWithoutExt}.lnk"));
                    shortcut.WorkingDirectory = Path.GetDirectoryName(vbScriptPath);
                    shortcut.TargetPath = vbScriptPath;
                    shortcut.WindowStyle = 7;
                    shortcut.Save();
                    var sourcePath = Path.Combine(lnkTempDirPath, $"{lnkNameWithoutExt}.lnk");
                    var destPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) +$"\\{lnkNameWithoutExt}.lnk";
                    File.Copy(sourcePath, destPath);
                    return File.Exists(destPath);
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
