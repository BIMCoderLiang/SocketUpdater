using System;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace UpdaterShare.Utility
{
    public static class LogUtils
    {
        //logFileName = $"{productName}{productVersion}-{revitVersion}";
        public static void Init(string productName, string logFileName, string logFolderPath)
        {
            if (string.IsNullOrEmpty(logFileName))
            {
                logFileName = productName;
            }

            // Begin Config
            string finalLogFolderPath = null;
            if (ValidOrCreateLogFolderPath(logFolderPath))
            {
                finalLogFolderPath = logFolderPath;
            }
            else
            {
                string userLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string defaultLogFolderPath = Path.Combine(userLocalPath, $"{productName}\\Logs\\");
                if (ValidOrCreateLogFolderPath(defaultLogFolderPath))
                {
                    finalLogFolderPath = defaultLogFolderPath;
                }
            }

            // Creat Pattern layout for outputed string in log file
            PatternLayout patternLayout = new PatternLayout { ConversionPattern = "%d [%t] %-5p %m%n" };
            patternLayout.ActivateOptions();

            // Create rolling file appender
            RollingFileAppender roller = new RollingFileAppender { AppendToFile = false };
            string nowDateString = DateTime.Now.ToString("yyyy-MM-dd");
            string finalLogFileName = $@"{logFileName}-{nowDateString}.log";
            if (finalLogFolderPath != null) roller.File = Path.Combine(finalLogFolderPath, finalLogFileName);
            roller.Layout = patternLayout;
            roller.RollingStyle = RollingFileAppender.RollingMode.Once;
            roller.MaxSizeRollBackups = 1000;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();

#if DEBUG
            ConsoleAppender consoleAppender = new ConsoleAppender {Layout = new SimpleLayout()};
            consoleAppender.ActivateOptions();
#endif

            // Get Hierarchy
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            Logger logger = hierarchy.GetLogger(logFileName) as Logger;

            hierarchy.Configured = true;
            logger.Additivity = false;
            logger.Level = log4net.Core.Level.All;

            // Must clear previous appenders, otherwise may output previous log folder !!!
            logger.RemoveAllAppenders();
            logger.AddAppender(roller);
        }

        private static bool ValidOrCreateLogFolderPath(string logFolderPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(logFolderPath))
                {
                    if (!Directory.Exists(logFolderPath))
                    {
                        Directory.CreateDirectory(logFolderPath);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
