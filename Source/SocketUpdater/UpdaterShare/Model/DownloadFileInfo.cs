using System;

namespace UpdaterShare.Model
{
    [Serializable]
    public class DownloadFileInfo
    {
        public string LatestProductVersion { get; set; }
        public string DownloadFileMd5 { get; set; }
        public long DownloadFileTotalSize { get; set; }
    }
}
