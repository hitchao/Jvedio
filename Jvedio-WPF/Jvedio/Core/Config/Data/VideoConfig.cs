using Jvedio.Core.Config.Base;
using Jvedio.Core.Crawler;
using Jvedio.Core.Global;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Entity;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Jvedio.Core.Config.Data
{
    public class VideoConfig : AbstractConfig
    {
        private VideoConfig() : base("VideoConfig")
        {
            ImageMode = 1;
            GlobalImageWidth = 300;
            SmallImage_Width = 180;
            SmallImage_Height = 150;
            BigImage_Height = 200;
            BigImage_Width = 250;
            GifImage_Width = 250;
            GifImage_Height = 200;
            SortType = 0;
            SortDescending = false;
            OnlyShowSubSection = false;
            PageSize = 20;
        }

        private long _PageSize;
        public long PageSize {
            get { return _PageSize; }
            set {
                _PageSize = value;
                RaisePropertyChanged();
            }
        }

        private bool _OnlyShowSubSection;
        public bool OnlyShowSubSection {
            get { return _OnlyShowSubSection; }
            set {
                _OnlyShowSubSection = value;
                RaisePropertyChanged();
            }
        }
        private bool _SortDescending;
        public bool SortDescending {
            get { return _SortDescending; }
            set {
                _SortDescending = value;
                RaisePropertyChanged();
            }
        }


        private long _SortType;
        public long SortType {
            get { return _SortType; }
            set {
                _SortType = value;
                RaisePropertyChanged();
            }
        }

        private long _ImageMode;
        public long ImageMode {
            get { return _ImageMode; }
            set {
                _ImageMode = value;
                RaisePropertyChanged();
            }
        }

        private long _GlobalImageWidth;
        public long GlobalImageWidth {
            get { return _GlobalImageWidth; }
            set {
                _GlobalImageWidth = value;
                RaisePropertyChanged();
            }
        }
        private long _SmallImage_Width;
        public long SmallImage_Width {
            get { return _SmallImage_Width; }
            set {
                _SmallImage_Width = value;
                RaisePropertyChanged();
            }
        }

        private long _SmallImage_Height;
        public long SmallImage_Height {
            get { return _SmallImage_Height; }
            set {
                _SmallImage_Height = value;
                RaisePropertyChanged();
            }
        }
        private long _BigImage_Height;
        public long BigImage_Height {
            get { return _BigImage_Height; }
            set {
                _BigImage_Height = value;
                RaisePropertyChanged();
            }
        }
        private long _BigImage_Width;
        public long BigImage_Width {
            get { return _BigImage_Width; }
            set {
                _BigImage_Width = value;
                RaisePropertyChanged();
            }
        }
        private long _GifImage_Height;
        public long GifImage_Height {
            get { return _GifImage_Height; }
            set {
                _GifImage_Height = value;
                RaisePropertyChanged();
            }
        }
        private long _GifImage_Width;
        public long GifImage_Width {
            get { return _GifImage_Width; }
            set {
                _GifImage_Width = value;
                RaisePropertyChanged();
            }
        }


        private static VideoConfig _instance = null;

        public static VideoConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new VideoConfig();

            return _instance;
        }

    }
}
