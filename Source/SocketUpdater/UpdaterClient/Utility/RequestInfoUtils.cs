using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdaterShare.Model;

namespace UpdaterClient.Utility
{
    public static class RequestInfoUtils
    {      
        /// <summary>
        /// Get Download File Info 
        /// </summary>
        /// <param name="basicInfo"></param>
        /// <param name="serverAddress"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <param name="serverResult"></param>
        /// <returns></returns>
        public static bool RequestDownloadFileInfo(ClientBasicInfo basicInfo,
                                                   string serverAddress, 
                                                   string controllerName,
                                                   string actionName,
                                                   ref DownloadFileInfo serverResult)
        {
            var packageInfo = JsonConvert.SerializeObject(basicInfo);

            try
            {
                HttpClient httpClient = new HttpClient
                {
                    BaseAddress = new Uri(serverAddress),
                    Timeout = TimeSpan.FromMinutes(20)
                };

                if (ConnectionTest(serverAddress))
                {
                    StringContent strData = new StringContent(packageInfo, Encoding.UTF8, "application/json");
                    string postUrl = httpClient.BaseAddress + $"api/{controllerName}/{actionName}";
                    Uri address = new Uri(postUrl);
                    Task<HttpResponseMessage> task = httpClient.PostAsync(address, strData);
                    try
                    {
                        task.Wait();
                    }
                    catch
                    {
                        return false;
                    }
                    HttpResponseMessage response = task.Result;
                    if (!response.IsSuccessStatusCode)
                        return false;

                    try
                    {
                        string jsonResult = response.Content.ReadAsStringAsync().Result;
                        serverResult = JsonConvert.DeserializeObject<DownloadFileInfo>(jsonResult);
                        if (serverResult != null)
                        {
                            return true;
                        }                     
                    }
                    catch(Exception ex)
                    {
                        return false;
                    }                
                }
            }
            catch
            {
                return false;
            }
            return false;
        }


        /// <summary>
        /// Connection Test
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <returns></returns>
        private static bool ConnectionTest(string serverAddress)
        {
            if (string.IsNullOrEmpty(serverAddress)) return false;
            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri(serverAddress),
                Timeout = TimeSpan.FromSeconds(30)
            };

            Uri address = new Uri(httpClient.BaseAddress + "api/Connection/ConnectionTest");
            Task<HttpResponseMessage> task = httpClient.GetAsync(address);
            try
            {          
                task.Wait();
            }
            catch
            {
                return false;
            }


            HttpResponseMessage response = task.Result;
            if (!response.IsSuccessStatusCode)
                return false;

            string connectionResult;
            try
            {
                var result = response.Content.ReadAsStringAsync().Result;
                connectionResult = JsonConvert.DeserializeObject<string>(result);
            }
            catch
            {
                return false;
            }
            return connectionResult.Equals("connected_success");
        }
    }
}
