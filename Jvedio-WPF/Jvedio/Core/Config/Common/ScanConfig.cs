using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class ScanConfig : AbstractConfig
    {
        private const double DEFAULT_MINFILESIZE = 1;       // 1 MB

        private ScanConfig() : base("ScanConfig")
        {
            CopyNFOPicture = true;
            CopyNFOPreview = true;
            CopyNFOActorPicture = true;
            CopyNFOScreenShot = true;

            CopyNFOActorPath = ".actor";
            CopyNFOPreviewPath = ".preview";
            CopyNFOScreenShotPath = ".screenshot";

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

        public bool CopyNFOOverriteImage { get; set; }
        public bool CopyNFOPicture { get; set; }
        public bool CopyNFOActorPicture { get; set; }
        public bool CopyNFOPreview { get; set; }
        public bool CopyNFOScreenShot { get; set; }
        public string CopyNFOActorPath { get; set; }
        public string CopyNFOScreenShotPath { get; set; }
        public string CopyNFOPreviewPath { get; set; }

        public bool FetchVID { get; set; }
    }
}
