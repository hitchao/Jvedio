using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Media;
using Jvedio.Core.Net;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Time;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;
using static Jvedio.Window_Server;
using static SuperUtils.Media.ImageHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio.ViewModel
{
    public class VieModel_Main : ViewModelBase
    {
        public event EventHandler PageChangedCompleted;

        public event EventHandler RenderSqlChanged;


        public bool IsFlipOvering { get; set; }

        public static string PreviousSql { get; set; } = string.Empty;

        public static int PreviousPage { get; set; } = 1;

        public string ClickFilterType { get; set; }

        private static Main MainWindow { get; set; }

        public CancellationTokenSource RenderVideoCTS { get; set; }

        public CancellationToken RenderVideoCT { get; set; }



        public static Queue<int> PageQueue { get; set; } = new Queue<int>();



        public AppDatabase CurrentAppDataBase { get; set; }

        public static string LabelJoinSql { get; set; } =
            " join metadata_to_label on metadata_to_label.DataID=metadata.DataID ";


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



        public static Dictionary<string, string> SELECT_TYPE = new Dictionary<string, string>()
        {
            { "All", "  " },
            { "Favorite", "  " },
            { "RecentWatch", "  " },
        };


        private static bool ShowRelativeScreen { get; set; } = false;


        #region "RelayCommand"
        public RelayCommand<object> SelectCommand { get; set; }

        public RelayCommand<object> ShowActorsCommand { get; set; }

        public RelayCommand<object> ShowLabelsCommand { get; set; }

        public RelayCommand<object> ShowClassifyCommand { get; set; }

        public RelayCommand<object> AddNewMovie { get; set; }
        #endregion


        public VieModel_Main(Main main)
        {
            MainWindow = main;
            ClickFilterType = string.Empty;
            InitCmd();
            RefreshVideoRenderToken();
            InitBinding();

            Logger.Info("init view model main ok");
        }

        private void InitCmd()
        {
            SelectCommand = new RelayCommand<object>(t => GenerateSelect(t));
            ShowActorsCommand = new RelayCommand<object>(t => ShowAllActors(t));
            ShowLabelsCommand = new RelayCommand<object>(t => ShowAllLabels(t));
            ShowClassifyCommand = new RelayCommand<object>(t => ShowClassify(t));
            AddNewMovie = new RelayCommand<object>(t => AddSingleMovie());
        }


        public void InitBinding()
        {
            CurrentVideoList.CollectionChanged += (s, e) => {
                if (CurrentVideoList != null && CurrentVideoList.Count > 0)
                    ShowSoft = false;
                else
                    ShowSoft = true;
            };
        }

        public void RefreshVideoRenderToken()
        {
            RenderVideoCTS = new CancellationTokenSource();
            RenderVideoCTS.Token.Register(() => { Logger.Warn("cancel load video page task"); });
            RenderVideoCT = RenderVideoCTS.Token;
        }



        #region "界面显示属性"

        public ServerStatus _ServerStatus;

        public ServerStatus ServerStatus {
            get { return _ServerStatus; }

            set {
                _ServerStatus = value;
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


        private bool _MainDataChecked;

        public bool MainDataChecked {
            get { return _MainDataChecked; }

            set {
                _MainDataChecked = value;
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

        private Visibility _ShowFirstRun = Visibility.Collapsed;

        public Visibility ShowFirstRun {
            get { return _ShowFirstRun; }

            set {
                _ShowFirstRun = value;
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

        private List<Video> _SelectedVideo = new List<Video>();

        public List<Video> SelectedVideo {
            get { return _SelectedVideo; }

            set {
                _SelectedVideo = value;
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



        public bool _EnableEditActress = false;

        public bool EnableEditActress {
            get { return _EnableEditActress; }

            set {
                _EnableEditActress = value;
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



        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();

                // BeginSearch();
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










        public bool _SideDefaultExpanded = ConfigManager.Main.SideDefaultExpanded;

        public bool SideDefaultExpanded {
            get { return _SideDefaultExpanded; }

            set {
                _SideDefaultExpanded = value;
                RaisePropertyChanged();
                ConfigManager.Main.SideDefaultExpanded = value;
                ConfigManager.Main.Save();
            }
        }
        public bool _SideTagStampExpanded = ConfigManager.Main.SideTagStampExpanded;

        public bool SideTagStampExpanded {
            get { return _SideTagStampExpanded; }

            set {
                _SideTagStampExpanded = value;
                RaisePropertyChanged();
                ConfigManager.Main.SideTagStampExpanded = value;
                ConfigManager.Main.Save();
            }
        }
        public bool _ShowSoft = true;

        public bool ShowSoft {
            get { return _ShowSoft; }

            set {
                _ShowSoft = value;
                RaisePropertyChanged();
            }
        }

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

        public void LoadData()
        {
            Select();
        }



        public void Reset() => Select();

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
            AddTags(videoList);// 新加入标记
            Statistic();
            if (ConfigManager.ScanConfig.LoadDataAfterScan)
                LoadData();
        }

        // 打上新加入的标记
        private void AddTags(List<Video> videos)
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
                            + $" LIMIT 0,{Properties.Settings.Default.SearchCandidateMaxCount}";

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
                .Take(Properties.Settings.Default.SearchCandidateMaxCount).ToList();
        }


        public void SetClassifyLoadingStatus(bool loading)
        {
            IsLoadingClassify = loading;
        }

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

        private delegate void LoadViewAssoVideoDelegate(Video video, int idx);

        private void LoadViewAssoVideo(Video video, int idx) => ViewAssociationDatas.Add(video);

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        // 获得标签
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

        private static AutoResetEvent resetEvent = new AutoResetEvent(false);

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

        public void RandomDisplay()
        {
            Select(true);
        }

        #region "影片"

        private bool _rendering;
        public bool Rendering {
            get { return _rendering; }
            set {
                _rendering = value;
                RaisePropertyChanged();
            }
        }

        public void SetSortOrder<T>(IWrapper<T> wrapper, bool random = false)
        {
            if (wrapper == null)
                return;
            int.TryParse(Properties.Settings.Default.SortType, out int sortIndex);
            if (sortIndex < 0 || sortIndex >= SortDict.Count)
                sortIndex = 0;
            string sortField = SortDict[sortIndex];
            if (random)
                wrapper.Asc("RANDOM()");
            else {
                if (Properties.Settings.Default.SortDescending)
                    wrapper.Desc(sortField);
                else
                    wrapper.Asc(sortField);
            }
        }

        public void ToLimit<T>(IWrapper<T> wrapper)
        {
            int row_count = PageSize;
            long offset = PageSize * (CurrentPage - 1);
            wrapper.Limit(offset, row_count);
        }



        public void GenerateSelect(object o = null)
        {
            if (!ShowRelativeScreen &&
             (PathType)ConfigManager.Settings.PicPathMode == PathType.RelativeToData) {
                ShowRelativeScreen = true;
                MessageCard.Info("当前图片模式为相对于影片，如果影片都位于同一目录，图片将会重复/覆盖");
            }


            ExtraWrapper = new SelectWrapper<Video>();

            // 侧边栏参数
            if (o != null && !string.IsNullOrEmpty(o.ToString())) {
                switch (o.ToString()) {
                    case "Favorite":
                        ExtraWrapper.Gt("metadata.Grade", 0);
                        break;
                    case "RecentWatch":
                        DateTime date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays);
                        DateTime date2 = DateTime.Now;
                        ExtraWrapper.Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2));
                        break;
                    default:
                        break;
                }
            }

            MainWindow.pagination.CurrentPage = 1;
            ClickFilterType = string.Empty;
            ShowActorGrid = false;
        }

        public async void Select(bool random = false)
        {
            Logger.Info("0.Select");
            TabSelectedIndex = 0; // 影片

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
                ScrollViewer scrollViewer =
                    MainWindow.FindVisualChild<ScrollViewer>(MainWindow.MovieItemsControl);
                scrollViewer.ScrollToTop(); // 滚到顶部
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
            List<MenuItem> allMenus = MainWindow.VideoTypeMenuItem.Items.OfType<MenuItem>().ToList();
            List<MenuItem> checkedMenus = new List<MenuItem>();

            App.Current.Dispatcher.Invoke(() => {
                checkedMenus = allMenus.Where(t => t.IsChecked).ToList();
            });

            if (checkedMenus.Count > 0 && checkedMenus.Count < 4) {
                // VideoType = 0 or VideoType = 1 or VideoType=2
                if (checkedMenus.Count == 1) {
                    int idx = allMenus.IndexOf(checkedMenus[0]);
                    wrapper.Eq("VideoType", idx);
                } else if (checkedMenus.Count == 2) {
                    int idx1 = allMenus.IndexOf(checkedMenus[0]);
                    int idx2 = allMenus.IndexOf(checkedMenus[1]);
                    wrapper.Eq("VideoType", idx1).LeftBracket().Or().Eq("VideoType", idx2).RightBracket();
                } else if (checkedMenus.Count == 3) {
                    int idx1 = allMenus.IndexOf(checkedMenus[0]);
                    int idx2 = allMenus.IndexOf(checkedMenus[1]);
                    int idx3 = allMenus.IndexOf(checkedMenus[2]);
                    wrapper.Eq("VideoType", idx1).LeftBracket().Or().Eq("VideoType", idx2).Or().Eq("VideoType", idx3).RightBracket();
                }
            }

            // 图片显示模式
            if (ConfigManager.Settings.PictureIndexCreated && PictureTypeIndex > 0) {
                sql += VideoMapper.COMMON_PICTURE_EXIST_JOIN_SQL;
                long pathType = ConfigManager.Settings.PicPathMode;
                int.TryParse(Properties.Settings.Default.ShowImageMode, out int imageType);
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
            App.Current.Dispatcher.Invoke(() => { MainWindow.pagination.Total = TotalCount; });
            RenderCurrentVideo(sql);
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
            int.TryParse(Properties.Settings.Default.ShowImageMode, out int imageMode);
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

        #endregion

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
            DateTime date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays);
            DateTime date2 = DateTime.Now;
            RecentWatchCount = metaDataMapper.SelectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Between("ViewDate", DateHelper.ToLocalDate(date1), DateHelper.ToLocalDate(date2)));
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
    }
}
