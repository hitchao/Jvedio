using ChaoControls.Style;
using DynamicData;
using HandyControl.Data;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.CustomTask;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Media;
using Jvedio.Core.Net;
using Jvedio.Core.Plugins.Theme;
using Jvedio.Core.Scan;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Logs;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Utils.Data;
using Jvedio.Utils.IO;
using Jvedio.Utils.Visual;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using static Jvedio.GlobalMapper;
using static Jvedio.GlobalVariable;
using static Jvedio.Main.Msg;
using static Jvedio.Utils.Media.ImageHelper;
using static Jvedio.Utils.Visual.VisualHelper;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : ChaoControls.Style.BaseWindow
    {
        public static int NOTICE_INTERVAL = 1800;//30分钟检测一次
        public static Msg msgCard = new Msg();

        private bool Resizing { get; set; }
        private DispatcherTimer ResizingTimer { get; set; }
        private DispatcherTimer NoticeTimer { get; set; }
        private List<Actress> SelectedActress { get; set; }
        private bool CanRateChange { get; set; }
        private bool IsToUpdate { get; set; }
        private Window_Edit windowEdit { get; set; }
        private Window_Filter windowFilter { get; set; }
        private Window_Details windowDetails { get; set; }
        public VieModel_Main vieModel { get; set; }
        public SelectWrapper<Video> CurrentWrapper { get; set; }
        public string CurrentSQL { get; set; }
        private static bool CheckingScanStatus { get; set; }
        private static bool CheckingDownloadStatus { get; set; }
        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance { get; set; }

        private bool canShowDetails { get; set; }

        private int firstidx = -1;
        private int secondidx = -1;
        private int actorfirstidx = -1;
        private int actorsecondidx = -1;



        private CancellationTokenSource LoadSearchCTS { get; set; }
        private CancellationToken LoadSearchCT { get; set; }
        private CancellationTokenSource scan_cts { get; set; }
        private CancellationToken scan_ct { get; set; }



        public List<ImageSlide> ImageSlides { get; set; }

        public static List<string> ClickFilterDict { get; set; }

        private long CurrentAssoDataID { get; set; }// 当前正在关联的影片的 dataID

        public void Init()
        {
            ResizingTimer = new DispatcherTimer();
            NoticeTimer = new DispatcherTimer();
            SelectedActress = new List<Actress>();
            ClickFilterDict = new List<string>() { "Genre", "Series", "Studio", "Director", };

            vieModel = new VieModel_Main();
            this.DataContext = vieModel;
            BindingEvent();// 绑定控件事件
            // 初始化任务栏的进度条
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
                taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
        }



        public Main()
        {
            InitializeComponent();
            Init();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            AdjustWindow();// 还原窗口为上一次状态
            ConfigFirstRun();
            SetSkin();//设置主题颜色
            InitNotice();//初始化公告
            SetLoadingStatus(false); // todo 删除该行
            CheckUpgrade();//检查更新
            setDataBases();// 设置当前下拉数据库
            setRecentWatched();// 显示最近播放
                               //vieModel.GetFilterInfo(); //todo 筛选器
            vieModel.Statistic();
            //// todo 设置图片类型
            //await vieModel.InitLettersNavigation(); // todo 


            BindingEventAfterRender(); // render 后才绑定的事件
            initTagStamp();
            AllRadioButton.IsChecked = true;

            vieModel.Reset();           // 加载数据
            OpenListen();
            //new Msgbox(this, "demo").ShowDialog();
        }


        private void OpenListen()
        {
            if (GlobalConfig.Settings.ListenEnabled)
            {
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
                Message message = new Message(MessageCard.MessageCardType.Succes, msg);
                MsgShown?.Invoke(this, new MessageEventArg(message));

            }
            public void Error(string msg)
            {
                MessageCard.Error(msg);
                Message message = new Message(MessageCard.MessageCardType.Succes, msg);
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
        private void initTagStamp()
        {
            GlobalVariable.TagStamps = tagStampMapper.getAllTagStamp();
            vieModel.InitCurrentTagStamps();
        }

        private void BindingEventAfterRender()
        {
            //翻页
            pagination.PageSizeChange += (s, e) =>
            {
                Pagination pagination = s as Pagination;
                vieModel.PageSize = pagination.PageSize;
                vieModel.LoadData();
            };


            setComboboxID();
            DatabaseComboBox.SelectionChanged += DatabaseComboBox_SelectionChanged;

            // 搜索框事件
            searchBox.TextChanged += RefreshCandiadte;
            searchTabControl.SelectionChanged += (s, e) =>
            {
                if (GlobalConfig.Main.SearchSelectedIndex == searchTabControl.SelectedIndex) return;
                GlobalConfig.Main.SearchSelectedIndex = searchTabControl.SelectedIndex;
                RefreshCandiadte(null, null);
            };

            vieModel.LoadAssoMetaDataCompleted += (s, e) =>
            {
                SetAssoSelected();
            };

            Global.Download.Dispatcher.onWorking += (s, e) =>
            {
                double progress = Global.Download.Dispatcher.Progress;
                if (progress is double.NaN) progress = 0;
                vieModel.DownLoadProgress = progress;
                if (progress < 100)
                    vieModel.DownLoadVisibility = Visibility.Visible;
                else
                    vieModel.DownLoadVisibility = Visibility.Hidden;
                // 任务栏进度条
                Dispatcher.Invoke(() =>
                {
                    if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
                    {

                        taskbarInstance.SetProgressValue((int)progress, 100, this);
                        if (progress >= 100 || progress <= 0)
                            taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
                        else
                            taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);

                    }
                });





            };

            Global.FFmpeg.Dispatcher.onWorking += (s, e) =>
            {
                vieModel.ScreenShotProgress = Global.FFmpeg.Dispatcher.Progress;
                vieModel.ScreenShotVisibility = Visibility.Visible;
            };

            // 右键菜单栏点击事件
            foreach (MenuItem item in VideoTypeMenuItem.Items.OfType<MenuItem>())
            {
                item.Click += (s, e) => vieModel.LoadData();
            }


            List<MenuItem> PictureFilterTypes = PictureFilterType.Items.OfType<MenuItem>().ToList();
            foreach (MenuItem item in PictureFilterTypes)
            {
                item.Click += (s, e) =>
                {
                    if (!GlobalConfig.Settings.PictureIndexCreated)
                    {
                        MessageCard.Error("请在【选项-库】中建立图片索引！");
                        return;
                    }
                    foreach (var t in PictureFilterTypes)
                        t.IsChecked = false;
                    MenuItem menuItem = s as MenuItem;
                    menuItem.IsChecked = true;
                    vieModel.PictureTypeIndex = PictureFilterTypes.IndexOf(menuItem);
                    vieModel.LoadData();
                };
            }

            List<MenuItem> DataExistMenuItems = DataExistMenuItem.Items.OfType<MenuItem>().ToList();
            foreach (MenuItem menuItem in DataExistMenuItems)
            {
                menuItem.Click += (s, e) =>
                {
                    if (!GlobalConfig.Settings.PlayableIndexCreated)
                    {
                        MessageCard.Error("请在【选项-库】中建立播放索引！");
                        return;
                    }
                    foreach (var t in DataExistMenuItems)
                        t.IsChecked = false;
                    MenuItem item = s as MenuItem;
                    item.IsChecked = true;
                    vieModel.DataExistIndex = DataExistMenuItems.IndexOf(item);
                    vieModel.LoadData();
                };
            }

            // 长时间暂停
            Global.Download.Dispatcher.onLongDelay += (s, e) =>
            {
                string message = (e as MessageCallBackEventArgs).Message;
                int.TryParse(message, out int value);
                vieModel.DownloadLongTaskDelay = value / 1000;
            };

        }

        public void setComboboxID()
        {
            int idx = vieModel.DataBases.ToList().FindIndex(arg => arg.DBId == GlobalConfig.Main.CurrentDBId);
            if (idx < 0 || idx > DatabaseComboBox.Items.Count) idx = 0;
            DatabaseComboBox.SelectedIndex = idx;
        }

        private async void RefreshCandiadte(object sender, TextChangedEventArgs e)
        {
            List<string> list = await vieModel.GetSearchCandidate();
            int idx = (int)GlobalConfig.Main.SearchSelectedIndex;
            TabItem tabItem = searchTabControl.Items[idx] as TabItem;
            addOrRefreshItem(tabItem, list);
        }


        private void addOrRefreshItem(TabItem tabItem, List<string> list)
        {
            ListBox listBox;
            if (tabItem.Content == null)
            {
                listBox = new ListBox();
                tabItem.Content = listBox;
            }
            else
            {
                listBox = tabItem.Content as ListBox;
            }
            listBox.Margin = new Thickness(0, 0, 0, 5);
            listBox.Style = (System.Windows.Style)App.Current.Resources["NormalListBox"];
            listBox.ItemContainerStyle = (System.Windows.Style)this.Resources["SearchBoxListItemContainerStyle"];
            listBox.Background = Brushes.Transparent;
            listBox.ItemsSource = list;
            if (vieModel.TabSelectedIndex == 0 && !string.IsNullOrEmpty(vieModel.SearchText))
                vieModel.Searching = true;
        }



        public void setDataBases()
        {
            List<AppDatabase> appDatabases =
                appDatabaseMapper.selectList(new SelectWrapper<AppDatabase>().Eq("DataType", (int)GlobalVariable.CurrentDataType));
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            appDatabases.ForEach(db => temp.Add(db));
            vieModel.DataBases = temp;
            if (temp.Count > 0)
            {
                vieModel.CurrentAppDataBase = appDatabases.Where(arg => arg.DBId == GlobalConfig.Main.CurrentDBId).FirstOrDefault();
                if (vieModel.CurrentAppDataBase == null) vieModel.CurrentAppDataBase = temp[0];
            }

        }

        private void setRecentWatched()
        {
            SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("DataType", (int)GlobalVariable.CurrentDataType).NotEq("ViewDate", "");
            long count = metaDataMapper.selectCount(wrapper);
            vieModel.RecentWatchedCount = count;
        }

        // todo 热键
        #region "热键"

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Console.WriteLine("***************OnSourceInitialized***************");

            //热键
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            //注册热键

            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (Properties.Settings.Default.HotKey_Enable && modifier != 0 && vk != 0)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success)
                {
                    MessageBox.Show(Jvedio.Language.Resources.HotKeyConflict, Jvedio.Language.Resources.HotKeyConflict);
                }
            }

        }




        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == Properties.Settings.Default.HotKey_VK)
                            {
                                if (vieModel.TaskIconVisible)
                                {
                                    SetWindowVisualStatus(false, false);
                                }
                                else
                                {
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

        private void SetWindowVisualStatus(bool visible, bool taskIconVisible = true)
        {

            if (visible)
            {
                foreach (Window window in App.Current.Windows)
                {
                    if (OpeningWindows.Contains(window.GetType().ToString()))
                    {
                        AnimateWindow(window);
                    }
                }

            }
            else
            {
                OpeningWindows.Clear();
                foreach (Window window in App.Current.Windows)
                {
                    window.Hide();
                    OpeningWindows.Add(window.GetType().ToString());
                }
            }
            vieModel.TaskIconVisible = taskIconVisible;
            WindowsVisible = visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            //Console.WriteLine("***************OnClosed***************");
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID); //取消热键
            vieModel.TaskIconVisible = false;                //隐藏图标
                                                             //DisposeGif("", true);                       //清除 gif 资源
            NoticeTimer.Stop();
            windowFilter?.Close();
            base.OnClosed(e);
        }


        #endregion




        //绑定事件
        private void BindingEvent()
        {
            this.MaximumToNormal += (s, e) =>
            {
                MaxPath.Data = Geometry.Parse(PathData.MaxPath);
                MaxMenuItem.Header = "最大化";
            };

            this.NormalToMaximum += (s, e) =>
            {
                MaxPath.Data = Geometry.Parse(PathData.MaxToNormalPath);
                MaxMenuItem.Header = "窗口化";
            };



            //设置排序类型
            int.TryParse(Properties.Settings.Default.SortType, out int sortType);
            var MenuItems = SortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Click += SortMenu_Click;
                MenuItems[i].IsCheckable = true;
                if (i == sortType) MenuItems[i].IsChecked = true;
            }

            //设置演员排序类型
            int ActorSortType = Properties.Settings.Default.ActorSortType;
            var ActorMenuItems = ActorSortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < ActorMenuItems.Count; i++)
            {
                ActorMenuItems[i].Click += ActorSortMenu_Click;
                ActorMenuItems[i].IsCheckable = true;
                if (i == ActorSortType) ActorMenuItems[i].IsChecked = true;
            }

            //设置图片显示模式
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int.TryParse(Properties.Settings.Default.ShowImageMode, out int viewMode);
            for (int i = 0; i < rbs.Count; i++)
            {
                rbs[i].Click += SetViewMode;
                if (i == viewMode) rbs[i].IsChecked = true;
            }

            //设置演员显示模式
            var arbs = ActorViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            for (int i = 0; i < arbs.Count; i++)
            {
                arbs[i].Click += SetActorViewMode;
                if (i == Properties.Settings.Default.ActorViewMode) arbs[i].IsChecked = true;
            }

            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);


            vieModel.PageChangedCompleted += (s, ev) =>
            {
                if (Properties.Settings.Default.EditMode) SetSelected();
                if (GlobalConfig.Settings.AutoGenScreenShot) AutoGenScreenShot();
            };

            vieModel.ActorPageChangedCompleted += (s, ev) =>
            {
                if (Properties.Settings.Default.ActorEditMode) ActorSetSelected();
            };

            vieModel.RenderSqlChanged += (s, ev) =>
            {
                WrapperEventArg<Video> arg = ev as WrapperEventArg<Video>;
                if (arg != null)
                {
                    CurrentWrapper = arg.Wrapper as SelectWrapper<Video>;
                    CurrentSQL = arg.SQL;
                }
            };

            // 绑定消息
            msgCard.MsgShown += (s, ev) =>
            {
                MessageEventArg eventArg = ev as MessageEventArg;
                if (eventArg != null && vieModel.Message != null)
                    vieModel.Message.Add(eventArg.Message);
            };
        }

        public childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem) return (childItem)child;
                childItem childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }


        private void AutoGenScreenShot()
        {
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            {
                if (vieModel.CurrentVideoList[i].BigImage == GlobalVariable.DefaultBigImage)
                {
                    // 检查有无截图
                    Video video = vieModel.CurrentVideoList[i];
                    string path = video.getScreenShot();
                    if (Directory.Exists(path))
                    {
                        string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (array.Length > 0)
                        {

                            Video.SetImage(ref video, array[array.Length / 2]);
                            vieModel.CurrentVideoList[i].BigImage = null;
                            vieModel.CurrentVideoList[i].BigImage = video.ViewImage;
                        }
                    }
                }
            }
        }

        public void SetLoadingStatus(bool loading)
        {
            vieModel.IsLoadingMovie = loading;
            vieModel.IsFlipOvering = loading;
        }


        private void ResizingTimer_Tick(object sender, EventArgs e)
        {
            Resizing = false;
            ResizingTimer.Stop();
        }

        public void Notify_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }



        public void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            SetWindowVisualStatus(true);
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
            NoticeTimer.Tick += (s, e) => ShowNotice();
            NoticeTimer.Interval = TimeSpan.FromSeconds(NOTICE_INTERVAL);
            NoticeTimer.Start();
            ShowNotice();
        }

        void ShowNotice()
        {
            Task.Run(async () =>
            {
                string configName = "Notice";
                //获取本地的公告
                string notices = "";
                SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
                wrapper.Eq("ConfigName", configName);
                AppConfig appConfig = appConfigMapper.selectOne(wrapper);
                if (appConfig != null && !string.IsNullOrEmpty(appConfig.ConfigValue))
                    notices = appConfig.ConfigValue;
                HttpResult httpResult = await HttpClient.Get(NoticeUrl, CrawlerHeader.GitHub);
                //判断公告是否内容不同
                if (httpResult.StatusCode == HttpStatusCode.OK && !SqlHelper.handleNewLine(httpResult.SourceCode).Equals(notices))
                {
                    //覆盖原有公告
                    string json = httpResult.SourceCode;
                    appConfig.ConfigValue = SqlHelper.handleNewLine(httpResult.SourceCode);
                    appConfig.ConfigName = configName;
                    appConfigMapper.insert(appConfig, Core.Enums.InsertMode.Replace);

                    Dictionary<string, object> dictionary = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(json);
                    if (dictionary != null && dictionary.ContainsKey("Date") && dictionary.ContainsKey("Data"))
                    {
                        string date = dictionary["Date"].ToString();
                        List<Dictionary<string, string>> Data = null;
                        try
                        {
                            Data = ((JArray)dictionary["Data"]).ToObject<List<Dictionary<string, string>>>();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }

                        if (Data != null && Data.Count > 0)
                        {
                            vieModel.Notices = new ObservableCollection<Notice>();
                            foreach (var dict in Data)
                            {
                                if (dict.ContainsKey("Type") && dict.ContainsKey("Message") && dict["Type"] != null && dict["Message"] != null)
                                {
                                    string type = dict["Type"].ToString();
                                    string message = dict["Message"].ToString();
                                    Enum.TryParse(type, out NoticeType noticeType);
                                    if (noticeType == NoticeType.MarkDown)
                                    {
                                        //弹窗提示
                                        this.Dispatcher.Invoke((Action)delegate ()
                                        {
                                            new Dialog_Notice(this, false, message).ShowDialog();
                                        });
                                    }
                                    else
                                    {
                                        Notice notice = new Notice();
                                        notice.NoticeType = noticeType;
                                        notice.Message = message;
                                        notice.Date = date;
                                        vieModel.Notices.Add(notice);
                                    }
                                }
                            }
                        }
                    }
                    Logger.Error("公告解析错误");
                }
                else
                {
                    Console.WriteLine("公告相同无需提示");
                }
            });
        }


        public void SelectAll(object sender, RoutedEventArgs e)
        {
            if (vieModel.CurrentVideoList == null || vieModel.SelectedVideo == null) return;
            Properties.Settings.Default.EditMode = true;
            bool allContain = true;// 检测是否取消选中
            foreach (var item in vieModel.CurrentVideoList)
            {
                if (!vieModel.SelectedVideo.Contains(item))
                {
                    vieModel.SelectedVideo.Add(item);
                    allContain = false;
                }
            }

            if (allContain)
                vieModel.SelectedVideo.RemoveMany(vieModel.CurrentVideoList);
            SetSelected();
        }




        #region "监听文件变动"





        // todo 监听文件变动
        public FileSystemWatcher[] fileSystemWatcher { get; set; }
        public string failwatcherMessage { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void AddListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            fileSystemWatcher = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++)
            {
                try
                {

                    if (drives[i] == @"C:\") { continue; }
                    FileSystemWatcher watcher = new FileSystemWatcher
                    {
                        Path = drives[i],
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                        Filter = "*.*"
                    };
                    watcher.Created += OnCreated;
                    watcher.Deleted += OnDeleted;
                    watcher.EnableRaisingEvents = true;
                    fileSystemWatcher[i] = watcher;

                }
                catch
                {
                    failwatcherMessage += drives[i] + ",";
                    continue;
                }
            }

            if (failwatcherMessage != "")
                msgCard.Info($"{Jvedio.Language.Resources.Message_WatchFail} {failwatcherMessage}");
        }




        // todo
        private static void OnCreated(object obj, FileSystemEventArgs e)
        {
            //导入数据库

            //if (ScanHelper.IsProperMovie(e.FullPath))
            //{
            //    FileInfo fileinfo = new FileInfo(e.FullPath);

            //    //获取创建日期
            //    string createDate = "";
            //    try { createDate = fileinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
            //    catch { }
            //    if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //    Movie movie = new Movie()
            //    {
            //        filepath = e.FullPath,
            //        id = Identify.GetVID(fileinfo.Name),
            //        filesize = fileinfo.Length,
            //        vediotype = Identify.GetVideoType(Identify.GetVID(fileinfo.Name)),
            //        otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        scandate = createDate
            //    };
            //    if (!string.IsNullOrEmpty(movie.id) & movie.vediotype > 0) { DataBase.InsertScanMovie(movie); }
            //    Console.WriteLine($"成功导入{e.FullPath}");
            //}




        }

        // todo
        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            //if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            //{
            //    DataBase.DeleteByField("movie", "filepath", e.FullPath);
            //}
            //Console.WriteLine("成功删除" + e.FullPath);
        }

        #endregion


        public void AdjustWindow()
        {

            if (GlobalConfig.Main.FirstRun)
            {
                this.Width = SystemParameters.WorkArea.Width * 0.8;
                this.Height = SystemParameters.WorkArea.Height * 0.8;
            }
            else
            {
                if (GlobalConfig.Main.Height == SystemParameters.WorkArea.Height && GlobalConfig.Main.Width < SystemParameters.WorkArea.Width)
                {
                    baseWindowState = 0;
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    this.CanResize = true;
                }
                else
                {
                    this.Left = GlobalConfig.Main.X;
                    this.Top = GlobalConfig.Main.Y;
                    this.Width = GlobalConfig.Main.Width;
                    this.Height = GlobalConfig.Main.Height;
                }


                baseWindowState = (BaseWindowState)GlobalConfig.Main.WindowState;
                if (baseWindowState == BaseWindowState.FullScreen)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
                else if (baseWindowState == BaseWindowState.None)
                {
                    baseWindowState = 0;
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                if (this.Width == SystemParameters.WorkArea.Width
                    && this.Height == SystemParameters.WorkArea.Height) baseWindowState = BaseWindowState.Maximized;

                if (baseWindowState == BaseWindowState.Maximized || baseWindowState == BaseWindowState.FullScreen)
                {
                    MaxPath.Data = Geometry.Parse(PathData.MaxToNormalPath);
                    MaxMenuItem.Header = "窗口化";
                }


            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //停止所有任务
            try
            {
                // todo 下载程序
                if (vieModel.DownLoadTasks != null)
                {
                    foreach (var item in vieModel.DownLoadTasks)
                    {
                        item.Cancel();
                    }
                }
                LoadSearchCTS?.Cancel();
                scan_cts?.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // 关闭所有窗口
            App.Current.Shutdown();

        }

        public void FadeOut()
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                var anim = new DoubleAnimation(0, (Duration)FadeInterval);
                anim.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.Close();
            }
        }



        public override void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (!IsToUpdate && GlobalConfig.Settings.CloseToTaskBar && this.IsVisible == true)
            {
                SetWindowVisualStatus(false);
            }
            else
            {
                FadeOut();
                base.CloseWindow(sender, e);
            }
        }

        public void MinWindow(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                var anim = new DoubleAnimation(0, (Duration)FadeInterval, FillBehavior.Stop);
                anim.Completed += (s, _) => this.WindowState = System.Windows.WindowState.Minimized;
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }
        }


        public void OnMaxWindow(object sender, RoutedEventArgs e)
        {
            this.MaxWindow(sender, e);

        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;

            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (baseWindowState == BaseWindowState.Maximized || (this.Width == SystemParameters.WorkArea.Width && this.Height == SystemParameters.WorkArea.Height))
                {
                    baseWindowState = 0;
                    double fracWidth = e.GetPosition(border).X / border.ActualWidth;
                    this.Width = WindowSize.Width;
                    this.Height = WindowSize.Height;
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.Left = e.GetPosition(border).X - border.ActualWidth * fracWidth;
                    this.Top = e.GetPosition(border).Y - border.ActualHeight / 2;
                    this.OnLocationChanged(EventArgs.Empty);
                    MaxPath.Data = Geometry.Parse(PathData.MaxPath);
                    MaxMenuItem.Header = "最大化";
                }
                this.DragMove();
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

        private async void CheckUpgrade()
        {
            // 启动后检查更新
            try
            {
                (string LatestVersion, string ReleaseDate, string ReleaseNote) result = await UpgradeHelper.getUpgardeInfo();
                string remote = result.LatestVersion;
                if (!string.IsNullOrEmpty(remote))
                {
                    string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (local.CompareTo(remote) < 0)
                    {
                        new Dialog_Upgrade(this, false, remote, result.ReleaseDate, result.ReleaseNote).ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }







        // todo 优化搜索栏
        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //doSearch(sender, null);
            }
            else if (e.Key == Key.Down)
            {
                //int count = vieModel.CurrentSearchCandidate.Count;

                //SearchSelectIdex += 1;
                //if (SearchSelectIdex >= count) SearchSelectIdex = 0;
                //SetSearchSelect();

            }
            else if (e.Key == Key.Up)
            {
                //int count = vieModel.CurrentSearchCandidate.Count;
                //SearchSelectIdex -= 1;
                //if (SearchSelectIdex < 0) SearchSelectIdex = count - 1;
                //SetSearchSelect();


            }
            else if (e.Key == Key.Escape)
            {
                vieModel.Searching = false;
            }
            else if (e.Key == Key.Delete)
            {
                //searchBox.clearte();
                searchBox.ClearText();
            }
            else if (e.Key == Key.Tab)
            {
                //int maxIndex = searchTabControl.Items.Count - 1;
                //int idx = searchTabControl.SelectedIndex;
                //if (idx + 1 > maxIndex)
                //{
                //    idx = 0;
                //}
                //else
                //{
                //    idx++;
                //}
                //searchTabControl.SelectedIndex = idx;
                //e.Handled = true;
                //searchBox.Focus();
                //searchTabControl.Focus();
            }
        }


        public void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            string text = tb.Text;
            if (string.IsNullOrEmpty(text) || text.IndexOf("(") <= 0) return;
            string labelName = text.Substring(0, text.IndexOf("("));
            ShowSameLabel(labelName);
        }

        public void ShowSameLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("LabelName", label);
            vieModel.extraWrapper = wrapper;
            vieModel.ClickFilterType = "Label";
            pagination.CurrentPageChange -= Pagination_CurrentPageChange;
            vieModel.CurrentPage = 1;
            vieModel.LoadData();
            pagination.CurrentPageChange += Pagination_CurrentPageChange;
        }



        public void ShowSameString(string str, string clickFilterType = "")
        {
            if (vieModel.ClassifySelectedIndex >= ClickFilterDict.Count || string.IsNullOrEmpty(str)) return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            if (string.IsNullOrEmpty(clickFilterType)) clickFilterType = ClickFilterDict[vieModel.ClassifySelectedIndex];
            wrapper.Like(clickFilterType, str);
            vieModel.extraWrapper = wrapper;
            vieModel.ClickFilterType = clickFilterType;
            vieModel.CurrentPage = 1;
            vieModel.LoadData();
        }

        private void ShowSameString(object sender, MouseButtonEventArgs e)
        {
            // todo 存在一些问题：like '%demo%' => '%demo-xxx%'，导致数目多出
            TextBlock tb = sender as TextBlock;
            string text = tb.Text;
            if (string.IsNullOrEmpty(text) || text.IndexOf("(") <= 0) return;
            string labelName = text.Substring(0, text.IndexOf("("));
            ShowSameString(labelName);

        }








        public void ActorSetSelected()
        {
            ItemsControl itemsControl = ActorItemsControl;
            if (itemsControl == null) return;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null) continue;
                Border border = FindElementByName<Border>(presenter, "rootBorder");
                if (border == null) continue;
                long actorID = getDataID(border);
                if (border != null && actorID > 0)
                {

                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (Properties.Settings.Default.ActorEditMode && vieModel.SelectedActors != null &&
                        vieModel.SelectedActors.Where(arg => arg.ActorID == actorID).Any())
                    {
                        border.Background = GlobalStyle.Common.HighLight.Background;
                        border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;

                    }
                }

            }
        }


        public void ActorBorderMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (Properties.Settings.Default.ActorEditMode && grid != null)
            {
                Border border = grid.Children[0] as Border;
                if (border != null)
                    border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            }
        }

        public void ActorBorderMouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (Properties.Settings.Default.ActorEditMode && grid != null)
            {

                long actorID = getDataID(element);
                Border border = grid.Children[0] as Border;
                if (actorID <= 0 || border == null || vieModel.SelectedActors == null) return;
                if (vieModel.SelectedActors.Where(arg => arg.ActorID == actorID).Any())
                    border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
                else
                    border.BorderBrush = Brushes.Transparent;
            }
        }



        // todo 优化多选
        public void SelectActor(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;//点击 border 也能选中
            long actorID = getDataID(element);
            if (actorID <= 0) return;
            if (Properties.Settings.Default.ActorEditMode && vieModel.CurrentActorList != null)
            {
                ActorInfo actorInfo = vieModel.CurrentActorList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
                if (actorInfo == null) return;
                int selectIdx = vieModel.CurrentActorList.IndexOf(actorInfo);

                // 多选
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (actorfirstidx == -1)
                        actorfirstidx = selectIdx;
                    else
                        actorsecondidx = selectIdx;
                }

                if (actorfirstidx >= 0 && actorsecondidx >= 0)
                {
                    if (actorfirstidx > actorsecondidx)
                    {
                        //交换一下顺序
                        int temp = actorfirstidx;
                        actorfirstidx = actorsecondidx - 1;
                        actorsecondidx = temp - 1;
                    }

                    for (int i = actorfirstidx + 1; i <= actorsecondidx; i++)
                    {
                        ActorInfo m = vieModel.CurrentActorList[i];
                        if (vieModel.SelectedActors.Contains(m))
                            vieModel.SelectedActors.Remove(m);
                        else
                            vieModel.SelectedActors.Add(m);
                    }
                    actorfirstidx = -1;
                    actorsecondidx = -1;
                }
                else
                {
                    if (vieModel.SelectedActors.Contains(actorInfo))
                        vieModel.SelectedActors.Remove(actorInfo);
                    else
                        vieModel.SelectedActors.Add(actorInfo);
                }


                ActorSetSelected();

            }
        }

        private void ShowDetails(object sender, MouseButtonEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
            if (Resizing || !canShowDetails) return;
            FrameworkElement element = sender as FrameworkElement;//点击 border 也能选中
            long ID = getDataID(element);
            if (ID <= 0) return;
            if (Properties.Settings.Default.EditMode && vieModel.CurrentVideoList != null)
            {
                Video video = vieModel.CurrentVideoList.Where(arg => arg.DataID == ID).FirstOrDefault();
                if (video == null) return;
                int selectIdx = vieModel.CurrentVideoList.IndexOf(video);

                // 多选
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (firstidx == -1)
                        firstidx = selectIdx;
                    else
                        secondidx = selectIdx;
                }

                if (firstidx >= 0 && secondidx >= 0)
                {
                    if (firstidx > secondidx)
                    {
                        //交换一下顺序
                        int temp = firstidx;
                        firstidx = secondidx - 1;
                        secondidx = temp - 1;
                    }

                    for (int i = firstidx + 1; i <= secondidx; i++)
                    {
                        Video m = vieModel.CurrentVideoList[i];
                        if (vieModel.SelectedVideo.Contains(m))
                            vieModel.SelectedVideo.Remove(m);
                        else
                            vieModel.SelectedVideo.Add(m);
                    }
                    firstidx = -1;
                    secondidx = -1;
                }
                else
                {
                    if (vieModel.SelectedVideo.Contains(video))
                        vieModel.SelectedVideo.Remove(video);
                    else
                        vieModel.SelectedVideo.Add(video);
                }


                SetSelected();

            }
            else
            {
                windowDetails?.Close();
                windowDetails = new Window_Details(ID);
                windowDetails.Show();
            }
            canShowDetails = false;
        }



        private void CanShowDetails(object sender, MouseButtonEventArgs e)
        {
            canShowDetails = true;
        }


        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            downloadStatusPopup.IsOpen = true;
        }
        public void ShowScreenShotPopup(object sender, MouseButtonEventArgs e)
        {
            screenShotStatusPopup.IsOpen = true;
        }


        public void SetSelected()
        {
            ItemsControl itemsControl = MovieItemsControl;
            if (itemsControl == null) return;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null) continue;
                Border border = FindElementByName<Border>(presenter, "rootBorder");
                if (border == null) continue;
                long dataID = getDataID(border);
                if (border != null && dataID > 0)
                {
                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (Properties.Settings.Default.EditMode && vieModel.SelectedVideo != null &&
                        vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any())
                    {
                        border.Background = GlobalStyle.Common.HighLight.Background;
                        border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;

                    }
                }

            }

        }


        // todo
        public void SetViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            if (radioButton == null) return;
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            ViewMode viewMode = (ViewMode)idx;


            Properties.Settings.Default.ShowImageMode = idx.ToString();
            Properties.Settings.Default.Save();


            //else if (idx == 2)
            //{
            //    AsyncLoadExtraPic();
            //}

            if (idx == 0)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.SmallImage_Width;
            else if (idx == 1)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.BigImage_Width;
            else if (idx == 2)
            {
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.GifImage_Width;
                AsyncLoadGif();
            }
            else if (idx == 3)
            {
                //vieModel.ShowDetailsData();
            }
        }

        public void SetActorViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            if (radioButton == null) return;
            var rbs = ActorViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            Properties.Settings.Default.ActorViewMode = idx;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();
        }


        // todo
        public void AsyncLoadExtraPic()
        {
            ItemsControl itemsControl = MovieItemsControl;
            if (ImageSlides == null) ImageSlides = new List<ImageSlide>();
            List<Image> images1 = new List<Image>();
            List<Image> images2 = new List<Image>();

            //从流动出的数目中开始加载预览图
            for (int i = ImageSlides.Count; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter myContentPresenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (myContentPresenter != null)
                {
                    DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                    Image myImage = (Image)myDataTemplate.FindName("myImage", myContentPresenter);
                    Image myImage2 = (Image)myDataTemplate.FindName("myImage2", myContentPresenter);
                    images1.Add(myImage);
                    images2.Add(myImage2);
                }
            }

            //从流动出的数目中开始加载预览图
            int idx = ImageSlides.Count;
            Task.Run(async () =>
            {
                for (int i = idx; i < vieModel.CurrentVideoList.Count; i++)
                {
                    await Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                    {
                        ImageSlide imageSlide = new ImageSlide(BasePicPath + $"ExtraPic\\{images1[i - idx].Tag}", images1[i - idx], images2[i - idx]);
                        ImageSlides.Add(imageSlide);

                    });
                }
            });
        }


        // todo
        public void AsyncLoadGif()
        {
            //if (vieModel.CurrentVideoList == null) return;
            //DisposeGif("", true);
            //Task.Run(async () =>
            //{
            //    for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            //    {
            //        Video video = vieModel.CurrentVideoList[i];
            //        string gifpath = Video.parseImagePath(video.GifImagePath);
            //        if (video.GifUri != null && !string.IsNullOrEmpty(video.GifUri.OriginalString)
            //            && video.GifUri.OriginalString.IndexOf("/NoPrinting_G.gif") < 0) continue;
            //        if (File.Exists(gifpath))
            //            video.GifUri = new Uri(gifpath);
            //        else
            //            video.GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            //        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
            //        {
            //            vieModel.CurrentVideoList[i] = null;
            //            vieModel.CurrentVideoList[i] = video;
            //        });
            //    }
            //});

        }

        ScrollViewer dataScrollViewer { get; set; }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //流动模式
            if (dataScrollViewer == null)
            {
                ItemsControl itemsControl = sender as ItemsControl;
                dataScrollViewer = FindVisualChild<ScrollViewer>(itemsControl);
            }

            if (dataScrollViewer == null) return;

            double offset = dataScrollViewer.VerticalOffset;


            if (offset >= 500)
                vieModel.GoToTopCanvas = Visibility.Visible;
            else
                vieModel.GoToTopCanvas = Visibility.Hidden;

            if (offset == dataScrollViewer.ScrollableHeight)
                vieModel.GoToBottomCanvas = Visibility.Hidden;
            else
                vieModel.GoToBottomCanvas = Visibility.Visible;


        }




        public void GotoTop(object sender, MouseButtonEventArgs e)
        {
            dataScrollViewer?.ScrollToTop();
        }

        private void GotoBottom(object sender, MouseButtonEventArgs e)
        {
            dataScrollViewer?.ScrollToBottom();
        }

        public void PlayVideo(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement el = sender as FrameworkElement;
            long dataid = getDataID(el);
            if (dataid <= 0) return;
            Video video = getVideo(dataid);
            if (video == null)
            {
                msgCard.Error("无法播放该视频！");
                return;
            }
            PlayVideoWithPlayer(video.Path, dataid);

        }
        public void PlayAssoVideo(object sender, MouseButtonEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
            FrameworkElement el = sender as FrameworkElement;
            long dataid = getDataID(el);
            if (dataid <= 0) return;
            Video video = getAssoVideo(dataid);
            if (video == null)
            {
                msgCard.Error("无法播放该视频！");
                return;
            }
            PlayVideoWithPlayer(video.Path, dataid);

        }

        public void PlayVideoWithPlayer(string filepath, long dataID = 0)
        {
            if (File.Exists(filepath))
            {
                bool success = false;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.VedioPlayerPath) && File.Exists(Properties.Settings.Default.VedioPlayerPath))
                {
                    success = FileHelper.TryOpenFile(Properties.Settings.Default.VedioPlayerPath, filepath);
                }
                else
                {
                    //使用默认播放器
                    success = FileHelper.TryOpenFile(filepath);
                }

                if (success && dataID > 0)
                {
                    metaDataMapper.updateFieldById("ViewDate", DateHelper.Now(), dataID);
                    vieModel.Statistic();
                }
            }
            else
            {
                msgCard.Error(Jvedio.Language.Resources.Message_OpenFail + "：" + filepath);
            }
        }

        // todo
