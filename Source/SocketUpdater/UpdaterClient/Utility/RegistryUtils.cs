using System;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace UpdaterClient.Utility
{
    public static class RegistryUtils
    {
        /// <summary>
        /// Read Registry Info
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="registryInfoPath"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string ReadRegistryInfo(RegistryKey registryKey, string registryInfoPath, string keyName)
        {
            if (registryKey == null ||string.IsNullOrEmpty(registryInfoPath) || string.IsNullOrEmpty(keyName)) return null;
            try
            {
                RegistryKey rsg = registryKey.OpenSubKey(registryInfoPath, false);
                if (rsg != null)
                {
                    var result = rsg?.GetValue(keyName).ToString();
                    rsg.Close();
                    return result;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Write Registry Info
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="registryInfoPath"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteRegistryInfo(RegistryKey registryKey, string registryInfoPath, string keyName, string value)
        {
            if (registryKey == null || string.IsNullOrEmpty(registryInfoPath) || string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(value)) return false;
            try
            {
                RegistryKey rsg = registryKey.OpenSubKey(registryInfoPath, 
                                                         RegistryKeyPermissionCheck.ReadWriteSubTree, 
                                                         RegistryRights.FullControl);
                if (rsg != null)
                {
                    rsg.SetValue(keyName, value);
                    rsg.Close();
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
        /// Create Registry Info
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="registryInfoPath"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CreateRegistryInfo(RegistryKey registryKey, string registryInfoPath, string keyName, string value)
        {
            if (registryKey == null || string.IsNullOrEmpty(registryInfoPath) || string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(value)) return false;
            try
            {
                RegistryKey rsg = registryKey.CreateSubKey(registryInfoPath);
                if (rsg != null)
                {
                    WriteRegistryInfo(registryKey, registryInfoPath, keyName, value);
                    rsg.Close();
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
        /// Delete Registry Info
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="registryInfoPath"></param>
        /// <returns></returns>
        public static bool DeleteRegistryInfo(RegistryKey registryKey, string registryInfoPath)
        {
            if (registryKey == null || string.IsNullOrEmpty(registryInfoPath)) return false;
            try
            {
                registryKey.DeleteSubKeyTree(registryInfoPath, true);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
