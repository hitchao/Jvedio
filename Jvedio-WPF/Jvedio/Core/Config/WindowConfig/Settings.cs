using Jvedio.Core.Config.Base;
using System.Collections.Generic;
using System.ComponentModel;

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
            AutoBackup = true;
            CurrentLanguage = "zh-CN";


        }

        public static List<int> BackUpPeriods = new List<int> { 1, 3, 7, 15, 30 };

        private static Settings _instance = null;

        public static Settings CreateInstance()
        {
            if (_instance == null) _instance = new Settings();

            return _instance;
        }

        public long CrawlerSelectedIndex { get; set; }

        public long PicPathMode { get; set; }

        public string PicPathJson { get; set; }

        public Dictionary<string, object> PicPaths { get; set; }

        public string PluginEnabledJson { get; set; }

        public Dictionary<string, bool> PluginEnabled;

        public bool DownloadPreviewImage { get; set; }

        public bool SkipExistImage { get; set; }

        [DefaultValue(true)]
        public bool DownloadWhenTitleNull { get; set; }

        public bool OverrideInfo { get; set; }

        [DefaultValue(true)]
        public bool IgnoreCertVal { get; set; }

        public bool AutoHandleHeader { get; set; }

        public long TabControlSelectedIndex { get; set; }

        public bool OpenDataBaseDefault { get; set; }

        private bool _CloseToTaskBar = true;
        public bool CloseToTaskBar
        {
            get { return _CloseToTaskBar; }
            set
            {
                _CloseToTaskBar = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoGenScreenShot { get; set; }

        public long DefaultDBID { get; set; }


        public string CurrentLanguage { get; set; }

        public bool SaveInfoToNFO { get; set; }

        public bool OverwriteNFO { get; set; }

        public string NFOSavePath { get; set; }

        public bool TeenMode { get; set; }

        public bool AutoAddPrefix { get; set; }

        public string Prefix { get; set; }

        public bool AutoBackup { get; set; }

        [DefaultValue(DEFAULT_BACKUP_PERIOD_INDEX)]
        public long AutoBackupPeriodIndex { get; set; }

        // 是否建立可播放索引
        public bool PlayableIndexCreated { get; set; }

        public bool PictureIndexCreated { get; set; }

        [DefaultValue(true)]
        public bool DetailShowBg { get; set; }

        // 端口
        public bool ListenEnabled { get; set; }

        public string ListenPort { get; set; }
        public long RemoteIndex { get; set; }
    }
}
