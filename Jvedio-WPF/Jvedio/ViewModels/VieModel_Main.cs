using Jvedio.Core.Enums;
using Jvedio.Core.Server;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
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
using System.Text;
using System.Windows;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.ViewModel
{

    public class VieModel_Main : ViewModelBase
    {

        public const int RECENT_DAY = 3;
        public const int SEARCH_CANDIDATE_MAX_COUNT = 3;

        #region "事件"

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        #endregion

        #region "静态属性"
        /// <summary>
        /// flag, 只显示一次提示
        /// </summary>
        private static bool ShowRelativeScreen { get; set; } = false;

        private static Main MainWindow { get; set; }

        #endregion

        #region "属性"

        public string ClickFilterType { get; set; }

        public TabItemManager TabItemManager { get; set; }

        public AppDatabase CurrentAppDataBase { get; set; }


        #endregion

        #region "RelayCommand"
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
            AddNewMovie = new RelayCommand<object>(t => AddSingleMovie());
        }

        public void LoadAll()
        {
            HandleSideButtonCmd("All");
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
                case "Label":
                    TabItemManager.Add(TabType.GeoLabel, LangManager.GetValueByKey("Label"), LabelType.LabelName, SearchText);
                    break;
                case "Genre":
                case "Studio":
                case "Director":
                case "Series":
                    if (Enum.TryParse(param, out LabelType type))
                        TabItemManager.Add(TabType.GeoLabel, LangManager.GetValueByKey(param), type, SearchText);
                    break;
                case "Actor":
                    TabItemManager.Add(TabType.GeoActor, LangManager.GetValueByKey(param), null, SearchText);
                    break;
                default:
                    break;
            }
        }


        public void InitBinding()
        {
            VideoList.onStatistic += Statistic;
            ViewVideo.onStatistic += Statistic;
            VideoList.onSearchingChange += (value) => Searching = value;
            VideoList.onWaiting += OnWaiting;
        }

        private void OnWaiting(string msg, bool status)
        {
            Waiting = status;
            WaitingMsg = msg;
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


        public string _WaitingMsg;

        public string WaitingMsg {
            get { return _WaitingMsg; }

            set {
                _WaitingMsg = value;
                RaisePropertyChanged();
            }
        }
        public bool _Waiting;

        public bool Waiting {
            get { return _Waiting; }

            set {
                _Waiting = value;
                RaisePropertyChanged();
            }
        }



        public bool _DragInFile;

        public bool DragInFile {
            get { return _DragInFile; }

            set {
                _DragInFile = value;
                RaisePropertyChanged();
            }
        }


        public ServerStatus _ServerStatus;

        public ServerStatus ServerStatus {
            get { return _ServerStatus; }

            set {
                _ServerStatus = value;
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


        private double _SideGridWidth = ConfigManager.Main.SideGridWidth;


        /// <summary>
        /// 侧边栏宽度，不可删
        /// </summary>
        public double SideGridWidth {
            get { return _SideGridWidth; }

            set {
                _SideGridWidth = value;
                RaisePropertyChanged();
            }
        }


        #endregion

        #region "ObservableCollection"



        private double _DownLoadProgress = 0;

        public double DownLoadProgress {
            get { return _DownLoadProgress; }

            set {
                _DownLoadProgress = value;
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



        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();
            }
        }
        private bool _Searching;

        public bool Searching {
            get { return _Searching; }

            set {
                _Searching = value;
                RaisePropertyChanged();
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


        #endregion

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
            string sql = VideoMapper.SQL_BASE;
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

            if (ConfigManager.ScanConfig.LoadDataAfterScan)
                LoadAll();

            if (exists.Count > 0) {
                StringBuilder builder = new StringBuilder();
                builder.Append($"以下识别码已存在：{Environment.NewLine}");

                builder.Append(string.Join(Environment.NewLine, exists));

                new Dialog_Logs(builder.ToString()).ShowDialog();
                ;
            }
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

        /// <summary>
        /// 获得标签
        /// </summary>
        public List<string> GetLabelList()
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

            return labels;

        }

        public void onLabelClick(string label, LabelType type)
        {

            string typeName = type.ToString();

            SelectWrapper<Video> ExtraWrapper = new SelectWrapper<Video>();


            string tabName = "";

            switch (type) {

                case LabelType.LabelName:
                    tabName = LangManager.GetValueByKey("Label");
                    ExtraWrapper.Eq(typeName, label);
                    ExtraWrapper.ExtraSql = VideoMapper.SQL_JOIN_LABEL;
                    break;

                case LabelType.Genre:
                case LabelType.Series:
                case LabelType.Studio:
                case LabelType.Director:
                    tabName = LangManager.GetValueByKey(typeName);
                    ExtraWrapper.Like(typeName, label);
                    break;
                default:
                    break;
            }

            TabItemManager.Add(TabType.GeoVideo, $"{tabName}: {label}", ExtraWrapper);
        }

        public void ShowSameActor(long actorID)
        {
            ActorInfo actorInfo = MapperManager.actorMapper.SelectOne(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            if (actorInfo == null)
                return;
            ActorInfo.SetImage(ref actorInfo);
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("actor_info.ActorID", actorID);
            wrapper.ExtraSql = ActorMapper.SQL_JOIN_ACTOR;
            TabItemManager.Add(TabType.GeoVideo, $"{LangManager.GetValueByKey("Actor")}: {actorInfo.ActorName}", wrapper, actorInfo);
        }

        private VideoSideMenu SideMenu { get; set; }
        public void SetSideMenu(VideoSideMenu menu)
        {
            SideMenu = menu;
        }

        public void Statistic()
        {
            SideMenu?.Statistic(SearchText);
        }
    }
}