#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        public async void TranslateMovie(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            //if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO)
            //{
            //    msgCard.Info(Jvedio.Language.Resources.Message_SetYoudao);
            //    return;
            //}


            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();

            //string id = GetIDFromMenuItem(sender, 1);
            //Movie CurrentMovie = GetMovieFromVieModel(id);
            //if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            //string result = "";
            //MySqlite dataBase = new MySqlite("Translate");


            //int successNum = 0;
            //int failNum = 0;
            //int translatedNum = 0;

            //foreach (Movie movie in vieModel.SelectedVideo)
            //{

            //    //检查是否已经翻译过，如有则跳过
            //    if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id))) { translatedNum++; continue; }
            //    if (movie.title != "")
            //    {

            //        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.title);
            //        //保存
            //        if (result != "")
            //        {

            //            dataBase.SaveYoudaoTranslateByID(movie.id, movie.title, result, "title");

            //            //显示
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

            //        }

            //    }
            //    else { failNum++; }

            //    if (movie.plot != "")
            //    {
            //        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.plot);
            //        //保存
            //        if (result != "")
            //        {
            //            dataBase.SaveYoudaoTranslateByID(movie.id, movie.plot, result, "plot");
            //            dataBase.CloseDB();
            //        }

            //    }

            //}
            //dataBase.CloseDB();
            //msgCard.Success($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum}");
            //msgCard.Error($"{Jvedio.Language.Resources.Message_FailNum} {failNum}");
            //msgCard.Info($"{Jvedio.Language.Resources.Message_SkipNum} {translatedNum}");

            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }



