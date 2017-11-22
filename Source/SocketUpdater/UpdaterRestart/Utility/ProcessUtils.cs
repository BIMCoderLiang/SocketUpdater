using System.Diagnostics;

namespace UpdaterRestart.Utility
{
    public static class ProcessUtils
    {
        /// <summary>
        /// Kill Process by Process Name
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="processPath"></param>
        /// <returns></returns>
        public static bool KillProcess(string processName, ref string processPath)
        {
            if (string.IsNullOrEmpty(processName)) return false;
            try
            {
                Process[] ps = Process.GetProcessesByName("Revit");
                foreach (Process p in ps)
                {
                    processPath = p.MainModule.FileName;
                    p.Kill();
                }
                return true;
            }
            catch
            {
                return false;
            }           
        }


        /// <summary>
        /// Start Process by its Path
        /// </summary>
        /// <param name="processPath"></param>
        public static void StartProcess(string processPath)
        {
            try
            {
                Process.Start(processPath);
            }
            catch
            {

            }           
        }
    }
}
