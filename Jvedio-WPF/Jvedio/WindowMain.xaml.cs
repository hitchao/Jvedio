using ChaoControls.Style;
using DynamicData;
using HandyControl.Data;
using Jvedio.Entity;
using Jvedio.Utils;
using Jvedio.Utils.FileProcess;
using Jvedio.Utils.Net;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using static Jvedio.ImageProcess;
using static Jvedio.Utils.Visual.VisualHelper;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Jvedio.Utils.Sqlite;
using Jvedio.Utils.Visual;
using Jvedio.Core.Enums;
using Jvedio.Utils.Common;
using Jvedio.Core;
using Jvedio.Core.Scan;
using static Jvedio.Main.Msg;
using System.Diagnostics;
using Jvedio.Test;
using Jvedio.Core.Net;
using Jvedio.Core.CustomEventArgs;
using Jvedio.CommonNet;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.CustomTask;
using Jvedio.Mapper;
using Jvedio.CommonNet.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JvedioLib;
using Jvedio.Utils.Enums;
using Jvedio.Core.Crawler;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : ChaoControls.Style.BaseWindow
    {
        public bool Resizing = false;
        public DispatcherTimer ResizingTimer = new DispatcherTimer();

        public List<Actress> SelectedActress = new List<Actress>();

        public bool IsMouseDown = false;
        public Point MosueDownPoint;

        public bool CanRateChange = false;
        public bool IsToUpdate = false;

        public CancellationTokenSource RefreshScanCTS;
        public CancellationToken RefreshScanCT;

        public CancellationTokenSource LoadSearchCTS;
        public CancellationToken LoadSearchCT;

        public Settings WindowSet = null;
        public VieModel_Main vieModel;

        public SelectWrapper<Video> CurrentWrapper;
        public string CurrentSQL;

        public DetailMovie CurrentLabelMovie;
        public bool IsFlowing = false;


        public DispatcherTimer FlipoverTimer = new DispatcherTimer();

        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance = null;

        public WindowEdit WindowEdit;

        public int firstidx = -1;
        public int secondidx = -1;

        public int actorfirstidx = -1;
        public int actorsecondidx = -1;
        WindowDetails windowDetails;
        Window_LabelManagement labelManagement;

        public static Msg msgCard = new Msg();

        public static bool CheckingScanStatus = false;
        public static bool CheckingDownloadStatus = false;

        public static int NOTICE_INTERVAL = 1800;//30分钟检测一次

        public Main()
        {
            InitializeComponent();
            FilterGrid.Visibility = Visibility.Collapsed;
            vieModel = new VieModel_Main();
            this.DataContext = vieModel;
            BindingEvent();// 绑定控件事件

            // 初始化任务栏的进度条
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported) taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            AdjustWindow();// 还原窗口为上一次状态
            ConfigFirstRun();
            FadeIn();//淡入
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
            InitMovie();

            BindingEventAfterRender(); // render 后才绑定的事件
            initTagStamp();
            AllRadioButton.IsChecked = true;

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
            vieModel.initCurrentTagStamps();
        }

        private void BindingEventAfterRender()
        {
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
                vieModel.DownLoadProgress = Global.Download.Dispatcher.Progress;
                vieModel.DownLoadVisibility = Visibility.Visible;
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
                                HideAllWindow();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void HideAllWindow()
        {

            if (IsHide)
            {
                foreach (Window window in App.Current.Windows)
                {
                    if (OpeningWindows.Contains(window.GetType().ToString()))
                    {
                        window.Show();
                    }
                }
                IsHide = false;
                ShowMainWindow(this, null);
            }
            else
            {
                OpeningWindows.Clear();
                foreach (Window window in App.Current.Windows)
                {
                    window.Hide();
                    OpeningWindows.Add(window.GetType().ToString());
                }
                IsHide = true;

                //隐藏图标
                vieModel.HideToIcon = false;
            }


        }

        protected override void OnClosed(EventArgs e)
        {
            //Console.WriteLine("***************OnClosed***************");
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消热键
            vieModel.HideToIcon = false;//隐藏图标
            DisposeGif("", true);//清除 gif 资源
            NoticeTimer.Stop();
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

            //绑定演员
            foreach (StackPanel item in ActorInfoStackPanel.Children.OfType<StackPanel>().ToList())
            {
                TextBox textBox = item.Children[1] as TextBox;

                textBox.PreviewKeyUp += SaveActress;
            }




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

            //ActorSortBorder.ismouse

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

            //设置分类中的视频格式
            //var rbs2 = ClassifyVedioTypeStackPanel.Children.OfType<RadioButton>().ToList();
            //foreach (RadioButton item in rbs2)
            //{
            //    item.Click += SetTypeValue;
            //}

            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);


            vieModel.PageChangedCompleted += (s, ev) =>
            {
                // todo 需要引入 virtual wrapper，否则内存占用率一直很高，每页 40 个 => 1.3 G 左右
                if (Properties.Settings.Default.EditMode) SetSelected();

                if (GlobalConfig.Settings.AutoGenScreenShot)
                {
                    AutoGenScreenShot();
                }

            };

            vieModel.ActorPageChangedCompleted += (s, ev) =>
            {
                // todo 需要引入 virtual wrapper，否则内存占用率一直很高，每页 40 个 => 1.3 G 左右
                //GC.Collect();
                if (Properties.Settings.Default.ActorEditMode) ActorSetSelected();

                //vieModel.canRender = true;
            };

            vieModel.RenderSqlChanged += (s, ev) =>
            {
                WrapperEventArg<Video> arg = ev as WrapperEventArg<Video>;
                CurrentWrapper = arg.Wrapper as SelectWrapper<Video>;
                CurrentSQL = arg.SQL;

            };

            // 绑定消息
            msgCard.MsgShown += (s, ev) =>
            {
                MessageEventArg eventArg = ev as MessageEventArg;
                vieModel.Message.Add(eventArg.Message);
            };
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
                               where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem) return (childItem)child;
                childItem childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }


        // 自动判断有无图片，然后生成
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



        public void InitMovie()
        {
            Task.Run(() =>
            {
                vieModel.Reset();
                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                {
                    ItemsControl itemsControl;
                    //if (Properties.Settings.Default.EasyMode)
                    //    itemsControl = SimpleMovieItemsControl;
                    //else
                    itemsControl = MovieItemsControl;
                    //itemsControl.ItemsSource = vieModel.CurrentVideoList;
                    //itemsControl.ItemsSource = vieModel.CurrentVideoList;
                    //vieModel.GetFilterInfo(); // todo
                });

            });

        }





        public void SetLoadingStatus(bool loading)
        {
            vieModel.IsLoadingMovie = loading;
            IsFlowing = loading;
            vieModel.IsFlipOvering = loading;
            //SortStackPanel.IsEnabled = !loading;

        }







        private void ResizingTimer_Tick(object sender, EventArgs e)
        {
            Resizing = false;
            ResizingTimer.Stop();
        }




        public void Notify_Close(object sender, RoutedEventArgs e)
        {
            vieModel.HideToIcon = false;
            this.Close();
        }



        public void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            vieModel.HideToIcon = false;
            this.Show();
            //double opacity = Properties.Settings.Default.Opacity_Main >= 0.5 ? Properties.Settings.Default.Opacity_Main : 1;
            double opacity = 1;
            var anim = new DoubleAnimation(1, opacity, (Duration)FadeInterval, FillBehavior.Stop);
            anim.Completed += (s, _) => this.Opacity = opacity;
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }




        DispatcherTimer NoticeTimer = new DispatcherTimer();

        public void InitNotice()
        {
            NoticeTimer.Tick += (s, e) =>
            {
                ShowNotice();
            };
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
                if (httpResult.StatusCode == HttpStatusCode.OK && !httpResult.SourceCode.Equals(notices))
                {
                    //覆盖原有公告
                    string json = httpResult.SourceCode;
                    appConfig.ConfigValue = SqliteHelper.handleNewLine(httpResult.SourceCode);
                    appConfig.ConfigName = configName;
                    appConfigMapper.insert(appConfig, Core.Enums.InsertMode.Replace);

                    Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (dictionary != null && dictionary.ContainsKey("Date") && dictionary.ContainsKey("Data"))
                    {
                        string date = dictionary["Date"].ToString();
                        List<Dictionary<string, string>> Data = ((JArray)dictionary["Data"]).ToObject<List<Dictionary<string, string>>>();
                        if (Data != null && Data.Count > 0)
                        {
                            vieModel.Notices = new ObservableCollection<Notice>();
                            foreach (Dictionary<string, string> dict in Data)
                            {
                                if (dict.ContainsKey("Type") && dict.ContainsKey("Message"))
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
                }
                else
                {
                    Console.WriteLine("公告相同无需提示");
                }
            });
        }

        private string GetNoticeByLanguage(string notice, string language)
        {
            int start = -1;
            int end = -1;
            switch (language)
            {

                case "中文":
                    end = notice.IndexOf("--English--");
                    if (end == -1) return notice;
                    else return notice.Substring(0, end).Replace("--中文--", "");

                case "English":
                    start = notice.IndexOf("--English--");
                    end = notice.IndexOf("--日本語--");
                    if (end == -1 || start == -1) return notice;
                    else return notice.Substring(start, end - start).Replace("--English--", "");

                case "日本語":
                    start = notice.IndexOf("--日本語--");
                    if (start == -1) return notice;
                    else return notice.Substring(start).Replace("--日本語--", "");

                default:
                    return notice;
            }
        }



        //public DownLoader DownLoader;

        public void StartDownload(List<Movie> movieslist, bool force = false)
        {
            List<Movie> movies = new List<Movie>();
            List<Movie> moviesFC2 = new List<Movie>();
            if (movieslist != null)
            {
                foreach (var item in movieslist)
                {
                    if (item.id.IndexOf("FC2") >= 0) { moviesFC2.Add(item); } else { movies.Add(item); }
                }
            }
            double totalcount = moviesFC2.Count + movies.Count;
            Console.WriteLine(totalcount);
            if (totalcount == 0) return;

            //添加到下载列表
            //DownLoader?.CancelDownload();
            //DownLoader = new DownLoader(movies, moviesFC2, true);
            ////UI更新
            //DownLoader.InfoUpdate += (s, e) =>
            //{
            //    InfoUpdateEventArgs eventArgs = e as InfoUpdateEventArgs;
            //    try
            //    {
            //        try { Refresh(eventArgs, totalcount); }
            //        catch (TaskCanceledException ex) { Logger.LogE(ex); }
            //    }
            //    catch (Exception ex1)
            //    {
            //        Console.WriteLine(ex1.StackTrace);
            //        Console.WriteLine(ex1.Message);
            //    }
            //};

            ////信息显示
            //DownLoader.MessageCallBack += (s, e) =>
            //{
            //    MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
            //    if (eventArgs != null) msgCard.Error(eventArgs.Message);
            //};

            //DownLoader.StartThread();
        }



        public void CancelSelect()
        {
            Properties.Settings.Default.EditMode = false;
            vieModel.SelectedVideo.Clear();
            SetSelected();
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
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




        public void Refresh(InfoUpdateEventArgs eventArgs, double totalcount)
        {
            //  Dispatcher.Invoke((Action)delegate ()
            //{
            //    vieModel.ProgressBarValue = (int)(100 * eventArgs.progress / totalcount);
            //    vieModel.ProgressBarVisibility = Visibility.Visible;
            //    if (vieModel.ProgressBarValue == 100) { DownLoader.State = DownLoadState.Completed; vieModel.ProgressBarVisibility = Visibility.Hidden; }
            //    if (DownLoader.State == DownLoadState.Completed | DownLoader.State == DownLoadState.Fail) vieModel.ProgressBarVisibility = Visibility.Hidden;
            //    RefreshMovieByID(eventArgs.Movie.id);
            //});
        }





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

        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            {
                DataBase.DeleteByField("movie", "filepath", e.FullPath);
            }
            Console.WriteLine("成功删除" + e.FullPath);
        }



        // todo 监听文件变动
        public FileSystemWatcher[] fileSystemWatcher;
        public string failwatcherMessage = "";

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
            if (!IsToUpdate && GlobalConfig.Settings.CloseToTaskBar && this.IsVisible == true)
            {
                vieModel.HideToIcon = true;
                this.Hide();
                WindowSet?.Hide();
                WindowEdit?.Hide();
            }
            else
            {
                //结束程序
                StopDownLoad();
                SaveRecentWatched();
                vieModel.ProgressBarVisibility = Visibility.Hidden;
                //停止所有任务
                try
                {
                    RefreshScanCTS?.Cancel();
                    LoadSearchCTS?.Cancel();
                    scan_cts?.Cancel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }



                //WindowTools windowTools = null;
                //foreach (Window item in App.Current.Windows)
                //{
                //    if (item.GetType().Name == "WindowTools") windowTools = item as WindowTools;
                //}



                //if (windowTools?.IsVisible == true)
                //{
                //}
                //else
                //{
                //    System.Windows.Application.Current.Shutdown();
                //}


            }
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

        public void FadeIn()
        {
            //if (Properties.Settings.Default.EnableWindowFade)
            //{
            //    this.Show();
            //    this.Opacity = Properties.Settings.Default.Opacity_Main;
            //}
            //else
            //{
            //    this.Show();
            //    this.Opacity = Properties.Settings.Default.Opacity_Main;
            //}
        }



        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            FadeOut();
        }

        public void MinWindow(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                //double opacity = Properties.Settings.Default.Opacity_Main;
                double opacity = 1;
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


        private void HideGrid(object sender, MouseButtonEventArgs e)
        {
            Grid grid = ((Border)sender).Parent as Grid;
            grid.Visibility = Visibility.Hidden;

        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            new Dialog_About(this, false).ShowDialog();

        }

        private void ShowThanks(object sender, RoutedEventArgs e)
        {
            new Dialog_Thanks(this, false).ShowDialog();
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






        private void OpenSet_MouseDown(object sender, RoutedEventArgs e)
        {
            if (WindowSet != null) WindowSet.Close();
            WindowSet = new Settings();
            WindowSet.Show();
        }








        private void SetSearchValue(object sender, MouseButtonEventArgs e)
        {
            //SearchBar.Text = ((TextBlock)sender).Text;
            //SearchBar.Select(SearchBar.Text.Length, 0);
            //vieModel.ShowSearchPopup = false;
            //Resizing = true;
            //ResizingTimer.Start();
            //vieModel.Search = SearchBar.Text;
        }



        public bool CanSearch = false;





        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //SearchBar_SearchStarted(null, null);
                doSearch(sender, null);
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

        private void SetSearchSelect()
        {
            //for (int i = 0; i < SearchItemsControl.Items.Count; i++)
            //{
            //    ContentPresenter c = (ContentPresenter)SearchItemsControl.ItemContainerGenerator.ContainerFromItem(SearchItemsControl.Items[i]);
            //    StackPanel stackPanel = FindElementByName<StackPanel>(c, "SearchStackPanel");
            //    if (stackPanel != null)
            //    {

            //        Border border = stackPanel.Children[0] as Border;
            //        TextBlock textBlock = border.Child as TextBlock;
            //        if (i == SearchSelectIdex)
            //        {
            //            border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
            //        }
            //        else
            //        {
            //            border.Background = new SolidColorBrush(Colors.Transparent);
            //        }

            //    }
            //}

        }


        private void HideListScrollViewer(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            string name = button.Content.ToString();
            if (name == Jvedio.Language.Resources.Hide)
                button.Content = Jvedio.Language.Resources.Show;
            else
                button.Content = Jvedio.Language.Resources.Hide;
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
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("LabelName", label);
            vieModel.extraWrapper = wrapper;
            vieModel.ClickFilterType = "Label";
            pagination.CurrentPageChange -= Pagination_CurrentPageChange;
            vieModel.CurrentPage = 1;
            vieModel.LoadData();
            pagination.CurrentPageChange += Pagination_CurrentPageChange;
        }


        public static Dictionary<int, string> ClickFilterDict = new Dictionary<int, string>() {

            {  0,"Genre"},
            {  1,"Series"},
            {  2,"Studio"},
            {  3,"Director"},

        };


        public void ShowSameString(string str, string clickFilterType = "")
        {
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





        public void Genre_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            string genre = textBlock.Text.ToString().Split('(').First();
            vieModel.GetMoviebyGenre(genre);
            vieModel.TextType = genre;
        }



        public void ActorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SelectedActress.Clear();
            ActorSetSelected();
        }

        public void ActorSetSelected()
        {
            ItemsControl itemsControl;
            //if (Properties.Settings.Default.EasyMode)
            //    itemsControl = SimpleMovieItemsControl;
            //else
            itemsControl = ActorItemsControl;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "rootBorder");
                Grid grid = border.Parent as Grid;
                long actorID = getDataID(border);
                if (border != null)
                {

                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (Properties.Settings.Default.ActorEditMode && vieModel.SelectedActors.Where(arg => arg.ActorID == actorID).Any())
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
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (Properties.Settings.Default.ActorEditMode)
            {
                Border border = grid.Children[0] as Border;
                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            }
        }

        public void ActorBorderMouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (Properties.Settings.Default.ActorEditMode)
            {

                long actorID = getDataID(element);
                Border border = grid.Children[0] as Border;
                if (vieModel.SelectedActors.Where(arg => arg.ActorID == actorID).Any())
                {
                    border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
                }
                else
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }



        public void ActorBorderMouseEnter(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            Border border = ((Grid)stackPanel.Parent).Children[0] as Border;
            if (Properties.Settings.Default.ActorEditMode)
            {

                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            }
            else
            {
                border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundTitle"];
            }

        }

        public void ActorBorderMouseLeave(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            Border border = ((Grid)stackPanel.Parent).Children[0] as Border;
            if (Properties.Settings.Default.ActorEditMode)
            {
                border.BorderBrush = Brushes.Transparent;
            }
            else
            {
                border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
            }
        }

        public void SelectActor(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;//点击 border 也能选中
            long actorID = getDataID(element);
            if (Properties.Settings.Default.ActorEditMode)
            {
                ActorInfo actorInfo = vieModel.CurrentActorList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
                int selectIdx = vieModel.CurrentActorList.IndexOf(actorInfo);

                // 多选
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (actorfirstidx == -1)
                        actorfirstidx = selectIdx;
                    else
                        actorsecondidx = selectIdx;
                }


                Console.WriteLine("actorfirstidx=" + actorfirstidx);
                Console.WriteLine("actorsecondidx=" + actorsecondidx);
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





            //Image image = sender as Image;
            //StackPanel stackPanel = image.Parent as StackPanel;
            //TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            //string name = textBox.Text.Split('(')[0];
            //if (Properties.Settings.Default.ActorEditMode)
            //{
            //    (Actress currentActress, int selectIdx) = GetActressFromCurrentActors(name);
            //    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        if (actorfirstidx == -1)
            //            actorfirstidx = selectIdx;
            //        else
            //            actorsecondidx = selectIdx;
            //    }


            //    if (actorfirstidx >= 0 && actorsecondidx >= 0)
            //    {
            //        if (actorfirstidx > actorsecondidx)
            //        {
            //            //交换一下顺序
            //            int temp = actorfirstidx;
            //            actorfirstidx = actorsecondidx - 1;
            //            actorsecondidx = temp - 1;
            //        }

            //        for (int i = actorfirstidx + 1; i <= actorsecondidx; i++)
            //        {
            //            var m = vieModel.CurrentActorList[i];
            //            if (SelectedActress.Contains(m))
            //                SelectedActress.Remove(m);
            //            else
            //                SelectedActress.Add(m);
            //        }
            //        actorfirstidx = -1;
            //        actorsecondidx = -1;
            //    }
            //    else
            //    {
            //        if (SelectedActress.Contains(currentActress))
            //            SelectedActress.Remove(currentActress);
            //        else
            //            SelectedActress.Add(currentActress);
            //    }


            //    ActorSetSelected();
            //}
            //else
            //{
            //    vieModel.ActorInfoGrid = Visibility.Visible;
            //    vieModel.IsLoadingMovie = true;
            //    vieModel.TabSelectedIndex = 0;
            //    var currentActress = vieModel.ActorList.Where(arg => arg.name == name).First();
            //    Actress actress = DataBase.SelectInfoByActress(currentActress);
            //    actress.id = "";//不按照 id 选取演员
            //    await vieModel.AsyncGetMoviebyActress(actress);
            //    vieModel.Actress = actress;
            //    vieModel.AsyncFlipOver();
            //    vieModel.TextType = actress.name;
            //}
        }



        private (Actress, int) GetActressFromCurrentActors(string name)
        {
            Actress result = null;
            int idx = 0;
            //for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            //{
            //    if (vieModel.CurrentActorList[i].name == name)
            //    {
            //        result = vieModel.CurrentActorList[i];
            //        idx = i;
            //        break;
            //    }
            //}
            return (result, idx);
        }

        private (Movie, int) GetMovieFromFilterMovie(string id)
        {
            Movie result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.FilterMovieList.Count; i++)
            {
                if (vieModel.FilterMovieList[i].id == id)
                {
                    result = vieModel.FilterMovieList[i];
                    idx = i;
                    break;
                }
            }
            return (result, idx);
        }


        private void ShowDetails(object sender, MouseButtonEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
            if (Resizing || !canShowDetails) return;
            FrameworkElement element = sender as FrameworkElement;//点击 border 也能选中
            long ID = getDataID(element);
            if (Properties.Settings.Default.EditMode)
            {
                Video video = vieModel.CurrentVideoList.Where(arg => arg.DataID == ID).FirstOrDefault();
                int selectIdx = vieModel.CurrentVideoList.IndexOf(video);

                // 多选
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (firstidx == -1)
                        firstidx = selectIdx;
                    else
                        secondidx = selectIdx;
                }


                Console.WriteLine("firstidx=" + firstidx);
                Console.WriteLine("secondidx=" + secondidx);
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
                windowDetails = new WindowDetails(ID);
                windowDetails.Show();
                //VieModel_Main.PreviousPage = vieModel.CurrentPage;
                //VieModel_Main.PreviousOffset = MovieScrollViewer.VerticalOffset;
            }
            canShowDetails = false;
        }


        private bool canShowDetails = false;
        private void CanShowDetails(object sender, MouseButtonEventArgs e)
        {
            canShowDetails = true;
        }



        public void ShowStatus(object sender, RoutedEventArgs e)
        {
            //if (StatusPopup.IsOpen == true)
            //    StatusPopup.IsOpen = false;
            //else
            //    StatusPopup.IsOpen = true;


        }

        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            downloadStatusPopup.IsOpen = true;
        }
        public void ShowScreenShotPopup(object sender, MouseButtonEventArgs e)
        {
            screenShotStatusPopup.IsOpen = true;
        }



        public void ShowMenu(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            Popup popup = grid.Children.OfType<Popup>().First();
            popup.IsOpen = true;
        }




        public void ShowDownloadMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadPopup.IsOpen = true;
        }





        public void ShowSearchMenu(object sender, MouseButtonEventArgs e)
        {
            //SearchOptionPopup.IsOpen = true;
        }





        /// <summary>
        /// 演员里的视频类型分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SetTypeValue(object sender, RoutedEventArgs e)
        {
            //RadioButton radioButton = sender as RadioButton;
            //int idx = ClassifyVedioTypeStackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            //vieModel.ClassifyVedioType = (VideoType)idx;
            ////刷新侧边栏显示
            //SetClassify(true);
        }






        public void SetSelected()
        {
            ItemsControl itemsControl;

            itemsControl = MovieItemsControl;


            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "rootBorder");
                if (border == null) continue;
                Grid grid = border.Parent as Grid;
                long dataID = getDataID(border);
                if (border != null)
                {

                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (Properties.Settings.Default.EditMode && vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any())
                    {
                        border.Background = GlobalStyle.Common.HighLight.Background;
                        border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;

                    }
                }

            }

        }


        public async Task<bool> DisposeImage()
        {
            return await Task.Run(() =>
            {
                ItemsControl itemsControl;
                //if (Properties.Settings.Default.EasyMode)
                //    itemsControl = SimpleMovieItemsControl;
                //else
                itemsControl = MovieItemsControl;
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {
                    ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                    if (c == null) continue;
                    vieModel.CurrentVideoList[i].SmallImage = null;
                    vieModel.CurrentVideoList[i].BigImage = null;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (c.ContentTemplate.FindName("GifImage", c) is GifImage gifImage)
                        {
                            gifImage.Dispose();
                        }
                    });
                }
                return true;
            });
        }


        public void DisposeGif(string id, bool disposeAll = false)
        {
            //ItemsControl itemsControl;
            ////if (Properties.Settings.Default.EasyMode)
            ////    itemsControl = SimpleMovieItemsControl;
            ////else
            //itemsControl = MovieItemsControl;
            //for (int i = 0; i < itemsControl.Items.Count; i++)
            //{
            //    ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
            //    if (c == null) continue;
            //    if (c.ContentTemplate.FindName("GifImage", c) is HandyControl.Controls.GifImage GifImage && c.ContentTemplate.FindName("IDTextBox", c) is TextBox textBox)
            //    {
            //        if (disposeAll)
            //        {
            //            GifImage.Source = null;
            //            GifImage.Dispose();
            //            GC.Collect();
            //        }
            //        else
            //        {
            //            if (id.ToUpper() == textBox.Text.ToUpper())
            //            {
            //                GifImage.Source = null;
            //                GifImage.Dispose();
            //                GC.Collect();
            //                break;
            //            }
            //        }

            //    }

            //}
        }














        public void SetViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
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

            Console.WriteLine(Properties.Settings.Default.ShowImageMode);
        }

        public void SetActorViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            var rbs = ActorViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            Properties.Settings.Default.ActorViewMode = idx;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();
        }





        public void AsyncLoadImage()
        {
            Console.WriteLine("AsyncLoadImage");
            //if (!Properties.Settings.Default.EasyMode) return;
            Task.Run(async () =>
            {
                for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
                {
                    await Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                    {
                        if (i >= vieModel.CurrentVideoList.Count) return;
                        Video video = vieModel.CurrentVideoList[i];
                        SetImage(ref video);
                        vieModel.CurrentVideoList[i] = null;
                        vieModel.CurrentVideoList[i] = video;
                    });
                }
            });
        }



        public List<ImageSlide> ImageSlides = null;
        public void AsyncLoadExtraPic()
        {
            ItemsControl itemsControl;
            //if (Properties.Settings.Default.EasyMode)
            //    itemsControl = SimpleMovieItemsControl;
            //else
            itemsControl = MovieItemsControl;
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







        private void LoadMovie()
        {
            vieModel.FlowNum++;
            vieModel.Flow();
        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //流动模式
            ScrollViewer sv = sender as ScrollViewer;
            if (sv.VerticalOffset >= 500)
                vieModel.GoToTopCanvas = Visibility.Visible;
            else
                vieModel.GoToTopCanvas = Visibility.Hidden;

            if (sv.VerticalOffset == sv.ScrollableHeight)
                vieModel.GoToBottomCanvas = Visibility.Hidden;
            else
                vieModel.GoToBottomCanvas = Visibility.Visible;


        }

        private WrapPanel GetItemsPanel(DependencyObject itemsControl)
        {
            ItemsPresenter itemsPresenter = GetVisualChild<ItemsPresenter>(itemsControl);
            WrapPanel itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as WrapPanel;
            return itemsPanel;
        }


        public T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        public void GotoTop(object sender, MouseButtonEventArgs e)
        {
            MovieScrollViewer.ScrollToTop();
        }

        private void GotoBottom(object sender, MouseButtonEventArgs e)
        {
            MovieScrollViewer.ScrollToBottom();
        }

        public void PlayVideo(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement el = sender as FrameworkElement;
            long dataid = getDataID(el);
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
                    //string arg = $"\"{Properties.Settings.Default.VedioPlayerPath}\" \"{filepath}\"";
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

        public async void TranslateMovie(object sender, RoutedEventArgs e)
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



        public async void GenerateActor(object sender, RoutedEventArgs e)
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
            //            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
            //            ImageSource actressImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetActressRect(bitmapImage, int32Rect));
            //            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(actressImage);
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

            foreach (MetaData metaData in metaDatas)
            {
                screenShotVideo(metaData);
            }
            if (!Global.FFmpeg.Dispatcher.Working)
                Global.FFmpeg.Dispatcher.BeginWork();
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






        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
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
            //        BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
            //        if (Properties.Settings.Default.HalfCutOFf)
            //        {
            //            double rate = 380f / 800f;

            //            Int32Rect int32Rect = new Int32Rect() { Height = SourceBitmap.Height, Width = (int)(rate * SourceBitmap.Width), X = (int)((1 - rate) * SourceBitmap.Width), Y = 0 };
            //            ImageSource smallImage = ImageProcess.CutImage(bitmapImage, int32Rect);
            //            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
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
            //                ImageSource smallImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetRect(bitmapImage, int32Rect));
            //                System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
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
            //        movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");

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
            WindowEdit?.Close();
            WindowEdit = new WindowEdit(GetIDFromMenuItem(sender));
            WindowEdit.ShowDialog();

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
            if (!fromDetailWindow && GetWindowByName("WindowDetails") is Window window)
            {
                WindowDetails windowDetails = (WindowDetails)window;
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






        public string GetFormatGenreString(List<Movie> movies, string type = "genre")
        {
            List<string> list = new List<string>();
            if (type == "genre")
            {
                movies.ForEach(arg =>
                {
                    foreach (var item in arg.genre.Split(' '))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }
            else if (type == "label")
            {
                movies.ForEach(arg =>
                {
                    foreach (var item in arg.label.Split(' '))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }
            else if (type == "actor")
            {

                movies.ForEach(arg =>
                {

                    foreach (var item in arg.actor.Split(actorSplitDict[arg.vediotype]))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }

            string result = "";
            list.ForEach(arg => { result += arg + " "; });
            return result;
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
                    FileHelper.TryOpenUrl(url);
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

        private void ForceToDownLoad(object sender, RoutedEventArgs e)
        {
            //if (DownLoader?.State == DownLoadState.DownLoading)
            //{
            //    msgCard.Info(Jvedio.Language.Resources.Message_WaitForDownload);
            //}
            //else if (!JvedioServers.IsProper())
            //{
            //    msgCard.Error(Jvedio.Language.Resources.Message_SetUrl);
            //}
            //else
            //{
            //    try
            //    {
            //        if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            //        string id = GetIDFromMenuItem(sender, 1);
            //        Movie CurrentMovie = GetMovieFromVieModel(id);
            //        if (CurrentMovie != null)
            //        {
            //            if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            //            StartDownload(vieModel.SelectedVideo.ToList(), true);
            //        }


            //    }
            //    catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            //}
            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }



        private void EditActress(object sender, MouseButtonEventArgs e)
        {
            vieModel.EnableEditActress = !vieModel.EnableEditActress;
            //Console.WriteLine(vieModel.Actress.age); 
        }

        private void SaveActress(object sender, KeyEventArgs e)
        {



        }

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



        private void ProgressBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ProgressBar PB = sender as ProgressBar;
            if (PB.Value + PB.LargeChange <= PB.Maximum)
            {
                PB.Value += PB.LargeChange;
            }
            else
            {
                PB.Value = PB.Minimum;
            }
        }

        private void DelLabel(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            StackPanel stackPanel = border.Parent as StackPanel;

            Console.WriteLine(stackPanel.Parent.GetType().ToString());

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

            if (!IsToUpdate && GlobalConfig.Settings.CloseToTaskBar && this.IsVisible == true)
            {
                e.Cancel = true;
                vieModel.HideToIcon = true;
                this.Hide();
                WindowSet?.Hide();
            }

            GlobalConfig.Main?.Save();
            GlobalConfig.Settings?.Save();

        }

        private void GoToActorPage(object sender, KeyEventArgs e)
        {
            //if (vieModel.TotalActorPage <= 1) return;
            //if (e.Key == Key.Enter)
            //{
            //    string pagestring = ((TextBox)sender).Text;
            //    int page = 1;
            //    if (pagestring == null) { page = 1; }
            //    else
            //    {
            //        var isnumeric = int.TryParse(pagestring, out page);
            //    }
            //    if (page > vieModel.TotalActorPage) { page = vieModel.TotalActorPage; } else if (page <= 0) { page = 1; }
            //    vieModel.CurrentActorPage = page;
            //    vieModel.ActorFlipOver();
            //}
        }

        private void GoToPage(object sender, KeyEventArgs e)
        {
            if (vieModel.TotalPage <= 1) return;
            if (e.Key == Key.Enter)
            {
                string pagestring = ((TextBox)sender).Text;
                int page = 1;
                if (pagestring == null) { page = 1; }
                else
                {
                    var isnumeric = int.TryParse(pagestring, out page);
                }
                if (page > vieModel.TotalPage) { page = vieModel.TotalPage; } else if (page <= 0) { page = 1; }
                vieModel.CurrentPage = page;
                vieModel.AsyncFlipOver();
            }
        }


        public void StopDownLoad()
        {
            //if (DownLoader != null && DownLoader.State == DownLoadState.DownLoading) msgCard.Warning(Jvedio.Language.Resources.Message_Stop);
            //DownLoader?.CancelDownload();
            ////downLoadActress?.CancelDownload();
            //this.Dispatcher.BeginInvoke((Action)delegate { vieModel.ProgressBarVisibility = Visibility.Hidden; });


        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }





        public T FindVisualChildOrContentByType<T>(DependencyObject parent)
       where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.GetType() == typeof(T))
            {
                return parent as T;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child.GetType() == typeof(T))
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildOrContentByType<T>(child);
                    if (result != null)
                        return result;
                }
            }

            if (parent is ContentControl contentControl)
            {
                return this.FindVisualChildOrContentByType<T>(contentControl.Content as DependencyObject);
            }

            return null;

        }



        private void NextPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalPage <= 1) return;
            if (vieModel.CurrentPage + 1 > vieModel.TotalPage) vieModel.CurrentPage = 1;
            else vieModel.CurrentPage += 1;

            vieModel.AsyncFlipOver();
            //if (Properties.Settings.Default.EasyMode)
            //    SimpleMovieScrollViewer.ScrollToTop();
            //else
            MovieScrollViewer.ScrollToTop();

        }

        private void PreviousPage(object sender, MouseButtonEventArgs e)
        {

            if (vieModel.TotalPage <= 1) return;
            if (vieModel.CurrentPage - 1 <= 0) vieModel.CurrentPage = vieModel.TotalPage;
            else vieModel.CurrentPage -= 1;

            vieModel.AsyncFlipOver();
            //if (Properties.Settings.Default.EasyMode)
            //    SimpleMovieScrollViewer.ScrollToTop();
            //else
            MovieScrollViewer.ScrollToTop();

        }







        private void ActorGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.ActorEditMode)
            //{
            //    foreach (var item in vieModel.ActorList)
            //    {
            //        if (!SelectedActress.Contains(item))
            //        {
            //            SelectedActress.Add(item);

            //        }
            //    }
            //    ActorSetSelected();
            //}
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.EditMode)
            {
                foreach (var item in vieModel.CurrentVideoList)
                {
                    if (!vieModel.SelectedVideo.Contains(item))
                    {
                        vieModel.SelectedVideo.Add(item);
                    }
                }
                SetSelected();
            }
        }

        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }


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

        public void LikeSelectedActor(object sender, RoutedEventArgs e)
        {

            //if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            //StackPanel sp = null;
            //if (sender is MenuItem mnu)
            //{
            //    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            //    string name = sp.Tag.ToString();
            //    Actress CurrentActress = GetActressFromVieModel(name);
            //    if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
            //    DataBase.CreateTable(DataBase.SQLITETABLE_ACTRESS_LOVE);
            //    foreach (Actress actress in SelectedActress)
            //    {
            //        Actress newActress = DataBase.SelectInfoByActress(actress);
            //        DataBase.SaveActressLikeByName(newActress.name, newActress.like == 0 ? 1 : 0);
            //    }

            //}
            //if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();

        }

        public void SelectAllActor(object sender, RoutedEventArgs e)
        {
            //if (Properties.Settings.Default.ActorEditMode) { ActorCancelSelect(); return; }
            //Properties.Settings.Default.ActorEditMode = true;
            //foreach (var item in vieModel.CurrentActorList)
            //    if (!SelectedActress.Contains(item)) SelectedActress.Add(item);

            //ActorSetSelected();
        }

        public void ActorCancelSelect()
        {
            Properties.Settings.Default.ActorEditMode = false; SelectedActress.Clear(); ActorSetSelected();
        }

        public void RefreshCurrentActressPage(object sender, RoutedEventArgs e)
        {
            //ActorCancelSelect();
            //if ((bool)LoveCheckBox.IsChecked)
            //{
            //    ShowLoveActors(null, null);
            //}
            //else
            //{
            //    vieModel.RefreshActor();
            //}

        }

        public void StartDownLoadActor(List<Actress> actresses)
        {
            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "BusActress.sqlite")) return;

            //downLoadActress = new DownLoadActress(actresses);


            //downLoadActress.InfoUpdate += (s, ev) =>
            //    {
            //        ActressUpdateEventArgs ae = ev as ActressUpdateEventArgs;
            //        var actores = vieModel.ActorList.Where(arg => arg.name.ToUpper() == ae.Actress.name.ToUpper()).ToList();
            //        if (actores == null || actores.Count == 0) return;
            //        int idx = vieModel.ActorList.IndexOf(actores.First());
            //        if (idx >= vieModel.ActorList.Count) return;

            //        vieModel.ActorList[idx] = ae.Actress;
            //        vieModel.ActorProgressBarValue = (int)(ae.progressBarUpdate.value / ae.progressBarUpdate.maximum * 100);
            //        if (vieModel.ActorProgressBarValue == 100) downLoadActress.State = DownLoadState.Completed;
            //        if (vieModel.ActorProgressBarValue == 100 || ae.state == DownLoadState.Fail || ae.state == DownLoadState.Completed) vieModel.ActorProgressBarVisibility = Visibility.Hidden;
            //    };



            //downLoadActress?.BeginDownLoad();
        }


        //DownLoadActress downLoadActress;

        /// <summary>
        /// 同步演员信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartDownLoadActress(object sender, RoutedEventArgs e)
        {
            //msgCard.Info(Jvedio.Language.Resources.ActressDownloadAttention);
            //DownloadActorPopup.IsOpen = false;
            //if (!JvedioServers.Bus.IsEnable)
            //{
            //    msgCard.Info($"BUS {Jvedio.Language.Resources.Message_NotOpenOrNotEnable}");
            //    return;
            //}

            //if (DownLoader?.State == DownLoadState.DownLoading)
            //    msgCard.Info(Jvedio.Language.Resources.Message_WaitForDownload);
            //else
            //    StartDownLoadActor(vieModel.CurrentActorList.ToList());



        }




        // todo
        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (vieModel.ProgressBarVisibility == Visibility.Hidden && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            //{
            //    taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            //}


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

            Button button = sender as Button;
            long dataID = getDataID(button);
            if (dataID <= 0) return;

            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();

            Video video = vieModel.VideoList.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video != null)
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
            Button button = sender as Button;
            long dataID = getDataID(button);
            if (dataID <= 0) return;

            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();

            Video video = vieModel.ViewAssociationDatas.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video != null)
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

        public void ShowSubsection(object sender, MouseButtonEventArgs e)
        {


            Image image = sender as Image;
            var grid = image.Parent as Grid;
            Popup popup = grid.Children.OfType<Popup>().First();
            popup.IsOpen = true;

        }

        private CancellationToken scan_ct;
        private CancellationTokenSource scan_cts;
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

        private void Button_StopDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;
            StopDownLoad();

        }

        private void Button_StartDownload(object sender, RoutedEventArgs e)
        {
            //DownloadPopup.IsOpen = false;

            //if (!JvedioServers.IsProper())
            //{
            //    msgCard.Error(Jvedio.Language.Resources.Message_SetUrl);

            //}
            //else
            //{
            //    if (DownLoader?.State == DownLoadState.DownLoading)
            //        msgCard.Info(Jvedio.Language.Resources.Message_WaitForDownload);
            //    else
            //        StartDownload(vieModel.CurrentVideoList.ToList());
            //}


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



        // todo
        private void ClearRecentWatched(object sender, RoutedEventArgs e)
        {
            //if (new RecentWatchedConfig("").Clear())
            //{
            //    //ReadRecentWatchedFromConfig();
            //    //vieModel.AddToRecentWatch("");
            //}
        }

        private void ConfigFirstRun()
        {
            if (GlobalConfig.Main.FirstRun)
            {
                vieModel.ShowFirstRun = Visibility.Visible;
                GlobalConfig.Main.FirstRun = false;
            }
        }




        SolidColorBrush originSideBg = (SolidColorBrush)Application.Current.Resources["Window.Side.Background"];
        SolidColorBrush originTitleBg = (SolidColorBrush)Application.Current.Resources["Window.Title.Background"];



        // todo 更改皮肤
        public void SetSkin()
        {
            FileProcess.SetSkin(Properties.Settings.Default.Themes);
            SettingsBorder.ContextMenu.UpdateDefaultStyle();//设置弹出的菜单正确显示
            switch (Properties.Settings.Default.Themes)
            {
                case "蓝色":
                    //设置渐变
                    LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                    myLinearGradientBrush.StartPoint = new Point(0.5, 0);
                    myLinearGradientBrush.EndPoint = new Point(0.5, 1);
                    myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 1));
                    myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 0));
                    //SideBorder.Background = myLinearGradientBrush;
                    break;

                default:
                    //SideBorder.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                    break;
            }

            if (Properties.Settings.Default.EnableBgImage && GlobalVariable.BackgroundImage != null)
            {
                SideBorder.Background = Brushes.Transparent;
                DatabaseComboBox.Background = (SolidColorBrush)Application.Current.Resources["Window.Side.Opacity.Background"];
                TitleBorder.Background = Brushes.Transparent;
                //MainProgressBar.Background = Brushes.Transparent;
                //ActorProgressBar.Background = Brushes.Transparent;
                //foreach (Expander expander in ExpanderStackPanel.Children.OfType<Expander>().ToList())
                //{
                //    expander.Background = Brushes.Transparent;
                //    Border border = expander.Content as Border;
                //    border.Background = Brushes.Transparent;
                //}
                BgImage.Source = GlobalVariable.BackgroundImage;
            }
            else
            {

                TitleBorder.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
                DatabaseComboBox.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
                SideBorder.SetResourceReference(Control.BackgroundProperty, "Window.Side.Background");
                //MainProgressBar.SetResourceReference(Control.BackgroundProperty, "Window.Side.Background");
                //ActorProgressBar.SetResourceReference(Control.BackgroundProperty, "Window.Side.Background");
                //foreach (Expander expander in ExpanderStackPanel.Children.OfType<Expander>().ToList())
                //{
                //    expander.SetResourceReference(Control.BackgroundProperty, "Window.Title.Background");
                //    Border border = expander.Content as Border;
                //    border.SetResourceReference(Control.BackgroundProperty, "Window.Background");
                //}
            }
            //设置背景图片
            BgImage.Source = GlobalVariable.BackgroundImage;


        }



        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedVideo.Clear();
            SetSelected();
        }





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
                    vieModel.AsyncFlipOver();
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
                    vieModel.AsyncFlipOver();
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
            //切换数据库

            vieModel.IsRefresh = true;
            vieModel.Statistic();
            vieModel.Reset();
            vieModel.initCurrentTagStamps();

            //vieModel.InitLettersNavigation();
            //vieModel.GetFilterInfo();
            AllRadioButton.IsChecked = true;



        }



        private void RandomDisplay(object sender, MouseButtonEventArgs e)
        {
            vieModel.RandomDisplay();
        }

        private async void ShowFilterGrid(object sender, MouseButtonEventArgs e)
        {
            //这里只能用 Visibility 判断
            if (FilterGrid.Visibility == Visibility.Visible)
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(2000, 0, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.HoldEnd);
                FilterGrid.BeginAnimation(FrameworkElement.MaxHeightProperty, doubleAnimation1);
                await Task.Delay(300);
                FilterGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (vieModel.Filters == null) vieModel.GetFilterInfo();
                FilterGrid.Visibility = Visibility.Visible;
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(0, 2000, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.HoldEnd);
                FilterGrid.BeginAnimation(FrameworkElement.MaxHeightProperty, doubleAnimation1);
                await Task.Delay(300);
            }

        }



        private void SetSelectMode(object sender, RoutedEventArgs e)
        {
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
            foreach (FrameworkElement element in contextMenu.Items)
            {
                if (element is MenuItem item && item.Header.ToString().Equals(header))
                    return item;
            }
            return null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && e.Key == Key.S)
            {
                //MessageBox.Show("1");
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.S)
            {
                //MessageBox.Show("2");
            }
        }

        private void cmdTextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vieModel.CmdVisibility = Visibility.Collapsed;
        }




        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowSamePath(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            vieModel.GetSamePathMovie(textBlock.Text);

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
            StackPanel stackPanel = rate.Parent as StackPanel;
            long id = getDataID(stackPanel);
            metaDataMapper.updateFieldById("Grade", rate.Value.ToString(), id);
            vieModel.Statistic();
            CanRateChange = false;
        }

        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {



            ////获得选中的标签
            //List<string> originLabels = new List<string>();
            //for (int i = 0; i < vieModel.LabelList.Count; i++)
            //{
            //    ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
            //    WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
            //    if (wrapPanel != null)
            //    {
            //        ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
            //        if ((bool)toggleButton.IsChecked)
            //        {
            //            Match match = Regex.Match(toggleButton.Content.ToString(), @"\( \d+ \)");
            //            if (match != null && match.Value != "")
            //            {
            //                string label = toggleButton.Content.ToString().Replace(match.Value, "");
            //                if (!originLabels.Contains(label)) originLabels.Add(label);
            //            }

            //        }
            //    }
            //}

            //if (originLabels.Count <= 0)
            //{
            //    //msgCard.Warning("请选择标签！");
            //    return;
            //}


            //foreach (Movie movie in vieModel.SelectedVideo)
            //{
            //    List<string> labels = LabelToList(movie.label);
            //    labels = labels.Union(originLabels).ToList();
            //    movie.label = string.Join(" ", labels);
            //    DataBase.UpdateMovieByID(movie.id, "label", movie.label, "String");
            //}
            //msgCard.Info(Jvedio.Language.Resources.Message_Success);
            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            //LabelGrid.Visibility = Visibility.Hidden;

        }



        private void AddNewLabel(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.CurrentMovieLabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelDelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelDelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        string label = toggleButton.Content.ToString();
                        if (!originLabels.Contains(label)) originLabels.Add(label);
                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                //msgCard.Warning("请选择标签！");
                return;
            }

            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Except(originLabels).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }

            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            if (vieModel.CurrentVideoList.Count == 0)
            {
                msgCard.Info(Jvedio.Language.Resources.Message_Success);
                LabelDelGrid.Visibility = Visibility.Hidden;
                vieModel.GetLabelList();
            }



        }


        private void ClearSingleLabel(object sender, RoutedEventArgs e)
        {
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", "", "String");
            vieModel.CurrentMovieLabelList = new List<string>();
            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;
        }

        private void AddSingleLabel(object sender, RoutedEventArgs e)
        {
            List<string> newLabel = new List<string>();

            var di = new DialogInput(this, Jvedio.Language.Resources.InputTitle1, "");
            di.ShowDialog();
            if (di.DialogResult == true & di.Text != "")
            {
                foreach (var item in di.Text.Split(' ').ToList())
                {
                    if (!newLabel.Contains(item)) newLabel.Add(item);
                }

            }
            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Union(newLabel).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }
            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;
        }

        private void DeleteSingleLabel(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.CurrentMovieLabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelDelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelDelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        string label = toggleButton.Content.ToString();
                        if (!originLabels.Contains(label)) originLabels.Add(label);
                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                //msgCard.Warning("请选择标签！");
                return;
            }

            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Except(originLabels).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }

            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            if (vieModel.CurrentVideoList.Count == 0)
            {
                msgCard.Info(Jvedio.Language.Resources.Message_Success);
                LabelDelGrid.Visibility = Visibility.Hidden;
                vieModel.GetLabelList();
            }



        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            Grid grid = (Grid)stackPanel.Parent;
            ((Grid)grid.Parent).Visibility = Visibility.Hidden;
        }








        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }






        private async void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {
            long id = GetIDFromMenuItem(sender, 1);


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

        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            var WrapPanels = FilterStackPanel.Children.OfType<WrapPanel>().ToList(); ;

            List<int> vediotype = new List<int>();
            WrapPanel wrapPanel = WrapPanels[0];
            foreach (var item in wrapPanel.Children.OfType<ToggleButton>())
            {
                if (item.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = item as ToggleButton;
                    if (tb != null) tb.IsChecked = false;
                }
            }
            for (int j = 1; j < WrapPanels.Count; j++)
            {
                ItemsControl itemsControl = WrapPanels[j].Children[1] as ItemsControl;
                if (itemsControl == null) continue;
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {

                    ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                    ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                    if (tb != null) tb.IsChecked = false;


                }
            }

            for (int i = 0; i < GenreItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)GenreItemsControl.ItemContainerGenerator.ContainerFromItem(GenreItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }
            for (int i = 0; i < ActorFilterItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ActorFilterItemsControl.ItemContainerGenerator.ContainerFromItem(ActorFilterItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }
            for (int i = 0; i < LabelFilterItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelFilterItemsControl.ItemContainerGenerator.ContainerFromItem(LabelFilterItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }

        }

        private void ApplyFilter(object sender, RoutedEventArgs e)
        {
            var WrapPanels = FilterStackPanel.Children.OfType<WrapPanel>().ToList(); ;

            List<int> vediotype = new List<int>();
            WrapPanel wrapPanel = WrapPanels[0];
            var tbs = wrapPanel.Children.OfType<ToggleButton>().ToList();

            for (int i = 0; i < tbs.Count; i++)
            {
                ToggleButton tb = tbs[i] as ToggleButton;
                if ((bool)tb.IsChecked)
                {
                    vediotype.Add(i + 1);
                    break;
                }
            }


            //年份
            wrapPanel = WrapPanels[1];
            ItemsControl itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> year = GetFilterFromItemsControl(itemsControl);



            //时长
            wrapPanel = WrapPanels[2];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> runtime = GetFilterFromItemsControl(itemsControl);

            //文件大小
            wrapPanel = WrapPanels[3];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> filesize = GetFilterFromItemsControl(itemsControl);

            //评分
            wrapPanel = WrapPanels[4];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> rating = GetFilterFromItemsControl(itemsControl);


            //类别
            List<string> genre = GetFilterFromItemsControl(GenreItemsControl);

            //演员
            List<string> actor = GetFilterFromItemsControl(ActorFilterItemsControl);

            //标签
            List<string> label = GetFilterFromItemsControl(LabelFilterItemsControl);

            string sql = "select * from movie where ";

            string s = "";
            vediotype.ForEach(arg => { s += $"vediotype={arg} or "; });
            if (vediotype.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s == "" | vediotype.Count == 3) s = "vediotype>0";
            sql += "(" + s + ") and "; s = "";

            year.ForEach(arg => { s += $"releasedate like '%{arg}%' or "; });
            if (year.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //类别
            genre.ForEach(arg => { s += $"genre like '%{arg}%' or "; });
            if (genre.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //演员
            actor.ForEach(arg => { s += $"actor like '%{arg}%' or "; });
            if (actor.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //类别
            label.ForEach(arg => { s += $"label like '%{arg}%' or "; });
            if (label.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";


            if (runtime.Count > 0 & rating.Count < 4)
            {
                runtime.ForEach(arg => { s += $"(runtime >={arg.Split('-')[0]} and runtime<={arg.Split('-')[1]}) or "; });
                if (runtime.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }

            if (filesize.Count > 0 & rating.Count < 4)
            {
                filesize.ForEach(arg => { s += $"(filesize >={double.Parse(arg.Split('-')[0]) * 1024 * 1024 * 1024} and filesize<={double.Parse(arg.Split('-')[1]) * 1024 * 1024 * 1024}) or "; });
                if (filesize.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }

            if (rating.Count > 0 & rating.Count < 5)
            {
                rating.ForEach(arg => { s += $"(rating >={arg.Split('-')[0]} and rating<={arg.Split('-')[1]}) or "; });
                if (rating.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }


            sql = sql.Substring(0, sql.Length - 5);
            Console.WriteLine(sql);
            vieModel.ExecutiveSqlCommand(0, Jvedio.Language.Resources.Filter, sql);
        }

        private List<string> GetFilterFromItemsControl(ItemsControl itemsControl)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {

                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null)
                    if ((bool)tb.IsChecked) result.Add(tb.Content.ToString());
            }
            return result;
        }

        private List<string> GetFilterFromSideItemsControl(ItemsControl itemsControl)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (c != null)
                {
                    CheckBox cb = c.ContentTemplate.FindName("CheckBox", c) as CheckBox;
                    if (cb != null)
                        if ((bool)cb.IsChecked) result.Add(cb.Content.ToString());
                }

            }
            return result;
        }

        private async void Genre_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.Filters == null || vieModel.Filters.Count < 1) return;
            vieModel.IsRefresh = false;
            vieModel.Genre = new ObservableCollection<string>();
            GenreItemsControl.ItemsSource = null;
            GenreItemsControl.ItemsSource = vieModel.Genre;
            foreach (var item in vieModel.Filters[1])
            {
                if (vieModel.IsRefresh) break;
                await this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadGenreItem), item);
            }

        }

        private async void Actor_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.Filters == null || vieModel.Filters.Count < 2) return;
            vieModel.IsRefresh = false;
            vieModel.Actor = new ObservableCollection<string>();
            ActorFilterItemsControl.ItemsSource = null;
            ActorFilterItemsControl.ItemsSource = vieModel.Actor;
            foreach (var item in vieModel.Filters[2])
            {
                if (vieModel.IsRefresh) break;
                await this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadActorItem), item);
            }
        }

        private delegate void LoadItemDelegate(string content);

        private void LoadGenreItem(string content)
        {
            if (!vieModel.IsRefresh) vieModel.Genre.Add(content);
        }

        private void LoadActorItem(string content)
        {
            if (!vieModel.IsRefresh) vieModel.Actor.Add(content);
        }













        private void ShowSameList(object sender, EventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            string listName = radioButton.MainText.ToString();
            vieModel.ExecutiveSqlCommand(10, listName, $"select * from {listName}", "mylist");




        }






        private void refreshTagStamp(ref Video video, long newTagID)
        {
            string tagIDs = video.TagIDs;
            if (string.IsNullOrEmpty(tagIDs))
            {
                video.TagStamp = new ObservableCollection<TagStamp>();
                video.TagStamp.Add(GlobalVariable.TagStamps.Where(arg => arg.TagID == newTagID).FirstOrDefault());
                video.TagIDs = newTagID.ToString();
            }
            else
            {
                List<string> list = tagIDs.Split(',').ToList();
                if (!list.Contains(newTagID.ToString()))
                {
                    list.Add(newTagID.ToString());
                    video.TagIDs = String.Join(",", list);
                    video.TagStamp = new ObservableCollection<TagStamp>();
                    foreach (var arg in list)
                    {
                        long.TryParse(arg, out long id);
                        video.TagStamp.Add(GlobalVariable.TagStamps.Where(item => item.TagID == id).FirstOrDefault());
                    }
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
            ContextMenu contextMenu = gifImage.ContextMenu;
            foreach (FrameworkElement item in contextMenu.Items)
            {
                if (item.Name == "TagMenuItems" && item is MenuItem menuItem)
                {
                    menuItem.Items.Clear();
                    GlobalVariable.TagStamps.ForEach(arg =>
                    {
                        MenuItem menu = new MenuItem()
                        {
                            Header = arg.TagName
                        };
                        menu.Click += (s, ev) =>
                        {
                            string sql = $"insert or replace into metadata_to_tagstamp (DataID,TagID)  values ({dataID},{arg.TagID})";
                            tagStampMapper.executeNonQuery(sql);
                            initTagStamp();
                            RefreshTagStamp(dataID, arg.TagID);

                        };
                        menuItem.Items.Add(menu);

                    });
                }
            }

        }


        private void RefreshTagStamp(long dataID, long tagID)
        {
            ObservableCollection<Video> datas = vieModel.CurrentVideoList;
            if (AssoDataPopup.IsOpen) datas = vieModel.ViewAssociationDatas;


            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].DataID == dataID)
                {
                    Video newVideo = videoMapper.SelectVideoByID(dataID);
                    Video video = datas[i];
                    //video.TagIDs = newVideo.TagIDs;
                    refreshTagStamp(ref video, tagID);
                    datas[i] = null;
                    datas[i] = video;

                }
            }



        }



        private void Rate_ValueChanged_1(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            actorMapper.updateFieldById("Grade", rate.Value.ToString(), vieModel.CurrentActorInfo.ActorID);
        }


        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //vieModel.GetActorList();
        }

        private void OpenLogPath(object sender, EventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            FileHelper.TryOpenPath(path);
        }

        private void OpenImageSavePath(object sender, EventArgs e)
        {
            FileHelper.TryOpenPath(Properties.Settings.Default.BasePicPath);
        }

        private void OpenApplicationPath(object sender, EventArgs e)
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void HideActressGrid(object sender, MouseButtonEventArgs e)
        {
            var anim = new DoubleAnimation(1, 0, (Duration)FadeInterval, FillBehavior.Stop);
            anim.Completed += (s, _) => vieModel.ShowActorGrid = Visibility.Collapsed; ;
            ActorInfoGrid.BeginAnimation(UIElement.OpacityProperty, anim);

        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
                taskbarInstance.SetProgressValue((int)e.NewValue, 100, this);
                if (e.NewValue >= 100 || e.NewValue <= 0) taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }
        }

        private void ActorProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (vieModel == null) return;
            if (vieModel.ActorProgressBarValue == 100 || vieModel.ActorProgressBarValue == 0)
                vieModel.ActorProgressBarVisibility = Visibility.Hidden;
            else
                vieModel.ActorProgressBarVisibility = Visibility.Visible;
        }


        private async void RemoveFromList(object sender, RoutedEventArgs e)
        {
            //string table = GetCurrentList();
            //if (string.IsNullOrEmpty(table)) return;




            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
            //string id = GetIDFromMenuItem(sender, 0);
            //Movie CurrentMovie = GetMovieFromVieModel(id);
            //if (!vieModel.SelectedVideo.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedVideo.Add(CurrentMovie);
            //if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToRemove).ShowDialog() == false) return;
            //MySqlite dB = new MySqlite("mylist");
            //vieModel.SelectedVideo.ToList().ForEach(arg =>
            //    {

            //        dB.DeleteByField(table, "id", arg.id);

            //        vieModel.CurrentVideoList.Remove(arg); //从主界面删除
            //        vieModel.MovieList.Remove(arg);
            //        vieModel.FilterMovieList.Remove(arg);
            //    });
            //dB.Close();
            ////从详情窗口删除
            //if (GetWindowByName("WindowDetails") != null)
            //{
            //    WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
            //    foreach (var item in vieModel.SelectedVideo.ToList())
            //    {
            //        if (windowDetails.vieModel.DetailMovie.id == item.id)
            //        {
            //            windowDetails.Close();
            //            break;
            //        }
            //    }
            //}

            //msgCard.Info(Jvedio.Language.Resources.Message_Success);
            ////修复数字显示
            //vieModel.CurrentCount -= vieModel.SelectedVideo.Count;
            ////vieModel.TotalCount -= vieModel.SelectedVideo.Count;

            //vieModel.SelectedVideo.Clear();

            //await Task.Run(() => { Task.Delay(100).Wait(); });
            ////ListItemsControl.ItemsSource = null;
            ////ListItemsControl.ItemsSource = vieModel.MyList;

            ////侧边栏选项选中
            //for (int i = 0; i < ListItemsControl.Items.Count; i++)
            //{
            //    ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
            //    StackPanel stackPanel = FindElementByName<StackPanel>(c, "ListStackPanel");
            //    if (stackPanel != null)
            //    {
            //        var grids = stackPanel.Children.OfType<Grid>().ToList();
            //        foreach (Grid grid in grids)
            //        {
            //            RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
            //            if (radioButton != null && radioButton.Content.ToString() == table)
            //            {
            //                radioButton.IsChecked = true;
            //                break;
            //            }
            //        }

            //    }
            //}
            //if (!Properties.Settings.Default.EditMode) vieModel.SelectedVideo.Clear();
        }








        private void ClearActressInfo(object sender, RoutedEventArgs e)
        {
            //string name = vieModel.Actress.name;
            //DataBase.DeleteByField("actress", "name", name);

            //Actress actress = new Actress(vieModel.Actress.name);
            //actress.like = vieModel.Actress.like;
            //actress.smallimage = vieModel.Actress.smallimage;

            //vieModel.Actress = actress;

        }







        private void ClassifyTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vieModel.SetClassify();
        }


        private void HideBeginScanGrid(object sender, RoutedEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
        }

        private void OpenActorPath(object sender, RoutedEventArgs e)
        {
            //string filepath = System.IO.Path.Combine(BasePicPath, "Actresses", $"{vieModel.Actress.name}.jpg");
            //FileHelper.TryOpenSelectPath(filepath);
        }

        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            //if (vieModel.Actress.sourceurl.IsProperUrl())
            //    FileHelper.TryOpenUrl(vieModel.Actress.sourceurl);

            //else
            //{
            //    if (JvedioServers.Bus.Url.IsProperUrl())
            //    {
            //        string url = $"{JvedioServers.Bus.Url}searchstar/{System.Web.HttpUtility.UrlEncode(vieModel.Actress.name)}&type=&parent=ce";
            //        FileHelper.TryOpenUrl(url);
            //    }
            //    else
            //    {
            //        msgCard.Error(Jvedio.Language.Resources.CannotOpen + $" {vieModel.Actress.sourceurl}");
            //    }
            //}


        }


        private void OpenPath(string path)
        {

        }





        private void ClearSearchHistory(object sender, MouseButtonEventArgs e)
        {
            vieModel.SearchHistory.Clear();
            vieModel.SaveSearchHistory();
            //SearchHistoryStackPanel.Visibility = Visibility.Collapsed;
        }



        private void CmdTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.ScrollToEnd();
        }


        private void RefreshClassify(object sender, MouseButtonEventArgs e)
        {
            vieModel.SetClassify(true);
        }

        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            try
            {
                scan_cts.Cancel();
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



        private List<ActorSearch> GetActorSearchReulst(string sourceCode)
        {
            List<ActorSearch> result = new List<ActorSearch>();
            //if (string.IsNullOrEmpty(sourceCode)) return result;
            //HtmlDocument doc = new HtmlDocument();
            //doc.LoadHtml(sourceCode);
            //HtmlNodeCollection actorNodes = doc.DocumentNode.SelectNodes("//a[@class='avatar-box text-center']");
            //if (actorNodes == null) return result;
            //foreach (HtmlNode actorNode in actorNodes)
            //{
            //    ActorSearch actorSearch = new ActorSearch();
            //    actorSearch.ID = result.Count;
            //    HtmlNode img = actorNode.SelectSingleNode("div/img");
            //    HtmlNode name = actorNode.SelectSingleNode("div/span");
            //    HtmlNode tag = actorNode.SelectSingleNode("div/span/button");

            //    if (actorNode.Attributes["href"]?.Value != "") actorSearch.Link = actorNode.Attributes["href"].Value;
            //    if (tag != null && tag.InnerText != "") actorSearch.Tag = tag.InnerText;
            //    if (name != null && name.InnerText != "") actorSearch.Name = name.InnerText.Replace(actorSearch.Tag, "");
            //    if (img != null && img.Attributes["src"]?.Value != "") actorSearch.Img = img.Attributes["src"].Value;

            //    result.Add(actorSearch);
            //}

            return result;


        }



        /// <summary>
        /// 加载该演员出演的其他作品，仅支持 bus
        /// </summary>
        /// <param name="name"></param>
        private async void LoadActor(string name)
        {

        }


        public async void ShowSameActors(string name)
        {
            //vieModel.ActorInfoGrid = Visibility.Visible;
            //vieModel.IsLoadingMovie = true;
            //vieModel.TabSelectedIndex = 0;
            //var currentActress = vieModel.ActorList.Where(arg => arg.name == name).First();
            //Actress actress = DataBase.SelectInfoByActress(currentActress);
            //actress.id = "";//不按照 id 选取演员
            //await vieModel.AsyncGetMoviebyActress(actress);
            //vieModel.Actress = actress;
            //vieModel.AsyncFlipOver();
            //vieModel.TextType = actress.name;
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
            LoadActor(name);
        }

        private bool CheckLoadActorCancel()
        {
            if (LoadSearchCT.IsCancellationRequested) return true;
            try
            {
                LoadSearchCT.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException ex)
            {
                msgCard.Error(Jvedio.Language.Resources.Cancel);
                Console.WriteLine(ex.Message);
                LoadSearchCTS?.Dispose();
                return true;
            }

            return false;
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

        private async void LoadFromPage(object sender, RoutedEventArgs e)
        {


        }

        private void ManageImage(object sender, RoutedEventArgs e)
        {
            //WindowManageImage windowManageImage = new WindowManageImage();
            //windowManageImage.Show();
        }



        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            //vieModel.SearchFirstLetter = false;
            //if (vieModel.CurrentSearchCandidate != null && (SearchSelectIdex >= 0 & SearchSelectIdex < vieModel.CurrentSearchCandidate.Count))
            //    SearchBar.Text = vieModel.CurrentSearchCandidate[SearchSelectIdex];
            //if (SearchBar.Text == "") return;
            //if (SearchBar.Text.ToUpper() == "JVEDIO")
            //{
            //    Properties.Settings.Default.ShowSecret = true;
            //}
            //else
            //{
            //    vieModel.Search = SearchBar.Text;
            //    vieModel.ShowSearchPopup = false;
            //}
        }

        private void SearchBar_SearchStarted_1(object sender, FunctionEventArgs<string> e)
        {
            //HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            //int idx = ActorTabControl.SelectedIndex;
            //switch (idx)
            //{
            //    case 0:

            //        break;
            //    case 1:

            //        break;
            //    case 2:

            //        break;
            //    case 3:

            //        break;
            //    case 4:

            //        break;
            //    case 5:

            //        break;
            //    case 6:

            //        break;
            //    case 7:

            //        break;
            //}
        }


        private void SearchBar_MouseEnter_1(object sender, MouseEventArgs e)
        {
            HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            Color color = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundSearch"].ToString());
            searchBar.BorderBrush = new SolidColorBrush(color);
        }

        private void SearchBar_MouseLeave_1(object sender, MouseEventArgs e)
        {
            HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            Color color = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundGlobal"].ToString());
            searchBar.BorderBrush = new SolidColorBrush(color);
        }

        private void SearchBar_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            if (searchBar.Text == "")
            {

            }
            else
            {
                SearchBar_SearchStarted(sender, null);
            }
        }



        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Application.Current.Resources.MergedDictionaries[2].Source = new Uri("pack://application:,,,/ChaoControls.Style;Component/XAML/Skin/White.xaml", UriKind.RelativeOrAbsolute);
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
                    if (i.ToString() == Properties.Settings.Default.SortType)
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
            //Console.WriteLine("Pagination_CurrentPageChange =>  vieModel.canRender=" + vieModel.canRender);
            Pagination pagination = sender as Pagination;
            vieModel.CurrentPage = pagination.CurrentPage;
            //if (!vieModel.canRender) return;
            VieModel_Main.pageQueue.Enqueue(pagination.CurrentPage);
            vieModel.LoadData();
        }

        private void Pagination_PageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.PageSize = pagination.PageSize;
            //vieModel.LoadData();
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
            //vieModel.LoadData();
        }



        private void NewTagStamp(object sender, MouseButtonEventArgs e)
        {
            Window_TagStamp window_TagStamp = new Window_TagStamp();
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog)
            {
                string name = window_TagStamp.TagName;
                SolidColorBrush BackgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;
                if (string.IsNullOrEmpty(name)) return;
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
            TagStamp tagStamp = GlobalVariable.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();


            Window_TagStamp window_TagStamp = new Window_TagStamp(tagStamp.TagName, tagStamp.BackgroundBrush, tagStamp.ForegroundBrush);
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog)
            {
                string name = window_TagStamp.TagName;
                SolidColorBrush BackgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;
                if (string.IsNullOrEmpty(name)) return;
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
                for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
                {
                    if (vieModel.CurrentVideoList[i].TagStamp != null
                        && vieModel.CurrentVideoList[i].TagStamp.Contains(tagStamp))
                    {
                        vieModel.CurrentVideoList[i].TagStamp.Remove(tagStamp);
                    }
                }



                // 更新详情窗口
            }




        }


        bool canDrawSelect = false;
        System.Windows.Shapes.Rectangle rectangle;
        System.Windows.Point startPoint;
        private void BeginDrawSelect(object sender, MouseButtonEventArgs e)
        {
            return;
            if (!Properties.Settings.Default.EditMode) return;
            Panel.SetZIndex(DragSelectCanvas, 1);
            canDrawSelect = true;
            rectangle = new System.Windows.Shapes.Rectangle();
            rectangle.Height = 0;
            rectangle.Width = 0;
            rectangle.Fill = GlobalStyle.Common.HighLight.Background;
            rectangle.Stroke = GlobalStyle.Common.HighLight.BorderBrush;
            rectangle.Opacity = 0.8;
            rectangle.StrokeThickness = 1;
            rectangle.MouseLeftButtonUp += DragSelectCanvas_MouseUp;
            rectangle.MouseMove += DrawSelect;

            DragSelectCanvas.Children.Add(rectangle);
            startPoint = e.GetPosition(DragSelectCanvas);
            Canvas.SetLeft(rectangle, startPoint.X);
            Canvas.SetTop(rectangle, startPoint.Y);
            (sender as FrameworkElement).CaptureMouse();
        }


        // todo 拖动选择
        private void DrawSelect(object sender, MouseEventArgs e)
        {
            return;
            if (canDrawSelect)
            {
                System.Windows.Point point = e.GetPosition(DragSelectCanvas);
                double x = point.X - startPoint.X;
                double y = point.Y - startPoint.Y;
                Console.WriteLine($"({x},{y})");
                rectangle.Width = Math.Abs(x);
                rectangle.Height = Math.Abs(y);
                if (x < 0)
                {
                    Canvas.SetLeft(rectangle, point.X);
                }

                if (y < 0)
                {
                    Canvas.SetTop(rectangle, point.Y);
                }

            }

        }

        private void DragSelectCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            return;
            cancelDrawSelect();
            (sender as FrameworkElement).ReleaseMouseCapture();
        }



        private void cancelDrawSelect()
        {
            canDrawSelect = false;
            DragSelectCanvas.Children.Clear();
            Panel.SetZIndex(DragSelectCanvas, -1);

        }

        private void NewList(object sender, MouseButtonEventArgs e)
        {
        }

        public void RefreshGrade(Video newVideo)
        {
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
            long dataid = newVideo.DataID;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            {
                if (vieModel.CurrentVideoList[i]?.DataID == dataid)
                {
                    vieModel.CurrentVideoList[i].SmallImage = null;
                    vieModel.CurrentVideoList[i].BigImage = null;
                    Video video = videoMapper.selectOne(new SelectWrapper<Video>().Eq("DataID", dataid));
                    SetImage(ref video);
                    vieModel.CurrentVideoList[i].SmallImage = video.SmallImage;
                    vieModel.CurrentVideoList[i].BigImage = video.BigImage;
                    break;
                }

            }
        }
        public void RefreshData(long dataID)
        {
            try
            {
                for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
                {
                    if (vieModel.CurrentVideoList[i]?.DataID == dataID)
                    {
                        vieModel.CurrentVideoList[i].SmallImage = null;
                        vieModel.CurrentVideoList[i].BigImage = null;
                        vieModel.CurrentVideoList[i] = null;
                        Video video = videoMapper.SelectVideoByID(dataID);
                        SetImage(ref video);
                        Video.setTagStamps(ref video);// 设置标签戳
                        Video.handleEmpty(ref video);// 设置标题和发行日期
                        vieModel.CurrentVideoList[i] = video;
                        vieModel.CurrentVideoList[i].SmallImage = video.SmallImage;
                        vieModel.CurrentVideoList[i].BigImage = video.BigImage;
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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

        private async void doSearch(object sender, RoutedEventArgs e)
        {
            vieModel.Searching = true;
            GlobalConfig.Main.SearchSelectedIndex = searchTabControl.SelectedIndex;
            await vieModel.Query((SearchType)searchTabControl.SelectedIndex);
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

        private void ShowLabelManagement(object sender, RoutedEventArgs e)
        {
            new Msgbox(this, "开发中").ShowDialog();
            return;
            labelManagement?.Close();
            labelManagement = new Window_LabelManagement();
            labelManagement.Show();
        }

        private void PathCheckButton_Click(object sender, RoutedEventArgs e)
        {
            // 获得当前所有标记状态
            vieModel.LoadData();
        }

        private void SideBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine(SideGridColumn.ActualWidth);
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



        private void Rate_ValueChanged_2(object sender, FunctionEventArgs<double> e)
        {
            HandyControl.Controls.Rate rate = sender as HandyControl.Controls.Rate;
            long.TryParse(rate.Tag.ToString(), out long actorID);
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
            scanTask.Cancel();
        }
        private void CancelDownloadTask(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            task.Cancel();
        }
        private void CancelScreenShotTask(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            ScreenShotTask task = vieModel.ScreenShotTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            task.Cancel();
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
            if (scanTask.Status != System.Threading.Tasks.TaskStatus.Running)
            {
                Window_ScanDetail scanDetail = new Window_ScanDetail(scanTask.ScanResult);
                scanDetail.Show();
            }
        }

        private void ShowDownloadDetail(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            DownLoadTask task = vieModel.DownLoadTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            new Dialog_Logs(this, string.Join(Environment.NewLine, task.Logs)).ShowDialog();
        }
        private void ShowScreenShotDetail(object sender, RoutedEventArgs e)
        {
            string dataID = (sender as Button).Tag.ToString();
            ScreenShotTask task = vieModel.ScreenShotTasks.Where(arg => arg.DataID.ToString().Equals(dataID)).FirstOrDefault();
            new Dialog_Logs(this, string.Join(Environment.NewLine, task.Logs)).ShowDialog();
        }



        private void GoToStartUp(object sender, MouseButtonEventArgs e)
        {
            GlobalVariable.ClickGoBackToStartUp = true;
            //GlobalConfig.Settings.OpenDataBaseDefault = false;
            WindowStartUp windowStartUp = new WindowStartUp();
            Application.Current.MainWindow = windowStartUp;
            windowStartUp.Show();
            this.Close();
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
            string header = menu.Header.ToString();
            long dataID = GetIDFromMenuItem(sender, 1);
            Video video = datas.Where(arg => arg.DataID == dataID).FirstOrDefault();
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
            ShowSameActor(actorID);
        }

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            OpenSet_MouseDown(null, null);
        }

        private void AddToPlayerList(object sender, RoutedEventArgs e)
        {
            string playerPath = Properties.Settings.Default.VedioPlayerPath;
            bool success = false;
            if (File.Exists(playerPath))
            {
                handleMenuSelected(sender);
                if (Path.GetFileName(playerPath).Equals("PotPlayerMini64.exe"))
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

            }
            if (!success)
                MessageCard.Error("目前仅支持 potplayer，请设置 potplayer 所在路径");

        }

        private void OpenImageSavePath(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
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
            msgCard.Info("开发中");
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

        private void ImportVideo(object sender, RoutedEventArgs e)
        {
            string path = FileHelper.SelectPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                AddScanTask(new string[] { path });
                MessageCard.Success("已添加扫描任务：=> " + path);
            }
        }

        private void DeleteNotExistVideo(object sender, RoutedEventArgs e)
        {
            if (vieModel.DownLoadTasks.Count > 0 || vieModel.ScanTasks.Count > 0 || vieModel.ScreenShotTasks.Count > 0)
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
                    await Task.Delay(5000);
                    vieModel.Statistic();
                    vieModel.LoadData();
                }
                vieModel.RunningLongTask = false;
            });
        }

        private void DeleteNotInScanPath(object sender, RoutedEventArgs e)
        {
            if (vieModel.DownLoadTasks.Count > 0 || vieModel.ScanTasks.Count > 0 || vieModel.ScreenShotTasks.Count > 0)
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
            List<string> scanPaths = new List<string>();
            bool success = false;
            try
            {
                scanPaths = JsonConvert.DeserializeObject<List<string>>(scanPath).Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
                success = false;
            }
            success = scanPaths.Count > 0;
            if (!success)
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
                        if (string.IsNullOrEmpty(dir)) continue;
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
                    await Task.Delay(5000);
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

        private void ManageIndexAndCache(object sender, RoutedEventArgs e)
        {
            if (vieModel.DownLoadTasks.Count > 0 || vieModel.ScanTasks.Count > 0 || vieModel.ScreenShotTasks.Count > 0)
            {
                msgCard.Error("此操作需要清空下载任务、扫描任务、截图任务");
                return;
            }
        }

        private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Video> datas = vieModel.CurrentVideoList;
            if (AssoDataPopup.IsOpen) datas = vieModel.ViewAssociationDatas;

            MenuItem menuItem = sender as MenuItem;
            Label label = (menuItem.Parent as ContextMenu).PlacementTarget as Label;
            long.TryParse(label.Tag.ToString(), out long TagID);

            ItemsControl itemsControl = label.FindParentOfType<ItemsControl>();
            long.TryParse(itemsControl.Tag.ToString(), out long DataID);
            if (TagID <= 0 || DataID <= 0) return;
            ObservableCollection<TagStamp> tagStamps = itemsControl.ItemsSource as ObservableCollection<TagStamp>;
            TagStamp tagStamp = tagStamps.Where(arg => arg.TagID.Equals(TagID)).FirstOrDefault();
            if (tagStamp != null)
            {
                tagStamps.Remove(tagStamp);
                string sql = $"delete from metadata_to_tagstamp where TagID='{TagID}' and DataID='{DataID}'";
                tagStampMapper.executeNonQuery(sql);

                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i].DataID.Equals(DataID))
                    {
                        Video video = videoMapper.SelectVideoByID(DataID);
                        datas[i].TagIDs = video.TagIDs;
                        break;
                    }
                }
                vieModel.initCurrentTagStamps();
            }

        }

        private void CopyVID(object sender, MouseButtonEventArgs e)
        {
            string vid = (sender as Border).Tag.ToString();
            ClipBoard.TrySetDataObject(vid);
        }



        private long CurrentAssoDataID = 0;// 当前正在关联的影片的 dataID

        private void AddDataAssociation(object sender, RoutedEventArgs e)
        {
            long dataID = GetIDFromMenuItem(sender as MenuItem, 1);
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
            vieModel.SaveAssociation(CurrentAssoDataID);
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
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "rootBorder");
                if (border == null) continue;
                Grid grid = border.Parent as Grid;
                long dataID = getDataID(border);
                if (border != null)
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
            Border border = grid.Children[0] as Border;
            border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;


        }

        public void AssoBorderMouseLeave(object sender, MouseEventArgs e)
        {


            GifImage image = sender as GifImage;
            long dataID = getDataID(image);
            Grid grid = image.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            if (vieModel.AssociationSelectedDatas.Where(arg => arg.DataID == dataID).Any())
            {
                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            }
            else
            {
                border.BorderBrush = Brushes.Transparent;
            }

        }

        private void RemoveAssociation(object sender, RoutedEventArgs e)
        {
            Grid grid = (sender as Button).Parent as Grid;
            long.TryParse(grid.Tag.ToString(), out long dataID);
            if (dataID <= 0) return;
            Video video = vieModel.AssociationSelectedDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            vieModel.AssociationSelectedDatas.Remove(video);
            SetAssoSelected();
        }
        private void RemoveExistAssociation(object sender, RoutedEventArgs e)
        {
            Grid grid = (sender as Button).Parent as Grid;
            long.TryParse(grid.Tag.ToString(), out long dataID);
            if (dataID <= 0) return;
            Video video = vieModel.ExistAssociationDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            vieModel.ExistAssociationDatas.Remove(video);
        }

        private void MovieScrollViewer_MouseEnter(object sender, MouseEventArgs e)
        {
            //(sender as ScrollViewer).VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void MovieScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            //(sender as ScrollViewer).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }


        private bool IsAsso = false;
        private void ViewAssoDatas(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = true;
            long dataID = getDataID(sender as FrameworkElement);
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

        private void LoadDataByPicureMode(object sender, RoutedEventArgs e)
        {
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData)
            {
                MessageCard.Error("由于当前图片资源文相对于影片，因此不可用");
                return;
            }

            if (!GlobalConfig.Settings.PictureIndexCreated)
            {
                MessageCard.Error("请在【选项-库】中建立图片索引！");
                return;
            }
            vieModel.LoadData();
        }

        private void ShowExist(object sender, RoutedEventArgs e)
        {
            if (!GlobalConfig.Settings.PlayableIndexCreated)
            {
                MessageCard.Error("请在【选项-库】中建立播放索引！");
                return;
            }
            vieModel.LoadData();

        }
    }





    public class ScrollViewerBehavior
    {
        public static DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset",
                                                typeof(double),
                                                typeof(ScrollViewerBehavior),
                                                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }
        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }
        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }




}
