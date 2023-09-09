using Jvedio.Core.Config.Base;
using System.Collections.Generic;

namespace Jvedio.Core.WindowConfig
{
    public class Settings : AbstractConfig
    {
        public const int DEFAULT_BACKUP_PERIOD_INDEX = 1;



        private Settings() : base($"WindowConfig.Settings")
        {
            PicPathMode = 1; // 相对路径
            AutoGenScreenShot = true;
            CloseToTaskBar = true;

            TeenMode = true;
            AutoAddPrefix = true;
            Prefix = string.Empty;
            AutoBackupPeriodIndex = DEFAULT_BACKUP_PERIOD_INDEX;
            AutoBackup = true;
            DetailShowBg = true;
            CurrentLanguage = "zh-CN";
            DownloadWhenTitleNull = true;
            IgnoreCertVal = true;


            HotKeyEnable = false;
            HotKeyModifiers = 0;
            HotKeyVK = 0;

            Debug = false;
            ImageCache = true;
            CacheExpiration = Jvedio.Core.Media.ImageCache.DEFAULT_CACHE_EXPIRATION;

            DelInfoAfterDelFile = true;
        }

        public static List<int> BackUpPeriods = new List<int> { 1, 3, 7, 15, 30 };

        private static Settings _instance = null;

        public static Settings CreateInstance()
        {
            if (_instance == null)
                _instance = new Settings();

            return _instance;
        }

        public long CrawlerSelectedIndex { get; set; }

        public long PicPathMode { get; set; }

        public string PicPathJson { get; set; }

        public Dictionary<string, object> PicPaths;

        public string PluginEnabledJson { get; set; }

        public Dictionary<string, bool> PluginEnabled;


        public bool SkipExistImage { get; set; }
        public bool DownloadWhenTitleNull { get; set; }

        public bool IgnoreCertVal { get; set; }

        public bool AutoHandleHeader { get; set; }

        public long TabControlSelectedIndex { get; set; }

        public bool OpenDataBaseDefault { get; set; }

        private bool _CloseToTaskBar = true;
        public bool CloseToTaskBar {
            get { return _CloseToTaskBar; }
            set {
                _CloseToTaskBar = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// 无封面时将截图作为封面
        /// </summary>
        public bool AutoGenScreenShot { get; set; }

        public long DefaultDBID { get; set; }


        public string CurrentLanguage { get; set; }

        public bool SaveInfoToNFO { get; set; }

        public bool OverwriteNFO { get; set; }

        public string NFOSavePath { get; set; }



        private bool _TeenMode = true;
        public bool TeenMode {
            get { return _TeenMode; }
            set {
                _TeenMode = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoAddPrefix { get; set; }

        public string Prefix { get; set; }

        public bool AutoBackup { get; set; }

        public long AutoBackupPeriodIndex { get; set; }

        // 是否建立可播放索引
        public bool PlayableIndexCreated { get; set; }
        public string VideoPlayerPath { get; set; }
        public bool DelInfoAfterDelFile { get; set; }

        public bool PictureIndexCreated { get; set; }


        private bool _DetailShowBg;
        public bool DetailShowBg {
            get { return _DetailShowBg; }
            set {
                _DetailShowBg = value;
                RaisePropertyChanged();
            }
        }

        // 端口
        public bool ListenEnabled { get; set; }

        public string ListenPort { get; set; }
        public long RemoteIndex { get; set; }


        public bool _HotKeyEnable;
        public bool HotKeyEnable {
            get { return _HotKeyEnable; }
            set {
                _HotKeyEnable = value;
                RaisePropertyChanged();
            }
        }

        public long _HotKeyModifiers;
        public long HotKeyModifiers {
            get { return _HotKeyModifiers; }
            set {
                _HotKeyModifiers = value;
                RaisePropertyChanged();
            }
        }
        public long _HotKeyVK;
        public long HotKeyVK {
            get { return _HotKeyVK; }
            set {
                _HotKeyVK = value;
                RaisePropertyChanged();
            }
        }
        public string _HotKeyString;
        public string HotKeyString {
            get { return _HotKeyString; }
            set {
                _HotKeyString = value;
                RaisePropertyChanged();
            }
        }
        public bool _Debug;
        public bool Debug {
            get { return _Debug; }
            set {
                _Debug = value;
                RaisePropertyChanged();
            }
        }
        public bool _ImageCache;
        public bool ImageCache {
            get { return _ImageCache; }
            set {
                _ImageCache = value;
                RaisePropertyChanged();
            }
        }
        public long _CacheExpiration;
        public long CacheExpiration {
            get { return _CacheExpiration; }
            set {
                _CacheExpiration = value;
                RaisePropertyChanged();
            }
        }
    }
}
