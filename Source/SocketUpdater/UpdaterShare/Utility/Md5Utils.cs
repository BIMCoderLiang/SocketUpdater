using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UpdaterShare.Utility
{
    public static class Md5Utils
    {
        /// <summary>
        /// Get Md5 value of certain flie
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public static string GetFileMd5(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            try
            {
                using (FileStream filestream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] file = md5.ComputeHash(filestream);
                    filestream.Close();
                    StringBuilder fileMd5 = new StringBuilder();
                    for (int i = 0; i < file.Length; i++)
                    {
                        fileMd5.Append(i.ToString("x2"));
                    }
                    return fileMd5.ToString();
                }
            }
            catch
            {
                return null;
            }          
        }


        /// <summary>
        /// Whether two md5 values are the same value
        /// </summary>
        /// <param name="md5One">first Md5 value</param>
        /// <param name="md5Two">second Md5 value</param>
        /// <returns></returns>
        public static bool IsMd5Equal(string md5One, string md5Two)
        {
            return string.Compare(md5One, md5Two, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
