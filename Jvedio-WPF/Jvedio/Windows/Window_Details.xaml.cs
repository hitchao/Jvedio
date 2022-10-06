using HandyControl.Data;
using Jvedio.CommonNet;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Logs;
using Jvedio.Core.Net;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.MapperManager;
using static Jvedio.VisualTools.WindowHelper;
using static SuperUtils.Media.ImageHelper;
using static SuperUtils.WPF.VisualTools.VisualHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_Details.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Details : Window
    {
        private List<string> oldLabels { get; set; }

        private VieModel_Details vieModel { get; set; }

        private static Main windowMain { get; set; }

        private Window_Edit windowEdit { get; set; }

        private List<long> DataIDs = new List<long>();

        public long DataID { get; set; }

        private bool cancelLoadImage { get; set; }// 切换到下一个影片时停止加载图片

        // 进度条
        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance { get; set; }

        private bool CanRateChange { get; set; }

        static Window_Details()
        {
            windowMain = GetWindowByName("Main") as Main;
        }

        public Window_Details(long dataID)
        {
            InitializeComponent();
            DataID = dataID;
            Init();
            InitProgressBar();
        }

        public void Init()
        {
            this.Height = SystemParameters.PrimaryScreenHeight * 0.8;
            this.Width = SystemParameters.PrimaryScreenHeight * 0.8 * 1230 / 720;
            DataIDs = new List<long>();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            SetShadow();            // 设置阴影
            SetSkin();
            vieModel = new VieModel_Details();
            vieModel.QueryCompleted += async delegate
            {
                SetStatus(true);    // 设置状态
                await LoadImage(vieModel.ShowScreenShot); // 加载图片
                renderMagnets();    // 显示磁力
                ShowActor();
                SetLabel();
            };
            vieModel.Load(DataID);
            this.DataContext = vieModel;
            rootGrid.Focus();       // 设置键盘左右可切换
            InitDataIDs();          // 设置切换的影片列表
            OpenOtherUrlMenuItem.Items.Clear(); // 设置右键菜单
        }

        private void SetLabel()
        {
            labelTagPanel.TagList = vieModel.CurrentVideo.LabelList;
            labelTagPanel.Refresh();
            oldLabels = vieModel.CurrentVideo.LabelList?.Select(arg => arg).ToList();
            vieModel.getLabels();
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TextBlock textBlock = border.Child as TextBlock;
            string text = textBlock.Text;
            addLabel(text.Substring(0, text.IndexOf("(")));
            searchLabelPopup.IsOpen = false;
        }

        private void SetShadow()
        {
            SuperControls.Style.Utils.DwmDropShadow.DropShadowToWindow(this);
        }

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

                    // 加载图片
                    string imagePath = actorInfo.GetImagePath(vieModel.CurrentVideo.Path);
                    BitmapImage smallimage = ReadImageFromFile(imagePath);
                    if (smallimage == null)
                    {
                        smallimage = MetaData.DefaultActorImage;
                        //// 根据地址下载图片
                        // if (!string.IsNullOrEmpty(actorInfo.ImageUrl))
                        // {
                        //    string url = actorInfo.ImageUrl;
                        //    string ext = Path.GetExtension(url);
                        //    string dir = Path.GetDirectoryName(imagePath);
                        //    string name = Path.GetFileNameWithoutExtension(imagePath);
                        //    string saveFileName = Path.Combine(dir, name + ext);

                        // Task.Run(() =>
                        //    {
                        //        StartDownLoadActorImage();
                        //    });

                        // }
                    }

                    actorInfo.SmallImage = smallimage;
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo);
                }
            }
        }

        private async void StartDownLoadActorImage()
        {
            // await HttpHelper.AsyncDownLoadFile(actorInfo.ImageUrl, CrawlerHeader.Default);
        }

        public void InitDataIDs()
        {
            DataIDs = new List<long>();
            SelectWrapper<Video> wrapper = windowMain?.CurrentWrapper;
            string sql = windowMain?.CurrentSQL;
            if (wrapper != null && !string.IsNullOrEmpty(sql))
            {
                if (Properties.Settings.Default.DetialWindowShowAllMovie)
                    sql = "select metadata.DataID" + sql + wrapper.toWhere(false) + wrapper.toOrder();
                else
                    sql = "select metadata.DataID" + sql + wrapper.toWhere(false) + wrapper.toOrder() + wrapper.toLimit();
                List<Dictionary<string, object>> list = videoMapper.Select(sql);
                if (list != null && list.Count > 0)
                    DataIDs = list.Select(arg => long.Parse(arg["DataID"].ToString())).ToList();
            }
        }

        private void InitProgressBar()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
                taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
        }

        public void Refresh()
        {
            vieModel.Load(DataID);
        }

        // todo
        public void SetSkin()
        {
            BgImage.Source = null;
            if (ConfigManager.Settings.DetailShowBg)
                BgImage.Source = StyleManager.BackgroundImage;
            ////设置字体
            // if (GlobalFont != null) this.FontFamily = GlobalFont;
        }

        public void DownLoad(object sender, RoutedEventArgs e)
        {
            Video video = vieModel.CurrentVideo;
            if (video == null || video.DataID <= 0) return;
            DownLoadTask task = new DownLoadTask(video, true, ConfigManager.Settings.OverrideInfo); // 详情页面下载预览图
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
                        vieModel.CurrentVideo.PreviewImageList.Add(ImageHelper.BitmapImageFromByte(arg.FileByte));
                    }
                });
            };
            if (!Global.DownloadManager.Dispatcher.Working)
                Global.DownloadManager.Dispatcher.BeginWork();
            windowMain?.addToDownload(task);
            windowMain?.setDownloadStatus();
        }

        public void GetScreenGif(object sender, RoutedEventArgs e)
        {
            Video video = vieModel.CurrentVideo;
            ScreenShotTask task = new ScreenShotTask(vieModel.CurrentVideo, true); // 详情页面下载预览图

            if (!Global.FFmpegManager.Dispatcher.Working)
                Global.FFmpegManager.Dispatcher.BeginWork();
            windowMain?.addToScreenShot(task);
        }

        public void GetScreenShot(object sender, RoutedEventArgs e)
        {
            Video video = vieModel.CurrentVideo;
            ScreenShotTask task = new ScreenShotTask(vieModel.CurrentVideo); // 详情页面下载预览图
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
            if (!Global.FFmpegManager.Dispatcher.Working)
                Global.FFmpegManager.Dispatcher.BeginWork();
            windowMain?.addToScreenShot(task);
        }

        public void CloseWindow(object sender, RoutedEventArgs e) => this.Close();

        // 显示类别
        public void ShowSameGenre(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Genre");
        }

        // 显示演员
        public void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null || grid.Tag == null) return;
            long.TryParse(grid.Tag.ToString(), out long actorID);
            if (actorID <= 0) return;
            windowMain.ShowSameActor(actorID);
            this.Close();
        }

        // 显示标签
        public void ShowSameLabel(object sender, MouseButtonEventArgs e)
        {
            string label = ((TextBlock)sender).Text;
            if (string.IsNullOrEmpty(label)) return;
            windowMain.ShowSameLabel(label);
            this.Close();
        }

        // 显示导演
        public void ShowSameDirector(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Director");
        }

        public void ShowSameSeries(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Series");
        }

        // 显示发行商
        public void ShowSameStudio(object sender, MouseButtonEventArgs e)
        {
            ShowSameString(sender, "Studio");
        }

        // 显示系列
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
            if (windowEdit != null) windowEdit.Close();
            windowEdit = new Window_Edit(vieModel.CurrentVideo.DataID);
            windowEdit.ShowDialog();
        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
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
            string path = GetExtraImagePath(sender as FrameworkElement);
            FileHelper.TryOpenSelectPath(path);
        }

        private string GetExtraImagePath(FrameworkElement element, int depth = 0)
        {
            if (element == null || depth < 0) return string.Empty;
            MenuItem menuItem = element as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (depth == 1) contextMenu = (menuItem.Parent as MenuItem).Parent as ContextMenu;
            Border border = contextMenu.PlacementTarget as Border;
            int.TryParse(border.Tag.ToString(), out int idx);
            if (idx >= 0 && idx < vieModel.CurrentVideo.PreviewImagePathList.Count)
                return vieModel.CurrentVideo.PreviewImagePathList[idx];
            return string.Empty;
        }

        public async void DeleteImage(object sender, RoutedEventArgs e)
        {
            string path = GetExtraImagePath(sender as FrameworkElement);
            if (string.IsNullOrEmpty(path)) return;
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
                    await Task.Delay(300); // todo 等待删除完成
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
            //if (vieModel.ShowScreenShot)
            //    videoMapper.UpdateFieldById("ScreenShotPath", "", vieModel.CurrentVideo.DataID);
            //else
            //    videoMapper.UpdateFieldById("PreviewImagePath", "", vieModel.CurrentVideo.DataID);
            await Task.Delay(300);
            Refresh();
            windowMain?.RefreshImage(vieModel.CurrentVideo);
        }

        // todo
        private void SetToSmallPic(object sender, RoutedEventArgs e)
        {
            // string path = getExtraImagePath(sender as FrameworkElement, 1);
            // videoMapper.UpdateFieldById("SmallImagePath", path, DataID);
            // windowMain?.RefreshImage(vieModel.CurrentVideo);
            // SuperControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
        }

        // todo
        private void SetToBigAndSmallPic(object sender, RoutedEventArgs e)
        {
            // string path = getExtraImagePath(sender as FrameworkElement, 1);
            //// todo 绝对地址
            // videoMapper.UpdateFieldById("SmallImagePath", path, DataID);
            // videoMapper.UpdateFieldById("BigImagePath", path, DataID);
            // Refresh();
            // windowMain?.RefreshImage(vieModel.CurrentVideo);
            // SuperControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
        }

        private void SetToBigPic(object sender, RoutedEventArgs e)
        {
            string path = GetExtraImagePath(sender as FrameworkElement, 1);
            videoMapper.UpdateFieldById("BigImagePath", path, DataID);
            Refresh();
            windowMain?.RefreshImage(vieModel.CurrentVideo);
            SuperControls.Style.MessageCard.Info(Jvedio.Language.Resources.Message_Success);
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
                SuperControls.Style.MessageCard.Error(Jvedio.Language.Resources.Message_FileNotExist);
            }
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            int num = 0;
            Video video = vieModel.CurrentVideo;
            if (video.SubSectionList?.Count > 0)
            {
                // 分段视频
                foreach (var path in video.SubSectionList)
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                            num++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
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
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
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
            if (string.IsNullOrEmpty(url)) return;
            if (url.IsProperUrl())
                FileHelper.TryOpenUrl(url);
        }

        public async void TranslateMovie(object sender, RoutedEventArgs e)
        {
            // if (IsTranslating) return;

            // if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO) { SuperControls.Style.MessageCard.Warning("请设置【有道翻译】并测试"); IsTranslating = false; return; }
            // string result = "";
            // MySqlite dataBase = new MySqlite("Translate");

            // CurrentVideo movie = vieModel.CurrentVideo;
            // IsTranslating = true;
            ////检查是否已经翻译过，如有提示
            // if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id)))
            // {
            //    if (new Msgbox(this, Jvedio.Language.Resources.AlreadyTranslate).ShowDialog() == false)
            //    {
            //        IsTranslating = false;
            //        return;
            //    }

            // }

            // string title = DataBase.SelectInfoByID("title", "movie", movie.id);

            // if (title != "")
            // {

            // if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(title);
            //    //保存
            //    if (result != "")
            //    {
            //        dataBase.SaveYoudaoTranslateByID(movie.id, title, result, "title");
            //        movie.title = result;
            //        UpdateInfo(movie);
            //    }
            //    else
            //    {
            //        SuperControls.Style.MessageCard.Info(Jvedio.Language.Resources.TranslateFail);
            //    }

            // }
            // string plot = DataBase.SelectInfoByID("plot", "movie", movie.id);
            // if (plot != "")
            // {
            //    if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(plot);
            //    //保存
            //    if (result != "")
            //    {
            //        dataBase.SaveYoudaoTranslateByID(movie.id, plot, result, "plot");
            //        movie.plot = result;
            //        UpdateInfo(movie);
            //        //SuperControls.Style.MessageCard.Info(Jvedio.Language.Resources.TranslateSuccess);
            //    }
            //    else
            //    {
            //        SuperControls.Style.MessageCard.Info(Jvedio.Language.Resources.TranslateFail);
            //    }

            // }
            // dataBase.CloseDB();
            // IsTranslating = false;
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
                // 设置为默认图片
                BigImage.Source = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            }
            else
            {
                if (idx < vieModel.CurrentVideo.PreviewImageList?.Count)
                    BigImage.Source = vieModel.CurrentVideo.PreviewImageList[idx];

                // 设置遮罩
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
            if (border == null || border.Tag == null) return;
            int idx = int.Parse(border.Tag.ToString());
            vieModel.SelectImageIndex = idx;
            SetImage(vieModel.SelectImageIndex);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Detail.ShowScreenShot = vieModel.ShowScreenShot;
            ConfigManager.Detail.InfoSelectedIndex = vieModel.InfoSelectedIndex;
            ConfigManager.Detail.Save();
        }

        private void BigImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        private void BigImage_Drop(object sender, DragEventArgs e)
        {
            // todo 拖动设置大图
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            // if (!IsFile(file)) return;
            // FileInfo fileInfo = new FileInfo(file);
            // FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"BigPic\\{vieModel.CurrentVideo.id}.jpg", true);
            // CurrentVideo CurrentVideo = vieModel.CurrentVideo;
            // CurrentVideo.bigimage = null;
            // CurrentVideo.bigimage = BitmapImageFromFile(fileInfo.FullName);

            // if (vieModel.CurrentVideo.extraimagelist.Count > 0)
            // {
            //    CurrentVideo.extraimagelist[0] = CurrentVideo.bigimage;
            //    CurrentVideo.extraimagePath[0] = fileInfo.FullName;

            // }
            // else
            // {
            //    CurrentVideo.extraimagelist.Add(CurrentVideo.bigimage);
            //    CurrentVideo.extraimagePath.Add(fileInfo.FullName);
            // }

            // vieModel.CurrentVideo = null;
            // vieModel.CurrentVideo = CurrentVideo;
            // RefreshUI("", fileInfo.FullName);
            // SetImage(0);
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            // 分为文件夹和文件
            // string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            // List<string> files = new List<string>();
            // StringCollection stringCollection = new StringCollection();
            // foreach (var item in dragdropFiles)
            // {
            //    if (IsFile(item))
            //        files.Add(item);
            //    else
            //        stringCollection.Add(item);
            // }
            // List<string> filepaths = new List<string>();
            ////扫描导入
            // foreach (var item in stringCollection)
            // {
            //    try { filepaths.AddRange(Directory.GetFiles(item, "*.jpg").ToList<string>()); }
            //    catch (Exception ex) { Console.WriteLine(ex.Message); continue; }
            // }
            // if (files.Count > 0) filepaths.AddRange(files);

            ////复制文件
            // string path;
            // if ((bool)ExtraImageRadioButton.IsChecked)
            //    path = BasePicPath + $"ExtraPic\\{vieModel.CurrentVideo.id}\\";
            // else
            //    path = BasePicPath + $"ScreenShot\\{vieModel.CurrentVideo.id}\\";

            // if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            // bool success = false;
            // foreach (var item in filepaths)
            // {
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

            // }
            // if (success)
            // {
            //    //更新UI
            //    if ((bool)ExtraImageRadioButton.IsChecked)
            //        ExtraImageRadioButton_Click(sender, new RoutedEventArgs());
            //    else
            //        ScreenShotRadioButton_Click(sender, new RoutedEventArgs());

            // SuperControls.Style.MessageCard.Success($"{Jvedio.Language.Resources.ImportNumber} {filepaths.Count}");

            // }
        }

        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (vieModel.CurrentVideo != null)
            {
                vieModel.SaveLove();

                // 更新主界面
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
            if (vieModel.CurrentVideo.PreviewImagePathList?.Count == 0) return;
            vieModel.SelectImageIndex += e.Delta > 0 ? -1 : 1;

            if (vieModel.SelectImageIndex < 0)
            {
                vieModel.SelectImageIndex = 0;
            }
            else if (vieModel.SelectImageIndex >= imageItemsControl.Items.Count)
            {
                vieModel.SelectImageIndex = imageItemsControl.Items.Count - 1;
            }

            SetImage(vieModel.SelectImageIndex);

            // 滚动到指定的
            ContentPresenter presenter = (ContentPresenter)imageItemsControl.ItemContainerGenerator
                .ContainerFromItem(imageItemsControl.Items[vieModel.SelectImageIndex]);
            presenter?.BringIntoView();
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
                NextMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else
                PreviousMovie(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
        }

        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            if (contextMenu == null || string.IsNullOrEmpty(header)) return null;
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
                if (magnet.Tags == null) continue;
                MenuItem menuItem = new MenuItem();
                string tag = string.Empty;
                if (magnet.Tags.Count > 0) tag = "（" + string.Join(" ", magnet.Tags) + "）";

                menuItem.Header = $"{magnet.Releasedate} {tag} {magnet.Size.ToProperFileSize()} {magnet.Title}";
                menuItem.ToolTip = menuItem.Header;
                menuItem.Click += (s, ev) => ClipBoard.TrySetDataObject(magnet.MagnetLink);
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

            // 加载大图到预览图
            DisposeImage();

            Video video = vieModel.CurrentVideo;

            video.PreviewImageList = new ObservableCollection<BitmapSource>();
            video.PreviewImagePathList = new ObservableCollection<string>();

            string bigImagePath = video.getBigImage();
            if (!isScreenShot)
            {
                if (File.Exists(bigImagePath))
                {
                    video.PreviewImageList.Add(video.BigImage);
                    video.PreviewImagePathList.Add(bigImagePath);
                }
                else
                {
                    if (video.BigImage == MetaData.DefaultBigImage)
                    {
                        // 检查有无截图
                        string path = video.getScreenShot();
                        if (Directory.Exists(path))
                        {
                            string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                            if (array.Length > 0)
                            {
                                string imgPath = array[array.Length / 2];
                                video.PreviewImageList.Add(ImageHelper.BitmapImageFromFile(imgPath));
                                video.PreviewImagePathList.Add(imgPath);
                            }
                        }
                    }
                }
            }

            await Task.Run(async () =>
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                {
                    imageItemsControl.ItemsSource = video.PreviewImageList;
                    SetImage(0);
                });
            });

            // 扫描预览图目录
            List<string> imagePathList = new List<string>();
            string imagePath = video.getExtraImage();
            if (isScreenShot) imagePath = video.getScreenShot();
            await Task.Run(() =>
            {
                if (Directory.Exists(imagePath))
                {
                    foreach (var path in FileHelper.TryScanDIr(imagePath, "*.*", System.IO.SearchOption.AllDirectories))
                        imagePathList.Add(path);

                    if (imagePathList.Count > 0) imagePathList = imagePathList.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).CustomSort().ToList();
                }
            });

            // 加载预览图/截图
            foreach (var path in imagePathList)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadExtraImageDelegate(LoadExtraImage), BitmapImageFromFile(path));
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
            // 切换为预览图
            await LoadImage();
            scrollViewer.ScrollToHorizontalOffset(0);
        }

        private async void ScreenShotRadioButton_Click(object sender, RoutedEventArgs e)
        {
            // 切换为截图
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

        // todo
        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            return;
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
            // if (JvedioServers.DMM.Url.IsProperUrl())
            // {
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

            // }
        }

        private void DownLoadMagnets(object sender, RoutedEventArgs e)
        {
            new Msgbox(this, "开发中").ShowDialog();
        }

        public void ShowSubsection(object sender)
        {
            ContextMenu contextMenu = (sender as Canvas).ContextMenu;
            contextMenu.Items.Clear();
            for (int i = 0; i < vieModel.CurrentVideo.SubSectionList?.Count; i++)
            {
                string filepath = vieModel.CurrentVideo.SubSectionList[i]; // 这样可以，放在  PlayVideoWithPlayer 就超出索引
                MenuItem menuItem = new MenuItem();
                menuItem.Header = i + 1;
                menuItem.Click += (s, _) => windowMain?.PlayVideoWithPlayer(filepath, DataID);
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
            contextMenu.Visibility = Visibility.Visible;
        }

        private void PlayVedio(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.CurrentVideo.HasSubSection)
                ShowSubsection(sender);
            else
                windowMain?.PlayVideoWithPlayer(vieModel.CurrentVideo.Path, vieModel.CurrentVideo.DataID);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            videoMapper.UpdateField("VideoType", comboBox.SelectedIndex.ToString(), new SelectWrapper<Video>().Eq("DataID", DataID));
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
            border.Background = StyleManager.Common.HighLight.Background;
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
            if (vieModel.ViewAssociationDatas == null) return;

            Button button = sender as Button;
            long dataID = getDataID(button);
            if (dataID <= 0) return;

            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();

            Video video = vieModel.ViewAssociationDatas.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (video != null)
            {
                for (int i = 0; i < video.SubSectionList?.Count; i++)
                {
                    string filepath = video.SubSectionList[i]; // 这样可以，放在  PlayVideoWithPlayer 就超出索引
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = i + 1;
                    menuItem.Click += (s, _) => windowMain?.PlayVideoWithPlayer(filepath, dataID);
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
            if (dataid <= 0) return;
            Video video = getAssoVideo(dataid);
            if (video == null)
            {
                MessageCard.Error("无法播放该视频！");
                return;
            }

            windowMain?.PlayVideoWithPlayer(video.Path, DataID);
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
            if (label == null || label.Tag == null) return;
            long.TryParse(label.Tag.ToString(), out long tagID);

            ItemsControl itemsControl = label.FindParentOfType<ItemsControl>();
            if (itemsControl == null || itemsControl.Tag == null || itemsControl.ItemsSource == null) return;
            long.TryParse(itemsControl.Tag.ToString(), out long DataID);
            if (tagID <= 0 || DataID <= 0) return;
            ObservableCollection<TagStamp> tagStamps = itemsControl.ItemsSource as ObservableCollection<TagStamp>;
            if (tagStamps == null) return;
            TagStamp tagStamp = tagStamps.Where(arg => arg.TagID.Equals(tagID)).FirstOrDefault();
            if (tagStamp != null)
            {
                tagStamps.Remove(tagStamp);
                string sql = $"delete from metadata_to_tagstamp where TagID='{tagID}' and DataID='{DataID}'";
                tagStampMapper.ExecuteNonQuery(sql);
            }
        }

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

        private void AssoDataRate_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            if (!CanRateChange) return;
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            StackPanel stackPanel = rate.Parent as StackPanel;
            long id = getDataID(stackPanel);
            if (id <= 0) return;
            metaDataMapper.UpdateFieldById("Grade", rate.Value.ToString(), id);
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

        private void NewLabel(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, "请输入");
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                addLabel(text);
            }
        }

        private void LabelChanged(object sender, ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
            {
                vieModel.CurrentVideo.Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, e.List);
                MapperManager.metaDataMapper.SaveLabel(vieModel.CurrentVideo.toMetaData(), oldLabels);
            }
        }

        private void addLabel(string label)
        {
            if (vieModel.CurrentVideo.LabelList == null)
                vieModel.CurrentVideo.LabelList = new System.Collections.Generic.List<string>();
            if (vieModel.CurrentVideo.LabelList.Contains(label)) return;
            vieModel.CurrentVideo.LabelList.Add(label);
            vieModel.CurrentVideo.Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, vieModel.CurrentVideo.LabelList);
            labelTagPanel.TagList = null;
            labelTagPanel.TagList = vieModel.CurrentVideo.LabelList;
            labelTagPanel.Refresh();
            MapperManager.metaDataMapper.SaveLabel(vieModel.CurrentVideo.toMetaData(), oldLabels);
        }

        private void AddToLabel(object sender, RoutedEventArgs e)
        {
            searchLabelPopup.IsOpen = true;
        }
    }
}
