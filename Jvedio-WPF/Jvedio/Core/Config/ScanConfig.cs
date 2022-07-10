using Jvedio.Core.Config.Base;
using Jvedio.Core.WindowConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class ScanConfig : AbstractConfig
    {

        private const double DEFAULT_MINFILESIZE = 1;       // 1 MB
        private ScanConfig() : base("ScanConfig")
        {
            CopyNFOPicture = true;
            FetchVID = true;
        }

        private static ScanConfig _instance = null;

        public static ScanConfig createInstance()
        {
            if (_instance == null) _instance = new ScanConfig();

            return _instance;
        }



        public double _MinFileSize = DEFAULT_MINFILESIZE;
        public double MinFileSize
        {
            get
            {
                return _MinFileSize;
            }
            set
            {
                if (value < 0)
                    _MinFileSize = DEFAULT_MINFILESIZE;
                else
                    _MinFileSize = value;
            }
        }
        public bool ScanOnStartUp { get; set; }
        public bool CopyNFOPicture { get; set; }
        public bool FetchVID { get; set; }


    }
}
