﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using Jvedio.Utils;
using Jvedio.Entity;
using Jvedio.Core;
using Jvedio.Core.Crawler;
using Jvedio.Core.Plugins;
using Newtonsoft.Json;
using Jvedio.Core.Enums;
using Jvedio.Utils.Enums;

namespace Jvedio.ViewModel
{
    public class VieModel_Settings : ViewModelBase
    {
        Jvedio.Settings settings = GetWindowByName("Settings") as Settings;
        public VieModel_Settings()
        {

            ThemeList = new ObservableCollection<Theme>();
            foreach (Theme theme in ThemeLoader.Themes)
            {
                if (theme.Name == Skin.白色.ToString() || theme.Name == Skin.黑色.ToString() || theme.Name == Skin.蓝色.ToString()) continue;
                ThemeList.Add(theme);
            }
            setServers();
            setPlugins();
            setBasePicPaths();
        }




        public void LoadScanPath(AppDatabase db)
        {
            //读取配置文件
            ScanPath = new ObservableCollection<string>();
            try
            {
                List<string> list = JsonConvert.DeserializeObject<List<string>>(db.ScanPath);
                if (list != null && list.Count > 0)
                    list.ForEach(arg => ScanPath.Add(arg));
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            if (ScanPath.Count == 0) ScanPath = null;
            Servers = new ObservableCollection<Server>();
        }


        public void setPlugins()
        {

            InstalledPlugins = new List<PluginInfo>();
            foreach (PluginInfo plugin in Global.Plugins.Crawlers)
                InstalledPlugins.Add(plugin);


            CurrentInstalledPlugins = new ObservableCollection<PluginInfo>();
            foreach (var item in getSortResult(InstalledPlugins))
                CurrentInstalledPlugins.Add(item);



            if (AllFreshPlugins != null)
            {
                CurrentFreshPlugins = new ObservableCollection<PluginInfo>();
                foreach (var item in getSortResult(AllFreshPlugins))
                    CurrentFreshPlugins.Add(item);

            }


        }


        public List<PluginInfo> getSortResult(IEnumerable<PluginInfo> pluginInfos)
        {
            IEnumerable<PluginInfo> list = pluginInfos.Where(arg => arg.Name.ToLower().IndexOf(PluginSearch.ToLower()) >= 0);
            if (PluginSortIndex == 0)
            {
                if (PluginSortDesc)
                    list = list.OrderByDescending(arg => arg.Name);
                else
                    list = list.OrderBy(arg => arg.Name);
            }
            else if (PluginSortIndex == 1)
            {
                if (PluginSortDesc)
                    list = list.OrderByDescending(arg => arg.Author);
                else
                    list = list.OrderBy(arg => arg.Author);
            }
            else if (PluginSortIndex == 2)
            {
                if (PluginSortDesc)
                    list = list.OrderByDescending(arg => arg.PublishDate);
                else
                    list = list.OrderBy(arg => arg.PublishDate);
            }
            return list.ToList();
        }

        public void setServers()
        {
            CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();
            foreach (PluginInfo plugin in Global.Plugins.Crawlers)
            {
                string serverName = plugin.ServerName;
                string name = plugin.Name;
                CrawlerServer crawlerServer = GlobalConfig.ServerConfig.CrawlerServers
                    .Where(arg => arg.ServerName.ToLower() == serverName.ToLower() && arg.Name == name).FirstOrDefault();
                if (crawlerServer == null)
                {
                    crawlerServer = new CrawlerServer();
                    crawlerServer.ServerName = serverName;
                    crawlerServer.Name = name;
                    CrawlerServers.Add(plugin.getUID(), null);
                }
                else
                {
                    ObservableCollection<CrawlerServer> crawlers = new ObservableCollection<CrawlerServer>();
                    GlobalConfig.ServerConfig.CrawlerServers.Where(arg => arg.ServerName.ToLower() == serverName.ToLower() && arg.Name == name).
                        ToList().ForEach(t => crawlers.Add(t));
                    if (!CrawlerServers.ContainsKey(plugin.getUID()))
                        CrawlerServers.Add(plugin.getUID(), crawlers);
                }

            }
            DisplayCrawlerServers = new ObservableCollection<string>();
            foreach (string key in CrawlerServers.Keys)
            {
                string name = key.Split('.').Last();
                DisplayCrawlerServers.Add(name);
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
                    int idx = key.IndexOf('.');
                    server.ServerName = key.Substring(0, idx);
                    server.Name = key.Substring(idx + 1);
                    if (server.Headers == null) server.Headers = "";
                    list.Add(server);

                }
            }
            GlobalConfig.ServerConfig.CrawlerServers = list;
            GlobalConfig.ServerConfig.Save();
            return true;
        }



