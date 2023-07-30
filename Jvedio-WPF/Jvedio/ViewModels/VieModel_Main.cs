using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Net;
using Jvedio.Core.Scan;
using Jvedio.Core.Server;
using Jvedio.Core.UserControls.ViewModels;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using Jvedio.ViewModels;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;
using static Jvedio.Window_Server;

namespace Jvedio.ViewModel
{

    public class VieModel_Main : ViewModelBase
    {

        public const int RECENT_DAY = 3;
        public const int SEARCH_CANDIDATE_MAX_COUNT = 3;

        #region "事件"

        public event EventHandler RenderSqlChanged;

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        #endregion

        #region "静态属性"
        /// <summary>
        /// flag, 只显示一次提示
        /// </summary>
        private static bool ShowRelativeScreen { get; set; } = false;
        public static string LabelJoinSql { get; set; } =
         " join metadata_to_label on metadata_to_label.DataID=metadata.DataID ";


        public static Dictionary<string, string> SELECT_TYPE = new Dictionary<string, string>()
        {
            { "All", "  " },
            { "Favorite", "  " },
            { "RecentWatch", "  " },
        };


        private static Main MainWindow { get; set; }

        #endregion

        #region "属性"



        public string ClickFilterType { get; set; }

        public TabItemManager TabItemManager { get; set; }

        public AppDatabase CurrentAppDataBase { get; set; }


        #endregion

        #region "RelayCommand"
        public RelayCommand<object> SideButtonCmd { get; set; }

        public RelayCommand<object> ShowActorsCommand { get; set; }

        public RelayCommand<object> ShowLabelsCommand { get; set; }

        public RelayCommand<object> ShowClassifyCommand { get; set; }

        public RelayCommand<object> AddNewMovie { get; set; }
        #endregion


        public VieModel_Main(Main main)
        {
            MainWindow = main;
            Init();
        }


        public override void Init()
        {
            ClickFilterType = string.Empty;
            InitCmd();
            InitBinding();
            InitTabData();
            Logger.Info("init view model main ok");
        }


        public void InitTabData()
        {
            TabItemManager = TabItemManager.CreateInstance(this, MainWindow.TabDataPanel);
        }

        private void InitCmd()
        {
            SideButtonCmd = new RelayCommand<object>(t => HandleSideButtonCmd(t));
            ShowActorsCommand = new RelayCommand<object>(t => ShowAllActors(t));
            ShowLabelsCommand = new RelayCommand<object>(t => ShowAllLabels(t));
            ShowClassifyCommand = new RelayCommand<object>(t => ShowClassify(t));
            AddNewMovie = new RelayCommand<object>(t => AddSingleMovie());
        }

        public void HandleSideButtonCmd(object o)
        {
            if (o == null || string.IsNullOrEmpty(o.ToString()))
                return;

            string param = o.ToString();

            if (!ShowRelativeScreen &&
             (PathType)ConfigManager.Settings.PicPathMode == PathType.RelativeToData) {
                ShowRelativeScreen = true;
                MessageCard.Info("当前图片模式为相对于影片，如果影片都位于同一目录，图片将会重复/覆盖");
            }

            SelectWrapper<Video> ExtraWrapper = new SelectWrapper<Video>();

            switch (param) {
                case "All":
                    TabItemManager.Add(TabType.GeoVideo, LangManager.GetValueByKey("AllVideo"), ExtraWrapper);
                    break;
                case "Favorite":
                    ExtraWrapper.Gt("metadata.Grade", 0);
                    TabItemManager.Add(TabType.GeoStar, LangManager.GetValueByKey("Favorites"), ExtraWrapper);
                    break;
                case "RecentWatch":
                    DateTime date1 = DateTime.Now.AddDays(-1 * RECENT_DAY);
                    DateTime date2 = DateTime.Now;
                    ExtraWrapper.Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2));
                    TabItemManager.Add(TabType.GeoRecentPlay, LangManager.GetValueByKey("RecentPlay"), ExtraWrapper);
                    break;
                default:
                    break;
            }


