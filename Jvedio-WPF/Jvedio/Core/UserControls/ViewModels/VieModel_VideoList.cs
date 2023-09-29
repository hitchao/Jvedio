using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UserControls.ViewModels
{
    class VieModel_VideoList : ViewModelBase
    {
        private const int SEARCH_CANDIDATE_MAX_COUNT = 10;

        #region "事件"

        //public Action<bool> onScroll;
        public Action<long> onPageChange;

        public static Action<bool> onSearchingChange;

        public event Action PageChangedStarted;

        public event EventHandler PageChangedCompleted;

        public event EventHandler RenderSqlChanged;

        private delegate void LoadVideoDelegate(Video video, int idx);


        private delegate void LoadViewAssoVideoDelegate(Video video, int idx);

        private void LoadViewAssoVideo(Video video, int idx) => ViewAssociationDatas.Add(video);

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        #endregion

        #region "静态属性"

        public static List<string> SortDict { get; set; } = new List<string>()
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
            "metadata.Size",
            "metadata.ViewCount",
            "metadata.ViewDate",
            "metadata.CreateDate",
            "metadata.UpdateDate",
            "metadata_video.SubSection",
            "metadata_video.ImageUrls",
            "metadata.ReleaseDate",
            "metadata.LastScanDate",
            "metadata_video.Director",
            "metadata_video.Studio",
            "metadata_video.Duration",
            "metadata_video.WebUrl",
            "metadata_video.WebType",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",
        };


        #endregion

        #region "属性"
        public Queue<int> PageQueue { get; set; } = new Queue<int>();


        /// <summary>
        /// 过滤器传进的
        /// </summary>
        public SelectWrapper<Video> FilterWrapper { get; set; }

        /// <summary>
        /// 过滤器传进的 SQL
        /// </summary>
        public string FilterSQL { get; set; }

        /// <summary>
        /// 侧边栏点击进入的
        /// </summary>
        public SelectWrapper<Video> ExtraWrapper { get; set; }

        /// <summary>
        /// 搜索
        /// </summary>
        public SelectWrapper<Video> SearchWrapper { get; set; }

        private List<Video> _SelectedVideo { get; set; } = new List<Video>();

        public CancellationTokenSource RenderVideoCTS { get; set; }

        public CancellationToken RenderVideoCT { get; set; }

        public string ClickFilterType { get; set; }



        private int _SearchSelectedIndex;

        public int SearchSelectedIndex {
            get { return _SearchSelectedIndex; }

            set {
                _SearchSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private bool _Nothing;

        public bool Nothing {
            get { return _Nothing; }

            set {
                _Nothing = value;
                RaisePropertyChanged();
            }
        }
        private bool _ShowTable = true;

        public bool ShowTable {
            get { return _ShowTable; }

            set {
                _ShowTable = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowAsso = true;

        public bool ShowAsso {
            get { return _ShowAsso; }

            set {
                _ShowAsso = value;
                RaisePropertyChanged();
            }
        }

        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();
            }
        }


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

        private int _SortType = (int)ConfigManager.VideoConfig.SortType;

        public int SortType {
            get { return _SortType; }

            set {
                _SortType = value;
                RaisePropertyChanged();
                ConfigManager.VideoConfig.SortType = value;
            }
        }
        private bool _SortDescending = ConfigManager.VideoConfig.SortDescending;

        public bool SortDescending {
            get { return _SortDescending; }

            set {
                _SortDescending = value;
                RaisePropertyChanged();
                ConfigManager.VideoConfig.SortDescending = value;
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
                onSearchingChange?.Invoke(value);
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
        private bool _ShowActorToggle;

        public bool ShowActorToggle {
            get { return _ShowActorToggle; }

            set {
                _ShowActorToggle = value;
                RaisePropertyChanged();
            }
        }


        private bool _EnableEditActress = false;

        public bool EnableEditActress {
            get { return _EnableEditActress; }

            set {
                _EnableEditActress = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentCount = 0;

        public int CurrentCount {
            get { return _CurrentCount; }

            set {
                _CurrentCount = value;
                RaisePropertyChanged();
            }
        }


        private long _TotalCount = 0;

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


        private ObservableCollection<Video> _CurrentVideoList;

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
                RaisePropertyChanged();
            }
        }

        private int _PageSize = (int)ConfigManager.VideoConfig.PageSize;

        public int PageSize {
            get { return _PageSize; }

            set {
                _PageSize = value;
                RaisePropertyChanged();
                ConfigManager.VideoConfig.PageSize = value;
            }
        }

        private ObservableCollection<Video> _ViewAssociationDatas;

        public ObservableCollection<Video> ViewAssociationDatas {
            get { return _ViewAssociationDatas; }

            set {
                _ViewAssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        // 影片关联
        private ObservableCollection<Video> _AssociationDatas;

        public ObservableCollection<Video> AssociationDatas {
            get { return _AssociationDatas; }

            set {
                _AssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "筛选"



        private bool _ShowFilter = ConfigManager.VideoConfig.ShowFilter;

        public bool ShowFilter {
            get { return _ShowFilter; }

            set {
                _ShowFilter = value;
                RaisePropertyChanged();
                ConfigManager.VideoConfig.ShowFilter = value;
            }
        }

        #endregion


        public VieModel_VideoList()
        {
            RefreshVideoRenderToken();
            Init();
        }

        public override void Init()
        {
            GlobalImageHeight = ViewVideo.GetImageHeight(ShowImageMode, GlobalImageWidth);
        }

        public void LoadData()
        {
            Select();
        }

        public void RandomDisplay()
        {
            Select(true);
        }

        public void Refresh() => Select();


        private void LoadVideo(Video video, int idx)
        {
            if (RenderVideoCT.IsCancellationRequested)
                return;
            if (CurrentVideoList.Count < PageSize) {
                if (idx < CurrentVideoList.Count) {
                    LoadVideo(idx, video);
                } else {
                    CurrentVideoList.Add(video);
                }
            } else {
                if (idx < CurrentVideoList.Count) {
                    LoadVideo(idx, video);
                }
            }
        }

        private void LoadVideo(int idx, Video video)
        {
            if (CurrentVideoList[idx].DataID == video.DataID) {
                // 不知为啥，如果 2 个对象相等，则不会触发 notify
                Video temp = CurrentVideoList[idx];
                RefreshData(ref temp, video);
            } else {
                CurrentVideoList[idx] = video;
            }
        }

        public void RefreshVideoRenderToken()
        {
            RenderVideoCTS = new CancellationTokenSource();
            RenderVideoCTS.Token.Register(() => { Logger.Warn("cancel load video page task"); });
            RenderVideoCT = RenderVideoCTS.Token;
        }



        public SelectWrapper<Video> GetSearchWrapper(SearchField searchType)
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
                default:
                    wrapper.Like(searchType.ToString(), searchContent);
                    break;
            }

            return wrapper;
        }


        public bool Query(SearchField searchType = SearchField.VID)
        {
            SearchWrapper = GetSearchWrapper(searchType);
            Select();
            return true;
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

            // 判断当前获取的队列
            while (PageQueue.Count > 1) {
                int page = PageQueue.Dequeue();
                Logger.Info($"skip page: {page}");
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (Rendering) {
                RenderVideoCTS?.Cancel();
                await Task.Delay(100);
            }

            SelectWrapper<Video> wrapper = Video.InitWrapper();

            SetSortOrder(wrapper, random);

            ToLimit(wrapper);
            wrapper.Select(SelectFields);

            string sql = VideoMapper.SQL_BASE;


            if (ExtraWrapper != null) {
                wrapper.Join(ExtraWrapper);
                if (!string.IsNullOrEmpty(ExtraWrapper.ExtraSql))
                    sql += ExtraWrapper.ExtraSql;
            }

            if (SearchWrapper != null) {
                wrapper.Join(SearchWrapper);
                if (!string.IsNullOrEmpty(SearchWrapper.ExtraSql))
                    sql += SearchWrapper.ExtraSql;
            }

            if (FilterWrapper != null) {
                wrapper.Join(FilterWrapper);
                if (!string.IsNullOrEmpty(FilterSQL))
                    sql += FilterSQL;
            }

            // todo 如果搜索框选中了标签，搜索出来的结果不一致
            SearchField searchType = (SearchField)SearchSelectedIndex;
            if (Searching) {
                if (searchType == SearchField.ActorName)
                    sql += VideoMapper.SQL_JOIN_ACTOR;
                else if (searchType == SearchField.LabelName)
                    sql += VideoMapper.SQL_JOIN_LABEL;
            } else if (!string.IsNullOrEmpty(ClickFilterType)) {
                if (ClickFilterType == "Label") {
                    sql += VideoMapper.SQL_JOIN_LABEL;
                } else if (ClickFilterType == "Actor") {
                    sql += VideoMapper.SQL_JOIN_ACTOR;
                } else {
                }
            }

            string count_sql = "select count(DISTINCT metadata.DataID) " + sql + wrapper.ToWhere(false);
            TotalCount = metaDataMapper.SelectCount(count_sql);

            WrapperEventArg<Video> arg = new WrapperEventArg<Video>();
            arg.Wrapper = wrapper;
            arg.SQL = sql;
            RenderSqlChanged?.Invoke(null, arg);

            sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false) + wrapper.ToOrder() + wrapper.ToLimit();
            onPageChange?.Invoke(TotalCount);
            RenderCurrentVideo(sql);
        }

        public void SetSortOrder<T>(IWrapper<T> wrapper, bool random = false)
        {
            if (wrapper == null)
                return;
            int sortIndex = SortType;
            if (sortIndex < 0 || sortIndex >= VieModel_VideoList.SortDict.Count)
                sortIndex = 0;
            string sortField = VieModel_VideoList.SortDict[sortIndex];
            if (random)
                wrapper.Asc("RANDOM()");
            else {
                if (SortDescending)
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
            if (CurrentVideoList == null) {
                CurrentVideoList = new ObservableCollection<Video>();
                Nothing = true;
                CurrentVideoList.CollectionChanged += (s, e) => {
                    Nothing = CurrentVideoList.Count == 0;
                };
            }

            PageChangedStarted?.Invoke();

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
                Video.SetImage(ref video, ShowImageMode);
                Video.SetTagStamps(ref video); // 设置标签戳
                Video.SetTitleAndDate(ref video); // 设置标题和发行日期
                Video.SetAsso(ref video);

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

        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetSearchCandidate()
        {
            return await Task.Run(() => {
                SearchField searchType = (SearchField)SearchSelectedIndex;
                string field = searchType.ToString();

                List<string> result = new List<string>();
                if (string.IsNullOrEmpty(SearchText))
                    return result;
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                SetSortOrder(wrapper); // 按照当前排序
                wrapper.Eq("metadata.DBId", ConfigManager.Main.CurrentDBId).Eq("metadata.DataType", 0);
                SelectWrapper<Video> selectWrapper = GetSearchWrapper(searchType);
                if (selectWrapper != null)
                    wrapper.Join(selectWrapper);


                string sql = $"SELECT DISTINCT {field} FROM metadata_video " +
                            "JOIN metadata " +
                            "on metadata.DataID=metadata_video.DataID ";

                if (ExtraWrapper != null) {
                    wrapper.Join(ExtraWrapper);
                    if (!string.IsNullOrEmpty(ExtraWrapper.ExtraSql))
                        sql += ExtraWrapper.ExtraSql;
                }

                if (FilterWrapper != null) {
                    wrapper.Join(FilterWrapper);
                    if (!string.IsNullOrEmpty(FilterSQL))
                        sql += FilterSQL;
                }

                if (searchType == SearchField.ActorName)
                    sql += ActorMapper.SQL_JOIN_ACTOR;
                else if (searchType == SearchField.LabelName)
                    sql += VideoMapper.SQL_JOIN_LABEL;


                string condition_sql = wrapper.ToWhere(false) + wrapper.ToOrder()
                            + $" LIMIT 0,{SEARCH_CANDIDATE_MAX_COUNT}";

                if (searchType == SearchField.Genre) {
                    // 类别特殊处理
                    string genre_sql = $"SELECT {field} FROM metadata_video " +
                            "JOIN metadata " +
                            "on metadata.DataID=metadata_video.DataID ";
                    List<Dictionary<string, object>> list = metaDataMapper.Select(genre_sql);
                    if (list != null && list.Count > 0)
                        SetGenreCandidate(field, list, ref result);
                } else {
                    List<Dictionary<string, object>> list = metaDataMapper.Select(sql + condition_sql);
                    if (list != null && list.Count > 0) {
                        foreach (Dictionary<string, object> dict in list) {
                            if (!dict.ContainsKey(field))
                                continue;
                            string value = dict[field].ToString();
                            if (string.IsNullOrEmpty(value))
                                continue;
                            result.Add(value);
                        }
                    }
                }

                return result;
            });
        }

        public void RefreshData(long dataID)
        {
            if (CurrentVideoList == null || CurrentVideoList.Count == 0)
                return;
            int idx = -1;
            for (int i = 0; i < VideoList.Count; i++) {
                if (VideoList[i].DataID == dataID) {
                    idx = i;
                    break;
                }
            }
            if (idx < 0 || idx >= CurrentVideoList.Count)
                return;
            Video video = Video.GetById(dataID);
            Video temp = VideoList[idx];
            RefreshData(ref temp, video);
            temp = CurrentVideoList[idx];
            RefreshData(ref temp, video);
        }

        public void RefreshTagStamp(long dataID)
        {
            for (int i = 0; i < VideoList.Count; i++) {
                if (VideoList[i].DataID == dataID) {
                    Video video = VideoList[i];
                    Video.SetTagStamps(ref video);
                    break;
                }
            }
            for (int i = 0; i < CurrentVideoList.Count; i++) {
                if (CurrentVideoList[i].DataID == dataID) {
                    Video video = CurrentVideoList[i];
                    Video.SetTagStamps(ref video);
                    break;
                }
            }
        }

        private void RefreshData(ref Video origin, Video target)
        {
            System.Reflection.PropertyInfo[] propertyInfos = target.GetType().GetProperties();
            foreach (var item in propertyInfos) {
                object v = item.GetValue(target);
                if (v != null) {
                    item.SetValue(origin, v);
                }
            }
        }

        private void SetGenreCandidate(string field, List<Dictionary<string, object>> list, ref List<string> result)
        {
            string search = SearchText.ToProperSql().ToLower();
            HashSet<string> set = new HashSet<string>();
            foreach (Dictionary<string, object> dict in list) {
                if (!dict.ContainsKey(field))
                    continue;
                string value = dict[field].ToString();
                if (string.IsNullOrEmpty(value))
                    continue;
                string[] arr = value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0) {
                    foreach (var item in arr) {
                        if (string.IsNullOrEmpty(item))
                            continue;
                        set.Add(item);
                    }
                }
            }

            result = set.Where(arg => arg.ToLower().IndexOf(search) >= 0).ToList()
                .Take(SEARCH_CANDIDATE_MAX_COUNT).ToList();
        }

    }
}
