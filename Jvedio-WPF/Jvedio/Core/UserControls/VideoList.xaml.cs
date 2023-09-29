using Google.Protobuf.WellKnownTypes;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Net;
using Jvedio.Core.UserControls.ViewModels;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Entity.CommonSQL;
using Microsoft.VisualBasic.FileIO;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Framework.Tasks;
using SuperUtils.IO;
using SuperUtils.Time;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.UserControls.VideoItemEventArgs;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.VisualHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// VideoList.xaml 的交互逻辑
    /// </summary>
    public partial class VideoList : UserControl, ITabItemControl
    {
        #region "事件"

        public static Action onStatistic;
        public static Action<bool> onSearchingChange;
        public static Action<long, long, bool> onTagStampChange;
        public static Action<List<Video>> onDeleteID;
        public static Action<string, bool> onWaiting;

        public Action<long, float> onGradeChange;
        public Action<WrapperEventArg<Video>> onRenderSql;
        public Action onInitTagStamps;
        public Action<string> onRenameFile;

        public static readonly RoutedEvent OnItemClickEvent =
            EventManager.RegisterRoutedEvent("OnItemClick", RoutingStrategy.Bubble,
                typeof(VideoItemEventHandler), typeof(VideoList));

        public event VideoItemEventHandler OnItemClick {
            add => AddHandler(OnItemClickEvent, value);
            remove => RemoveHandler(OnItemClickEvent, value);
        }

        public static readonly RoutedEvent OnItemViewAssoEvent =
            EventManager.RegisterRoutedEvent("OnItemViewAsso", RoutingStrategy.Bubble,
                typeof(VideoItemEventHandler), typeof(VideoList));

        public event VideoItemEventHandler OnItemViewAsso {
            add => AddHandler(OnItemViewAssoEvent, value);
            remove => RemoveHandler(OnItemViewAssoEvent, value);
        }

        #endregion

        #region "属性"

        private Style SearchBoxListItemContainerStyle { get; set; }
        private VieModel_VideoList vieModel { get; set; }

        private DispatcherTimer ResizingTimer { get; set; }
        private ScrollViewer dataScrollViewer { get; set; }
        private bool Resizing { get; set; }


        private object RefreshLock { get; set; } = new object();



        public TabItemEx TabItemEx { get; set; }

        private int firstIdx { get; set; } = -1;
        private int secondIdx { get; set; } = -1;
        private int actorFirstIdx { get; set; } = -1;
        private int actorSecondIdx { get; set; } = -1;

        private bool canShowDetails { get; set; }
        public List<ImageSlide> ImageSlides { get; set; }

        private bool CanRateChange { get; set; }

        #endregion

        public VideoList(SelectWrapper<Video> extraWrapper, TabItemEx tabItemEx)
        {
            InitializeComponent();

            vieModel = new VieModel_VideoList();
            this.DataContext = vieModel;

            vieModel.ExtraWrapper = extraWrapper;
            vieModel.UUID = tabItemEx.UUID;
            TabItemEx = tabItemEx;

            SetDataGrid();

            Init();
        }

        public void Init()
        {
            ResizingTimer = new DispatcherTimer();
            BindingEvent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            BindingEventAfterRender();
            Refresh(vieModel.CurrentPage);
        }

        public void Refresh(int page = -1)
        {
            if (page > 0 && vieModel.CurrentPage != page) {
                vieModel.CurrentPage = page;
                pagination.CurrentPage = page;
            } else {
                vieModel.Refresh();
            }
        }
        public void RefreshData(long dataID)
        {
            vieModel.RefreshData(dataID);
        }

        public void BindingEvent()
        {
            SetSortType();
            SetImageViewMode();
            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);
            vieModel.PageChangedStarted += PageChangedStarted;
            vieModel.PageChangedCompleted += PageChangedCompleted;
            vieModel.RenderSqlChanged += (s, ev) => onRenderSql?.Invoke(ev as WrapperEventArg<Video>);
            vieModel.onPageChange += (totalCount) => pagination.Total = totalCount;
            this.onInitTagStamps += this.filter.InitTagStamp;
            VieModel_VideoList.onSearchingChange += onSearchingChange;
            DownLoadTask.onDownloadSuccess += onDownloadSuccess;
            ScreenShotTask.onScreenShotCompleted += onScreenShotCompleted;
            Window_Edit.onRefreshData += RefreshData;
        }


        /// <summary>
        /// 设置排序类型
        /// </summary>
        private void SetSortType()
        {
            var menuItems = SortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < menuItems.Count; i++) {
                menuItems[i].Click += SortMenu_Click;
                menuItems[i].IsCheckable = true;
                if (i == vieModel.SortType)
                    menuItems[i].IsChecked = true;
            }

        }

        /// <summary>
        /// 设置图片显示模式
        /// </summary>
        private void SetImageViewMode()
        {
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            for (int i = 0; i < rbs.Count; i++) {
                rbs[i].Click += SetViewMode;
                if (i == vieModel.ShowImageMode)
                    rbs[i].IsChecked = true;
            }
        }

        private void PageChangedStarted()
        {
            GotoTop(null, null);
        }

        private void PageChangedCompleted(object sender, EventArgs ev)
        {
            if (vieModel.EditMode)
                SetSelected();
            if (ConfigManager.Settings.AutoGenScreenShot)
                AutoGenScreenShot(vieModel.CurrentVideoList);
            if (tableData.Visibility == Visibility.Visible && tableData.Items.Count > 0)
                tableData.ScrollIntoView(tableData.Items[0]);
        }

        private void onDownloadSuccess(DownLoadTask task)
        {
            Dispatcher.Invoke(() => {
                lock (RefreshLock) {
                    vieModel.RefreshData(task.DataID);
                    // 更新图片存在
                    if (task.Success)
                        UpdateImageIndex(task.DataID, true, true);
                }
            });
        }

        private void onScreenShotCompleted(bool ok, long dataID)
        {
            Dispatcher.Invoke(() => {
                if (ok) {
                    LoadImageAfterScreenShort(dataID);
                }
            });
        }

        private void ResizingTimer_Tick(object sender, EventArgs e)
        {
            Resizing = false;
            ResizingTimer.Stop();
        }

        private void AutoGenScreenShot(ObservableCollection<Video> data)
        {
            for (int i = 0; i < data.Count; i++) {
                if (data[i].BigImage == MetaData.DefaultBigImage ||
                    data[i].BigImage == MetaData.DefaultSmallImage) {
                    // 检查有无截图
                    Video video = data[i];
                    string path = video.GetScreenShot();
                    if (Directory.Exists(path)) {
                        string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (array.Length > 0) {
                            Video.SetImage(ref video, array[array.Length / 2]);
                            data[i].BigImage = null;
                            data[i].BigImage = video.ViewImage;
                        }
                    }
                }
            }
        }

        public void SetViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            if (radioButton == null)
                return;
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            ViewMode viewMode = (ViewMode)idx;

            vieModel.ShowImageMode = idx;

            if (idx == 0)
                vieModel.GlobalImageWidth = (int)ConfigManager.VideoConfig.SmallImage_Width;
            else if (idx == 1)
                vieModel.GlobalImageWidth = (int)ConfigManager.VideoConfig.BigImage_Width;
            else if (idx == 2) {
                vieModel.GlobalImageWidth = (int)ConfigManager.VideoConfig.GifImage_Width;
            } else if (idx == 3) {
                AsyncLoadGif();
            }
            SetDataGrid();
        }

        private void SetDataGrid()
        {
            if (vieModel.ShowImageMode == 2) {
                vieModel.ShowTable = true;
            } else {
                vieModel.ShowTable = false;
            }
        }

        /// <summary>
        /// todo 加载 gif
        /// </summary>
        public void AsyncLoadGif()
        {
            // if (vieModel.CurrentVideoList == null) return;
            // DisposeGif("", true);
            // Task.Run(async () =>
            // {
            //    for (int i = 0; i < vieModel.CurrentVideoList.Count; i++)
            //    {
            //        Video video = vieModel.CurrentVideoList[i];
            //        string gifPath = Video.parseImagePath(video.GifImagePath);
            //        if (video.GifUri != null && !string.IsNullOrEmpty(video.GifUri.OriginalString)
            //            && video.GifUri.OriginalString.IndexOf("/NoPrinting_G.gif") < 0) continue;
            //        if (File.Exists(gifPath))
            //            video.GifUri = new Uri(gifPath);
            //        else
            //            video.GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            //        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
            //        {
            //            vieModel.CurrentVideoList[i] = null;
            //            vieModel.CurrentVideoList[i] = video;
            //        });
            //    }
            // });
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++) {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem) {
                    item.IsChecked = true;
                    if (i == vieModel.SortType)
                        vieModel.SortDescending = !vieModel.SortDescending;
                    vieModel.SortType = i;
                } else {
                    item.IsChecked = false;
                }
            }

            vieModel.Refresh();
        }


        private void Pagination_CurrentPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentPage = pagination.CurrentPage;
            vieModel.PageQueue.Enqueue(pagination.CurrentPage);
            vieModel.LoadData();
        }

        private void MovieItemsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (e.Delta > 0) {
                    imageSizeSlider.Value += imageSizeSlider.LargeChange;
                } else {
                    imageSizeSlider.Value -= imageSizeSlider.LargeChange;
                }
                e.Handled = true;

            }
        }


        private void MovieItemsControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private ScrollViewer GetScrollViewer()
        {
            if (MovieItemsControl != null && VisualTreeHelper.GetChildrenCount(MovieItemsControl) > 0)
                return VisualTreeHelper.GetChild(MovieItemsControl, 0) as ScrollViewer;
            return null;
        }

        public void GotoTop(object sender, RoutedEventArgs e)
        {
            if (dataScrollViewer == null)
                dataScrollViewer = GetScrollViewer();
            dataScrollViewer?.ScrollToTop();
        }

        public void GotoBottom(object sender, RoutedEventArgs e)
        {
            if (dataScrollViewer == null)
                dataScrollViewer = GetScrollViewer();
            dataScrollViewer?.ScrollToBottom();
        }

        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            if (contextMenu == null || string.IsNullOrEmpty(header))
                return null;
            foreach (FrameworkElement element in contextMenu.Items) {
                if (element is MenuItem item && item.Header.ToString().Equals(header))
                    return item;
            }

            return null;
        }

        public void EditInfo(object sender, RoutedEventArgs e)
        {
            long dataID = 0;
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out dataID)) {

            } else {
                dataID = GetIDFromMenuItem(sender);
            }
            Window_Edit windowEdit = new Window_Edit(dataID);
            windowEdit.ShowDialog();
        }



        public async void DeleteIDs(ObservableCollection<Video> originSource, List<Video> to_delete, bool fromDetailWindow = true)
        {
            if (originSource == null || to_delete == null || to_delete.Count == 0 || originSource.Count == 0)
                return;
            if (!fromDetailWindow) {
                originSource.RemoveMany(to_delete);
                vieModel.VideoList.RemoveMany(to_delete);
            } else {
                // 影片只有单个
                Video video = to_delete[0];
                int idx = -1;
                for (int i = 0; i < originSource.Count; i++) {
                    if (originSource[i].DataID == video.DataID) {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0) {
                    originSource.RemoveAt(idx);
                    vieModel.VideoList.RemoveAt(idx);
                }
            }

            videoMapper.deleteVideoByIds(to_delete.Select(arg => arg.DataID.ToString()).ToList());

            // 关闭详情窗口
            if (!fromDetailWindow && GetWindowByName("Window_Details", App.Current.Windows) is Window window) {
                Window_Details windowDetails = (Window_Details)window;
                foreach (var item in to_delete) {
                    if (windowDetails.DataID == item.DataID) {
                        windowDetails.Close();
                        break;
                    }
                }
            }

            to_delete.Clear();
            onStatistic?.Invoke();

            await Task.Delay(200);
            vieModel.EditMode = false;
            vieModel.SelectedVideo.Clear();
            SetSelected();
            vieModel.Refresh();
        }


        private void AddDataAssociation(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);

            if (vieModel.SelectedVideo.Count == 0) {
                MessageNotify.Error("无选择的资源");
                return;
            }

            Window_SearchAsso window_SearchAsso = new Window_SearchAsso(vieModel.SelectedVideo);
            window_SearchAsso.Owner = Application.Current.MainWindow as Main;
            window_SearchAsso.OnDataRefresh += (dataid) => {
                vieModel.RefreshData(dataid);
            };
            window_SearchAsso.OnSelectData += () => {
                SetSelected();
            };
            window_SearchAsso.ShowDialog();

        }


        public void DeleteID(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Video> videos = HandleMenuSelected(sender);
            if (vieModel.EditMode &&
                new MsgBox(SuperControls.Style.LangManager.GetValueByKey("IsToDelete")).ShowDialog() == false)
                return;
            List<Video> temp = vieModel.SelectedVideo.ToList();
            DeleteIDs(videos, vieModel.SelectedVideo, false);
            onDeleteID?.Invoke(temp);
        }

        public void DeleteID(List<Video> list, bool fromDetail)
        {
            DeleteIDs(vieModel.CurrentVideoList, list, fromDetail);
        }

        private async Task<(int, int)> AsyncDeleteFile()
        {
            //return (0, 0);
            int num = 0;
            int totalCount = vieModel.SelectedVideo.Count;
            await Task.Run(() => {
                vieModel.SelectedVideo.ForEach((Action<Video>)(arg => {
                    if (arg.SubSectionList?.Count > 0) {
                        totalCount += arg.SubSectionList.Count - 1;

                        // 分段视频
                        foreach (var path in arg.SubSectionList.Select(t => t.Value)) {
                            if (File.Exists(path)) {
                                try {
                                    FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                    num++;
                                } catch (Exception ex) {
                                    Logger.Error(ex);
                                }
                            }
                        }
                    } else {
                        if (File.Exists(arg.Path)) {
                            try {
                                FileSystem.DeleteFile(arg.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                num++;
                            } catch (Exception ex) {
                                Logger.Error(ex);
                            }
                        }
                    }
                }));
            });
            return (num, totalCount);
        }

        public async void DeleteFile(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            if (vieModel.EditMode && new MsgBox(SuperControls.Style.LangManager.GetValueByKey("IsToDelete")).ShowDialog() == false) {
                return;
            }
            onWaiting?.Invoke("删除中", true);
            (int num, int totalCount) = await AsyncDeleteFile();
            //await Task.Delay(2000);
            if (ConfigManager.Settings.DelInfoAfterDelFile) {
                List<Video> temp = vieModel.SelectedVideo.ToList();
                DeleteIDs(GetVideosByMenu(sender as MenuItem, 0), vieModel.SelectedVideo, false);
                onDeleteID?.Invoke(temp);
            }

            MessageNotify.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_DeleteToRecycleBin")} {num}/{totalCount}");
            onWaiting?.Invoke("", false);
            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }


        public ObservableCollection<Video> GetVideosByMenu(MenuItem menuItem, int depth)
        {
            if (menuItem == null)
                return null;

            MenuItem p1 = menuItem;
            if (depth == 1)
                p1 = p1.Parent as MenuItem;

            if (p1 == null)
                return null;


            ContextMenu contextMenu = p1.Parent as ContextMenu;
            if (contextMenu == null || contextMenu.PlacementTarget == null)
                return null;

            ViewVideo viewVideo = contextMenu.PlacementTarget as ViewVideo;
            if (viewVideo == null)
                return null;

            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(viewVideo);
            if (itemsControl == null)
                return null;



            return itemsControl.ItemsSource as ObservableCollection<Video>;
        }

        public void RenameFile(object sender, RoutedEventArgs e)
        {
            if (ConfigManager.RenameConfig.FormatString.IndexOf("{") < 0) {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_SetRenameRule"));
                return;
            }

            HandleMenuSelected(sender, 1);

            ObservableCollection<Video> videos = GetVideosByMenu(sender as MenuItem, 1);
            if (videos == null)
                return;

            List<string> logs = new List<string>();
            TaskLogger logger = new TaskLogger(logs);
            List<Video> toRename = new List<Video>();
            foreach (Video video in vieModel.SelectedVideo) {
                if (File.Exists(video.Path)) {
                    toRename.Add(video);
                } else {
                    logger.Error(SuperControls.Style.LangManager.GetValueByKey("Message_FileNotExist") + $" => {video.Path}");
                }
            }

            int totalCount = toRename.Count;

            Dictionary<long, List<string>> dict = new Dictionary<long, List<string>>();

            // 重命名文件
            int successCount = RenameFile(toRename, logger, ref dict);

            // 更新
            if (dict.Count > 0) {

                UpdateVideo(dict, ref videos);
                MessageNotify.Success($"{SuperControls.Style.LangManager.GetValueByKey("Message_SuccessNum")} {successCount}/{totalCount} ");
            } else {
                MessageNotify.Info(LangManager.GetValueByKey("NoFileToRename"));
            }

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();

            if (logs.Count > 0)
                //onRenameFile?.Invoke(string.Join(Environment.NewLine, logs));
                new Dialog_Logs(string.Join(Environment.NewLine, logs)).ShowDialog(App.Current.MainWindow);
        }

        public int RenameFile(List<Video> toRename, TaskLogger logger, ref Dictionary<long, List<string>> dict)
        {
            int successCount = 0;
            foreach (Video video in toRename) {
                long dataID = video.DataID;
                Video newVideo = videoMapper.SelectVideoByID(dataID);
                string[] newPath = null;
                try {
                    newPath = newVideo.ToFileName();
                } catch (Exception ex) {
                    logger.Error(ex.Message);
                    continue;
                }

                if (newPath == null || newPath.Length == 0)
                    continue;

                if (newVideo.HasSubSection) {
                    bool success = false;
                    bool changed = false;
                    string[] oldPaths = newVideo.SubSectionList.Select(arg => arg.Value).ToArray();

                    // 判断是否改变了文件名
                    for (int i = 0; i < newPath.Length; i++) {
                        if (!newPath[i].Equals(oldPaths[i])) {
                            changed = true;
                            break;
                        }
                    }

                    if (!changed) {
                        //logger.Info(LangManager.GetValueByKey("SameFileNameToOrigin"));
                        break;
                    }

                    for (int i = 0; i < newPath.Length; i++) {
                        if (File.Exists(newPath[i])) {
                            logger.Error($"{LangManager.GetValueByKey("SameFileNameExists")} => {newPath[i]}");
                            newPath[i] = oldPaths[i]; // 换回原来的
                            continue;
                        }

                        try {
                            File.Move(video.SubSectionList[i].ToString(), newPath[i]);
                            success = true;
                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                            newPath[i] = oldPaths[i]; // 换回原来的
                            continue;
                        }
                    }

                    if (success)
                        successCount++;
                    if (!dict.ContainsKey(dataID))
                        dict.Add(dataID, newPath.ToList());
                } else {
                    string target = newPath[0];
                    string origin = newVideo.Path;
                    if (origin.Equals(target)) {
                        //logger.Info(LangManager.GetValueByKey("SameFileNameToOrigin") +
                        //    $"{Environment.NewLine}    origin: {origin}{Environment.NewLine}    target: {target}");
                        continue;
                    }

                    if (!File.Exists(target)) {
                        try {
                            File.Move(origin, target);
                            successCount++;
                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                            continue;
                        }

                        // 显示
                        if (!dict.ContainsKey(dataID))
                            dict.Add(dataID, new List<string>() { target });
                    } else {
                        logger.Error($"{LangManager.GetValueByKey("SameFileNameExists")} => {target}");
                    }
                }
            }
            return successCount;
        }


        public void UpdateVideo(Dictionary<long, List<string>> dict, ref ObservableCollection<Video> videos)
        {
            if (videos == null || videos.Count == 0)
                return;
            for (int i = 0; i < videos.Count; i++) {
                Video video = videos[i];
                long dataID = video.DataID;
                if (dict.ContainsKey(dataID)) {
                    if (video.HasSubSection) {
                        List<string> list = dict[dataID];
                        string subSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);
                        videos[i].Path = list[0];
                        videos[i].SubSection = subSection;
                        metaDataMapper.UpdateFieldById("Path", list[0], dataID);
                        videoMapper.UpdateFieldById("SubSection", subSection, dataID);
                    } else {
                        string path = dict[dataID][0];
                        videos[i].Path = path;
                        metaDataMapper.UpdateFieldById("Path", path, dataID);
                    }
                }
            }


        }


        public static void UpdateImageIndex(long dataID, bool smallImageExists = false, bool bigImageExists = false)
        {
            long pathType = ConfigManager.Settings.PicPathMode;
            List<string> list = new List<string>();
            // 小图
            list.Add($"({dataID},{pathType},0,{(smallImageExists ? 1 : 0)})");
            // 大图
            list.Add($"({dataID},{pathType},1,{(bigImageExists ? 1 : 0)})");
            string insertSql = $"begin;insert or replace into common_picture_exist(DataID,PathType,ImageType,Exist) values {string.Join(",", list)};commit;";
            MapperManager.videoMapper.ExecuteNonQuery(insertSql);
        }

        public void ReMoveZero(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);

            ObservableCollection<Video> videos = GetVideosByMenu(sender as MenuItem, 1);
            if (videos == null)
                return;

            int successNum = 0;
            for (int i = 0; i < vieModel.SelectedVideo.Count; i++) {
                Video video = vieModel.SelectedVideo[i];
                string oldVID = video.VID.ToUpper();

                Logger.Info($"remove vid zero, old vid: {oldVID}");

                if (oldVID.IndexOf("-") <= 0) {
                    Logger.Warn($"vid[{oldVID}] not contain '-'");
                    continue;
                }

                string num = oldVID.Split('-').Last();
                string eng = oldVID.Remove(oldVID.Length - num.Length, num.Length);
                if (num.StartsWith("00")) {
                    string newVID = eng + num.Remove(0, 2);
                    video.VID = newVID;
                    Logger.Info($"update vid from {oldVID} to {newVID}");
                    if (videoMapper.UpdateFieldById("VID", newVID, video.DataID)) {
                        successNum++;
                        vieModel.RefreshData(video.DataID);
                    }
                } else {
                    Logger.Warn($"{num} not starts with 00");
                }

            }

            MessageCard.Info($"{SuperControls.Style.LangManager.GetValueByKey("Message_Success")} {successNum}/{vieModel.SelectedVideo.Count}");

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }

        public void CopyFile(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            StringCollection paths = new StringCollection();
            int count = 0;
            int total = 0;
            foreach (var video in vieModel.SelectedVideo) {
                if (video == null)
                    continue;
                if (video.SubSectionList != null && video.SubSectionList.Count > 0) {
                    total += video.SubSectionList.Count;
                    foreach (var path in video.SubSectionList.Select(arg => arg.Value)) {
                        if (File.Exists(path)) {
                            paths.Add(path);
                            count++;
                        }
                    }
                } else {
                    total++;
                    if (File.Exists(video.Path)) {
                        paths.Add(video.Path);
                        count++;
                    }
                }
            }

            if (paths.Count <= 0) {
                MessageNotify.Warning(LangManager.GetValueByKey("CopyFileNameNull"));
                return;
            }

            bool success = ClipBoard.TrySetFileDropList(paths, (error) => { MessageCard.Error(error); });

            if (success)
                MessageNotify.Success($"{SuperControls.Style.LangManager.GetValueByKey("Message_Copied")} {count}/{total}");

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }

        public void CutFile(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            StringCollection paths = new StringCollection();
            int count = 0;
            int total = 0;
            foreach (var video in vieModel.SelectedVideo) {
                if (video.SubSectionList?.Count > 0) {
                    total += video.SubSectionList.Count;
                    foreach (var path in video.SubSectionList.Select(arg => arg.Value)) {
                        if (File.Exists(path)) {
                            paths.Add(path);
                            count++;
                        }
                    }
                } else {
                    total++;
                    if (File.Exists(video.Path)) {
                        paths.Add(video.Path);
                        count++;
                    }
                }
            }

            if (paths.Count <= 0) {
                MessageNotify.Warning(LangManager.GetValueByKey("CutFileNameNull"));
                return;
            }

            bool success = ClipBoard.TryCutFileDropList(paths, (error) => { MessageCard.Error(error); });

            if (success)
                MessageNotify.Success($"{LangManager.GetValueByKey("Cut")} {count}/{total}");

            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }


        public void RefreshGrade(long dataID, float grade)
        {
            if (dataID <= 0)
                return;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
                if (vieModel.CurrentVideoList[i].DataID == dataID) {
                    vieModel.CurrentVideoList[i].Grade = grade;
                    onStatistic?.Invoke();
                    break;
                }
            }
        }

        public void RefreshImage(Video newVideo)
        {
            if (newVideo == null || newVideo.DataID <= 0 || vieModel.CurrentVideoList?.Count <= 0)
                return;
            long dataId = newVideo.DataID;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
                if (vieModel.CurrentVideoList[i]?.DataID == dataId) {
                    Video video = videoMapper.SelectOne(new SelectWrapper<Video>().Eq("DataID", dataId));
                    if (video == null)
                        continue;
                    Video.SetImage(ref video);
                    vieModel.CurrentVideoList[i].SmallImage = null;
                    vieModel.CurrentVideoList[i].BigImage = null;
                    vieModel.CurrentVideoList[i].SmallImage = video.SmallImage;
                    vieModel.CurrentVideoList[i].BigImage = video.BigImage;
                    break;
                }
            }
        }


        /// <summary>
        /// 将点击的该项也加入到选中列表中
        /// </summary>
        /// <param name="dataID"></param>
        private ObservableCollection<Video> HandleMenuSelected(object sender, int depth = 0)
        {
            long dataID = GetIDFromMenuItem(sender, depth);
            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();

            ObservableCollection<Video> videos = GetVideosByMenu(sender as MenuItem, depth);
            if (videos == null)
                return null;

            Video currentVideo = videos.FirstOrDefault(arg => arg.DataID == dataID);
            if (currentVideo == null)
                return null;
            if (!vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any())
                vieModel.SelectedVideo.Add(currentVideo);
            return videos;
        }

        /// <summary>
        /// 打开网址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenWeb(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);

            // 超过 3 个网页，询问是否继续
            if (vieModel.SelectedVideo.Count >= 3 && new MsgBox(
                $"{LangManager.GetValueByKey("ReadyToOpenReadyToOpen")} {vieModel.SelectedVideo.Count} {LangManager.GetValueByKey("SomeWebSite")}").ShowDialog() == false)
                return;

            foreach (Video video in vieModel.SelectedVideo) {
                video.OpenWeb();
            }
        }

        private long GetIDFromMenuItem(object sender, int depth = 0)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = null;
            if (depth == 0) {
                contextMenu = mnu.Parent as ContextMenu;
            } else {
                MenuItem _mnu = mnu.Parent as MenuItem;
                contextMenu = _mnu.Parent as ContextMenu;
            }

            FrameworkElement ele = contextMenu.PlacementTarget as FrameworkElement;
            if (ele.Tag is Video video)
                return video.DataID;
            return -1;
        }

        public void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_DeleteInfo"));
                if (menuItem != null)
                    DeleteID(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.T) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_DeleteFile"));
                if (menuItem != null)
                    DeleteFile(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.S) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_SyncInfo"));
                if (menuItem != null)
                    DownLoadSelectMovie(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.E) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_EditInfo"));
                if (menuItem != null)
                    EditInfo(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.W) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_OpenWebSite"));
                if (menuItem != null)
                    OpenWeb(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.C) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_CopyFile"));
                if (menuItem != null)
                    CopyFile(menuItem, new RoutedEventArgs());
            } else if (e.Key == Key.X) {
                MenuItem menuItem = GetMenuItem(contextMenu, SuperControls.Style.LangManager.GetValueByKey("Menu_CopyFile"));
                if (menuItem != null)
                    CutFile(menuItem, new RoutedEventArgs());
            }

            contextMenu.IsOpen = false;
        }


        private void DownLoadSelectMovie(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender);
            DownLoadVideo(vieModel.SelectedVideo);
        }

        public void DownLoadVideo(List<Video> videoList)
        {
            foreach (Video video in videoList) {
                DownLoadTask.DownloadVideo(video);
            }
            App.DownloadManager.Start();
        }

        private void SetSelectMode(object sender, RoutedEventArgs e)
        {
            SuperControls.Style.Switch s = sender as SuperControls.Style.Switch;
            vieModel.SelectedVideo.Clear();
            SetSelected();
        }

        public void SetSelected()
        {
            ItemsControl itemsControl = MovieItemsControl;
            if (itemsControl == null)
                return;

            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null)
                    continue;
                ViewVideo viewVideo = FindElementByName<ViewVideo>(presenter, "viewVideo");
                if (viewVideo == null)
                    continue;
                long dataID = GetDataID(viewVideo);

                viewVideo.SetEditMode(vieModel.EditMode);

                if (dataID > 0) {
                    viewVideo.SetBackground((SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"]);
                    viewVideo.SetBorderBrush(Brushes.Transparent);
                    if (vieModel.EditMode && vieModel.SelectedVideo != null &&
                        vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any()) {

                        viewVideo.SetBackground(StyleManager.Common.HighLight.Background);
                        viewVideo.SetBorderBrush(StyleManager.Common.HighLight.BorderBrush);
                    }
                }
            }
        }


        private long GetDataID(ViewVideo viewVideo)
        {
            if (viewVideo != null && viewVideo.Tag != null && viewVideo.Tag is Video video && video.DataID > 0) {
                return video.DataID;
            }
            return -1;
        }

        private void RandomDisplay(object sender, RoutedEventArgs e)
        {
            vieModel.RandomDisplay();
        }



        private void NavigationToLetter(object sender, RoutedEventArgs e)
        {
            // vieModel.SearchFirstLetter = true;
            // vieModel.Search = ((Button)sender).Content.ToString();
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
            if (vieModel.CurrentVideoList == null || vieModel.SelectedVideo == null)
                return;
            vieModel.EditMode = true;
            bool allContain = true; // 检测是否取消选中
            foreach (var item in vieModel.CurrentVideoList) {
                if (!vieModel.SelectedVideo.Contains(item)) {
                    vieModel.SelectedVideo.Add(item);
                    allContain = false;
                }
            }

            if (allContain)
                vieModel.SelectedVideo.RemoveMany(vieModel.CurrentVideoList);
            SetSelected();
        }



        public void BindingEventAfterRender()
        {
            // 翻页完成
            pagination.PageSizeChange += (s, e) => {
                Pagination pagination = s as Pagination;
                vieModel.PageSize = pagination.PageSize;
                vieModel.LoadData();
            };


            // 搜索
            searchBox.TextChanged += RefreshCandidate;
            searchTabControl.SelectionChanged += (s, e) => {
                if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0].GetType() != typeof(string)) {
                    // 不知道为啥，点击 tabitem 会导致 SelectionChanged
                    RefreshCandidate(null, null);
                }
            };

            // 搜索的 style
            // 参考：https://social.msdn.microsoft.com/Forums/vstudio/en-US/cefcfaa5-cb86-426f-b57a-b31a3ea5fcdd/how-to-add-eventsetter-by-code?forum=wpf
            SearchBoxListItemContainerStyle = (System.Windows.Style)this.Resources["SearchBoxListItemContainerStyle"];
            EventSetter eventSetter = new EventSetter() {
                Event = ListBoxItem.MouseDoubleClickEvent,
                Handler = new MouseButtonEventHandler(ListBoxItem_MouseDoubleClick)
            };

            SearchBoxListItemContainerStyle.Setters.Add(eventSetter);

            actorInfoView.Close += () => vieModel.ShowActorGrid = false;

        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem &&
                listBoxItem.Content != null &&
                listBoxItem.Content.ToString() is string search) {
                BeginSearch(search);
                e.Handled = true;
            }

        }

        private async void RefreshCandidate(object sender, TextChangedEventArgs e)
        {
            Logger.Info("refresh candidate");
            List<string> list = await vieModel.GetSearchCandidate();
            int idx = vieModel.SearchSelectedIndex;
            if (idx < 0 || idx >= searchTabControl.Items.Count)
                return;
            TabItem tabItem = searchTabControl.Items[idx] as TabItem;
            AddOrRefreshItem(tabItem, list);
        }

        private void AddOrRefreshItem(TabItem tabItem, List<string> list)
        {
            ListBox listBox;
            if (tabItem.Content == null) {
                listBox = new ListBox();
                tabItem.Content = listBox;
                listBox.Margin = new Thickness(0, 0, 0, 5);
                listBox.Style = (System.Windows.Style)App.Current.Resources["NormalListBox"];
                listBox.ItemContainerStyle = SearchBoxListItemContainerStyle;
                listBox.Background = Brushes.Transparent;
                listBox.PreviewKeyUp += searchTabItem_PreviewKeyUp;
            } else {
                listBox = tabItem.Content as ListBox;
            }
            listBox.ItemsSource = list;
            if (!string.IsNullOrEmpty(vieModel.SearchText))
                vieModel.Searching = true;
        }



        public void GenerateAllScreenShot(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(ConfigManager.FFmpegConfig.Path)) {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_SetFFmpeg"));
                return;
            }

            List<Video> videos = Video.GetAllByDBID(ConfigManager.Main.CurrentDBId);
            if (videos == null || videos.Count == 0) {
                MessageNotify.Error("数目为空");
                return;
            }

            if (new MsgBox($"即将开始截图 {videos.Count} 个资源，是否继续？").ShowDialog() == false) {
                return;
            }

            foreach (Video video in videos) {
                ScreenShotTask.ScreenShotVideo(video);
            }
        }


        public void DownloadAllVideo(object sender, RoutedEventArgs e)
        {
            List<Video> videos = Video.GetAllByDBID(ConfigManager.Main.CurrentDBId);

            if (videos == null || videos.Count == 0) {
                MessageNotify.Error("数目为空");
                return;
            }

            if (new MsgBox($"即将开始同步 {videos.Count} 个资源，是否继续？").ShowDialog() == false) {
                return;
            }

            MessageCard.Warning(LangManager.GetValueByKey("CrawlAllWarning"));
            DownLoadVideo(videos);
        }



        private void LoadImageAfterScreenShort(long dataID)
        {
            if (dataID <= 0)
                return;
            for (int i = 0; i < vieModel.CurrentVideoList.Count; i++) {
                if (vieModel.CurrentVideoList[i] == null)
                    continue;
                try {
                    if (!dataID.Equals(vieModel.CurrentVideoList[i].DataID))
                        continue;
                    if (vieModel.CurrentVideoList[i].BigImage == MetaData.DefaultBigImage) {
                        // 检查有无截图
                        Video currentVideo = vieModel.CurrentVideoList[i];
                        string path = currentVideo.GetScreenShot();
                        if (Directory.Exists(path)) {
                            string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                            if (array.Length > 0) {
                                Video.SetImage(ref currentVideo, array[array.Length / 2]);
                                vieModel.CurrentVideoList[i].BigImage = null;
                                vieModel.CurrentVideoList[i].BigImage = currentVideo.ViewImage;
                                // 更新索引
                                UpdateImageIndex(currentVideo.DataID, false, true);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
                break;
            }
        }


        public Video GetVideoFromChildEle(FrameworkElement ele, long dataID)
        {
            if (ele == null)
                return null;
            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(ele);
            if (itemsControl == null)
                return null;
            ObservableCollection<Video> videos = itemsControl.ItemsSource as ObservableCollection<Video>;
            if (videos == null)
                return null;
            return videos.FirstOrDefault(arg => arg.DataID == dataID);
        }

        public void GenerateGif(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);
            GenerateScreenShot(vieModel.SelectedVideo, true);
        }

        public void GenerateScreenShot(object sender, RoutedEventArgs e)
        {
            HandleMenuSelected(sender, 1);
            GenerateScreenShot(vieModel.SelectedVideo);
        }

        public void GenerateScreenShot(List<Video> Videos, bool gif = false)
        {
            if (!File.Exists(ConfigManager.FFmpegConfig.Path)) {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_SetFFmpeg"));
                return;
            }

            foreach (Video video in Videos) {
                ScreenShotTask.ScreenShotVideo(video, gif);
            }
        }


        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (vieModel.Rendering) {
                e.Handled = true;
                return;
            }

            // 标记
            FrameworkElement element = sender as FrameworkElement;
            if (element == null || element.Tag == null)
                return;
            Video video = element.Tag as Video;
            if (video == null)
                return;

            long dataID = video.DataID;

            if (dataID <= 0)
                return;

            ContextMenu contextMenu = element.ContextMenu;
            if (contextMenu == null)
                return;

            ItemsControl itemsControl = VisualHelper.FindParentOfType<ItemsControl>(element);
            if (itemsControl == null)
                return;

            ObservableCollection<Video> videos = itemsControl.ItemsSource as ObservableCollection<Video>;
            if (videos == null)
                return;

            List<string> tagIDs = new List<string>();
            if (!string.IsNullOrEmpty(video.TagIDs))
                tagIDs = video.TagIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (FrameworkElement item in contextMenu.Items) {
                if ("TagMenuItems".Equals(item.Name) && item is MenuItem menuItem) {
                    menuItem.Items.Clear();
                    TagStamp.TagStamps.ForEach(arg => {
                        string tagID = arg.TagID.ToString();
                        MenuItem menu = new MenuItem() {
                            Header = arg.TagName,
                            IsCheckable = true,
                            IsChecked = tagIDs.Contains(tagID),
                        };
                        menu.Click += (s, ev) => {
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
            HandleMenuSelected(sender, 1);

            MenuItem menuItem = sender as MenuItem;
            bool deleted = false;
            if (menuItem != null)
                deleted = !menuItem.IsChecked;

            // 构造 sql 语句
            if (vieModel.SelectedVideo?.Count <= 0)
                return;

            if (deleted) {
                StringBuilder builder = new StringBuilder();
                foreach (var item in vieModel.SelectedVideo) {
                    builder.Append($"delete from metadata_to_tagstamp where DataID={item.DataID} and TagID={tagID};");
                }

                string sql = "begin;" + builder.ToString() + "commit;";
                tagStampMapper.ExecuteNonQuery(sql);
            } else {
                List<string> values = new List<string>();
                foreach (var item in vieModel.SelectedVideo) {
                    values.Add($"({item.DataID},{tagID})");
                }

                if (values.Count <= 0)
                    return;
                string sql = $"insert or replace into metadata_to_tagstamp (DataID,TagID)  values {string.Join(",", values)}";
                tagStampMapper.ExecuteNonQuery(sql);
            }

            onInitTagStamps?.Invoke();

            // 更新主界面
            ObservableCollection<Video> datas = GetVideosByMenu(menuItem, 1);

            if (datas != null) {
                foreach (var item in vieModel.SelectedVideo) {
                    long dataID = item.DataID;
                    if (dataID <= 0 || tagID <= 0 || datas == null || datas.Count == 0)
                        continue;
                    for (int i = 0; i < datas.Count; i++) {
                        if (datas[i].DataID == dataID) {
                            Video video = datas[i];
                            Video.RefreshTagStamp(ref video, tagID, deleted);
                            onTagStampChange?.Invoke(dataID, tagID, deleted);
                            break;
                        }
                    }
                }
            }


            if (!vieModel.EditMode)
                vieModel.SelectedVideo.Clear();
        }



        public void RefreshTagStamps(long tagID)
        {
            List<long> toRefreshData = new List<long>();
            foreach (var video in vieModel.CurrentVideoList) {
                string tagIDs = video.TagIDs;
                List<string> list = tagIDs.Split(',').ToList();
                if (!list.Contains(tagID.ToString()))
                    continue;
                toRefreshData.Add(video.DataID);
            }
            foreach (var item in toRefreshData) {
                vieModel.RefreshTagStamp(item);
            }
        }



        public void SetActor(ActorInfo actorInfo)
        {
            vieModel.ShowActorGrid = true;
            vieModel.ShowActorToggle = true;
            actorInfoView.CurrentActorInfo = actorInfo;
        }

        public bool IsShowActor()
        {
            return vieModel.ShowActorGrid;
        }

        public ActorInfo GetCurrentActor()
        {
            return actorInfoView.CurrentActorInfo;
        }


        private void OpenPath(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (menu == null)
                return;

            ObservableCollection<Video> datas = GetVideosByMenu(menu, 1);
            if (datas == null)
                return;


            string header = menu.Header.ToString();

            OpenPathType openPathType = Video.StringToImageType(header);

            long dataID = GetIDFromMenuItem(sender, 1);
            if (dataID <= 0)
                return;
            Video video = datas.Where(arg => arg.DataID == dataID).FirstOrDefault();
            video?.OpenPath(openPathType);
        }


        private void AddToPlayerList(object sender, RoutedEventArgs e)
        {
            string playerPath = ConfigManager.Settings.VideoPlayerPath;
            bool success = false;

            if (!File.Exists(playerPath)) {
                MessageNotify.Error(LangManager.GetValueByKey("VideoPlayerPathNotSet"));
                return;
            }

            HandleMenuSelected(sender);
            if (Path.GetFileName(playerPath).ToLower().Equals("PotPlayerMini64.exe".ToLower())) {
                List<string> list = vieModel.SelectedVideo
                    .Where(arg => File.Exists(arg.Path)).Select(arg => arg.Path).ToList();
                if (list.Count > 0) {
                    // potplayer 添加清单
                    string processParameters = $"\"{playerPath}\" \"{string.Join("\" \"", list)}\" /add";
                    using (Process process = new Process()) {
                        process.StartInfo.FileName = "cmd.exe";

                        // process.StartInfo.Arguments = arguments;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardInput = true; // 接受来自调用程序的输入信息
                        process.Start();
                        process.StandardInput.WriteLine(processParameters);
                        process.StandardInput.AutoFlush = true;
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        // if (process.ExitCode != 0)
                        //    MessageCard.Error("添加失败");
                    }
                }

                success = true;
            }

            if (!success)
                MessageNotify.Error(LangManager.GetValueByKey("SupportPotPlayerOnly"));
        }

        private void onItemShowDetail(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele != null && ele.Tag != null && ele.Tag is Video video) {
                if (vieModel.EditMode) {
                    if (vieModel.CurrentVideoList == null)
                        return;
                    // 多选
                    int selectIdx = vieModel.CurrentVideoList.IndexOf(video);

                    // 多选
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
                        if (firstIdx == -1)
                            firstIdx = selectIdx;
                        else
                            secondIdx = selectIdx;
                    }

                    if (firstIdx >= 0 && secondIdx >= 0) {
                        if (firstIdx > secondIdx) {
                            // 交换一下顺序
                            int temp = firstIdx;
                            firstIdx = secondIdx - 1;
                            secondIdx = temp - 1;
                        }

                        for (int i = firstIdx + 1; i <= secondIdx; i++) {
                            Video m = vieModel.CurrentVideoList[i];
                            if (vieModel.SelectedVideo.Contains(m))
                                vieModel.SelectedVideo.Remove(m);
                            else
                                vieModel.SelectedVideo.Add(m);
                        }

                        firstIdx = -1;
                        secondIdx = -1;
                    } else {
                        if (vieModel.SelectedVideo.Contains(video))
                            vieModel.SelectedVideo.Remove(video);
                        else
                            vieModel.SelectedVideo.Add(video);
                    }

                    SetSelected();
                } else {
                    RaiseEvent(new VideoItemEventArgs(video.DataID, OnItemClickEvent, sender));
                }
            }
        }

        private void viewVideo_ImageMouseEnter(object sender, RoutedEventArgs e)
        {
            if (vieModel.EditMode && sender is ViewVideo viewVideo) {
                viewVideo.SetBorderBrush(StyleManager.Common.HighLight.BorderBrush);
            }
        }

        private void viewVideo_ImageMouseLeave(object sender, RoutedEventArgs e)
        {
            if (vieModel.EditMode &&
                sender is ViewVideo viewVideo &&
                GetDataID(viewVideo) is long dataID && dataID > 0) {
                if (vieModel.SelectedVideo.Where(arg => arg.DataID == dataID).Any()) {
                    viewVideo.SetBorderBrush(StyleManager.Common.HighLight.BorderBrush);
                } else {
                    viewVideo.SetBorderBrush(Brushes.Transparent);
                }
            }
        }

        private void viewVideo_OnPlayVideo(object sender, RoutedEventArgs e)
        {
            if (!vieModel.EditMode &&
                sender is ViewVideo viewVideo &&
                GetDataID(viewVideo) is long dataId && dataId > 0) {

                Video video = GetVideoFromChildEle(viewVideo, dataId);
                PlayVideo(video);
            }
        }

        private void PlayVideo(Video video)
        {
            if (video == null) {
                MessageNotify.Error(LangManager.GetValueByKey("CanNotPlay"));
                return;
            }
            long dataId = video.DataID;
            string sql = $"delete from metadata_to_tagstamp where TagID='{TagStamp.TAG_ID_NEW_ADD}' and DataID='{dataId}'";
            tagStampMapper.ExecuteNonQuery(sql);
            onInitTagStamps?.Invoke();
            vieModel.RefreshData(dataId);
            Video.PlayVideoWithPlayer(video.Path, dataId);
        }

        private void OnPlayVideo(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out long dataID) &&
                dataID > 0) {
                Video video = videoMapper.SelectVideoByID(dataID);
                PlayVideo(video);
            }
        }

        private void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {

        }

        private void TranslateMovie(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateSmallImage(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateActor(object sender, RoutedEventArgs e)
        {

        }

        private void FilterClose()
        {
            vieModel.ShowFilter = false;
        }

        private void Filter_OnApplyWrapper(object sender, EventArgs ev)
        {
            if (ev is WrapperEventArg<Video> e &&
                e.Wrapper != null &&
                e.Wrapper is SelectWrapper<Video> wrapper) {
                vieModel.FilterWrapper = wrapper;
                vieModel.FilterSQL = e.SQL;
                vieModel.LoadData();
            }
        }


        private void viewVideo_OnTagStampRemove(object sender, RoutedEventArgs e)
        {
            this.filter.InitTagStamp();
        }

        private void viewVideo_OnViewAssoData(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ele &&
                ele.Tag is Video video)
                RaiseEvent(new VideoItemEventArgs(video.DataID, OnItemViewAssoEvent, sender));
        }

        public void SetAsso(bool asso)
        {
            vieModel.ShowAsso = asso;
        }


        private void doSearch(object sender, RoutedEventArgs e)
        {
            int idx = vieModel.SearchSelectedIndex;
            if (idx < 0)
                idx = 0;
            //SearchMode mode = (SearchMode)vieModel.TabSelectedIndex;
            vieModel.Query((SearchField)idx);
            //SaveSearchHistory(mode, (SearchField)idx);
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



        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                vieModel.Searching = false;
            } else if (e.Key == Key.Down || e.Key == Key.Up) {
                if (searchTabControl.SelectedItem is TabItem tabItem &&
                    tabItem != null && tabItem.Content is ListBox listbox && listbox.Items.Count > 0) {
                    listbox.Focus();
                    //listbox.SelectedIndex = 0;
                }
            } else if (e.Key == Key.Escape) {
                vieModel.Searching = false;
            } else if (e.Key == Key.Delete) {
                searchBox.ClearText();
            } else if (e.Key == Key.Tab) {

            }
        }

        private void searchTabItem_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            App.Logger.Info(e.Key.ToString());
            if (e.Key == Key.Left) {
                int idx = searchTabControl.SelectedIndex - 1;
                if (idx < 0)
                    idx = searchTabControl.Items.Count - 1;
                searchTabControl.SelectedIndex = idx;
                e.Handled = true;
            } else if (e.Key == Key.Right) {
                int idx = searchTabControl.SelectedIndex + 1;
                if (idx >= searchTabControl.Items.Count - 1) {
                    idx = 0;
                }
                searchTabControl.SelectedIndex = idx;
                e.Handled = true;
            } else if (e.Key == Key.Enter) {
                if (sender is ListBox listBox && listBox.Items.Count > 0 && listBox.SelectedItem is string search
                    && !string.IsNullOrEmpty(search)) {
                    BeginSearch(search);
                    e.Handled = true;
                }
            }
        }

        private void BeginSearch(string search)
        {
            searchBox.TextChanged -= RefreshCandidate;
            vieModel.SearchText = search;
            doSearch(null, null);
            vieModel.Searching = false;
            searchBox.TextChanged += RefreshCandidate;
        }

        private void searchTabControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                vieModel.Searching = false;
                searchBox.SetFocus();
            }
        }

        public void SetSearchFocus()
        {
            searchBox.SetFocus();
        }

        public void ResetSearch()
        {
            vieModel.SearchText = "";
            vieModel.SearchWrapper = null;
            vieModel.Searching = false;
        }

        public void NextPage()
        {
            pagination.NextPage();
        }

        public void PreviousPage()
        {
            pagination.PrevPage();
        }

        public void GoToTop()
        {
            GotoTop(null, null);
        }

        public void GoToBottom()
        {
            GotoBottom(null, null);
        }

        public void FirstPage()
        {
            pagination.FirstPage();
        }

        public void LastPage()
        {
            pagination.LastPage();
        }

        private void Rate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }


        private void Rate_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            if (!CanRateChange)
                return;

            if (sender is Rate rate &&
                rate.Tag != null &&
                long.TryParse(rate.Tag.ToString(), out long dataID) && dataID > 0) {
                metaDataMapper.UpdateFieldById("Grade", rate.Value.ToString(), dataID);
                onStatistic?.Invoke();
                onGradeChange?.Invoke(dataID, (float)rate.Value);
            }
            CanRateChange = false;
        }

        private void OpenVideoPath(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out long dataID) &&
                dataID > 0) {
                Video video = videoMapper.SelectVideoByID(dataID);

                if (video == null || !File.Exists(video.Path))
                    MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_FileNotExist") + ": " + video.Path);
                else
                    FileHelper.TryOpenSelectPath(video.Path);
            }
        }

        private void ShowDetail(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                long.TryParse(button.Tag.ToString(), out long id)) {
                RaiseEvent(new VideoItemEventArgs(id, OnItemClickEvent, sender));
            }
        }



        private void viewVideo_OnItemChange(object sender, ObjectEventArgs e)
        {
            if (sender is ViewVideo viewVideo &&
               GetDataID(viewVideo) is long dataID && dataID > 0 &&
               e.Data is float grade) {
                onGradeChange?.Invoke(dataID, grade);
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = tableData.SelectedIndex;
            if (idx < 0 || idx >= vieModel.CurrentVideoList.Count)
                return;
            Video video = vieModel.CurrentVideoList[idx];
            if (video == null)
                return;
            RaiseEvent(new VideoItemEventArgs(video.DataID, OnItemClickEvent, sender));
        }

    }
}
