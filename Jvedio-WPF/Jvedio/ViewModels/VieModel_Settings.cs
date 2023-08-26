using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using SuperControls.Style.Plugin;
using SuperUtils.Common;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.ViewModel
{
    public class VieModel_Settings : ViewModelBase
    {
        public const int PIC_PATH_MODE_COUNT = 3;

        #region "基本属性"
        public Dictionary<string, object> PicPaths { get; set; } = new Dictionary<string, object>();

        private int _TabControlSelectedIndex = (int)ConfigManager.Settings.TabControlSelectedIndex;

        public int TabControlSelectedIndex {
            get { return _TabControlSelectedIndex; }

            set {
                _TabControlSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private string _ViewRenameFormat;

        public string ViewRenameFormat {
            get { return _ViewRenameFormat; }

            set {
                _ViewRenameFormat = value;
                RaisePropertyChanged();
            }
        }

        private bool _AutoHandleHeader = ConfigManager.Settings.AutoHandleHeader;

        public bool AutoHandleHeader {
            get { return _AutoHandleHeader; }

            set {
                _AutoHandleHeader = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowSearchHistory = ConfigManager.Main.ShowSearchHistory;

        public bool ShowSearchHistory {
            get { return _ShowSearchHistory; }

            set {
                _ShowSearchHistory = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Server> _Servers;

        public ObservableCollection<Server> Servers {
            get { return _Servers; }

            set {
                _Servers = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _ScanPath;

        public ObservableCollection<string> ScanPath {
            get { return _ScanPath; }

            set {
                _ScanPath = value;
                RaisePropertyChanged();
            }
        }

        private int _PicPathMode = (int)ConfigManager.Settings.PicPathMode;

        public int PicPathMode {
            get { return _PicPathMode; }

            set {
                _PicPathMode = value;
                RaisePropertyChanged();
            }
        }

        private string _BasePicPath = string.Empty;

        public string BasePicPath {
            get { return _BasePicPath; }

            set {
                _BasePicPath = value;
                if (value != null) {
                    PathType type = (PathType)PicPathMode;
                    if (type != PathType.RelativeToData)
                        PicPaths[type.ToString()] = value;
                }

                RaisePropertyChanged();
            }
        }

        private string _BigImagePath = string.Empty;

        public string BigImagePath {
            get { return _BigImagePath; }

            set {
                _BigImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _SmallImagePath = string.Empty;

        public string SmallImagePath {
            get { return _SmallImagePath; }

            set {
                _SmallImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _PreviewImagePath = string.Empty;

        public string PreviewImagePath {
            get { return _PreviewImagePath; }

            set {
                _PreviewImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _ScreenShotPath = string.Empty;

        public string ScreenShotPath {
            get { return _ScreenShotPath; }

            set {
                _ScreenShotPath = value;
                RaisePropertyChanged();
            }
        }

        private string _ActorImagePath = string.Empty;

        public string ActorImagePath {
            get { return _ActorImagePath; }

            set {
                _ActorImagePath = value;
                RaisePropertyChanged();
            }
        }


        private bool _SkipExistImage = ConfigManager.Settings.SkipExistImage;

        public bool SkipExistImage {
            get { return _SkipExistImage; }

            set {
                _SkipExistImage = value;
                RaisePropertyChanged();
            }
        }
        private bool _DownloadWhenTitleNull = ConfigManager.Settings.DownloadWhenTitleNull;

        public bool DownloadWhenTitleNull {
            get { return _DownloadWhenTitleNull; }

            set {
                _DownloadWhenTitleNull = value;
                RaisePropertyChanged();
            }
        }

        private bool _IgnoreCertVal = ConfigManager.Settings.IgnoreCertVal;

        public bool IgnoreCertVal {
            get { return _IgnoreCertVal; }

            set {
                _IgnoreCertVal = value;
                RaisePropertyChanged();
            }
        }

        private Dictionary<string, ObservableCollection<CrawlerServer>> _CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();

        public Dictionary<string, ObservableCollection<CrawlerServer>> CrawlerServers {
            get { return _CrawlerServers; }

            set {
                _CrawlerServers = value;
                RaisePropertyChanged();
            }
        }
        private PluginMetaData _CurrentPlugin;

        public PluginMetaData CurrentPlugin {
            get { return _CurrentPlugin; }

            set {
                _CurrentPlugin = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _DisplayCrawlerServers = new ObservableCollection<string>();

        public ObservableCollection<string> DisplayCrawlerServers {
            get { return _DisplayCrawlerServers; }

            set {
                _DisplayCrawlerServers = value;
                RaisePropertyChanged();
            }
        }
        private bool _ShowCurrentPlugin = false;

        public bool ShowCurrentPlugin {
            get { return _ShowCurrentPlugin; }

            set {
                _ShowCurrentPlugin = value;
                RaisePropertyChanged();
            }
        }

        private string _ProxyServer = ConfigManager.ProxyConfig.Server;

        public string ProxyServer {
            get { return _ProxyServer; }

            set {
                _ProxyServer = value;
                RaisePropertyChanged();
            }
        }

        private int _ProxyPort = (int)ConfigManager.ProxyConfig.Port;

        public int ProxyPort {
            get { return _ProxyPort; }

            set {
                _ProxyPort = value;
                RaisePropertyChanged();
            }
        }

        private string _ProxyUserName = ConfigManager.ProxyConfig.UserName;

        public string ProxyUserName {
            get { return _ProxyUserName; }

            set {
                _ProxyUserName = value;
                RaisePropertyChanged();
            }
        }

        private string _ProxyPwd = ConfigManager.ProxyConfig.Password;

        public string ProxyPwd {
            get {
                return _ProxyPwd;
            }

            set {
                _ProxyPwd = value;
                RaisePropertyChanged();
            }
        }

        private TaskStatus _TestProxyStatus;

        public TaskStatus TestProxyStatus {
            get { return _TestProxyStatus; }

            set {
                _TestProxyStatus = value;
                RaisePropertyChanged();
            }
        }

        private int _HttpTimeout = (int)ConfigManager.ProxyConfig.HttpTimeout;

        public int HttpTimeout {
            get { return _HttpTimeout; }

            set {
                _HttpTimeout = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "扫描"

        private bool _CopyNFOOverwriteImage = ConfigManager.ScanConfig.CopyNFOOverwriteImage;

        public bool CopyNFOOverwriteImage {
            get { return _CopyNFOOverwriteImage; }

            set {
                _CopyNFOOverwriteImage = value;
                RaisePropertyChanged();
            }
        }
        private bool _CopyNFOPicture = ConfigManager.ScanConfig.CopyNFOPicture;

        public bool CopyNFOPicture {
            get { return _CopyNFOPicture; }

            set {
                _CopyNFOPicture = value;
                RaisePropertyChanged();
            }
        }
        private bool _CopyNFOActorPicture = ConfigManager.ScanConfig.CopyNFOActorPicture;

        public bool CopyNFOActorPicture {
            get { return _CopyNFOActorPicture; }

            set {
                _CopyNFOActorPicture = value;
                RaisePropertyChanged();
            }
        }
        private bool _CopyNFOScreenShot = ConfigManager.ScanConfig.CopyNFOScreenShot;

        public bool CopyNFOScreenShot {
            get { return _CopyNFOScreenShot; }

            set {
                _CopyNFOScreenShot = value;
                RaisePropertyChanged();
            }
        }
        private bool _CopyNFOPreview = ConfigManager.ScanConfig.CopyNFOPreview;

        public bool CopyNFOPreview {
            get { return _CopyNFOPreview; }

            set {
                _CopyNFOPreview = value;
                RaisePropertyChanged();
            }
        }



        private string _CopyNFOActorPath = ConfigManager.ScanConfig.CopyNFOActorPath;

        public string CopyNFOActorPath {
            get { return _CopyNFOActorPath; }

            set {
                _CopyNFOActorPath = value;
                RaisePropertyChanged();
            }
        }
        private string _CopyNFOScreenShotPath = ConfigManager.ScanConfig.CopyNFOScreenShotPath;

        public string CopyNFOScreenShotPath {
            get { return _CopyNFOScreenShotPath; }

            set {
                _CopyNFOScreenShotPath = value;
                RaisePropertyChanged();
            }
        }
        private string _CopyNFOPreviewPath = ConfigManager.ScanConfig.CopyNFOPreviewPath;

        public string CopyNFOPreviewPath {
            get { return _CopyNFOPreviewPath; }

            set {
                _CopyNFOPreviewPath = value;
                RaisePropertyChanged();
            }
        }
        private Dictionary<string, NfoParse> _NfoParseRules;

        public Dictionary<string, NfoParse> NfoParseRules {
            get { return _NfoParseRules; }

            set {
                _NfoParseRules = value;
                RaisePropertyChanged();
            }
        }

        private double _MinFileSize = ConfigManager.ScanConfig.MinFileSize;

        public double MinFileSize {
            get { return _MinFileSize; }

            set {
                _MinFileSize = value;
                RaisePropertyChanged();
            }
        }

        private bool _ScanOnStartUp = ConfigManager.ScanConfig.ScanOnStartUp;

        public bool ScanOnStartUp {
            get { return _ScanOnStartUp; }

            set {
                _ScanOnStartUp = value;
                RaisePropertyChanged();
            }
        }

        private bool _FetchVID = ConfigManager.ScanConfig.FetchVID;

        public bool FetchVID {
            get { return _FetchVID; }

            set {
                _FetchVID = value;
                RaisePropertyChanged();
            }
        }
        private bool _LoadDataAfterScan = ConfigManager.ScanConfig.LoadDataAfterScan;

        public bool LoadDataAfterScan {
            get { return _LoadDataAfterScan; }

            set {
                _LoadDataAfterScan = value;
                RaisePropertyChanged();
            }
        }
        private bool _DataExistsIndexAfterScan = ConfigManager.ScanConfig.DataExistsIndexAfterScan;

        public bool DataExistsIndexAfterScan {
            get { return _DataExistsIndexAfterScan; }

            set {
                _DataExistsIndexAfterScan = value;
                RaisePropertyChanged();
            }
        }
        private bool _ImageExistsIndexAfterScan = ConfigManager.ScanConfig.ImageExistsIndexAfterScan;

        public bool ImageExistsIndexAfterScan {
            get { return _ImageExistsIndexAfterScan; }

            set {
                _ImageExistsIndexAfterScan = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "常规设置"

        private bool _AutoGenScreenShot = ConfigManager.Settings.AutoGenScreenShot;

        public bool AutoGenScreenShot {
            get { return _AutoGenScreenShot; }

            set {
                _AutoGenScreenShot = value;
                RaisePropertyChanged();
            }
        }

        private bool _OpenDataBaseDefault = ConfigManager.Settings.OpenDataBaseDefault;

        public bool OpenDataBaseDefault {
            get { return _OpenDataBaseDefault; }

            set {
                _OpenDataBaseDefault = value;
                RaisePropertyChanged();
            }
        }


        private bool _CloseToTaskBar = ConfigManager.Settings.CloseToTaskBar;

        public bool CloseToTaskBar {
            get { return _CloseToTaskBar; }

            set {
                _CloseToTaskBar = value;

                RaisePropertyChanged();
            }
        }

        private bool _MainWindowVisible;

        public bool MainWindowVisible {
            get { return _MainWindowVisible; }

            set {
                _MainWindowVisible = value;
                RaisePropertyChanged();
            }
        }

        private string _CurrentLanguage = ConfigManager.Settings.CurrentLanguage;

        public string CurrentLanguage {
            get { return _CurrentLanguage; }

            set {
                _CurrentLanguage = value;
                RaisePropertyChanged();
            }
        }



        #endregion

        #region "NFO"
        private bool _SaveInfoToNFO = ConfigManager.Settings.SaveInfoToNFO;

        public bool SaveInfoToNFO {
            get { return _SaveInfoToNFO; }

            set {
                _SaveInfoToNFO = value;
                RaisePropertyChanged();
            }
        }

        private bool _OverwriteNFO = ConfigManager.Settings.OverwriteNFO;

        public bool OverwriteNFO {
            get { return _OverwriteNFO; }

            set {
                _OverwriteNFO = value;
                RaisePropertyChanged();
            }
        }

        private string _NFOSavePath = ConfigManager.Settings.NFOSavePath;

        public string NFOSavePath {
            get { return _NFOSavePath; }

            set {
                _NFOSavePath = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "FFMPEG"
        private string _FFMPEG_Path = ConfigManager.FFmpegConfig.Path;

        public string FFMPEG_Path {
            get { return _FFMPEG_Path; }

            set {
                _FFMPEG_Path = value;
                RaisePropertyChanged();
            }
        }

        private int _ScreenShot_ThreadNum = (int)ConfigManager.FFmpegConfig.ThreadNum;

        public int ScreenShot_ThreadNum {
            get { return _ScreenShot_ThreadNum; }

            set {
                _ScreenShot_ThreadNum = value;
                RaisePropertyChanged();
            }
        }

        private int _ScreenShot_TimeOut = (int)ConfigManager.FFmpegConfig.TimeOut;

        public int ScreenShot_TimeOut {
            get { return _ScreenShot_TimeOut; }

            set {
                _ScreenShot_TimeOut = value;
                RaisePropertyChanged();
            }
        }

        private int _ScreenShotNum = (int)ConfigManager.FFmpegConfig.ScreenShotNum;

        public int ScreenShotNum {
            get { return _ScreenShotNum; }

            set {
                if (value <= 0 || value > 30)
                    _ScreenShotNum = 10;
                else
                    _ScreenShotNum = value;
                RaisePropertyChanged();
            }
        }

        private int _ScreenShotIgnoreStart = (int)ConfigManager.FFmpegConfig.ScreenShotIgnoreStart;

        public int ScreenShotIgnoreStart {
            get { return _ScreenShotIgnoreStart; }

            set {
                _ScreenShotIgnoreStart = value;
                RaisePropertyChanged();
            }
        }

        private int _ScreenShotIgnoreEnd = (int)ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd;

        public int ScreenShotIgnoreEnd {
            get { return _ScreenShotIgnoreEnd; }

            set {
                _ScreenShotIgnoreEnd = value;
                RaisePropertyChanged();
            }
        }

        private bool _SkipExistGif = ConfigManager.FFmpegConfig.SkipExistGif;

        public bool SkipExistGif {
            get { return _SkipExistGif; }

            set {
                _SkipExistGif = value;
                RaisePropertyChanged();
            }
        }

        private bool _SkipExistScreenShot = ConfigManager.FFmpegConfig.SkipExistScreenShot;

        public bool SkipExistScreenShot {
            get { return _SkipExistScreenShot; }

            set {
                _SkipExistScreenShot = value;
                RaisePropertyChanged();
            }
        }
        private bool _ScreenShotAfterImport = ConfigManager.FFmpegConfig.ScreenShotAfterImport;

        public bool ScreenShotAfterImport {
            get { return _ScreenShotAfterImport; }

            set {
                _ScreenShotAfterImport = value;
                RaisePropertyChanged();
            }
        }

        private bool _GifAutoHeight = ConfigManager.FFmpegConfig.GifAutoHeight;

        public bool GifAutoHeight {
            get { return _GifAutoHeight; }

            set {
                _GifAutoHeight = value;
                RaisePropertyChanged();
            }
        }

        private int _GifWidth = (int)ConfigManager.FFmpegConfig.GifWidth;

        public int GifWidth {
            get { return _GifWidth; }

            set {
                _GifWidth = value;
                RaisePropertyChanged();
            }
        }

        private int _GifHeight = (int)ConfigManager.FFmpegConfig.GifHeight;

        public int GifHeight {
            get { return _GifHeight; }

            set {
                _GifHeight = value;
                RaisePropertyChanged();
            }
        }

        private int _GifDuration = (int)ConfigManager.FFmpegConfig.GifDuration;

        public int GifDuration {
            get { return _GifDuration; }

            set {
                _GifDuration = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "重命名"

        private bool _RemoveTitleSpace = ConfigManager.RenameConfig.RemoveTitleSpace;

        public bool RemoveTitleSpace {
            get { return _RemoveTitleSpace; }

            set {
                _RemoveTitleSpace = value;
                RaisePropertyChanged();
            }
        }

        private bool _AddRenameTag = ConfigManager.RenameConfig.AddRenameTag;

        public bool AddRenameTag {
            get { return _AddRenameTag; }

            set {
                _AddRenameTag = value;
                RaisePropertyChanged();
            }
        }

        private string _FormatString = ConfigManager.RenameConfig.FormatString;

        public string FormatString {
            get { return _FormatString; }

            set {
                _FormatString = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "库"
        private bool _AutoBackup = ConfigManager.Settings.AutoBackup;

        public bool AutoBackup {
            get { return _AutoBackup; }

            set {
                _AutoBackup = value;
                RaisePropertyChanged();
            }
        }

        private int _AutoBackupPeriodIndex = (int)ConfigManager.Settings.AutoBackupPeriodIndex;

        public int AutoBackupPeriodIndex {
            get { return _AutoBackupPeriodIndex; }

            set {
                _AutoBackupPeriodIndex = value;
                RaisePropertyChanged();
            }
        }


        private bool _IndexCreating;

        public bool IndexCreating {
            get { return _IndexCreating; }

            set {
                _IndexCreating = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "端口"

        private bool _ListenEnabled = ConfigManager.Settings.ListenEnabled;

        public bool ListenEnabled {
            get { return _ListenEnabled; }

            set {
                _ListenEnabled = value;
                RaisePropertyChanged();
            }
        }

        private string _ListenPort = ConfigManager.Settings.ListenPort;

        public string ListenPort {
            get { return _ListenPort; }

            set {
                _ListenPort = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public VieModel_Settings()
        {
            Init();
        }

        public override void Init()
        {
            SetServers();
            SetBasePicPaths();
            LoadNfoParseData();
            Logger.Info("init view model setting ok");
        }

        public void LoadNfoParseData()
        {
            NfoParseRules = NfoParse.LoadData();
        }


        public void SaveNFOParseData()
        {
            NfoParse.SaveData(NfoParseRules);
        }


        public void LoadScanPath(AppDatabase db)
        {
            // 读取配置文件
            ScanPath = new ObservableCollection<string>();

            List<string> list = JsonUtils.TryDeserializeObject<List<string>>(db.ScanPath);
            if (list != null && list.Count > 0)
                list.ForEach(arg => ScanPath.Add(arg));

            if (ScanPath.Count == 0)
                ScanPath = null;
            Servers = new ObservableCollection<Server>();
        }



        public void SetServers()
        {
            CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();
            foreach (PluginMetaData plugin in CrawlerManager.PluginMetaDatas) {
                string pluginID = plugin.PluginID;
                if (string.IsNullOrEmpty(pluginID))
                    continue;
                string name = plugin.PluginName;
                CrawlerServer crawlerServer = ConfigManager.ServerConfig.CrawlerServers
                    .Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
                if (crawlerServer == null) {
                    crawlerServer = new CrawlerServer();
                    crawlerServer.PluginID = pluginID;
                    CrawlerServers.Add(pluginID, null);
                } else {
                    ObservableCollection<CrawlerServer> crawlers = new ObservableCollection<CrawlerServer>();
                    ConfigManager.ServerConfig.CrawlerServers.Where(arg => arg.PluginID.Equals(pluginID)).
                        ToList().ForEach(t => crawlers.Add(t));
                    if (!CrawlerServers.ContainsKey(pluginID))
                        CrawlerServers.Add(pluginID, crawlers);
                }
            }

            DisplayCrawlerServers = new ObservableCollection<string>();
            foreach (string key in CrawlerServers.Keys) {
                int len = PluginType.Crawler.ToString().Length + 1;
                DisplayCrawlerServers.Add(key.Substring(len));
            }
        }

        public bool SaveServers(Action<string> callback = null)
        {
            List<CrawlerServer> list = new List<CrawlerServer>();
            foreach (string key in CrawlerServers.Keys) {
                List<CrawlerServer> crawlerServers = CrawlerServers[key]?.ToList();
                if (crawlerServers == null || crawlerServers.Count <= 0)
                    continue;
                foreach (CrawlerServer server in crawlerServers) {
                    if (!server.IsHeaderProper()) {
                        string format = "{\"UserAgent\":\"value\",...}";
                        callback?.Invoke($"【{key}】 刮削器处地址为 {server.Url} 的 Headers 不合理，格式必须为：{format}");
                        return false;
                    }

                    if (server.Headers == null)
                        server.Headers = string.Empty;
                    list.Add(server);
                }
            }

            ConfigManager.ServerConfig.CrawlerServers = list;
            ConfigManager.ServerConfig.Save();
            return true;
        }


        public void SetBasePicPaths()
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

    }
}
