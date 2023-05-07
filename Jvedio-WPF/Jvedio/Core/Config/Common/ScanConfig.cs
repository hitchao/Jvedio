using Jvedio.Core.Config.Base;
using System.ComponentModel;

namespace Jvedio.Core.Config
{
    public class ScanConfig : AbstractConfig
    {
        public const double DEFAULT_MIN_FILE_SIZE = 1;       // 1 MB

        private ScanConfig() : base("ScanConfig")
        {
        }

        private static ScanConfig _instance = null;

        public static ScanConfig CreateInstance()
        {
            if (_instance == null) _instance = new ScanConfig();

            return _instance;
        }

        public double _MinFileSize = DEFAULT_MIN_FILE_SIZE;

        public double MinFileSize
        {
            get
            {
                return _MinFileSize;
            }

            set
            {
                if (value < 0)
                    _MinFileSize = DEFAULT_MIN_FILE_SIZE;
                else
                    _MinFileSize = value;
            }
        }

        public bool ScanOnStartUp { get; set; }

        public bool CopyNFOOverwriteImage { get; set; }

        [DefaultValue(true)]
        public bool CopyNFOPicture { get; set; }

        [DefaultValue(true)]
        public bool CopyNFOActorPicture { get; set; }

        [DefaultValue(true)]
        public bool CopyNFOPreview { get; set; }

        [DefaultValue(true)]
        public bool CopyNFOScreenShot { get; set; }

        [DefaultValue(".actor")]
        public string CopyNFOActorPath { get; set; }


        [DefaultValue(".screenshot")]
        public string CopyNFOScreenShotPath { get; set; }

        [DefaultValue(".preview")]
        public string CopyNFOPreviewPath { get; set; }

        [DefaultValue(true)]
        public bool FetchVID { get; set; }

        [DefaultValue(true)]
        public bool LoadDataAfterScan { get; set; }

        [DefaultValue(true)]
        public bool DataExistsIndexAfterScan { get; set; }

        [DefaultValue(true)]
        public bool ImageExistsIndexAfterScan { get; set; }
        public string NFOParseConfig { get; set; }
    }
}
