using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Net;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Core.Server;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Entity.CommonSQL;
using Jvedio.Pages;
using Jvedio.Upgrade;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using SuperControls.Style;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Framework.Tasks;
using SuperUtils.IO;
using SuperUtils.NetWork;
using SuperUtils.Systems;
using SuperUtils.Time;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.Global.UrlManager;
using static Jvedio.Main.Msg;
using static Jvedio.MapperManager;
using static Jvedio.Window_Settings;
using static SuperUtils.WPF.VisualTools.VisualHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : SuperControls.Style.BaseWindow
    {

        public static int NOTICE_INTERVAL = 1800; // 30分钟检测一次
        public static Msg msgCard = new Msg();


        private List<Actress> SelectedActress { get; set; }


        private bool IsToUpdate { get; set; }

        private Window_Edit windowEdit { get; set; }

        private Window_Filter windowFilter { get; set; }

        private Window_Details windowDetails { get; set; }

        public VieModel_Main vieModel { get; set; }



        private static bool CheckingScanStatus { get; set; }

        private static bool CheckingDownloadStatus { get; set; }

        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance { get; set; }



        private CancellationTokenSource LoadSearchCTS { get; set; }

        private CancellationToken LoadSearchCT { get; set; }

        private CancellationTokenSource scan_cts { get; set; }

        private CancellationToken scan_ct { get; set; }


        public static List<string> ClickFilterDict { get; set; }


        // 标签戳，全局缓存，避免每次都查询
        public static List<TagStamp> TagStamps { get; set; }

        // 如果包含以下文本，则显示对应的标签戳
        public static string[] TagStrings_HD { get; set; }

        public static string[] TagStrings_Translated { get; set; }

        public static TimeSpan FadeInterval { get; set; }

        public static DataBaseType CurrentDataBaseType { get; set; }

        public static bool ClickGoBackToStartUp { get; set; }// 是否是点击了返回去到 Startup

        public static DataType CurrentDataType { get; set; }

        private bool AnimatingSideGrid = false;

        static Main()
        {
            TagStamps = new List<TagStamp>();
            TagStrings_HD = new string[] { "hd", "高清" };
            TagStrings_Translated = new string[] { "中文", "日本語", "Translated", "English" };
            FadeInterval = TimeSpan.FromMilliseconds(150); // 淡入淡出时间

            CurrentDataBaseType = DataBaseType.SQLite;

            ClickGoBackToStartUp = false;
            CurrentDataType = DataType.Video;
        }

        public void Init()
        {

            SelectedActress = new List<Actress>();
            ClickFilterDict = new List<string>() { "Genre", "Series", "Studio", "Director", };

            vieModel = new VieModel_Main(this);
            this.DataContext = vieModel;
            BindingEvent(); // 绑定控件事件

            // 初始化任务栏的进度条
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
                taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;

            LoadNotifyIcon();
            this.OnSideTrigger += async () => {
                //SideGridColumn.Width = new GridLength(200);
                AnimatingSideGrid = true;
                ButtonSideTop.Visibility = Visibility.Collapsed;
                await Task.Run(async () => {
                    for (int i = 0; i <= 200; i += 10) {
                        await App.Current.Dispatcher.InvokeAsync(() => {
                            SideGridColumn.Width = new GridLength(i);
                        });
                        await Task.Delay(5);
                    }
                });
                AnimatingSideGrid = false;
            };
        }

        public Main()
        {
            InitializeComponent();
            Init();
        }
        public void LoadNotifyIcon()
        {
            SetNotiIconPopup(notiIconPopup);
            this.OnNotifyIconMouseLeftClick += (s, e) => {
                ShowMainWindow(s, new RoutedEventArgs());
            };
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            //AdjustWindow(); // 还原窗口为上一次状态
            ConfigFirstRun();
            InitThemeSelector();
            //SetSkin(); // 设置主题颜色
            InitNotice(); // 初始化公告

            setDataBases(); // 设置当前下拉数据库
            setRecentWatched(); // 显示最近播放

            // vieModel.GetFilterInfo(); //todo 筛选器
            vieModel.Statistic();
            //// todo 设置图片类型
            // await vieModel.InitLettersNavigation(); // todo
            BindingEventAfterRender(); // render 后才绑定的事件
            InitTagStamp();
            vieModel.MainDataChecked = true;


            OpenListen();

            //Window_Settings window_Settings = new Window_Settings();
            //window_Settings.Owner = this;
            //window_Settings.ShowDialog();
            // new MsgBox( "demo").ShowDialog();
            InitUpgrade();
            //ShowPluginWindow(null, null);

            CheckServerStatus();

            // 加载数据
            vieModel.HandleSideButtonCmd("All");
        }

        public async void CheckServerStatus()
        {
            vieModel.ServerStatus = await ServerManager.CheckStatus();
        }

        public void SetAllSelect()
        {
            // todo tab
        }


        public void InitThemeSelector()
        {
            ThemeSelectorDefault.AddTransParentColor("TabItem.Background");
            ThemeSelectorDefault.AddTransParentColor("ListBoxItem.Background");
            ThemeSelectorDefault.AddTransParentColor("Window.Side.Background");
            ThemeSelectorDefault.AddTransParentColor("Window.Side.Hover.Background");
            ThemeSelectorDefault.SetThemeConfig(ConfigManager.ThemeConfig.ThemeIndex, ConfigManager.ThemeConfig.ThemeID);
            ThemeSelectorDefault.onThemeChanged += (ThemeIdx, ThemeID) => {
                ConfigManager.ThemeConfig.ThemeIndex = ThemeIdx;
                ConfigManager.ThemeConfig.ThemeID = ThemeID;
                ConfigManager.ThemeConfig.Save();
                SetAllSelect();

                windowDetails?.SetSkin();
            };
            ThemeSelectorDefault.onBackGroundImageChanged += (image) => {
                ImageBackground.Source = image;
                StyleManager.BackgroundImage = image;
            };
            ThemeSelectorDefault.onSetBgColorTransparent += () => {
                BorderTitle.Background = Brushes.Transparent;
            };

            ThemeSelectorDefault.onReSetBgColorBinding += () => {
                BorderTitle.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
            };

            ThemeSelectorDefault.InitThemes();
        }


        public void InitUpgrade()
        {
            UpgradeHelper.Init(this);
            CheckUpgrade(); // 检查更新
        }


        private void OpenListen()
        {
            if (ConfigManager.Settings.ListenEnabled) {
                // 开启端口监听
            }
        }

        public class Msg
        {
            public EventHandler MsgShown;

            public class MessageEventArg : EventArgs
            {
                public MessageEventArg(Message message)
                {
                    this.Message = message;
                }

                public Message Message { get; set; }
            }

            public void Success(string msg)
            {
                MessageCard.Success(msg);
                Message message = new Message(MessageCard.MessageCardType.Success, msg);
                MsgShown?.Invoke(this, new MessageEventArg(message));
            }

            public void Error(string msg)
            {
                MessageCard.Error(msg);
                Message message = new Message(MessageCard.MessageCardType.Error, msg);
                MsgShown?.Invoke(this, new MessageEventArg(message));
            }

            public void Warning(string msg)
            {
                MessageCard.Warning(msg);
                Message message = new Message(MessageCard.MessageCardType.Warning, msg);
                MsgShown?.Invoke(this, new MessageEventArg(message));
            }

            public void Info(string msg)
            {
                MessageCard.Info(msg);
                Message message = new Message(MessageCard.MessageCardType.Info, msg);
                MsgShown?.Invoke(this, new MessageEventArg(message));
            }
        }

        public void InitTagStamp()
        {
            // 记住之前的状态
            List<TagStamp> tagStamps = vieModel.TagStamps.ToList();
            Main.TagStamps = tagStampMapper.GetAllTagStamp();
            if (tagStamps != null && tagStamps.Count > 0) {
                foreach (var item in Main.TagStamps) {
                    TagStamp tagStamp = tagStamps.FirstOrDefault(arg => arg.TagID == item.TagID);
                    if (tagStamp != null)
                        item.Selected = tagStamp.Selected;
                }
            }
            vieModel.InitCurrentTagStamps(tagStamps);
        }

        private void BindingEventAfterRender()
        {


            SetComboboxID();

            // 搜索框事件
            searchBox.TextChanged += RefreshCandidate;
            searchTabControl.SelectionChanged += (s, e) => {
                if (ConfigManager.Main.SearchSelectedIndex == searchTabControl.SelectedIndex)
                    return;
                ConfigManager.Main.SearchSelectedIndex = searchTabControl.SelectedIndex;
                RefreshCandidate(null, null);
            };

            // 加载关联影片完成
            //vieModel.LoadAssocMetaDataCompleted += (s, e) => {
            //    //SetAssocSelected();
            //    if (ConfigManager.Settings.AutoGenScreenShot)
            //        AutoGenScreenShot(vieModel.AssociationDatas);
            //};

            // 下载中
            Global.DownloadManager.Dispatcher.onWorking += (s, e) => {
                double progress = Global.DownloadManager.Dispatcher.Progress;
                if (progress is double.NaN)
                    progress = 0;
                vieModel.DownLoadProgress = progress;
                if (progress < 100)
                    vieModel.DownLoadVisibility = Visibility.Visible;
                else
                    vieModel.DownLoadVisibility = Visibility.Hidden;

                // 任务栏进度条
                Dispatcher.Invoke(() => {
                    if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null) {
                        taskbarInstance.SetProgressValue((int)progress, 100, this);
                        if (progress >= 100 || progress <= 0)
                            taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
                        else
                            taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
                    }
                });
            };

            // 截图中
            Global.FFmpegManager.Dispatcher.onWorking += (s, e) => {
                vieModel.ScreenShotProgress = Global.FFmpegManager.Dispatcher.Progress;
                vieModel.ScreenShotVisibility = Visibility.Visible;
            };







            // 长时间暂停
            Global.DownloadManager.Dispatcher.onLongDelay += (s, e) => {
                string message = (e as MessageCallBackEventArgs).Message;
                int.TryParse(message, out int value);
                vieModel.DownloadLongTaskDelay = value / 1000;
            };




            // 此处参考：https://social.msdn.microsoft.com/Forums/vstudio/en-US/cefcfaa5-cb86-426f-b57a-b31a3ea5fcdd/how-to-add-eventsetter-by-code?forum=wpf
            SearchBoxListItemContainerStyle = (System.Windows.Style)this.Resources["SearchBoxListItemContainerStyle"];
            EventSetter eventSetter = new EventSetter() {
                Event = ListBoxItem.MouseDoubleClickEvent,
                Handler = new MouseButtonEventHandler(ListBoxItem_MouseDoubleClick)
            };
            SearchBoxListItemContainerStyle.Setters.Add(eventSetter);


            Main.OnRecvWinMsg += (str) => {
                Logger.Info($"recv win msg: {str}");
                switch (str) {
                    case Win32Helper.WIN_CUSTOM_MSG_OPEN_WINDOW:
                        SetWindowVisualStatus(true, true);
                        break;
                    default:
                        break;
                }

            };

        }

        private static Style SearchBoxListItemContainerStyle { get; set; }


        public void SetComboboxID()
        {
            vieModel.CurrentDbId = vieModel.DataBases.ToList().FindIndex(arg => arg.DBId == ConfigManager.Main.CurrentDBId);
        }

        private async void RefreshCandidate(object sender, TextChangedEventArgs e)
        {
            List<string> list = await vieModel.GetSearchCandidate();
            int idx = (int)ConfigManager.Main.SearchSelectedIndex;
            TabItem tabItem = searchTabControl.Items[idx] as TabItem;
            AddOrRefreshItem(tabItem, list);
        }

        private void AddOrRefreshItem(TabItem tabItem, List<string> list)
        {
            ListBox listBox;
            if (tabItem.Content == null) {
                listBox = new ListBox();
                tabItem.Content = listBox;
            } else {
                listBox = tabItem.Content as ListBox;
            }

            listBox.Margin = new Thickness(0, 0, 0, 5);
            listBox.Style = (System.Windows.Style)App.Current.Resources["NormalListBox"];
            listBox.ItemContainerStyle = SearchBoxListItemContainerStyle;
            listBox.Background = Brushes.Transparent;
            listBox.ItemsSource = list;
            if (vieModel.TabSelectedIndex == 0 && !string.IsNullOrEmpty(vieModel.SearchText))
                vieModel.Searching = true;
        }

        public void setDataBases()
        {
            List<AppDatabase> appDatabases =
                appDatabaseMapper.SelectList(new SelectWrapper<AppDatabase>().Eq("DataType", (int)Main.CurrentDataType));
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            appDatabases.ForEach(db => temp.Add(db));
            vieModel.DataBases = temp;
            if (temp.Count > 0) {
                vieModel.CurrentAppDataBase = appDatabases.Where(arg => arg.DBId == ConfigManager.Main.CurrentDBId).FirstOrDefault();
                if (vieModel.CurrentAppDataBase == null)
                    vieModel.CurrentAppDataBase = temp[0];
            }
        }

        private void setRecentWatched()
        {
            SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("DataType", (int)Main.CurrentDataType).NotEq("ViewDate", string.Empty);
            long count = metaDataMapper.SelectCount(wrapper);
            vieModel.RecentWatchedCount = count;
        }

        // todo 热键
        #region "热键"

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Console.WriteLine("***************OnSourceInitialized***************");

            // 热键
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            // 注册热键
            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (Properties.Settings.Default.HotKey_Enable && modifier != 0 && vk != 0) {
                UnregisterHotKey(_windowHandle, HOTKEY_ID); // 取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success) {
                    new MsgBox(SuperControls.Style.LangManager.GetValueByKey("HotKeyConflict")).ShowDialog(this);
                }
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg) {
                case WM_HOTKEY:
                    switch (wParam.ToInt32()) {
                        case HOTKEY_ID:
                            int key = ((int)lParam >> 16) & 0xFFFF;
                            if (key == Properties.Settings.Default.HotKey_VK) {
                                if (TaskIconVisible) {
                                    SetWindowVisualStatus(false, false);
                                } else {
                                    SetWindowVisualStatus(!WindowsVisible, !WindowsVisible);
                                }
                            }

                            handled = true;
                            break;
                    }

                    break;
            }

            return IntPtr.Zero;
        }

        //private void SetWindowVisualStatus(bool visible, bool taskIconVisible = true)
        //{
        //    if (visible)
        //    {
        //        bool showMain = true;               // 如果是库选择界面，则不显示 Main 窗口
        //        string nameMain = "Main";
        //        string nameStartUp = "WindowStartUp";
        //        foreach (Window window in App.Current.Windows)
        //        {
        //            string name = window.GetType().Name;
        //            if (name.Equals(nameStartUp))
        //            {
        //                showMain = false;
        //                break;
        //            }
        //        }

        //        foreach (Window window in App.Current.Windows)
        //        {
        //            if (OpeningWindows.Contains(window.GetType().ToString()))
        //            {
        //                string name = window.GetType().Name;
        //                if (name.Equals(nameMain) && !showMain) continue;
        //                AnimateWindow(window);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        OpeningWindows.Clear();
        //        foreach (Window window in App.Current.Windows)
        //        {
        //            window.Hide();
        //            OpeningWindows.Add(window.GetType().ToString());
        //        }
        //    }

        //    vieModel.TaskIconVisible = taskIconVisible;
        //    SetIconVisible(taskIconVisible);
        //    WindowsVisible = visible;
        //}

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID); // 取消热键
            windowFilter?.Close();
            base.OnClosed(e);
        }

        #endregion

        // 绑定事件
        private void BindingEvent()
        {



            // 绑定消息
            msgCard.MsgShown += (s, ev) => {
                MessageEventArg eventArg = ev as MessageEventArg;
                if (eventArg != null && vieModel.Message != null)
                    vieModel.Message.Add(eventArg.Message);
            };
        }

        public childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null)
                return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem)
                    return (childItem)child;
                childItem childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }




        public void Notify_Close(object sender, RoutedEventArgs e)
        {
            notiIconPopup.IsOpen = false;
            this.CloseWindow();
        }

        public void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            SetWindowVisualStatus(true);
            notiIconPopup.IsOpen = false;
        }

        private void AnimateWindow(Window window)
        {
            window.Show();
            double opacity = 1;
            var anim = new DoubleAnimation(1, opacity, (Duration)FadeInterval, FillBehavior.Stop);
            anim.Completed += (s, _) => window.Opacity = opacity;
            window.BeginAnimation(UIElement.OpacityProperty, anim);
        }




        public void InitNotice()
        {
            noticeViewer.SetConfig(UrlManager.NoticeUrl, ConfigManager.Main.LatestNotice);
            noticeViewer.onError += (error) => {
                App.Logger?.Error(error);
            };

            noticeViewer.onShowMarkdown += (markdown) => {
                //MessageCard.Info(markdown);
            };
            noticeViewer.onNewNotice += (newNotice) => {
                ConfigManager.Main.LatestNotice = newNotice;
                ConfigManager.Main.Save();
            };

            noticeViewer.BeginCheckNotice();
        }




        #region "监听文件变动"

        // todo 监听文件变动
        public FileSystemWatcher[] fileSystemWatcher { get; set; }

        public string fileWatcherMessage { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void AddListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            fileSystemWatcher = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++) {
                try {
                    if (drives[i] == @"C:\") {
                        continue;
                    }

                    FileSystemWatcher watcher = new FileSystemWatcher {
                        Path = drives[i],
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                        Filter = "*.*",
                    };
                    watcher.Created += OnCreated;
                    watcher.Deleted += OnDeleted;
                    watcher.EnableRaisingEvents = true;
                    fileSystemWatcher[i] = watcher;
                } catch {
                    fileWatcherMessage += drives[i] + ",";
                    continue;
                }
            }

            if (fileWatcherMessage != string.Empty)
                msgCard.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_WatchFail")} {fileWatcherMessage}");
        }

        // todo
        private static void OnCreated(object obj, FileSystemEventArgs e)
        {
            // 导入数据库

            // if (ScanHelper.IsProperMovie(e.FullPath))
            // {
            //    FileInfo fileInfo = new FileInfo(e.FullPath);

            // //获取创建日期
            //    string createDate = "";
            //    try { createDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
            //    catch { }
            //    if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Movie movie = new Movie()
            //    {
            //        filepath = e.FullPath,
            //        id = Identify.GetVID(fileInfo.Name),
            //        filesize = fileInfo.Length,
            //        vediotype = Identify.GetVideoType(Identify.GetVID(fileInfo.Name)),
            //        otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        scandate = createDate
            //    };
            //    if (!string.IsNullOrEmpty(movie.id) & movie.vediotype > 0) { DataBase.InsertScanMovie(movie); }
            //    Console.WriteLine($"成功导入{e.FullPath}");
            // }
        }

        // todo
        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            // if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            // {
            //    DataBase.DeleteByField("movie", "filepath", e.FullPath);
            // }
            // Console.WriteLine("成功删除" + e.FullPath);
        }

        #endregion

        public void AdjustWindow()
        {
            if (ConfigManager.Main.FirstRun) {
                this.Width = SystemParameters.WorkArea.Width * 0.8;
                this.Height = SystemParameters.WorkArea.Height * 0.8;
            } else {
                if (ConfigManager.Main.Height == SystemParameters.WorkArea.Height && ConfigManager.Main.Width < SystemParameters.WorkArea.Width) {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                } else {
                    this.Left = ConfigManager.Main.X;
                    this.Top = ConfigManager.Main.Y;
                    this.Width = ConfigManager.Main.Width;
                    this.Height = ConfigManager.Main.Height;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 停止所有任务
            try {
                // todo 下载程序
                if (vieModel.DownLoadTasks != null) {
                    foreach (var item in vieModel.DownLoadTasks) {
                        item.Cancel();
                    }
                }

                LoadSearchCTS?.Cancel();
                scan_cts?.Cancel();
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            // 关闭所有窗口
            App.Current.Shutdown();
        }

        public void FadeOut()
        {
            if (Properties.Settings.Default.EnableWindowFade) {
                var anim = new DoubleAnimation(0, (Duration)FadeInterval);
                anim.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            } else {
                this.Close();
            }
        }



        private void OpenFeedBack(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(FeedBackUrl);
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(WikiUrl);
        }

        private void OpenJvedioWebPage(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(WebPageUrl);
        }

        //private async void CheckUpgrade()
        //{
        //    // 启动后检查更新
        //    await Task.Delay(UpgradeHelper.AUTO_CHECK_UPGRADE_DELAY);
        //    try
        //    {
        //        (string LatestVersion, string ReleaseDate, string ReleaseNote) result = await UpgradeHelper.GetUpgradeInfo();
        //        string remote = result.LatestVersion;
        //        if (!string.IsNullOrEmpty(remote))
        //        {
        //            string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //            if (local.CompareTo(remote) < 0)
        //            {
        //                // todo 
        //                //new Dialog_Upgrade(this, false, remote, result.ReleaseDate, result.ReleaseNote).ShowDialog();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //    }
        //}

        private async void CheckUpgrade()
        {
            // 启动后检查更新
            try {
                await Task.Delay(UpgradeHelper.AUTO_CHECK_UPGRADE_DELAY);
                (string LatestVersion, string ReleaseDate, string ReleaseNote) result = await UpgradeHelper.GetUpgradeInfo();
                string remote = result.LatestVersion;
                string ReleaseDate = result.ReleaseDate;
                if (!string.IsNullOrEmpty(remote)) {
                    string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    //local = local.Substring(0, local.Length - ".0.0".Length);
                    if (local.CompareTo(remote) < 0) {
                        bool opened = (bool)new MsgBox(
                            $"存在新版本\n版本：{remote}\n日期：{ReleaseDate}").ShowDialog();
                        if (opened)
                            UpgradeHelper.OpenWindow();
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        // todo 优化搜索栏
        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                // doSearch(sender, null);
            } else if (e.Key == Key.Down) {
                // int count = vieModel.CurrentSearchCandidate.Count;

                // SearchSelectIdex += 1;
                // if (SearchSelectIdex >= count) SearchSelectIdex = 0;
                // SetSearchSelect();
            } else if (e.Key == Key.Up) {
                // int count = vieModel.CurrentSearchCandidate.Count;
                // SearchSelectIdex -= 1;
                // if (SearchSelectIdex < 0) SearchSelectIdex = count - 1;
                // SetSearchSelect();
            } else if (e.Key == Key.Escape) {
                vieModel.Searching = false;
            } else if (e.Key == Key.Delete) {
                // searchBox.clearte();
                searchBox.ClearText();
            } else if (e.Key == Key.Tab) {
                // int maxIndex = searchTabControl.Items.Count - 1;
                // int idx = searchTabControl.SelectedIndex;
                // if (idx + 1 > maxIndex)
                // {
                //    idx = 0;
                // }
                // else
                // {
                //    idx++;
                // }
                // searchTabControl.SelectedIndex = idx;
                // e.Handled = true;
                // searchBox.Focus();
                // searchTabControl.Focus();
            }
        }

        public void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            string text = tb.Text;
            if (string.IsNullOrEmpty(text) || text.IndexOf("(") <= 0)
                return;
            string labelName = text.Substring(0, text.IndexOf("("));
            ShowSameLabel(labelName);
            vieModel.StatusText = labelName;
        }

        public void ShowSameLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
                return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("LabelName", label);
            //vieModel.ExtraWrapper = wrapper;
            vieModel.ClickFilterType = "Label";
            // todo tab
            //pagination.CurrentPageChange -= Pagination_CurrentPageChange;
            //vieModel.CurrentPage = 1;
            //vieModel.LoadData();
            //pagination.CurrentPageChange += Pagination_CurrentPageChange;
        }

        public void ShowSameString(string str, string clickFilterType = "")
        {
            if (vieModel.ClassifySelectedIndex >= ClickFilterDict.Count || string.IsNullOrEmpty(str))
                return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            if (string.IsNullOrEmpty(clickFilterType))
                clickFilterType = ClickFilterDict[vieModel.ClassifySelectedIndex];
            wrapper.Like(clickFilterType, str);
            //vieModel.ExtraWrapper = wrapper;
            vieModel.ClickFilterType = clickFilterType;

            // todo tab
            //vieModel.CurrentPage = 1;
            //vieModel.LoadData();
        }

        private void ShowSameString(object sender, MouseButtonEventArgs e)
        {
            // todo 存在一些问题：like '%demo%' => '%demo-xxx%'，导致数目多出
            TextBlock tb = sender as TextBlock;
            string text = tb.Text;
            if (string.IsNullOrEmpty(text) || text.IndexOf("(") <= 0)
                return;
            string labelName = text.Substring(0, text.IndexOf("("));
            ShowSameString(labelName);
            vieModel.StatusText = labelName;
        }



        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            downloadStatusPopup.IsOpen = true;
        }

        public void ShowScreenShotPopup(object sender, MouseButtonEventArgs e)
        {
            screenShotStatusPopup.IsOpen = true;
        }





        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddScanTask(dragdropFiles);
            dragOverBorder.Visibility = Visibility.Collapsed;
        }





        // todo
#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        public async void TranslateMovie(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            // if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO)
            // {
            //    msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_SetYoudao"));
            //    return;
            // }

            // if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();

            // string id = GetIDFromMenuItem(sender, 1);
            // Movie CurrentMovie = GetMovieFromVieModel(id);
            // if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            // string result = "";
            // MySqlite dataBase = new MySqlite("Translate");

            // int successNum = 0;
            // int failNum = 0;
            // int translatedNum = 0;

            // foreach (Movie movie in vieModel.SelectedVideo)
            // {

            // //检查是否已经翻译过，如有则跳过
            //    if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id))) { translatedNum++; continue; }
            //    if (movie.title != "")
            //    {

            // if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.title);
            //        //保存
            //        if (result != "")
            //        {

            // dataBase.SaveYoudaoTranslateByID(movie.id, movie.title, result, "title");

            // //显示
            //            int index1 = vieModel.CurrentVideoList.IndexOf(vieModel.CurrentVideoList.Where(arg => arg.id == movie.id).First()); ;
            //            int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
            //            int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
            //            movie.title = result;
            //            try
            //            {
            //                vieModel.CurrentVideoList[index1] = null;
            //                vieModel.MovieList[index2] = null;
            //                vieModel.FilterMovieList[index3] = null;
            //                vieModel.CurrentVideoList[index1] = movie;
            //                vieModel.MovieList[index2] = movie;
            //                vieModel.FilterMovieList[index3] = movie;
            //                successNum++;
            //            }
            //            catch (ArgumentNullException) { }

            // }

            // }
            //    else { failNum++; }

            // if (movie.plot != "")
            //    {
            //        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.plot);
            //        //保存
            //        if (result != "")
            //        {
            //            dataBase.SaveYoudaoTranslateByID(movie.id, movie.plot, result, "plot");
            //            dataBase.CloseDB();
            //        }

            // }

            // }
            // dataBase.CloseDB();
            // msgCard.Success($"{SuperControls.Style.LangManager.GetValueByKey("Message_Success")Num} {successNum}");
            // msgCard.Error($"{SuperControls.Style.LangManager.GetValueByKey("Message_Fail")Num} {failNum}");
            // msgCard.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_SkipNum")} {translatedNum}");

            // if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }

#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        public async void GenerateActor(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            // if (!Properties.Settings.Default.Enable_BaiduAI) { msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_SetBaiduAI")); return; }
            // if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();

            // string id = GetIDFromMenuItem(sender, 1);
            // Movie CurrentMovie = GetMovieFromVieModel(id);
            // if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            // this.Cursor = Cursors.Wait;
            // int successNum = 0;

            // foreach (Movie movie in vieModel.SelectedVideo)
            // {
            //    if (movie.actor == "") continue;
            //    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";

            // string name;
            //    if (vieModel.ActorInfoGrid == Visibility.Visible)
            //        name = vieModel.Actress.name;
            //    else
            //        name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];

            // string ActressesPicPath = Properties.Settings.Default.BasePicPath + $"Actresses\\{name}.jpg";
            //    if (File.Exists(BigPicPath))
            //    {
            //        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
            //        if (int32Rect != Int32Rect.Empty)
            //        {
            //            await Task.Delay(500);
            //            //切割演员头像
            //            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
            //            BitmapImage bitmapImage = ImageHelper.BitmapToBitmapImage(SourceBitmap);
            //            ImageSource actressImage = ImageHelper.CutImage(bitmapImage, ImageHelper.GetActressRect(bitmapImage, int32Rect));
            //            System.Drawing.Bitmap bitmap = ImageHelper.ImageSourceToBitmap(actressImage);
            //            try { bitmap.Save(ActressesPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++; }
            //            catch (Exception ex) { Logger.LogE(ex); }
            //        }
            //    }
            //    else
            //    {
            //        msgCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_PosterMustExist"));
            //    }
            // }
            // msgCard.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_Success")Num} {successNum} / {vieModel.SelectedVideo.Count}");
            ////更新到窗口中
            // foreach (Movie movie1 in vieModel.SelectedVideo)
            // {
            //    if (!string.IsNullOrEmpty(movie1.actor) && movie1.actor.IndexOf(vieModel.Actress.name) >= 0)
            //    {
            //        vieModel.Actress.smallimage = GetActorImage(vieModel.Actress.name);
            //        break;
            //    }
            // }

            // if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            // this.Cursor = Cursors.Arrow;
        }


