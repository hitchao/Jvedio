using DynamicData;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;
using static Jvedio.ImageProcess;
using System.Windows.Media.Effects;
using System.Text;
using System.Security.Cryptography;
using Jvedio.Utils.Encrypt;
using Jvedio.Utils;
using HtmlAgilityPack;
using System.Net;
using Jvedio.Style;
using Jvedio.Utils.Net;
using Jvedio.Utils.FileProcess;
using HandyControl.Data;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {



        public static string GrowlToken = "Main";

        public bool Resizing = false;
        public DispatcherTimer ResizingTimer = new DispatcherTimer();

        public Point WindowPoint = new Point(100, 100);
        public Size WindowSize = new Size(1000, 600);
        public JvedioWindowState WinState = JvedioWindowState.Normal;

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

        private HwndSource _hwndSource;

        public DetailMovie CurrentLabelMovie;
        public bool IsFlowing = false;


        public DispatcherTimer FlipoverTimer = new DispatcherTimer();

        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance = null;


        public Main()
        {
            InitializeComponent();
            this.Cursor = Cursors.Wait;
            FilterGrid.Visibility = Visibility.Collapsed;
            WinState = 0;



            InitImage();

            Properties.Settings.Default.Selected_Background = "#FF8000";
            Properties.Settings.Default.Selected_BorderBrush = "#FF8000";
            //Properties.Settings.Default.DisplayNumber = 5;

            BindingEvent();

            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported) taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
            #region "改变窗体大小"
            //https://www.cnblogs.com/yang-fei/p/4737308.html

            if (resizeGrid != null)
            {
                foreach (UIElement element in resizeGrid.Children)
                {
                    if (element is Rectangle resizeRectangle)
                    {
                        resizeRectangle.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                        resizeRectangle.MouseMove += ResizeRectangle_MouseMove;
                    }
                }
            }
            PreviewMouseMove += OnPreviewMouseMove;
            #endregion
        }

        #region "改变窗体大小"
        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) return;
            if (this.Width == SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height) return;

            if (sender is Rectangle rectangle)
            {
                switch (rectangle.Name)
                {
                    case "TopRectangle":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Top);
                        break;
                    case "Bottom":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Bottom);
                        break;
                    case "LeftRectangle":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Left);
                        break;
                    case "Right":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Right);
                        break;
                    case "TopLeft":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.TopLeft);
                        break;
                    case "TopRight":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.TopRight);
                        break;
                    case "BottomLeft":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.BottomLeft);
                        break;
                    case "BottomRight":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.BottomRight);
                        break;
                    default:
                        break;
                }
            }
        }


        protected void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }

        private void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) return;
            if (this.Width == SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height) return;

            if (sender is Rectangle rectangle)
            {
                switch (rectangle.Name)
                {
                    case "TopRectangle":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "Bottom":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "LeftRectangle":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "Right":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "TopLeft":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case "TopRight":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "BottomLeft":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "BottomRight":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += MainWindow_SourceInitialized;
            base.OnInitialized(e);
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion


        #region "热键"



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);


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
                if (!success) { MessageBox.Show(Jvedio.Language.Resources.HotKeyConflict, Jvedio.Language.Resources.HotKeyConflict); }
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
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消热键
            vieModel.HideToIcon = false;//隐藏图标
            DisposeGif("", true);//清除 gif 资源
            base.OnClosed(e);
        }


        #endregion


        private void InitImage()
        {
            //设置背景
            string path = Properties.Settings.Default.BackgroundImage;
            if (!File.Exists(path)) path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.jpg");
            GlobalVariable.BackgroundImage = null;
            GC.Collect();

            if (File.Exists(path))
                GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);

            DefaultBigImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            DefaultSmallImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_S.png", UriKind.Relative));
            DefaultActorImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));

        }

        //绑定事件
        private void BindingEvent()
        {


            //绑定演员
            foreach (StackPanel item in ActorInfoStackPanel.Children.OfType<StackPanel>().ToList())
            {
                TextBox textBox = item.Children[1] as TextBox;

                textBox.PreviewKeyUp += SaveActress;
            }




            //设置排序类型
            var radioButtons = SortStackPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in radioButtons)
            {
                item.Click += SetSortValue;
            }

            //设置图片显示模式
            var rbs = ImageTypeStackPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in rbs)
            {
                item.Click += SaveShowImageMode;
            }

            //设置分类中的视频格式
            var rbs2 = ClassifyVedioTypeStackPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in rbs2)
            {
                item.Click += SetTypeValue;
            }

            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);
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



        public async Task<bool> InitMovie()
        {
            return await Task.Run(() =>
            {
                vieModel = new VieModel_Main();
                if (Properties.Settings.Default.RandomDisplay)
                    vieModel.RandomDisplay();
                else
                    vieModel.Reset();



                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                {
                    this.DataContext = vieModel;
                    ItemsControl itemsControl;
                    if (Properties.Settings.Default.EasyMode)
                        itemsControl = SimpleMovieItemsControl;
                    else
                        itemsControl = MovieItemsControl;
                    itemsControl.ItemsSource = vieModel.CurrentMovieList;
                    vieModel.GetFilterInfo();
                });

                vieModel.CurrentMovieListHideOrChanged += (s, ev) => { StopDownLoad(); };
                vieModel.MovieFlipOverCompleted += (s, ev) =>
                {
                    //等待加载
                    Dispatcher.BeginInvoke((Action)delegate
                {
                    vieModel.CurrentCount = vieModel.CurrentMovieList.Count;
                    vieModel.TotalCount = vieModel.FilterMovieList.Count;
                    if (Properties.Settings.Default.EditMode) SetSelected();

                    if (Properties.Settings.Default.ShowImageMode == "2") AsyncLoadExtraPic();
                    else if (Properties.Settings.Default.ShowImageMode == "3") AsyncLoadGif();
                    else AsyncLoadImage();
                    SetLoadingStatus(false);
                    //滚动到指定位置
                    if (autoScroll)
                    {
                        DoubleAnimation verticalAnimation = new DoubleAnimation();
                        verticalAnimation.From = 0;
                        verticalAnimation.To = VieModel_Main.PreviousOffset;
                        verticalAnimation.Duration = TimeSpan.FromMilliseconds(500);
                        Storyboard storyboard = new Storyboard();
                        storyboard.Children.Add(verticalAnimation);

                        if (Properties.Settings.Default.EasyMode)
                            Storyboard.SetTarget(verticalAnimation, SimpleMovieScrollViewer);
                        else
                            Storyboard.SetTarget(verticalAnimation, MovieScrollViewer);

                        Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
                        storyboard.Begin();
                        autoScroll = false;
                    }


                }, DispatcherPriority.ContextIdle, null);
                };

                vieModel.OnCurrentMovieListRemove += (s, ev) =>
                {
                    ItemsControl itemsControl;
                    if (Properties.Settings.Default.EasyMode)
                        itemsControl = SimpleMovieItemsControl;
                    else
                        itemsControl = MovieItemsControl;

                    //清除gif
                    for (int i = 0; i < itemsControl.Items.Count; i++)
                    {
                        ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                        if (c.ContentTemplate.FindName("GifImage", c) is GifImage GifImage)
                        {
                            if (GifImage.Tag.ToString() == s.ToString())
                            {
                                GifImage.Source = null;
                                GC.Collect();
                                break;
                            }

                        }
                    }
                    //清除预览图
                    for (int i = 0; i < itemsControl.Items.Count; i++)
                    {
                        ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                        if (c.ContentTemplate.FindName("myImage", c) is Image myImage && c.ContentTemplate.FindName("myImage2", c) is Image myImage2)
                        {
                            if (myImage.Tag.ToString() == s.ToString())
                            {
                                myImage.Source = null;
                                myImage2.Source = null;
                                break;
                            }
                        }
                    }
                };

                vieModel.ActorFlipOverCompleted += (s, ev) =>
                {
                    //等待加载
                    Dispatcher.BeginInvoke((Action)delegate
                        {
                            vieModel.ActorCurrentCount = vieModel.CurrentActorList.Count;
                            vieModel.ActorTotalCount = vieModel.ActorList.Count;
                            if (Properties.Settings.Default.ActorEditMode) ActorSetSelected();
                            vieModel.SetClassifyLoadingStatus(false);
                        }, DispatcherPriority.ContextIdle, null);
                };


                return true;
            });

        }





        public void SetLoadingStatus(bool loading)
        {
            vieModel.IsLoadingMovie = loading;
            IsFlowing = loading;
            vieModel.IsFlipOvering = loading;
            SortStackPanel.IsEnabled = !loading;

        }






        public async Task<bool> InitActor()
        {
            vieModel.GetActorList();
            await Task.Delay(1);
            return true;
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
            double opacity = Properties.Settings.Default.Opacity_Main >= 0.5 ? Properties.Settings.Default.Opacity_Main : 1;
            var anim = new DoubleAnimation(1, opacity, (Duration)FadeInterval, FillBehavior.Stop);
            anim.Completed += (s, _) => this.Opacity = opacity;
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }





        void ShowNotice()
        {
            Task.Run(async () =>
            {
                //获取本地的公告
                string notices = "";
                string path = AppDomain.CurrentDomain.BaseDirectory + "Notice.txt";
                notices = StreamHelper.TryRead(path);
                HttpResult httpResult = await new MyNet().Http(NoticeUrl);
                //判断公告是否内容不同
                if (httpResult != null && httpResult.SourceCode != "" && httpResult.SourceCode != notices)
                {
                    //覆盖原有公告
                    StreamHelper.TryWrite(path, httpResult.SourceCode);
                    //提示用户
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        new Dialog_Notice(this, false, GetNoticeByLanguage(httpResult.SourceCode, Properties.Settings.Default.Language)).ShowDialog();
                    });
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



        public DownLoader DownLoader;

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
            DownLoader?.CancelDownload();
            DownLoader = new DownLoader(movies, moviesFC2, true);
            //UI更新
            DownLoader.InfoUpdate += (s, e) =>
            {
                InfoUpdateEventArgs eventArgs = e as InfoUpdateEventArgs;
                try
                {
                    try { Refresh(eventArgs, totalcount); }
                    catch (TaskCanceledException ex) { Logger.LogE(ex); }
                }
                catch (Exception ex1)
                {
                    Console.WriteLine(ex1.StackTrace);
                    Console.WriteLine(ex1.Message);
                }
            };

            //信息显示
            DownLoader.MessageCallBack += (s, e) =>
            {
                MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                if (eventArgs != null) HandyControl.Controls.Growl.Error(eventArgs.Message, GrowlToken);
            };

            DownLoader.StartThread();
        }

        public async void RefreshCurrentPage(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_StopAndTry, GrowlToken);
                return;
            }

            //刷新文件夹

            if (vieModel.IsScanning)
            {
                vieModel.IsScanning = false;
                try
                {
                    RefreshScanCTS?.Cancel();
                }
                catch (ObjectDisposedException ex) { Console.WriteLine(ex.Message); }

            }
            else
            {
                if (Properties.Settings.Default.ScanWhenRefresh)
                {
                    vieModel.IsScanning = true;
                    await ScanWhenRefresh();
                    vieModel.IsScanning = false;
                }
            }
            CancelSelect();
            await Task.Run(() =>
            {
                if (Properties.Settings.Default.ScanWhenRefresh)
                    vieModel.Reset();
                else
                    vieModel.FlipOver();
            });
        }

        public async Task<bool> ScanWhenRefresh()
        {
            vieModel.IsScanning = true;
            RefreshScanCTS = new CancellationTokenSource();
            RefreshScanCTS.Token.Register(() => { Console.WriteLine("取消任务"); this.Cursor = Cursors.Arrow; });
            RefreshScanCT = RefreshScanCTS.Token;
            await Task.Run(() =>
            {
                List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(System.IO.Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath)), RefreshScanCT);
                double num = Scan.InsertWithNfo(filepaths, RefreshScanCT);
                vieModel.IsScanning = false;

                if (Properties.Settings.Default.AutoDeleteNotExistMovie)
                {
                    //删除不存在影片
                    var movies = DataBase.SelectMoviesBySql("select * from movie");
                    movies.ForEach(movie =>
                    {
                        if (!File.Exists(movie.filepath))
                        {
                            DataBase.DeleteByField("movie", "id", movie.id);
                        }
                    });

                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //vieModel.Reset();
                    if (num > 0) HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_ScanNum} {num} --- {Jvedio.Language.Resources.Message_ViewLog}", GrowlToken);
                }), System.Windows.Threading.DispatcherPriority.Render);


            }, RefreshScanCTS.Token);
            RefreshScanCTS.Dispose();
            return true;
        }


        public void CancelSelect()
        {
            Properties.Settings.Default.EditMode = false; vieModel.SelectedMovie.Clear(); SetSelected();
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode) { CancelSelect(); return; }
            Properties.Settings.Default.EditMode = true;
            foreach (var item in vieModel.CurrentMovieList)
            {
                if (!vieModel.SelectedMovie.Contains(item))
                {
                    vieModel.SelectedMovie.Add(item);

                }
            }
            SetSelected();


        }




        public void Refresh(InfoUpdateEventArgs eventArgs, double totalcount)
        {
            Dispatcher.Invoke((Action)delegate ()
          {
              vieModel.ProgressBarValue = (int)(100 * eventArgs.progress / totalcount);
              vieModel.ProgressBarVisibility = Visibility.Visible;
              if (vieModel.ProgressBarValue == 100) { DownLoader.State = DownLoadState.Completed; vieModel.ProgressBarVisibility = Visibility.Hidden; }
              if (DownLoader.State == DownLoadState.Completed | DownLoader.State == DownLoadState.Fail) vieModel.ProgressBarVisibility = Visibility.Hidden;
              RefreshMovieByID(eventArgs.Movie.id);
          });
        }

        public void RefreshMovieByID(string ID)
        {
            Movie movie = DataBase.SelectMovieByID(ID);
            addTag(ref movie);
            if (Properties.Settings.Default.ShowImageMode == "3")
            {
                //gif
                string gifpath = System.IO.Path.Combine(BasePicPath, "GIF", $"{movie.id}.gif");
                if (File.Exists(gifpath))
                    movie.GifUri = new Uri(gifpath);
                else
                    movie.GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            }
            else
            {
                SetImage(ref movie);
            }
            (Movie currentMovie1, int idx1) = GetMovieFromCurrentMovie(ID);
            (Movie currentMovie2, int idx2) = GetMovieFromAllMovie(ID);
            (Movie currentMovie3, int idx3) = GetMovieFromFilterMovie(ID);

            if (currentMovie1 != null && idx1 < vieModel.CurrentMovieList.Count)
            {
                vieModel.CurrentMovieList[idx1] = null;
                vieModel.CurrentMovieList[idx1] = movie;
            }
            if (currentMovie2 != null && idx2 < vieModel.MovieList.Count)
            {
                vieModel.MovieList[idx2] = null;
                vieModel.MovieList[idx2] = movie;
            }

            if (currentMovie3 != null && idx3 < vieModel.FilterMovieList.Count)
            {
                vieModel.FilterMovieList[idx2] = null;
                vieModel.FilterMovieList[idx2] = movie;
            }
        }




        public void OpenSubSuctionVedio(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            TextBlock textBlock = stackPanel.Children.OfType<TextBlock>().Last();
            string filepath = textBlock.Text;
            PlayVideoWithPlayer(filepath, "");
        }



        private static void OnCreated(object obj, FileSystemEventArgs e)
        {
            //导入数据库

            if (Scan.IsProperMovie(e.FullPath))
            {
                FileInfo fileinfo = new FileInfo(e.FullPath);

                //获取创建日期
                string createDate = "";
                try { createDate = fileinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                catch { }
                if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Movie movie = new Movie()
                {
                    filepath = e.FullPath,
                    id = Identify.GetFanhao(fileinfo.Name),
                    filesize = fileinfo.Length,
                    vediotype = (int)Identify.GetVideoType(Identify.GetFanhao(fileinfo.Name)),
                    otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    scandate = createDate
                };
                if (!string.IsNullOrEmpty(movie.id) & movie.vediotype > 0) { DataBase.InsertScanMovie(movie); }
                Console.WriteLine($"成功导入{e.FullPath}");
            }




        }

        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            {
                DataBase.DeleteByField("movie", "filepath", e.FullPath);
            }
            Console.WriteLine("成功删除" + e.FullPath);
        }



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
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_WatchFail} {failwatcherMessage}", GrowlToken);
        }



        public void AdjustWindow()
        {
            SetWindowProperty();
            if (Properties.Settings.Default.FirstRun)
            {
                this.Width = SystemParameters.WorkArea.Width * 0.8;
                this.Height = SystemParameters.WorkArea.Height * 0.8;
            }


            HideMargin();
            vieModel.SideBorderWidth = Properties.Settings.Default.SideGridWidth < 200 ? 200 : Properties.Settings.Default.SideGridWidth;
        }

        private void SetWindowProperty()
        {
            //读取窗体设置
            WindowConfig cj = new WindowConfig(this.GetType().Name);
            WindowProperty windowProperty = cj.Read();
            Rect rect = new Rect() { Location = windowProperty.Location, Size = windowProperty.Size };
            WinState = windowProperty.WinState;
            //读到属性值
            if (WinState == JvedioWindowState.FullScreen)
            {
                this.WindowState = WindowState.Maximized;
            }
            else if (WinState == JvedioWindowState.None)
            {
                WinState = 0;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                this.Left = rect.X >= 0 ? rect.X : 0;
                this.Top = rect.Y >= 0 ? rect.Y : 0;
                this.Height = rect.Height > 100 ? rect.Height : 100;
                this.Width = rect.Width > 100 ? rect.Width : 100;
                if (this.Width == SystemParameters.WorkArea.Width | this.Height == SystemParameters.WorkArea.Height) { WinState = JvedioWindowState.Maximized; }
            }
        }






        private void Window_Closed(object sender, EventArgs e)
        {
            if (!IsToUpdate && Properties.Settings.Default.CloseToTaskBar && this.IsVisible == true)
            {
                vieModel.HideToIcon = true;
                this.Hide();
                WindowSet?.Hide();
                WindowTools?.Hide();
                WindowBatch?.Hide();
                WindowEdit?.Hide();
                window_DBManagement?.Hide();
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



                WindowTools windowTools = null;
                foreach (Window item in App.Current.Windows)
                {
                    if (item.GetType().Name == "WindowTools") windowTools = item as WindowTools;
                }



                if (windowTools?.IsVisible == true)
                {
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }


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
            if (Properties.Settings.Default.EnableWindowFade)
            {
                //double opacity = Properties.Settings.Default.Opacity_Main >= 0.5 ? Properties.Settings.Default.Opacity_Main : 1;
                //var anim = new DoubleAnimation(0, opacity, (Duration)FadeInterval,FillBehavior.Stop);
                //anim.Completed += (s, _) => this.Opacity = opacity;
                //this.BeginAnimation(UIElement.OpacityProperty, anim);
                this.Show();
                this.Opacity = Properties.Settings.Default.Opacity_Main;
            }
            else
            {
                this.Show();
                this.Opacity = Properties.Settings.Default.Opacity_Main;
            }

        }



        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            FadeOut();
        }

        public void MinWindow(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                double opacity = Properties.Settings.Default.Opacity_Main;
                var anim = new DoubleAnimation(0, (Duration)FadeInterval, FillBehavior.Stop);
                anim.Completed += (s, _) => this.WindowState = WindowState.Minimized;
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.WindowState = WindowState.Minimized;
            }
        }


        public async void MaxWindow(object sender, RoutedEventArgs e)
        {
            Resizing = true;
            if (WinState == 0)
            {
                var anim = new DoubleAnimation(0, Properties.Settings.Default.Opacity_Main, new Duration(TimeSpan.FromSeconds(0.25)), FillBehavior.Stop);
                this.BeginAnimation(UIElement.OpacityProperty, anim);
                //最大化
                WinState = JvedioWindowState.Maximized;
                WindowPoint = new Point(this.Left, this.Top);
                WindowSize = new Size(this.Width, this.Height);
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = SystemParameters.WorkArea.Top;
                this.Left = SystemParameters.WorkArea.Left;
                MaxButtonPath.Data = Geometry.Parse(PathData.MaxToNormalPath);
            }
            else
            {
                var anim = new DoubleAnimation(0, Properties.Settings.Default.Opacity_Main, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.Stop);
                this.BeginAnimation(UIElement.OpacityProperty, anim);
                WinState = JvedioWindowState.Normal;
                this.Left = WindowPoint.X;
                this.Width = WindowSize.Width;
                this.Top = WindowPoint.Y;
                this.Height = WindowSize.Height;
                MaxButtonPath.Data = Geometry.Parse(PathData.MaxPath);
            }
            this.WindowState = WindowState.Normal;
            this.OnLocationChanged(EventArgs.Empty);
            HideMargin();
        }

        private void HideMargin()
        {
            if (WinState == JvedioWindowState.Normal)
            {
                vieModel.MainGridThickness = new Thickness(10);
                this.ResizeMode = ResizeMode.CanResize;
            }
            else if (WinState == JvedioWindowState.Maximized || this.WindowState == WindowState.Maximized)
            {
                vieModel.MainGridThickness = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            ResizingTimer.Start();
        }



        private void MoveWindow(object sender, MouseEventArgs e)
        {
            vieModel.ShowSearchPopup = false;
            Border border = sender as Border;

            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized || (this.Width == SystemParameters.WorkArea.Width && this.Height == SystemParameters.WorkArea.Height))
                {
                    WinState = 0;
                    double fracWidth = e.GetPosition(border).X / border.ActualWidth;
                    this.Width = WindowSize.Width;
                    this.Height = WindowSize.Height;
                    this.WindowState = WindowState.Normal;
                    this.Left = e.GetPosition(border).X - border.ActualWidth * fracWidth;
                    this.Top = e.GetPosition(border).Y - border.ActualHeight / 2;
                    this.OnLocationChanged(EventArgs.Empty);
                    HideMargin();
                }
                this.DragMove();
            }
        }

        WindowTools WindowTools;

        private void OpenTools(object sender, RoutedEventArgs e)
        {
            if (WindowTools != null) WindowTools.Close();
            WindowTools = new WindowTools();
            WindowTools.Show();
        }

        Window_DBManagement window_DBManagement;
        private void OpenDataBase(object sender, RoutedEventArgs e)
        {
            if (window_DBManagement != null) window_DBManagement.Close();
            window_DBManagement = new Window_DBManagement();
            window_DBManagement.Show();
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

        private async void CheckUpgrade(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                new Dialog_Upgrade(this, false, "", "").ShowDialog();
            }
            else
            {
                (bool success, string remote, string updateContent) = await new MyNet().CheckUpdate(UpdateUrl);
                string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (success && local.CompareTo(remote) < 0)
                {
                    new Dialog_Upgrade(this, false, remote, updateContent).ShowDialog();
                }
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
            SearchBar.Text = ((TextBlock)sender).Text;
            SearchBar.Select(SearchBar.Text.Length, 0);
            vieModel.ShowSearchPopup = false;
            Resizing = true;
            ResizingTimer.Start();
            vieModel.Search = SearchBar.Text;
        }

        public void SearchBar_GotFocus(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            Border border = ((Grid)searchBar.Parent).Children[0] as Border;
            Color color1 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString());
            Color color2 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundSearch"].ToString());
            border.BorderBrush = new SolidColorBrush(color1);
            ColorAnimation colorAnimation = new ColorAnimation(color1, color2, new Duration(TimeSpan.FromMilliseconds(200)));
            border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        public void SearchBar_LostFocus(object sender, RoutedEventArgs e)
        {
            vieModel.ShowSearchPopup = false;
            HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            Border border = ((Grid)searchBar.Parent).Children[0] as Border;
            Color color1 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString());
            Color color2 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundSearch"].ToString());
            border.BorderBrush = new SolidColorBrush(color2);
            ColorAnimation colorAnimation = new ColorAnimation(color2, color1, new Duration(TimeSpan.FromMilliseconds(200)));
            border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

        }


        public bool CanSearch = false;

        private async void RefreshCandiadte(object sender, TextChangedEventArgs e)
        {
            vieModel.ShowSearchPopup = true;
            await vieModel?.GetSearchCandidate(SearchBar.Text);

        }



        private int SearchSelectIdex = -1;

        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //SearchBar_SearchStarted(null, null);
            }
            else if (e.Key == Key.Down)
            {
                int count = vieModel.CurrentSearchCandidate.Count;

                SearchSelectIdex += 1;
                if (SearchSelectIdex >= count) SearchSelectIdex = 0;
                SetSearchSelect();

            }
            else if (e.Key == Key.Up)
            {
                int count = vieModel.CurrentSearchCandidate.Count;
                SearchSelectIdex -= 1;
                if (SearchSelectIdex < 0) SearchSelectIdex = count - 1;
                SetSearchSelect();


            }
            else if (e.Key == Key.Escape)
            {
                vieModel.ShowSearchPopup = false;
            }
            else if (e.Key == Key.Delete)
            {
                SearchBar.Clear();
            }
        }

        private void SetSearchSelect()
        {
            for (int i = 0; i < SearchItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)SearchItemsControl.ItemContainerGenerator.ContainerFromItem(SearchItemsControl.Items[i]);
                StackPanel stackPanel = FindElementByName<StackPanel>(c, "SearchStackPanel");
                if (stackPanel != null)
                {

                    Border border = stackPanel.Children[0] as Border;
                    TextBlock textBlock = border.Child as TextBlock;
                    if (i == SearchSelectIdex)
                    {
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
                    }
                    else
                    {
                        border.Background = new SolidColorBrush(Colors.Transparent);
                    }

                }
            }

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




        private void ShowLabelEditGrid(object sender, RoutedEventArgs e)
        {
            //LabelEditGrid.Visibility = Visibility.Visible;
        }


        public void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string type = sender.GetType().ToString();
            string label = "";
            if (type == "System.Windows.Controls.TextBlock")
            {
                TextBlock textBlock = (TextBlock)sender;
                label = textBlock.Text;
                Match match = Regex.Match(label, @"\( \d+ \)");
                if (match != null && match.Value != "")
                {
                    label = label.Replace(match.Value, "");
                }
                TabItem tabItem = ActorTabControl.SelectedItem as TabItem;
                vieModel.GetMoviebyLabel(label, tabItem.Header.ToString().ToSqlString());
            }
            else if (type == "System.Windows.Controls.TextBox")
            {
                TextBox textBox = (TextBox)sender;
                label = textBox.Text;
                vieModel.GetMoviebyLabel(label);
            }

        }





        public void Genre_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            string genre = textBlock.Text.ToString().Split('(').First();
            vieModel.GetMoviebyGenre(genre);
            vieModel.TextType = genre;
        }

        public void ShowActorMovieFromDetailWindow(Actress actress)
        {
            vieModel.GetMoviebyActress(actress);
            actress = DataBase.SelectInfoByActress(actress);
            actress.smallimage = GetActorImage(actress.name);
            vieModel.Actress = actress;
            vieModel.ActorInfoGrid = Visibility.Visible;
        }

        public void ActorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SelectedActress.Clear();
            ActorSetSelected();
        }

        public void ActorSetSelected()
        {
            for (int i = 0; i < ActorItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ActorItemsControl.ItemContainerGenerator.ContainerFromItem(ActorItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "ActorWrapPanel");
                if (wrapPanel != null)
                {
                    Grid grid = wrapPanel.Children[0] as Grid;
                    Border border = grid.Children[0] as Border;
                    if (c.ContentTemplate.FindName("ActorNameTextBox", c) is TextBox textBox)
                    {
                        //DropShadowEffect dropShadowEffect = new DropShadowEffect() { Color = Colors.SkyBlue, BlurRadius = 10, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
                        //border.Effect = dropShadowEffect;
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                        foreach (Actress actress in SelectedActress)
                        {
                            if (actress.name == textBox.Text.Split('(')[0])
                            {
                                border.Background = (SolidColorBrush)Application.Current.Resources["Selected_Background"];
                                break;
                                //dropShadowEffect = new DropShadowEffect() { Color = Colors.OrangeRed, BlurRadius = 10, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
                                //border.Effect = dropShadowEffect;
                            }
                        }
                    }
                }
            }

        }


        public void BorderMouseEnter(object sender, MouseEventArgs e)
        {

            if (Properties.Settings.Default.EditMode)
            {
                Image image = sender as Image;
                Grid grid = image.Parent as Grid;
                StackPanel stackPanel = grid.Parent as StackPanel;
                Grid grid1 = stackPanel.Parent as Grid;
                Border border = grid1.Children[0] as Border;
                border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
            }
        }

        public void BorderMouseLeave(object sender, MouseEventArgs e)
        {

            if (Properties.Settings.Default.EditMode)
            {
                Image image = sender as Image;
                Grid grid = image.Parent as Grid;
                StackPanel stackPanel = grid.Parent as StackPanel;
                Grid grid1 = stackPanel.Parent as Grid;
                Border border = grid1.Children[0] as Border;
                string id = image.ToolTip.ToString();
                if (vieModel.SelectedMovie.Select(arg => arg.id).ToList().Contains(id))
                {
                    border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
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

                border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
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

        public async void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            string name = textBox.Text.Split('(')[0];
            if (Properties.Settings.Default.ActorEditMode)
            {
                (Actress currentActress, int selectIdx) = GetActressFromCurrentActors(name);
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
                        var m = vieModel.CurrentActorList[i];
                        if (SelectedActress.Contains(m))
                            SelectedActress.Remove(m);
                        else
                            SelectedActress.Add(m);
                    }
                    firstidx = -1;
                    secondidx = -1;
                }
                else
                {
                    if (SelectedActress.Contains(currentActress))
                        SelectedActress.Remove(currentActress);
                    else
                        SelectedActress.Add(currentActress);
                }


                ActorSetSelected();
            }
            else
            {
                vieModel.ActorInfoGrid = Visibility.Visible;
                vieModel.IsLoadingMovie = true;
                vieModel.TabSelectedIndex = 0;
                var currentActress = vieModel.ActorList.Where(arg => arg.name == name).First();
                Actress actress = DataBase.SelectInfoByActress(currentActress);
                actress.id = "";//不按照 id 选取演员
                await vieModel.AsyncGetMoviebyActress(actress);
                vieModel.Actress = actress;
                vieModel.AsyncFlipOver();
                vieModel.TextType = actress.name;
            }
        }


        private (Movie, int) GetMovieFromAllMovie(string id)
        {
            Movie result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
            {
                if (vieModel.CurrentMovieList[i].id == id)
                {
                    result = vieModel.CurrentMovieList[i];
                    idx = i;
                    break;
                }
            }
            return (result, idx);
        }

        private (Movie, int) GetMovieFromCurrentMovie(string id)
        {
            Movie result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
            {
                if (vieModel.CurrentMovieList[i].id == id)
                {
                    result = vieModel.CurrentMovieList[i];
                    idx = i;
                    break;
                }
            }
            return (result, idx);
        }


        private (Actress, int) GetActressFromCurrentActors(string name)
        {
            Actress result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            {
                if (vieModel.CurrentActorList[i].name == name)
                {
                    result = vieModel.CurrentActorList[i];
                    idx = i;
                    break;
                }
            }
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

        public int firstidx = -1;
        public int secondidx = -1;
        WindowDetails wd;
        private void ShowDetails(object sender, MouseButtonEventArgs e)
        {
            if (Resizing || !canShowDetails) return;
            //StackPanel parent = ((sender as FrameworkElement).Parent as Grid).Parent as StackPanel;
            FrameworkElement framework = (FrameworkElement)sender;

            //var TB = parent.Children.OfType<TextBox>().First();//识别码
            string id = framework.ToolTip.ToString();
            if (Properties.Settings.Default.EditMode)
            {
                (Movie movie, int selectIdx) = GetMovieFromCurrentMovie(id);
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
                        var m = vieModel.CurrentMovieList[i];
                        if (vieModel.SelectedMovie.Contains(m))
                            vieModel.SelectedMovie.Remove(m);
                        else
                            vieModel.SelectedMovie.Add(m);
                    }
                    firstidx = -1;
                    secondidx = -1;
                }
                else
                {
                    if (vieModel.SelectedMovie.Contains(movie))
                        vieModel.SelectedMovie.Remove(movie);
                    else
                        vieModel.SelectedMovie.Add(movie);
                }


                SetSelected();

            }
            else
            {
                wd?.Close();
                wd = new WindowDetails(id);
                wd.Show();
                VieModel_Main.PreviousPage = vieModel.CurrentPage;

                if (Properties.Settings.Default.EasyMode)
                    VieModel_Main.PreviousOffset = SimpleMovieScrollViewer.VerticalOffset;
                else
                    VieModel_Main.PreviousOffset = MovieScrollViewer.VerticalOffset;
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
            DownloadPopup.IsOpen = true;
        }

        public void ShowSortPopup(object sender, MouseButtonEventArgs e)
        {
            SortPopup.IsOpen = true;
        }

        public void ShowImagePopup(object sender, MouseButtonEventArgs e)
        {
            ImageSortPopup.IsOpen = true;
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
            SearchOptionPopup.IsOpen = true;
        }





        /// <summary>
        /// 演员里的视频类型分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SetTypeValue(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            int idx = ClassifyVedioTypeStackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            vieModel.ClassifyVedioType = (VedioType)idx;
            //刷新侧边栏显示
            SetClassify(true);
        }


        public void ShowDownloadActorMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadActorPopup.IsOpen = true;
        }



        public void SetSelected()
        {
            ItemsControl itemsControl;
            if (Properties.Settings.Default.EasyMode)
                itemsControl = SimpleMovieItemsControl;
            else
                itemsControl = MovieItemsControl;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "MovieBorder");
                if (border != null)
                {
                    if (c.ContentTemplate.FindName("Image", c) is Image image)
                    {
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                        border.BorderBrush = Brushes.Transparent;
                        foreach (Movie movie in vieModel.SelectedMovie)
                        {
                            if (movie.id == image.ToolTip.ToString())
                            {
                                border.Background = (SolidColorBrush)Application.Current.Resources["Selected_Background"];
                                border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_Background"];
                                break;
                            }
                        }

                    }
                }

            }

        }


        public void DisposeGif(string id, bool disposeAll = false)
        {
            ItemsControl itemsControl;
            if (Properties.Settings.Default.EasyMode)
                itemsControl = SimpleMovieItemsControl;
            else
                itemsControl = MovieItemsControl;
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (c.ContentTemplate.FindName("GifImage", c) is HandyControl.Controls.GifImage GifImage && c.ContentTemplate.FindName("IDTextBox", c) is TextBox textBox)
                {
                    if (disposeAll)
                    {
                        GifImage.Source = null;
                        GifImage.Dispose();
                        GC.Collect();
                    }
                    else
                    {
                        if (id.ToUpper() == textBox.Text.ToUpper())
                        {
                            GifImage.Source = null;
                            GifImage.Dispose();
                            GC.Collect();
                            break;
                        }
                    }

                }

            }
        }

        public T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            if (element == null) return childElement;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }

            return childElement;
        }







        public void SetSortValue(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            var rbs = SortStackPanel.Children.OfType<RadioButton>().ToList();
            int sortindex = rbs.IndexOf(radioButton);
            Sort sorttype = (Sort)sortindex;

            if (sorttype == vieModel.SortType) Properties.Settings.Default.SortDescending = !Properties.Settings.Default.SortDescending;

            vieModel.SortDescending = Properties.Settings.Default.SortDescending;
            Properties.Settings.Default.SortType = ((int)sorttype).ToString();
            Properties.Settings.Default.Save();
            vieModel.SortType = sorttype;
            vieModel.AsyncFlipOver();
        }

        public void SaveAllSearchType(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StackPanel stackPanel = radioButton.Parent as StackPanel;
            radioButton.IsChecked = true;
            int idx = stackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            Properties.Settings.Default.AllSearchType = idx.ToString();
            //vieModel?.GetSearchCandidate(SearchBar.Text);
            vieModel.SearchHint = Jvedio.Language.Resources.Search + radioButton.Content.ToString();
            Properties.Settings.Default.Save();
            vieModel.AllSearchType = Properties.Settings.Default.AllSearchType.Length == 1 ? (MySearchType)int.Parse(Properties.Settings.Default.AllSearchType) : 0;
        }


        public void SaveShowViewMode(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuItem father = menuItem.Parent as MenuItem;
            int idx = father.Items.IndexOf(menuItem);

            for (int i = 0; i < father.Items.Count; i++)
            {
                MenuItem item = (MenuItem)father.Items[i];
                if (i == idx)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }


            Properties.Settings.Default.ShowViewMode = idx.ToString();
            Properties.Settings.Default.Save();
            vieModel.AsyncFlipOver();
        }

        public void SaveVedioType(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuItem father = menuItem.Parent as MenuItem;
            int idx = father.Items.IndexOf(menuItem);


            for (int i = 0; i < father.Items.Count; i++)
            {
                MenuItem item = (MenuItem)father.Items[i];
                if (i == idx)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }

            Properties.Settings.Default.VedioType = idx.ToString();
            Properties.Settings.Default.Save();
            vieModel.AsyncFlipOver();
        }

        public void SaveStampsType(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuItem father = menuItem.Parent as MenuItem;
            int idx = father.Items.IndexOf(menuItem);


            for (int i = 0; i < father.Items.Count; i++)
            {
                MenuItem item = (MenuItem)father.Items[i];
                if (i == idx)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }

            vieModel.ShowStampType = idx;
            vieModel.AsyncFlipOver();
        }
        public void SaveShowImageMode(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            var rbs = ImageTypeStackPanel.Children.OfType<RadioButton>().ToList();
            int sortindex = rbs.IndexOf(radioButton);
            MyImageType imageType = (MyImageType)sortindex;


            Properties.Settings.Default.ShowImageMode = sortindex.ToString();
            Properties.Settings.Default.Save();

            if (sortindex == 4)
            {
                vieModel.ShowDetailsData();
            }
            else if (sortindex == 3)
            {
                //加载GIF
                AsyncLoadGif();
            }
            else if (sortindex == 2)
            {
                AsyncLoadExtraPic();
            }

            if (sortindex == 0)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.SmallImage_Width;
            else if (sortindex == 1)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.BigImage_Width;
            else if (sortindex == 2)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.ExtraImage_Width;
            else if (sortindex == 3)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.GifImage_Width;
        }




        public void AsyncLoadImage()
        {
            if (!Properties.Settings.Default.EasyMode) return;
            Task.Run(async () =>
            {
                for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
                {
                    await Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                    {
                        if (i >= vieModel.CurrentMovieList.Count) return;
                        Movie movie = vieModel.CurrentMovieList[i];
                        SetImage(ref movie);
                        vieModel.CurrentMovieList[i] = null;
                        vieModel.CurrentMovieList[i] = movie;
                    });
                }
            });
        }



        public List<ImageSlide> ImageSlides = null;
        public void AsyncLoadExtraPic()
        {
            ItemsControl itemsControl;
            if (Properties.Settings.Default.EasyMode)
                itemsControl = SimpleMovieItemsControl;
            else
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
                for (int i = idx; i < vieModel.CurrentMovieList.Count; i++)
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
            if (vieModel.CurrentMovieList == null) return;
            DisposeGif("", true);
            Task.Run(async () =>
            {
                for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
                {
                    Movie movie = vieModel.CurrentMovieList[i];
                    string gifpath = System.IO.Path.Combine(BasePicPath, "GIF", $"{movie.id}.gif");
                    if (movie.GifUri != null && !string.IsNullOrEmpty(movie.GifUri.OriginalString)
                        && movie.GifUri.OriginalString.IndexOf("/NoPrinting_G.gif") < 0) continue;
                    if (File.Exists(gifpath))
                        movie.GifUri = new Uri(gifpath);
                    else
                        movie.GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
                    //加载
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                    {
                        vieModel.CurrentMovieList[i] = null;
                        vieModel.CurrentMovieList[i] = movie;
                    });
                }
            });

        }






        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //无进度条时
            ScrollViewer sv = sender as ScrollViewer;
            if (sv.VerticalOffset == 0 && sv.VerticalOffset == sv.ScrollableHeight && vieModel.CurrentMovieList.Count < Properties.Settings.Default.DisplayNumber && !IsFlowing)
            {
                IsFlowing = true;
                LoadMovie();
            }
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




            if (!IsFlowing && sv.ScrollableHeight - sv.VerticalOffset <= 10 && sv.VerticalOffset != 0)
            {
                Console.WriteLine("1");
                if (!IsFlowing && vieModel.CurrentMovieList.Count < Properties.Settings.Default.DisplayNumber && vieModel.CurrentMovieList.Count < vieModel.FilterMovieList.Count && vieModel.CurrentMovieList.Count + (vieModel.CurrentPage - 1) * Properties.Settings.Default.DisplayNumber < vieModel.FilterMovieList.Count)
                {
                    IsFlowing = true;
                    LoadMovie();
                }
            }

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
            if (Properties.Settings.Default.EasyMode)
                SimpleMovieScrollViewer.ScrollToTop();
            else
                MovieScrollViewer.ScrollToTop();
        }

        public void PlayVideo(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            string id = frameworkElement.ToolTip.ToString();
            string filepath = DataBase.SelectInfoByID("filepath", "movie", id);
            PlayVideoWithPlayer(filepath, id);

        }

        public void PlayVideoWithPlayer(string filepath, string ID, string token = "")
        {
            if (token == "") token = GrowlToken;
            if (File.Exists(filepath))
            {
                bool success = false;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.VedioPlayerPath) && File.Exists(Properties.Settings.Default.VedioPlayerPath))
                {
                    success = FileHelper.TryOpenFile(Properties.Settings.Default.VedioPlayerPath, filepath, token);
                }
                else
                {
                    //使用默认播放器
                    success = FileHelper.TryOpenFile(filepath, token);
                }

                if (success) vieModel.AddToRecentWatch(ID);
            }
            else
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_OpenFail + "：" + filepath, token);
            }
        }


        public void OpenImagePath(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            StackPanel sp = null;
            if (_mnu.Parent is MenuItem mnu)
            {
                int index = mnu.Items.IndexOf(_mnu);
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                string id = sp.Tag.ToString();
                Movie CurrentMovie = GetMovieFromVieModel(id);

                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id))
                    vieModel.SelectedMovie.Add(CurrentMovie);

                string filepath = "";
                vieModel.SelectedMovie.ToList().ForEach(arg =>
                {
                    filepath = arg.filepath;
                    if (index == 0) { filepath = arg.filepath; }
                    else if (index == 1) { filepath = BasePicPath + $"BigPic\\{arg.id}.jpg"; }
                    else if (index == 2) { filepath = BasePicPath + $"SmallPic\\{arg.id}.jpg"; }
                    else if (index == 3) { filepath = BasePicPath + $"Gif\\{arg.id}.gif"; }
                    else if (index == 4) { filepath = BasePicPath + $"ExtraPic\\{arg.id}\\"; }
                    else if (index == 5) { filepath = BasePicPath + $"ScreenShot\\{arg.id}\\"; }
                    else if (index == 6) { if (arg.actor.Length > 0) filepath = BasePicPath + $"Actresses\\{arg.actor.Split(actorSplitDict[arg.vediotype])[0]}.jpg"; else filepath = ""; }

                    if (index == 4 | index == 5)
                    {

                        if (!FileHelper.TryOpenPath(filepath) && vieModel.SelectedMovie.Count == 1)
                            HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {filepath}", GrowlToken);
                    }
                    else
                    {
                        if (!FileHelper.TryOpenSelectPath(filepath) && vieModel.SelectedMovie.Count == 1)
                            HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {filepath}", GrowlToken);
                    }
                });
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public void OpenFilePath(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            vieModel.SelectedMovie.ToList().ForEach(arg =>
            {
                if (!FileHelper.TryOpenSelectPath(arg.filepath) && vieModel.SelectedMovie.Count == 1)
                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {arg.filepath}", GrowlToken);
            });
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        public async void TranslateMovie(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetYoudao, GrowlToken);
                return;
            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            string result = "";
            MySqlite dataBase = new MySqlite("Translate");


            int successNum = 0;
            int failNum = 0;
            int translatedNum = 0;

            foreach (Movie movie in vieModel.SelectedMovie)
            {

                //检查是否已经翻译过，如有则跳过
                if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id))) { translatedNum++; continue; }
                if (movie.title != "")
                {

                    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.title);
                    //保存
                    if (result != "")
                    {

                        dataBase.SaveYoudaoTranslateByID(movie.id, movie.title, result, "title");

                        //显示
                        int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                        int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                        int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
                        movie.title = result;
                        try
                        {
                            vieModel.CurrentMovieList[index1] = null;
                            vieModel.MovieList[index2] = null;
                            vieModel.FilterMovieList[index3] = null;
                            vieModel.CurrentMovieList[index1] = movie;
                            vieModel.MovieList[index2] = movie;
                            vieModel.FilterMovieList[index3] = movie;
                            successNum++;
                        }
                        catch (ArgumentNullException) { }

                    }

                }
                else { failNum++; }

                if (movie.plot != "")
                {
                    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.plot);
                    //保存
                    if (result != "")
                    {
                        dataBase.SaveYoudaoTranslateByID(movie.id, movie.plot, result, "plot");
                        dataBase.CloseDB();
                    }

                }

            }
            dataBase.CloseDB();
            HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum}", GrowlToken);
            HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FailNum} {failNum}", GrowlToken);
            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SkipNum} {translatedNum}", GrowlToken);

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }



        public async void GenerateActor(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetBaiduAI, GrowlToken); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            this.Cursor = Cursors.Wait;
            int successNum = 0;

            foreach (Movie movie in vieModel.SelectedMovie)
            {
                if (movie.actor == "") continue;
                string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";

                string name;
                if (vieModel.ActorInfoGrid == Visibility.Visible)
                    name = vieModel.Actress.name;
                else
                    name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];


                string ActressesPicPath = Properties.Settings.Default.BasePicPath + $"Actresses\\{name}.jpg";
                if (File.Exists(BigPicPath))
                {
                    Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
                    if (int32Rect != Int32Rect.Empty)
                    {
                        await Task.Delay(500);
                        //切割演员头像
                        System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                        BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                        ImageSource actressImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetActressRect(bitmapImage, int32Rect));
                        System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(actressImage);
                        try { bitmap.Save(ActressesPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++; }
                        catch (Exception ex) { Logger.LogE(ex); }
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_PosterMustExist, GrowlToken);
                }
            }
            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", GrowlToken);
            //更新到窗口中
            foreach (Movie movie1 in vieModel.SelectedMovie)
            {
                if (!string.IsNullOrEmpty(movie1.actor) && movie1.actor.IndexOf(vieModel.Actress.name) >= 0)
                {
                    vieModel.Actress.smallimage = GetActorImage(vieModel.Actress.name);
                    break;
                }
            }



            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }



        public async void GenerateGif(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetFFmpeg, GrowlToken);
                return;
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            string id = GetIDFromMenuItem(sender, 1);
            int successNum = 0;
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            this.Cursor = Cursors.Wait;
            vieModel.CmdText = "";
            vieModel.CmdVisibility = Visibility.Visible;
            foreach (Movie movie in vieModel.SelectedMovie)
            {
                if (Properties.Settings.Default.SkipExistGif && File.Exists(System.IO.Path.Combine(BasePicPath, "GIF", $"{movie.id}.gif")))
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        vieModel.CmdText += $"{movie.id} {Jvedio.Language.Resources.Skip}\n";
                    });

                    continue;
                }


                bool success = false;
                string message = "";
                ScreenShot screenShot = new ScreenShot();
                screenShot.SingleScreenShotCompleted += (s, ev) =>
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Console.WriteLine(((ScreenShotEventArgs)ev).FFmpegCommand);
                        vieModel.CmdText += $"{Jvedio.Language.Resources.SuccessGifTo} ： {((ScreenShotEventArgs)ev).FilePath}\n";
                        RefreshMovieByID(movie.id);
                    });
                };
                (success, message) = await screenShot.AsyncGenrateGif(movie);
                if (success)
                    successNum++;
                else
                    await this.Dispatcher.BeginInvoke((Action)delegate
                    {
                        vieModel.CmdText += $"{movie.id} {Jvedio.Language.Resources.Message_Fail}，{Jvedio.Language.Resources.Reason}：{message}\n";

                    });
            }
            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", GrowlToken);

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }

        private async void ScreenShot(object sender, MouseButtonEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            contextMenu.IsOpen = false;
            await Task.Run(async () =>
            {
                await Task.Delay(300);
            });
            Window_ScreenShot window_ScreenShot = new Window_ScreenShot(this, ImageProcess.GetScreenShot());
            window_ScreenShot.ShowDialog();
        }

        public async void GenerateScreenShot(object sender, RoutedEventArgs e)
        {

            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetFFmpeg, GrowlToken);
                return;
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            int successNum = 0;
            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            this.Cursor = Cursors.Wait;
            vieModel.CmdText = "";
            vieModel.CmdVisibility = Visibility.Visible;
            foreach (Movie movie in vieModel.SelectedMovie)
            {
                bool success = false;
                string message = "";
                ScreenShot screenShot = new ScreenShot();
                screenShot.SingleScreenShotCompleted += (s, ev) =>
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Console.WriteLine(((ScreenShotEventArgs)ev).FFmpegCommand);
                        vieModel.CmdText += $"{Jvedio.Language.Resources.SuccessScreenShotTo} ： {((ScreenShotEventArgs)ev).FilePath}\n";
                        //cmdTextBox.ScrollToEnd();
                    });
                };
                (success, message) = await screenShot.AsyncScreenShot(movie);
                if (success) successNum++;
                else this.Dispatcher.Invoke((Action)delegate { vieModel.CmdText += $"{movie.id} {Jvedio.Language.Resources.Message_Fail}，{Jvedio.Language.Resources.Reason}：{message}\n"; });
            }
            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", GrowlToken);

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;

        }







        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetBaiduAI, GrowlToken); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            int successNum = 0;
            this.Cursor = Cursors.Wait;
            foreach (Movie movie in vieModel.SelectedMovie)
            {
                string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
                string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
                if (File.Exists(BigPicPath))
                {
                    System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                    BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                    if (Properties.Settings.Default.HalfCutOFf)
                    {
                        double rate = 380f / 800f;

                        Int32Rect int32Rect = new Int32Rect() { Height = SourceBitmap.Height, Width = (int)(rate * SourceBitmap.Width), X = (int)((1 - rate) * SourceBitmap.Width), Y = 0 };
                        ImageSource smallImage = ImageProcess.CutImage(bitmapImage, int32Rect);
                        System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
                        try
                        {
                            bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++;
                        }
                        catch (Exception ex) { Logger.LogE(ex); }
                    }
                    else
                    {
                        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
                        if (int32Rect != Int32Rect.Empty)
                        {
                            await Task.Delay(500);
                            //切割缩略图
                            ImageSource smallImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetRect(bitmapImage, int32Rect));
                            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
                            try
                            {
                                bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++;
                            }
                            catch (Exception ex) { Logger.LogE(ex); }
                        }

                    }

                    //读取
                    int index1 = vieModel.CurrentMovieList.IndexOf(movie);
                    int index2 = vieModel.MovieList.IndexOf(movie);
                    int index3 = vieModel.FilterMovieList.IndexOf(movie);
                    movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");

                    vieModel.CurrentMovieList[index1] = null;
                    vieModel.MovieList[index2] = null;
                    vieModel.FilterMovieList[index3] = null;
                    vieModel.CurrentMovieList[index1] = movie;
                    vieModel.MovieList[index2] = movie;
                    vieModel.FilterMovieList[index3] = movie;



                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_PosterMustExist, GrowlToken);
                }

            }
            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", GrowlToken);

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }


        public void RenameFile(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.RenameFormat.IndexOf("{") < 0)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetRenameRule, GrowlToken);
                return;
            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            StringCollection paths = new StringCollection();
            int num = 0;
            vieModel.SelectedMovie.ToList().ForEach(arg => { if (File.Exists(arg.filepath)) { paths.Add(arg.filepath); } });
            if (paths.Count > 0)
            {
                //重命名文件
                foreach (Movie m in vieModel.SelectedMovie)
                {
                    if (!File.Exists(m.filepath)) continue;
                    DetailMovie movie = DataBase.SelectDetailMovieById(m.id);
                    //try
                    //{
                    string[] newPath = movie.ToFileName();
                    if (movie.hassubsection)
                    {
                        for (int i = 0; i < newPath.Length; i++)
                        {
                            File.Move(movie.subsectionlist[i], newPath[i]);
                        }
                        num++;

                        //显示
                        int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                        int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                        int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
                        movie.filepath = newPath[0];
                        movie.subsection = string.Join(";", newPath);
                        try
                        {
                            vieModel.CurrentMovieList[index1].filepath = movie.filepath;
                            vieModel.MovieList[index2].filepath = movie.filepath;
                            vieModel.CurrentMovieList[index1].subsection = movie.subsection;
                            vieModel.MovieList[index2].subsection = movie.subsection;
                            vieModel.FilterMovieList[index3].filepath = movie.filepath;
                            vieModel.FilterMovieList[index3].subsection = movie.subsection;
                        }
                        catch (ArgumentNullException) { }
                        DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                        DataBase.UpdateMovieByID(movie.id, "subsection", movie.subsection, "string");//保存
                        if (vieModel.SelectedMovie.Count == 1) HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, GrowlToken);
                    }
                    else
                    {
                        if (!File.Exists(newPath[0]))
                        {
                            File.Move(movie.filepath, newPath[0]);
                            num++;
                            //显示
                            int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                            int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                            int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
                            movie.filepath = newPath[0];
                            try
                            {
                                vieModel.CurrentMovieList[index1].filepath = movie.filepath;
                                vieModel.MovieList[index2].filepath = movie.filepath;
                                vieModel.FilterMovieList[index3].filepath = movie.filepath;
                            }
                            catch (ArgumentNullException) { }
                            DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                            if (vieModel.SelectedMovie.Count == 1) HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, GrowlToken);
                        }
                        else
                        {
                            //存在同名文件
                            HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_Fail, GrowlToken);
                        }

                    }


                    //}catch(Exception ex)
                    //{
                    //    HandyControl.Controls.Growl.Error(ex.Message);
                    //    continue;
                    //}
                }
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {num}/{vieModel.SelectedMovie.Count} ", GrowlToken);
            }
            else
            {
                //文件不存在！无法重命名！
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, GrowlToken);
            }




            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }



        public void ReMoveZero(object sender, RoutedEventArgs e)
        {


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender, 1));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            int successnum = 0;
            for (int i = 0; i < vieModel.SelectedMovie.Count; i++)
            {
                Movie movie = vieModel.SelectedMovie[i];
                string oldID = movie.id.ToUpper();

                Console.WriteLine(vieModel.CurrentMovieList[0].id);

                if (oldID.IndexOf("-") > 0)
                {
                    string num = oldID.Split('-').Last();
                    string eng = oldID.Remove(oldID.Length - num.Length, num.Length);
                    if (num.Length == 5 && eng.Replace("-", "").All(char.IsLetter))
                    {
                        string newID = eng + num.Remove(0, 2);
                        if (DataBase.SelectMovieByID(newID) == null)
                        {
                            Movie newMovie = DataBase.SelectMovieByID(oldID);
                            DataBase.DeleteByField("movie", "id", oldID);
                            newMovie.id = newID;
                            DataBase.InsertFullMovie(newMovie);
                            UpdateInfo(oldID, newID);
                            successnum++;
                        }
                    }


                }
            }

            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successnum}/{vieModel.SelectedMovie.Count}", GrowlToken);






            vieModel.SelectedMovie.Clear();
            SetSelected();
        }

        private void UpdateInfo(string oldID, string newID)
        {
            Movie movie = DataBase.SelectMovieByID(newID);
            SetImage(ref movie);

            for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
            {
                try
                {
                    if (vieModel.CurrentMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        vieModel.CurrentMovieList[i] = null;
                        vieModel.CurrentMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }


            for (int i = 0; i < vieModel.MovieList.Count; i++)
            {
                try
                {
                    if (vieModel.MovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        vieModel.MovieList[i] = null;
                        vieModel.MovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }

            for (int i = 0; i < vieModel.FilterMovieList.Count; i++)
            {
                try
                {
                    if (vieModel.FilterMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        vieModel.FilterMovieList[i] = null;
                        vieModel.FilterMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }
        }


        public void CopyFile(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            StringCollection paths = new StringCollection();
            int num = 0;
            vieModel.SelectedMovie.ToList().ForEach(arg => { if (File.Exists(arg.filepath)) { paths.Add(arg.filepath); num++; } });

            if (paths.Count > 0 && ClipBoard.TrySetFileDropList(paths, GrowlToken, false))
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_Copied} {num}/{vieModel.SelectedMovie.Count}", GrowlToken);

            if (vieModel.SelectedMovie.Count == 1 && !File.Exists(vieModel.SelectedMovie[0].filepath))
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, GrowlToken);





            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

            int num = 0;
            vieModel.SelectedMovie.ToList().ForEach(arg =>
            {

                if (arg.subsectionlist.Count > 0)
                {
                    //分段视频
                    arg.subsectionlist.ForEach(path =>
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
                });
                }
                else
                {
                    if (File.Exists(arg.filepath))
                    {
                        try
                        {
                            FileSystem.DeleteFile(arg.filepath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                            num++;
                        }
                        catch (Exception ex) { Logger.LogF(ex); }

                    }
                }




            });

            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_DeleteToRecycleBin} {num}/{vieModel.SelectedMovie.Count}", GrowlToken);

            if (num > 0 && Properties.Settings.Default.DelInfoAfterDelFile)
            {
                try
                {
                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {
                        DataBase.DeleteByField("movie", "id", arg.id);
                        vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                        vieModel.MovieList.Remove(arg);
                        vieModel.FilterMovieList.Remove(arg);
                    });

                    //从详情窗口删除
                    if (GetWindowByName("WindowDetails") != null)
                    {
                        WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                        foreach (var item in vieModel.SelectedMovie.ToList())
                        {
                            if (windowDetails.vieModel.DetailMovie.id == item.id)
                            {
                                windowDetails.Close();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                vieModel.Statistic();
            }



            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public WindowEdit WindowEdit;


        public void EditInfo(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_NotEdit, GrowlToken); return; }
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken); return; }
            if (WindowEdit != null) { WindowEdit.Close(); }
            WindowEdit = new WindowEdit(GetIDFromMenuItem(sender));
            WindowEdit.ShowDialog();

        }

        public async void DeleteID(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

            if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToDelete).ShowDialog() == false) { return; }

            vieModel.SelectedMovie.ToList().ForEach(arg =>
            {
                DataBase.DeleteByField("movie", "id", arg.id);
                vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                vieModel.MovieList.Remove(arg);
                vieModel.FilterMovieList.Remove(arg);
            });

            //从详情窗口删除
            if (GetWindowByName("WindowDetails") != null)
            {
                WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                foreach (var item in vieModel.SelectedMovie.ToList())
                {
                    if (windowDetails.vieModel.DetailMovie.id == item.id)
                    {
                        windowDetails.Close();
                        break;
                    }
                }
            }

            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.SuccessDelete} {vieModel.SelectedMovie.Count} ", GrowlToken);
            //修复数字显示
            vieModel.CurrentCount -= vieModel.SelectedMovie.Count;
            vieModel.TotalCount -= vieModel.SelectedMovie.Count;

            vieModel.SelectedMovie.Clear();
            vieModel.Statistic();

            await Task.Run(() => { Task.Delay(1000).Wait(); });


            vieModel.SelectedMovie.Clear();
            Properties.Settings.Default.EditMode = false;
            SetSelected();
        }


        public void DeleteInfo(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

            if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToClearInfo).ShowDialog() == false) { return; }

            vieModel.SelectedMovie.ToList().ForEach(arg =>
            {
                DataBase.ClearInfoByID(arg.id);
                RefreshMovieByID(arg.id);
            });
            vieModel.SelectedMovie.Clear();
            Properties.Settings.Default.EditMode = false;
            SetSelected();
        }

        public Movie GetMovieFromVieModel(string id)
        {
            foreach (Movie movie in vieModel.CurrentMovieList)
            {
                if (movie.id == id)
                {
                    return movie;
                }
            }
            return null;
        }

        public Actress GetActressFromVieModel(string name)
        {
            foreach (Actress actress in vieModel.ActorList)
            {
                if (actress.name == name)
                {
                    return actress;
                }
            }
            return null;
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


        //清空标签
        public void ClearLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender, 1));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

            foreach (var movie in this.vieModel.MovieList)
            {
                foreach (var item in vieModel.SelectedMovie)
                {
                    if (item.id == movie.id)
                    {
                        DataBase.UpdateMovieByID(item.id, "label", "", "String");
                        break;
                    }
                }
            }
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);

            vieModel.GetLabelList();


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

        }



        //删除单个影片标签
        public void DelSingleLabel(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode)
            {
                //HandyControl.Controls.Growl.Info("不支持批量", GrowlToken);
                return;
            }

            DetailMovie CurrentMovie = DataBase.SelectDetailMovieById(GetIDFromMenuItem(sender, 1));
            LabelDelGrid.Visibility = Visibility.Visible;
            vieModel.CurrentMovieLabelList = new List<string>();
            vieModel.CurrentMovieLabelList = CurrentMovie.label.Split(' ').ToList();
            CurrentLabelMovie = CurrentMovie;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;


        }


        //删除多个影片标签
        public void DelLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender, 1));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

            //string TotalLabel = GetFormatGenreString(vieModel.SelectedMovie,"label");
            var di = new DialogInput(this, Jvedio.Language.Resources.InputTitle6, "");
            di.ShowDialog();
            if (di.DialogResult == true & di.Text != "")
            {
                foreach (var movie in this.vieModel.MovieList)
                {
                    foreach (var item in vieModel.SelectedMovie)
                    {
                        if (item.id == movie.id)
                        {
                            List<string> originlabel = LabelToList(movie.label);
                            List<string> newlabel = LabelToList(di.Text);
                            movie.label = string.Join(" ", originlabel.Except(newlabel).ToList());
                            DataBase.UpdateMovieByID(item.id, "label", movie.label, "String");
                            break;
                        }
                    }

                }
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);
                vieModel.GetLabelList();
            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        //增加标签
        public void AddLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender, 1));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            var di = new DialogInput(this, Jvedio.Language.Resources.InputTitle1, "");
            di.ShowDialog();
            if (di.DialogResult == true & di.Text != "")
            {
                foreach (var movie in this.vieModel.MovieList)
                {
                    foreach (var item in vieModel.SelectedMovie)
                    {
                        if (item.id == movie.id)
                        {
                            List<string> originlabel = LabelToList(movie.label);
                            List<string> newlabel = LabelToList(di.Text);
                            movie.label = string.Join(" ", originlabel.Union(newlabel).ToList());
                            originlabel.ForEach(arg => Console.WriteLine(arg));
                            newlabel.ForEach(arg => Console.WriteLine(arg));
                            originlabel.Union(newlabel).ToList().ForEach(arg => Console.WriteLine(arg));
                            DataBase.UpdateMovieByID(item.id, "label", movie.label, "String");
                            break;
                        }
                    }

                }
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);

                vieModel.GetLabelList();

            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }





        //打开网址
        private void OpenWeb(object sender, RoutedEventArgs e)
        {

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            Movie CurrentMovie = GetMovieFromVieModel(GetIDFromMenuItem(sender));
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

            vieModel.SelectedMovie.ToList().ForEach(arg =>
            {
                if (arg.sourceurl.IsProperUrl())
                {
                    FileHelper.TryOpenUrl(arg.GetSourceUrl(), GrowlToken);
                }
                else
                {
                    //为空则使用 bus 打开
                    if (!string.IsNullOrEmpty(JvedioServers.Bus.Url) && JvedioServers.Bus.Url.IsProperUrl())
                    {
                        FileHelper.TryOpenUrl(JvedioServers.Bus.Url + arg.id, GrowlToken);
                    }
                    else if (arg.id.StartsWith("FC2") && JvedioServers.FC2.Url.IsProperUrl())
                    {
                        FileHelper.TryOpenUrl($"{JvedioServers.FC2.Url}article/{arg.id}/", GrowlToken);
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_UrlNotSet, GrowlToken);
                    }

                }
            });

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        private string GetIDFromMenuItem(object sender, int depth = 0)
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
            StackPanel sp = contextMenu.PlacementTarget as StackPanel;
            return sp.Tag.ToString();
        }


        private void DownLoadSelectMovie(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken);
            }
            else if (!JvedioServers.IsProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetUrl, GrowlToken);
            }
            else
            {
                try
                {
                    if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                    string id = GetIDFromMenuItem(sender);
                    Movie CurrentMovie = GetMovieFromVieModel(id);
                    if (CurrentMovie != null)
                    {
                        if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                        StartDownload(vieModel.SelectedMovie.ToList());
                    }


                }
                catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        private void ForceToDownLoad(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken);
            }
            else if (!JvedioServers.IsProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetUrl, GrowlToken);
            }
            else
            {
                try
                {
                    if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                    string id = GetIDFromMenuItem(sender, 1);
                    Movie CurrentMovie = GetMovieFromVieModel(id);
                    if (CurrentMovie != null)
                    {
                        if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                        StartDownload(vieModel.SelectedMovie.ToList(), true);
                    }


                }
                catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }
        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ImageSlides == null) return;
            Canvas canvas = (Canvas)sender;
            int index = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == canvas.ToolTip.ToString()).FirstOrDefault());
            if (index < ImageSlides.Count)
            {
                ImageSlides[index].LoadAllImage();
                ImageSlides[index].Start();
            }

        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImageSlides == null) return;
            Canvas canvas = (Canvas)sender;
            int index = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == canvas.ToolTip.ToString()).FirstOrDefault());
            if (index >= 0 && index < ImageSlides.Count)
            {
                ImageSlides[index].Stop();
            }

        }



        private void EditActress(object sender, MouseButtonEventArgs e)
        {
            vieModel.EnableEditActress = !vieModel.EnableEditActress;
            //Console.WriteLine(vieModel.Actress.age); 
        }

        private void SaveActress(object sender, KeyEventArgs e)
        {

            if (vieModel.EnableEditActress && e.Key == Key.Enter)
            {
                FocusTextBox.Focus();
                vieModel.EnableEditActress = false;
                //Console.WriteLine(vieModel.Actress.age);
                DataBase.InsertActress(vieModel.Actress);
            }


        }

        private void BeginDownLoadActress(object sender, MouseButtonEventArgs e)
        {
            List<Actress> actresses = new List<Actress>();
            actresses.Add(vieModel.Actress);
            DownLoadActress downLoadActress = new DownLoadActress(actresses);
            downLoadActress.BeginDownLoad();
            downLoadActress.InfoUpdate += (s, ev) =>
            {
                ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
                try
                {
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        vieModel.Actress = null;
                        vieModel.Actress = actressUpdateEventArgs.Actress;
                        downLoadActress.State = DownLoadState.Completed;
                    });
                }
                catch (TaskCanceledException ex) { Logger.LogE(ex); }

            };

            downLoadActress.MessageCallBack += (s, ev) =>
            {
                MessageCallBackEventArgs actressUpdateEventArgs = ev as MessageCallBackEventArgs;
                HandyControl.Controls.Growl.Info(actressUpdateEventArgs.Message, GrowlToken);

            };


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



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                if (this.WindowState == WindowState.Normal) WinState = JvedioWindowState.Normal;
                else if (this.WindowState == WindowState.Maximized) WinState = JvedioWindowState.FullScreen;
                else if (this.Width == SystemParameters.WorkArea.Width & this.Height == SystemParameters.WorkArea.Height) WinState = JvedioWindowState.Maximized;

                WindowConfig cj = new WindowConfig(this.GetType().Name);
                cj.Save(new WindowProperty() { Location = new Point(this.Left, this.Top), Size = new Size(this.Width, this.Height), WinState = WinState });
            }
            Properties.Settings.Default.EditMode = false;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();

            if (!IsToUpdate && Properties.Settings.Default.CloseToTaskBar && this.IsVisible == true)
            {
                e.Cancel = true;
                vieModel.HideToIcon = true;
                this.Hide();
                WindowSet?.Hide();
            }


        }

        private void GoToActorPage(object sender, KeyEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (e.Key == Key.Enter)
            {
                string pagestring = ((TextBox)sender).Text;
                int page = 1;
                if (pagestring == null) { page = 1; }
                else
                {
                    var isnumeric = int.TryParse(pagestring, out page);
                }
                if (page > vieModel.TotalActorPage) { page = vieModel.TotalActorPage; } else if (page <= 0) { page = 1; }
                vieModel.CurrentActorPage = page;
                vieModel.ActorFlipOver();
            }
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
            if (DownLoader != null && DownLoader.State == DownLoadState.DownLoading) HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_Stop, GrowlToken);
            DownLoader?.CancelDownload();
            downLoadActress?.CancelDownload();
            this.Dispatcher.BeginInvoke((Action)delegate { vieModel.ProgressBarVisibility = Visibility.Hidden; });


        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }


        private void PreviousActorPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (vieModel.CurrentActorPage - 1 <= 0)
                vieModel.CurrentActorPage = vieModel.TotalActorPage;
            else
                vieModel.CurrentActorPage -= 1;
            vieModel.ActorFlipOver();


        }

        private void NextActorPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (vieModel.CurrentActorPage + 1 > vieModel.TotalActorPage)
                vieModel.CurrentActorPage = 1;
            else
                vieModel.CurrentActorPage += 1;
            vieModel.ActorFlipOver();
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
            if (Properties.Settings.Default.EasyMode)
                SimpleMovieScrollViewer.ScrollToTop();
            else
                MovieScrollViewer.ScrollToTop();

        }

        private void PreviousPage(object sender, MouseButtonEventArgs e)
        {

            if (vieModel.TotalPage <= 1) return;
            if (vieModel.CurrentPage - 1 <= 0) vieModel.CurrentPage = vieModel.TotalPage;
            else vieModel.CurrentPage -= 1;

            vieModel.AsyncFlipOver();
            if (Properties.Settings.Default.EasyMode)
                SimpleMovieScrollViewer.ScrollToTop();
            else
                MovieScrollViewer.ScrollToTop();

        }







        private void ActorGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.ActorEditMode)
            {
                foreach (var item in vieModel.ActorList)
                {
                    if (!SelectedActress.Contains(item))
                    {
                        SelectedActress.Add(item);

                    }
                }
                ActorSetSelected();
            }
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.EditMode)
            {
                foreach (var item in vieModel.CurrentMovieList)
                {
                    if (!vieModel.SelectedMovie.Contains(item))
                    {
                        vieModel.SelectedMovie.Add(item);
                    }
                }
                SetSelected();
            }
        }

        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }

        public void StopDownLoadActress(object sender, RoutedEventArgs e)
        {
            DownloadActorPopup.IsOpen = false;
            downLoadActress?.CancelDownload();
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Stop, GrowlToken);
            vieModel.ActorProgressBarVisibility = Visibility.Collapsed;
        }

        public void DownLoadSelectedActor(object sender, RoutedEventArgs e)
        {
            if (downLoadActress?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken); return;
            }

            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            StackPanel sp = null;
            if (sender is MenuItem mnu)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                string name = sp.Tag.ToString();
                Actress CurrentActress = GetActressFromVieModel(name);
                if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
                StartDownLoadActor(SelectedActress);

            }
            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
        }

        public void LikeSelectedActor(object sender, RoutedEventArgs e)
        {

            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            StackPanel sp = null;
            if (sender is MenuItem mnu)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                string name = sp.Tag.ToString();
                Actress CurrentActress = GetActressFromVieModel(name);
                if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
                DataBase.CreateTable(DataBase.SQLITETABLE_ACTRESS_LOVE);
                foreach (Actress actress in SelectedActress)
                {
                    Actress newActress = DataBase.SelectInfoByActress(actress);
                    DataBase.SaveActressLikeByName(newActress.name, newActress.like == 0 ? 1 : 0);
                }

            }
            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();

        }

        public void SelectAllActor(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ActorEditMode) { ActorCancelSelect(); return; }
            Properties.Settings.Default.ActorEditMode = true;
            foreach (var item in vieModel.CurrentActorList)
                if (!SelectedActress.Contains(item)) SelectedActress.Add(item);

            ActorSetSelected();
        }

        public void ActorCancelSelect()
        {
            Properties.Settings.Default.ActorEditMode = false; SelectedActress.Clear(); ActorSetSelected();
        }

        public void RefreshCurrentActressPage(object sender, RoutedEventArgs e)
        {
            ActorCancelSelect();
            if ((bool)LoveCheckBox.IsChecked)
            {
                ShowLoveActors(null, null);
            }
            else
            {
                vieModel.RefreshActor();
            }

        }

        public void StartDownLoadActor(List<Actress> actresses)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "BusActress.sqlite")) return;

            downLoadActress = new DownLoadActress(actresses);


            downLoadActress.InfoUpdate += (s, ev) =>
                {
                    ActressUpdateEventArgs ae = ev as ActressUpdateEventArgs;
                    var actores = vieModel.ActorList.Where(arg => arg.name.ToUpper() == ae.Actress.name.ToUpper()).ToList();
                    if (actores == null || actores.Count == 0) return;
                    int idx = vieModel.ActorList.IndexOf(actores.First());
                    if (idx >= vieModel.ActorList.Count) return;

                    vieModel.ActorList[idx] = ae.Actress;
                    vieModel.ActorProgressBarValue = (int)(ae.progressBarUpdate.value / ae.progressBarUpdate.maximum * 100);
                    if (vieModel.ActorProgressBarValue == 100) downLoadActress.State = DownLoadState.Completed;
                    if (vieModel.ActorProgressBarValue == 100 || ae.state == DownLoadState.Fail || ae.state == DownLoadState.Completed) vieModel.ActorProgressBarVisibility = Visibility.Hidden;
                };



            downLoadActress?.BeginDownLoad();
        }


        DownLoadActress downLoadActress;

        /// <summary>
        /// 同步演员信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartDownLoadActress(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.ActressDownloadAttention, GrowlToken);
            DownloadActorPopup.IsOpen = false;
            if (!JvedioServers.Bus.IsEnable)
            {
                HandyControl.Controls.Growl.Info($"BUS {Jvedio.Language.Resources.Message_NotOpenOrNotEnable}", GrowlToken);
                return;
            }

            if (DownLoader?.State == DownLoadState.DownLoading)
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken);
            else
                StartDownLoadActor(vieModel.CurrentActorList.ToList());



        }




        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (vieModel.ProgressBarVisibility == Visibility.Hidden && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }


        }


        public void ShowSubSection(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();
            string id = button.Tag.ToString();
            Movie movie = vieModel.CurrentMovieList.Where(arg => arg.id == id).FirstOrDefault();
            if (movie != null)
            {
                for (int i = 0; i < movie.subsectionlist.Count; i++)
                {
                    string filepath = movie.subsectionlist[i];//这样可以，放在  PlayVideoWithPlayer 就超出索引
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = i + 1;
                    menuItem.Click += (s, _) =>
                    {
                        PlayVideoWithPlayer(filepath, id);
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
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            //WaitingPanel.Visibility = Visibility.Visible;
            vieModel.IsScanning = true;
            scan_cts = new CancellationTokenSource();
            scan_cts.Token.Register(() => { Console.WriteLine("取消任务"); });
            scan_ct = scan_cts.Token;

            await Task.Run(() =>
            {
                //分为文件夹和文件
                string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> files = new List<string>();
                StringCollection stringCollection = new StringCollection();
                foreach (var item in dragdropFiles)
                {
                    if (IsFile(item))
                        files.Add(item);
                    else
                        stringCollection.Add(item);
                }
                List<string> filepaths = new List<string>();
                //扫描导入
                if (stringCollection.Count > 0)
                    filepaths = Scan.ScanPaths(stringCollection, scan_ct);

                if (files.Count > 0) filepaths.AddRange(files);

                Scan.InsertWithNfo(filepaths, scan_ct, (message) => { HandyControl.Controls.Growl.Info(message, GrowlToken); });
                Task.Delay(300).Wait();
            });
            //WaitingPanel.Visibility = Visibility.Hidden;
            scan_cts.Dispose();
            vieModel.IsScanning = false;
            vieModel.Reset();
        }



        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void Button_StopDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;
            StopDownLoad();

        }

        private void Button_StartDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;

            if (!JvedioServers.IsProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetUrl, GrowlToken);

            }
            else
            {
                if (DownLoader?.State == DownLoadState.DownLoading)
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken);
                else
                    StartDownload(vieModel.CurrentMovieList.ToList());
            }


        }







        public void ShowSettingsPopup(object sender, MouseButtonEventArgs e)
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


        private void ClearRecentWatched(object sender, RoutedEventArgs e)
        {
            if (new RecentWatchedConfig("").Clear())
            {
                ReadRecentWatchedFromConfig();
                vieModel.AddToRecentWatch("");
            }
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.FirstRun)
            {
                vieModel.ShowFirstRun = Visibility.Visible;
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }
            AdjustWindow();
            FadeIn();
            SetSkin();
            //显示公告
            ShowNotice();
            InitList();
            //检查更新
            CheckUpgrade(null, null);
            this.Cursor = Cursors.Arrow;
            //设置当前数据库
            for (int i = 0; i < vieModel.DataBases.Count; i++)
            {
                if (vieModel.DataBases[i].ToLower() == System.IO.Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    vieModel.DatabaseSelectedIndex = i;
                    break;
                }
            }
            ReadRecentWatchedFromConfig();//显示最近播放
            vieModel.AddToRecentWatch("");
            //vieModel.GetFilterInfo();

            //设置排序类型
            var radioButtons = SortStackPanel.Children.OfType<RadioButton>().ToList();
            int.TryParse(Properties.Settings.Default.SortType, out int idx);
            radioButtons[idx].IsChecked = true;

            //设置图片类型
            var rbs = ImageTypeStackPanel.Children.OfType<RadioButton>().ToList();
            int.TryParse(Properties.Settings.Default.ShowImageMode, out int idx2);
            rbs[idx2].IsChecked = true;

            InitList();
            vieModel.InitLettersNavigation();

        }

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
                    SideBorder.Background = myLinearGradientBrush;
                    break;

                default:
                    SideBorder.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                    break;
            }

            if (BackgroundImage != null)
            {
                SideBorder.Background = Brushes.Transparent;
                TitleBorder.Background = Brushes.Transparent;
                MainProgressBar.Background = Brushes.Transparent;
                ActorProgressBar.Background = Brushes.Transparent;
                foreach (Expander expander in ExpanderStackPanel.Children.OfType<Expander>().ToList())
                {
                    expander.Background = Brushes.Transparent;
                    Border border = expander.Content as Border;
                    border.Background = Brushes.Transparent;
                }
                BgImage.Source = BackgroundImage;
            }
            else
            {
                TitleBorder.Background = (SolidColorBrush)Application.Current.Resources["BackgroundTitle"];
                MainProgressBar.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                ActorProgressBar.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                foreach (Expander expander in ExpanderStackPanel.Children.OfType<Expander>().ToList())
                {
                    expander.Background = (SolidColorBrush)Application.Current.Resources["BackgroundTitle"];
                    Border border = expander.Content as Border;
                    border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
                }
            }


        }

        public void InitList()
        {
            MySqlite dB = new MySqlite("mylist");
            List<string> tables = dB.GetAllTable();
            vieModel.MyList = new ObservableCollection<MyListItem>();
            foreach (string table in tables)
            {
                vieModel.MyList.Add(new MyListItem(table, (long)dB.SelectCountByTable(table)));
            }
            dB.Close();
            ListItemsControl.ItemsSource = null;
            ListItemsControl.ItemsSource = vieModel.MyList;//以前直接在 xaml 中绑定即可，现在不知为啥必须写代码绑定
        }






        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedMovie.Clear();
            SetSelected();
        }



        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
            OpenTools(sender, e);
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
                    vieModel.CurrentActorPage = vieModel.TotalActorPage;
                    vieModel.ActorFlipOver();
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
                    vieModel.ActorFlipOver();
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
            else if (vieModel.TabSelectedIndex == 0 && e.Key == Key.Right)
                NextPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (vieModel.TabSelectedIndex == 0 && e.Key == Key.Left)
                PreviousPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (vieModel.TabSelectedIndex == 1 && e.Key == Key.Right)
                NextActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (vieModel.TabSelectedIndex == 1 && e.Key == Key.Left)
                PreviousActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));




        }


        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            string name = e.AddedItems[0].ToString().ToLower();
            string path = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite";
            if (!File.Exists(path))
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists);
            }
            else
            {
                if (name != System.IO.Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    Properties.Settings.Default.DataBasePath = path;
                    //切换数据库
                    vieModel.IsRefresh = true;
                    vieModel.Reset();
                    vieModel.InitLettersNavigation();
                    vieModel.GetFilterInfo();
                }
            }


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



        private bool IsDragingSideGrid = false;

        private void DragRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender.GetType().Name == "Border") vieModel.ShowSearchPopup = false;
            if (vieModel.SideBorderWidth >= 200) { if (sender is Rectangle rectangle) rectangle.Cursor = Cursors.SizeWE; }
            if (IsDragingSideGrid)
            {
                this.Cursor = Cursors.SizeWE;
                double width = e.GetPosition(this).X;
                if (width > 500 || width < 200)
                    return;
                else
                {
                    vieModel.SideBorderWidth = width;
                }

            }
        }

        private void DragRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && vieModel.SideBorderWidth >= 200)
            {
                IsDragingSideGrid = true;
            }
        }

        private void DragRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsDragingSideGrid = false;
            Properties.Settings.Default.SideGridWidth = vieModel.SideBorderWidth;
            Properties.Settings.Default.Save();
        }


        private void SetSelectMode(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedMovie.Clear();
            SetSelected();
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
            foreach (MenuItem item in contextMenu.Items)
            {
                if (item.Header.ToString() == header)
                {
                    return item;
                }
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


        private void ShowSearchPopup(object sender, MouseButtonEventArgs e)
        {
            vieModel.ShowSearchPopup = true;
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
            else if (Properties.Settings.Default.ShowImageMode == "2")
            {
                Properties.Settings.Default.ExtraImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.ExtraImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
            else if (Properties.Settings.Default.ShowImageMode == "3")
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
            string id = stackPanel.Tag.ToString();

            if (vieModel.CurrentMovieList != null && vieModel.CurrentMovieList.Count > 0)
            {
                foreach (var item in vieModel.CurrentMovieList)
                {
                    if (item != null)
                    {
                        if (item.id.ToUpper() == id.ToUpper())
                        {
                            string table = GetCurrentList();
                            if (!string.IsNullOrEmpty(table))
                            {
                                using (MySqlite mySqlite = new MySqlite("mylist"))
                                {
                                    mySqlite.ExecuteSql($"update {table} set favorites={ item.favorites} where id='{item.id}'");
                                }
                            }
                            else
                            {
                                DataBase.UpdateMovieByID(item.id, "favorites", item.favorites, "string");
                            }
                            vieModel.Statistic();
                            break;
                        }
                    }


                }

            }
            CanRateChange = false;



        }

        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }

        private void ManageLabel(object sender, RoutedEventArgs e)
        {
            vieModel.GetLabelList();
            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);



            LabelGrid.Visibility = Visibility.Visible;
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    toggleButton.IsChecked = false;
                }
            }


        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {



            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        Match match = Regex.Match(toggleButton.Content.ToString(), @"\( \d+ \)");
                        if (match != null && match.Value != "")
                        {
                            string label = toggleButton.Content.ToString().Replace(match.Value, "");
                            if (!originLabels.Contains(label)) originLabels.Add(label);
                        }

                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                //HandyControl.Controls.Growl.Warning("请选择标签！", GrowlToken);
                return;
            }


            foreach (Movie movie in vieModel.SelectedMovie)
            {
                List<string> labels = LabelToList(movie.label);
                labels = labels.Union(originLabels).ToList();
                movie.label = string.Join(" ", labels);
                DataBase.UpdateMovieByID(movie.id, "label", movie.label, "String");
            }
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            LabelGrid.Visibility = Visibility.Hidden;

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
                //HandyControl.Controls.Growl.Warning("请选择标签！", GrowlToken);
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

            if (vieModel.CurrentMovieList.Count == 0)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);
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
                //HandyControl.Controls.Growl.Warning("请选择标签！", GrowlToken);
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

            if (vieModel.CurrentMovieList.Count == 0)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);
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


        WindowBatch WindowBatch;

        private void OpenBatching(object sender, RoutedEventArgs e)
        {
            if (WindowBatch != null) WindowBatch.Close();
            WindowBatch = new WindowBatch();
            WindowBatch.Show();
        }

        private void Test(object sender, RoutedEventArgs e)
        {
            DoubleAnimation verticalAnimation = new DoubleAnimation();
            verticalAnimation.From = 0;
            verticalAnimation.To = 200;
            verticalAnimation.Duration = FadeInterval;
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(verticalAnimation);
            Storyboard.SetTarget(verticalAnimation, MovieScrollViewer);
            Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
            storyboard.Begin();

        }






        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Width == SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height)
            {
                vieModel.MainGridThickness = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                vieModel.MainGridThickness = new Thickness(5);
                this.ResizeMode = ResizeMode.NoResize;
                MaxButtonPath.Data = Geometry.Parse(PathData.MaxToNormalPath);
            }
            else
            {
                vieModel.MainGridThickness = new Thickness(10);
                this.ResizeMode = ResizeMode.CanResize;
                MaxButtonPath.Data = Geometry.Parse(PathData.MaxPath);
            }




        }



        private async void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {
            string id = GetIDFromMenuItem(sender, 1);

            DialogInput dialogInput = new DialogInput(this, Jvedio.Language.Resources.InputTitle2);
            if (dialogInput.ShowDialog() == true)
            {
                string url = dialogInput.Text;
                if (!url.StartsWith("http"))
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_WrongUrl, GrowlToken);
                }
                else
                {

                    string host = new Uri(url).Host;
                    WebSite webSite = await new MyNet().CheckUrlType(url.Split(':')[0] + "://" + host);
                    if (webSite == WebSite.None)
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_NotRecognize, GrowlToken);
                    }
                    else
                    {
                        if (webSite == WebSite.DMM || webSite == WebSite.Jav321)
                        {
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.NotSupport, GrowlToken);
                        }
                        else
                        {
                            HandyControl.Controls.Growl.Info($"{webSite} {Jvedio.Language.Resources.Message_BeginParse}", GrowlToken);
                            bool result = await MyNet.ParseSpecifiedInfo(webSite, id, url);
                            if (result)
                            {
                                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_BeginDownloadImage, GrowlToken);
                                //更新到主界面
                                RefreshMovieByID(id);

                                //下载图片
                                DetailMovie dm = DataBase.SelectDetailMovieById(id);
                                //下载小图
                                await MyNet.DownLoadSmallPic(dm, true);
                                dm.smallimage = ImageProcess.GetBitmapImage(dm.id, "SmallPic");
                                RefreshMovieByID(id);


                                if (dm.sourceurl?.IndexOf("fc2club") >= 0)
                                {
                                    //复制大图
                                    if (File.Exists(GlobalVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg") & !File.Exists(GlobalVariable.BasePicPath + $"BigPic\\{dm.id}.jpg"))
                                    {
                                        FileHelper.TryCopyFile(GlobalVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg", GlobalVariable.BasePicPath + $"BigPic\\{dm.id}.jpg");
                                    }
                                }
                                else
                                {
                                    //下载大图
                                    await MyNet.DownLoadBigPic(dm, true);
                                }
                                dm.bigimage = ImageProcess.GetBitmapImage(dm.id, "BigPic");
                                RefreshMovieByID(id);


                            }
                            else
                            {
                                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_Fail, GrowlToken);
                            }
                        }
                    }
                }


            }
        }

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            if (IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{vieModel.Actress.name}.jpg", true);
                    Actress actress = vieModel.Actress;
                    actress.smallimage = null;
                    actress.smallimage = GetActorImage(actress.name);
                    vieModel.Actress = null;
                    vieModel.Actress = actress;

                    if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actress.name)
                        {
                            vieModel.ActorList[i] = actress;
                            break;
                        }
                    }

                }
                else
                {
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_OnlySupportJPG, GrowlToken);
                }
            }
        }

        private void ActorImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            string name = textBox.Text.Split('(')[0];

            Actress currentActress = null;
            for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            {
                if (vieModel.CurrentActorList[i].name == name)
                {
                    currentActress = vieModel.CurrentActorList[i];
                    break;
                }
            }

            if (currentActress == null) return;


            if (IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{currentActress.name}.jpg", true);
                    Actress actress = currentActress;
                    actress.smallimage = null;
                    actress.smallimage = GetActorImage(actress.name);

                    if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actress.name)
                        {
                            vieModel.ActorList[i] = null;
                            vieModel.ActorList[i] = actress;
                            break;
                        }
                    }

                    for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
                    {
                        if (vieModel.CurrentActorList[i].name == actress.name)
                        {
                            vieModel.CurrentActorList[i] = null;
                            vieModel.CurrentActorList[i] = actress;
                            break;
                        }
                    }

                }
                else
                {
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_OnlySupportJPG, GrowlToken);
                }
            }
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
            RadioButton radioButton = sender as RadioButton;
            string listName = radioButton.Content.ToString();
            vieModel.ExecutiveSqlCommand(10, listName, $"select * from {listName}", "mylist");




        }


        private void EditListItem(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            RadioButton radioButton = contextMenu.PlacementTarget as RadioButton;

            string oldName = radioButton.Content.ToString();


            var r = new DialogInput(this, Jvedio.Language.Resources.InputTitle3, oldName);
            if (r.ShowDialog() == true)
            {
                string text = r.Text;
                if (text != "" & text != "+" & text.IndexOf(" ") < 0)
                {
                    if (vieModel.MyList.Where(arg => arg.Name == text).Count() > 0)
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_AlreadyExist, GrowlToken);
                        return;
                    }
                    //重命名
                    if (Rename(oldName, text))
                    {
                        radioButton.Content = text;
                        for (int i = 0; i < vieModel.MyList.Count; i++)
                        {
                            if (vieModel.MyList[i].Name == oldName)
                            {
                                vieModel.MyList[i].Name = text;
                            }
                        }
                    }

                }

            }
        }

        private bool Rename(string oldName, string newName)
        {
            MySqlite dB = new MySqlite("mylist");
            if (!dB.IsTableExist(oldName))
            {
                dB.CloseDB();
                return false;
            }
            try
            {
                dB.CreateTable($"ALTER TABLE {oldName} RENAME TO {newName}");
            }
            catch
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotSupport, GrowlToken);
                return false;
            }
            finally
            {
                dB.CloseDB();
            }
            return true;
        }

        public RadioButton ListRadibutton;

        private void RemoveListItem(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            ListRadibutton = contextMenu.PlacementTarget as RadioButton;

            if (ListRadibutton == null) return;
            if (new Msgbox(this, Jvedio.Language.Resources.IsToRemove).ShowDialog() == true)
            {

                string listName = ListRadibutton.Content.ToString();
                MyListItem myListItem = vieModel.MyList.Where(arg => arg.Name == listName).FirstOrDefault();
                vieModel.MyList.Remove(myListItem);
                ReMoveFromMyList(listName);
                vieModel.CurrentMovieList.Clear();
            }

        }


        private void AddListItem(object sender, RoutedEventArgs e)
        {
            var r = new DialogInput(this, Jvedio.Language.Resources.InputTitle4);
            if (r.ShowDialog() == true)
            {
                string text = r.Text;
                if (text != "" & text != "+" & text.IndexOf(" ") < 0)
                {
                    if (vieModel.MyList.Where(arg => arg.Name == text).Count() > 0)
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_AlreadyExist, GrowlToken);
                        return;
                    }
                    if (AddToMyList(text)) vieModel.MyList.Add(new MyListItem(text, 0));
                    ListItemsControl.ItemsSource = null;
                    ListItemsControl.ItemsSource = vieModel.MyList;
                }

            }
        }

        private bool AddToMyList(string name)
        {
            MySqlite dB = new MySqlite("mylist");
            if (dB.IsTableExist(name))
            {
                dB.CloseDB();
                return false;
            }
            name = DataBase.SQLITETABLE_MOVIE.Replace("movie", name);
            try
            {
                if (!dB.IsTableExist(name)) dB.CreateTable(name);
            }
            catch
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotSupport, GrowlToken);
                return false;
            }
            finally
            {
                dB.CloseDB();
            }
            return true;

        }

        private void ReMoveFromMyList(string name)
        {
            MySqlite dB = new MySqlite("mylist");
            if (dB.IsTableExist(name)) dB.DeleteTable(name);
            dB.CloseDB();
        }



        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (vieModel.IsLoadingMovie)
            {
                e.Handled = true;
                return;
            }

            bool isInList = false;
            for (int i = 0; i < ListItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
                StackPanel sp = FindElementByName<StackPanel>(c, "ListStackPanel");
                if (sp != null)
                {
                    var grids = sp.Children.OfType<Grid>().ToList();
                    foreach (Grid grid in grids)
                    {
                        RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
                        if (radioButton != null && (bool)radioButton.IsChecked)
                        {
                            isInList = true;
                            break;

                        }
                    }

                }
            }

            StackPanel stackPanel = sender as StackPanel;
            ContextMenu contextMenu = stackPanel.ContextMenu;

            if (isInList)
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                    if (item.Header.ToString() != Jvedio.Language.Resources.Menu_RemoveFromList)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                    if (item.Header.ToString() == Jvedio.Language.Resources.Menu_RemoveFromList)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }





            if (contextMenu.Visibility != Visibility.Visible) return;
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                this.Dispatcher.Invoke(() =>
                {
                    MenuItem menuItem = FindElementByName<MenuItem>(contextMenu, "ListMenuItem");
                    if (menuItem != null)
                    {
                        menuItem.Items.Clear();
                        foreach (var item in vieModel.MyList)
                        {
                            MenuItem menuItem1 = new MenuItem();
                            menuItem1.Header = item.Name;
                            //menuItem1.Name = item.Name;
                            menuItem1.Click += MyListItemClick;
                            menuItem.Items.Add(menuItem1);
                        }
                    }
                });
            });


        }


        private void MyListItemClick(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            string table = menuItem.Header.ToString();
            string id = GetIDFromMenuItem(sender, 1);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            foreach (Movie movie in vieModel.SelectedMovie)
            {
                Movie newMovie = DataBase.SelectMovieByID(movie.id);
                MySqlite dB = new MySqlite("mylist");
                dB.InsertFullMovie(newMovie, table);
                dB.CloseDB();
                InitList();
            }

        }

        private void Rate_ValueChanged_1(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (vieModel.Actress != null)
            {
                DataBase.CreateTable(DataBase.SQLITETABLE_ACTRESS_LOVE);
                DataBase.SaveActressLikeByName(vieModel.Actress.name, vieModel.Actress.like);
            }
        }

        private void ShowLoveActors(object sender, RoutedEventArgs e)
        {

            vieModel.CurrentActorPage = 1;
            List<string> actressNames = DataBase.SelectActressNameByLove(1);

            List<Actress> oldActress = vieModel.ActorList.ToList();
            List<Actress> newActress = new List<Actress>();
            vieModel.ActorList = new ObservableCollection<Actress>();
            foreach (Actress actress in oldActress)
            {
                if (actressNames.Contains(actress.name))
                {
                    newActress.Add(actress);
                }
            }

            vieModel.ActorList = new ObservableCollection<Actress>();
            vieModel.ActorList.AddRange(newActress);

            vieModel.ActorFlipOver();

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            vieModel.GetActorList();
        }

        private void OpenLogPath(object sender, EventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            FileHelper.TryOpenPath(path, GrowlToken);
        }

        private void OpenImageSavePath(object sender, EventArgs e)
        {
            FileHelper.TryOpenPath(Properties.Settings.Default.BasePicPath, GrowlToken);
        }

        private void OpenApplicationPath(object sender, EventArgs e)
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory, GrowlToken);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void HideActressGrid(object sender, MouseButtonEventArgs e)
        {
            var anim = new DoubleAnimation(1, 0, (Duration)FadeInterval, FillBehavior.Stop);
            anim.Completed += (s, _) => vieModel.ActorInfoGrid = Visibility.Collapsed; ;
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


        public string GetCurrentList()
        {
            string table = "";
            for (int i = 0; i < ListItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
                StackPanel stackPanel = FindElementByName<StackPanel>(c, "ListStackPanel");
                if (stackPanel != null)
                {
                    var grids = stackPanel.Children.OfType<Grid>().ToList();
                    foreach (Grid grid in grids)
                    {
                        RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
                        if (radioButton != null && (bool)radioButton.IsChecked)
                        {
                            table = radioButton.Content.ToString();
                            break;
                        }
                    }

                }
            }
            return table;
        }

        private async void RemoveFromList(object sender, RoutedEventArgs e)
        {
            string table = GetCurrentList();
            if (string.IsNullOrEmpty(table)) return;




            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            string id = GetIDFromMenuItem(sender, 0);
            Movie CurrentMovie = GetMovieFromVieModel(id);
            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
            if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToRemove).ShowDialog() == false) return;
            MySqlite dB = new MySqlite("mylist");
            vieModel.SelectedMovie.ToList().ForEach(arg =>
                {

                    dB.DeleteByField(table, "id", arg.id);

                    vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                    vieModel.MovieList.Remove(arg);
                    vieModel.FilterMovieList.Remove(arg);
                });
            dB.Close();
            //从详情窗口删除
            if (GetWindowByName("WindowDetails") != null)
            {
                WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                foreach (var item in vieModel.SelectedMovie.ToList())
                {
                    if (windowDetails.vieModel.DetailMovie.id == item.id)
                    {
                        windowDetails.Close();
                        break;
                    }
                }
            }

            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, GrowlToken);
            //修复数字显示
            vieModel.CurrentCount -= vieModel.SelectedMovie.Count;
            vieModel.TotalCount -= vieModel.SelectedMovie.Count;

            vieModel.SelectedMovie.Clear();

            await Task.Run(() => { Task.Delay(100).Wait(); });
            //ListItemsControl.ItemsSource = null;
            //ListItemsControl.ItemsSource = vieModel.MyList;

            //侧边栏选项选中
            for (int i = 0; i < ListItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
                StackPanel stackPanel = FindElementByName<StackPanel>(c, "ListStackPanel");
                if (stackPanel != null)
                {
                    var grids = stackPanel.Children.OfType<Grid>().ToList();
                    foreach (Grid grid in grids)
                    {
                        RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
                        if (radioButton != null && radioButton.Content.ToString() == table)
                        {
                            radioButton.IsChecked = true;
                            break;
                        }
                    }

                }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }








        private void ClearActressInfo(object sender, RoutedEventArgs e)
        {
            string name = vieModel.Actress.name;
            DataBase.DeleteByField("actress", "name", name);

            Actress actress = new Actress(vieModel.Actress.name);
            actress.like = vieModel.Actress.like;
            actress.smallimage = vieModel.Actress.smallimage;

            vieModel.Actress = actress;

        }



        private void ShowRestMovie(object sender, RoutedEventArgs e)
        {
            int low = ++vieModel.FlowNum;

            for (int i = low; i < 5; i++)
            {
                vieModel.FlowNum = i;
                vieModel.Flow();
            }



        }

        private void SetClassify(bool refresh = false)
        {
            if (ActorTabControl != null)
            {
                vieModel.ShowActorTools = false;
                switch (ActorTabControl.SelectedIndex)
                {
                    case 0:
                        vieModel.ShowActorTools = true;
                        if (vieModel.ActorList != null && vieModel.ActorList.Count > 0 && !refresh) return;
                        vieModel.GetActorList();
                        break;

                    case 1:
                        if (vieModel.GenreList != null && vieModel.GenreList.Count > 0 && !refresh) return;
                        vieModel.GetGenreList();
                        break;

                    case 2:
                        if (vieModel.LabelList != null && vieModel.LabelList.Count > 0 && !refresh) return;
                        vieModel.GetLabelList();
                        break;

                    case 3:
                        if (vieModel.TagList != null && vieModel.TagList.Count > 0 && !refresh) return;
                        vieModel.GetTagList();
                        break;

                    case 4:
                        if (vieModel.StudioList != null && vieModel.StudioList.Count > 0 && !refresh) return;
                        vieModel.GetStudioList();
                        break;


                    case 5:
                        if (vieModel.DirectorList != null && vieModel.DirectorList.Count > 0 && !refresh) return;
                        vieModel.GetDirectoroList();
                        break;

                    default:

                        break;
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetClassify();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            vieModel.ShowSearchPopup = false;
        }

        private void HideBeginScanGrid(object sender, MouseButtonEventArgs e)
        {
            vieModel.ShowFirstRun = Visibility.Hidden;
        }

        private void OpenActorPath(object sender, RoutedEventArgs e)
        {
            string filepath = System.IO.Path.Combine(BasePicPath, "Actresses", $"{vieModel.Actress.name}.jpg");
            FileHelper.TryOpenSelectPath(filepath, GrowlToken);
        }

        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            if (vieModel.Actress.sourceurl.IsProperUrl())
                FileHelper.TryOpenUrl(vieModel.Actress.sourceurl);

            else
            {
                if (JvedioServers.Bus.Url.IsProperUrl())
                {
                    string url = $"{JvedioServers.Bus.Url}searchstar/{System.Web.HttpUtility.UrlEncode(vieModel.Actress.name)}&type=&parent=ce";
                    FileHelper.TryOpenUrl(url);
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.CannotOpen + $" {vieModel.Actress.sourceurl}", GrowlToken);
                }
            }


        }


        private void OpenPath(string path)
        {

        }


        private void ToolsGrid_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ToolsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FocusTextBox.Focus();
        }

        private void ClearSearchHistory(object sender, MouseButtonEventArgs e)
        {
            vieModel.SearchHistory.Clear();
            vieModel.SaveSearchHistory();
            SearchHistoryStackPanel.Visibility = Visibility.Collapsed;
        }



        private void CmdTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.ScrollToEnd();
        }

        private void ShowClassifyGrid(object sender, RoutedEventArgs e)
        {
            vieModel.TabSelectedIndex = 1;
        }

        private void RefreshClassify(object sender, MouseButtonEventArgs e)
        {
            SetClassify(true);
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
            vieModel.SearchFirstLetter = true;
            vieModel.Search = ((Button)sender).Content.ToString();

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            vieModel.CurrentMovieList.Clear();
        }

        private void CopyText(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ClipBoard.TrySetDataObject(textBlock.Text, GrowlToken);
        }



        private List<ActorSearch> GetActorSearchReulst(string sourceCode)
        {
            List<ActorSearch> result = new List<ActorSearch>();
            if (string.IsNullOrEmpty(sourceCode)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sourceCode);
            HtmlNodeCollection actorNodes = doc.DocumentNode.SelectNodes("//a[@class='avatar-box text-center']");
            if (actorNodes == null) return result;
            foreach (HtmlNode actorNode in actorNodes)
            {
                ActorSearch actorSearch = new ActorSearch();
                actorSearch.ID = result.Count;
                HtmlNode img = actorNode.SelectSingleNode("div/img");
                HtmlNode name = actorNode.SelectSingleNode("div/span");
                HtmlNode tag = actorNode.SelectSingleNode("div/span/button");

                if (actorNode.Attributes["href"]?.Value != "") actorSearch.Link = actorNode.Attributes["href"].Value;
                if (tag != null && tag.InnerText != "") actorSearch.Tag = tag.InnerText;
                if (name != null && name.InnerText != "") actorSearch.Name = name.InnerText.Replace(actorSearch.Tag, "");
                if (img != null && img.Attributes["src"]?.Value != "") actorSearch.Img = img.Attributes["src"].Value;

                result.Add(actorSearch);
            }

            return result;


        }



        /// <summary>
        /// 加载该演员出演的其他作品，仅支持 bus
        /// </summary>
        /// <param name="name"></param>
        private async void LoadActor(string name)
        {
            if (!JvedioServers.Bus.IsEnable || JvedioServers.Bus.Url.IsProperUrl())
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.OnlySupportBus, GrowlToken);
                LoadSearchWaitingPanel.Visibility = Visibility.Collapsed;
                LoadSearchCTS?.Dispose();
                return;
            }
            string log = "";
            //先搜索出演员
            await Task.Run(async () =>
            {
                Console.WriteLine(name);
                string url = $"{JvedioServers.Bus.Url}searchstar/{System.Web.HttpUtility.UrlEncode(name)}&type=&parent=ce";

                HttpResult httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
                if (httpResult != null && httpResult.SourceCode != "")
                {
                    if (CheckLoadActorCancel()) return;
                    //解析
                    List<ActorSearch> actorSearches = GetActorSearchReulst(httpResult.SourceCode);
                    List<ActorSearch> toDownload = new List<ActorSearch>();
                    if (actorSearches.Count == 0)
                    {
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.NoResult, GrowlToken);
                    }
                    else
                    {
                        int vt = 1;
                        int startpage = 1;
                        int endpage = 500;
                        //让用户选择
                        Dispatcher.Invoke((Action)delegate
                        {
                            Dialog_SelectActor dialog = new Dialog_SelectActor(this, true, actorSearches);

                            if (dialog.ShowDialog() == true)
                            {
                                for (int i = 0; i < dialog.SelectedActor.Count; i++)
                                {
                                    toDownload.Add(actorSearches.Where(arg => arg.ID == dialog.SelectedActor[i]).First());
                                }
                                vt = dialog.VedioType;
                                startpage = dialog.StartPage;
                                endpage = dialog.EndPage;
                            }
                        });


                        if (toDownload.Count > 3)
                        {
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.MoreThanThree, GrowlToken);
                        }
                        else
                        {
                            long total = 0;
                            //遍历要下载的演员
                            for (int i = 0; i < toDownload.Count; i++)
                            {
                                int page = startpage;
                                if (CheckLoadActorCancel()) break;
                                ActorSearch actorSearch = toDownload[i];
                                while (true)
                                {
                                    if (CheckLoadActorCancel()) break;
                                    url = actorSearch.Link;
                                    if (page > 1) url += $"/{page}";
                                    Dispatcher.Invoke((Action)delegate
                                    {
                                        LoadSearchWaitingPanel.ShowCancelButton = Visibility.Visible;
                                        LoadSearchWaitingPanel.NoticeExtraText = Jvedio.Language.Resources.TotalImport + "：" + total + "\n" + Jvedio.Language.Resources.CurrentPage + "：" + page;
                                        LoadSearchWaitingPanel.ShowExtraText = Visibility.Visible;
                                        log = LoadSearchWaitingPanel.NoticeExtraText;
                                    });

                                    HttpResult newResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
                                    page++;
                                    if (newResult != null && newResult.SourceCode != "" && newResult.StatusCode == HttpStatusCode.OK)
                                    {
                                        //解析演员
                                        List<Movie> movies = BusParse.GetMoviesFromPage(newResult.SourceCode);
                                        List<string> idlist = DataBase.SelectAllID();
                                        foreach (Movie movie in movies)
                                        {
                                            if (idlist.Contains(movie.id)) continue;//如果数据库存在则跳过，不会更新信息
                                            movie.vediotype = vt;
                                            movie.actor = name;
                                            movie.otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            if (vt != 3) movie.id = movie.id.ToUpper();
                                            DataBase.InsertSearchMovie(movie);
                                            total += 1;
                                        }

                                        if (page == 2)
                                        {
                                            Actress actress = DataBase.SelectInfoByActress(new Actress() { name = actorSearch.Name });
                                            if (string.IsNullOrEmpty(actress.birthday))
                                            {
                                                BusParse busParse = new BusParse("", newResult.SourceCode, VedioType.所有);
                                                Actress saveActress = busParse.ParseActress();
                                                saveActress.sourceurl = url;
                                                saveActress.source = "javbus";
                                                saveActress.id = "";
                                                saveActress.name = actorSearch.Name;
                                                if (saveActress != null && !string.IsNullOrEmpty(saveActress.birthday))
                                                    DataBase.InsertActress(saveActress);//保存演员的信息到数据库
                                            }
                                        }
                                        if (page > endpage) break;//达到末页
                                        if (movies.Count < 30) break;//小于每页的最大数目
                                    }
                                    else
                                    {
                                        Console.WriteLine(Jvedio.Language.Resources.HttpFail);
                                        break;
                                        //HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.HttpFail, GrowlToken);
                                    }

                                    await Task.Delay(5000);
                                }
                                await Task.Delay(5000);
                            }
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Complete, GrowlToken);
                        }


                    }
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.HttpFail, GrowlToken);
                }
            }, LoadSearchCT);
            LoadSearchWaitingPanel.Visibility = Visibility.Collapsed;
            LoadSearchCTS?.Dispose();
            //显示加载的信息
            if (log != "")
            {
                new Msgbox(this, log).ShowDialog();
                ShowSameActors(name);
            }

        }


        public async void ShowSameActors(string name)
        {
            vieModel.ActorInfoGrid = Visibility.Visible;
            vieModel.IsLoadingMovie = true;
            vieModel.TabSelectedIndex = 0;
            var currentActress = vieModel.ActorList.Where(arg => arg.name == name).First();
            Actress actress = DataBase.SelectInfoByActress(currentActress);
            actress.id = "";//不按照 id 选取演员
            await vieModel.AsyncGetMoviebyActress(actress);
            vieModel.Actress = actress;
            vieModel.AsyncFlipOver();
            vieModel.TextType = actress.name;
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
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Cancel, GrowlToken);
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
            Dialog_LoadPage dialog_LoadPage = new Dialog_LoadPage(this, true);
            if (dialog_LoadPage.ShowDialog() == true)
            {
                int start = dialog_LoadPage.StartPage;
                int end = dialog_LoadPage.EndPage;
                int vt = dialog_LoadPage.VedioType;
                string url = dialog_LoadPage.url;
                string log = "";
                if (url.IsProperUrl())
                {
                    WebSite webSite = WebSite.Bus;
                    if (url.IndexOf("javdb") > 0)
                        webSite = WebSite.DB;
                    else
                        webSite = WebSite.Bus;
                    if (!url.EndsWith("/") && url.IndexOf("javdb") < 0) url += "/";
                    //遍历要下载的页码
                    int total = 0;
                    InitLoadSearch(Jvedio.Language.Resources.LoadFromNet);
                    await Task.Run(async () =>
                    {
                        List<string> allMovies = new List<string>();
                        for (int i = start; i <= end; i++)
                        {
                            if (CheckLoadActorCancel()) break;
                            string Url;
                            if (webSite == WebSite.DB)
                            {
                                if (url.IndexOf("?") > 0)
                                    Url = url + "&page=" + i;
                                else
                                    Url = url + "?page=" + i;
                            }

                            else
                                Url = url + i;
                            Dispatcher.Invoke((Action)delegate
                            {
                                LoadSearchWaitingPanel.NoticeText = Jvedio.Language.Resources.LoadFromNet;
                                LoadSearchWaitingPanel.ShowCancelButton = Visibility.Visible;
                                LoadSearchWaitingPanel.NoticeExtraText = Jvedio.Language.Resources.TotalImport + "：" + total + "\n" + Jvedio.Language.Resources.CurrentPage + "：" + i;
                                LoadSearchWaitingPanel.ShowExtraText = Visibility.Visible;
                                log = LoadSearchWaitingPanel.NoticeExtraText;
                            });
                            HttpResult newResult = null;
                            if (webSite == WebSite.DB)
                                newResult = await new MyNet().Http(Url, new CrawlerHeader() { Cookies = JvedioServers.DB.Cookie });
                            else
                                newResult = await new MyNet().Http(Url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
                            if (newResult != null && newResult.SourceCode != "" && newResult.StatusCode == HttpStatusCode.OK)
                            {
                                //解析影片
                                List<Movie> movies = new List<Movie>();
                                if (webSite == WebSite.DB)
                                    movies = JavDBParse.GetMoviesFromPage(newResult.SourceCode);
                                else
                                    movies = BusParse.GetMoviesFromPage(newResult.SourceCode);
                                List<string> idlist = DataBase.SelectAllID();
                                foreach (Movie movie in movies)
                                {
                                    if (idlist.Contains(movie.id)) continue;//如果数据库存在则跳过，不会更新信息
                                    movie.vediotype = vt;
                                    movie.otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    if (vt != 3) movie.id = movie.id.ToUpper();
                                    DataBase.InsertSearchMovie(movie);
                                    total += 1;
                                }
                                Dispatcher.Invoke((Action)delegate
                                {
                                    LoadSearchWaitingPanel.NoticeExtraText = Jvedio.Language.Resources.TotalImport + "：" + total + "\n" + Jvedio.Language.Resources.CurrentPage + "：" + i;
                                    log = LoadSearchWaitingPanel.NoticeExtraText;
                                });


                                if (webSite == WebSite.Bus && movies.Count < 30) break;//小于 bus 每页的最大数目
                                if (webSite == WebSite.DB && movies.Count < 40) break;//小于 db 每页的最大数目
                                if (webSite == WebSite.DB && allMovies.Intersect(movies.Select(arg => arg.id)).Count() >= 30) break;// 如果新添加的影片未变化，则说明到了末页
                                allMovies.AddRange(movies.Select(arg => arg.id));

                            }
                            else
                            {
                                Console.WriteLine(Jvedio.Language.Resources.HttpFail);
                                break;
                                //HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.HttpFail, GrowlToken);
                            }
                            await Task.Delay(1000);

                        }



                    });
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Complete, GrowlToken);
                    LoadSearchWaitingPanel.Visibility = Visibility.Collapsed;
                    LoadSearchCTS?.Dispose();

                    //显示加载的信息
                    new Msgbox(this, log).ShowDialog();
                    vieModel.Reset();

                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.ErrorUrl, GrowlToken);
                }
            }


        }

        private void ManageImage(object sender, RoutedEventArgs e)
        {
            //WindowManageImage windowManageImage = new WindowManageImage();
            //windowManageImage.Show();
        }

        private void FilterSide(object sender, RoutedEventArgs e)
        {
            AllRadioButton.IsChecked = true;
            //年份
            List<string> year = GetFilterFromSideItemsControl(SideFilterYear);
            //时长
            List<string> runtime = GetFilterFromSideItemsControl(SideFilterRuntime);

            //文件大小
            List<string> filesize = GetFilterFromSideItemsControl(SideFilterFileSize);

            //评分
            List<string> rating = GetFilterFromSideItemsControl(SideFilterRating);

            //标签
            List<string> label = GetFilterFromSideItemsControl(SideFilterLabel);
            string sql = "select * from movie where ";

            string s = "";

            year.ForEach(arg => { s += $"releasedate like '%{arg}%' or "; });
            if (year.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

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

        private bool autoScroll = false;
        private void BackTo(object sender, RoutedEventArgs e)
        {
            (sender as Button).Visibility = Visibility.Collapsed;
            autoScroll = true;
            HideActressGrid(this, null);
            vieModel.ExecutiveSqlCommand(0, vieModel.TextType, VieModel_Main.PreviousSql, istorecord: false, flip: false);
            //TODO
            //流动模式
        }

        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            vieModel.SearchFirstLetter = false;
            if (vieModel.CurrentSearchCandidate != null && (SearchSelectIdex >= 0 & SearchSelectIdex < vieModel.CurrentSearchCandidate.Count))
                SearchBar.Text = vieModel.CurrentSearchCandidate[SearchSelectIdex];
            if (SearchBar.Text == "") return;
            if (SearchBar.Text.ToUpper() == "JVEDIO")
            {
                Properties.Settings.Default.ShowSecret = true;
            }
            else
            {
                vieModel.Search = SearchBar.Text;
                vieModel.ShowSearchPopup = false;
            }
        }

        private void SearchBar_SearchStarted_1(object sender, FunctionEventArgs<string> e)
        {
            HandyControl.Controls.SearchBar searchBar = sender as HandyControl.Controls.SearchBar;
            int idx = ActorTabControl.SelectedIndex;
            switch (idx)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:

                    break;
                case 6:

                    break;
                case 7:

                    break;
            }
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
