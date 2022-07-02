using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class ScanResult
    {


        public ScanResult()
        {
            Update = new Dictionary<string, string>();
            Import = new List<string>();
            NotImport = new Dictionary<string, string>();
            FailNFO = new List<string>();
            Logs = new List<string>();
            ScanDate = DateHelper.Now();
        }


        public Dictionary<string, string> Update { get; set; }
        public List<string> Logs { get; set; }
        public List<string> Import { get; set; }

        /// <summary>
        /// （路径，原因）
        /// </summary>
        public Dictionary<string, string> NotImport { get; set; }
        public List<string> FailNFO { get; set; }

        public string ScanDate { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long TotalCount { get; set; }


    }
}
