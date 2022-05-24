using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using static Jvedio.FileProcess;
using static Jvedio.ImageProcess;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using HandyControl.Data;
using Jvedio.Utils;
using Jvedio.Utils.Net;
using Jvedio.Style.UserControls;
using Jvedio.Entity;
using ChaoControls.Style;
using Jvedio.Core.Enums;
using Jvedio.Core.SimpleORM;
using Jvedio.Utils.Visual;
using Jvedio.Core.Scan;
using Jvedio.CommonNet.Crawler;
using Jvedio.CommonNet;
using Jvedio.Core.Net;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.FFmpeg;
using Jvedio.Entity.CommonSQL;
using System.Text;

namespace Jvedio
{
    /// <summary>
    /// WindowDetails.xaml 的交互逻辑
    /// </summary>
    public partial class WindowDetails : BaseWindow
    {


        public VieModel_Details vieModel;

        Main windowMain = GetWindowByName("Main") as Main;
        WindowEdit WindowEdit;

        public List<long> DataIDs = new List<long>();
        public long DataID;

        private bool cancelLoadImage = false;// 切换到下一个影片时停止加载图片

        // 进度条
        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance = null;


        public WindowDetails() : this(1893)
        {
            Properties.Settings.Default.TeenMode = false;
        }

        public WindowDetails(long dataID)
        {
            InitializeComponent();
            DataID = dataID;

            this.Height = SystemParameters.PrimaryScreenHeight * 0.8;
            this.Width = SystemParameters.PrimaryScreenHeight * 0.8 * 1230 / 720;

            initProgressBar();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            SetSkin();

            vieModel = new VieModel_Details();
            vieModel.QueryCompleted += async delegate
            {
                SetStatus(true);//设置状态
                await LoadImage(vieModel.ShowScreenShot);//加载图片
                renderMagnets();// 显示磁力
                ShowActor();
                //if (GlobalConfig.Settings.AutoGenScreenShot)
                //{
                //    AutoGenScreenShot();
                //}

            };
            vieModel.Load(DataID);
            this.DataContext = vieModel;
            rootGrid.Focus();// 设置键盘左右可切换
            initDataIDs(); // 设置切换的影片列表
            // 设置右键菜单
            OpenOtherUrlMenuItem.Items.Clear();

            // todo 同步网址

        }

        // 自动判断有无图片，然后生成
        //private void AutoGenScreenShot()
        //{
        //    if (vieModel.CurrentVideo.BigImage == GlobalVariable.DefaultBigImage)
        //    {
        //        // 检查有无截图
        //        Video video = vieModel.CurrentVideo;
        //        string path = video.getScreenShot();
        //        if (Directory.Exists(path))
        //        {
        //            string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
        //            if (array.Length > 0)
        //            {

        //                Video.SetImage(ref video, array[array.Length / 2]);
        //                vieModel.CurrentVideo.BigImage = null;
        //                vieModel.CurrentVideo.BigImage = video.ViewImage;
        //            }
        //        }
        //    }

        //}


        private delegate void LoadActorDelegate(ActorInfo actor);
        private void LoadActor(ActorInfo actor) => vieModel.CurrentActorList.Add(actor);

        public async void ShowActor()
        {
            vieModel.CurrentActorList = new ObservableCollection<ActorInfo>();
            // 加载演员
            if (vieModel.CurrentVideo.ActorInfos != null)
            {
                for (int i = 0; i < vieModel.CurrentVideo.ActorInfos.Count; i++)
                {
                    if (cancelLoadImage) break;
                    ActorInfo actorInfo = vieModel.CurrentVideo.ActorInfos[i];
                    //加载图片
                    string imagePath = actorInfo.getImagePath(vieModel.CurrentVideo.Path);
                    BitmapImage smallimage = ReadImageFromFile(imagePath);
                    if (smallimage == null) smallimage = DefaultActorImage;
                    actorInfo.SmallImage = smallimage;
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo);
                }
            }
        }



        public void initDataIDs()
        {
            DataIDs = new List<long>();

            SelectWrapper<Video> wrapper = windowMain?.CurrentWrapper;
            string sql = windowMain?.CurrentSQL;
            if (wrapper != null && !string.IsNullOrEmpty(sql))
            {
                if (Properties.Settings.Default.DetialWindowShowAllMovie)
                {
                    sql = "select metadata.DataID" + sql + wrapper.toWhere(false) + wrapper.toOrder();
                }
                else
                {
                    sql = "select metadata.DataID" + sql + wrapper.toWhere(false) + wrapper.toOrder() + wrapper.toLimit();
                }

                List<Dictionary<string, object>> list = videoMapper.select(sql);
                if (list != null && list.Count > 0)
                {
                    DataIDs = list.Select(arg => long.Parse(arg["DataID"].ToString())).ToList();
                }
            }

        }

