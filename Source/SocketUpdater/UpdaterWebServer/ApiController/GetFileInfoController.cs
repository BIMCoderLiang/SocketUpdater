using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using UpdaterShare.Model;
using UpdaterShare.Utility;

namespace UpdaterWebServer.ApiController
{
    
    public class GetFileInfoController : System.Web.Http.ApiController
    {
        [HttpPost]
        public HttpResponseMessage GetInfo(ClientBasicInfo clientBasicInfo)
        {
            var productName = clientBasicInfo.ProductName;
            var revitVersion = clientBasicInfo.RevitVersion;
            var currentProductVersion = clientBasicInfo.CurrentProductVersion;

            //ProductName_RevitVersion_OldProductVersion_NewProductVersion
            var latestFilePath = ServerFileUtils.GetLatestFilePath(productName, revitVersion, currentProductVersion);
            if (string.IsNullOrEmpty(latestFilePath) || !File.Exists(latestFilePath)) return null;

            var latestFile = new FileInfo(latestFilePath);
            var downloadInfo = new DownloadFileInfo()
            {
                LatestProductVersion = ServerFileUtils.GetLatestVersion(Path.GetFileNameWithoutExtension(latestFilePath)),
                DownloadFileMd5 = Md5Utils.GetFileMd5(latestFilePath),
                DownloadFileTotalSize = latestFile.Length
            };
            HttpResponseMessage response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(downloadInfo), Encoding.GetEncoding("UTF-8"),"application/json")
            };
            return response;
        }
    }
}
