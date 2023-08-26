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
            OverrideInfo = false;
        }

        private static DownloadConfig _instance = null;

        public static DownloadConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new DownloadConfig();

            return _instance;
        }

        private bool _DownloadInfo;
        public bool DownloadInfo {
            get { return _DownloadInfo; }
            set {
                _DownloadInfo = value;
                RaisePropertyChanged();
            }
        }
        private bool _DownloadThumbNail;
        public bool DownloadThumbNail {
            get { return _DownloadThumbNail; }
            set {
                _DownloadThumbNail = value;
                RaisePropertyChanged();
            }
        }
        private bool _DownloadPoster;
        public bool DownloadPoster {
            get { return _DownloadPoster; }
            set {
                _DownloadPoster = value;
                RaisePropertyChanged();
            }
        }
        private bool _DownloadPreviewImage;
        public bool DownloadPreviewImage {
            get { return _DownloadPreviewImage; }
            set {
                _DownloadPreviewImage = value;
                RaisePropertyChanged();
            }
        }
        private bool _DownloadActor;
        public bool DownloadActor {
            get { return _DownloadActor; }
            set {
                _DownloadActor = value;
                RaisePropertyChanged();
            }
        }
        private bool _OverrideInfo;
        public bool OverrideInfo {
            get { return _OverrideInfo; }
            set {
                _OverrideInfo = value;
                RaisePropertyChanged();
            }
        }

    }
}
