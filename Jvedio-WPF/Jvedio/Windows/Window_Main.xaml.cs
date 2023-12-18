using Jvedio.AvalonEdit;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Core.Server;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Upgrade;
using Jvedio.ViewModel;
using Jvedio.ViewModels;
using SuperControls.Style;
using SuperControls.Style.CSFile.Interfaces;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Systems;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.Global.UrlManager;
using static Jvedio.MapperManager;
using static Jvedio.Window_Settings;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : BaseWindow, IBaseWindow
    {

        #region "静态属性"

        public static List<string> ClickFilterDict { get; set; }

        /// <summary>
        /// 如果包含以下文本，则显示对应的标签戳
        /// </summary>
        public static string[] TagStringHD { get; set; }

        public static string[] TagStringTranslated { get; set; }


        public static DataBaseType CurrentDataBaseType { get; set; }

        public static bool ClickGoBackToStartUp { get; set; }// 是否是点击了返回去到 Startup

        public static DataType CurrentDataType { get; set; }

        public static string[] TransParentBackGround { get; set; } = new string[] {
            "TabItem.Background",
            "ListBoxItem.Background",
            "Window.Side.Background",
            "Window.Side.Hover.Background",
            "DataGrid.Row.Even.Background",
            "DataGrid.Row.Odd.Background",
            "DataGrid.Row.Hover.Background",
            "DataGrid.Header.Background",
        };


        #endregion

        #region "属性"

        private Window_Server window_Server { get; set; }
        private bool CanDragTabItem { get; set; } = false;

        private FrameworkElement CurrentDragElement { get; set; }

        private Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager TaskbarInstance { get; set; }

        private SuperControls.Style.Plugin.Window_Plugin window_Plugin { get; set; }


        private Window_Details windowDetails { get; set; }

        private bool AnimatingSideGrid { get; set; } = false;

        public VieModel_Main vieModel { get; set; }


        #endregion

        static Main()
        {
            StaticInit();
        }

        static void StaticInit()
        {
            ClickFilterDict = new List<string>() { "Genre", "Series", "Studio", "Director", };
            TagStringHD = new string[] { "hd", "高清" };
            TagStringTranslated = new string[] { "中文", "日本語", "Translated", "English" };
            CurrentDataBaseType = DataBaseType.SQLite;
            ClickGoBackToStartUp = false;
            CurrentDataType = DataType.Video;
        }

        public Main()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            InitContext();
            BindingEvent();
            LoadNotifyIcon();
        }

        public void InitContext()
        {
            vieModel = new VieModel_Main(this);
            this.DataContext = vieModel;
        }

        public void Dispose()
        {
            SaveConfigValue();
            DownloadManager.CancelAll();
            ScreenShotManager.CancelAll();
            ScanManager.CancelAll();
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ConfigFirstRun();
            InitTheme();
            InitNotice();
            InitDataBases();
            BindingEventAfterRender();
            InitUpgrade();
            CheckServerStatus();
            InitAvalonEdit();
            InitSideMenu();
        }

        public void InitSideMenu()
        {
            if (Main.CurrentDataType != DataType.Video) {
                sideMenuContainer.Child = null;
            }
            // 初始化绑定关系
            switch (Main.CurrentDataType) {
                case DataType.Video:
                    VideoSideMenu sideMenu = sideMenuContainer.Child as VideoSideMenu;
                    sideMenu.onSideButtonCmd = vieModel.HandleSideButtonCmd;
                    vieModel.SetSideMenu(sideMenu);
                    break;
                case DataType.Picture:
                    PictureSideMenu pictureSideMenu = new PictureSideMenu();
                    pictureSideMenu.onSideButtonCmd = vieModel.HandleSideButtonCmd;
                    sideMenuContainer.Child = pictureSideMenu;
                    break;
                case DataType.Game:
                    break;
                case DataType.Comics:
                    break;
                default:
                    break;
            }

            vieModel.Statistic();
        }



        public void InitAvalonEdit()
        {
            AvalonEditManager.Init();
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        public void LoadAll()
        {
            vieModel.LoadAll(); // 加载数据
        }

        public void LoadNotifyIcon()
        {
            SetNotiIconPopup(notiIconPopup);
            this.OnNotifyIconMouseLeftClick += (s, e) => {
                ShowMainWindow(s, new RoutedEventArgs());
            };
        }

        public async void CheckServerStatus()
        {
            vieModel.ServerStatus = await ServerManager.CheckStatus();
        }

        public void SetAllSelect()
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            List<ActorList> actorLists = vieModel.TabItemManager.GetAllActorList();
            if (videoLists != null) {
                foreach (VideoList video in videoLists) {
                    video.SetSelected();
                }
            }
        }


        public void InitTheme()
        {
            foreach (var item in TransParentBackGround) {
                ThemeSelectorDefault.AddTransParentColor(item);
            }
            ThemeSelectorDefault.SetThemeConfig(ConfigManager.ThemeConfig.ThemeIndex,
                ConfigManager.ThemeConfig.ThemeID);
            ThemeSelectorDefault.onThemeChanged += (ThemeIdx, ThemeID) => {
                ConfigManager.ThemeConfig.ThemeIndex = ThemeIdx;
                ConfigManager.ThemeConfig.ThemeID = ThemeID;
                ConfigManager.ThemeConfig.Save();
                SetAllSelect();
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

        private void BindingEventAfterRender()
        {
            SetComboboxID();
            App.DownloadManager.onRunning += onDownloading;
            App.DownloadManager.onLongDelay += onLoadDelay;
            Main.OnRecvWinMsg += onRecvWinMsg;
            Video.onPlayVideo += () => vieModel.Statistic();
            ActorList.onStatistic += () => vieModel.Statistic();
            VideoList.onDeleteID += (list) => DeleteID(list, false);
            Window_EditActor.onActorInfoChanged += onActorInfoChanged;
            Window_Edit.onRefreshData += (id) => vieModel.Statistic();
            TabItemManager.OnFocusItem += OnFocusItem;
        }

        private void OnFocusItem(TabItemEx tabItem)
        {
            var container = tabItemsControl.ItemContainerGenerator.ContainerFromItem(tabItem) as FrameworkElement;
            if (container != null)
                container.BringIntoView();
        }

        private void onRecvWinMsg(string str)
        {
            Logger.Info($"recv win msg: {str}");
            switch (str) {
                case Win32Helper.WIN_CUSTOM_MSG_OPEN_WINDOW:
                    ShowMainWindow(null, null);
                    break;
                default:
                    break;
            }
        }

        private void onActorInfoChanged(long id)
        {
            vieModel.TabItemManager.GetAllActorList().ForEach(arg => {
                arg.RefreshActor(id);
            });

            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            wrapper.Eq("ActorID", id);
            ActorInfo actorInfo = actorMapper.SelectById(wrapper);
            if (actorInfo != null) {
                ActorInfo.SetImage(ref actorInfo);

                vieModel.TabItemManager.GetAllVideoList().ForEach(arg => {
                    if (arg.actorInfoView.CurrentActorInfo != null) {
                        arg.actorInfoView.CurrentActorInfo = null;
                        arg.actorInfoView.CurrentActorInfo = actorInfo;
                        arg.actorInfoView.CurrentActorInfo.SmallImage = actorInfo.SmallImage;
                    }

                });
            }
        }

        private void onLoadDelay(object sender, EventArgs e)
        {
            string message = (e as MessageCallBackEventArgs).Message;
            int.TryParse(message, out int value);
        }

        private void onDownloading()
        {
            double progress = App.DownloadManager.Progress;
            if (progress is double.NaN)
                progress = 0;
            vieModel.DownLoadProgress = progress;
            if (progress < 100)
                vieModel.DownLoadVisibility = Visibility.Visible;
            else
                vieModel.DownLoadVisibility = Visibility.Hidden;

            // 任务栏进度条
            Dispatcher.Invoke(() => {
                if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && TaskbarInstance != null) {
                    TaskbarInstance.SetProgressValue((int)progress, 100, this);
                    if (progress >= 100 || progress <= 0)
                        TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
                    else
                        TaskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
                }
            });
        }

        public void SetComboboxID()
        {
            vieModel.CurrentDbId =
                vieModel.DataBases.ToList().FindIndex(arg => arg.DBId == ConfigManager.Main.CurrentDBId);
        }


        /// <summary>
        /// 设置当前下拉数据库
        /// </summary>
        public void InitDataBases()
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



        #region "热键"

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Logger.Info("***************OnSourceInitialized***************");

            // 热键
            WindowHandle = new WindowInteropHelper(this).Handle;
            HSource = HwndSource.FromHwnd(WindowHandle);
            HSource.AddHook(HwndHook);

            // 注册热键
            uint modifier = (uint)ConfigManager.Settings.HotKeyModifiers;
            uint vk = (uint)ConfigManager.Settings.HotKeyVK;

            if (ConfigManager.Settings.HotKeyEnable && modifier != 0 && vk != 0) {
                Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
                bool success = Win32Helper.RegisterHotKey(WindowHandle, HOTKEY_ID, modifier, vk);
                Logger.Info($"register hot key, modifier: {modifier}, vk: {vk}, ret: {success}");
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
                            if (key == (uint)ConfigManager.Settings.HotKeyVK) {
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

        protected override void OnClosed(EventArgs e)
        {
            HSource.RemoveHook(HwndHook);
            Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消热键
            base.OnClosed(e);
        }

        #endregion

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindingEvent()
        {
            // 初始化任务栏的进度条
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
                TaskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;

            this.OnSideTrigger += onSideTrigger;
            Filter.onTagStampDelete += onTagStampDelete;
            Filter.onTagStampRefresh += RefreshTagStamp;
        }


        /// <summary>
        /// 侧边栏动画
        /// </summary>
        private async void onSideTrigger()
        {
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
        }

        public void RefreshTagStamp(long id)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            if (videoLists != null) {
                foreach (VideoList video in videoLists) {
                    video.RefreshTagStamps(id);
                }
            }
        }


        public void DeleteID(List<Video> list, bool fromDetail)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            if (videoLists != null) {
                foreach (VideoList video in videoLists) {
                    video.DeleteID(list.ToList(), fromDetail);
                }
            }
        }

        private void onTagStampDelete(long id)
        {
            // 删除主窗体所有标签戳
            VideoList videoList = vieModel.TabItemManager.GetVideoListByType(TabType.GeoVideo);
            if (videoList != null)
                videoList.RefreshTagStamps(id);

            // 删除详情窗口的标签戳
            Window window = GetWindowByName("Window_Details", App.Current.Windows);
            if (window != null && window is Window_Details window_Details)
                window_Details?.RemoveTag(id);
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
            this.Activate();
        }


        /// <summary>
        /// 初始化公告
        /// </summary>
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
        }

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

        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            // if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            // {
            //    DataBase.DeleteByField("movie", "filepath", e.FullPath);
            // }
            // Console.WriteLine("成功删除" + e.FullPath);
        }

        #endregion

        /// <summary>
        /// 还原窗口为上一次状态
        /// </summary>
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
            StopAllTask();
            App.Current.Shutdown();
        }

        /// <summary>
        /// 停止所有任务
        /// </summary>
        private void StopAllTask()
        {
            if (App.ScanManager.RunningCount > 0)
                App.ScanManager.CancelAll();

            if (App.ScreenShotManager.RunningCount > 0)
                App.ScreenShotManager.CancelAll();

            if (App.DownloadManager.RunningCount > 0)
                App.DownloadManager.CancelAll();

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

        private async void CheckUpgrade()
        {
            // 启动后检查更新
            try {
                await Task.Delay(UpgradeHelper.AUTO_CHECK_UPGRADE_DELAY);
                (string LatestVersion, string ReleaseDate, string ReleaseNote) result = await UpgradeHelper.GetUpgradeInfo();
                string remote = result.LatestVersion;
                string ReleaseDate = result.ReleaseDate;
                if (!string.IsNullOrEmpty(remote)) {
                    string local = App.GetLocalVersion(false);
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


        public void ShowSameString(string str, LabelType labelType)
        {
            vieModel.onLabelClick(str, labelType);
        }



        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            vieModel.TabItemManager
                .Add(Entity.Common.TabType.GeoTask, LangManager.GetValueByKey("Download"), TaskType.Download);
        }

        public void ShowScreenShotTab(object sender, MouseButtonEventArgs e)
        {
            vieModel.TabItemManager
                .Add(Entity.Common.TabType.GeoTask, LangManager.GetValueByKey("ScreenShotTask"), TaskType.ScreenShot);
        }

        private void ShowMsgScanPopup(object sender, MouseButtonEventArgs e)
        {
            vieModel.TabItemManager
                .Add(TabType.GeoTask, LangManager.GetValueByKey("Scan"), TaskType.Scan);
        }


        private void OnDragFileDrop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            vieModel.DragInFile = false;
            AddScanTask(dragdropFiles);
        }

        private void SaveConfigValue()
        {
            ConfigManager.Main.X = this.Left;
            ConfigManager.Main.Y = this.Top;
            ConfigManager.Main.Width = this.Width;
            ConfigManager.Main.Height = this.Height;
            ConfigManager.Main.SideGridWidth = SideGridColumn.ActualWidth;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }


        private void AddScanTask(string[] scanFileList)
        {
            List<string> files = new List<string>();
            List<string> paths = new List<string>();

            foreach (var item in scanFileList) {
                if (FileHelper.IsFile(item))
                    files.Add(item);
                else
                    paths.Add(item);
            }

            Core.Scan.ScanTask scanTask = new Core.Scan.ScanTask(paths, files);
            scanTask.Title = "-";

            scanTask.onCanceled += (s, ev) => Logger.Warn("cancel scan task");
            scanTask.onError += (s, ev) => MessageCard.Error((ev as MessageCallBackEventArgs).Message);
            scanTask.onCompleted += ScanComplete;
            App.ScanManager.AddTask(scanTask);

            scanTask.Start();
        }

        private void ScanComplete(object sender, EventArgs ev)
        {
            ScanTask scanTask = sender as ScanTask;
            if (scanTask != null && scanTask.Success) {

                Dispatcher.Invoke(() => {
                    vieModel.Statistic();
                    ScanResult scanResult = scanTask.ScanResult;
                    List<Video> insertVideos = null;
                    if (scanResult != null) {
                        insertVideos = scanResult.InsertVideos;
                        MessageCard.Info($"总数    {scanResult.TotalCount.ToString().PadRight(8)}已导入    {scanResult.Import.Count}{Environment.NewLine}" +
                            $"更新    {scanResult.Update.Count.ToString().PadRight(8)} 未导入    {scanResult.NotImport.Count}");
                    }

                    if (ConfigManager.ScanConfig.LoadDataAfterScan)
                        LoadAll();

                    if (ConfigManager.FFmpegConfig.ScreenShotAfterImport) {
                        ScreenShotAfterImport(insertVideos);
                    }
                });
            }
            scanTask.Running = false;
        }


        private void ScreenShotAfterImport(List<Video> import)
        {
            if (import != null &&
                import.Count > 0 &&
                File.Exists(ConfigManager.FFmpegConfig.Path)) {
                VideoList videoList = vieModel.TabItemManager.GetVideoListByType(TabType.GeoVideo);
                if (videoList != null)
                    videoList.GenerateScreenShot(import); // 让主 tab 去处理
            }
        }

        private void OnDragFileOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
            vieModel.DragInFile = true;

        }

        private void OnDragFileLeave(object sender, DragEventArgs e)
        {
            vieModel.DragInFile = false;
        }

        private void ConfigFirstRun()
        {
            if (ConfigManager.Main.FirstRun) {
                vieModel.ShowFirstRun = Visibility.Visible;
                ConfigManager.Main.FirstRun = false;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F) {
                SetTabItemStatus(TabActionType.Search);
                return;
            }

            if (vieModel.Searching) {
                return;
            }


            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Up) {
                SetTabItemStatus(TabActionType.GoToTop);
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Down) {
                SetTabItemStatus(TabActionType.GoToBottom);
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left) {
                SetTabItemStatus(TabActionType.FirstPage);
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right) {
                SetTabItemStatus(TabActionType.LastPage);
            } else if (e.Key == Key.Right) {
                SetTabItemStatus(TabActionType.NextPage);
            } else if (e.Key == Key.Left) {
                SetTabItemStatus(TabActionType.PreviousPage);
            }
        }

        private void SetTabItemStatus(TabActionType type)
        {
            ITabItemControl ele = vieModel.TabItemManager.GetSelected() as ITabItemControl;
            switch (type) {
                case TabActionType.Search:
                    ele.SetSearchFocus();
                    break;
                case TabActionType.NextPage:
                    ele.NextPage();
                    break;
                case TabActionType.PreviousPage:
                    ele.PreviousPage();
                    break;
                case TabActionType.GoToTop:
                    ele.GoToTop();
                    break;
                case TabActionType.GoToBottom:
                    ele.GoToBottom();
                    break;
                case TabActionType.FirstPage:
                    ele.FirstPage();
                    break;
                case TabActionType.LastPage:
                    ele.LastPage();
                    break;

                default:
                    break;

            }

        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (!comboBox.IsLoaded)
                return;

            if (e.AddedItems.Count == 0 || vieModel.CurrentAppDataBase == null)
                return;

            vieModel.CurrentAppDataBase = (AppDatabase)e.AddedItems[0];
            ConfigManager.Main.CurrentDBId = vieModel.CurrentAppDataBase.DBId;
            ConfigManager.Settings.DefaultDBID = vieModel.CurrentAppDataBase.DBId;

            // 切换数据库
            vieModel.Statistic();
            vieModel.TabItemManager.RefreshAllTab(1); // 回到第一页
        }

        public void RefreshData(long dataID)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            foreach (var item in videoLists) {
                item.RefreshData(dataID);
            }
        }

        public void RefreshImage(Video video)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            foreach (var item in videoLists) {
                item.RefreshImage(video);
            }
        }

        public void RefreshGrade(long dataID, float grade)
        {
            List<VideoList> videoLists = vieModel.TabItemManager.GetAllVideoList();
            foreach (var item in videoLists) {
                item.RefreshGrade(dataID, grade);
            }
        }

        private void HideBeginScanGrid(object sender, RoutedEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
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


        public void ShowSameActor(long actorID)
        {
            vieModel.ShowSameActor(actorID);
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
            FileHelper.TryOpenSelectPath(PathManager.LogPath);
        }

        private void OpenApplicationPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory);
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


        private void ShowUpgradeWindow(object sender, RoutedEventArgs e)
        {
            UpgradeHelper.OpenWindow();
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            App.ShowAbout();
        }

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
            notiIconPopup.IsOpen = false;
        }



        private void ManageDataBase(object sender, RoutedEventArgs e)
        {
            Window_DataBase dataBase = new Window_DataBase();
            dataBase.Owner = this;

            dataBase.OnDataChanged += () => {
                vieModel.Statistic();
            };

            dataBase.ShowDialog();
        }

        public bool IsTaskRunning()
        {
            return (App.ScreenShotManager.RunningCount > 0 ||
                App.ScanManager.RunningCount > 0 ||
                App.DownloadManager.RunningCount > 0);
        }


        private void OpenServer(object sender, RoutedEventArgs e)
        {
            MessageNotify.Info("开发中");
            return;

            //if (window_Server != null)
            //    window_Server.Close();
            //window_Server = new Window_Server();

            //window_Server.OnServerStatusChanged += (status) => {
            //    vieModel.ServerStatus = status;
            //};

            //window_Server.Owner = this;
            //window_Server.ShowDialog();

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

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            ImageCache.Clear();
        }

        private void Tab_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
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


        private void Border_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is Border border &&
                border.Tag != null &&
                int.TryParse(border.Tag.ToString(), out int index)) {
                vieModel.TabItemManager.SetTabSelected(index);
            }
        }

        private void RefreshCurrentTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            vieModel.TabItemManager.RefreshTab(idx, -1);
        }

        private void CloseCurrentTab(object sender, RoutedEventArgs e)
        {
            int idx = GetTabIndexByMenuItem(sender);
            if (idx < 0)
                return;
            vieModel.TabItemManager.RemoveTabItem(idx);
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
                vieModel.TabItemManager.RemoveTabItem(index);
            }

        }

        private void BeginDragTabItem(object sender, MouseButtonEventArgs e)
        {

            FrameworkElement ele = (FrameworkElement)sender;
            if (ele == null || ele.Tag == null)
                return;
            string idxStr = ele.Tag.ToString();
            if (string.IsNullOrEmpty(idxStr) || vieModel.TabItems == null ||
                vieModel.TabItems.Count <= 0)
                return;
            int.TryParse(idxStr, out int index);
            if (index >= 0) {
                vieModel.TabItemManager?.SetTabSelected(index);
            }

            CanDragTabItem = true;
            CurrentDragElement = ele;
            Mouse.Capture(CurrentDragElement, CaptureMode.Element);

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