#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        public async void GenerateActor(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            //if (!Properties.Settings.Default.Enable_BaiduAI) { msgCard.Info(Jvedio.Language.Resources.Message_SetBaiduAI); return; }
            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();

            //string id = GetIDFromMenuItem(sender, 1);
            //Movie CurrentMovie = GetMovieFromVieModel(id);
            //if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            //this.Cursor = Cursors.Wait;
            //int successNum = 0;

            //foreach (Movie movie in vieModel.SelectedVideo)
            //{
            //    if (movie.actor == "") continue;
            //    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";

            //    string name;
            //    if (vieModel.ActorInfoGrid == Visibility.Visible)
            //        name = vieModel.Actress.name;
            //    else
            //        name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];


            //    string ActressesPicPath = Properties.Settings.Default.BasePicPath + $"Actresses\\{name}.jpg";
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
            //        msgCard.Error(Jvedio.Language.Resources.Message_PosterMustExist);
            //    }
            //}
            //msgCard.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedVideo.Count}");
            ////更新到窗口中
            //foreach (Movie movie1 in vieModel.SelectedVideo)
            //{
            //    if (!string.IsNullOrEmpty(movie1.actor) && movie1.actor.IndexOf(vieModel.Actress.name) >= 0)
            //    {
            //        vieModel.Actress.smallimage = GetActorImage(vieModel.Actress.name);
            //        break;
            //    }
            //}



            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            //this.Cursor = Cursors.Arrow;
        }



        public void GenerateGif(object sender, RoutedEventArgs e)
        {
            GenerateScreenShot(sender, true);
        }


        public void GenerateScreenShot(object sender, RoutedEventArgs e)
        {
            GenerateScreenShot(sender);
        }
        public void GenerateAllScreenShot(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(GlobalConfig.FFmpegConfig.Path))
            {
                MessageCard.Error(Jvedio.Language.Resources.Message_SetFFmpeg);
                return;
            }

            SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("DBId", GlobalConfig.Main.CurrentDBId).Eq("DataType", "0");
            List<MetaData> metaDatas = metaDataMapper.selectList(wrapper);
            if (metaDatas == null || metaDatas.Count <= 0) return;

            foreach (MetaData metaData in metaDatas)
            {
                screenShotVideo(metaData);
            }
            if (!Global.FFmpeg.Dispatcher.Working)
                Global.FFmpeg.Dispatcher.BeginWork();
        }
        public void DownloadAllVideo(object sender, RoutedEventArgs e)
        {
            MessageCard.Info("全库爬取可能会导致你的IP被封，请谨慎使用，尽量同步仅需要的资源");
            vieModel.DownloadStatus = "Downloading";
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("DBId", GlobalConfig.Main.CurrentDBId).Eq("DataType", "0");
            List<Video> videos = videoMapper.selectList();
            foreach (Video video in videos)
            {
                downloadVideo(video);
            }
            if (!Global.Download.Dispatcher.Working)
                Global.Download.Dispatcher.BeginWork();
            setDownloadStatus();
        }


        public void GenerateScreenShot(object sender, bool gif = false)
        {
            if (!File.Exists(GlobalConfig.FFmpegConfig.Path))
            {
                MessageCard.Error(Jvedio.Language.Resources.Message_SetFFmpeg);
                return;
            }
            handleMenuSelected(sender, 1);
            foreach (Video video in vieModel.SelectedVideo)
            {
                screenShotVideo(video, gif);
            }
            if (!Global.FFmpeg.Dispatcher.Working)
                Global.FFmpeg.Dispatcher.BeginWork();
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }






