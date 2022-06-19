using Jvedio.Core.Config.Base;
using Jvedio.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Settings : AbstractConfig
    {
        private Settings() : base($"WindowConfig.Settings")
        {
            PicPathMode = 1;// 相对路径
            AutoGenScreenShot = true;
            CloseToTaskBar = true;

            TeenMode = true;
            AutoAddPrefix = true;
            Prefix = "";
            AutoBackupPeriodIndex = 0;
            AutoBackup = true;
        }

        public static List<int> BackUpPeriods = new List<int> { 1, 3, 7, 15, 30 };

        private static Settings _instance = null;

        public static Settings createInstance()
        {
            if (_instance == null) _instance = new Settings();

            return _instance;
        }
        public long CrawlerSelectedIndex { get; set; }
        public long PicPathMode { get; set; }
        public string PicPathJson { get; set; }

        public Dictionary<string, object> PicPaths;
        public string PluginEnabledJson { get; set; }
        public Dictionary<string, bool> PluginEnabled;
        public bool DownloadPreviewImage { get; set; }
        public bool OverrideInfo { get; set; }
        public bool IgnoreCertVal { get; set; }
        public bool AutoHandleHeader { get; set; }
        public long TabControlSelectedIndex { get; set; }
        public bool OpenDataBaseDefault { get; set; }
        public bool CloseToTaskBar { get; set; }
        public bool AutoGenScreenShot { get; set; }
        public long DefaultDBID { get; set; }

        /// <summary>
        /// 0-中文 1-English 2-日語
        /// </summary>
        public long SelectedLanguage { get; set; }
        public bool SaveInfoToNFO { get; set; }
        public bool OverriteNFO { get; set; }
        public string NFOSavePath { get; set; }

        public bool TeenMode { get; set; }

        public bool AutoAddPrefix { get; set; }

        public string Prefix { get; set; }


        public bool AutoBackup { get; set; }

        public long AutoBackupPeriodIndex { get; set; }



        // 是否建立可播放索引
        public bool PlayableIndexCreated { get; set; }
        public bool PictureIndexCreated { get; set; }
        public bool AutoCreatePlayableIndex { get; set; }

    }
}
