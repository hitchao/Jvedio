using GalaSoft.MvvmLight;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Plugins;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using Jvedio.Core.Logs;
using SuperUtils.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static SuperUtils.Visual.VisualHelper;

namespace Jvedio.ViewModel
{
    public class VieModel_Settings : ViewModelBase
    {
        Window_Settings settings = GetWindowByName("Window_Settings") as Window_Settings;


        public Dictionary<string, object> PicPaths { get; set; }




        public VieModel_Settings()
        {
            PicPaths = new Dictionary<string, object>();
            SetServers();
            RenderPlugins();
            setBasePicPaths();
        }




        public void LoadScanPath(AppDatabase db)
        {
            //读取配置文件
            ScanPath = new ObservableCollection<string>();

            List<string> list = JsonUtils.TryDeserializeObject<List<string>>(db.ScanPath);
            if (list != null && list.Count > 0)
                list.ForEach(arg => ScanPath.Add(arg));

            if (ScanPath.Count == 0) ScanPath = null;
            Servers = new ObservableCollection<Server>();
        }


        public void RenderPlugins()
        {

            InstalledPlugins = new List<PluginMetaData>();
            foreach (PluginMetaData plugin in PluginManager.PluginList)
                InstalledPlugins.Add(plugin);



            RefreshCurrentPlugins();


            //if (AllFreshPlugins != null)
            //{
            //    CurrentFreshPlugins = new ObservableCollection<PluginMetaData>();
            //    foreach (var item in getSortResult(AllFreshPlugins))
            //        CurrentFreshPlugins.Add(item);
            //}


        }

        public void RefreshCurrentPlugins()
        {
            CurrentInstalledPlugins = new ObservableCollection<PluginMetaData>();
            foreach (var item in GetSortResult(InstalledPlugins))
                CurrentInstalledPlugins.Add(item);

            CurrentFreshPlugins = new ObservableCollection<PluginMetaData>();
            foreach (var item in GetSortResult(AllFreshPlugins))
                CurrentFreshPlugins.Add(item);
        }


        public int SortEnabledIndex = -1;           // 0 启用 1 未启用
        public PluginType SortPluginType = PluginType.None;

        public List<PluginMetaData> GetSortResult(IEnumerable<PluginMetaData> PluginMetaDatas)
        {
            // 筛选
            IEnumerable<PluginMetaData> list = PluginMetaDatas;
            if (list == null || list.Count() == 0) return new List<PluginMetaData>();


            if (SortEnabledIndex >= 0)
            {
                bool enabled = SortEnabledIndex == 0;
                list = list.Where(arg => arg.Enabled == enabled);
            }

            if (SortPluginType != PluginType.None)
            {
                list = list.Where(arg => arg.PluginType == SortPluginType);
            }

            if (!string.IsNullOrEmpty(PluginSearch))
            {
                list = PluginMetaDatas.Where(arg => arg.PluginName.ToLower().IndexOf(PluginSearch.ToLower()) >= 0);
            }



            if (PluginSortIndex == 0)
            {
                if (PluginSortDesc)
                    list = list.OrderByDescending(arg => arg.PluginName);
                else
                    list = list.OrderBy(arg => arg.PluginName);
            }
            else if (PluginSortIndex == 1)
            {
                if (PluginSortDesc)
                    list = list.OrderByDescending(arg => arg.AuthorNames);
                else
                    list = list.OrderBy(arg => arg.AuthorNames);
            }
            else if (PluginSortIndex == 2)
            {
                if (PluginSortDesc)
                    list = list.OrderByDescending(arg => arg.ReleaseNotes.Date);
                else
                    list = list.OrderBy(arg => arg.ReleaseNotes.Date);
            }
            return list.ToList();
        }