#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            //if (!Properties.Settings.Default.Enable_BaiduAI) { msgCard.Info(Jvedio.Language.Resources.Message_SetBaiduAI); return; }
            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            //string id = GetIDFromMenuItem(sender, 1);
            //Movie CurrentMovie = GetMovieFromVieModel(id);
            //if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            //int successNum = 0;
            //this.Cursor = Cursors.Wait;
            //foreach (Movie movie in vieModel.SelectedVideo)
            //{
            //    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
            //    string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
            //    if (File.Exists(BigPicPath))
            //    {
            //        System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
            //        BitmapImage bitmapImage = ImageHelper.BitmapToBitmapImage(SourceBitmap);
            //        if (Properties.Settings.Default.HalfCutOFf)
            //        {
            //            double rate = 380f / 800f;

            //            Int32Rect int32Rect = new Int32Rect() { Height = SourceBitmap.Height, Width = (int)(rate * SourceBitmap.Width), X = (int)((1 - rate) * SourceBitmap.Width), Y = 0 };
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

            //        }

            //        //读取
            //        int index1 = vieModel.CurrentVideoList.IndexOf(movie);
            //        int index2 = vieModel.MovieList.IndexOf(movie);
            //        int index3 = vieModel.FilterMovieList.IndexOf(movie);
            //        movie.smallimage = ImageHelper.GetBitmapImage(movie.id, "SmallPic");

            //        vieModel.CurrentVideoList[index1] = null;
            //        vieModel.MovieList[index2] = null;
            //        vieModel.FilterMovieList[index3] = null;
            //        vieModel.CurrentVideoList[index1] = movie;
            //        vieModel.MovieList[index2] = movie;
            //        vieModel.FilterMovieList[index3] = movie;



            //    }
            //    else
            //    {
            //        msgCard.Error(Jvedio.Language.Resources.Message_PosterMustExist);
            //    }

            //}
            //msgCard.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedVideo.Count}");

            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            //this.Cursor = Cursors.Arrow;
        }


        public void RenameFile(object sender, RoutedEventArgs e)
        {
            if (GlobalConfig.RenameConfig.FormatString.IndexOf("{") < 0)
            {
                msgCard.Error(Jvedio.Language.Resources.Message_SetRenameRule);
                return;
            }
            handleMenuSelected(sender, 1);

            List<string> logs = new List<string>();
            TaskLogger logger = new TaskLogger(logs);
            List<Video> toRename = new List<Video>();
            foreach (Video video in vieModel.SelectedVideo)
            {
                if (File.Exists(video.Path))
                {
                    toRename.Add(video);
                }
                else
                {
                    logger.Error(Jvedio.Language.Resources.Message_FileNotExist + $" => {video.Path}");
                }
            }

            int successCount = 0;
            int totalCount = toRename.Count;

            Dictionary<long, List<string>> dict = new Dictionary<long, List<string>>();

            //重命名文件
            foreach (Video video in toRename)
            {
                long dataID = video.DataID;
                Video newVideo = videoMapper.SelectVideoByID(dataID);
                string[] newPath = null;
                try
                {
                    newPath = newVideo.ToFileName();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    continue;
                }
                if (newPath == null || newPath.Length == 0) continue;

                if (newVideo.HasSubSection)
                {
                    bool success = false;
                    bool changed = false;
                    string[] oldPaths = newVideo.SubSectionList.ToArray();
                    // 判断是否改变了文件名
                    for (int i = 0; i < newPath.Length; i++)
                    {
                        if (!newPath[i].Equals(oldPaths[i]))
                        {
                            changed = true;
                            break;
                        }
                    }
                    if (!changed)
                    {
                        logger.Info("新文件名与源文件相同！");
                        break;
                    }



                    for (int i = 0; i < newPath.Length; i++)
                    {
                        if (File.Exists(newPath[i]))
                        {
                            logger.Error($"存在同名文件 => {newPath[i]}");
                            newPath[i] = oldPaths[i];// 换回原来的
                            continue;
                        }
                        try
                        {
                            File.Move(video.SubSectionList[i], newPath[i]);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                            newPath[i] = oldPaths[i];// 换回原来的
                            continue;
                        }
                    }
                    if (success) successCount++;
                    if (!dict.ContainsKey(dataID))
                        dict.Add(dataID, newPath.ToList());
                }
                else
                {
                    string target = newPath[0];
                    string origin = newVideo.Path;
                    if (origin.Equals(target))
                    {
                        logger.Info("新文件名与源文件相同！");
                        continue;
                    }


                    if (!File.Exists(target))
                    {
                        try
                        {
                            File.Move(origin, target);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                            continue;
                        }

                        //显示
                        if (!dict.ContainsKey(dataID))
                            dict.Add(dataID, new List<string>() { target });
                    }
                    else
                    {
                        logger.Error($"存在同名文件 => {target}");
                    }

                }
            }
            // 更新
            if (dict.Count > 0)
            {
                for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
                {
                    Video video = vieModel.CurrentVideoList[i];
                    long dataID = video.DataID;
                    if (dict.ContainsKey(dataID))
                    {
                        if (video.HasSubSection)
                        {
                            List<string> list = dict[dataID];
                            string SubSection = string.Join(GlobalVariable.Separator.ToString(), list);
                            vieModel.CurrentVideoList[i].Path = list[0];
                            vieModel.CurrentVideoList[i].SubSection = SubSection;
                            metaDataMapper.updateFieldById("Path", list[0], dataID);
                            videoMapper.updateFieldById("SubSection", SubSection, dataID);
                        }
                        else
                        {
                            string path = dict[dataID][0];
                            vieModel.CurrentVideoList[i].Path = path;
                            metaDataMapper.updateFieldById("Path", path, dataID);
                        }
                    }
                }
                msgCard.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successCount}/{totalCount} ");
            }
            else
            {
                msgCard.Info($"没有需要重命名的文件");
            }

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();

            if (logs.Count > 0)
            {
                new Dialog_Logs(this, string.Join(Environment.NewLine, logs)).ShowDialog();
            }
        }



        public void ReMoveZero(object sender, RoutedEventArgs e)
        {


            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();

            //Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender, 1));
            //if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            //int successnum = 0;
            //for (int i = 0; i < vieModel.SelectedVideo.Count; i++)
            //{
            //    Movie movie = vieModel.SelectedVideo[i];
            //    string oldID = movie.id.ToUpper();

            //    Console.WriteLine(vieModel.CurrentVideoList[0].id);

            //    if (oldID.IndexOf("-") > 0)
            //    {
            //        string num = oldID.Split('-').Last();
            //        string eng = oldID.Remove(oldID.Length - num.Length, num.Length);
            //        if (num.Length == 5 && eng.Replace("-", "").All(char.IsLetter))
            //        {
            //            string newID = eng + num.Remove(0, 2);
            //            if (DataBase.SelectMovieByID(newID) == null)
            //            {
            //                Movie newMovie = DataBase.SelectMovieByID(oldID);
            //                DataBase.DeleteByField("movie", "id", oldID);
            //                newMovie.id = newID;
            //                DataBase.InsertFullMovie(newMovie);
            //                UpdateInfo(oldID, newID);
            //                successnum++;
            //            }
            //        }


            //    }
            //}

            //msgCard.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successnum}/{vieModel.SelectedVideo.Count}");






            //vieModel.SelectedVideo.Clear();
            //SetSelected();
        }

        private void UpdateInfo(string oldID, string newID)
        {
            //Movie movie = DataBase.SelectMovieByID(newID);
            //SetImage(ref movie);

            //for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            //{
            //    try
            //    {
            //        if (vieModel.CurrentVideoList[i]?.id.ToUpper() == oldID.ToUpper())
            //        {
            //            vieModel.CurrentVideoList[i] = null;
            //            vieModel.CurrentVideoList[i] = movie;
            //            break;
            //        }
            //    }
            //    catch { }
            //}


            //for (int i = 0; i < vieModel.MovieList.Count; i++)
            //{
            //    try
            //    {
            //        if (vieModel.MovieList[i]?.id.ToUpper() == oldID.ToUpper())
            //        {
            //            vieModel.MovieList[i] = null;
            //            vieModel.MovieList[i] = movie;
            //            break;
            //        }
            //    }
            //    catch { }
            //}

            //for (int i = 0; i < vieModel.FilterMovieList.Count; i++)
            //{
            //    try
            //    {
            //        if (vieModel.FilterMovieList[i]?.id.ToUpper() == oldID.ToUpper())
            //        {
            //            vieModel.FilterMovieList[i] = null;
            //            vieModel.FilterMovieList[i] = movie;
            //            break;
            //        }
            //    }
            //    catch { }
            //}
        }


        public void CopyFile(object sender, RoutedEventArgs e)
        {
            handleMenuSelected((sender));
            StringCollection paths = new StringCollection();
            int count = 0;
            int total = 0;
            foreach (var video in vieModel.SelectedVideo)
            {

                if (video.SubSectionList?.Count > 0)
                {
                    total += video.SubSectionList.Count;
                    foreach (var path in video.SubSectionList)
                    {
                        if (File.Exists(path))
                        {
                            paths.Add(path);
                            count++;
                        }
                    }

                }
                else
                {
                    total++;
                    if (File.Exists(video.Path))
                    {
                        paths.Add(video.Path);
                        count++;
                    }
                }
            }

            if (paths.Count <= 0)
            {
                msgCard.Warning($"需要复制文件的个数为 0，文件可能不存在");
                return;
            }
            bool success = ClipBoard.TrySetFileDropList(paths, (error) => { msgCard.Error(error); });

            if (success)
                msgCard.Success($"{Jvedio.Language.Resources.Message_Copied} {count}/{total}");


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }






        public void CopyAssoFile(object sender, RoutedEventArgs e)
        {
            handleMenuSelected((sender));
            StringCollection paths = new StringCollection();
            int count = 0;
            int total = 0;
            foreach (var video in vieModel.SelectedVideo)
            {

                if (video.SubSectionList?.Count > 0)
                {
                    total += video.SubSectionList.Count;
                    foreach (var path in video.SubSectionList)
                    {
                        if (File.Exists(path))
                        {
                            paths.Add(path);
                            count++;
                        }
                    }

                }
                else
                {
                    total++;
                    if (File.Exists(video.Path))
                    {
                        paths.Add(video.Path);
                        count++;
                    }
                }
            }

            if (paths.Count <= 0)
            {
                msgCard.Warning($"需要复制文件的个数为 0，文件可能不存在");
                return;
            }
            bool success = ClipBoard.TrySetFileDropList(paths, (error) => { msgCard.Error(error); });

            if (success)
                msgCard.Success($"{Jvedio.Language.Resources.Message_Copied} {count}/{total}");


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }


        /// <summary>
        /// 将点击的该项也加入到选中列表中
        /// </summary>
        /// <param name="dataID"></param>
        private void handleMenuSelected(object sender, int depth = 0)
        {
            long dataID = GetIDFromMenuItem(sender, depth);
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            Video currentVideo = vieModel.CurrentVideoList.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (!vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any()) vieModel.SelectedVideo.Add(currentVideo);
        }


        // todo 异步删除
        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            handleMenuSelected((sender));
            if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToDelete).ShowDialog() == false) { return; }
            int num = 0;
            int totalCount = vieModel.SelectedVideo.Count;
            vieModel.SelectedVideo.ForEach(arg =>
            {

                if (arg.SubSectionList?.Count > 0)
                {
                    totalCount += arg.SubSectionList.Count - 1;
                    //分段视频
                    foreach (var path in arg.SubSectionList)
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                num++;
                            }
                            catch (Exception ex) { Logger.LogF(ex); }
                        }
                    }


                }
                else
                {
                    if (File.Exists(arg.Path))
                    {
                        try
                        {
                            FileSystem.DeleteFile(arg.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                            num++;
                        }
                        catch (Exception ex) { Logger.LogF(ex); }
                    }
                }
            });
            msgCard.Info($"{Jvedio.Language.Resources.Message_DeleteToRecycleBin} {num}/{totalCount}");

            if (num > 0 && Properties.Settings.Default.DelInfoAfterDelFile)
                deleteIDs(vieModel.SelectedVideo, false);

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }




        public void EditInfo(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
            windowEdit?.Close();
            windowEdit = new Window_Edit(GetIDFromMenuItem(sender));
            windowEdit.ShowDialog();

        }

        public async void deleteIDs(List<Video> to_delete, bool fromDetailWindow = true)
        {
            if (!fromDetailWindow)
            {
                vieModel.CurrentVideoList.RemoveMany(to_delete);
                vieModel.VideoList.RemoveMany(to_delete);
            }
            else
            {
                // 影片只有单个
                Video video = to_delete[0];
                int idx = -1;
                for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
                {
                    if (vieModel.CurrentVideoList[i].DataID == video.DataID)
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx >= 0)
                {
                    vieModel.CurrentVideoList.RemoveAt(idx);
                    vieModel.VideoList.RemoveAt(idx);
                }

            }


            // todo FilterMovieList
            //vieModel.FilterMovieList.Remove(arg);
            videoMapper.deleteVideoByIds(to_delete.Select(arg => arg.DataID.ToString()).ToList());


            // 关闭详情窗口
            if (!fromDetailWindow && GetWindowByName("Window_Details") is Window window)
            {
                Window_Details windowDetails = (Window_Details)window;
                foreach (var item in to_delete)
                {
                    if (windowDetails.DataID == item.DataID)
                    {
                        windowDetails.Close();
                        break;
                    }
                }
            }

            //msgCard.Info($"{Jvedio.Language.Resources.SuccessDelete} {to_delete.Count} ");
            //修复数字显示
            vieModel.CurrentCount -= to_delete.Count;
            vieModel.TotalCount -= to_delete.Count;

            to_delete.Clear();
            vieModel.Statistic();

            await Task.Delay(1000);
            Properties.Settings.Default.EditMode = false;
            vieModel.SelectedVideo.Clear();
            SetSelected();
        }

        public void DeleteID(object sender, RoutedEventArgs e)
        {
            handleMenuSelected(sender);
            if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToDelete).ShowDialog() == false) { return; }
            deleteIDs(vieModel.SelectedVideo, false);
        }












        //打开网址
        private void OpenWeb(object sender, RoutedEventArgs e)
        {
            handleMenuSelected(sender);

            // 超过 3 个网页，询问是否继续
            if (vieModel.SelectedVideo.Count >= 3 && new Msgbox(this, $"即将打开 {vieModel.SelectedVideo.Count} 个网页，是否继续？").ShowDialog() == false) return;

            foreach (Video video in vieModel.SelectedVideo)
            {
                string url = video.WebUrl;
                if (url.IsProperUrl())
                    FileHelper.TryOpenUrl(url, (err) =>
                    {
                        MessageCard.Error(err);
                    });
            }
        }




        private long GetIDFromMenuItem(object sender, int depth = 0)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = null;
            if (depth == 0)
            {
                contextMenu = mnu.Parent as ContextMenu;
            }
            else
            {
                MenuItem _mnu = mnu.Parent as MenuItem;
                contextMenu = _mnu.Parent as ContextMenu;
            }
            GifImage gifImage = contextMenu.PlacementTarget as GifImage;
            return getDataID(gifImage);
        }


        public void downloadVideo(Video video)
        {
            DownLoadTask task = new DownLoadTask(video, GlobalConfig.Settings.DownloadPreviewImage, GlobalConfig.Settings.OverrideInfo);
            long vid = video.DataID;
            task.onError += (s, ev) =>
            {
                msgCard.Error((ev as MessageCallBackEventArgs).Message);
            };
            task.onDownloadSuccess += (s, ev) =>
            {
                DownLoadTask t = s as DownLoadTask;
                Dispatcher.Invoke(() =>
                {
                    RefreshData(t.DataID);
                });
            };

            addToDownload(task);

        }
        public void screenShotVideo(Video video, bool gif = false)
        {
            ScreenShotTask task = new ScreenShotTask(video, gif);
            task.onError += (s, ev) =>
            {
                msgCard.Error((ev as MessageCallBackEventArgs).Message);
            };
            addToScreenShot(task);

        }
        public void screenShotVideo(MetaData metaData)
        {
            ScreenShotTask task = new ScreenShotTask(metaData);
            task.onError += (s, ev) =>
            {
                msgCard.Error((ev as MessageCallBackEventArgs).Message);
            };
            addToScreenShot(task);

        }

        public void addToDownload(DownLoadTask task)
        {
            if (!vieModel.DownLoadTasks.Contains(task))
            {
                Global.Download.Dispatcher.Enqueue(task);
                vieModel.DownLoadTasks.Add(task);
            }
            else
            {
                MessageCard.Info("任务已存在！");
            }
        }
        public void addToScreenShot(ScreenShotTask task)
        {
            if (!vieModel.ScreenShotTasks.Contains(task))
            {
                Global.FFmpeg.Dispatcher.Enqueue(task);
                vieModel.ScreenShotTasks.Add(task);
            }
            else
            {
                MessageCard.Info("任务已存在！");
            }
        }


        private void DownLoadSelectMovie(object sender, RoutedEventArgs e)
        {
            handleMenuSelected(sender);
            vieModel.DownloadStatus = "Downloading";
            foreach (Video video in vieModel.SelectedVideo)
            {
                downloadVideo(video);
            }
            if (!Global.Download.Dispatcher.Working)
                Global.Download.Dispatcher.BeginWork();
            setDownloadStatus();
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }


        public void setDownloadStatus()
        {
            if (!CheckingDownloadStatus)
            {
                CheckingDownloadStatus = true;
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (vieModel.DownLoadTasks.All(arg =>
                         arg.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                         arg.Status == System.Threading.Tasks.TaskStatus.RanToCompletion
                        ))
                        {
                            vieModel.DownloadStatus = "Complete";
                            CheckingDownloadStatus = false;
                            break;
                        }
                        else
                        {
                            Task.Delay(1000).Wait();
                        }

                    }
                });
            }
        }



        private void EditActress(object sender, MouseButtonEventArgs e)
        {
            vieModel.EnableEditActress = !vieModel.EnableEditActress;

        }


        // todo
        private void BeginDownLoadActress(object sender, MouseButtonEventArgs e)
        {
            //List<Actress> actresses = new List<Actress>();
            //actresses.Add(vieModel.Actress);
            //DownLoadActress downLoadActress = new DownLoadActress(actresses);
            //downLoadActress.BeginDownLoad();
            //downLoadActress.InfoUpdate += (s, ev) =>
            //{
            //    ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
            //    try
            //    {
            //        Dispatcher.Invoke((Action)delegate ()
            //        {
            //            vieModel.Actress = null;
            //            vieModel.Actress = actressUpdateEventArgs.Actress;
            //            downLoadActress.State = DownLoadState.Completed;
            //        });
            //    }
            //    catch (TaskCanceledException ex) { Logger.LogE(ex); }

            //};

            //downLoadActress.MessageCallBack += (s, ev) =>
            //{
            //    MessageCallBackEventArgs actressUpdateEventArgs = ev as MessageCallBackEventArgs;
            //    msgCard.Info(actressUpdateEventArgs.Message);

            //};


        }





        private void SetConfigValue()
        {
            GlobalConfig.Main.X = this.Left;
            GlobalConfig.Main.Y = this.Top;
            GlobalConfig.Main.Width = this.Width;
            GlobalConfig.Main.Height = this.Height;
            GlobalConfig.Main.WindowState = (long)baseWindowState;
            GlobalConfig.Main.SearchSelectedIndex = vieModel.SearchSelectedIndex;
            GlobalConfig.Main.ClassifySelectedIndex = vieModel.ClassifySelectedIndex;
            GlobalConfig.Main.SideGridWidth = SideGridColumn.ActualWidth;

            GlobalConfig.Main.Save();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Minimized)
            {
                if (this.WindowState == System.Windows.WindowState.Normal) baseWindowState = BaseWindowState.Normal;
                else if (this.WindowState == System.Windows.WindowState.Maximized) baseWindowState = BaseWindowState.FullScreen;
                else if (this.Width == SystemParameters.WorkArea.Width & this.Height == SystemParameters.WorkArea.Height) baseWindowState = BaseWindowState.Maximized;

            }
            SetConfigValue();
            Properties.Settings.Default.EditMode = false;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();
            GlobalConfig.Main?.Save();
            GlobalConfig.Settings?.Save();


            if (!IsToUpdate && GlobalConfig.Settings.CloseToTaskBar && this.IsVisible == true)
            {
                SetWindowVisualStatus(false);
                e.Cancel = true;
            }


        }



        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }









        // todo
        public void DownLoadSelectedActor(object sender, RoutedEventArgs e)
        {
            //if (downLoadActress?.State == DownLoadState.DownLoading)
            //{
            //    msgCard.Info(Jvedio.Language.Resources.Message_WaitForDownload); return;
            //}

            //if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            //StackPanel sp = null;
            //if (sender is MenuItem mnu)
            //{
            //    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            //    string name = sp.Tag.ToString();
            //    Actress CurrentActress = GetActressFromVieModel(name);
            //    if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
            //    StartDownLoadActor(SelectedActress);

            //}
            //if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
        }


        public void SelectAllActor(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ActorEditMode = true;
            bool allContain = true;// 检测是否取消选中
            foreach (var item in vieModel.CurrentActorList)
            {
                if (!vieModel.SelectedActors.Contains(item))
                {
                    vieModel.SelectedActors.Add(item);
                    allContain = false;
                }
            }

            if (allContain)
                vieModel.SelectedActors.RemoveMany(vieModel.CurrentActorList);
            ActorSetSelected();


        }


        private long getDataID(UIElement o)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element == null) return -1;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (grid != null && grid.Tag != null)
            {
                long.TryParse(grid.Tag.ToString(), out long result);
                return result;
            }
            return -1;
        }

        // todo 库界面双击会导致提前播放视频触发异常(VideoList 未初始化)
        private Video getVideo(long dataID)
        {
            if (dataID <= 0 || vieModel?.VideoList?.Count <= 0) return null;
            Video video = vieModel.VideoList.Where(item => item.DataID == dataID).First();
            if (video != null && video.DataID > 0) return video;
            return null;
        }
        private Video getAssoVideo(long dataID)
        {
            if (dataID <= 0 || vieModel?.ViewAssociationDatas?.Count <= 0) return null;
            Video video = vieModel.ViewAssociationDatas.Where(item => item.DataID == dataID).First();
            if (video != null && video.DataID > 0) return video;
            return null;
        }


        public void ShowSubSection(object sender, RoutedEventArgs e)
        {
            if (vieModel.VideoList == null || vieModel.VideoList.Count <= 0) return;
            Button button = sender as Button;
            long dataID = getDataID(button);
            if (dataID <= 0) return;
            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();

            Video video = vieModel.VideoList.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video != null && video.SubSectionList?.Count > 0)
            {
                for (int i = 0; i < video.SubSectionList.Count; i++)
                {
                    string filepath = video.SubSectionList[i];//这样可以，放在  PlayVideoWithPlayer 就超出索引
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = i + 1;
                    menuItem.Click += (s, _) =>
                    {
                        PlayVideoWithPlayer(filepath, dataID);
                    };
                    contextMenu.Items.Add(menuItem);
                }

                contextMenu.IsOpen = true;
            }


        }

        public void ShowAssoSubSection(object sender, RoutedEventArgs e)
        {

            if (vieModel.ViewAssociationDatas == null) return;
            Button button = sender as Button;
            long dataID = getDataID(button);
            if (dataID <= 0) return;

            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();

            Video video = vieModel.ViewAssociationDatas.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video != null && video.SubSectionList?.Count > 0)
            {
                for (int i = 0; i < video.SubSectionList.Count; i++)
                {
                    string filepath = video.SubSectionList[i];//这样可以，放在  PlayVideoWithPlayer 就超出索引
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = i + 1;
                    menuItem.Click += (s, _) =>
                    {
                        PlayVideoWithPlayer(filepath, dataID);
                    };
                    contextMenu.Items.Add(menuItem);
                }
                contextMenu.IsOpen = true;
            }


        }



        private void Grid_Drop(object sender, DragEventArgs e)
        {

            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddScanTask(dragdropFiles);
            dragOverBorder.Visibility = Visibility.Collapsed;
        }


        private void AddScanTask(string[] toScanfiles)
        {
            vieModel.ScanStatus = "Scanning";

            List<string> files = new List<string>();
            List<string> paths = new List<string>();

            foreach (var item in toScanfiles)
            {
                if (FileHelper.IsFile(item))
                    files.Add(item);
                else
                    paths.Add(item);
            }
            Core.Scan.ScanTask scanTask = new Core.Scan.ScanTask(paths, files);

            scanTask.onCanceled += (s, ev) =>
            {
                //msgCard.Warning("取消扫描任务");
            };
            scanTask.onError += (s, ev) =>
            {
                msgCard.Error((ev as MessageCallBackEventArgs).Message);
            };
            scanTask.onCompleted += (s, ev) =>
            {
                Dispatcher.Invoke(() =>
                {
                    vieModel.Statistic();
                    if (vieModel.CurrentVideoList?.Count <= 0)
                        vieModel.LoadData();
                });
                (s as ScanTask).Running = false;
            };
            vieModel.ScanTasks.Add(scanTask);
            scanTask.Start();
            setScanStatus();
        }


        private void setScanStatus()
        {
            if (!CheckingScanStatus)
            {
                CheckingScanStatus = true;
                Task.Run(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("检查状态");
                        if (vieModel.ScanTasks.All(arg =>
                         arg.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                         arg.Status == System.Threading.Tasks.TaskStatus.RanToCompletion
                        ))
                        {
                            vieModel.ScanStatus = "Complete";
                            CheckingScanStatus = false;
                            break;
                        }
                        else
                        {
                            Task.Delay(1000).Wait();
                        }

                    }
                });
            }
        }



        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
            dragOverBorder.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            dragOverBorder.Visibility = Visibility.Collapsed;
        }







        public void ShowSettingsPopup(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
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
            wrapper.Eq("DBId", GlobalConfig.Main.CurrentDBId).Eq("DataType", "0");
            metaDataMapper.updateField("ViewDate", "", wrapper);
            vieModel.Statistic();
        }

        private void ConfigFirstRun()
        {
            if (GlobalConfig.Main.FirstRun)
            {
                vieModel.ShowFirstRun = Visibility.Visible;
                GlobalConfig.Main.FirstRun = false;
            }
        }





        // todo 更改皮肤
        public void SetSkin()
        {
            ThemeHelper.SetSkin(Properties.Settings.Default.Themes);
            //SettingsBorder.ContextMenu.UpdateDefaultStyle();//设置弹出的菜单正确显示
            //switch (Properties.Settings.Default.Themes)
            //{
            //    case "蓝色":
            //        //设置渐变
            //        LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
            //        myLinearGradientBrush.StartPoint = new Point(0.5, 0);
            //        myLinearGradientBrush.EndPoint = new Point(0.5, 1);
            //        myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 1));
            //        myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 0));
            //        //SideBorder.Background = myLinearGradientBrush;
            //        break;

            //    default:
            //        //SideBorder.Background = (SolidColorBrush)Application.Current.Resources["Window.Side.Background"];
            //        break;
            //}

            //if (Properties.Settings.Default.EnableBgImage && GlobalVariable.BackgroundImage != null)
            //{
            //    SideBorder.Background = Brushes.Transparent;
            //    DatabaseComboBox.Background = (SolidColorBrush)Application.Current.Resources["Window.Side.Opacity.Background"];
            //    TitleBorder.Background = Brushes.Transparent;
            //    //MainProgressBar.Background = Brushes.Transparent;
            //    //ActorProgressBar.Background = Brushes.Transparent;
            //    //foreach (Expander expander in ExpanderStackPanel.Children.OfType<Expander>().ToList())
            //    //{
            //    //    expander.Background = Brushes.Transparent;
            //    //    Border border = expander.Content as Border;
            //    //    border.Background = Brushes.Transparent;
            //    //}
            //    BgImage.Source = GlobalVariable.BackgroundImage;
            //}
            //else
            //{

            //    TitleBorder.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
            //    DatabaseComboBox.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
            //    SideBorder.SetResourceReference(Control.BackgroundProperty, "Window.Side.Background");
            //    //MainProgressBar.SetResourceReference(Control.BackgroundProperty, "Window.Side.Background");
            //    //ActorProgressBar.SetResourceReference(Control.BackgroundProperty, "Window.Side.Background");
            //    //foreach (Expander expander in ExpanderStackPanel.Children.OfType<Expander>().ToList())
            //    //{
            //    //    expander.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
            //    //    Border border = expander.Content as Border;
            //    //    border.SetResourceReference(Control.BackgroundProperty, "Window.Background");
            //    //}
            //}
            ////设置背景图片
            //BgImage.Source = GlobalVariable.BackgroundImage;


        }


        // todo
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
            {
                //高级检索
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right)
            {
                //末页
                if (vieModel.TabSelectedIndex == 0)
                {
                    vieModel.CurrentPage = vieModel.TotalPage;
                    //vieModel.AsyncFlipOver();
                    SetSelected();
                }
                else
                {
                    //vieModel.CurrentActorPage = vieModel.TotalActorPage;
                    //vieModel.ActorFlipOver();
                    ActorSetSelected();
                }

            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left)
            {
                //首页
                if (vieModel.TabSelectedIndex == 0)
                {
                    vieModel.CurrentPage = 1;
                    //vieModel.AsyncFlipOver();
                    SetSelected();
                }

                else
                {
                    vieModel.CurrentActorPage = 1;
                    //vieModel.ActorFlipOver();
                    ActorSetSelected();
                }

            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Up)
            {
                //回到顶部
                //ScrollViewer.ScrollToTop();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Down)
            {
                //滑倒底端

            }
            //else if (vieModel.TabSelectedIndex == 0 && e.Key == Key.Right)
            //    NextPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            //else if (vieModel.TabSelectedIndex == 0 && e.Key == Key.Left)
            //    PreviousPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            //else if (vieModel.TabSelectedIndex == 1 && e.Key == Key.Right)
            //    NextActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            //else if (vieModel.TabSelectedIndex == 1 && e.Key == Key.Left)
            //    PreviousActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));




        }


        //todo DatabaseComboBox_SelectionChanged

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            //AppDatabase database = 
            vieModel.CurrentAppDataBase = (AppDatabase)e.AddedItems[0];
            GlobalConfig.Main.CurrentDBId = vieModel.CurrentAppDataBase.DBId;
            GlobalConfig.Settings.DefaultDBID = vieModel.CurrentAppDataBase.DBId;

            //切换数据库

            vieModel.IsRefresh = true;
            vieModel.Statistic();
            vieModel.Reset();
            vieModel.InitCurrentTagStamps();

            //vieModel.InitLettersNavigation();
            //vieModel.GetFilterInfo();
            AllRadioButton.IsChecked = true;



        }



        private void RandomDisplay(object sender, MouseButtonEventArgs e)
        {
            vieModel.RandomDisplay();
        }

        private void ShowFilterGrid(object sender, MouseButtonEventArgs e)
        {

            if (windowFilter == null) windowFilter = new Window_Filter();
            windowFilter.Show();
            windowFilter.BringIntoView();
            windowFilter.Activate();
        }



        private void SetSelectMode(object sender, RoutedEventArgs e)
        {
            ChaoControls.Style.Switch s = sender as ChaoControls.Style.Switch;
            vieModel.SelectedVideo.Clear();
            SetSelected();
        }
        private void SetActorSelectMode(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedActors.Clear();
            ActorSetSelected();
        }

        private void TopBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                MaxWindow(sender, new RoutedEventArgs());

            }
        }

        public void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteInfo);
                if (menuItem != null) DeleteID(menuItem, new RoutedEventArgs());
            }
            else if (e.Key == Key.T)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteFile);
                if (menuItem != null) DeleteFile(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.S)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_SyncInfo);
                if (menuItem != null) DownLoadSelectMovie(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.E)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_EditInfo);
                if (menuItem != null) EditInfo(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.W)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_OpenWebSite);
                if (menuItem != null) OpenWeb(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.C)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_CopyFile);
                if (menuItem != null) CopyFile(menuItem, new RoutedEventArgs());

            }
            contextMenu.IsOpen = false;
        }


        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            if (contextMenu == null || string.IsNullOrEmpty(header)) return null;
            foreach (FrameworkElement element in contextMenu.Items)
            {
                if (element is MenuItem item && item.Header.ToString().Equals(header))
                    return item;
            }
            return null;
        }


        private void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Properties.Settings.Default.ShowImageMode == "0")
            {
                Properties.Settings.Default.SmallImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.SmallImage_Height = (int)((double)Properties.Settings.Default.SmallImage_Width * (200 / 147));

            }
            else if (Properties.Settings.Default.ShowImageMode == "1")
            {
                Properties.Settings.Default.BigImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.BigImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
            //else if (Properties.Settings.Default.ShowImageMode == "2")
            //{
            //    Properties.Settings.Default.ExtraImage_Width = Properties.Settings.Default.GlobalImageWidth;
            //    Properties.Settings.Default.ExtraImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            //}
            else if (Properties.Settings.Default.ShowImageMode == "2")
            {
                Properties.Settings.Default.GifImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.GifImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
            Properties.Settings.Default.Save();
        }


        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (!CanRateChange) return;
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            if (rate == null) return;
            StackPanel stackPanel = rate.Parent as StackPanel;
            long id = getDataID(stackPanel);
            if (id <= 0) return;
            metaDataMapper.updateFieldById("Grade", rate.Value.ToString(), id);
            vieModel.Statistic();
            CanRateChange = false;
        }

        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }






        private void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {



        }

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            //string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            //string file = dragdropFiles[0];

            //if (IsFile(file))
            //{
            //    FileInfo fileInfo = new FileInfo(file);
            //    if (fileInfo.Extension.ToLower() == ".jpg")
            //    {
            //        FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{vieModel.Actress.name}.jpg", true);
            //        Actress actress = vieModel.Actress;
            //        actress.smallimage = null;
            //        actress.smallimage = GetActorImage(actress.name);
            //        vieModel.Actress = null;
            //        vieModel.Actress = actress;

            //        if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

            //        for (int i = 0; i < vieModel.ActorList.Count; i++)
            //        {
            //            if (vieModel.ActorList[i].name == actress.name)
            //            {
            //                vieModel.ActorList[i] = actress;
            //                break;
            //            }
            //        }

            //    }
            //    else
            //    {
            //        msgCard.Info(Jvedio.Language.Resources.Message_OnlySupportJPG);
            //    }
            //}
        }

        private void ActorImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            //    string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            //    string file = dragdropFiles[0];

            //    Image image = sender as Image;
            //    StackPanel stackPanel = image.Parent as StackPanel;
            //    TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            //    string name = textBox.Text.Split('(')[0];

            //    Actress currentActress = null;
            //    for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            //    {
            //        if (vieModel.CurrentActorList[i].name == name)
            //        {
            //            currentActress = vieModel.CurrentActorList[i];
            //            break;
            //        }
            //    }

            //    if (currentActress == null) return;


            //    if (IsFile(file))
            //    {
            //        FileInfo fileInfo = new FileInfo(file);
            //        if (fileInfo.Extension.ToLower() == ".jpg")
            //        {
            //            FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{currentActress.name}.jpg", true);
            //            Actress actress = currentActress;
            //            actress.smallimage = null;
            //            actress.smallimage = GetActorImage(actress.name);

            //            if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

            //            for (int i = 0; i < vieModel.ActorList.Count; i++)
            //            {
            //                if (vieModel.ActorList[i].name == actress.name)
            //                {
            //                    vieModel.ActorList[i] = null;
            //                    vieModel.ActorList[i] = actress;
            //                    break;
            //                }
            //            }

            //            for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            //            {
            //                if (vieModel.CurrentActorList[i].name == actress.name)
            //                {
            //                    vieModel.CurrentActorList[i] = null;
            //                    vieModel.CurrentActorList[i] = actress;
            //                    break;
            //                }
            //            }

            //        }
            //        else
            //        {
            //            msgCard.Info(Jvedio.Language.Resources.Message_OnlySupportJPG);
            //        }
            //    }
        }



        private void refreshTagStamp(ref Video video, long newTagID, bool deleted)
        {
            if (video == null || newTagID <= 0) return;
            string tagIDs = video.TagIDs;
            if (!deleted && string.IsNullOrEmpty(tagIDs))
            {
                video.TagStamp = new ObservableCollection<TagStamp>();
                video.TagStamp.Add(GlobalVariable.TagStamps.Where(arg => arg.TagID == newTagID).FirstOrDefault());
                video.TagIDs = newTagID.ToString();
            }
            else
            {
                List<string> list = tagIDs.Split(',').ToList();
                if (!deleted && !list.Contains(newTagID.ToString()))
                    list.Add(newTagID.ToString());
                if (deleted && list.Contains(newTagID.ToString()))
                    list.Remove(newTagID.ToString());
                video.TagIDs = string.Join(",", list);
                video.TagStamp = new ObservableCollection<TagStamp>();
                foreach (var arg in list)
                {
                    long.TryParse(arg, out long id);
                    video.TagStamp.Add(GlobalVariable.TagStamps.Where(item => item.TagID == id).FirstOrDefault());
                }
            }
        }



        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

            if (vieModel.IsLoadingMovie)
            {
                e.Handled = true;
                return;
            }


            // 标记
            GifImage gifImage = e.Source as GifImage;
            if (gifImage == null) return;
            long dataID = getDataID(gifImage);
            if (dataID <= 0) return;
            ContextMenu contextMenu = gifImage.ContextMenu;
            if (contextMenu == null) return;

            Video video = vieModel.CurrentVideoList.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video == null) return;
            List<string> tagIDs = new List<string>();
            if (!string.IsNullOrEmpty(video.TagIDs))
                tagIDs = video.TagIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (FrameworkElement item in contextMenu.Items)
            {
                if ("TagMenuItems".Equals(item.Name) && item is MenuItem menuItem)
                {
                    menuItem.Items.Clear();
                    GlobalVariable.TagStamps.ForEach(arg =>
                    {
                        string tagID = arg.TagID.ToString();
                        MenuItem menu = new MenuItem()
                        {
                            Header = arg.TagName,
                            IsCheckable = true,
                            IsChecked = tagIDs.Contains(tagID)
                        };
                        menu.Click += (s, ev) =>
                        {
                            long TagID = arg.TagID;
                            AddTagHandler(menu, TagID);
                        };
                        menuItem.Items.Add(menu);
                    });
                }
            }

        }


        private void AddTagHandler(object sender, long tagID)
        {
            handleMenuSelected(sender, 1);

            MenuItem menuItem = sender as MenuItem;
            bool deleted = false;
            if (menuItem != null) deleted = !menuItem.IsChecked;



            // 构造 sql 语句
            if (vieModel.SelectedVideo?.Count <= 0) return;

            if (deleted)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var item in vieModel.SelectedVideo)
                {

                    builder.Append($"delete from metadata_to_tagstamp where DataID={item.DataID} and TagID={tagID};");

                }
                string sql = "begin;" + builder.ToString() + "commit;";
                tagStampMapper.executeNonQuery(sql);
            }
            else
            {
                List<string> values = new List<string>();
                foreach (var item in vieModel.SelectedVideo)
                {
                    values.Add($"({item.DataID},{tagID})");
                }
                if (values.Count <= 0) return;
                string sql = $"insert or replace into metadata_to_tagstamp (DataID,TagID)  values {(string.Join(",", values))}";
                tagStampMapper.executeNonQuery(sql);
            }
            initTagStamp();

            // 更新主界面
            ObservableCollection<Video> datas = vieModel.CurrentVideoList;
            if (AssoDataPopup.IsOpen) datas = vieModel.ViewAssociationDatas;




            foreach (var item in vieModel.SelectedVideo)
            {
                long dataID = item.DataID;
                if (dataID <= 0 || tagID <= 0 || datas == null || datas.Count == 0) continue;
                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i].DataID == dataID)
                    {
                        Video video = datas[i];
                        refreshTagStamp(ref video, tagID, deleted);
                        datas[i] = null;
                        datas[i] = video;
                        break;
                    }
                }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();



        }






        private void ActorRate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            actorMapper.updateFieldById("Grade", rate.Value.ToString(), vieModel.CurrentActorInfo.ActorID);
        }


        private void HideActressGrid(object sender, MouseButtonEventArgs e)
        {
            var anim = new DoubleAnimation(1, 0, (Duration)FadeInterval, FillBehavior.Stop);
            anim.Completed += (s, _) => vieModel.ShowActorGrid = Visibility.Collapsed; ;
            ActorInfoGrid.BeginAnimation(UIElement.OpacityProperty, anim);

        }









        private void ClearActressInfo(object sender, RoutedEventArgs e)
        {


        }


        private void ClassifyTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vieModel.SetClassify();
            Console.WriteLine("ClassifyTabControl_SelectionChanged");
        }


        private void HideBeginScanGrid(object sender, RoutedEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
        }

        private void OpenActorPath(object sender, RoutedEventArgs e)
        {
            ActorInfo info = vieModel.CurrentActorInfo;
            if (info != null)
            {
                FileHelper.TryOpenSelectPath(info.getImagePath());
            }

        }

        private void OpenWebsite(object sender, RoutedEventArgs e)
        {



        }




        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            try
            {
                scan_cts?.Cancel();
            }
            catch (ObjectDisposedException ex) { Console.WriteLine(ex.Message); }

        }

        private void NavigationToLetter(object sender, RoutedEventArgs e)
        {
            //vieModel.SearchFirstLetter = true;
            //vieModel.Search = ((Button)sender).Content.ToString();

        }



        private void CopyText(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ClipBoard.TrySetDataObject(textBlock.Text);
        }




        private void InitLoadSearch(string notice)
        {
            LoadSearchWaitingPanel.Visibility = Visibility.Visible;
            LoadSearchWaitingPanel.ShowProgressBar = Visibility.Collapsed;
            LoadSearchWaitingPanel.NoticeText = notice;
            LoadSearchWaitingPanel.ShowCancelButton = Visibility.Collapsed;
            LoadSearchWaitingPanel.NoticeExtraText = "";
            LoadSearchWaitingPanel.ShowExtraText = Visibility.Collapsed;
            LoadSearchCTS = new CancellationTokenSource();
            LoadSearchCTS.Token.Register(() => { Console.WriteLine("取消任务"); this.Cursor = Cursors.Arrow; });
            LoadSearchCT = LoadSearchCTS.Token;
        }

        private void LoadActorOtherMovie(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            string name = border.Tag.ToString();
            InitLoadSearch(Jvedio.Language.Resources.SearchActor);
            //LoadActor(name);
        }

        private void CancelLoadActor(object sender, RoutedEventArgs e)
        {
            LoadSearchWaitingPanel.Visibility = Visibility.Hidden;

            try
            {
                LoadSearchCTS.Cancel();
                LoadSearchCTS?.Dispose();
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        private void ShowContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
            e.Handled = true;
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++)
            {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem)
                {
                    item.IsChecked = true;
                    if (i.ToString().Equals(Properties.Settings.Default.SortType))
                    {
                        Properties.Settings.Default.SortDescending = !Properties.Settings.Default.SortDescending;
                    }
                    Properties.Settings.Default.SortType = i.ToString();

                }
                else item.IsChecked = false;
            }
            vieModel.Reset();
        }

        private void ActorSortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++)
            {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem)
                {
                    item.IsChecked = true;
                    if (i == Properties.Settings.Default.ActorSortType)
                    {
                        Properties.Settings.Default.ActorSortDescending = !Properties.Settings.Default.ActorSortDescending;
                    }
                    Properties.Settings.Default.ActorSortType = i;

                }
                else item.IsChecked = false;
            }
            vieModel.LoadActor();
        }





        private void Pagination_CurrentPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentPage = pagination.CurrentPage;
            VieModel_Main.pageQueue.Enqueue(pagination.CurrentPage);
            vieModel.LoadData();
        }



        private void CurrentActorPageChange(object sender, EventArgs e)
        {

            Pagination pagination = sender as Pagination;
            vieModel.CurrentActorPage = pagination.CurrentPage;
            VieModel_Main.ActorPageQueue.Enqueue(pagination.CurrentPage);
            vieModel.LoadActor();
        }

        private void ActorPageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.ActorPageSize = pagination.PageSize;

        }



        private void NewTagStamp(object sender, MouseButtonEventArgs e)
        {
            Window_TagStamp window_TagStamp = new Window_TagStamp();
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog)
            {
                string name = window_TagStamp.TagName;
                if (string.IsNullOrEmpty(name)) return;
                SolidColorBrush BackgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;

                TagStamp tagStamp = new TagStamp()
                {
                    TagName = name,
                    Foreground = VisualHelper.SerilizeBrush(ForegroundBrush),
                    Background = VisualHelper.SerilizeBrush(BackgroundBrush),
                };
                tagStampMapper.insert(tagStamp);
                initTagStamp();
            }
        }

        private void EditTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string tag = (contextMenu.PlacementTarget as PathCheckButton).Tag.ToString();
            long.TryParse(tag, out long id);
            if (id <= 0) return;

            TagStamp tagStamp = GlobalVariable.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();
            Window_TagStamp window_TagStamp = new Window_TagStamp(tagStamp.TagName, tagStamp.BackgroundBrush, tagStamp.ForegroundBrush);
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog)
            {
                string name = window_TagStamp.TagName;
                if (string.IsNullOrEmpty(name)) return;
                SolidColorBrush BackgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;
                tagStamp.TagName = name;
                tagStamp.Background = VisualHelper.SerilizeBrush(BackgroundBrush);
                tagStamp.Foreground = VisualHelper.SerilizeBrush(ForegroundBrush);
                tagStampMapper.updateById(tagStamp);
                initTagStamp();
            }
        }

        private void DeleteTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string tag = (contextMenu.PlacementTarget as PathCheckButton).Tag.ToString();
            long.TryParse(tag, out long id);
            if (id <= 0) return;
            TagStamp tagStamp = GlobalVariable.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();
            if (tagStamp.TagID == 1 || tagStamp.TagID == 2)
            {
                msgCard.Error("默认标记不可删除");
                return;
            }
            if (new Msgbox(this, Jvedio.Language.Resources.IsToDelete + $"标记 【{tagStamp.TagName}】").ShowDialog() == true)
            {

                tagStampMapper.deleteById(id);
                // 删除
                string sql = $"delete from metadata_to_tagstamp where TagID={tagStamp.TagID};";
                tagStampMapper.executeNonQuery(sql);
                initTagStamp();

                // 更新主窗体
                if (vieModel.CurrentVideoList != null)
                {
                    for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
                    {
                        if (vieModel.CurrentVideoList[i].TagStamp != null
                            && vieModel.CurrentVideoList[i].TagStamp.Contains(tagStamp))
                        {
                            vieModel.CurrentVideoList[i].TagStamp.Remove(tagStamp);
                        }
                    }

                }
                // todo 更新详情窗口
            }

        }



        private void NewList(object sender, MouseButtonEventArgs e)
        {
        }

        public void RefreshGrade(Video newVideo)
        {
            if (newVideo == null || vieModel.CurrentVideoList == null || vieModel.CurrentVideoList.Count <= 0) return;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            {
                if (vieModel.CurrentVideoList[i]?.DataID == newVideo.DataID)
                {
                    Video video = vieModel.CurrentVideoList[i];
                    vieModel.CurrentVideoList[i] = null;
                    video.Grade = newVideo.Grade;
                    vieModel.CurrentVideoList[i] = video;
                    vieModel.Statistic();
                }
            }
        }
        public void RefreshImage(Video newVideo)
        {
            if (newVideo == null || newVideo.DataID <= 0 || vieModel.CurrentVideoList?.Count <= 0) return;
            long dataid = newVideo.DataID;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            {
                if (vieModel.CurrentVideoList[i]?.DataID == dataid)
                {

                    Video video = videoMapper.selectOne(new SelectWrapper<Video>().Eq("DataID", dataid));
                    if (video == null) continue;
                    SetImage(ref video);
                    vieModel.CurrentVideoList[i].SmallImage = null;
                    vieModel.CurrentVideoList[i].BigImage = null;
                    vieModel.CurrentVideoList[i].SmallImage = video.SmallImage;
                    vieModel.CurrentVideoList[i].BigImage = video.BigImage;
                    break;
                }

            }
        }
        public void RefreshData(long dataID)
        {
            if (vieModel.CurrentVideoList?.Count <= 0) return;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            {
                if (vieModel.CurrentVideoList[i]?.DataID == dataID)
                {

                    Video video = videoMapper.SelectVideoByID(dataID);
                    if (video == null) continue;
                    SetImage(ref video);
                    Video.setTagStamps(ref video);// 设置标签戳
                    Video.handleEmpty(ref video);// 设置标题和发行日期
                                                 // 设置关联
                    HashSet<long> set = associationMapper.getAssociationDatas(dataID);
                    if (set != null)
                    {
                        video.HasAssociation = set.Count > 0;
                        video.AssociationList = set.ToList();
                    }




                    vieModel.CurrentVideoList[i].SmallImage = null;
                    vieModel.CurrentVideoList[i].BigImage = null;
                    vieModel.CurrentVideoList[i] = null;

                    vieModel.CurrentVideoList[i] = video;
                    vieModel.CurrentVideoList[i].SmallImage = video.SmallImage;
                    vieModel.CurrentVideoList[i].BigImage = video.BigImage;


                    if (GlobalConfig.Settings.AutoGenScreenShot)
                    {
                        if (vieModel.CurrentVideoList[i].BigImage == GlobalVariable.DefaultBigImage)
                        {
                            // 检查有无截图
                            string path = video.getScreenShot();
                            if (Directory.Exists(path))
                            {
                                string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                                if (array.Length > 0)
                                {

                                    Video.SetImage(ref video, array[array.Length / 2]);
                                    vieModel.CurrentVideoList[i].BigImage = null;
                                    vieModel.CurrentVideoList[i].BigImage = video.ViewImage;
                                }
                            }
                        }
                    }

                    break;
                }

            }

        }

        public void RefreshActor(long actorID)
        {

            if (vieModel.CurrentActorList?.Count <= 0) return;
            for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            {
                if (vieModel.CurrentActorList[i]?.ActorID == actorID)
                {
                    long count = vieModel.CurrentActorList[i].Count;
                    vieModel.CurrentActorList[i].SmallImage = null;
                    vieModel.CurrentActorList[i] = null;

                    SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
                    wrapper.Eq("ActorID", actorID);
                    ActorInfo actorInfo = actorMapper.selectById(wrapper);
                    if (actorInfo == null) continue;
                    ActorInfo.SetImage(ref actorInfo);
                    actorInfo.Count = count;

                    vieModel.CurrentActorList[i] = actorInfo;
                    vieModel.CurrentActorList[i].SmallImage = actorInfo.SmallImage;
                    break;
                }

            }


        }


        private async void doSearch(object sender, RoutedEventArgs e)
        {
            SearchMode mode = (SearchMode)vieModel.TabSelectedIndex;

            if (vieModel.TabSelectedIndex == 0)
            {
                vieModel.Searching = true;
                GlobalConfig.Main.SearchSelectedIndex = searchTabControl.SelectedIndex;
                await vieModel.Query((SearchField)searchTabControl.SelectedIndex);
                SaveSearchHistory(mode,
                (SearchField)searchTabControl.SelectedIndex);
            }
            else if (vieModel.TabSelectedIndex == 1)
            {
                // 搜寻演员
                vieModel.SearchingActor = true;
                vieModel.SelectActor();
                SaveSearchHistory(mode, 0);
            }
            else if (vieModel.TabSelectedIndex == 2)
            {
                // 搜寻标签
                vieModel.GetLabelList();
                SaveSearchHistory(mode, 0);
            }
            else if (vieModel.TabSelectedIndex == 3)
            {
                // 搜寻分类
                vieModel.SetClassify(true);
                SaveSearchHistory(mode, (SearchField)vieModel.ClassifySelectedIndex);
            }

            vieModel.Searching = false;


        }


        private void SaveSearchHistory(SearchMode mode, SearchField field)
        {
            string searchValue = vieModel.SearchText.ToProperSql();
            if (string.IsNullOrEmpty(searchValue)) return;
            SearchHistory history = new SearchHistory()
            {
                SearchMode = mode,
                SearchValue = searchValue,
                CreateDate = DateHelper.Now(),
                SearchField = field,
                CreateYear = DateTime.Now.Year,
                CreateMonth = DateTime.Now.Month,
                CreateDay = DateTime.Now.Day,
            };
            searchHistoryMapper.insert(history);
        }

        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                (searchTabControl.Items[(int)GlobalConfig.Main.SearchSelectedIndex] as TabItem).Focus();
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            vieModel.SearchText = (sender as ListBoxItem).Content.ToString();
            doSearch(null, null);
        }


        private void PathCheckButton_Click(object sender, RoutedEventArgs e)
        {
            // 获得当前所有标记状态
            vieModel.LoadData();
        }

        private void SideBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (SideGridColumn.ActualWidth <= 100)
            {
                SideGridColumn.Width = new GridLength(0);
                SideTriggerBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SideTriggerBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowSideGrid(object sender, MouseButtonEventArgs e)
        {
            SideGridColumn.Width = new GridLength(200);
            SideTriggerBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowMessage(object sender, MouseButtonEventArgs e)
        {
            msgPopup.IsOpen = true;
        }

        private void HideMsgPopup(object sender, MouseButtonEventArgs e)
        {
            msgPopup.IsOpen = false;
        }

        private void ClearMsg(object sender, MouseButtonEventArgs e)
        {
            vieModel.Message.Clear();
        }




        public void ShowSameActor(long actorID)
        {
            if (actorID <= 0) return;
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("actor_info.ActorID", actorID);
            vieModel.extraWrapper = wrapper;
            vieModel.ClickFilterType = "Actor";
            pagination.CurrentPageChange -= Pagination_CurrentPageChange;
            vieModel.CurrentPage = 1;
            vieModel.LoadData();
            ActorInfo actorInfo = actorMapper.selectOne(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            ActorInfo.SetImage(ref actorInfo);
            vieModel.CurrentActorInfo = actorInfo;
            vieModel.ShowActorGrid = Visibility.Visible;
            pagination.CurrentPageChange += Pagination_CurrentPageChange;

        }



        private void SideActorRate_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            HandyControl.Controls.Rate rate = sender as HandyControl.Controls.Rate;
            if (rate.Tag == null) return;
            long.TryParse(rate.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;
            actorMapper.updateFieldById("Grade", rate.Value.ToString(), actorID);

        }


        private void ShowMsgScanPopup(object sender, MouseButtonEventArgs e)
        {
            scanStatusPopup.IsOpen = true;

        }

        private void HideScanPopup(object sender, MouseButtonEventArgs e)
        {
            scanStatusPopup.IsOpen = false;
        }

        private void ClearScanTasks(object sender, MouseButtonEventArgs e)
        {

            for (int i = vieModel.ScanTasks.Count - 1; i >= 0; i--)
            {
                Core.Scan.ScanTask scanTask = vieModel.ScanTasks[i];
                if (scanTask.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                    scanTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion
                    )
                {
                    vieModel.ScanTasks.RemoveAt(i);
                }
            }
            vieModel.ScanStatus = "None";
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
            foreach (DownLoadTask task in vieModel.DownLoadTasks)
            {
                task.Cancel();
            }
        }
        private void CancelScreenShotTasks(object sender, RoutedEventArgs e)
        {
            foreach (ScreenShotTask task in vieModel.ScreenShotTasks)
            {
                task.Cancel();
            }
        }
        private void PauseDownloadTask(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            if (button.Content.ToString() == "暂停")
            {
                button.Content = "继续";
                task.Pause();
            }
            else
            {
                button.Content = "暂停";
            }

        }

        private void ShowScanDetail(object sender, RoutedEventArgs e)
        {
            string createTime = (sender as Button).Tag.ToString();
            ScanTask scanTask = vieModel.ScanTasks.Where(arg => arg.CreateTime.Equals(createTime)).FirstOrDefault();
            if (scanTask?.Status != System.Threading.Tasks.TaskStatus.Running)
            {
                Window_ScanDetail scanDetail = new Window_ScanDetail(scanTask.ScanResult);
                scanDetail.Show();
            }
        }

        private void ShowDownloadDetail(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            if (task == null) return;
            new Dialog_Logs(this, string.Join(Environment.NewLine, task.Logs)).ShowDialog();
        }
        private void ShowScreenShotDetail(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            ScreenShotTask task = vieModel.ScreenShotTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            if (task == null) return;
            new Dialog_Logs(this, string.Join(Environment.NewLine, task.Logs)).ShowDialog();
        }



        private void GoToStartUp(object sender, MouseButtonEventArgs e)
        {
            GlobalVariable.ClickGoBackToStartUp = true;
            SetWindowVisualStatus(false);// 隐藏所有窗体
            WindowStartUp windowStartUp = GetWindowByName("WindowStartUp") as WindowStartUp;
            if (windowStartUp == null) windowStartUp = new WindowStartUp();
            Application.Current.MainWindow = windowStartUp;
            windowStartUp.Show();
        }

        private void HideDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            downloadStatusPopup.IsOpen = false;
        }
        private void HideScreenShotPopup(object sender, MouseButtonEventArgs e)
        {
            screenShotStatusPopup.IsOpen = false;
        }

        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void RemoveCompleteTask(object sender, RoutedEventArgs e)
        {
            for (int i = vieModel.DownLoadTasks.Count - 1; i >= 0; i--)
            {
                if (vieModel.DownLoadTasks[i].Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                    vieModel.DownLoadTasks.RemoveAt(i);
                }
            }
            Global.Download.Dispatcher.ClearDoneList();
            if (vieModel.DownLoadTasks.Count == 0)
                vieModel.DownLoadVisibility = Visibility.Collapsed;
        }

        private void RemoveCancelTask(object sender, RoutedEventArgs e)
        {
            for (int i = vieModel.DownLoadTasks.Count - 1; i >= 0; i--)
            {
                if (vieModel.DownLoadTasks[i].Status == System.Threading.Tasks.TaskStatus.Canceled)
                {
                    vieModel.DownLoadTasks.RemoveAt(i);
                }
            }
            Global.Download.Dispatcher.ClearDoneList();
            if (vieModel.DownLoadTasks.Count == 0)
                vieModel.DownLoadVisibility = Visibility.Collapsed;
        }
        private void RemoveCompleteScreenShot(object sender, RoutedEventArgs e)
        {
            for (int i = vieModel.ScreenShotTasks.Count - 1; i >= 0; i--)
            {
                if (vieModel.ScreenShotTasks[i].Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                    vieModel.ScreenShotTasks.RemoveAt(i);
                }
            }
            Global.Download.Dispatcher.ClearDoneList();
            if (vieModel.ScreenShotTasks.Count == 0)
                vieModel.ScreenShotVisibility = Visibility.Collapsed;
        }

        private void RemoveCancelScreenShot(object sender, RoutedEventArgs e)
        {
            for (int i = vieModel.ScreenShotTasks.Count - 1; i >= 0; i--)
            {
                if (vieModel.ScreenShotTasks[i].Status == System.Threading.Tasks.TaskStatus.Canceled)
                {
                    vieModel.ScreenShotTasks.RemoveAt(i);
                }
            }
            Global.Download.Dispatcher.ClearDoneList();
            if (vieModel.ScreenShotTasks.Count == 0)
                vieModel.ScreenShotVisibility = Visibility.Collapsed;
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Video> datas = vieModel.CurrentVideoList;
            if (AssoDataPopup.IsOpen) datas = vieModel.ViewAssociationDatas;
            MenuItem menu = sender as MenuItem;
            if (menu == null) return;
            string header = menu.Header.ToString();
            long dataID = GetIDFromMenuItem(sender, 1);
            if (dataID <= 0) return;
            Video video = datas.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video == null) return;
            if (header.Equals(Jvedio.Language.Resources.Poster))
            {
                FileHelper.TryOpenSelectPath(video.getBigImage());
            }
            else if (header.Equals(Jvedio.Language.Resources.Thumbnail))
            {
                FileHelper.TryOpenSelectPath(video.getSmallImage());
            }
            else if (header.Equals(Jvedio.Language.Resources.Preview))
            {
                FileHelper.TryOpenSelectPath(video.getExtraImage());
            }
            else if (header.Equals(Jvedio.Language.Resources.ScreenShot))
            {
                FileHelper.TryOpenSelectPath(video.getScreenShot());
            }
            else if (header.Equals("GIF"))
            {
                FileHelper.TryOpenSelectPath(video.getGifPath());
            }



        }


        private void EditActor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;

            Window_EditActor window_EditActor = new Window_EditActor(actorID);
            window_EditActor.ShowDialog();
        }

        private void ShowSameActor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;
            ShowSameActor(actorID);
        }

        private void EditActor(object sender, MouseButtonEventArgs e)
        {
            Border button = sender as Border;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;

            Window_EditActor window_EditActor = new Window_EditActor(actorID);
            window_EditActor.ShowDialog();

        }

        private void ShowSameActor(object sender, MouseButtonEventArgs e)
        {

            Border button = sender as Border;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;
            ShowSameActor(actorID);
        }



        private void AddToPlayerList(object sender, RoutedEventArgs e)
        {



            string playerPath = Properties.Settings.Default.VedioPlayerPath;
            bool success = false;

            if (!File.Exists(playerPath))
            {
                msgCard.Error("未设置播放器路径！");
                return;
            }


            handleMenuSelected(sender);
            if (Path.GetFileName(playerPath).ToLower().Equals("PotPlayerMini64.exe".ToLower()))
            {
                List<string> list = vieModel.SelectedVideo
                    .Where(arg => File.Exists(arg.Path)).Select(arg => arg.Path).ToList();
                if (list.Count > 0)
                {
                    // potplayer 添加清单
                    string ProcessParameters = $"\"{playerPath}\" \"{string.Join("\" \"", list)}\" /add";
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = "cmd.exe";
                        //process.StartInfo.Arguments = arguments;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                        process.Start();
                        process.StandardInput.WriteLine(ProcessParameters);
                        process.StandardInput.AutoFlush = true;
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        //if (process.ExitCode != 0)
                        //    MessageCard.Error("添加失败");
                    }

                }
                success = true;
            }


            if (!success)
                MessageCard.Error("目前仅支持 potplayer，请设置 PotPlayerMini64.exe 所在路径");

        }

        private void OpenImageSavePath(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (!GlobalConfig.Settings.PicPaths.ContainsKey(pathType.ToString())) return;
            string basePicPath = GlobalConfig.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType == PathType.RelativeToApp)
                basePicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePicPath);
            basePicPath = Path.GetFullPath(basePicPath);
            FileHelper.TryOpenPath(basePicPath);
        }

        private void OpenLogPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log"));
        }

        private void OpenApplicationPath(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory);
        }

        private void ShowHelpTagStamp(object sender, MouseButtonEventArgs e)
        {
            msgCard.Info("用于添加带有颜色和文字的标记，显示在主页面或者详情页面");
        }

        private void NewActor(object sender, RoutedEventArgs e)
        {
            bool? success = new Window_EditActor(0).ShowDialog();
            if ((bool)success)
            {
                msgCard.Success("成功添加！");
                vieModel.Statistic();
            }
        }

        private void ShowActorNotice(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType.Equals(PathType.RelativeToData))
                msgCard.Info("由于当前图片资源文相对于影片，因此该页面不显示头像");
        }

        private void HideMsg(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Border border = (button.Parent as Grid).Parent as Border;
            border.Visibility = Visibility.Collapsed;
        }

        public void BorderMouseEnter(object sender, MouseEventArgs e)
        {
            if (Properties.Settings.Default.EditMode)
            {
                GifImage image = sender as GifImage;
                Grid grid = image.FindParentOfType<Grid>("rootGrid");
                Border border = grid.Children[0] as Border;
                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            }
        }

        public void BorderMouseLeave(object sender, MouseEventArgs e)
        {

            if (Properties.Settings.Default.EditMode)
            {
                GifImage image = sender as GifImage;
                long dataID = getDataID(image);
                Grid grid = image.FindParentOfType<Grid>("rootGrid");
                Border border = grid.Children[0] as Border;
                if (vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any())
                {
                    border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
                }
                else
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        private void ImportVideo(object sender, RoutedEventArgs e)
        {
            string path = FileHelper.SelectPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                AddScanTask(new string[] { path });
                MessageCard.Success("已添加扫描任务：=> " + path);
            }



        }

        private void ImportVideoByPaths(object sender, RoutedEventArgs e)
        {
            Window_SelectPaths window_SelectPaths = new Window_SelectPaths();
            if (window_SelectPaths.ShowDialog() == true)
            {
                List<string> folders = window_SelectPaths.Folders;
                if (folders.Count == 0)
                {
                    MessageCard.Warning("并未选择文件夹");
                }
                else
                {
                    AddScanTask(folders.ToArray());
                    MessageCard.Success($"已添加 {folders.Count} 个文件夹到扫描任务!");
                }
            }


        }

        private void DeleteNotExistVideo(object sender, RoutedEventArgs e)
        {
            if (vieModel.DownLoadTasks?.Count > 0 || vieModel.ScanTasks?.Count > 0 || vieModel.ScreenShotTasks?.Count > 0)
            {
                msgCard.Error("此操作需要清空下载任务、扫描任务、截图任务");
                return;
            }

            vieModel.RunningLongTask = true;
            Task.Run(async () =>
            {
                List<string> toDelete = new List<string>();
                SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
                wrapper.Select("DataID", "Path").Eq("DBId", GlobalConfig.Main.CurrentDBId).Eq("DataType", 0);
                List<MetaData> metaDatas = metaDataMapper.selectList(wrapper);
                if (metaDatas?.Count <= 0)
                {
                    vieModel.RunningLongTask = false;
                    return;
                }
                foreach (MetaData data in metaDatas)
                {
                    if (!File.Exists(data.Path)) toDelete.Add(data.DataID.ToString());
                }
                if (toDelete.Count <= 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        msgCard.Info("所有影片都存在，无需操作");
                    });
                    vieModel.RunningLongTask = false;
                    return;
                }
                MessageBoxResult messageBoxResult = MessageBox.Show($"确认从数据库删除 {toDelete.Count} 个不存在的影片", "提示", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    videoMapper.deleteVideoByIds(toDelete);
                    await Task.Delay(5000); // todo
                    vieModel.Statistic();
                    vieModel.LoadData();
                }
                vieModel.RunningLongTask = false;
            });
        }

        private void DeleteNotInScanPath(object sender, RoutedEventArgs e)
        {
            if (vieModel.DownLoadTasks?.Count > 0 || vieModel.ScanTasks?.Count > 0 || vieModel.ScreenShotTasks?.Count > 0)
            {
                msgCard.Error("此操作需要清空下载任务、扫描任务、截图任务");
                return;
            }



            string scanPath = vieModel.CurrentAppDataBase.ScanPath;
            if (string.IsNullOrEmpty(scanPath))
            {
                msgCard.Error("该库未设置目录");
                return;
            }

            List<string> scanPaths = JsonUtils.TryDeserializeObject<List<string>>(scanPath).Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            if (scanPaths == null || scanPaths.Count <= 0)
            {
                msgCard.Error("该库未设置目录");
                return;
            }

            vieModel.RunningLongTask = true;
            Task.Run(async () =>
            {
                List<string> toDelete = new List<string>();
                SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
                wrapper.Select("DataID", "Path").Eq("DBId", GlobalConfig.Main.CurrentDBId).Eq("DataType", 0);
                List<MetaData> metaDatas = metaDataMapper.selectList(wrapper);
                if (metaDatas?.Count <= 0)
                {
                    vieModel.RunningLongTask = false;
                    return;
                }
                foreach (MetaData data in metaDatas)
                {
                    string path = data.Path;
                    if (string.IsNullOrEmpty(path))
                    {
                        toDelete.Add(data.DataID.ToString());
                        continue;
                    }
                    foreach (string dir in scanPaths)
                    {
                        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(dir)) continue;
                        if (path.IndexOf(dir) < 0)
                        {
                            toDelete.Add(data.DataID.ToString());
                            break;
                        }
                    }
                }
                if (toDelete.Count <= 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        msgCard.Info("所有影片都存在，无需操作");
                    });
                    vieModel.RunningLongTask = false;
                    return;
                }
                MessageBoxResult messageBoxResult = MessageBox.Show($"确认从数据库删除 {toDelete.Count} 个不位于启动时扫描目录的影片", "提示", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    videoMapper.deleteVideoByIds(toDelete);
                    await Task.Delay(5000); // todo
                    vieModel.Statistic();
                    vieModel.LoadData();

                }
                vieModel.RunningLongTask = false;
            });


        }

        private void ExportToNFO(object sender, RoutedEventArgs e)
        {
            if (vieModel.DownLoadTasks.Count > 0 || vieModel.ScanTasks.Count > 0 || vieModel.ScreenShotTasks.Count > 0)
            {
                msgCard.Error("此操作需要清空下载任务、扫描任务、截图任务");
                return;
            }
        }

        private void ShowStatistic(object sender, RoutedEventArgs e)
        {

        }


        private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
        {


            MenuItem menuItem = sender as MenuItem;
            Label label = (menuItem.Parent as ContextMenu).PlacementTarget as Label;
            long.TryParse(label.Tag.ToString(), out long TagID);
            if (TagID <= 0) return;

            ItemsControl itemsControl = label.FindParentOfType<ItemsControl>();
            if (itemsControl == null || itemsControl.Tag == null) return;
            long.TryParse(itemsControl.Tag.ToString(), out long DataID);
            if (DataID <= 0) return;
            ObservableCollection<TagStamp> tagStamps = itemsControl.ItemsSource as ObservableCollection<TagStamp>;
            if (tagStamps == null) return;
            TagStamp tagStamp = tagStamps.Where(arg => arg.TagID.Equals(TagID)).FirstOrDefault();
            if (tagStamp != null)
            {
                tagStamps.Remove(tagStamp);
                string sql = $"delete from metadata_to_tagstamp where TagID='{TagID}' and DataID='{DataID}'";
                tagStampMapper.executeNonQuery(sql);

                ObservableCollection<Video> datas = vieModel.CurrentVideoList;
                if (AssoDataPopup.IsOpen) datas = vieModel.ViewAssociationDatas;

                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i].DataID.Equals(DataID))
                    {
                        Video video = videoMapper.SelectVideoByID(DataID);
                        if (video == null) continue;
                        datas[i].TagIDs = video.TagIDs;
                        break;
                    }
                }
                vieModel.InitCurrentTagStamps();
            }

        }

        private void CopyVID(object sender, MouseButtonEventArgs e)
        {
            string vid = (sender as Border).Tag.ToString();
            ClipBoard.TrySetDataObject(vid);
        }





        private void AddDataAssociation(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.EditMode = false;
            vieModel.SelectedVideo.Clear();
            SetSelected();
            long dataID = GetIDFromMenuItem(sender as MenuItem, 1);
            if (dataID <= 0) return;
            vieModel.LoadExistAssociationDatas(dataID);
            CurrentAssoDataID = dataID;
            searchDataBox.Text = "";
            vieModel.AssociationDatas?.Clear();
            vieModel.AssociationSelectedDatas?.Clear();
            vieModel.LoadAssoMetaData();
            searchDataPopup.IsOpen = true;
        }

        private void associationCancel(object sender, RoutedEventArgs e)
        {
            searchDataPopup.IsOpen = false;
        }

        private void associationConfirm(object sender, RoutedEventArgs e)
        {
            searchDataPopup.IsOpen = false;
            if (CurrentAssoDataID <= 0) return;
            List<long> toDelete = vieModel.SaveAssociation(CurrentAssoDataID);
            // 刷新关联的影片
            HashSet<long> set = associationMapper.getAssociationDatas(CurrentAssoDataID);
            set.Add(CurrentAssoDataID);
            foreach (var item in toDelete)
                set.Add(item);
            foreach (var item in set)
            {
                RefreshData(item);
            }
        }

        private void searchDataBox_Search(object sender, RoutedEventArgs e)
        {
            SearchBox box = sender as SearchBox;
            string searchText = box.Text;
            vieModel.AssoSearchText = searchText;
            vieModel.LoadAssoMetaData();
        }

        private void AssoSearchPageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.AssoSearchPageSize = pagination.PageSize;
            vieModel.LoadAssoMetaData();
        }

        private void AssoSearchPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentAssoSearchPage = pagination.CurrentPage;
            vieModel.LoadAssoMetaData();
        }

        private void AddToAssociation(object sender, MouseButtonEventArgs e)
        {
            long dataID = getDataID(sender as FrameworkElement);
            Video video = vieModel.AssociationDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (vieModel.ExistAssociationDatas.Contains(video) || dataID.Equals(CurrentAssoDataID))
                return;
            if (!vieModel.AssociationSelectedDatas.Contains(video)) vieModel.AssociationSelectedDatas.Add(video);
            else vieModel.AssociationSelectedDatas.Remove(video);
            SetAssoSelected();
        }

        private void SetAssoSelected()
        {
            ItemsControl itemsControl = assoSearchItemsControl;
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null) continue;
                Border border = FindElementByName<Border>(presenter, "rootBorder");
                if (border == null) continue;
                Grid grid = border.Parent as Grid;
                if (grid == null) continue;
                long dataID = getDataID(border);
                if (dataID > 0 && vieModel.AssociationSelectedDatas?.Count > 0)
                {
                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (vieModel.AssociationSelectedDatas.Where(arg => arg.DataID == dataID).Any())
                    {
                        border.Background = GlobalStyle.Common.HighLight.Background;
                        border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
                    }
                }

            }
        }

        public void AssoBorderMouseEnter(object sender, MouseEventArgs e)
        {

            GifImage image = sender as GifImage;
            Grid grid = image.FindParentOfType<Grid>("rootGrid");
            if (grid == null || grid.Children.Count <= 0) return;
            Border border = grid.Children[0] as Border;
            if (border != null)
                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;


        }

        public void AssoBorderMouseLeave(object sender, MouseEventArgs e)
        {


            GifImage image = sender as GifImage;
            if (image == null) return;
            long dataID = getDataID(image);
            Grid grid = image.FindParentOfType<Grid>("rootGrid");
            if (grid == null || grid.Children.Count <= 0) return;
            Border border = grid.Children[0] as Border;
            if (border == null || vieModel.AssociationSelectedDatas == null) return;
            if (vieModel.AssociationSelectedDatas.Where(arg => arg.DataID == dataID).Any())
                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            else
                border.BorderBrush = Brushes.Transparent;
        }

        private void RemoveAssociation(object sender, RoutedEventArgs e)
        {
            Grid grid = (sender as Button).Parent as Grid;
            if (grid == null || grid.Tag == null) return;
            long.TryParse(grid.Tag.ToString(), out long dataID);
            if (dataID <= 0) return;
            Video video = vieModel.AssociationSelectedDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (video != null)
            {
                vieModel.AssociationSelectedDatas.Remove(video);
                SetAssoSelected();
            }

        }
        private void RemoveExistAssociation(object sender, RoutedEventArgs e)
        {
            Grid grid = (sender as Button).Parent as Grid;
            if (grid == null || grid.Tag == null) return;
            long.TryParse(grid.Tag.ToString(), out long dataID);
            if (dataID <= 0) return;
            Video video = vieModel.ExistAssociationDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (video != null)
                vieModel.ExistAssociationDatas.Remove(video);



        }



        private void ViewAssoDatas(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = true;
            long dataID = getDataID(sender as FrameworkElement);
            if (dataID <= 0) return;
            vieModel.LoadViewAssoData(dataID);
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void HideAssoPopup(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
        }

        private void LoadData(object sender, RoutedEventArgs e)
        {
            vieModel.LoadData();
        }



        private void RestartTask(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            if (string.IsNullOrEmpty(dataID)) return;
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            task?.Restart();
            if (!Global.Download.Dispatcher.Working)
                Global.Download.Dispatcher.BeginWork();
        }

        private void ShowSponsor(object sender, RoutedEventArgs e)
        {
            // 检测
            string message = "请设置一个刮削网址后在尝试";
            if (GlobalConfig.ServerConfig.CrawlerServers != null &&
                GlobalConfig.ServerConfig.CrawlerServers.Count > 0
                )
            {
                bool found = false;

                foreach (var item in GlobalConfig.ServerConfig.CrawlerServers)
                {
                    if (!string.IsNullOrEmpty(item.Url))
                    {
                        found = true;
                        break;
                    }
                }


                if (found)
                {
                    new Dialog_Sponsor(this).ShowDialog();
                    return;
                }
            }
            msgCard.Info(message);
        }

        private void DeleteActors(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, "即将删除演员信息，是否继续？").ShowDialog() == true)
            {
                MenuItem mnu = sender as MenuItem;
                ContextMenu contextMenu = mnu.Parent as ContextMenu;
                //FrameworkElement image = contextMenu.PlacementTarget as FrameworkElement;
                long.TryParse(contextMenu.Tag.ToString(), out long actorID);
                if (actorID <= 0) return;

                if (!Properties.Settings.Default.ActorEditMode) vieModel.SelectedActors.Clear();
                ActorInfo actor = vieModel.CurrentActorList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
                if (!vieModel.SelectedActors.Where(arg => arg.ActorID == actorID).Any()) vieModel.SelectedActors.Add(actor);

                foreach (ActorInfo actorInfo in vieModel.SelectedActors)
                {
                    actorMapper.deleteById(actorInfo.ActorID);
                    string sql = $"delete from metadata_to_actor where metadata_to_actor.ActorID='{actorInfo.ActorID}'";
                    actorMapper.executeNonQuery(sql);
                }
                vieModel.SelectActor();
            }
        }

        private void OpenActorImagePath(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = mnu.Parent as ContextMenu;
            long.TryParse(contextMenu.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;
            ActorInfo actorInfo = actorMapper.selectById(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            string path = Path.GetFullPath(actorInfo.getImagePath());
            FileHelper.TryOpenSelectPath(path);
        }

        private void CopyTextBlock(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            TextBlock textBlock = contextMenu.PlacementTarget as TextBlock;
            if (textBlock != null)
            {
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
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null) continue;
                PathCheckButton button = FindElementByName<PathCheckButton>(presenter, "pathCheckButton");
                if (button == null) continue;
                button.IsChecked = allChecked;

            }
            vieModel.LoadData();
        }

        private void ShowThemes(object sender, RoutedEventArgs e)
        {
            themesPopup.IsOpen = true;
        }

        private void CloseThemePopup(object sender, RoutedEventArgs e)
        {
            themesPopup.IsOpen = false;
        }
    }

}
