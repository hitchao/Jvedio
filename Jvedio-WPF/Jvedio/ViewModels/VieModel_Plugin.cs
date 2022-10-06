using GalaSoft.MvvmLight;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Plugins;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.VisualTools.WindowHelper;

namespace Jvedio.ViewModel
{
    public class VieModel_Plugin : ViewModelBase
    {


        public Dictionary<string, object> PicPaths { get; set; }

        public VieModel_Plugin()
        {
            PicPaths = new Dictionary<string, object>();
            SetServers();
            RenderPlugins();
            setBasePicPaths();
        }

        public void LoadScanPath(AppDatabase db)
        {
            // 读取配置文件
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
            foreach (var item in GetSortResult(AllFreshPlugins, false))
                CurrentFreshPlugins.Add(item);
        }

        public int SortEnabledIndex = -1;           // 0 启用 1 未启用
        public PluginType SortPluginType = PluginType.None;

        public List<PluginMetaData> GetSortResult(IEnumerable<PluginMetaData> pluginMetaDatas, bool enabledSort = true)
        {
            // 筛选
            IEnumerable<PluginMetaData> list = pluginMetaDatas;
            if (list == null || list.Count() == 0) return new List<PluginMetaData>();

            if (SortEnabledIndex >= 0 && enabledSort)
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
                list = pluginMetaDatas.Where(arg => arg.PluginName.ToLower().IndexOf(PluginSearch.ToLower()) >= 0);
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

                    if (server.Headers == null) server.Headers = string.Empty;
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

        private string _PluginSearch = string.Empty;

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

        private string _BasePicPath = string.Empty;

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

        private string _BigImagePath = string.Empty;

        public string BigImagePath
        {
            get { return _BigImagePath; }

            set
            {
                _BigImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _SmallImagePath = string.Empty;

        public string SmallImagePath
        {
            get { return _SmallImagePath; }

            set
            {
                _SmallImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _PreviewImagePath = string.Empty;

        public string PreviewImagePath
        {
            get { return _PreviewImagePath; }

            set
            {
                _PreviewImagePath = value;
                RaisePropertyChanged();
            }
        }

        private string _ScreenShotPath = string.Empty;

        public string ScreenShotPath
        {
            get { return _ScreenShotPath; }

            set
            {
                _ScreenShotPath = value;
                RaisePropertyChanged();
            }
        }

        private string _ActorImagePath = string.Empty;

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

    }
}
