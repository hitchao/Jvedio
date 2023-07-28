using Jvedio.Core.Enums;
using Jvedio.Entity;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Media;
using SuperUtils.WPF.VieModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.WindowHelper;
using static Jvedio.App;
using Jvedio.Pages;
using Jvedio.Core.Media;
using Jvedio.Entity.Common;
using Jvedio.ViewModels;
using SuperControls.Style;
using System;
using Jvedio.Mapper;
using SuperUtils.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Entity.CommonSQL;
using SuperUtils.Time;
using System.Windows.Controls;
using System.Web.UI;
using System.Windows.Media;
using Google.Protobuf.WellKnownTypes;

namespace Jvedio.Core.UserControls.ViewModels
{
    class VieModel_VideoList : ViewModelBase
    {


        public Action<bool> onScroll;
        public Action<long> onPageChange;


        public SelectWrapper<Video> ExtraWrapper { get; set; }

        public static List<string> SortDict = new List<string>()
        {
            "metadata_video.VID",
            "metadata.Grade",
            "metadata.Size",
            "metadata.LastScanDate",
            "metadata.FirstScanDate",
            "metadata.Title",
            "metadata.ViewCount",
            "metadata.ReleaseDate",
            "metadata.Rating",
            "metadata_video.Duration",
        };

        public static string[] SelectFields =
        {
            "DISTINCT metadata.DataID",
            "MVID",
            "VID",
            "metadata.Grade",
            "metadata.Title",
            "metadata.Path",
            "metadata.Hash",
            "metadata_video.SubSection",
            "metadata_video.ImageUrls",
            "metadata.ReleaseDate",
            "metadata.LastScanDate",
            "metadata_video.WebUrl",
            "metadata_video.WebType",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",
        };
        public static Queue<int> PageQueue { get; set; } = new Queue<int>();



        private List<Video> _SelectedVideo = new List<Video>();

        public CancellationTokenSource RenderVideoCTS { get; set; }

        public CancellationToken RenderVideoCT { get; set; }

        public string ClickFilterType { get; set; }



        public event EventHandler PageChangedCompleted;

        public event EventHandler RenderSqlChanged;

        public VieModel_VideoList()
        {
            RefreshVideoRenderToken();
            GlobalImageHeight = ViewVideo.GetImageHeight(ShowImageMode, GlobalImageWidth);
        }

        #region "属性"


        private string _UUID;

        public string UUID {
            get { return _UUID; }

            set {
                _UUID = value;
                RaisePropertyChanged();
            }
        }


        private int _GlobalImageHeight;

        public int GlobalImageHeight {
            get { return _GlobalImageHeight; }

            set {
                _GlobalImageHeight = value;
                RaisePropertyChanged();
            }
        }


        private int _GlobalImageWidth = (int)ConfigManager.VideoConfig.GlobalImageWidth;

        public int GlobalImageWidth {
            get { return _GlobalImageWidth; }

            set {
                _GlobalImageWidth = value;
                GlobalImageHeight = ViewVideo.GetImageHeight(ShowImageMode, value);
                ConfigManager.VideoConfig.GlobalImageWidth = value;

                RaisePropertyChanged();
            }
        }

        private int _ShowImageMode = (int)ConfigManager.VideoConfig.ImageMode;

        public int ShowImageMode {
            get { return _ShowImageMode; }

            set {
                _ShowImageMode = value;
                RaisePropertyChanged();
                ConfigManager.VideoConfig.ImageMode = value;
            }
        }

        private int _SortType;

        public int SortType {
            get { return _SortType; }

            set {
                _SortType = value;
                RaisePropertyChanged();
                Properties.Settings.Default.SortType = value.ToString();
            }
        }

        private bool _EditMode;

