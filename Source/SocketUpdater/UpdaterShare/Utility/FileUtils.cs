using System;
using System.IO;

namespace UpdaterShare.Utility
{
    public static class FileUtils
    {
        /// <summary>
        /// Combine Temp Files
        /// </summary>
        /// <param name="localSavePath"></param>
        /// <param name="tempFilesDirectory">temp files Directory</param>
        /// <returns></returns>
        public static void CombineTempFiles(string localSavePath, string tempFilesDirectory)
        {
            if (!string.IsNullOrEmpty(localSavePath) && !string.IsNullOrEmpty(tempFilesDirectory))
            {
                try
                {
                    var tempFilesDir = new DirectoryInfo(tempFilesDirectory);
                    using (FileStream writestream = new FileStream(localSavePath, FileMode.Create, FileAccess.Write,FileShare.Write))
                    {
                        foreach (FileInfo tempfile in tempFilesDir.GetFiles())
                        {
                            using (FileStream readTempStream = new FileStream(tempfile.FullName, FileMode.Open,FileAccess.Read, FileShare.ReadWrite))
                            {
                                long onefileLength = tempfile.Length;
                                byte[] buffer = new byte[Convert.ToInt32(onefileLength)];
                                readTempStream.Read(buffer, 0, Convert.ToInt32(onefileLength));
                                writestream.Write(buffer, 0, Convert.ToInt32(onefileLength));
                            }
                        }
                        writestream.Flush();
                        writestream.Close();
                        writestream.Dispose();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        /// <summary>
        /// Get File by Start Index and Length
        /// </summary>
        /// <param name="serverFilePath"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetFile(string serverFilePath, int start, int length)
        {
            if (!string.IsNullOrEmpty(serverFilePath) && length != 0)
            {
                try
                {
                    using (FileStream serverStream = new FileStream(serverFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 80, true))
                    {
                        byte[] buffer = new byte[length];
                        serverStream.Position = start;
                        serverStream.Read(buffer, 0, length);
                        return buffer;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }


        /// <summary>
        /// Generate Temp File
        /// </summary>
        /// <param name="tempFilePath"></param>
        /// <param name="importBytes"></param>
        public static void GenerateTempFile(string tempFilePath, byte[] importBytes)
        {
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                try
                {
                    using (FileStream tempstream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        tempstream.Write(importBytes, 0, importBytes.Length);
                        tempstream.Flush();
                        tempstream.Close();
                        tempstream.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
