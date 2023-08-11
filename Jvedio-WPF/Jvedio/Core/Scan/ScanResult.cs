using Jvedio.Entity;
using SuperUtils.Time;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public class ScanResult
    {
        #region "属性"

        public Dictionary<string, string> Update { get; set; }

        public List<string> Logs { get; set; }

        public List<string> Import { get; set; }
        public List<Video> InsertVideos { get; set; }

        /// <summary>
        /// （路径，原因）
        /// </summary>
        public Dictionary<string, ScanDetailInfo> NotImport { get; set; }

        public List<string> FailNFO { get; set; }

        public string ScanDate { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public long TotalCount { get; set; }

        #endregion

        public ScanResult()
        {
            ScanDate = DateHelper.Now();
            Update = new Dictionary<string, string>();
            Import = new List<string>();
            NotImport = new Dictionary<string, ScanDetailInfo>();
            FailNFO = new List<string>();
            Logs = new List<string>();
            InsertVideos = new List<Video>();
        }
    }
}