        public bool EditMode {
            get { return _EditMode; }

            set {
                _EditMode = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<TagStamp> _TagStamps = new ObservableCollection<TagStamp>();

        public ObservableCollection<TagStamp> TagStamps {
            get { return _TagStamps; }

            set {
                _TagStamps = value;
                RaisePropertyChanged();
            }
        }

        private int _RenderProgress;

        public int RenderProgress {
            get { return _RenderProgress; }

            set {
                _RenderProgress = value;
                RaisePropertyChanged();
            }
        }


        private bool _rendering;
        public bool Rendering {
            get { return _rendering; }
            set {
                _rendering = value;
                RaisePropertyChanged();
            }
        }

        private bool _Searching = false;

        public bool Searching {
            get { return _Searching; }

            set {
                _Searching = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowActorGrid;

        public bool ShowActorGrid {
            get { return _ShowActorGrid; }

            set {
                _ShowActorGrid = value;
                RaisePropertyChanged();
            }
        }


        public bool _EnableEditActress = false;

        public bool EnableEditActress {
            get { return _EnableEditActress; }

            set {
                _EnableEditActress = value;
                RaisePropertyChanged();
            }
        }

        public int _CurrentCount = 0;

        public int CurrentCount {
            get { return _CurrentCount; }

            set {
                _CurrentCount = value;
                RaisePropertyChanged();
            }
        }


        public long _TotalCount = 0;

        public long TotalCount {
            get { return _TotalCount; }

            set {
                _TotalCount = value;
                RaisePropertyChanged();
            }
        }

        private int _TotalPage = 1;

        public int TotalPage {
            get { return _TotalPage; }

            set {
                _TotalPage = value;
                RaisePropertyChanged();
            }
        }



        private List<Video> _VideoList;

        public List<Video> VideoList {
            get { return _VideoList; }

            set {
                _VideoList = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Video> _CurrentVideoList = new ObservableCollection<Video>();

        public ObservableCollection<Video> CurrentVideoList {
            get { return _CurrentVideoList; }

            set {
                _CurrentVideoList = value;
                RaisePropertyChanged();
            }
        }


        public List<Video> SelectedVideo {
            get { return _SelectedVideo; }

            set {
                _SelectedVideo = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentPage = 1;

        public int CurrentPage {
            get { return _CurrentPage; }

            set {
                _CurrentPage = value;

                // FlowNum = 0;
                RaisePropertyChanged();
            }
        }

        private int _PageSize = Properties.Settings.Default.PageSize;

        public int PageSize {
            get { return _PageSize; }

            set {
                _PageSize = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Video> _ViewAssociationDatas;

        public ObservableCollection<Video> ViewAssociationDatas {
            get { return _ViewAssociationDatas; }

            set {
                _ViewAssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        // 影片关联
        public ObservableCollection<Video> _AssociationDatas;

        public ObservableCollection<Video> AssociationDatas {
            get { return _AssociationDatas; }

            set {
                _AssociationDatas = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region "右键筛选"

        private int _DataExistIndex = 0;

        public int DataExistIndex {
            get { return _DataExistIndex; }

            set {
                if (value < 0 || value > 2)
                    _DataExistIndex = 0;
                else
                    _DataExistIndex = value;
                RaisePropertyChanged();
            }
        }




        private int _PictureTypeIndex = 0;

        public int PictureTypeIndex {
            get { return _PictureTypeIndex; }

            set {
                if (value < 0 || value > 2)
                    _PictureTypeIndex = 0;
                else
                    _PictureTypeIndex = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        public void LoadData()
        {
            Select();
        }




        public void RandomDisplay()
        {
            Select(true);
            //TabItemManager.Add(TabType.GeoRandom, LangManager.GetValueByKey("ToolTip_RandomShow"));
        }

        public void Reset() => Select();



        private delegate void LoadVideoDelegate(Video video, int idx);

        private void LoadVideo(Video video, int idx)
        {
            if (RenderVideoCT.IsCancellationRequested)
                return;
            if (CurrentVideoList.Count < PageSize) {
                if (idx < CurrentVideoList.Count) {
                    CurrentVideoList[idx] = null;
                    CurrentVideoList[idx] = video;
                } else {
                    CurrentVideoList.Add(video);
                }
            } else {
                if (idx < CurrentVideoList.Count) {
                    CurrentVideoList[idx] = null;
                    CurrentVideoList[idx] = video;
                }
            }
        }

        public void RefreshVideoRenderToken()
        {
            RenderVideoCTS = new CancellationTokenSource();
            RenderVideoCTS.Token.Register(() => { Logger.Warn("cancel load video page task"); });
            RenderVideoCT = RenderVideoCTS.Token;
        }


        private delegate void LoadViewAssoVideoDelegate(Video video, int idx);

        private void LoadViewAssoVideo(Video video, int idx) => ViewAssociationDatas.Add(video);

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);


        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();

                // BeginSearch();
            }
        }

        public SelectWrapper<Video> GetWrapper(SearchField searchType)
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            if (string.IsNullOrEmpty(SearchText))
                return null;
            string formatSearch = SearchText.ToProperSql().Trim();
            if (string.IsNullOrEmpty(formatSearch))
                return null;
            string searchContent = formatSearch;

            switch (searchType) {
                case SearchField.VID:

                    string vid = JvedioLib.Security.Identify.GetVID(formatSearch);
                    if (string.IsNullOrEmpty(vid))
                        searchContent = formatSearch;
                    else
                        searchContent = vid;
                    wrapper.Like("VID", searchContent);
                    break;

                // case SearchType.Title:
                //    wrapper.Like("Title", searchContent).LeftBracket().Or().Like("Path", searchContent).RightBracket();
                // break;
                default:
                    wrapper.Like(searchType.ToString(), searchContent);
                    break;
            }

            return wrapper;
        }


        public async Task<bool> Query(SearchField searchType = SearchField.VID)
        {
            ExtraWrapper = GetWrapper(searchType);
            Select();
            return true;
        }

        public void LoadViewAssoData(long dataID)
        {
            if (ViewAssociationDatas == null)
                ViewAssociationDatas = new ObservableCollection<Video>();
            ViewAssociationDatas.Clear();
            GC.Collect();
            Video currentVideo = CurrentVideoList.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (currentVideo.AssociationList == null || currentVideo.AssociationList.Count <= 0)
                return;
            SelectWrapper<Video> wrapper = Video.InitWrapper();
            wrapper.In("metadata.DataID", currentVideo.AssociationList.Select(arg => arg.ToString()));
            wrapper.Select(SelectFields);

            string sql = VideoMapper.BASE_SQL;

            sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false);

            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            List<Video> videos = metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

            if (videos == null)
                return;

            for (int i = 0; i < videos.Count; i++) {
                Video video = videos[i];
                if (video == null)
                    continue;
                BitmapImage smallimage = ImageCache.Get(video.GetSmallImage(), Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
                BitmapImage bigimage = ImageCache.Get(video.GetBigImage(), Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
                if (smallimage == null)
                    smallimage = MetaData.DefaultSmallImage;
                if (bigimage == null)
                    bigimage = smallimage;
                video.BigImage = bigimage;
                Video.SetTagStamps(ref video); // 设置标签戳
                Video.HandleEmpty(ref video); // 设置标题和发行日期

                if (ConfigManager.Settings.AutoGenScreenShot) {
                    string path = video.GetScreenShot();
                    if (Directory.Exists(path)) {
                        string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (array.Length > 0) {
                            Video.SetImage(ref video, array[array.Length / 2]);
                            video.BigImage = null;
                            video.BigImage = video.ViewImage;
                        }
                    }
                }

                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new LoadViewAssoVideoDelegate(LoadViewAssoVideo), video, i);
            }

            // 清除
            for (int i = ViewAssociationDatas.Count - 1; i > videos.Count - 1; i--) {
                ViewAssociationDatas.RemoveAt(i);
            }
        }

        public void ToLimit<T>(IWrapper<T> wrapper)
        {
            int row_count = PageSize;
            long offset = PageSize * (CurrentPage - 1);
            wrapper.Limit(offset, row_count);
        }




        public async void Select(bool random = false)
        {
            Logger.Info("0.Select");
            //TabSelectedIndex = 0; // 影片

            // 判断当前获取的队列
            while (PageQueue.Count > 1) {
                int page = PageQueue.Dequeue();
                Logger.Info($"skip page: {page}");
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (Rendering) {
                RenderVideoCTS?.Cancel(); // 取消加载
                await Task.Delay(100);
            }

            App.Current.Dispatcher.Invoke((Action)delegate {
                onScroll?.Invoke(true);
                //ScrollViewer scrollViewer =
                //    MainWindow.FindVisualChild<ScrollViewer>(MainWindow.MovieItemsControl);
                //scrollViewer?.ScrollToTop(); // 滚到顶部
            });

            SelectWrapper<Video> wrapper = Video.InitWrapper();

            SetSortOrder(wrapper, random);

            ToLimit(wrapper);
            wrapper.Select(SelectFields);
            if (ExtraWrapper != null)
                wrapper.Join(ExtraWrapper);

            string sql = VideoMapper.BASE_SQL;

            // todo 如果搜索框选中了标签，搜索出来的结果不一致
            SearchField searchType = (SearchField)ConfigManager.Main.SearchSelectedIndex;
            if (Searching) {
                if (searchType == SearchField.ActorName)
                    sql += VideoMapper.ACTOR_JOIN_SQL;
                else if (searchType == SearchField.LabelName)
                    sql += VideoMapper.LABEL_JOIN_SQL;
            } else if (!string.IsNullOrEmpty(ClickFilterType)) {
                if (ClickFilterType == "Label") {
                    sql += VideoMapper.LABEL_JOIN_SQL;
                } else if (ClickFilterType == "Actor") {
                    sql += VideoMapper.ACTOR_JOIN_SQL;
                } else {
                }
            }

            // 标记
            bool allFalse = TagStamps.All(item => item.Selected == false);
            if (allFalse) {
                wrapper.IsNull("TagID");
                sql += VideoMapper.TAGSTAMP_LEFT_JOIN_SQL;
            } else {
                bool allTrue = TagStamps.All(item => item.Selected == true);
                if (!allTrue) {
                    wrapper.In("metadata_to_tagstamp.TagID", TagStamps.Where(item => item.Selected == true).Select(item => item.TagID.ToString()));
                    sql += VideoMapper.TAGSTAMP_JOIN_SQL;
                }
            }

            // 右侧菜单的一些筛选项

            // 1. 仅显示分段视频
            if (Properties.Settings.Default.OnlyShowSubSection)
                wrapper.NotEq("SubSection", string.Empty);

            // 2. 视频类型
            //List<MenuItem> allMenus = MainWindow.VideoTypeMenuItem.Items.OfType<MenuItem>().ToList();
            //List<MenuItem> checkedMenus = new List<MenuItem>();

            //App.Current.Dispatcher.Invoke(() => {
            //    checkedMenus = allMenus.Where(t => t.IsChecked).ToList();
            //});

            //if (checkedMenus.Count > 0 && checkedMenus.Count < 4) {
            //    // VideoType = 0 or VideoType = 1 or VideoType=2
            //    if (checkedMenus.Count == 1) {
            //        int idx = allMenus.IndexOf(checkedMenus[0]);
            //        wrapper.Eq("VideoType", idx);
            //    } else if (checkedMenus.Count == 2) {
            //        int idx1 = allMenus.IndexOf(checkedMenus[0]);
            //        int idx2 = allMenus.IndexOf(checkedMenus[1]);
            //        wrapper.Eq("VideoType", idx1).LeftBracket().Or().Eq("VideoType", idx2).RightBracket();
            //    } else if (checkedMenus.Count == 3) {
            //        int idx1 = allMenus.IndexOf(checkedMenus[0]);
            //        int idx2 = allMenus.IndexOf(checkedMenus[1]);
            //        int idx3 = allMenus.IndexOf(checkedMenus[2]);
            //        wrapper.Eq("VideoType", idx1).LeftBracket().Or().Eq("VideoType", idx2).Or().Eq("VideoType", idx3).RightBracket();
            //    }
            //}

            // 图片显示模式
            if (ConfigManager.Settings.PictureIndexCreated && PictureTypeIndex > 0) {
                sql += VideoMapper.COMMON_PICTURE_EXIST_JOIN_SQL;
                long pathType = ConfigManager.Settings.PicPathMode;
                int imageType = ShowImageMode;
                if (imageType > 1)
                    imageType = 0;
                wrapper.Eq("common_picture_exist.PathType", pathType).Eq("common_picture_exist.ImageType", imageType).Eq("common_picture_exist.Exist", PictureTypeIndex - 1);
            }

            // 是否可播放
            if (ConfigManager.Settings.PlayableIndexCreated && DataExistIndex > 0)
                wrapper.Eq("metadata.PathExist", DataExistIndex - 1);

            string count_sql = "select count(DISTINCT metadata.DataID) " + sql + wrapper.ToWhere(false);
            TotalCount = metaDataMapper.SelectCount(count_sql);

            WrapperEventArg<Video> arg = new WrapperEventArg<Video>();
            arg.Wrapper = wrapper;
            arg.SQL = sql;
            RenderSqlChanged?.Invoke(null, arg);

            sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false) + wrapper.ToOrder() + wrapper.ToLimit();

            // 只能手动设置页码，很奇怪
            onPageChange?.Invoke(TotalCount);
            //App.Current.Dispatcher.Invoke(() => { MainWindow.pagination.Total = TotalCount; });
            RenderCurrentVideo(sql);
        }

        public void SetSortOrder<T>(IWrapper<T> wrapper, bool random = false)
        {
            if (wrapper == null)
                return;
            int.TryParse(Properties.Settings.Default.SortType, out int sortIndex);
            if (sortIndex < 0 || sortIndex >= VieModel_VideoList.SortDict.Count)
                sortIndex = 0;
            string sortField = VieModel_VideoList.SortDict[sortIndex];
            if (random)
                wrapper.Asc("RANDOM()");
            else {
                if (Properties.Settings.Default.SortDescending)
                    wrapper.Desc(sortField);
                else
                    wrapper.Asc(sortField);
            }
        }



        public void RenderCurrentVideo(string sql)
        {
            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            List<Video> videos = metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

            VideoList = new List<Video>();
            if (videos == null)
                videos = new List<Video>();
            VideoList.AddRange(videos);
            CurrentCount = VideoList.Count;
            Render();
        }

        public async void Render()
        {
            Logger.Info("1.Render");
            if (CurrentVideoList == null)
                CurrentVideoList = new ObservableCollection<Video>();
            int imageMode = ShowImageMode;
            for (int i = 0; i < VideoList.Count; i++) {
                try {
                    RenderVideoCT.ThrowIfCancellationRequested();
                } catch (OperationCanceledException) {
                    RenderVideoCTS?.Dispose();
                    break;
                }

                Rendering = true;
                Video video = VideoList[i];
                if (video == null)
                    continue;
                Video.SetImage(ref video, imageMode);
                Video.SetTagStamps(ref video); // 设置标签戳
                Video.HandleEmpty(ref video); // 设置标题和发行日期

                // 设置关联
                HashSet<long> set = associationMapper.GetAssociationDatas(video.DataID);
                if (set != null) {
                    video.HasAssociation = set.Count > 0;
                    video.AssociationList = set.ToList();
                }

                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadVideoDelegate(LoadVideo), video, i);
                RenderProgress = (int)(100 * (i + 1) / (float)VideoList.Count);
            }

            // 清除
            for (int i = CurrentVideoList.Count - 1; i > VideoList.Count - 1; i--) {
                CurrentVideoList.RemoveAt(i);
            }

            if (RenderVideoCT.IsCancellationRequested)
                RefreshVideoRenderToken();
            Rendering = false;
            PageChangedCompleted?.Invoke(this, null);
        }


    }
}