            //MainWindow.pagination.CurrentPage = 1;
            //ClickFilterType = string.Empty;
            //ShowActorGrid = false;
        }


        public void InitBinding()
        {
            // todo tab
            //CurrentVideoList.CollectionChanged += (s, e) => {
            //    if (CurrentVideoList != null && CurrentVideoList.Count > 0)
            //        ShowSoft = false;
            //    else
            //        ShowSoft = true;
            //};
        }


        #region "TabItem"
        public ObservableCollection<TabItemEx> _TabItems;
        public ObservableCollection<TabItemEx> TabItems {
            get { return _TabItems; }

            set {
                _TabItems = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region "界面显示属性"

        public ServerStatus _ServerStatus;

        public ServerStatus ServerStatus {
            get { return _ServerStatus; }

            set {
                _ServerStatus = value;
                RaisePropertyChanged();
            }
        }


        private bool _MainDataChecked;

        public bool MainDataChecked {
            get { return _MainDataChecked; }

            set {
                _MainDataChecked = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _ShowFirstRun = Visibility.Collapsed;

        public Visibility ShowFirstRun {
            get { return _ShowFirstRun; }

            set {
                _ShowFirstRun = value;
                RaisePropertyChanged();
            }
        }



        private Visibility _ActorProgressBarVisibility = Visibility.Collapsed;

        public Visibility ActorProgressBarVisibility {
            get { return _ActorProgressBarVisibility; }

            set {
                _ActorProgressBarVisibility = value;
                RaisePropertyChanged();
            }
        }

        private int _SearchSelectedIndex = (int)ConfigManager.Main.SearchSelectedIndex;

        public int SearchSelectedIndex {
            get { return _SearchSelectedIndex; }

            set {
                _SearchSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private int _ClassifySelectedIndex = (int)ConfigManager.Main.ClassifySelectedIndex;

        public int ClassifySelectedIndex {
            get { return _ClassifySelectedIndex; }

            set {
                _ClassifySelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private double _SideGridWidth = ConfigManager.Main.SideGridWidth;

        public double SideGridWidth {
            get { return _SideGridWidth; }

            set {
                _SideGridWidth = value;
                RaisePropertyChanged();
            }
        }

        private int _TabSelectedIndex = 0;

        public int TabSelectedIndex {
            get { return _TabSelectedIndex; }

            set {
                _TabSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsLoadingMovie = true;

        public bool IsLoadingMovie {
            get { return _IsLoadingMovie; }

            set {
                _IsLoadingMovie = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsLoadingClassify = false;

        public bool IsLoadingClassify {
            get { return _IsLoadingClassify; }

            set {
                _IsLoadingClassify = value;
                RaisePropertyChanged();
            }
        }

        private Thickness _MainGridThickness = new Thickness(10);

        public Thickness MainGridThickness {
            get { return _MainGridThickness; }

            set {
                _MainGridThickness = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "ObservableCollection"

        private ObservableCollection<ScanTask> _ScanTasks = new ObservableCollection<ScanTask>();

        public ObservableCollection<ScanTask> ScanTasks {
            get { return _ScanTasks; }

            set {
                _ScanTasks = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<DownLoadTask> _DownLoadTasks = new ObservableCollection<DownLoadTask>();

        public ObservableCollection<DownLoadTask> DownLoadTasks {
            get { return _DownLoadTasks; }

            set {
                _DownLoadTasks = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ScreenShotTask> _ScreenShotTasks = new ObservableCollection<ScreenShotTask>();

        public ObservableCollection<ScreenShotTask> ScreenShotTasks {
            get { return _ScreenShotTasks; }

            set {
                _ScreenShotTasks = value;
                RaisePropertyChanged();
            }
        }

        private double _DownLoadProgress = 0;

        public double DownLoadProgress {
            get { return _DownLoadProgress; }

            set {
                _DownLoadProgress = value;
                RaisePropertyChanged();
            }
        }

        private double _ScreenShotProgress = 0;

        public double ScreenShotProgress {
            get { return _ScreenShotProgress; }

            set {
                _ScreenShotProgress = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _ScreenShotVisibility = Visibility.Collapsed;

        public Visibility ScreenShotVisibility {
            get { return _ScreenShotVisibility; }

            set {
                _ScreenShotVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _DownLoadVisibility = Visibility.Collapsed;

        public Visibility DownLoadVisibility {
            get { return _DownLoadVisibility; }

            set {
                _DownLoadVisibility = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Message> _Message = new ObservableCollection<Message>();

        public ObservableCollection<Message> Message {
            get { return _Message; }

            set {
                _Message = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<AppDatabase> _DataBases = new ObservableCollection<AppDatabase>();

        public ObservableCollection<AppDatabase> DataBases {
            get { return _DataBases; }

            set {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentDbId;

        public int CurrentDbId {
            get { return _CurrentDbId; }

            set {
                _CurrentDbId = value;
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

        private ObservableCollection<string> _GenreList;

        public ObservableCollection<string> GenreList {
            get { return _GenreList; }

            set {
                _GenreList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> labellist;

        public ObservableCollection<string> LabelList {
            get { return labellist; }

            set {
                labellist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _SeriesList;

        public ObservableCollection<string> SeriesList {
            get { return _SeriesList; }

            set {
                _SeriesList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> studioList;

        public ObservableCollection<string> StudioList {
            get { return studioList; }

            set {
                studioList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> directorList
;

        public ObservableCollection<string> DirectorList {
            get { return directorList; }

            set {
                directorList = value;
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



        private ObservableCollection<string> _FilePathClassification;

        public ObservableCollection<string> FilePathClassification {
            get { return _FilePathClassification; }

            set {
                _FilePathClassification = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _SearchHistory;

        public ObservableCollection<string> SearchHistory {
            get { return _SearchHistory; }

            set {
                _SearchHistory = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region "Variable"



        private string _StatusText;

        public string StatusText {
            get { return _StatusText; }

            set {
                _StatusText = value;
                RaisePropertyChanged();
            }
        }


        private string _ScanStatus;

        public string ScanStatus {
            get { return _ScanStatus; }

            set {
                _ScanStatus = value;
                RaisePropertyChanged();
            }
        }

        private string _DownloadStatus;

        public string DownloadStatus {
            get { return _DownloadStatus; }

            set {
                _DownloadStatus = value;
                RaisePropertyChanged();
            }
        }

        private double _RecentWatchedCount = 0;

        public double RecentWatchedCount {
            get { return _RecentWatchedCount; }

            set {
                _RecentWatchedCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllVideoCount = 0;

        public long AllVideoCount {
            get { return _AllVideoCount; }

            set {
                _AllVideoCount = value;
                RaisePropertyChanged();
            }
        }

        private double _FavoriteVideoCount = 0;

        public double FavoriteVideoCount {
            get { return _FavoriteVideoCount; }

            set {
                _FavoriteVideoCount = value;
                RaisePropertyChanged();
            }
        }

        private long _RecentWatchCount = 0;

        public long RecentWatchCount {
            get { return _RecentWatchCount; }

            set {
                _RecentWatchCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllActorCount = 0;

        public long AllActorCount {
            get { return _AllActorCount; }

            set {
                _AllActorCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllLabelCount = 0;

        public long AllLabelCount {
            get { return _AllLabelCount; }

            set {
                _AllLabelCount = value;
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



        #endregion

        #region "筛选"

        private bool _IsRefresh = false;

        public bool IsRefresh {
            get { return _IsRefresh; }

            set {
                _IsRefresh = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Notice> _Notices;

        public ObservableCollection<Notice> Notices {
            get { return _Notices; }

            set {
                _Notices = value;
                RaisePropertyChanged();
            }
        }

        private bool _SideDefaultExpanded = ConfigManager.Main.SideDefaultExpanded;

        public bool SideDefaultExpanded {
            get { return _SideDefaultExpanded; }

            set {
                _SideDefaultExpanded = value;
                RaisePropertyChanged();
                ConfigManager.Main.SideDefaultExpanded = value;
                ConfigManager.Main.Save();
            }
        }
        private bool _SideTagStampExpanded = ConfigManager.Main.SideTagStampExpanded;

        public bool SideTagStampExpanded {
            get { return _SideTagStampExpanded; }

            set {
                _SideTagStampExpanded = value;
                RaisePropertyChanged();
                ConfigManager.Main.SideTagStampExpanded = value;
                ConfigManager.Main.Save();
            }
        }
        private bool _ShowSoft = true;

        public bool ShowSoft {
            get { return _ShowSoft; }

            set {
                _ShowSoft = value;
                RaisePropertyChanged();
            }
        }



        private int _DownloadLongTaskDelay = 0;

        public int DownloadLongTaskDelay {
            get { return _DownloadLongTaskDelay; }

            set {
                _DownloadLongTaskDelay = value;
                if (value > 0)
                    DisplayDownloadLongTaskDelay = Visibility.Visible;
                else
                    DisplayDownloadLongTaskDelay = Visibility.Hidden;
                RaisePropertyChanged();
            }
        }

        private Visibility _DisplayDownloadLongTaskDelay = Visibility.Hidden;

        public Visibility DisplayDownloadLongTaskDelay {
            get { return _DisplayDownloadLongTaskDelay; }

            set {
                _DisplayDownloadLongTaskDelay = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public void ShowAllActors(object o)
        {
            TabSelectedIndex = 1;
            // 新开一个 tab 页
            //SelectActor();
        }

        public void ShowAllLabels(object o)
        {
            TabSelectedIndex = 2;
            GetLabelList();
        }

        public void ShowClassify(object o)
        {
            TabSelectedIndex = 3;
        }

        public void InitCurrentTagStamps(List<TagStamp> beforeTagStamps = null)
        {
            List<Dictionary<string, object>> list = tagStampMapper.Select(TagStampMapper.GetTagSql());
            List<TagStamp> tagStamps = new List<TagStamp>();
            if (list != null && list.Count > 0) {
                tagStamps = tagStampMapper.ToEntity<TagStamp>(list, typeof(TagStamp).GetProperties(), false);
                if (beforeTagStamps != null && beforeTagStamps.Count > 0) {
                    foreach (var item in tagStamps) {
                        TagStamp tagStamp = beforeTagStamps.FirstOrDefault(arg => arg.TagID == item.TagID);
                        if (tagStamp != null)
                            item.Selected = tagStamp.Selected;
                    }
                }
            }



            TagStamps = new ObservableCollection<TagStamp>();

            // 先增加默认的：高清、中文
            foreach (TagStamp item in Main.TagStamps) {
                TagStamp tagStamp = tagStamps.Where(arg => arg.TagID == item.TagID).FirstOrDefault();
                if (tagStamp != null)
                    TagStamps.Add(tagStamp);
                else {
                    // 无该标记
                    item.Count = 0;
                    TagStamps.Add(item);
                }
            }
            Logger.Info("init current tag stamps ok");
        }

        private void AddSingleMovie()
        {
            Dialog_NewMovie dialog_NewMovie = new Dialog_NewMovie();
            if (!(bool)dialog_NewMovie.ShowDialog(MainWindow)) {
                return;
            }

            NewVideoDialogResult result = dialog_NewMovie.Result;
            if (string.IsNullOrEmpty(result.Text)) {
                Logger.Warn("new video input is empty");
                return;
            }

            List<string> vidList = ParseVIDList(result.Text, result.Prefix, result.VideoType);

            // 1.查询是否存在对应的 VID
            string sql = VideoMapper.BASE_SQL;
            IWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Select("VID").Eq("metadata.DBId", ConfigManager.Main.CurrentDBId).Eq("metadata.DataType", 0).In("VID", vidList);
            sql = wrapper.ToSelect() + sql + wrapper.ToWhere(false);


            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            List<Video> videos = metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), true);

            List<string> exists = new List<string>();
            if (videos != null && videos.Count > 0)
                exists = videos.Select(arg => arg.VID).ToList();

            if (exists.Count > 0) {
                exists.ForEach(arg => Logger.Warn($"video id exist:{arg}"));
            }

            vidList = vidList.Except(exists).ToList();

            List<Video> videoList = new List<Video>();
            foreach (string vid in vidList) {
                Video video = new Video() {
                    VID = vid,
                    DBId = ConfigManager.Main.CurrentDBId,
                    VideoType = result.VideoType,
                    FirstScanDate = DateHelper.Now(),
                    LastScanDate = DateHelper.Now(),
                };

                MetaData metaData = video.toMetaData();
                metaDataMapper.Insert(metaData);
                videoMapper.Insert(video);
                videoList.Add(video);
                Logger.Info($"add new video: {vid}");
            }
            AddNewDataTag(videoList);
            Statistic();

            // todo tab
            //if (ConfigManager.ScanConfig.LoadDataAfterScan)
            //    LoadData();
        }

        /// <summary>
        /// 打上新加入的标记
        /// </summary>
        /// <param name="videos"></param>
        private void AddNewDataTag(List<Video> videos)
        {
            List<string> list = new List<string>();
            foreach (Video video in videos)
                list.Add($"({video.DataID},10000)");
            if (list.Count > 0) {
                string sql =
                    $"insert or ignore into metadata_to_tagstamp (DataID,TagID) values {string.Join(",", list)}";
                videoMapper.ExecuteNonQuery(sql);
            }
        }

        public List<string> ParseVIDList(string str, string prefix, VideoType vedioType)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(str))
                return result;
            foreach (var item in str.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                string vid = (string.IsNullOrEmpty(prefix) ? string.Empty : prefix) + item;
                if (vedioType == VideoType.Europe)
                    vid = vid.Replace(" ", string.Empty);
                else
                    vid = vid.ToUpper().Replace(" ", string.Empty);
                if (!string.IsNullOrEmpty(vid) && !result.Contains(vid))
                    result.Add(vid);
            }

            return result;
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
                default:
                    wrapper.Like(searchType.ToString(), searchContent);
                    break;
            }

            return wrapper;
        }


        public async Task<List<string>> GetSearchCandidate()
        {
            return await Task.Run(() => {
                SearchField searchType = (SearchField)ConfigManager.Main.SearchSelectedIndex;
                string field = searchType.ToString();

                List<string> result = new List<string>();
                if (string.IsNullOrEmpty(SearchText))
                    return result;
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                SetSortOrder(wrapper); // 按照当前排序
                wrapper.Eq("metadata.DBId", ConfigManager.Main.CurrentDBId).Eq("metadata.DataType", 0);
                SelectWrapper<Video> selectWrapper = GetWrapper(searchType);
                if (selectWrapper != null)
                    wrapper.Join(selectWrapper);

                string condition_sql = wrapper.ToWhere(false) + wrapper.ToOrder()
                            + $" LIMIT 0,{SEARCH_CANDIDATE_MAX_COUNT}";

                string sql = $"SELECT DISTINCT {field} FROM metadata_video " +
                            "JOIN metadata " +
                            "on metadata.DataID=metadata_video.DataID ";
                if (searchType == SearchField.ActorName)
                    sql += ActorMapper.actor_join_sql;
                else if (searchType == SearchField.LabelName)
                    sql += LabelJoinSql;

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


        public void SetClassifyLoadingStatus(bool loading)
        {
            IsLoadingClassify = loading;
        }

        /// <summary>
        /// 获得标签
        /// </summary>
        public async void GetLabelList()
        {
            string like_sql = string.Empty;
            if (!string.IsNullOrEmpty(SearchText))
                like_sql = $" and LabelName like '%{SearchText.ToProperSql()}%' ";

            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label " +
                "JOIN metadata on metadata.DataID=metadata_to_label.DataID " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                $"{(!string.IsNullOrEmpty(like_sql) ? like_sql : string.Empty)}" +
                $"GROUP BY LabelName ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            if (list != null) {
                foreach (Dictionary<string, object> item in list) {
                    if (!item.ContainsKey("LabelName") || !item.ContainsKey("Count"))
                        continue;
                    string labelName = item["LabelName"].ToString();
                    long.TryParse(item["Count"].ToString(), out long count);
                    if (string.IsNullOrEmpty(labelName))
                        continue;
                    labels.Add($"{labelName}({count})");
                }
            }

            LabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++) {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), LabelList, labels[i]);
            }
        }


        public async void SetClassify(bool refresh = false)
        {
            List<string> list;

            if (ClassifySelectedIndex == 0) {
                // todo 这里可以考虑使用 SQLITE 的 WITH RECURSIVE
                // 官方：https://www.sqlite.org/lang_with.html
                // 中文：https://www.jianshu.com/p/135ce4e5b11f
                if (GenreList != null && GenreList.Count > 0 && !refresh)
                    return;

                Dictionary<string, long> genreDict = new Dictionary<string, long>();
                string sql = $"SELECT Genre from metadata " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} AND Genre !=''";

                List<Dictionary<string, object>> lists = metaDataMapper.Select(sql);
                if (lists != null) {
                    string searchText = string.IsNullOrEmpty(SearchText) ? string.Empty : SearchText;
                    bool search = !string.IsNullOrEmpty(searchText);

                    foreach (Dictionary<string, object> item in lists) {
                        if (!item.ContainsKey("Genre"))
                            continue;
                        string genre = item["Genre"].ToString();
                        if (string.IsNullOrEmpty(genre))
                            continue;
                        List<string> genres = genre.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (string g in genres) {
                            if (search && g.IndexOf(searchText) < 0)
                                continue;
                            if (genreDict.ContainsKey(g))
                                genreDict[g] = genreDict[g] + 1;
                            else
                                genreDict.Add(g, 1);
                        }
                    }
                }

                Dictionary<string, long> ordered = null;
                try {
                    ordered = genreDict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                SetClassifyLoadingStatus(true);
                GenreList = new ObservableCollection<string>();
                GenreList.Clear();
                await Task.Delay(10);
                if (ordered != null) {
                    foreach (var key in ordered.Keys) {
                        string v = $"{key}({ordered[key]})";
                        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), GenreList, v);
                    }
                }

                SetClassifyLoadingStatus(false);
            } else if (ClassifySelectedIndex == 1) {
                if (SeriesList != null && SeriesList.Count > 0 && !refresh)
                    return;
                list = GetListByField("Series");
                SetClassifyLoadingStatus(true);
                SeriesList = new ObservableCollection<string>();
                for (int i = 0; i < list.Count; i++) {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), SeriesList, list[i]);
                }

                SetClassifyLoadingStatus(false);
            } else if (ClassifySelectedIndex == 2) {
                if (StudioList != null && StudioList.Count > 0 && !refresh)
                    return;
                list = GetListByField("Studio");
                SetClassifyLoadingStatus(true);
                StudioList = new ObservableCollection<string>();
                for (int i = 0; i < list.Count; i++) {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), StudioList, list[i]);
                }

                SetClassifyLoadingStatus(false);
            } else if (ClassifySelectedIndex == 3) {
                if (DirectorList != null && DirectorList.Count > 0 && !refresh)
                    return;
                list = GetListByField("Director");
                SetClassifyLoadingStatus(true);
                DirectorList = new ObservableCollection<string>();
                for (int i = 0; i < list.Count; i++) {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), DirectorList, list[i]);
                }

                SetClassifyLoadingStatus(false);
            }
        }

        public List<string> GetListByField(string field)
        {
            string like_sql = string.Empty;
            if (!string.IsNullOrEmpty(SearchText))
                like_sql = $" and {field} like '%{SearchText.ToProperSql()}%' ";

            List<string> result = new List<string>();
            string sql = $"SELECT {field},Count({field}) as Count from metadata " +
                "JOIN metadata_video on metadata.DataID=metadata_video.DataID " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} AND {field} !='' " +
                $"{(!string.IsNullOrEmpty(like_sql) ? like_sql : string.Empty)}" +
                $"GROUP BY {field} ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            if (list != null) {
                foreach (Dictionary<string, object> item in list) {
                    if (!item.ContainsKey(field))
                        continue;
                    string name = item[field].ToString();
                    long.TryParse(item["Count"].ToString(), out long count);
                    if (string.IsNullOrEmpty(name))
                        continue;
                    result.Add($"{name}({count})");
                }
            }

            return result;
        }

        public void SetSortOrder<T>(IWrapper<T> wrapper, bool random = false)
        {
            if (wrapper == null)
                return;
            int sortIndex = 0;
            bool SortDescending = false;
            // todo tab
            //int.TryParse(Properties.Settings.Default.SortType, out int sortIndex);
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


        /// <summary>
        /// 统计：加载时间 <70ms (15620个信息)
        /// </summary>
        public void Statistic()
        {
            long dbid = ConfigManager.Main.CurrentDBId;
            AllVideoCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0));
            appDatabaseMapper.UpdateFieldById("Count", AllVideoCount.ToString(), dbid);

            FavoriteVideoCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Gt("Grade", 0));

            string actor_count_sql = "SELECT count(*) as Count " +
                     "from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                     "on metadata_to_actor.ActorID=actor_info.ActorID " +
                     "join metadata " +
                     "on metadata_to_actor.DataID=metadata.DataID " +
                     $"WHERE metadata.DBId={dbid} and metadata.DataType={0} " +
                     "GROUP BY actor_info.ActorID " +
                     "UNION " +
                     "select actor_info.ActorID  " +
                     "FROM actor_info WHERE NOT EXISTS " +
                     "(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) " +
                     "GROUP BY actor_info.ActorID)";

            AllActorCount = actorMapper.SelectCount(actor_count_sql);

            string label_count_sql = "SELECT COUNT(DISTINCT LabelName) as Count  from metadata_to_label " +
                                    "join metadata on metadata_to_label.DataID=metadata.DataID " +
                                     $"WHERE metadata.DBId={dbid} and metadata.DataType={0} ";

            AllLabelCount = metaDataMapper.SelectCount(label_count_sql);
            DateTime date1 = DateTime.Now.AddDays(-1 * RECENT_DAY);
            DateTime date2 = DateTime.Now;
            RecentWatchCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2)));
        }
    }
}