        private void initProgressBar()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
                taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
        }




        public void Refresh()
        {
            vieModel.Load(DataID);
        }


        public void SetSkin()
        {
            switch (Properties.Settings.Default.Themes)
            {
                case "蓝色":
                    //if (BackGroundImage == null)
                    //{
                    //    //设置渐变
                    //    LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                    //    myLinearGradientBrush.StartPoint = new Point(0.5, 0);
                    //    myLinearGradientBrush.EndPoint = new Point(0.5, 1);
                    //    myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 1));
                    //    myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 0));
                    //    BackBorder.Background = myLinearGradientBrush;
                    //}

                    break;
            }

            //设置背景

            //BackGroundImage.Source = GlobalVariable.BackgroundImage;
            MouseMoveGrid.Background = Brushes.Transparent;
            ImageGrid.Background = Brushes.Transparent;
            ExtraImageBorder.Background = Brushes.Transparent;
            TitleTextBox.Background = Brushes.Transparent;
            //InfoBorder.Background = (SolidColorBrush)Application.Current.Resources["Window.Background"];
            InfoBorder.Opacity = 0.7;
            NextBorder.Opacity = 0.7;
            PreviousBorder.Opacity = 0.7;

            //设置字体
            if (GlobalFont != null) this.FontFamily = GlobalFont;
        }

        public void DownLoad(object sender, RoutedEventArgs e)
        {
            Video video = vieModel.CurrentVideo;
            DownLoadTask task = new DownLoadTask(video, true, GlobalConfig.Settings.OverrideInfo);// 详情页面下载预览图
            long dataid = video.DataID;
            task.onDownloadSuccess += (s, ev) =>
            {
                DownLoadTask t = s as DownLoadTask;
                Dispatcher.Invoke(() =>
                {
                    if (dataid.Equals(vieModel.CurrentVideo.DataID))
                        vieModel.Load(dataid);
                    // 通知主界面刷新
                    windowMain?.RefreshData(dataid);
                });
            };
            task.onDownloadPreview += (s, ev) =>
            {
                DownLoadTask t = s as DownLoadTask;
                PreviewImageEventArgs arg = ev as PreviewImageEventArgs;
                Dispatcher.Invoke(() =>
                {
                    if (dataid.Equals(vieModel.CurrentVideo.DataID) && !vieModel.ShowScreenShot)
                    {
                        // 加入到列表
                        if (vieModel.CurrentVideo.PreviewImagePathList == null) vieModel.CurrentVideo.PreviewImagePathList = new ObservableCollection<string>();
                        if (vieModel.CurrentVideo.PreviewImageList == null) vieModel.CurrentVideo.PreviewImageList = new ObservableCollection<BitmapSource>();
                        vieModel.CurrentVideo.PreviewImagePathList.Add(arg.Path);
                        vieModel.CurrentVideo.PreviewImageList.Add(ImageProcess.BitmapImageFromByte(arg.FileByte));
                    }
                });
            };
            if (!Global.Download.Dispatcher.Working)
                Global.Download.Dispatcher.BeginWork();
            windowMain?.addToDownload(task);
            windowMain?.setDownloadStatus();
        }

        public void GetScreenGif(object sender, RoutedEventArgs e)
        {
            Video video = vieModel.CurrentVideo;
            ScreenShotTask task = new ScreenShotTask(vieModel.CurrentVideo, true);// 详情页面下载预览图
            //long dataid = video.DataID;
            //task.onCompleted += (s, ev) =>
            //{
            //    ScreenShotTask t = s as ScreenShotTask;
            //    PreviewImageEventArgs arg = ev as PreviewImageEventArgs;
            //    Dispatcher.Invoke(async () =>
            //   {
            //       if (vieModel.ShowScreenShot && dataid.Equals(vieModel.CurrentVideo.DataID))
            //       {
            //           // 加入到列表
            //           if (vieModel.CurrentVideo.PreviewImagePathList == null) vieModel.CurrentVideo.PreviewImagePathList = new ObservableCollection<string>();
            //           if (vieModel.CurrentVideo.PreviewImageList == null) vieModel.CurrentVideo.PreviewImageList = new ObservableCollection<BitmapSource>();
            //           await LoadImage(true);
            //       }
            //   });
            //};
            if (!Global.FFmpeg.Dispatcher.Working)
                Global.FFmpeg.Dispatcher.BeginWork();
            windowMain?.addToScreenShot(task);
        }

        public void GetScreenShot(object sender, RoutedEventArgs e)
        {
            Video video = vieModel.CurrentVideo;
            ScreenShotTask task = new ScreenShotTask(vieModel.CurrentVideo);// 详情页面下载预览图
            long dataid = video.DataID;
            task.onCompleted += (s, ev) =>
            {
                ScreenShotTask t = s as ScreenShotTask;
                PreviewImageEventArgs arg = ev as PreviewImageEventArgs;
                Dispatcher.Invoke(async () =>
               {
                   if (vieModel.ShowScreenShot && dataid.Equals(vieModel.CurrentVideo.DataID))
                   {
                       // 加入到列表
                       if (vieModel.CurrentVideo.PreviewImagePathList == null) vieModel.CurrentVideo.PreviewImagePathList = new ObservableCollection<string>();
                       if (vieModel.CurrentVideo.PreviewImageList == null) vieModel.CurrentVideo.PreviewImageList = new ObservableCollection<BitmapSource>();
                       await LoadImage(true);
                   }
               });
            };
            if (!Global.FFmpeg.Dispatcher.Working)
                Global.FFmpeg.Dispatcher.BeginWork();
            windowMain?.addToScreenShot(task);
        }


        private bool cancelDownload = false;
        public void StartDownload()
        {
            //List<string> urlList = new List<string>();
            //foreach (var item in vieModel.CurrentVideo.extraimageurl?.Split(';')) { if (!string.IsNullOrEmpty(item)) urlList.Add(item); }
            //if (vieModel.CurrentVideo.extraimagelist.Count - 1 >= urlList.Count && vieModel.CurrentVideo.bigimage != null && vieModel.CurrentVideo.title != "" && vieModel.CurrentVideo.sourceurl != "") return;



            ////添加到下载列表
            //DetailDownLoad = new DetailDownLoad(vieModel.CurrentVideo);
            //cancelDownload = false;
            //DetailDownLoad.DownLoad();
            //Dispatcher.Invoke((Action)delegate () { ProgressBar.Value = 0; ProgressBar.Visibility = Visibility.Visible; });

            ////监听取消下载：
            //DetailDownLoad.CancelEvent += (s, e) =>
            //{
            //    Dispatcher.Invoke((Action)delegate ()
            //    {
            //        ProgressBar.Visibility = Visibility.Collapsed;
            //        cancelDownload = true;
            //    });
            //};

            ////显示详细信息
            //DetailDownLoad.InfoDownloadCompleted += (s, e) =>
            //{
            //    MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
            //    if (vieModel.CurrentVideo.id == eventArgs.Message && !cancelDownload)
            //    {
            //        //判断是否是当前番号

            //        //vieModel.CurrentVideo = new CurrentVideo();
            //        CurrentVideo CurrentVideo = DataBase.SelectCurrentVideoById(eventArgs.Message);
            //        if (CurrentVideo != null)
            //        {
            //            MySqlite db = new MySqlite("Translate");
            //            //加载翻译结果
            //            if (Properties.Settings.Default.TitleShowTranslate)
            //            {
            //                string translate_title = db.GetInfoBySql($"select translate_title from youdao where id='{CurrentVideo.id}'");
            //                if (translate_title != "") CurrentVideo.title = translate_title;
            //            }

            //            if (Properties.Settings.Default.PlotShowTranslate)
            //            {
            //                string translate_plot = db.GetInfoBySql($"select translate_plot from youdao where id='{CurrentVideo.id}'");
            //                if (translate_plot != "") CurrentVideo.plot = translate_plot;
            //            }
            //            db.CloseDB();


            //            CurrentVideo.extraimagelist = vieModel.CurrentVideo.extraimagelist;
            //            CurrentVideo.extraimagePath = vieModel.CurrentVideo.extraimagePath;
            //            CurrentVideo.bigimage = vieModel.CurrentVideo.bigimage;

            //            vieModel.CurrentVideo = CurrentVideo;
            //            vieModel.VideoInfo = MediaParse.GetMediaInfo(CurrentVideo.filepath);
            //        }

            //        //显示到主界面
            //        this.Dispatcher.Invoke((Action)delegate
            //        {
            //            Main main = App.Current.Windows[0] as Main;
            //            main.RefreshMovieByID(eventArgs.Message);
            //        });
            //    }

            //};

            ////进度
            //DetailDownLoad.InfoUpdate += (s, e) =>
            //{
            //    Dispatcher.Invoke((Action)delegate ()
            //    {
            //        CurrentVideoEventArgs eventArgs = e as CurrentVideoEventArgs;
            //        ProgressBar.Value = (eventArgs.value / eventArgs.maximum) * 100; ProgressBar.Visibility = Visibility.Visible;
            //        if (ProgressBar.Value == ProgressBar.Maximum) ProgressBar.Visibility = Visibility.Collapsed;
            //    });
            //};


            ////显示错误消息
            //DetailDownLoad.MessageCallBack += (s, e) =>
            //{
            //    Dispatcher.Invoke((Action)delegate ()
            //    {
            //        MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
            //        ChaoControls.Style.MessageCard.Error(eventArgs.Message);
            //    });
            //};



            ////显示大图
            //DetailDownLoad.BigImageDownLoadCompleted += (s, e) =>
            //{
            //    if (!File.Exists(BasePicPath + $"BigPic\\{vieModel.CurrentVideo.id}.jpg")) return;
            //    MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
            //    if (vieModel.CurrentVideo.id == eventArgs.Message && !cancelDownload)
            //    {
            //        Dispatcher.Invoke((Action)delegate ()
            //    {
            //        vieModel.CurrentVideo.bigimage = null;
            //        vieModel.CurrentVideo.bigimage = GetBitmapImage(vieModel.CurrentVideo.id, "BigPic");
            //        BigImage.Source = vieModel.CurrentVideo.bigimage;
            //        if (vieModel.CurrentVideo.extraimagelist.Count == 0)
            //        {
            //            vieModel.CurrentVideo.extraimagelist = new ObservableCollection<BitmapSource>();
            //            vieModel.CurrentVideo.extraimagePath = new ObservableCollection<string>();
            //        }
            //        vieModel.CurrentVideo.extraimagelist.Insert(0, vieModel.CurrentVideo.bigimage);
            //        vieModel.CurrentVideo.extraimagePath.Insert(0, BasePicPath + $"BigPic\\{vieModel.CurrentVideo.id}.jpg");
            //        imageItemsControl.ItemsSource = vieModel.CurrentVideo.extraimagelist;

            //    });
            //    }

            //    Dispatcher.Invoke((Action)delegate ()
            //    {
            //        //显示到主界面
            //        Main main = App.Current.Windows[0] as Main;
            //        main.RefreshMovieByID(vieModel.CurrentVideo.id);
            //    });
            //};


            ////显示小图
            //DetailDownLoad.SmallImageDownLoadCompleted += (s, e) =>
            //{
            //    if (!File.Exists(BasePicPath + $"SmallPic\\{vieModel.CurrentVideo.id}.jpg")) return;
            //    MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
            //    Dispatcher.Invoke((Action)delegate ()
            //    {
            //        //显示到主界面
            //        Main main = App.Current.Windows[0] as Main;
            //        main.RefreshMovieByID(vieModel.CurrentVideo.id);
            //    });
            //};


            ////显示预览图
            //DetailDownLoad.ExtraImageDownLoadCompleted += (s, e) =>
            //{
            //    if (cancelDownload) return;
            //    Dispatcher.Invoke((Action)delegate ()
            //    {
            //        if ((bool)ScreenShotRadioButton.IsChecked) return;
            //        MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
            //        if (!File.Exists(eventArgs.Message)) return;
            //        vieModel.CurrentVideo.extraimagelist.Add(GetExtraImage(eventArgs.Message));
            //        vieModel.CurrentVideo.extraimagePath.Add(eventArgs.Message);
            //        imageItemsControl.ItemsSource = vieModel.CurrentVideo.extraimagelist;

            //    });
            //};


        }

        public void StopDownLoad()
        {
            //if (DetailDownLoad != null && DetailDownLoad.IsDownLoading == true) ChaoControls.Style.MessageCard.Warning(Jvedio.Language.Resources.Message_CancelCurrentTask);
            //DetailDownLoad?.CancelDownload();
        }



        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }






        //显示类别
        public void ShowSameGenre(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Genre");
        }

        //显示演员
        public void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            long.TryParse(grid.Tag.ToString(), out long actorID);
            windowMain.ShowSameActor(actorID);
            this.Close();
        }

        //显示标签
        public void ShowSameLabel(object sender, MouseButtonEventArgs e)
        {
            string label = ((TextBlock)sender).Text;
            if (string.IsNullOrEmpty(label)) return;
            windowMain.ShowSameLabel(label);
            this.Close();
        }

        //显示导演
        public void ShowSameDirector(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Director");
        }

        public void ShowSameSeries(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Series");
        }

        //显示发行商
        public void ShowSameStudio(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Studio");


        }

        //显示系列
        public void ShowSameString(object sender, string clickFilterType)
        {
            Border border = sender as Border;
            string text = ((TextBlock)border.Child).Text;
            if (string.IsNullOrEmpty(text)) return;
            windowMain.ShowSameString(text, clickFilterType);
            this.Close();
        }



        public void EditInfo(object sender, RoutedEventArgs e)
        {

            if (WindowEdit != null) WindowEdit.Close();
            WindowEdit = new WindowEdit(vieModel.CurrentVideo.DataID);
            WindowEdit.ShowDialog();
        }



        private void MoveWindow(object sender, MouseEventArgs e)
        {
            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }

        }

        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }


        // todo 滚动过快导致演员超出
        public void PreviousMovie(object sender, MouseButtonEventArgs e)
        {
            if (DataIDs.Count == 0) return;
            cancelLoadImage = true;
            StopDownLoad();
            long nextID = 0L;
            for (int i = 0; i < DataIDs.Count; i++)
            {
                long id = DataIDs[i];
                int idx = i;
                if (id == vieModel.CurrentVideo.DataID)
                {
                    idx--;
                    if (idx < 0) idx = DataIDs.Count - 1;
                    nextID = DataIDs[idx];
                    break;
                }
            }
            if (nextID > 0)
            {
                vieModel.CleanUp();
                cancelLoadImage = false;
                vieModel.Load(nextID);
                vieModel.SelectImageIndex = 0;
            }
        }



        public void NextMovie(object sender, MouseButtonEventArgs e)
        {
            if (DataIDs.Count == 0) return;
            cancelLoadImage = true;
            StopDownLoad();
            long nextID = 0L;
            for (int i = 0; i < DataIDs.Count; i++)
            {
                long id = DataIDs[i];
                int idx = i;
                if (id == vieModel.CurrentVideo.DataID)
                {
                    idx++;
                    if (idx >= DataIDs.Count) idx = 0;
                    nextID = DataIDs[idx];
                    break;
                }
            }
            if (nextID > 0)
            {
                cancelLoadImage = false;
                vieModel.CleanUp();
                vieModel.Load(nextID);
                vieModel.SelectImageIndex = 0;
            }
        }

        public void OpenExtraImagePath(object sender, RoutedEventArgs e)
        {
            string path = getExtraImagePath(sender as FrameworkElement);
            FileHelper.TryOpenSelectPath(path);
        }

        private string getExtraImagePath(FrameworkElement element, int depth = 0)
        {
            MenuItem menuItem = element as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (depth == 1) contextMenu = (menuItem.Parent as MenuItem).Parent as ContextMenu;
            Border border = contextMenu.PlacementTarget as Border;
            int.TryParse(border.Tag.ToString(), out int idx);
            if (idx >= 0 && idx < vieModel.CurrentVideo.PreviewImagePathList.Count)
                return vieModel.CurrentVideo.PreviewImagePathList[idx];
            return "";
        }

        public async void DeleteImage(object sender, RoutedEventArgs e)
        {
            string path = getExtraImagePath(sender as FrameworkElement);
            int idx = vieModel.CurrentVideo.PreviewImagePathList.IndexOf(path);

            if (idx >= 0)
            {
                FileHelper.TryMoveToRecycleBin(path, 0);
                bool deleteBigImage = vieModel.CurrentVideo.PreviewImageList[0] == vieModel.CurrentVideo.BigImage;
                vieModel.CurrentVideo.PreviewImagePathList.RemoveAt(idx);
                vieModel.CurrentVideo.PreviewImageList.RemoveAt(idx);
                // todo 更新数据库 路径

                if (deleteBigImage && idx == 0)
                {
                    await Task.Delay(300);// 等待删除完成
                    Refresh();
                    windowMain?.RefreshImage(vieModel.CurrentVideo);
                }
                else if (vieModel.CurrentVideo.PreviewImageList.Count > 0)
                {
                    SetImage(0);
                }
            }


        }
        public async void DeleteAllImage(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < vieModel.CurrentVideo.PreviewImagePathList.Count; i++)
            {
                string path = vieModel.CurrentVideo.PreviewImagePathList[i];
                FileHelper.TryMoveToRecycleBin(path, 0);
            }
            vieModel.CurrentVideo.PreviewImageList.Clear();
            vieModel.CurrentVideo.PreviewImagePathList.Clear();
            if (vieModel.ShowScreenShot)
                videoMapper.updateFieldById("ScreenShotPath", "", vieModel.CurrentVideo.DataID);
            else
                videoMapper.updateFieldById("PreviewImagePath", "", vieModel.CurrentVideo.DataID);
            await Task.Delay(300);
            Refresh();
            windowMain?.RefreshImage(vieModel.CurrentVideo);
        }


        private void SetToSmallPic(object sender, RoutedEventArgs e)
        {
            //string path = getExtraImagePath(sender as FrameworkElement, 1);
            //videoMapper.updateFieldById("SmallImagePath", path, DataID);
            //windowMain?.RefreshImage(vieModel.CurrentVideo);
            //ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
        }


        private void SetToBigAndSmallPic(object sender, RoutedEventArgs e)
        {

            //string path = getExtraImagePath(sender as FrameworkElement, 1);
            //// todo 绝对地址
            //videoMapper.updateFieldById("SmallImagePath", path, DataID);
            //videoMapper.updateFieldById("BigImagePath", path, DataID);
            //Refresh();
            //windowMain?.RefreshImage(vieModel.CurrentVideo);
            //ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);

        }



        private void SetToBigPic(object sender, RoutedEventArgs e)
        {
            string path = getExtraImagePath(sender as FrameworkElement, 1);
            videoMapper.updateFieldById("BigImagePath", path, DataID);
            Refresh();
            windowMain?.RefreshImage(vieModel.CurrentVideo);
            ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
        }






        public void CopyFile(object sender, RoutedEventArgs e)
        {
            string filepath = vieModel.CurrentVideo.Path;
            if (File.Exists(filepath))
            {
                StringCollection paths = new StringCollection { filepath };
                bool success = ClipBoard.TrySetFileDropList(paths, (error) => { MessageCard.Error(error); });
                if (success) MessageCard.Success(Jvedio.Language.Resources.HasCopy);
            }
            else
            {
                ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.Message_FileNotExist);
            }
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            int totalCount = 0;
            int num = 0;
            Video video = vieModel.CurrentVideo;
            if (video.SubSectionList?.Count > 0)
            {
                totalCount += video.SubSectionList.Count - 1;
                //分段视频
                foreach (var path in video.SubSectionList)
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
                if (File.Exists(video.Path))
                {
                    try
                    {
                        FileSystem.DeleteFile(video.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                        num++;
                    }
                    catch (Exception ex) { Logger.LogF(ex); }
                }
            }
            DeleteID(null, null);
        }


        public void DeleteID(object sender, RoutedEventArgs e)
        {
            windowMain?.deleteIDs(new List<Video> { vieModel.CurrentVideo }, true);
            int idx = DataIDs.IndexOf(vieModel.CurrentVideo.DataID);
            DataIDs.RemoveAll(arg => arg == vieModel.CurrentVideo.DataID);
            if (idx >= DataIDs.Count) idx = 0;
            if (idx >= 0 && idx < DataIDs.Count)
            {
                cancelLoadImage = false;
                vieModel.CleanUp();
                vieModel.Load(DataIDs[idx]);
                vieModel.SelectImageIndex = 0;
            }
            else
            {
                this.Close();
            }
        }

        private void OpenWeb(object sender, RoutedEventArgs e)
        {
            string url = vieModel.CurrentVideo.WebUrl;
            if (url.IsProperUrl())
                FileHelper.TryOpenUrl(url);
        }


        private bool IsTranslating = false;
        public async void TranslateMovie(object sender, RoutedEventArgs e)
        {
            //if (IsTranslating) return;

            //if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO) { ChaoControls.Style.MessageCard.Warning("请设置【有道翻译】并测试"); IsTranslating = false; return; }
            //string result = "";
            //MySqlite dataBase = new MySqlite("Translate");

            //CurrentVideo movie = vieModel.CurrentVideo;
            //IsTranslating = true;
            ////检查是否已经翻译过，如有提示
            //if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id)))
            //{
            //    if (new Msgbox(this, Jvedio.Language.Resources.AlreadyTranslate).ShowDialog() == false)
            //    {
            //        IsTranslating = false;
            //        return;
            //    }

            //}

            //string title = DataBase.SelectInfoByID("title", "movie", movie.id);

            //if (title != "")
            //{

            //    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(title);
            //    //保存
            //    if (result != "")
            //    {
            //        dataBase.SaveYoudaoTranslateByID(movie.id, title, result, "title");
            //        movie.title = result;
            //        UpdateInfo(movie);
            //    }
            //    else
            //    {
            //        ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.TranslateFail);
            //    }

            //}
            //string plot = DataBase.SelectInfoByID("plot", "movie", movie.id);
            //if (plot != "")
            //{
            //    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(plot);
            //    //保存
            //    if (result != "")
            //    {
            //        dataBase.SaveYoudaoTranslateByID(movie.id, plot, result, "plot");
            //        movie.plot = result;
            //        UpdateInfo(movie);
            //        //ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.TranslateSuccess);
            //    }
            //    else
            //    {
            //        ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.TranslateFail);
            //    }

            //}
            //dataBase.CloseDB();
            //IsTranslating = false;
        }



        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
        {
            //if (!Properties.Settings.Default.Enable_BaiduAI) { ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_SetBaiduAI); return; }
            //MenuItem _mnu = sender as MenuItem;
            //MenuItem mnu = _mnu.Parent as MenuItem;
            //if (mnu != null)
            //{
            //    this.Cursor = Cursors.Wait;

            //    CurrentVideo movie = vieModel.CurrentVideo;
            //    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
            //    string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
            //    if (File.Exists(BigPicPath))
            //    {
            //        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);

            //        if (int32Rect != Int32Rect.Empty)
            //        {
            //            await Task.Delay(500);
            //            //切割缩略图
            //            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
            //            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
            //            ImageSource smallImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetRect(bitmapImage, int32Rect));
            //            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
            //            try
            //            {
            //                bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            //            }
            //            catch (Exception ex) { ChaoControls.Style.MessageCard.Error(ex.Message); }

            //            movie.smallimage = GetBitmapImage(movie.id, "SmallPic");
            //            UpdateInfo(movie);
            //            ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
            //        }
            //        else
            //        {
            //            ChaoControls.Style.MessageCard.Warning(Jvedio.Language.Resources.Message_Fail);
            //        }

            //    }
            //    else
            //    {
            //        ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.Message_PosterMustExist);
            //    }




            //}
            //this.Cursor = Cursors.Arrow;
        }


        public async void GenerateActor(object sender, RoutedEventArgs e)
        {
            //if (!Properties.Settings.Default.Enable_BaiduAI) { ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_SetBaiduAI); return; }
            //MenuItem _mnu = sender as MenuItem;
            //if (_mnu.Parent is MenuItem mnu)
            //{

            //    Video video = vieModel.CurrentVideo;



            //    if (movie.actor == "") { ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.MovieHasNoActor); return; }
            //    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
            //    string name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];
            //    this.Cursor = Cursors.Wait;
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
            //            try { bitmap.Save(ActressesPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); }
            //            catch (Exception ex) { ChaoControls.Style.MessageCard.Error(ex.Message); }
            //            ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
            //        }
            //        else
            //        {
            //            ChaoControls.Style.MessageCard.Warning(Jvedio.Language.Resources.Message_Fail);
            //        }
            //    }
            //    else
            //    {
            //        ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.Message_PosterMustExist);
            //    }

            //}
            //this.Cursor = Cursors.Arrow;
        }




        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
            else if (e.Key == Key.Left)
                PreviousMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.Right)
                NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.P)
                PlayVedio(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.E)
                EditInfo(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (e.Key == Key.D)
                DownLoad(sender, new RoutedEventArgs());

        }





        private void SetImage(int idx)
        {
            if (cancelLoadImage) return;
            if (vieModel.CurrentVideo.PreviewImageList.Count == 0)
            {
                //设置为默认图片
                BigImage.Source = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            }
            else
            {
                if (idx < vieModel.CurrentVideo.PreviewImageList?.Count)
                    BigImage.Source = vieModel.CurrentVideo.PreviewImageList[idx];

                //设置遮罩
                for (int i = 0; i < imageItemsControl.Items.Count; i++)
                {
                    ContentPresenter c = (ContentPresenter)imageItemsControl.ItemContainerGenerator.ContainerFromItem(imageItemsControl.Items[i]);
                    StackPanel stackPanel = VisualHelper.FindElementByName<StackPanel>(c, "ImageStackPanel");
                    if (stackPanel != null)
                    {
                        Grid grid = stackPanel.Children[0] as Grid;
                        Border border = grid.Children[0] as Border;
                        if (border != null)
                        {
                            if (int.Parse(border.Tag.ToString()) == idx)
                                border.Opacity = 0;
                            else
                                border.Opacity = 0.5;
                        }
                    }
                }
            }
        }



        private void ShowExtraImage(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            int idx = int.Parse(border.Tag.ToString());
            vieModel.SelectImageIndex = idx;
            SetImage(vieModel.SelectImageIndex);
        }






        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopDownLoad();
            DisposeImage();

            GlobalConfig.Detail.ShowScreenShot = vieModel.ShowScreenShot;
            GlobalConfig.Detail.InfoSelectedIndex = vieModel.InfoSelectedIndex;
            GlobalConfig.Detail.Save();
        }


        private void BigImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void BigImage_Drop(object sender, DragEventArgs e)
        {
            // todo 拖动设置大图
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            //if (!IsFile(file)) return;
            //FileInfo fileInfo = new FileInfo(file);
            //FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"BigPic\\{vieModel.CurrentVideo.id}.jpg", true);
            //CurrentVideo CurrentVideo = vieModel.CurrentVideo;
            //CurrentVideo.bigimage = null;
            //CurrentVideo.bigimage = BitmapImageFromFile(fileInfo.FullName);

            //if (vieModel.CurrentVideo.extraimagelist.Count > 0)
            //{
            //    CurrentVideo.extraimagelist[0] = CurrentVideo.bigimage;
            //    CurrentVideo.extraimagePath[0] = fileInfo.FullName;

            //}
            //else
            //{
            //    CurrentVideo.extraimagelist.Add(CurrentVideo.bigimage);
            //    CurrentVideo.extraimagePath.Add(fileInfo.FullName);
            //}


            //vieModel.CurrentVideo = null;
            //vieModel.CurrentVideo = CurrentVideo;
            //RefreshUI("", fileInfo.FullName);
            //SetImage(0);


        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            //分为文件夹和文件
            //string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            //List<string> files = new List<string>();
            //StringCollection stringCollection = new StringCollection();
            //foreach (var item in dragdropFiles)
            //{
            //    if (IsFile(item))
            //        files.Add(item);
            //    else
            //        stringCollection.Add(item);
            //}
            //List<string> filepaths = new List<string>();
            ////扫描导入
            //foreach (var item in stringCollection)
            //{
            //    try { filepaths.AddRange(Directory.GetFiles(item, "*.jpg").ToList<string>()); }
            //    catch (Exception ex) { Console.WriteLine(ex.Message); continue; }
            //}
            //if (files.Count > 0) filepaths.AddRange(files);

            ////复制文件
            //string path;
            //if ((bool)ExtraImageRadioButton.IsChecked)
            //    path = BasePicPath + $"ExtraPic\\{vieModel.CurrentVideo.id}\\";
            //else
            //    path = BasePicPath + $"ScreenShot\\{vieModel.CurrentVideo.id}\\";

            //if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            //bool success = false;
            //foreach (var item in filepaths)
            //{
            //    try
            //    {
            //        File.Copy(item, path + item.Split('\\').Last());
            //        success = true;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //        continue;
            //    }

            //}
            //if (success)
            //{
            //    //更新UI
            //    if ((bool)ExtraImageRadioButton.IsChecked)
            //        ExtraImageRadioButton_Click(sender, new RoutedEventArgs());
            //    else
            //        ScreenShotRadioButton_Click(sender, new RoutedEventArgs());

            //    ChaoControls.Style.MessageCard.Success($"{Jvedio.Language.Resources.ImportNumber} {filepaths.Count}");

            //}

        }

        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (vieModel.CurrentVideo != null)
            {
                vieModel.SaveLove();
                //更新主界面
                windowMain?.RefreshGrade(vieModel.CurrentVideo);
            }

        }



        // 设置预览图鼠标滑过效果
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            border.Opacity = 0;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            int idx = int.Parse(border.Tag.ToString());
            if (idx != vieModel.SelectImageIndex)
                border.Opacity = 0.5;
            else
                border.Opacity = 0;
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (vieModel.CurrentVideo.PreviewImagePathList.Count == 0) return;
            if (e.Delta > 0)
            {
                vieModel.SelectImageIndex -= 1;
            }
            else
            {
                vieModel.SelectImageIndex += 1;
            }

            if (vieModel.SelectImageIndex < 0) { vieModel.SelectImageIndex = 0; } else if (vieModel.SelectImageIndex >= imageItemsControl.Items.Count) { vieModel.SelectImageIndex = imageItemsControl.Items.Count - 1; }
            SetImage(vieModel.SelectImageIndex);
            //滚动到指定的
            ContentPresenter c = (ContentPresenter)imageItemsControl.ItemContainerGenerator.ContainerFromItem(imageItemsControl.Items[vieModel.SelectImageIndex]);
            c.BringIntoView();


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Window_ImageViewer window_ImageViewer = new Window_ImageViewer(this, BigImage.Source);
                window_ImageViewer.ShowDialog();
            }
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            }
            else
            {
                PreviousMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            }
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

        private void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
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
                if (menuItem != null) DownLoad(menuItem, new RoutedEventArgs());

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

        public void SetStatus(bool status)
        {
            NextGrid.IsEnabled = status;
            PreviousGrid.IsEnabled = status;
            ImageChangeStackPanel.IsEnabled = status;
        }



        private void renderMagnets()
        {
            if (Properties.Settings.Default.TeenMode
                || vieModel.CurrentVideo.Magnets == null
                || vieModel.CurrentVideo.Magnets.Count == 0) return;

            CopyMagnetsMenuItem.Items.Clear();
            foreach (var magnet in vieModel.CurrentVideo.Magnets)
            {
                MenuItem menuItem = new MenuItem();
                string tag = "";
                if (magnet.Tags.Count > 0) tag = "（" + string.Join(" ", magnet.Tags) + "）";

                menuItem.Header = $"{magnet.Releasedate} {tag} {magnet.Size.ToProperFileSize()} {magnet.Title}";
                menuItem.Click += (s, ev) => ClipBoard.TrySetDataObject(magnet.MagnetLink, false);
                CopyMagnetsMenuItem.Items.Add(menuItem);
            }

        }



        private void DisposeImage()
        {
            cancelLoadImage = true;
            if (vieModel.CurrentVideo.PreviewImageList != null)
            {
                for (int i = 0; i < vieModel.CurrentVideo.PreviewImageList.Count; i++)
                {
                    vieModel.CurrentVideo.PreviewImageList[i] = null;
                }
            }
            GC.Collect();
            cancelLoadImage = false;
        }

        private async Task<bool> LoadImage(bool isScreenShot = false)
        {
            scrollViewer.ScrollToHorizontalOffset(0);
            //加载大图到预览图
            DisposeImage();
            vieModel.CurrentVideo.PreviewImageList = new ObservableCollection<BitmapSource>();
            vieModel.CurrentVideo.PreviewImagePathList = new ObservableCollection<string>();
            string BigImagePath = vieModel.CurrentVideo.getBigImage();
            if (!isScreenShot)
            {
                if (File.Exists(BigImagePath))
                {
                    vieModel.CurrentVideo.PreviewImageList.Add(vieModel.CurrentVideo.BigImage);
                    vieModel.CurrentVideo.PreviewImagePathList.Add(BigImagePath);
                }
                else
                {
                    if (vieModel.CurrentVideo.BigImage == GlobalVariable.DefaultBigImage)
                    {
                        // 检查有无截图
                        Video video = vieModel.CurrentVideo;
                        string path = video.getScreenShot();
                        if (Directory.Exists(path))
                        {
                            string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                            if (array.Length > 0)
                            {
                                string imgPath = array[array.Length / 2];
                                vieModel.CurrentVideo.PreviewImageList.Add(ImageProcess.BitmapImageFromFile(imgPath));
                                vieModel.CurrentVideo.PreviewImagePathList.Add(imgPath);
                            }
                        }
                    }
                }
            }

            await Task.Run(async () =>
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                {
                    imageItemsControl.ItemsSource = vieModel.CurrentVideo.PreviewImageList;
                    SetImage(0);
                });
            });

            //扫描预览图目录
            List<string> imagePathList = new List<string>();
            string imagePath = vieModel.CurrentVideo.getExtraImage();
            if (isScreenShot) imagePath = vieModel.CurrentVideo.getScreenShot();
            await Task.Run(() =>
            {
                if (Directory.Exists(imagePath))
                {

                    foreach (var path in FileHelper.TryScanDIr(imagePath, "*.*", System.IO.SearchOption.AllDirectories))
                        imagePathList.Add(path);

                    if (imagePathList.Count > 0) imagePathList = imagePathList.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).CustomSort().ToList();
                }
            });

            //加载预览图/截图
            foreach (var path in imagePathList)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraImageDelegate(LoadExtraImage), GetExtraImage(path));
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraPathDelegate(LoadExtraPath), path);
                if (cancelLoadImage) break;
            }
            SetImage(0);
            return true;
        }


        private delegate void LoadExtraImageDelegate(BitmapSource bitmapSource);
        private void LoadExtraImage(BitmapSource bitmapSource)
        {
            vieModel.CurrentVideo.PreviewImageList.Add(bitmapSource);

        }

        private delegate void LoadExtraPathDelegate(string path);
        private void LoadExtraPath(string path)
        {
            if (vieModel.CurrentVideo.PreviewImagePathList != null)
                vieModel.CurrentVideo.PreviewImagePathList.Add(path);

        }

        private async void ExtraImageRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //切换为预览图
            await LoadImage();
            scrollViewer.ScrollToHorizontalOffset(0);
        }

        private async void ScreenShotRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //切换为截图
            await LoadImage(true);
            scrollViewer.ScrollToHorizontalOffset(0);
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

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ProgressBar.Visibility == Visibility.Collapsed && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.TextDecorations = System.Windows.TextDecorations.Underline;
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.TextDecorations = null;
        }

        private void OpenFilePath(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenSelectPath(vieModel.CurrentVideo.Path);
        }


        private void GetPlot(object sender, RoutedEventArgs e)
        {
            //if (JvedioServers.DMM.Url.IsProperUrl())
            //{
            //    Task.Run(async () =>
            //    {
            //        HttpResult httpResult = await new FANZACrawler(vieModel.CurrentVideo.id, true).Crawl();
            //        if (this.Dispatcher != null)
            //        {
            //            Dispatcher.Invoke((Action)delegate
            //            {
            //                RefreshCurrent();
            //            });
            //        }
            //    });

            //}
        }

        private async void StartScreenShot(object sender, MouseButtonEventArgs e)
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


        private void DownLoadMagnets(object sender, RoutedEventArgs e)
        {
            new Msgbox(this, "开发中").ShowDialog();
        }

        public void ShowSubsection(object sender)
        {
            ContextMenu contextMenu = (sender as Canvas).ContextMenu;
            contextMenu.Items.Clear();
            for (int i = 0; i < vieModel.CurrentVideo.SubSectionList.Count; i++)
            {
                string filepath = vieModel.CurrentVideo.SubSectionList[i];//这样可以，放在  PlayVideoWithPlayer 就超出索引
                MenuItem menuItem = new MenuItem();
                menuItem.Header = i + 1;
                menuItem.Click += (s, _) =>
                {
                    (GetWindowByName("Main") as Main)?.PlayVideoWithPlayer(filepath, DataID);
                };
                contextMenu.Items.Add(menuItem);
            }
            contextMenu.IsOpen = true;

        }

        private void PlayVedio(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.CurrentVideo.HasSubSection)
            {
                ShowSubsection(sender);
            }
            else
            {
                string filepath = vieModel.CurrentVideo.Path;
                Main main = GetWindowByName("Main") as Main;
                main.PlayVideoWithPlayer(filepath, vieModel.CurrentVideo.DataID);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            videoMapper.updateField("VideoType", comboBox.SelectedIndex.ToString(), new SelectWrapper<Video>().Eq("DataID", DataID));
        }

        private void DownLoadInfo(object sender, MouseButtonEventArgs e)
        {
            DownLoad(null, null);
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            border.Background = GlobalStyle.Common.HighLight.Background;

        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];


        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            string header = menu.Header.ToString();
            Video video = vieModel.CurrentVideo;
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

        private void ViewAssoDatas(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = true;
            vieModel.LoadViewAssoData();
        }

        private void HideAssoPopup(object sender, RoutedEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
        }

        private void ShowAssoSubSection(object sender, RoutedEventArgs e)
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
                        (GetWindowByName("Main") as Main)?.PlayVideoWithPlayer(filepath, dataID);
                    };
                    contextMenu.Items.Add(menuItem);
                }
                contextMenu.IsOpen = true;
            }
        }

        private void PlayAssoVideo(object sender, MouseButtonEventArgs e)
        {
            AssoDataPopup.IsOpen = false;
            FrameworkElement el = sender as FrameworkElement;
            long dataid = getDataID(el);
            Video video = getAssoVideo(dataid);
            if (video == null)
            {
                MessageCard.Error("无法播放该视频！");
                return;
            }
            (GetWindowByName("Main") as Main)?.PlayVideoWithPlayer(video.Path, DataID);
        }

        private Video getAssoVideo(long dataID)
        {
            if (dataID <= 0 || vieModel?.ViewAssociationDatas?.Count <= 0) return null;
            Video video = vieModel.ViewAssociationDatas.Where(item => item.DataID == dataID).First();
            if (video != null && video.DataID > 0) return video;
            return null;
        }

        private void CopyVID(object sender, MouseButtonEventArgs e)
        {
            string vid = (sender as Border).Tag.ToString();
            ClipBoard.TrySetDataObject(vid);
        }

        private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
        {
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
            }

        }
        public bool CanRateChange = false;
        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }

        private void CopyText(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ClipBoard.TrySetDataObject(textBlock.Text);
        }

        private void ShowDetails(object sender, MouseButtonEventArgs e)
        {
            long dataid = getDataID(sender as FrameworkElement);
            vieModel.Load(dataid);
            AssoDataPopup.IsOpen = false;
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

        private void Rate_ValueChanged_1(object sender, FunctionEventArgs<double> e)
        {
            if (!CanRateChange) return;
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            StackPanel stackPanel = rate.Parent as StackPanel;
            long id = getDataID(stackPanel);
            metaDataMapper.updateFieldById("Grade", rate.Value.ToString(), id);
            CanRateChange = false;
        }

        private void CopyVideoInfo(object sender, MouseButtonEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in infoStackPanel.Children)
            {
                if (item is StackPanel stackPanel)
                {
                    foreach (FrameworkElement element in stackPanel.Children)
                    {
                        if (element is TextBlock textBlock)
                            builder.Append(textBlock.Text);
                        else if (element is TextBox textBox)
                            builder.Append(textBox.Text);
                    }
                    builder.Append(Environment.NewLine);
                }
            }
            if (builder.Length > 0)
                ClipBoard.TrySetDataObject(builder.ToString());
            else
                MessageCard.Error("无信息！");
        }
    }







}