#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            // if (!Properties.Settings.Default.Enable_BaiduAI) { msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_SetBaiduAI")); return; }
            // if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            // string id = GetIDFromMenuItem(sender, 1);
            // Movie CurrentMovie = GetMovieFromVieModel(id);
            // if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            // int successNum = 0;
            // this.Cursor = Cursors.Wait;
            // foreach (Movie movie in vieModel.SelectedVideo)
            // {
            //    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
            //    string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
            //    if (File.Exists(BigPicPath))
            //    {
            //        System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
            //        BitmapImage bitmapImage = ImageHelper.BitmapToBitmapImage(SourceBitmap);
            //        if (Properties.Settings.Default.HalfCutOFf)
            //        {
            //            double rate = 380f / 800f;

            // Int32Rect int32Rect = new Int32Rect() { Height = SourceBitmap.Height, Width = (int)(rate * SourceBitmap.Width), X = (int)((1 - rate) * SourceBitmap.Width), Y = 0 };
            //            ImageSource smallImage = ImageHelper.CutImage(bitmapImage, int32Rect);
            //            System.Drawing.Bitmap bitmap = ImageHelper.ImageSourceToBitmap(smallImage);
            //            try
            //            {
            //                bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++;
            //            }
            //            catch (Exception ex) { Logger.LogE(ex); }
            //        }
            //        else
            //        {
            //            Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
            //            if (int32Rect != Int32Rect.Empty)
            //            {
            //                await Task.Delay(500);
            //                //切割缩略图
            //                ImageSource smallImage = ImageHelper.CutImage(bitmapImage, ImageHelper.GetRect(bitmapImage, int32Rect));
            //                System.Drawing.Bitmap bitmap = ImageHelper.ImageSourceToBitmap(smallImage);
            //                try
            //                {
            //                    bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++;
            //                }
            //                catch (Exception ex) { Logger.LogE(ex); }
            //            }

            // }

            // //读取
            //        int index1 = vieModel.CurrentVideoList.IndexOf(movie);
            //        int index2 = vieModel.MovieList.IndexOf(movie);
            //        int index3 = vieModel.FilterMovieList.IndexOf(movie);
            //        movie.smallimage = ImageHelper.GetBitmapImage(movie.id, "SmallPic");

            // vieModel.CurrentVideoList[index1] = null;
            //        vieModel.MovieList[index2] = null;
            //        vieModel.FilterMovieList[index3] = null;
            //        vieModel.CurrentVideoList[index1] = movie;
            //        vieModel.MovieList[index2] = movie;
            //        vieModel.FilterMovieList[index3] = movie;

            // }
            //    else
            //    {
            //        msgCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_PosterMustExist"));
            //    }

            // }
            // msgCard.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_Success")Num} {successNum} / {vieModel.SelectedVideo.Count}");

            // if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            // this.Cursor = Cursors.Arrow;
        }


        private void SetConfigValue()
        {
            ConfigManager.Main.X = this.Left;
            ConfigManager.Main.Y = this.Top;
            ConfigManager.Main.Width = this.Width;
            ConfigManager.Main.Height = this.Height;
            //ConfigManager.Main.WindowState = (long)baseWindowState;
            ConfigManager.Main.SearchSelectedIndex = vieModel.SearchSelectedIndex;
            ConfigManager.Main.ClassifySelectedIndex = vieModel.ClassifySelectedIndex;
            ConfigManager.Main.SideGridWidth = SideGridColumn.ActualWidth;

            ConfigManager.Main.Save();

            ConfigManager.VideoConfig.Save();

            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            SetConfigValue();

            //if (!IsToUpdate && ConfigManager.Settings.CloseToTaskBar && this.IsVisible == true)
            //{
            //    //SetWindowVisualStatus(false);
            //    e.Cancel = true;
            //}
        }


        private void AddScanTask(string[] scanFileList)
        {
            vieModel.ScanStatus = "Scanning";

            List<string> files = new List<string>();
            List<string> paths = new List<string>();

            foreach (var item in scanFileList) {
                if (FileHelper.IsFile(item))
                    files.Add(item);
                else
                    paths.Add(item);
            }

            Core.Scan.ScanTask scanTask = new Core.Scan.ScanTask(paths, files);

            scanTask.onCanceled += (s, ev) => {
                Console.WriteLine("取消扫描任务");
            };
            scanTask.onError += (s, ev) => {
                msgCard.Error((ev as MessageCallBackEventArgs).Message);
            };
            scanTask.onCompleted += (s, ev) => {
                if (scanTask.Success) {

                    Dispatcher.Invoke(() => {
                        vieModel.Statistic();
                        ScanResult scanResult = scanTask.ScanResult;
                        List<Video> insertVideos = null;
                        if (scanResult != null) {
                            insertVideos = scanResult.InsertVideos;
                            MessageCard.Info($"总数    {scanResult.TotalCount.ToString().PadRight(8)}已导入    {scanResult.Import.Count}{Environment.NewLine}" +
                                $"更新    {scanResult.Update.Count.ToString().PadRight(8)} 未导入    {scanResult.NotImport.Count}");
                        }

                        // todo tab
                        //if (ConfigManager.ScanConfig.LoadDataAfterScan)
                        //    vieModel.LoadData();
                        if (ConfigManager.FFmpegConfig.ScreenShotAfterImport) {
                            ScreenShotAfterImport(insertVideos);
                        }


                        if (ConfigManager.ScanConfig.ImageExistsIndexAfterScan)
                            SetImageExistsIndexAfterScan();


                    });
                }
                (s as ScanTask).Running = false;
            };
            vieModel.ScanTasks.Add(scanTask);
            scanTask.Start();
            setScanStatus();
        }

        private void SetImageExistsIndexAfterScan()
        {

        }
        private void SetDataExistsIndexAfterScan()
        {

        }


        private void ScreenShotAfterImport(List<Video> import)
        {
            // todo tab
            //if (import?.Count > 0 && File.Exists(ConfigManager.FFmpegConfig.Path)) {
            //    for (int i = import.Count - 1; i >= 0; i--)
            //        screenShotVideo(import[i], false);

            //    if (!Global.FFmpegManager.Dispatcher.Working)
            //        Global.FFmpegManager.Dispatcher.BeginWork();
            //}
        }

        private void setScanStatus()
        {
            if (!CheckingScanStatus) {
                CheckingScanStatus = true;
                Task.Run(() => {
                    while (true) {
                        if (vieModel.ScanTasks.All(arg =>
                         arg.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                         arg.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)) {
                            vieModel.ScanStatus = "Complete";
                            CheckingScanStatus = false;
                            break;
                        } else {
                            Task.Delay(1000).Wait();
                        }
                    }
                });
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
            dragOverBorder.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            dragOverBorder.Visibility = Visibility.Collapsed;
        }

        public void ShowSettingsPopup(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Top;
                contextMenu.IsOpen = true;
            }

            e.Handled = true;
        }

        private void ClearRecentWatched(object sender, RoutedEventArgs e)
        {
            SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("DBId", ConfigManager.Main.CurrentDBId).Eq("DataType", "0");
            metaDataMapper.UpdateField("ViewDate", string.Empty, wrapper);
            vieModel.Statistic();
        }

        private void ConfigFirstRun()
        {
            if (ConfigManager.Main.FirstRun) {
                vieModel.ShowFirstRun = Visibility.Visible;
                ConfigManager.Main.FirstRun = false;
            }
            this.ButtonSideTop.Visibility = Visibility.Visible;
        }
        // todo
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F) {
            //    // 高级检索
            //} else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right) {
            //    // 末页
            //    if (vieModel.TabSelectedIndex == 0) {
            //        vieModel.CurrentPage = vieModel.TotalPage;

            //        // vieModel.AsyncFlipOver();
            //        SetSelected();
            //    }
            //} else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left) {
            //    // 首页
            //    if (vieModel.TabSelectedIndex == 0) {
            //        vieModel.CurrentPage = 1;

            //        // vieModel.AsyncFlipOver();
            //        SetSelected();
            //    }
            //} else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Up) {
            //    // 回到顶部
            //    // ScrollViewer.ScrollToTop();
            //} else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Down) {
            //    // 滑倒底端
            //}

            // else if (vieModel.TabSelectedIndex == 0 && e.Key == Key.Right)
            //    NextPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            // else if (vieModel.TabSelectedIndex == 0 && e.Key == Key.Left)
            //    PreviousPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            // else if (vieModel.TabSelectedIndex == 1 && e.Key == Key.Right)
            //    NextActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            // else if (vieModel.TabSelectedIndex == 1 && e.Key == Key.Left)
            //    PreviousActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
        }

        // todo DatabaseComboBox_SelectionChanged
        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (!comboBox.IsLoaded)
                return;

            if (e.AddedItems.Count == 0 || vieModel.CurrentAppDataBase == null)
                return;

            // AppDatabase database =
            vieModel.CurrentAppDataBase = (AppDatabase)e.AddedItems[0];
            ConfigManager.Main.CurrentDBId = vieModel.CurrentAppDataBase.DBId;
            ConfigManager.Settings.DefaultDBID = vieModel.CurrentAppDataBase.DBId;

            // 切换数据库
            vieModel.IsRefresh = true;
            vieModel.Statistic();
            //vieModel.Reset(); // todo tab
            vieModel.InitCurrentTagStamps();
            vieModel.SetClassify(true);

            // vieModel.InitLettersNavigation();
            // vieModel.GetFilterInfo();
            vieModel.MainDataChecked = true;
        }


        public void PlayVideoWithPlayer(string filepath, long dataID = 0)
        {
        }


        public void RefreshData(long dataID)
        {

        }

        public void RefreshImage(Video video)
        {

        }

        public void RefreshGrade(Video newVideo)
        {
        }


        public async void DeleteIDs(ObservableCollection<Video> originSource, List<Video> to_delete, bool fromDetailWindow = true)
        {

        }



    

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            // string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            // string file = dragdropFiles[0];

            // if (IsFile(file))
            // {
            //    FileInfo fileInfo = new FileInfo(file);
            //    if (fileInfo.Extension.ToLower() == ".jpg")
            //    {
            //        FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{vieModel.Actress.name}.jpg", true);
            //        Actress actress = vieModel.Actress;
            //        actress.smallimage = null;
            //        actress.smallimage = GetActorImage(actress.name);
            //        vieModel.Actress = null;
            //        vieModel.Actress = actress;

            // if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

            // for (int i = 0; i < vieModel.ActorList.Count; i++)
            //        {
            //            if (vieModel.ActorList[i].name == actress.name)
            //            {
            //                vieModel.ActorList[i] = actress;
            //                break;
            //            }
            //        }

            // }
            //    else
            //    {
            //        msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_OnlySupportJPG"));
            //    }
            // }
        }




        private void ClearActressInfo(object sender, RoutedEventArgs e)
        {
        }

        private void ClassifyTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vieModel.SetClassify();

        }

        private void HideBeginScanGrid(object sender, RoutedEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
        }


        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
        }

        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            try {
                scan_cts?.Cancel();
            } catch (ObjectDisposedException ex) {
                Logger.Error(ex);
            }
        }


        private void CopyText(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ClipBoard.TrySetDataObject(textBlock.Text);
        }

        private void InitLoadSearch(string notice)
        {

        }

        private void LoadActorOtherMovie(object sender, MouseButtonEventArgs e)
        {

        }









        public void RefreshTagStamps(long id)
        {
            // todo tab
        }

        private void EditTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string tag = (contextMenu.PlacementTarget as PathCheckButton).Tag.ToString();
            long.TryParse(tag, out long id);
            if (id <= 0)
                return;

            TagStamp tagStamp = Main.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();
            Window_TagStamp window_TagStamp = new Window_TagStamp(tagStamp.TagName, tagStamp.BackgroundBrush, tagStamp.ForegroundBrush);
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog) {
                string name = window_TagStamp.TagName;
                if (string.IsNullOrEmpty(name))
                    return;
                SolidColorBrush backgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;
                tagStamp.TagName = name;
                tagStamp.Background = VisualHelper.SerializeBrush(backgroundBrush);
                tagStamp.Foreground = VisualHelper.SerializeBrush(ForegroundBrush);
                tagStampMapper.UpdateById(tagStamp);
                InitTagStamp();
                RefreshTagStamps(id);// 刷新标记
            }
        }

        public void onDeleteTagStamps()
        {
            // 更新主窗体
            //if (vieModel.CurrentVideoList != null) {
            //    for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
            //        if (vieModel.CurrentVideoList[i].TagStamp != null
            //            && vieModel.CurrentVideoList[i].TagStamp.Contains(tagStamp)) {
            //            vieModel.CurrentVideoList[i].TagStamp.Remove(tagStamp);
            //        }
            //    }
            //}

        }

        private void DeleteTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string tag = (contextMenu.PlacementTarget as PathCheckButton).Tag.ToString();
            long.TryParse(tag, out long id);
            if (id <= 0)
                return;
            TagStamp tagStamp = Main.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();
            if (tagStamp.IsSystemTag()) {
                MessageNotify.Error(LangManager.GetValueByKey("CanNotDeleteDefaultTag"));
                return;
            }


            if (new MsgBox(SuperControls.Style.LangManager.GetValueByKey("IsToDelete") + $"{LangManager.GetValueByKey("TagStamp")} 【{tagStamp.TagName}】").ShowDialog() == true) {
                tagStampMapper.DeleteById(id);

                // 删除
                string sql = $"delete from metadata_to_tagstamp where TagID={tagStamp.TagID};";
                tagStampMapper.ExecuteNonQuery(sql);
                InitTagStamp();
                onDeleteTagStamps();


                // todo 更新详情窗口
            }
        }

        private void NewList(object sender, MouseButtonEventArgs e)
        {
        }






        private async void doSearch(object sender, RoutedEventArgs e)
        {
            SearchMode mode = (SearchMode)vieModel.TabSelectedIndex;

            if (vieModel.TabSelectedIndex == 0) {
                vieModel.Searching = true;
                ConfigManager.Main.SearchSelectedIndex = searchTabControl.SelectedIndex;
                //await vieModel.Query((SearchField)searchTabControl.SelectedIndex);
                SaveSearchHistory(mode,
                (SearchField)searchTabControl.SelectedIndex);
            } else if (vieModel.TabSelectedIndex == 1) {
                // 搜寻演员
                //vieModel.SearchingActor = true;
                //vieModel.SelectActor();
                //SaveSearchHistory(mode, 0);
            } else if (vieModel.TabSelectedIndex == 2) {
                // 搜寻标签
                vieModel.GetLabelList();
                SaveSearchHistory(mode, 0);
            } else if (vieModel.TabSelectedIndex == 3) {
                // 搜寻分类
                vieModel.SetClassify(true);
                SaveSearchHistory(mode, (SearchField)vieModel.ClassifySelectedIndex);
            }

            vieModel.Searching = false;
        }

        private void SaveSearchHistory(SearchMode mode, SearchField field)
        {
            string searchValue = vieModel.SearchText.ToProperSql();
            if (string.IsNullOrEmpty(searchValue))
                return;
            SearchHistory history = new SearchHistory() {
                SearchMode = mode,
                SearchValue = searchValue,
                CreateDate = DateHelper.Now(),
                SearchField = field,
                CreateYear = DateTime.Now.Year,
                CreateMonth = DateTime.Now.Month,
                CreateDay = DateTime.Now.Day,
            };
            searchHistoryMapper.Insert(history);
        }

        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab) {
                (searchTabControl.Items[(int)ConfigManager.Main.SearchSelectedIndex] as TabItem).Focus();
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            vieModel.SearchText = (sender as ListBoxItem).Content.ToString();
            doSearch(null, null);
        }


        private void PathCheckButton_Click(object sender, RoutedEventArgs e)
        {
            // todo tab 获得当前所有标记状态
            //vieModel.LoadData();
        }

        private void SideBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SideGridColumn.ActualWidth <= 100 && !AnimatingSideGrid) {
                SideGridColumn.Width = new GridLength(0);
                if (ButtonSideTop != null)
                    ButtonSideTop.Visibility = Visibility.Visible;
            } else {
                if (ButtonSideTop != null)
                    ButtonSideTop.Visibility = Visibility.Collapsed;
            }
        }



        private void ShowMessage(object sender, MouseButtonEventArgs e)
        {
            msgPopup.IsOpen = true;
        }



        public void ShowSameActor(long actorID)
        {
            if (actorID <= 0)
                return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("actor_info.ActorID", actorID);
            //vieModel.ExtraWrapper = wrapper;
            vieModel.ClickFilterType = "Actor";
            //pagination.CurrentPageChange -= Pagination_CurrentPageChange;
            //vieModel.CurrentPage = 1;
            //vieModel.LoadData();
            //ActorInfo actorInfo = actorMapper.SelectOne(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            //ActorInfo.SetImage(ref actorInfo);
            //vieModel.ShowActorGrid = true;
            //pagination.CurrentPageChange += Pagination_CurrentPageChange;
            //ActorNavigator.Navigate("/Pages/ActorInfoPage.xaml", actorInfo);
            //vieModel.StatusText = actorInfo.ActorName;
        }


        private void ShowMsgScanPopup(object sender, MouseButtonEventArgs e)
        {
            scanStatusPopup.IsOpen = true;
        }




        private void CancelScanTask(object sender, RoutedEventArgs e)
        {
            string createTime = (sender as Button).Tag.ToString();
            ScanTask scanTask = vieModel.ScanTasks.Where(arg => arg.CreateTime.Equals(createTime)).FirstOrDefault();
            scanTask?.Cancel();
        }

        private void CancelDownloadTask(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            task?.Cancel();
        }

        private void CancelScreenShotTask(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            ScreenShotTask task = vieModel.ScreenShotTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            task?.Cancel();
        }

        private void CancelDownloadTasks(object sender, RoutedEventArgs e)
        {
            foreach (DownLoadTask task in vieModel.DownLoadTasks) {
                task.Cancel();
            }
        }

        private void CancelScreenShotTasks(object sender, RoutedEventArgs e)
        {
            foreach (ScreenShotTask task in vieModel.ScreenShotTasks) {
                task.Cancel();
            }
        }

        private void PauseDownloadTask(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            if (button.Content.ToString() == LangManager.GetValueByKey("Pause")) {
                button.Content = LangManager.GetValueByKey("Continue");
                task.Pause();
            } else {
                button.Content = LangManager.GetValueByKey("Pause");
            }
        }

        private void ShowScanDetail(object sender, RoutedEventArgs e)
        {
            string createTime = (sender as Button).Tag.ToString();
            ScanTask scanTask = vieModel.ScanTasks.Where(arg => arg.CreateTime.Equals(createTime)).FirstOrDefault();
            if (scanTask?.Status != System.Threading.Tasks.TaskStatus.Running) {
                Window_ScanDetail scanDetail = new Window_ScanDetail(scanTask.ScanResult);
                scanDetail.Show();
            }
        }

        private void ShowDownloadDetail(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            if (task == null)
                return;
            new Dialog_Logs(string.Join(Environment.NewLine, task.Logs)).ShowDialog(this);
        }

        private void ShowScreenShotDetail(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            ScreenShotTask task = vieModel.ScreenShotTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            if (task == null)
                return;
            new Dialog_Logs(string.Join(Environment.NewLine, task.Logs)).ShowDialog(this);
        }




        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            downloadStatusPopup.StaysOpen = true;
            screenShotStatusPopup.StaysOpen = true;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void RemoveCompleteTask(object sender, RoutedEventArgs e)
        {
            downloadStatusPopup.StaysOpen = false;
            for (int i = vieModel.DownLoadTasks.Count - 1; i >= 0; i--) {
                if (vieModel.DownLoadTasks[i].Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                    vieModel.DownLoadTasks.RemoveAt(i);
                }
            }

            Global.DownloadManager.Dispatcher.ClearDoneList();
            if (vieModel.DownLoadTasks.Count == 0)
                vieModel.DownLoadVisibility = Visibility.Collapsed;
        }

        private void RemoveCancelTask(object sender, RoutedEventArgs e)
        {
            downloadStatusPopup.StaysOpen = false;
            for (int i = vieModel.DownLoadTasks.Count - 1; i >= 0; i--) {
                if (vieModel.DownLoadTasks[i].Status == System.Threading.Tasks.TaskStatus.Canceled) {
                    vieModel.DownLoadTasks.RemoveAt(i);
                }
            }

            Global.DownloadManager.Dispatcher.ClearDoneList();
            if (vieModel.DownLoadTasks.Count == 0)
                vieModel.DownLoadVisibility = Visibility.Collapsed;
        }

        private void RemoveCompleteScreenShot(object sender, RoutedEventArgs e)
        {
            screenShotStatusPopup.StaysOpen = false;
            for (int i = vieModel.ScreenShotTasks.Count - 1; i >= 0; i--) {
                if (vieModel.ScreenShotTasks[i].Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                    vieModel.ScreenShotTasks.RemoveAt(i);
                }
            }

            Global.DownloadManager.Dispatcher.ClearDoneList();
            if (vieModel.ScreenShotTasks.Count == 0)
                vieModel.ScreenShotVisibility = Visibility.Collapsed;
        }

        private void RemoveCancelScreenShot(object sender, RoutedEventArgs e)
        {
            screenShotStatusPopup.StaysOpen = false;
            for (int i = vieModel.ScreenShotTasks.Count - 1; i >= 0; i--) {
                if (vieModel.ScreenShotTasks[i].Status == System.Threading.Tasks.TaskStatus.Canceled) {
                    vieModel.ScreenShotTasks.RemoveAt(i);
                }
            }

            Global.DownloadManager.Dispatcher.ClearDoneList();
            if (vieModel.ScreenShotTasks.Count == 0)
                vieModel.ScreenShotVisibility = Visibility.Collapsed;
        }


        private void OpenImageSavePath(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (!ConfigManager.Settings.PicPaths.ContainsKey(pathType.ToString()))
                return;
            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType == PathType.RelativeToApp)
                basePicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePicPath);
            basePicPath = Path.GetFullPath(basePicPath);
            FileHelper.TryOpenPath(basePicPath);
        }

        private void OpenLogPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));
        }

        private void OpenApplicationPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory);
        }



        private void ShowActorNotice(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType.Equals(PathType.RelativeToData))
                msgCard.Info(LangManager.GetValueByKey("ShowActorImageWarning"));
        }

        private void HideMsg(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Border border = (button.Parent as Grid).Parent as Border;
            border.Visibility = Visibility.Collapsed;
        }



        private void ImportVideo(object sender, RoutedEventArgs e)
        {
            string path = FileHelper.SelectPath(this);
            if (!string.IsNullOrEmpty(path)) {
                AddScanTask(new string[] { path });
                MessageNotify.Success($"{LangManager.GetValueByKey("AddScanTaskSuccess")} => " + path);
            }
        }

        private void ImportVideoByPaths(object sender, RoutedEventArgs e)
        {
            Window_SelectPaths window_SelectPaths = new Window_SelectPaths();
            if (window_SelectPaths.ShowDialog() == true) {
                List<string> folders = window_SelectPaths.Folders;
                if (folders.Count == 0) {
                    MessageNotify.Warning(LangManager.GetValueByKey("PathNotSelect"));
                } else {
                    AddScanTask(folders.ToArray());
                    // MessageCard.Success($"已添加 {folders.Count} 个文件夹到扫描任务!");
                }
            }
        }


        private void ShowStatistic(object sender, RoutedEventArgs e)
        {
        }


        public ObservableCollection<Video> GetVideosFromTagMenu(MenuItem menuItem)
        {
            if (menuItem == null)
                return null;

            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null || contextMenu.PlacementTarget == null)
                return null;

            Border border = contextMenu.PlacementTarget as Border;
            if (border == null)
                return null;

            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(border);
            if (itemsControl == null)
                return null;


            ItemsControl control = VisualHelper.FindParentOfType<ItemsControl>(itemsControl);
            if (control == null)
                return null;



            return control.ItemsSource as ObservableCollection<Video>;
        }



        private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Border border = (menuItem.Parent as ContextMenu).PlacementTarget as Border;
            long.TryParse(border.Tag.ToString(), out long tagID);
            if (tagID <= 0)
                return;

            ItemsControl itemsControl = border.FindParentOfType<ItemsControl>();
            if (itemsControl == null || itemsControl.Tag == null)
                return;
            long.TryParse(itemsControl.Tag.ToString(), out long DataID);
            if (DataID <= 0)
                return;
            ObservableCollection<TagStamp> tagStamps = itemsControl.ItemsSource as ObservableCollection<TagStamp>;
            if (tagStamps == null)
                return;
            TagStamp tagStamp = tagStamps.FirstOrDefault(arg => arg.TagID.Equals(tagID));
            if (tagStamp != null) {


                ObservableCollection<Video> datas = GetVideosFromTagMenu(sender as MenuItem);

                tagStamps.Remove(tagStamp);
                string sql = $"delete from metadata_to_tagstamp where TagID='{tagID}' and DataID='{DataID}'";
                tagStampMapper.ExecuteNonQuery(sql);

                for (int i = 0; i < datas.Count; i++) {
                    if (datas[i].DataID.Equals(DataID)) {
                        Video video = videoMapper.SelectVideoByID(DataID);
                        if (video == null)
                            continue;
                        datas[i].TagIDs = video.TagIDs;
                        break;
                    }
                }

                InitTagStamp();
            }
        }

        private void CopyVID(object sender, MouseButtonEventArgs e)
        {
            string text = (sender as TextBlock).Text;
            if (string.IsNullOrEmpty(text))
                return;
            ClipBoard.TrySetDataObject(text);
            MessageNotify.Success($"已复制：{text}");
        }



        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void HideAssocPopup(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
        }



        private void RestartTask(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            if (string.IsNullOrEmpty(dataID))
                return;
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            task?.Restart();
            if (!Global.DownloadManager.Dispatcher.Working)
                Global.DownloadManager.Dispatcher.BeginWork();
        }

        private void ShowSponsor(object sender, RoutedEventArgs e)
        {
            // 检测
            string message = "请设置一个刮削网址后在尝试";
            if (ConfigManager.ServerConfig.CrawlerServers != null &&
                ConfigManager.ServerConfig.CrawlerServers.Count > 0) {
                bool found = false;

                foreach (var item in ConfigManager.ServerConfig.CrawlerServers) {
                    if (!string.IsNullOrEmpty(item.Url)) {
                        found = true;
                        break;
                    }
                }

                if (found) {
                    //new Dialog_Sponsor(this).ShowDialog();
                    return;
                }
            }

            MessageNotify.Info(message);
        }


        private void CopyTextBlock(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            TextBlock textBlock = contextMenu.PlacementTarget as TextBlock;
            if (textBlock != null) {
                string txt = textBlock.Text;
                if (!string.IsNullOrEmpty(txt))
                    ClipBoard.TrySetDataObject(txt);
            }
        }

        private void SetTagStampsSelected(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            bool allChecked = (bool)toggleButton.IsChecked;
            ItemsControl itemsControl = TagStampItemsControl;
            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null)
                    continue;
                PathCheckButton button = FindElementByName<PathCheckButton>(presenter, "pathCheckButton");
                if (button == null)
                    continue;
                button.IsChecked = allChecked;
            }

            // todo tab
            //vieModel.LoadData();
        }






        private void ShowUpgradeWindow(object sender, RoutedEventArgs e)
        {
            UpgradeHelper.OpenWindow();
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            Dialog_About about = new Dialog_About();
            string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (local.EndsWith(".0.0"))
                local = local.Substring(0, local.Length - ".0.0".Length);
            about.AppName = "Jvedio";
            about.AppSubName = "本地视频管理软件";
            about.Version = local;
            about.ReleaseDate = ConfigManager.RELEASE_DATE;
            about.Author = "Chao";
            about.License = "GPL-3.0";
            about.GithubUrl = UrlManager.ProjectUrl;
            about.WebUrl = UrlManager.WebPage;
            about.JoinGroupUrl = UrlManager.ProjectUrl;
            about.Image = SuperUtils.Media.ImageHelper.ImageFromUri("pack://application:,,,/Resources/Picture/Jvedio.png");
            about.ShowDialog();
        }

        private void MessageCard_Close(object sender, EventArgs e)
        {
            MessageCard messageCard = sender as MessageCard;
            string Date = messageCard.Tag.ToString();
            List<Message> messages = vieModel.Message.Where(arg => arg.Date.Equals(Date)).ToList();
            vieModel.Message.RemoveMany(messages);
        }

        private void OpenScanPath(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag != null) {
                FileHelper.TryOpenPath(menuItem.Tag.ToString());
            }
        }

        SuperControls.Style.Plugin.Window_Plugin window_Plugin;

        private void ShowPluginWindow(object sender, RoutedEventArgs e)
        {
            if (window_Plugin == null || window_Plugin.IsClosed) {
                SuperControls.Style.Plugin.PluginConfig config = new SuperControls.Style.Plugin.PluginConfig();
                config.PluginBaseDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
                config.RemoteUrl = UrlManager.GetPluginUrl();
                window_Plugin = new SuperControls.Style.Plugin.Window_Plugin();
                window_Plugin.SetConfig(config);

                window_Plugin.OnEnabledChange += (data, enabled) => {
                    return true;
                };

                window_Plugin.OnBeginDelete += (PluginMetaData data) => {
                    PluginType pluginType = data.PluginType;

                    if (pluginType == PluginType.Crawler) {
                        List<string> list = JsonUtils.TryDeserializeObject<List<string>>(ConfigManager.PluginConfig.DeleteList);
                        if (list == null)
                            list = new List<string>();
                        if (!list.Contains(data.PluginID)) {
                            list.Add(data.PluginID);
                            ConfigManager.PluginConfig.DeleteList = JsonUtils.TrySerializeObject(list);
                            ConfigManager.PluginConfig.Save();
                            MessageNotify.Info("已加入待删除列表，重启后生效");
                        } else {
                            MessageNotify.Warning("已经在待删除列表里");
                        }
                    }
                    return false;
                };
                window_Plugin.OnDeleteCompleted += (data) => {
                    PluginType pluginType = data.PluginType;
                    if (pluginType == PluginType.Theme) {
                        ThemeSelectorDefault.InitThemes();
                    } else if (pluginType == PluginType.Crawler) {
                        CrawlerManager.Init(false);
                    }
                };

                window_Plugin.OnBeginDownload += (data) => {
                    return true;
                };
                window_Plugin.OnDownloadCompleted += (data) => {
                    // 根据类型，通知到对应模块
                    PluginType pluginType = data.PluginType;
                    if (pluginType == PluginType.Theme) {
                        ThemeSelectorDefault.InitThemes();
                    } else if (pluginType == PluginType.Crawler) {
                        // 如果是新下载，则可以直接使用
                        // 如果是更新，则需要重启
                        CrawlerManager.Init(false);
                    }
                };
            }
            window_Plugin.Show();
            window_Plugin.BringIntoView();
            window_Plugin.Focus();
            window_Plugin.Activate();
        }

        private void ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            Window_Settings window_Settings = new Window_Settings();
            window_Settings.Show();
        }



        private void ManageDataBase(object sender, RoutedEventArgs e)
        {
            Window_DataBase dataBase = new Window_DataBase();
            dataBase.Owner = this;

            dataBase.OnDataChanged += () => {
                vieModel.Statistic();
                //vieModel.LoadData(); // todo tab
            };

            dataBase.ShowDialog();
        }

        public bool IsTaskRunning()
        {
            return (vieModel.DownLoadTasks?.Count > 0 || vieModel.ScanTasks?.Count > 0 || vieModel.ScreenShotTasks?.Count > 0);
        }


        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
        }

        private Window_Server window_Server;

        private void OpenServer(object sender, RoutedEventArgs e)
        {
            MessageNotify.Info("开发中");
            return;

            if (window_Server != null)
                window_Server.Close();
            window_Server = new Window_Server();

            window_Server.OnServerStatusChanged += (status) => {
                vieModel.ServerStatus = status;
            };

            window_Server.Owner = this;
            window_Server.ShowDialog();

        }

        private void OpenServerWindow(object sender, MouseButtonEventArgs e)
        {
            OpenServer(null, null);
        }

        private void GoToStartUp(object sender, RoutedEventArgs e)
        {
            Main.ClickGoBackToStartUp = true;
            SetWindowVisualStatus(false); // 隐藏所有窗体
            WindowStartUp windowStartUp = GetWindowByName("WindowStartUp", App.Current.Windows) as WindowStartUp;
            if (windowStartUp == null)
                windowStartUp = new WindowStartUp();
            Application.Current.MainWindow = windowStartUp;
            windowStartUp.Show();
        }

        private void NewTagStamp(object sender, RoutedEventArgs e)
        {
            Window_TagStamp window_TagStamp = new Window_TagStamp();
            window_TagStamp.Owner = this;
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog) {
                string name = window_TagStamp.TagName;
                if (string.IsNullOrEmpty(name))
                    return;
                SolidColorBrush backgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;

                TagStamp tagStamp = new TagStamp() {
                    TagName = name,
                    Foreground = VisualHelper.SerializeBrush(ForegroundBrush),
                    Background = VisualHelper.SerializeBrush(backgroundBrush),
                };
                tagStampMapper.Insert(tagStamp);
                InitTagStamp();

            }
        }


        private void ShowFilterGrid(object sender, RoutedEventArgs e)
        {
            MessageNotify.Info("开发中");
            //if (windowFilter == null) windowFilter = new Window_Filter();
            //windowFilter.Show();
            //windowFilter.BringIntoView();
            //windowFilter.Activate();
        }

        private void ClearScanTasks(object sender, RoutedEventArgs e)
        {
            for (int i = vieModel.ScanTasks.Count - 1; i >= 0; i--) {
                Core.Scan.ScanTask scanTask = vieModel.ScanTasks[i];
                if (scanTask.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                    scanTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                    vieModel.ScanTasks.RemoveAt(i);
                }
            }

            vieModel.ScanStatus = "None";
        }

        private void ClearMsg(object sender, RoutedEventArgs e)
        {
            vieModel.Message.Clear();
        }

        private void HideScanPopup(object sender, RoutedEventArgs e)
        {
            scanStatusPopup.IsOpen = false;
        }

        private void HideDownloadPopup(object sender, RoutedEventArgs e)
        {
            downloadStatusPopup.IsOpen = false;
            downloadStatusPopup.StaysOpen = false;
        }

        private void HideMsgPopup(object sender, RoutedEventArgs e)
        {
            msgPopup.IsOpen = false;
        }

        private void HideScreenShotPopup(object sender, RoutedEventArgs e)
        {
            screenShotStatusPopup.IsOpen = false;
            screenShotStatusPopup.StaysOpen = false;
        }

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            ImageCache.Clear();
        }

        private void PortTab_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }


        private void RemoveTabItem(int idx)
        {
            if (vieModel.TabItems == null)
                return;
            if (idx >= 0 && idx < vieModel.TabItems.Count) {
                vieModel.TabItems[idx].Pinned = false;
                vieModel.TabItems.RemoveAt(idx);
            }
            // 默认选中左边的
            int selectIndex = idx - 1;
            if (selectIndex < 0)
                selectIndex = 0;

            if (vieModel.TabItems.Count > 0)
                vieModel.TabItemManager?.SetTabSelected(selectIndex);
        }

        private void SetGridVisible(string portName)
        {
            Logger.Debug(portName);
            if (string.IsNullOrEmpty(portName))
                return;

            ItemsControl itemsControl = null;

            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null) {
                    Logger.Debug($"presenter[{i}] is null");
                    continue;
                }
                Grid grid = VisualHelper.FindElementByName<Grid>(presenter, "baseGrid");
                if (grid == null || grid.Tag == null) {
                    Logger.Debug($"presenter[{i}] baseGrid is null");
                    continue;
                }

                string name = grid.Tag.ToString();
                if (portName.Equals(name))
                    grid.Visibility = Visibility.Visible;
                else
                    grid.Visibility = Visibility.Hidden;
            }
        }



        private int GetTabIndexByMenuItem(object sender)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu != null &&
                contextMenu.PlacementTarget is Border border &&
                border.Tag != null &&
                int.TryParse(border.Tag.ToString(), out int index)) {
                return index;
            }
            return -1;
        }

        private void CloseCurrentTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            RemoveTabItem(idx);
        }


        private void CloseOtherTab(object sender, RoutedEventArgs e)
        {
            if (vieModel == null ||
                vieModel.TabItems == null ||
                vieModel.TabItems.Count <= 1)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;

            vieModel.TabItemManager?.RemoveRange(idx + 1, vieModel.TabItems.Count - 1);
            vieModel.TabItemManager?.RemoveRange(0, idx - 1);
        }

        private void CloseAllLeftTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx <= 0)
                return;
            vieModel.TabItemManager?.RemoveRange(0, idx - 1);
        }

        private void CloseAllRightTab(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            vieModel.TabItemManager?.RemoveRange(idx + 1, vieModel.TabItems.Count - 1);
        }



        private void CloseAllTabs(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;
            vieModel.TabItemManager?.RemoveRange(0, vieModel.TabItems.Count - 1);
        }

        private void MoveToFirst(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            vieModel.TabItemManager.MoveToFirst(idx);

        }

        private void MoveToLast(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            vieModel.TabItemManager?.MoveToLast(idx);
        }




        private void PinTab(object sender, RoutedEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            vieModel.TabItemManager?.PinByIndex(idx);
        }

        private void PinTab(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            if (sender is FrameworkElement ele && ele.Tag != null &&
                int.TryParse(ele.Tag.ToString(), out int index) &&
                index >= 0)
                vieModel.TabItemManager?.PinByIndex(index);
        }

        private void CloseTabItem(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele != null && ele.Tag != null && int.TryParse(ele.Tag.ToString(), out int index) &&
                index >= 0) {
                RemoveTabItem(index);
            }

        }

        private bool CanDragTabItem = false;

        private FrameworkElement CurrentDragElement;
        private void BeginDragTabItem(object sender, MouseButtonEventArgs e)
        {
            CanDragTabItem = true;
            CurrentDragElement = sender as FrameworkElement;
            Mouse.Capture(CurrentDragElement, CaptureMode.Element);

            Border border = (Border)sender;
            if (border == null || border.Tag == null)
                return;
            string idxStr = border.Tag.ToString();
            if (string.IsNullOrEmpty(idxStr) || vieModel.TabItems == null ||
                vieModel.TabItems.Count <= 0)
                return;
            int.TryParse(idxStr, out int index);
            if (index >= 0) {
                vieModel.TabItemManager?.SetTabSelected(index);
            }
        }

        private void SetTabSelected(object sender, MouseButtonEventArgs e)
        {
            CanDragTabItem = false;
            if (CurrentDragElement != null)
                Mouse.Capture(CurrentDragElement, CaptureMode.None);
        }

        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!CanDragTabItem)
                return;
        }
    }

}