        public void SetServers()
        {
            CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();
            foreach (PluginMetaData plugin in CrawlerManager.PluginMetaDatas)
            {
                string pluginID = plugin.PluginID;
                if (string.IsNullOrEmpty(pluginID)) continue;
                string name = plugin.PluginName;
                CrawlerServer crawlerServer = ConfigManager.ServerConfig.CrawlerServers
                    .Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
                if (crawlerServer == null)
                {
                    crawlerServer = new CrawlerServer();
                    crawlerServer.PluginID = pluginID;
                    CrawlerServers.Add(pluginID, null);
                }
                else
                {
                    ObservableCollection<CrawlerServer> crawlers = new ObservableCollection<CrawlerServer>();
                    ConfigManager.ServerConfig.CrawlerServers.Where(arg => arg.PluginID.Equals(pluginID)).
                        ToList().ForEach(t => crawlers.Add(t));
                    if (!CrawlerServers.ContainsKey(pluginID))
                        CrawlerServers.Add(pluginID, crawlers);
                }

            }
            DisplayCrawlerServers = new ObservableCollection<string>();
            foreach (string key in CrawlerServers.Keys)
            {
                int len = PluginType.Crawler.ToString().Length + 1;
                DisplayCrawlerServers.Add(key.Substring(len));
            }
        }

        public bool SaveServers(Action<string> callback = null)
        {
            List<CrawlerServer> list = new List<CrawlerServer>();
            foreach (string key in CrawlerServers.Keys)
            {
                List<CrawlerServer> crawlerServers = CrawlerServers[key]?.ToList();
                if (crawlerServers == null || crawlerServers.Count <= 0) continue;
                foreach (CrawlerServer server in crawlerServers)
                {
                    if (!server.isHeaderProper())
                    {
                        string format = "{\"UserAgent\":\"value\",...}";
                        callback?.Invoke($"【{key}】 刮削器处地址为 {server.Url} 的 Headers 不合理，格式必须为：{format}");
                        return false;
                    }
                    if (server.Headers == null) server.Headers = "";
                    list.Add(server);

                }
            }
            ConfigManager.ServerConfig.CrawlerServers = list;
            ConfigManager.ServerConfig.Save();
            return true;
        }



        public int PIC_PATH_MODE_COUNT = 3;
        public void setBasePicPaths()
        {

            PicPaths = ConfigManager.Settings.PicPaths;

            PathType type = (PathType)PicPathMode;
            BasePicPath = PicPaths[type.ToString()].ToString();

            Dictionary<string, string> dict = (Dictionary<string, string>)PicPaths[PathType.RelativeToData.ToString()];
            BigImagePath = dict["BigImagePath"];
            SmallImagePath = dict["SmallImagePath"];
            PreviewImagePath = dict["PreviewImagePath"];
            ScreenShotPath = dict["ScreenShotPath"];
            ActorImagePath = dict["ActorImagePath"];

        }


        private int _TabControlSelectedIndex = (int)ConfigManager.Settings.TabControlSelectedIndex;

        public int TabControlSelectedIndex
        {
            get { return _TabControlSelectedIndex; }
            set
            {
                _TabControlSelectedIndex = value;
                RaisePropertyChanged();
            }
        }
        private string _ViewRenameFormat;

        public string ViewRenameFormat
        {
            get { return _ViewRenameFormat; }
            set
            {
                _ViewRenameFormat = value;
                RaisePropertyChanged();
            }
        }
        private bool _AutoHandleHeader = ConfigManager.Settings.AutoHandleHeader;

        public bool AutoHandleHeader
        {
            get { return _AutoHandleHeader; }
            set
            {
                _AutoHandleHeader = value;
                RaisePropertyChanged();
            }
        }
        private bool _ShowSearchHistory = ConfigManager.Main.ShowSearchHistory;

