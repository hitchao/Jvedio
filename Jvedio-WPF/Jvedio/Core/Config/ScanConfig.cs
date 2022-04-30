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

        static double DEFAULT_MINFILESIZE = 1;
        private ScanConfig() : base("ScanConfig")
        {
        }

        private static ScanConfig _instance = null;

        public static ScanConfig createInstance()
        {
            if (_instance == null) _instance = new ScanConfig();

            return _instance;
        }



        public double _MinFileSize = 1;
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


    }
}
