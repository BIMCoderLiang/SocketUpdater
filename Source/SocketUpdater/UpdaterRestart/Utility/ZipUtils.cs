using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace UpdaterRestart.Utility
{
    public static class ZipUtils
    {
        #region Zip
        /// <summary>
        /// Zip File
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static bool CreateZip(string sourceFilePath, string destinationZipFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationZipFilePath)) return false;
            try
            {
                if (sourceFilePath[sourceFilePath.Length - 1] != Path.DirectorySeparatorChar)
                    sourceFilePath += Path.DirectorySeparatorChar;
                ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
                zipStream.SetLevel(6);  // 压缩级别 0-9
                var isCreateSuccess = CreateZipFiles(sourceFilePath, zipStream);
                if (isCreateSuccess)
                {
                    zipStream.Finish();
                    zipStream.Close();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Create Zip File
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="zipStream"></param>
        private static bool CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream)
        {
            try
            {
                Crc32 crc = new Crc32();
                string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
                foreach (string file in filesArray)
                {
                    if (Directory.Exists(file))
                    {
                        CreateZipFiles(file, zipStream);
                    }
                    else
                    {
                        FileStream fileStream = File.OpenRead(file);
                        byte[] buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                        ZipEntry entry = new ZipEntry(tempFile)
                        {
                            DateTime = DateTime.Now,
                            Size = fileStream.Length
                        };
                        fileStream.Close();
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        zipStream.PutNextEntry(entry);
                        zipStream.Write(buffer, 0, buffer.Length);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion



        #region Unzip
        public static bool UnZip(Stream stream, string targetPath)
        {
            try
            {
                using (ZipInputStream zipInStream = new ZipInputStream(stream))
                {
                    ZipEntry entry;
                    while ((entry = zipInStream.GetNextEntry()) != null)
                    {
                        string directorName = Path.Combine(targetPath, Path.GetDirectoryName(entry.Name));
                        string fileName = Path.Combine(directorName, Path.GetFileName(entry.Name));
                        if (directorName.Length > 0)
                        {
                            Directory.CreateDirectory(directorName);
                        }
                        if (fileName != string.Empty && !entry.IsDirectory)
                        {
                            using (FileStream streamWriter = File.Create(fileName))
                            {
                                byte[] data = new byte[4 * 1024];
                                while (true)
                                {
                                    var size = zipInStream.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else break;
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