        public bool ShowSearchHistory
        {
            get { return _ShowSearchHistory; }
            set
            {
                _ShowSearchHistory = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Theme> _ThemeList;


        [Obsolete]
        public ObservableCollection<Theme> ThemeList
        {
            get { return _ThemeList; }
            set
            {
                _ThemeList = value;
                RaisePropertyChanged();
            }
        }

        public List<PluginMetaData> InstalledPlugins { get; set; }
        public List<PluginMetaData> AllFreshPlugins { get; set; }
        private ObservableCollection<PluginMetaData> _CurrentInstalledPlugins;

        public ObservableCollection<PluginMetaData> CurrentInstalledPlugins
        {
            get { return _CurrentInstalledPlugins; }
            set
            {
                _CurrentInstalledPlugins = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<PluginMetaData> _CurrentFreshPlugins;

        public ObservableCollection<PluginMetaData> CurrentFreshPlugins
        {
            get { return _CurrentFreshPlugins; }
            set
            {
                _CurrentFreshPlugins = value;
                RaisePropertyChanged();
            }
        }
        private bool _PluginSortDesc;

        public bool PluginSortDesc
        {
            get { return _PluginSortDesc; }
            set
            {
                _PluginSortDesc = value;
                RaisePropertyChanged();
            }
        }
        private int _PluginSortIndex;

        public int PluginSortIndex
        {
            get { return _PluginSortIndex; }
            set
            {
                _PluginSortIndex = value;
                RaisePropertyChanged();
            }
        }
        private string _PluginSearch = "";

        public string PluginSearch
        {
            get { return _PluginSearch; }
            set
            {
                _PluginSearch = value;
                RaisePropertyChanged();
            }
        }




        private bool _PluginEnabled;

        public bool PluginEnabled
        {
            get { return _PluginEnabled; }
            set
            {
                _PluginEnabled = value;
                RaisePropertyChanged();
            }
        }
        private PluginMetaData _CurrentPlugin;

        public PluginMetaData CurrentPlugin
        {
            get { return _CurrentPlugin; }
            set
            {
                _CurrentPlugin = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Server> _Servers;

        public ObservableCollection<Server> Servers
        {
            get { return _Servers; }
            set
            {
                _Servers = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _ScanPath;

        public ObservableCollection<string> ScanPath
        {
            get { return _ScanPath; }
            set
            {
                _ScanPath = value;
                RaisePropertyChanged();
            }
        }




        private int _PicPathMode = (int)ConfigManager.Settings.PicPathMode;

        public int PicPathMode
        {
            get { return _PicPathMode; }
            set
            {
                _PicPathMode = value;
                RaisePropertyChanged();
            }
        }
        private string _BasePicPath = "";

        public string BasePicPath
        {
            get { return _BasePicPath; }
            set
            {
                _BasePicPath = value;
                if (value != null)
                {
                    PathType type = (PathType)PicPathMode;
                    if (type != PathType.RelativeToData)
                        PicPaths[type.ToString()] = value;
                }
                RaisePropertyChanged();
            }
        }


        private string _BigImagePath = "";

        public string BigImagePath
        {
            get { return _BigImagePath; }
            set
            {
                _BigImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _SmallImagePath = "";

        public string SmallImagePath
        {
            get { return _SmallImagePath; }
            set
            {
                _SmallImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _PreviewImagePath = "";

        public string PreviewImagePath
        {
            get { return _PreviewImagePath; }
            set
            {
                _PreviewImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _ScreenShotPath = "";

        public string ScreenShotPath
        {
            get { return _ScreenShotPath; }
            set
            {
                _ScreenShotPath = value;
                RaisePropertyChanged();
            }
        }

        private string _ActorImagePath = "";

        public string ActorImagePath
        {
            get { return _ActorImagePath; }
            set
            {
                _ActorImagePath = value;
                RaisePropertyChanged();
            }
        }




        private bool _DownloadPreviewImage = ConfigManager.Settings.DownloadPreviewImage;

        public bool DownloadPreviewImage
        {
            get { return _DownloadPreviewImage; }
            set
            {
                _DownloadPreviewImage = value;
                RaisePropertyChanged();
            }
        }


        private bool _SkipExistImage = ConfigManager.Settings.SkipExistImage;

        public bool SkipExistImage
        {
            get { return _SkipExistImage; }
            set
            {
                _SkipExistImage = value;
                RaisePropertyChanged();
            }
        }

        private bool _OverrideInfo = ConfigManager.Settings.OverrideInfo;

        public bool OverrideInfo
        {
            get { return _OverrideInfo; }
            set
            {
                _OverrideInfo = value;
                RaisePropertyChanged();
            }
        }
        private bool _IgnoreCertVal = ConfigManager.Settings.IgnoreCertVal;

        public bool IgnoreCertVal
        {
            get { return _IgnoreCertVal; }
            set
            {
                _IgnoreCertVal = value;
                RaisePropertyChanged();
            }
        }


        private Dictionary<string, ObservableCollection<CrawlerServer>> _CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();

        public Dictionary<string, ObservableCollection<CrawlerServer>> CrawlerServers
        {
            get { return _CrawlerServers; }
            set
            {
                _CrawlerServers = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> _DisplayCrawlerServers = new ObservableCollection<string>();

        public ObservableCollection<string> DisplayCrawlerServers
        {
            get { return _DisplayCrawlerServers; }
            set
            {
                _DisplayCrawlerServers = value;
                RaisePropertyChanged();
            }
        }

        private string _ProxyServer = ConfigManager.ProxyConfig.Server;

        public string ProxyServer
        {
            get { return _ProxyServer; }
            set
            {
                _ProxyServer = value;
                RaisePropertyChanged();
            }
        }
        private int _ProxyPort = (int)ConfigManager.ProxyConfig.Port;

        public int ProxyPort
        {
            get { return _ProxyPort; }
            set
            {
                _ProxyPort = value;
                RaisePropertyChanged();
            }
        }
        private string _ProxyUserName = ConfigManager.ProxyConfig.UserName;

        public string ProxyUserName
        {
            get { return _ProxyUserName; }
            set
            {
                _ProxyUserName = value;
                RaisePropertyChanged();
            }
        }
        private string _ProxyPwd = ConfigManager.ProxyConfig.Password;

        public string ProxyPwd
        {
            get
            {
                return _ProxyPwd;
            }
            set
            {
                _ProxyPwd = value;
                RaisePropertyChanged();
            }
        }


        private TaskStatus _TestProxyStatus;

        public TaskStatus TestProxyStatus
        {
            get { return _TestProxyStatus; }
            set
            {
                _TestProxyStatus = value;
                RaisePropertyChanged();
            }
        }
        private int _HttpTimeout = (int)ConfigManager.ProxyConfig.HttpTimeout;

        public int HttpTimeout
        {
            get { return _HttpTimeout; }
            set
            {
                _HttpTimeout = value;
                RaisePropertyChanged();
            }
        }



        #region "扫描"

        private bool _CopyNFOPicture = ConfigManager.ScanConfig.CopyNFOPicture;

        public bool CopyNFOPicture
        {
            get { return _CopyNFOPicture; }
            set
            {
                _CopyNFOPicture = value;
                RaisePropertyChanged();
            }
        }
        private double _MinFileSize = ConfigManager.ScanConfig.MinFileSize;

        public double MinFileSize
        {
            get { return _MinFileSize; }
            set
            {
                _MinFileSize = value;
                RaisePropertyChanged();
            }
        }
        private bool _ScanOnStartUp = ConfigManager.ScanConfig.ScanOnStartUp;

        public bool ScanOnStartUp
        {
            get { return _ScanOnStartUp; }
            set
            {
                _ScanOnStartUp = value;
                RaisePropertyChanged();
            }
        }

        private bool _FetchVID = ConfigManager.ScanConfig.FetchVID;

        public bool FetchVID
        {
            get { return _FetchVID; }
            set
            {
                _FetchVID = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "常规设置"

        private bool _AutoGenScreenShot = ConfigManager.Settings.AutoGenScreenShot;

        public bool AutoGenScreenShot
        {
            get { return _AutoGenScreenShot; }
            set
            {
                _AutoGenScreenShot = value;
                RaisePropertyChanged();
            }
        }
        private bool _OpenDataBaseDefault = ConfigManager.Settings.OpenDataBaseDefault;

        public bool OpenDataBaseDefault
        {
            get { return _OpenDataBaseDefault; }
            set
            {
                _OpenDataBaseDefault = value;
                RaisePropertyChanged();
            }
        }
        private bool _TeenMode = ConfigManager.Settings.TeenMode;

        public bool TeenMode
        {
            get { return _TeenMode; }
            set
            {
                _TeenMode = value;
                RaisePropertyChanged();
            }
        }
        private bool _CloseToTaskBar = ConfigManager.Settings.CloseToTaskBar;

        public bool CloseToTaskBar
        {
            get { return _CloseToTaskBar; }
            set
            {
                _CloseToTaskBar = value;
                RaisePropertyChanged();
            }
        }
        private bool _MainWindowVisiblie;

        public bool MainWindowVisiblie
        {
            get { return _MainWindowVisiblie; }
            set
            {
                _MainWindowVisiblie = value;
                RaisePropertyChanged();
            }
        }
        private int _SelectedLanguage = (int)ConfigManager.Settings.SelectedLanguage;

        public int SelectedLanguage
        {
            get { return _SelectedLanguage; }
            set
            {
                _SelectedLanguage = value;
                settings?.SetLanguage();
                RaisePropertyChanged();
            }
        }


        private bool _DetailShowBg = ConfigManager.Settings.DetailShowBg;

        public bool DetailShowBg
        {
            get { return _DetailShowBg; }
            set
            {
                _DetailShowBg = value;
                RaisePropertyChanged();
            }
        }


        #endregion



        #region "nfo"
        private bool _SaveInfoToNFO = ConfigManager.Settings.SaveInfoToNFO;

        public bool SaveInfoToNFO
        {
            get { return _SaveInfoToNFO; }
            set
            {
                _SaveInfoToNFO = value;
                RaisePropertyChanged();
            }
        }
        private bool _OverriteNFO = ConfigManager.Settings.OverriteNFO;

        public bool OverriteNFO
        {
            get { return _OverriteNFO; }
            set
            {
                _OverriteNFO = value;
                RaisePropertyChanged();
            }
        }
        private string _NFOSavePath = ConfigManager.Settings.NFOSavePath;

        public string NFOSavePath
        {
            get { return _NFOSavePath; }
            set
            {
                _NFOSavePath = value;
                RaisePropertyChanged();
            }
        }





        #endregion

        #region "ffmpeg"
        private string _FFMPEG_Path = ConfigManager.FFmpegConfig.Path;

        public string FFMPEG_Path
        {
            get { return _FFMPEG_Path; }
            set
            {
                _FFMPEG_Path = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShot_ThreadNum = (int)ConfigManager.FFmpegConfig.ThreadNum;

        public int ScreenShot_ThreadNum
        {
            get { return _ScreenShot_ThreadNum; }
            set
            {
                _ScreenShot_ThreadNum = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShot_TimeOut = (int)ConfigManager.FFmpegConfig.TimeOut;

        public int ScreenShot_TimeOut
        {
            get { return _ScreenShot_TimeOut; }
            set
            {
                _ScreenShot_TimeOut = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShotNum = (int)ConfigManager.FFmpegConfig.ScreenShotNum;

        public int ScreenShotNum
        {
            get { return _ScreenShotNum; }
            set
            {

                if (value <= 0 || value > 30) _ScreenShotNum = 10;
                else _ScreenShotNum = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShotIgnoreStart = (int)ConfigManager.FFmpegConfig.ScreenShotIgnoreStart;

        public int ScreenShotIgnoreStart
        {
            get { return _ScreenShotIgnoreStart; }
            set
            {
                _ScreenShotIgnoreStart = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShotIgnoreEnd = (int)ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd;

        public int ScreenShotIgnoreEnd
        {
            get { return _ScreenShotIgnoreEnd; }
            set
            {
                _ScreenShotIgnoreEnd = value;
                RaisePropertyChanged();
            }
        }
        private bool _SkipExistGif = ConfigManager.FFmpegConfig.SkipExistGif;

        public bool SkipExistGif
        {
            get { return _SkipExistGif; }
            set
            {
                _SkipExistGif = value;
                RaisePropertyChanged();
            }
        }
        private bool _SkipExistScreenShot = ConfigManager.FFmpegConfig.SkipExistScreenShot;

        public bool SkipExistScreenShot
        {
            get { return _SkipExistScreenShot; }
            set
            {
                _SkipExistScreenShot = value;
                RaisePropertyChanged();
            }
        }
        private bool _GifAutoHeight = ConfigManager.FFmpegConfig.GifAutoHeight;

        public bool GifAutoHeight
        {
            get { return _GifAutoHeight; }
            set
            {
                _GifAutoHeight = value;
                RaisePropertyChanged();
            }
        }

        private int _GifWidth = (int)ConfigManager.FFmpegConfig.GifWidth;

        public int GifWidth
        {
            get { return _GifWidth; }
            set
            {
                _GifWidth = value;
                RaisePropertyChanged();
            }
        }
        private int _GifHeight = (int)ConfigManager.FFmpegConfig.GifHeight;

        public int GifHeight
        {
            get { return _GifHeight; }
            set
            {
                _GifHeight = value;
                RaisePropertyChanged();
            }
        }
        private int _GifDuration = (int)ConfigManager.FFmpegConfig.GifDuration;

        public int GifDuration
        {
            get { return _GifDuration; }
            set
            {
                _GifDuration = value;
                RaisePropertyChanged();
            }
        }




        #endregion


        #region "重命名"

        private bool _RemoveTitleSpace = ConfigManager.RenameConfig.RemoveTitleSpace;

        public bool RemoveTitleSpace
        {
            get { return _RemoveTitleSpace; }
            set
            {
                _RemoveTitleSpace = value;
                RaisePropertyChanged();
            }
        }
        private bool _AddRenameTag = ConfigManager.RenameConfig.AddRenameTag;

        public bool AddRenameTag
        {
            get { return _AddRenameTag; }
            set
            {
                _AddRenameTag = value;
                RaisePropertyChanged();
            }
        }
        private string _FormatString = ConfigManager.RenameConfig.FormatString;

        public string FormatString
        {
            get { return _FormatString; }
            set
            {
                _FormatString = value;
                RaisePropertyChanged();
            }
        }


        #endregion



        #region "库"
        private bool _AutoBackup = ConfigManager.Settings.AutoBackup;

        public bool AutoBackup
        {
            get { return _AutoBackup; }
            set
            {
                _AutoBackup = value;
                RaisePropertyChanged();
            }
        }
        private int _AutoBackupPeriodIndex = (int)ConfigManager.Settings.AutoBackupPeriodIndex;

        public int AutoBackupPeriodIndex
        {
            get { return _AutoBackupPeriodIndex; }
            set
            {
                _AutoBackupPeriodIndex = value;
                RaisePropertyChanged();
            }
        }
        private bool _AutoCreatePlayableIndex = ConfigManager.Settings.AutoCreatePlayableIndex;

        public bool AutoCreatePlayableIndex
        {
            get { return _AutoCreatePlayableIndex; }
            set
            {
                _AutoCreatePlayableIndex = value;
                RaisePropertyChanged();
            }
        }
        private bool _IndexCreating;

        public bool IndexCreating
        {
            get { return _IndexCreating; }
            set
            {
                _IndexCreating = value;
                RaisePropertyChanged();
            }
        }



        #endregion


        #region "端口"


        private bool _ListenEnabled = ConfigManager.Settings.ListenEnabled;

        public bool ListenEnabled
        {
            get { return _ListenEnabled; }
            set
            {
                _ListenEnabled = value;
                RaisePropertyChanged();
            }
        }

        private string _ListenPort = ConfigManager.Settings.ListenPort;

        public string ListenPort
        {
            get { return _ListenPort; }
            set
            {
                _ListenPort = value;
                RaisePropertyChanged();
            }
        }


        #endregion

    }
}