        public int PIC_PATH_MODE_COUNT = 3;
        public void setBasePicPaths()
        {
            //PathType type = (PathType)PicPathMode;
            //BasePicPath = PicPaths[type.ToString()].ToString();
            PicPaths = GlobalConfig.Settings.PicPaths;

            Dictionary<string, string> dict = (Dictionary<string, string>)PicPaths[PathType.RelativeToData.ToString()];
            BigImagePath = dict["BigImagePath"];
            SmallImagePath = dict["SmallImagePath"];
            PreviewImagePath = dict["PreviewImagePath"];
            ScreenShotPath = dict["ScreenShotPath"];
            ActorImagePath = dict["ActorImagePath"];

        }


        private int _TabControlSelectedIndex = (int)GlobalConfig.Settings.TabControlSelectedIndex;

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
        private bool _AutoHandleHeader = GlobalConfig.Settings.AutoHandleHeader;

        public bool AutoHandleHeader
        {
            get { return _AutoHandleHeader; }
            set
            {
                _AutoHandleHeader = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Theme> _ThemeList;

        public ObservableCollection<Theme> ThemeList
        {
            get { return _ThemeList; }
            set
            {
                _ThemeList = value;
                RaisePropertyChanged();
            }
        }

        public List<PluginInfo> InstalledPlugins;
        public List<PluginInfo> AllFreshPlugins;
        private ObservableCollection<PluginInfo> _CurrentInstalledPlugins;

        public ObservableCollection<PluginInfo> CurrentInstalledPlugins
        {
            get { return _CurrentInstalledPlugins; }
            set
            {
                _CurrentInstalledPlugins = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<PluginInfo> _CurrentFreshPlugins;

        public ObservableCollection<PluginInfo> CurrentFreshPlugins
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
        private PluginInfo _CurrentPlugin;

        public PluginInfo CurrentPlugin
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




        private int _PicPathMode = (int)GlobalConfig.Settings.PicPathMode;

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


        public Dictionary<string, object> PicPaths = new Dictionary<string, object>();




        private bool _DownloadPreviewImage = GlobalConfig.Settings.DownloadPreviewImage;

        public bool DownloadPreviewImage
        {
            get { return _DownloadPreviewImage; }
            set
            {
                _DownloadPreviewImage = value;
                RaisePropertyChanged();
            }
        }

        private bool _OverrideInfo = GlobalConfig.Settings.OverrideInfo;

        public bool OverrideInfo
        {
            get { return _OverrideInfo; }
            set
            {
                _OverrideInfo = value;
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

        private string _ProxyServer = GlobalConfig.ProxyConfig.Server;

        public string ProxyServer
        {
            get { return _ProxyServer; }
            set
            {
                _ProxyServer = value;
                RaisePropertyChanged();
            }
        }
        private int _ProxyPort = (int)GlobalConfig.ProxyConfig.Port;

        public int ProxyPort
        {
            get { return _ProxyPort; }
            set
            {
                _ProxyPort = value;
                RaisePropertyChanged();
            }
        }
        private string _ProxyUserName = GlobalConfig.ProxyConfig.UserName;

        public string ProxyUserName
        {
            get { return _ProxyUserName; }
            set
            {
                _ProxyUserName = value;
                RaisePropertyChanged();
            }
        }
        private string _ProxyPwd = GlobalConfig.ProxyConfig.Password;

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
        private int _HttpTimeout = (int)GlobalConfig.ProxyConfig.HttpTimeout;

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

        private bool _CopyNFOPicture = GlobalConfig.ScanConfig.CopyNFOPicture;

        public bool CopyNFOPicture
        {
            get { return _CopyNFOPicture; }
            set
            {
                _CopyNFOPicture = value;
                RaisePropertyChanged();
            }
        }
        private double _MinFileSize = GlobalConfig.ScanConfig.MinFileSize;

        public double MinFileSize
        {
            get { return _MinFileSize; }
            set
            {
                _MinFileSize = value;
                RaisePropertyChanged();
            }
        }
        private bool _ScanOnStartUp = GlobalConfig.ScanConfig.ScanOnStartUp;

        public bool ScanOnStartUp
        {
            get { return _ScanOnStartUp; }
            set
            {
                _ScanOnStartUp = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "常规设置"

        private bool _AutoGenScreenShot = GlobalConfig.Settings.AutoGenScreenShot;

        public bool AutoGenScreenShot
        {
            get { return _AutoGenScreenShot; }
            set
            {
                _AutoGenScreenShot = value;
                RaisePropertyChanged();
            }
        }
        private bool _OpenDataBaseDefault = GlobalConfig.Settings.OpenDataBaseDefault;

        public bool OpenDataBaseDefault
        {
            get { return _OpenDataBaseDefault; }
            set
            {
                _OpenDataBaseDefault = value;
                RaisePropertyChanged();
            }
        }
        private bool _TeenMode = GlobalConfig.Settings.TeenMode;

        public bool TeenMode
        {
            get { return _TeenMode; }
            set
            {
                _TeenMode = value;
                RaisePropertyChanged();
            }
        }
        private bool _CloseToTaskBar = GlobalConfig.Settings.CloseToTaskBar;

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
        private int _SelectedLanguage = (int)GlobalConfig.Settings.SelectedLanguage;

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



        #endregion



        #region "nfo"
        private bool _SaveInfoToNFO = GlobalConfig.Settings.SaveInfoToNFO;

        public bool SaveInfoToNFO
        {
            get { return _SaveInfoToNFO; }
            set
            {
                _SaveInfoToNFO = value;
                RaisePropertyChanged();
            }
        }
        private bool _OverriteNFO = GlobalConfig.Settings.OverriteNFO;

        public bool OverriteNFO
        {
            get { return _OverriteNFO; }
            set
            {
                _OverriteNFO = value;
                RaisePropertyChanged();
            }
        }
        private string _NFOSavePath = GlobalConfig.Settings.NFOSavePath;

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
        private string _FFMPEG_Path = GlobalConfig.FFmpegConfig.Path;

        public string FFMPEG_Path
        {
            get { return _FFMPEG_Path; }
            set
            {
                _FFMPEG_Path = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShot_ThreadNum = (int)GlobalConfig.FFmpegConfig.ThreadNum;

        public int ScreenShot_ThreadNum
        {
            get { return _ScreenShot_ThreadNum; }
            set
            {
                _ScreenShot_ThreadNum = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShot_TimeOut = (int)GlobalConfig.FFmpegConfig.TimeOut;

        public int ScreenShot_TimeOut
        {
            get { return _ScreenShot_TimeOut; }
            set
            {
                _ScreenShot_TimeOut = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShotNum = (int)GlobalConfig.FFmpegConfig.ScreenShotNum;

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
        private int _ScreenShotIgnoreStart = (int)GlobalConfig.FFmpegConfig.ScreenShotIgnoreStart;

        public int ScreenShotIgnoreStart
        {
            get { return _ScreenShotIgnoreStart; }
            set
            {
                _ScreenShotIgnoreStart = value;
                RaisePropertyChanged();
            }
        }
        private int _ScreenShotIgnoreEnd = (int)GlobalConfig.FFmpegConfig.ScreenShotIgnoreEnd;

        public int ScreenShotIgnoreEnd
        {
            get { return _ScreenShotIgnoreEnd; }
            set
            {
                _ScreenShotIgnoreEnd = value;
                RaisePropertyChanged();
            }
        }
        private bool _SkipExistGif = GlobalConfig.FFmpegConfig.SkipExistGif;

        public bool SkipExistGif
        {
            get { return _SkipExistGif; }
            set
            {
                _SkipExistGif = value;
                RaisePropertyChanged();
            }
        }
        private bool _SkipExistScreenShot = GlobalConfig.FFmpegConfig.SkipExistScreenShot;

        public bool SkipExistScreenShot
        {
            get { return _SkipExistScreenShot; }
            set
            {
                _SkipExistScreenShot = value;
                RaisePropertyChanged();
            }
        }
        private bool _GifAutoHeight = GlobalConfig.FFmpegConfig.GifAutoHeight;

        public bool GifAutoHeight
        {
            get { return _GifAutoHeight; }
            set
            {
                _GifAutoHeight = value;
                RaisePropertyChanged();
            }
        }

        private int _GifWidth = (int)GlobalConfig.FFmpegConfig.GifWidth;

        public int GifWidth
        {
            get { return _GifWidth; }
            set
            {
                _GifWidth = value;
                RaisePropertyChanged();
            }
        }
        private int _GifHeight = (int)GlobalConfig.FFmpegConfig.GifHeight;

        public int GifHeight
        {
            get { return _GifHeight; }
            set
            {
                _GifHeight = value;
                RaisePropertyChanged();
            }
        }
        private int _GifDuration = (int)GlobalConfig.FFmpegConfig.GifDuration;

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

        private bool _RemoveTitleSpace = GlobalConfig.RenameConfig.RemoveTitleSpace;

        public bool RemoveTitleSpace
        {
            get { return _RemoveTitleSpace; }
            set
            {
                _RemoveTitleSpace = value;
                RaisePropertyChanged();
            }
        }
        private bool _AddRenameTag = GlobalConfig.RenameConfig.AddRenameTag;

        public bool AddRenameTag
        {
            get { return _AddRenameTag; }
            set
            {
                _AddRenameTag = value;
                RaisePropertyChanged();
            }
        }
        private string _FormatString = GlobalConfig.RenameConfig.FormatString;

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



    }
}
