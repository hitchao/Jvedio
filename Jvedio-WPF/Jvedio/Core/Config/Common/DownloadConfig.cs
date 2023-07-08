using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class DownloadConfig : AbstractConfig
    {

        private DownloadConfig() : base("DownloadConfig")
        {
            DownloadInfo = true;
            DownloadThumbNail = true;
            DownloadPoster = true;
            DownloadPreviewImage = false;
            DownloadActor = true;
        }

        private static DownloadConfig _instance = null;

        public static DownloadConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new DownloadConfig();

            return _instance;
        }

        public bool DownloadInfo { get; set; }
        public bool DownloadThumbNail { get; set; }
        public bool DownloadPoster { get; set; }
        public bool DownloadPreviewImage { get; set; }
        public bool DownloadActor { get; set; }

    }
}
