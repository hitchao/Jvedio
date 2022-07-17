using DynamicData;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Net;
using Jvedio.Core.Scan;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Logs;
using Jvedio.Mapper;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Utils.IO;
using JvedioLib.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.GlobalMapper;
using static Jvedio.GlobalVariable;
using static Jvedio.Utils.Media.ImageHelper;
using static Jvedio.Utils.Visual.VisualHelper;
namespace Jvedio.ViewModel
{
    public class VieModel_Main : ViewModelBase
    {

        public event EventHandler PageChangedCompleted;
        public event EventHandler ActorPageChangedCompleted;
        public event EventHandler RenderSqlChanged;
        public event EventHandler LoadAssoMetaDataCompleted;



        public bool IsFlipOvering { get; set; }

        public static string PreviousSql { get; set; }

        public static int PreviousPage { get; set; }

        public string ClickFilterType { get; set; }

        private static Main main { get; set; }

        public CancellationTokenSource renderVideoCTS { get; set; }
        public CancellationToken renderVideoCT { get; set; }


        public CancellationTokenSource renderActorCTS { get; set; }
        public CancellationToken renderActorCT { get; set; }

        public static Queue<int> pageQueue { get; set; }
        public static Queue<int> ActorPageQueue { get; set; }


        public AppDatabase CurrentAppDataBase { get; set; }


        public static string label_join_sql { get; set; }



        public List<Movie> MovieList { get; set; }

        private List<Video> CurrentExistAssoData { get; set; }


        #region "RelayCommand"
        public RelayCommand<object> SelectCommand { get; set; }
        public RelayCommand<object> ShowActorsCommand { get; set; }
        public RelayCommand<object> ShowLabelsCommand { get; set; }
        public RelayCommand<object> ShowClassifyCommand { get; set; }


        public RelayCommand AddNewMovie { get; set; }
        #endregion


        static VieModel_Main()
        {
            PreviousSql = "";
            PreviousPage = 1;
            pageQueue = new Queue<int>();
            ActorPageQueue = new Queue<int>();
            main = GetWindowByName("Main") as Main;
            label_join_sql = " join metadata_to_label on metadata_to_label.DataID=metadata.DataID ";
            ActorSortDict = new Dictionary<int, string>();
            for (int i = 0; i < ActorSortDictList.Count; i++)
            {
                ActorSortDict.Add(i, ActorSortDictList[i]);
            }


        }

        public VieModel_Main()
        {
            ClickFilterType = string.Empty;
            CurrentExistAssoData = new List<Video>();

            SelectCommand = new RelayCommand<object>(t => GenerateSelect(t));
            ShowActorsCommand = new RelayCommand<object>(t => ShowAllActors(t));
            ShowLabelsCommand = new RelayCommand<object>(t => ShowAllLabels(t));
            ShowClassifyCommand = new RelayCommand<object>(t => ShowClassify(t));
            AddNewMovie = new RelayCommand(AddSingleMovie);
            DataBases = new ObservableCollection<AppDatabase>();

            refreshVideoRenderToken();
            refreshActorRenderToken();

            // 注册 
            //GenreList.CollectionChanged += (sender, eventArgs) =>
            //{
            //    if (eventArgs..Cast<string>().Any(a => a.Equals("0005"))) resetEvent.Set();

            //};
            //resetEvent.WaitOne();
        }



        public void refreshVideoRenderToken()
        {
            renderVideoCTS = new CancellationTokenSource();
            renderVideoCTS.Token.Register(() => { Console.WriteLine("取消加载页码的任务"); });
            renderVideoCT = renderVideoCTS.Token;
        }

        public void refreshActorRenderToken()
        {
            renderActorCTS = new CancellationTokenSource();
            renderActorCTS.Token.Register(() => { Console.WriteLine("取消加载演员页码的任务"); });
            renderVideoCT = renderActorCTS.Token;
        }



        #region "界面显示属性"




        private Visibility _ShowFirstRun = Visibility.Collapsed;

        public Visibility ShowFirstRun
        {
            get { return _ShowFirstRun; }
            set
            {
                _ShowFirstRun = value;
                RaisePropertyChanged();
            }
        }


        private Visibility _ShowActorGrid = Visibility.Collapsed;

        public Visibility ShowActorGrid
        {
            get { return _ShowActorGrid; }
            set
            {
                _ShowActorGrid = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _GoToTopCanvas = Visibility.Collapsed;

        public Visibility GoToTopCanvas
        {
            get { return _GoToTopCanvas; }
            set
            {
                _GoToTopCanvas = value;
                RaisePropertyChanged();
            }
        }
        private Visibility _GoToBottomCanvas = Visibility.Visible;

        public Visibility GoToBottomCanvas
        {
            get { return _GoToBottomCanvas; }
            set
            {
                _GoToBottomCanvas = value;
                RaisePropertyChanged();
            }
        }



        private Visibility _ActorProgressBarVisibility = Visibility.Collapsed;

        public Visibility ActorProgressBarVisibility
        {
            get { return _ActorProgressBarVisibility; }
            set
            {
                _ActorProgressBarVisibility = value;
                RaisePropertyChanged();
            }
        }



        private int _SearchSelectedIndex = (int)GlobalConfig.Main.SearchSelectedIndex;

        public int SearchSelectedIndex
        {
            get { return _SearchSelectedIndex; }
            set
            {
                _SearchSelectedIndex = value;
                RaisePropertyChanged();
            }
        }
        private int _ClassifySelectedIndex = (int)GlobalConfig.Main.ClassifySelectedIndex;

        public int ClassifySelectedIndex
        {
            get { return _ClassifySelectedIndex; }
            set
            {
                _ClassifySelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private double _SideGridWidth = GlobalConfig.Main.SideGridWidth;

        public double SideGridWidth
        {
            get { return _SideGridWidth; }
            set
            {
                _SideGridWidth = value;
                RaisePropertyChanged();
            }
        }




        private BitmapSource _BackgroundImage = GlobalVariable.BackgroundImage;

        public BitmapSource BackgroundImage
        {
            get { return _BackgroundImage; }
            set
            {
                _BackgroundImage = value;
                RaisePropertyChanged();
            }
        }

        private int _TabSelectedIndex = 0;

        public int TabSelectedIndex
        {
            get { return _TabSelectedIndex; }
            set
            {
                _TabSelectedIndex = value;
                RaisePropertyChanged();
            }
        }



        private bool _IsLoadingMovie = true;

        public bool IsLoadingMovie
        {
            get { return _IsLoadingMovie; }
            set
            {
                _IsLoadingMovie = value;
                RaisePropertyChanged();
            }
        }


        private bool _IsLoadingClassify = false;

        public bool IsLoadingClassify
        {
            get { return _IsLoadingClassify; }
            set
            {
                _IsLoadingClassify = value;
                RaisePropertyChanged();
            }
        }

        private bool _TaskIconVisible = true;

        public bool TaskIconVisible
        {
            get { return _TaskIconVisible; }
            set
            {
                _TaskIconVisible = value;
                RaisePropertyChanged();
            }
        }


        private Thickness _MainGridThickness = new Thickness(10);

        public Thickness MainGridThickness
        {
            get { return _MainGridThickness; }
            set
            {
                _MainGridThickness = value;
                RaisePropertyChanged();
            }
        }


        #endregion



        #region "ObservableCollection"

        private ObservableCollection<ScanTask> _ScanTasks = new ObservableCollection<ScanTask>();
        public ObservableCollection<ScanTask> ScanTasks
        {
            get { return _ScanTasks; }
            set
            {
                _ScanTasks = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<DownLoadTask> _DownLoadTasks = new ObservableCollection<DownLoadTask>();
        public ObservableCollection<DownLoadTask> DownLoadTasks
        {
            get { return _DownLoadTasks; }
            set
            {
                _DownLoadTasks = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<ScreenShotTask> _ScreenShotTasks = new ObservableCollection<ScreenShotTask>();
        public ObservableCollection<ScreenShotTask> ScreenShotTasks
        {
            get { return _ScreenShotTasks; }
            set
            {
                _ScreenShotTasks = value;
                RaisePropertyChanged();
            }
        }

        private double _DownLoadProgress = 0;
        public double DownLoadProgress
        {
            get { return _DownLoadProgress; }
            set
            {
                _DownLoadProgress = value;
                RaisePropertyChanged();
            }
        }
        private double _ScreenShotProgress = 0;
        public double ScreenShotProgress
        {
            get { return _ScreenShotProgress; }
            set
            {
                _ScreenShotProgress = value;
                RaisePropertyChanged();
            }
        }
        private Visibility _ScreenShotVisibility = Visibility.Collapsed;
        public Visibility ScreenShotVisibility
        {
            get { return _ScreenShotVisibility; }
            set
            {
                _ScreenShotVisibility = value;
                RaisePropertyChanged();
            }
        }
        private Visibility _DownLoadVisibility = Visibility.Collapsed;
        public Visibility DownLoadVisibility
        {
            get { return _DownLoadVisibility; }
            set
            {
                _DownLoadVisibility = value;
                RaisePropertyChanged();
            }
        }




        private ObservableCollection<Message> _Message = new ObservableCollection<Message>();
        public ObservableCollection<Message> Message
        {
            get { return _Message; }
            set
            {
                _Message = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<AppDatabase> _DataBases;


        public ObservableCollection<AppDatabase> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }

        private List<Video> _VideoList;
        public List<Video> VideoList
        {
            get { return _VideoList; }
            set
            {
                _VideoList = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<Video> _CurrentVideoList;
        public ObservableCollection<Video> CurrentVideoList
        {
            get { return _CurrentVideoList; }
            set
            {
                _CurrentVideoList = value;
                RaisePropertyChanged();
            }
        }







        private List<Video> _SelectedVideo = new List<Video>();

        public List<Video> SelectedVideo
        {
            get { return _SelectedVideo; }
            set
            {
                _SelectedVideo = value;
                RaisePropertyChanged();
            }
        }


        private List<ActorInfo> _SelectedActors = new List<ActorInfo>();

        public List<ActorInfo> SelectedActors
        {
            get { return _SelectedActors; }
            set
            {
                _SelectedActors = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<TagStamp> _TagStamps = new ObservableCollection<TagStamp>();

        public ObservableCollection<TagStamp> TagStamps
        {
            get { return _TagStamps; }
            set
            {
                _TagStamps = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _GenreList;
        public ObservableCollection<string> GenreList
        {
            get { return _GenreList; }
            set
            {
                _GenreList = value;
                RaisePropertyChanged();

            }
        }


        private List<ActorInfo> actorlist;
        public List<ActorInfo> ActorList
        {
            get { return actorlist; }
            set
            {
                actorlist = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<ActorInfo> _CurrentActorList;


        public ObservableCollection<ActorInfo> CurrentActorList
        {
            get { return _CurrentActorList; }
            set
            {
                _CurrentActorList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> labellist;
        public ObservableCollection<string> LabelList
        {
            get { return labellist; }
            set
            {
                labellist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _SeriesList;
        public ObservableCollection<string> SeriesList
        {
            get { return _SeriesList; }
            set
            {
                _SeriesList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> studiollist;
        public ObservableCollection<string> StudioList
        {
            get { return studiollist; }
            set
            {
                studiollist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> directorList
;
        public ObservableCollection<string> DirectorList

        {
            get { return directorList; }
            set
            {
                directorList = value;
                RaisePropertyChanged();
            }
        }


        private bool _Searching = false;


        public bool Searching
        {
            get { return _Searching; }
            set
            {
                _Searching = value;
                RaisePropertyChanged();

            }
        }
        private bool _SearchingActor = false;


        public bool SearchingActor
        {
            get { return _SearchingActor; }
            set
            {
                _SearchingActor = value;
                RaisePropertyChanged();

            }
        }




        private ObservableCollection<string> _FilePathClassification;


        public ObservableCollection<string> FilePathClassification
        {
            get { return _FilePathClassification; }
            set
            {
                _FilePathClassification = value;
                RaisePropertyChanged();

            }
        }

        private ObservableCollection<string> _SearchHistory;


        public ObservableCollection<string> SearchHistory
        {
            get { return _SearchHistory; }
            set
            {
                _SearchHistory = value;
                RaisePropertyChanged();

            }
        }
        #endregion


        #region "Variable"









        private string _ScanStatus;

        public string ScanStatus
        {
            get { return _ScanStatus; }
            set
            {
                _ScanStatus = value;
                RaisePropertyChanged();

            }
        }
        private string _DownloadStatus;

        public string DownloadStatus
        {
            get { return _DownloadStatus; }
            set
            {
                _DownloadStatus = value;
                RaisePropertyChanged();

            }
        }







        private double _RecentWatchedCount = 0;
        public double RecentWatchedCount
        {
            get { return _RecentWatchedCount; }
            set
            {
                _RecentWatchedCount = value;
                RaisePropertyChanged();
            }
        }


        private long _AllVideoCount = 0;
        public long AllVideoCount
        {
            get { return _AllVideoCount; }
            set
            {
                _AllVideoCount = value;
                RaisePropertyChanged();
            }
        }

        private double _FavoriteVideoCount = 0;
        public double FavoriteVideoCount
        {
            get { return _FavoriteVideoCount; }
            set
            {
                _FavoriteVideoCount = value;
                RaisePropertyChanged();
            }
        }

        private long _RecentWatchCount = 0;
        public long RecentWatchCount
        {
            get { return _RecentWatchCount; }
            set
            {
                _RecentWatchCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllActorCount = 0;
        public long AllActorCount
        {
            get { return _AllActorCount; }
            set
            {
                _AllActorCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllLabelCount = 0;
        public long AllLabelCount
        {
            get { return _AllLabelCount; }
            set
            {
                _AllLabelCount = value;
                RaisePropertyChanged();
            }
        }


        public bool _IsScanning = false;
        public bool IsScanning
        {
            get { return _IsScanning; }
            set
            {
                _IsScanning = value;
                RaisePropertyChanged();
            }
        }


        public bool _EnableEditActress = false;

        public bool EnableEditActress
        {
            get { return _EnableEditActress; }
            set
            {
                _EnableEditActress = value;
                RaisePropertyChanged();
            }
        }



        private int _CurrentPage = 1;
        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                //FlowNum = 0;
                RaisePropertyChanged();
            }
        }


        private int _PageSize = Properties.Settings.Default.PageSize;
        public int PageSize
        {
            get { return _PageSize; }
            set
            {
                _PageSize = value;
                RaisePropertyChanged();
            }
        }


        public int _CurrentCount = 0;
        public int CurrentCount
        {
            get { return _CurrentCount; }
            set
            {
                _CurrentCount = value;
                RaisePropertyChanged();

            }
        }

        public int _CurrentActorCount = 0;
        public int CurrentActorCount
        {
            get { return _CurrentActorCount; }
            set
            {
                _CurrentActorCount = value;
                RaisePropertyChanged();

            }
        }


        public long _TotalCount = 0;
        public long TotalCount
        {
            get { return _TotalCount; }
            set
            {
                _TotalCount = value;
                RaisePropertyChanged();

            }
        }

        public int totalpage = 1;
        public int TotalPage
        {
            get { return totalpage; }
            set
            {
                totalpage = value;
                RaisePropertyChanged();

            }
        }


        private int _ActorPageSize = 10;
        public int ActorPageSize
        {
            get { return _ActorPageSize; }
            set
            {
                _ActorPageSize = value;
                RaisePropertyChanged();
            }
        }


        private long _ActorTotalCount = 0;
        public long ActorTotalCount
        {
            get { return _ActorTotalCount; }
            set
            {
                _ActorTotalCount = value;
                RaisePropertyChanged();

            }
        }


        private int currentactorpage = 1;
        public int CurrentActorPage
        {
            get { return currentactorpage; }
            set
            {
                currentactorpage = value;

                RaisePropertyChanged();
            }
        }








        private string _SearchText = string.Empty;


        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                RaisePropertyChanged();
                //BeginSearch();
            }
        }



        private ActorInfo _CurrentActorInfo;
        public ActorInfo CurrentActorInfo
        {
            get { return _CurrentActorInfo; }
            set
            {
                _CurrentActorInfo = value;
                RaisePropertyChanged();
            }
        }


        #endregion



        #region "筛选"

        private bool _IsRefresh = false;

        public bool IsRefresh
        {
            get { return _IsRefresh; }
            set
            {
                _IsRefresh = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Notice> _Notices;

        public ObservableCollection<Notice> Notices
        {
            get { return _Notices; }
            set
            {
                _Notices = value;
                RaisePropertyChanged();
            }
        }
        private bool _TeenMode = GlobalConfig.Settings.TeenMode;

        public bool TeenMode
        {
            get { return _TeenMode; }
            set
            {
                _TeenMode = value;
                RaisePropertyChanged();
            }
        }
        private bool _RunningLongTask = false;

        public bool RunningLongTask
        {
            get { return _RunningLongTask; }
            set
            {
                _RunningLongTask = value;
                RaisePropertyChanged();
            }
        }


        // 影片关联
        public ObservableCollection<Video> _AssociationDatas;
        public ObservableCollection<Video> AssociationDatas
        {
            get { return _AssociationDatas; }
            set
            {
                _AssociationDatas = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<Video> _ViewAssociationDatas;
        public ObservableCollection<Video> ViewAssociationDatas
        {
            get { return _ViewAssociationDatas; }
            set
            {
                _ViewAssociationDatas = value;
                RaisePropertyChanged();
            }
        }
        public long _AssoSearchTotalCount;
        public long AssoSearchTotalCount
        {
            get { return _AssoSearchTotalCount; }
            set
            {
                _AssoSearchTotalCount = value;
                RaisePropertyChanged();
            }
        }
        public int _AssoSearchPageSize = 20;
        public int AssoSearchPageSize
        {
            get { return _AssoSearchPageSize; }
            set
            {
                _AssoSearchPageSize = value;
                RaisePropertyChanged();
            }
        }


        private int _CurrentAssoSearchPage = 1;
        public int CurrentAssoSearchPage
        {
            get { return _CurrentAssoSearchPage; }
            set
            {
                _CurrentAssoSearchPage = value;
                RaisePropertyChanged();
            }
        }
        private string _AssoSearchText = "";
        public string AssoSearchText
        {
            get { return _AssoSearchText; }
            set
            {
                _AssoSearchText = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<Video> _AssociationSelectedDatas = new ObservableCollection<Video>();
        public ObservableCollection<Video> AssociationSelectedDatas
        {
            get { return _AssociationSelectedDatas; }
            set
            {
                _AssociationSelectedDatas = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<Video> _ExistAssociationDatas = new ObservableCollection<Video>();
        public ObservableCollection<Video> ExistAssociationDatas
        {
            get { return _ExistAssociationDatas; }
            set
            {
                _ExistAssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        #region "右键筛选"

        private int _DataExistIndex = 0;
        public int DataExistIndex
        {
            get { return _DataExistIndex; }
            set
            {
                if (value < 0 || value > 2)
                    _DataExistIndex = 0;
                else
                    _DataExistIndex = value;
                RaisePropertyChanged();
            }
        }
        private int _PictureTypeIndex = 0;
        public int PictureTypeIndex
        {
            get { return _PictureTypeIndex; }
            set
            {
                if (value < 0 || value > 2)
                    _PictureTypeIndex = 0;
                else
                    _PictureTypeIndex = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        private int _DownloadLongTaskDelay = 0;
        public int DownloadLongTaskDelay
        {
            get { return _DownloadLongTaskDelay; }
            set
            {
                _DownloadLongTaskDelay = value;
                if (value > 0) DisplayDownloadLongTaskDelay = Visibility.Visible;
                else DisplayDownloadLongTaskDelay = Visibility.Hidden;
                RaisePropertyChanged();
            }
        }

        private Visibility _DisplayDownloadLongTaskDelay = Visibility.Hidden;
        public Visibility DisplayDownloadLongTaskDelay
        {
            get { return _DisplayDownloadLongTaskDelay; }
            set
            {
                _DisplayDownloadLongTaskDelay = value;
                RaisePropertyChanged();
            }
        }





        #endregion


        #region "皮肤"


        private ActorInfo _ThemesColor;
        public ActorInfo ThemesColor
        {
            get { return _ThemesColor; }
            set
            {
                _ThemesColor = value;
                RaisePropertyChanged();
            }
        }

        #endregion




        public void LoadData()
        {
            Select();
        }
        public void LoadActor()
        {
            SelectActor();
        }


        public void Reset() => Select();


        public void ShowAllActors(object o)
        {
            TabSelectedIndex = 1;
            SelectActor();
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


        public void InitCurrentTagStamps()
        {
            List<Dictionary<string, object>> list = tagStampMapper.select(TagStampMapper.GetTagSql());
            List<TagStamp> tagStamps = new List<TagStamp>();
            if (list?.Count > 0)
                tagStamps = tagStampMapper.toEntity<TagStamp>(list, typeof(TagStamp).GetProperties(), false);
            TagStamps = new ObservableCollection<TagStamp>();
            // 先增加默认的：高清、中文
            foreach (TagStamp item in GlobalVariable.TagStamps)
            {
                TagStamp tagStamp = tagStamps.Where(arg => arg.TagID == item.TagID).FirstOrDefault();
                if (tagStamp != null) TagStamps.Add(tagStamp);
                else
                {
                    // 无该标记
                    item.Count = 0;
                    TagStamps.Add(item);
                }
            }
        }


        private void AddSingleMovie()
        {
            Dialog_NewMovie dialog_NewMovie = new Dialog_NewMovie(main);
            if ((bool)dialog_NewMovie.ShowDialog())
            {
                NewVideoDialogResult result = dialog_NewMovie.Result;
                if (!string.IsNullOrEmpty(result.Text))
                {
                    List<string> vidList = parseVIDList(result.Text, result.Prefix, result.VideoType);

                    string sql = VideoMapper.BASE_SQL;
                    IWrapper<Video> wrapper = new SelectWrapper<Video>();
                    wrapper.Select("VID").Eq("metadata.DBId", GlobalConfig.Main.CurrentDBId).Eq("metadata.DataType", 0).In("VID", vidList);
                    sql = wrapper.toSelect() + sql + wrapper.toWhere(false);
                    List<Dictionary<string, object>> list = metaDataMapper.select(sql);
                    List<Video> videos = metaDataMapper.toEntity<Video>(list, typeof(Video).GetProperties(), true);

                    List<string> exists = new List<string>();
                    if (videos != null && videos.Count > 0)
                        exists = videos.Select(arg => arg.VID).ToList();
                    vidList = vidList.Except(exists).ToList();
                    foreach (string vid in vidList)
                    {
                        Video video = new Video()
                        {
                            VID = vid,
                            DBId = GlobalConfig.Main.CurrentDBId,
                            VideoType = result.VideoType,
                            FirstScanDate = DateHelper.Now(),
                            LastScanDate = DateHelper.Now(),
                        };

                        MetaData metaData = video.toMetaData();
                        metaDataMapper.insert(metaData);
                        videoMapper.insert(video);
                    }
                    Statistic();

                }
            }



        }




        public List<string> parseVIDList(string str, string prefix, VideoType vedioType)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(str)) return result;
            foreach (var item in str.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                string vid = (string.IsNullOrEmpty(prefix) ? "" : prefix) + item;
                if (vedioType == VideoType.Europe)
                    vid = vid.Replace(" ", "");
                else
                    vid = vid.ToUpper().Replace(" ", "");
                if (!string.IsNullOrEmpty(vid) && !result.Contains(vid))
                    result.Add(vid);
            }
            return result;
        }





        public async Task<List<string>> GetSearchCandidate()
        {
            return await Task.Run(() =>
            {
                SearchField searchType = (SearchField)GlobalConfig.Main.SearchSelectedIndex;
                string field = searchType.ToString();

                List<string> result = new List<string>();
                if (string.IsNullOrEmpty(SearchText)) return result;
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                setSortOrder(wrapper);//按照当前排序
                wrapper.Eq("metadata.DBId", GlobalConfig.Main.CurrentDBId).Eq("metadata.DataType", 0);
                SelectWrapper<Video> selectWrapper = getWrapper(searchType);
                if (selectWrapper != null) wrapper.Join(selectWrapper);

                string condition_sql = wrapper.toWhere(false) + wrapper.toOrder()
                            + $" LIMIT 0,{Properties.Settings.Default.SearchCandidateMaxCount}";

                string sql = $"SELECT DISTINCT {field} FROM metadata_video " +
                            "JOIN metadata " +
                            "on metadata.DataID=metadata_video.DataID ";
                if (searchType == SearchField.ActorName)
                    sql += ActorMapper.actor_join_sql;
                else if (searchType == SearchField.LabelName)
                    sql += label_join_sql;

                if (searchType == SearchField.Genre)
                {
                    // 类别特殊处理
                    string genre_sql = $"SELECT {field} FROM metadata_video " +
                            "JOIN metadata " +
                            "on metadata.DataID=metadata_video.DataID ";
                    List<Dictionary<string, object>> list = metaDataMapper.select(genre_sql);
                    if (list != null && list.Count > 0) SetGenreCandidate(field, list, ref result);
                }
                else
                {
                    List<Dictionary<string, object>> list = metaDataMapper.select(sql + condition_sql);
                    if (list != null && list.Count > 0)
                    {
                        foreach (Dictionary<string, object> dict in list)
                        {
                            if (!dict.ContainsKey(field)) continue;
                            string value = dict[field].ToString();
                            if (string.IsNullOrEmpty(value)) continue;
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
            foreach (Dictionary<string, object> dict in list)
            {
                if (!dict.ContainsKey(field)) continue;
                string value = dict[field].ToString();
                if (string.IsNullOrEmpty(value)) continue;
                string[] arr = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    foreach (var item in arr)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        set.Add(item);
                    }
                }
            }
            result = set.Where(arg => arg.ToLower().IndexOf(search) >= 0).ToList()
                .Take(Properties.Settings.Default.SearchCandidateMaxCount).ToList();
        }

        private delegate void LoadActorDelegate(ActorInfo actor, int idx);
        private void LoadActor(ActorInfo actor, int idx)
        {
            if (renderActorCT.IsCancellationRequested) return;
            if (CurrentActorList.Count < ActorPageSize)
            {
                if (idx < CurrentActorList.Count)
                {
                    CurrentActorList[idx] = null;
                    CurrentActorList[idx] = actor;
                }
                else
                {
                    CurrentActorList.Add(actor);
                }

            }
            else
            {
                CurrentActorList[idx] = null;
                CurrentActorList[idx] = actor;
            }
            CurrentActorCount = CurrentActorList.Count;
        }



        public void SetClassifyLoadingStatus(bool loading)
        {
            IsLoadingClassify = loading;

        }



        private delegate void LoadVideoDelegate(Video video, int idx);
        private void LoadVideo(Video video, int idx)
        {
            if (renderVideoCT.IsCancellationRequested) return;
            if (CurrentVideoList.Count < PageSize)
            {
                if (idx < CurrentVideoList.Count)
                {
                    CurrentVideoList[idx] = null;
                    CurrentVideoList[idx] = video;
                }
                else
                {
                    CurrentVideoList.Add(video);
                }

            }
            else
            {
                if (idx < CurrentVideoList.Count)
                {
                    CurrentVideoList[idx] = null;
                    CurrentVideoList[idx] = video;
                }

            }

        }


        private delegate void LoadAssoVideoDelegate(Video video, int idx);
        private void LoadAssoVideo(Video video, int idx)
        {
            if (AssociationDatas.Count < main.assoPagination.PageSize)
            {
                if (idx < AssociationDatas.Count)
                {
                    AssociationDatas[idx] = null;
                    AssociationDatas[idx] = video;
                }
                else
                {
                    AssociationDatas.Add(video);
                }

            }
            else
            {
                AssociationDatas[idx] = null;
                AssociationDatas[idx] = video;
            }
        }
        private delegate void LoadViewAssoVideoDelegate(Video video, int idx);
        private void LoadViewAssoVideo(Video video, int idx) => ViewAssociationDatas.Add(video);



        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);
        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        //获得标签
        public async void GetLabelList()
        {
            string like_sql = "";
            if (!string.IsNullOrEmpty(SearchText))
                like_sql = $" and LabelName like '%{SearchText.ToProperSql()}%' ";


            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label " +
                "JOIN metadata on metadata.DataID=metadata_to_label.DataID " +
                $"where metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                $"{(!string.IsNullOrEmpty(like_sql) ? like_sql : "")}" +
                $"GROUP BY LabelName ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            if (list != null)
            {
                foreach (Dictionary<string, object> item in list)
                {
                    if (!item.ContainsKey("LabelName") || !item.ContainsKey("Count")) continue;
                    string LabelName = item["LabelName"].ToString();
                    long.TryParse(item["Count"].ToString(), out long count);
                    if (string.IsNullOrEmpty(LabelName)) continue;
                    labels.Add($"{LabelName}({count})");
                }
            }

            LabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), LabelList, labels[i]);
            }
        }






        private static AutoResetEvent resetEvent = new AutoResetEvent(false);

        public async void SetClassify(bool refresh = false)
        {

            List<string> list;

            if (ClassifySelectedIndex == 0)
            {
                // todo 这里可以考虑使用 SQLITE 的 WITH RECURSIVE
                // 官方：https://www.sqlite.org/lang_with.html
                // 中文：https://www.jianshu.com/p/135ce4e5b11f
                if (GenreList != null && GenreList.Count > 0 && !refresh) return;

                Dictionary<string, long> genreDict = new Dictionary<string, long>();
                string sql = $"SELECT Genre from metadata " +
                    $"where metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} AND Genre !=''";

                List<Dictionary<string, object>> lists = metaDataMapper.select(sql);
                if (lists != null)
                {
                    string searchText = string.IsNullOrEmpty(SearchText) ? "" : SearchText;
                    bool search = !string.IsNullOrEmpty(searchText);

                    foreach (Dictionary<string, object> item in lists)
                    {
                        if (!item.ContainsKey("Genre")) continue;
                        string genre = item["Genre"].ToString();
                        if (string.IsNullOrEmpty(genre)) continue;
                        List<string> genres = genre.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (string g in genres)
                        {
                            if (search && g.IndexOf(searchText) < 0) continue;
                            if (genreDict.ContainsKey(g))
                                genreDict[g] = genreDict[g] + 1;

                            else
                                genreDict.Add(g, 1);
                        }
                    }

                }

                Dictionary<string, long> ordered = null;
                try
                {
                    ordered = genreDict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                }
                catch (Exception ex)
                { Logger.Error(ex); }

                SetClassifyLoadingStatus(true);
                GenreList = new ObservableCollection<string>();
                GenreList.Clear();
                await Task.Delay(10);
                if (ordered != null)
                {
                    foreach (var key in ordered.Keys)
                    {
                        string v = $"{key}({ordered[key]})";
                        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), GenreList, v);
                    }
                }


                SetClassifyLoadingStatus(false);
            }
            else if (ClassifySelectedIndex == 1)
            {
                if (SeriesList != null && SeriesList.Count > 0 && !refresh) return;
                list = GetListByField("Series");
                SetClassifyLoadingStatus(true);
                SeriesList = new ObservableCollection<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), SeriesList, list[i]);
                }
                SetClassifyLoadingStatus(false);
            }
            else if (ClassifySelectedIndex == 2)
            {
                if (StudioList != null && StudioList.Count > 0 && !refresh) return;
                list = GetListByField("Studio");
                SetClassifyLoadingStatus(true);
                StudioList = new ObservableCollection<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), StudioList, list[i]);
                }
                SetClassifyLoadingStatus(false);
            }
            else if (ClassifySelectedIndex == 3)
            {
                if (DirectorList != null && DirectorList.Count > 0 && !refresh) return;
                list = GetListByField("Director");
                SetClassifyLoadingStatus(true);
                DirectorList = new ObservableCollection<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), DirectorList, list[i]);
                }
                SetClassifyLoadingStatus(false);
            }

        }


        public List<string> GetListByField(string field)
        {
            string like_sql = "";
            if (!string.IsNullOrEmpty(SearchText))
                like_sql = $" and {field} like '%{SearchText.ToProperSql()}%' ";



            List<string> result = new List<string>();
            string sql = $"SELECT {field},Count({field}) as Count from metadata " +
                "JOIN metadata_video on metadata.DataID=metadata_video.DataID " +
                $"where metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} AND {field} !='' " +
                $"{(!string.IsNullOrEmpty(like_sql) ? like_sql : "")}" +
                $"GROUP BY {field} ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            if (list != null)
            {
                foreach (Dictionary<string, object> item in list)
                {
                    if (!item.ContainsKey(field)) continue;
                    string name = item[field].ToString();
                    long.TryParse(item["Count"].ToString(), out long count);
                    if (string.IsNullOrEmpty(name)) continue;
                    result.Add($"{name}({count})");
                }
            }

            return result;
        }


        //获得演员，信息照片都获取

        #region "演员" 


        public bool renderingActor { get; set; }

        public static List<string> ActorSortDictList = new List<string>()
        {
            "actor_info.Grade" ,
            "actor_info.ActorName" ,
            "Count",
            "actor_info.Country" ,
            "Nation" ,
            "BirthPlace" ,
            "Birthday" ,
            "BloodType" ,
            "Height" ,
            "Weight" ,
            "Gender" ,
            "Hobby" ,
            "Cup" ,
            "Chest" ,
            "Waist" ,
            "Hipline" ,
            "actor_info.Grade" ,
            "Age"
        };

        public static Dictionary<int, string> ActorSortDict { get; set; }

        public static string[] ActorSelectedField = new string[]
        {
            "count(ActorName) as Count",
            "actor_info.ActorID",
            "actor_info.ActorName",
            "actor_info.Country",
            "Nation",
            "BirthPlace",
            "Birthday",
            "BloodType",
            "Height",
            "Weight",
            "Gender",
            "Hobby",
            "Cup",
            "Chest",
            "Waist",
            "Hipline",
            "WebType",
            "WebUrl",
            "actor_info.Grade",
            "actor_info.ExtraInfo",
            "actor_info.CreateDate",
            "actor_info.UpdateDate",
        };

        public void ActorSetActorSortOrder<T>(IWrapper<T> wrapper)
        {
            if (wrapper == null || Properties.Settings.Default.ActorSortType >= ActorSortDict.Count) return;
            string sortField = ActorSortDict[Properties.Settings.Default.ActorSortType];
            if (Properties.Settings.Default.ActorSortDescending) wrapper.Desc(sortField);
            else wrapper.Asc(sortField);
        }


        public string ActorToLimit()
        {
            int row_count = ActorPageSize;
            long offset = ActorPageSize * (CurrentActorPage - 1);
            return $" LIMIT {offset},{row_count}";
        }

        public static Dictionary<string, string> Actor_SELECT_TYPE = new Dictionary<string, string>() {
            { "All","  "  },
            { "Favorite","  "  },
        };



        public async void SelectActor()
        {
            TabSelectedIndex = 1; // 演员
            // 判断当前获取的队列
            while (ActorPageQueue.Count > 1)
            {
                int page = ActorPageQueue.Dequeue();
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (renderingActor)
            {
                renderActorCTS?.Cancel();// 取消加载
                await Task.Delay(100);
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                main.ActorScrollViewer.ScrollToTop();//滚到顶部
            });

            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            ActorSetActorSortOrder(wrapper);

            bool search = SearchingActor && !string.IsNullOrEmpty(SearchText);

            string count_sql = "SELECT count(*) as Count " +
                         "from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                         "on metadata_to_actor.ActorID=actor_info.ActorID " +
                         "join metadata " +
                         "on metadata_to_actor.DataID=metadata.DataID " +
                         $"WHERE metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                         $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : "")} " +
                         "GROUP BY actor_info.ActorID " +
                         "UNION " +
                         "select actor_info.ActorID  " +
                         "FROM actor_info WHERE NOT EXISTS " +
                         "(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) " +
                         $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : "")} " +
                         "GROUP BY actor_info.ActorID)";

            ActorTotalCount = actorMapper.selectCount(count_sql);

            string sql = $"{wrapper.Select(ActorSelectedField).toSelect(false)} FROM actor_info " +
                $"join metadata_to_actor on metadata_to_actor.ActorID=actor_info.ActorID " +
                $"join metadata on metadata_to_actor.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : "")} " +
                $"GROUP BY actor_info.ActorID " +
                "UNION " +
                $"{wrapper.Select(ActorSelectedField).toSelect(false)} FROM actor_info " +
                "WHERE NOT EXISTS(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) GROUP BY actor_info.ActorID " +
                $"{(search ? $"and actor_info.ActorName like '%{SearchText.ToProperSql()}%' " : "")} " +
                wrapper.toOrder() + ActorToLimit();
            // 只能手动设置页码，很奇怪
            App.Current.Dispatcher.Invoke(() => { main.actorPagination.Total = ActorTotalCount; });
            RenderCurrentActors(sql);
        }





        public void RenderCurrentActors(string sql)
        {
            List<Dictionary<string, object>> list = actorMapper.select(sql);
            List<ActorInfo> actors = actorMapper.toEntity<ActorInfo>(list, typeof(ActorInfo).GetProperties(), false);
            ActorList = new List<ActorInfo>();
            if (actors == null) actors = new List<ActorInfo>();
            ActorList.AddRange(actors);
            renderActor();

        }




        public async void renderActor()
        {
            if (CurrentActorList == null) CurrentActorList = new ObservableCollection<ActorInfo>();

            for (int i = 0; i < ActorList.Count; i++)
            {
                try { renderActorCT.ThrowIfCancellationRequested(); }
                catch (OperationCanceledException) { renderVideoCTS?.Dispose(); break; }
                renderingActor = true;
                ActorInfo actorInfo = ActorList[i];
                ActorInfo.SetImage(ref actorInfo);
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new LoadActorDelegate(LoadActor), actorInfo, i);
            }

            // 清除
            for (int i = CurrentActorList.Count - 1; i > ActorList.Count - 1; i--)
            {
                CurrentActorList.RemoveAt(i);
            }



            if (renderActorCT.IsCancellationRequested) refreshActorRenderToken();
            renderingActor = false;
            //if (pageQueue.Count > 0) pageQueue.Dequeue();
            ActorPageChangedCompleted?.Invoke(this, null);
        }


        #endregion

        public SelectWrapper<Video> getWrapper(SearchField searchType)
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            if (string.IsNullOrEmpty(SearchText)) return null;
            string FormatSearch = SearchText.ToProperSql().Trim();
            if (string.IsNullOrEmpty(FormatSearch)) return null;
            string searchContent = FormatSearch;

            switch (searchType)
            {
                case SearchField.VID:

                    string vid = Identify.GetVID(FormatSearch);
                    if (string.IsNullOrEmpty(vid)) searchContent = FormatSearch;
                    else searchContent = vid;
                    wrapper.Like("VID", searchContent);
                    break;
                //case SearchType.Title:
                //    wrapper.Like("Title", searchContent).LeftBacket().Or().Like("Path", searchContent).RightBacket();
                //break;
                default:
                    wrapper.Like(searchType.ToString(), searchContent);
                    break;
            }

            return wrapper;
        }


        public async Task<bool> Query(SearchField searchType = SearchField.VID)
        {
            extraWrapper = getWrapper(searchType);
            Select();
            return true;
        }

        public void RandomDisplay()
        {
            Select(true);
        }

        #region "影片" 




        public bool rendering { get; set; }
        public SelectWrapper<Video> extraWrapper { get; set; }
        public static List<string> SortDict = new List<string>()
        {
            "metadata_video.VID" ,
            "metadata.Grade" ,
            "metadata.Size" ,
            "metadata.LastScanDate" ,
            "metadata.FirstScanDate" ,
            "metadata.Title" ,
            "metadata.ViewCount" ,
            "metadata.ReleaseDate" ,
            "metadata.Rating" ,
            "metadata_video.Duration" ,
        };


        public static string[] SelectFields = {
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

        public void setSortOrder<T>(IWrapper<T> wrapper, bool random = false)
        {
            if (wrapper == null) return;
            int.TryParse(Properties.Settings.Default.SortType, out int sortindex);
            if (sortindex < 0 || sortindex >= SortDict.Count) sortindex = 0;
            string sortField = SortDict[sortindex];
            if (random)
                wrapper.Asc("RANDOM()");
            else
            {
                if (Properties.Settings.Default.SortDescending) wrapper.Desc(sortField);
                else wrapper.Asc(sortField);
            }


        }


        public void toLimit<T>(IWrapper<T> wrapper)
        {

            int row_count = PageSize;
            long offset = PageSize * (CurrentPage - 1);
            wrapper.Limit(offset, row_count);
        }

        public static Dictionary<string, string> SELECT_TYPE = new Dictionary<string, string>() {
            { "All","  "  },
            { "Favorite","  "  },
            { "RecentWatch","  "  },
        };






        public void GenerateSelect(object o = null)
        {
            extraWrapper = new SelectWrapper<Video>();
            // 侧边栏参数
            if (o != null && !string.IsNullOrEmpty(o.ToString()))
            {
                switch (o.ToString())
                {
                    case "Favorite":
                        extraWrapper.Gt("metadata.Grade", 0);
                        break;
                    case "RecentWatch":
                        DateTime date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays);
                        DateTime date2 = DateTime.Now;
                        extraWrapper.Between("ViewDate", DateHelper.toLocalDate(date1), DateHelper.toLocalDate(date2));
                        break;
                    default: break;
                }
            }
            main.pagination.CurrentPage = 1;
            ClickFilterType = string.Empty;
            ShowActorGrid = Visibility.Collapsed;
            Select();
        }

        public async void Select(bool random = false)
        {
            TabSelectedIndex = 0; // 影片
            // 判断当前获取的队列
            while (pageQueue.Count > 1)
            {
                int page = pageQueue.Dequeue();
                Console.WriteLine("跳过该页码 ： " + page);
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (rendering)
            {
                renderVideoCTS?.Cancel();// 取消加载
                await Task.Delay(100);
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ScrollViewer scrollViewer = main.FindVisualChild<ScrollViewer>(main.MovieItemsControl);
                scrollViewer.ScrollToTop();//滚到顶部
            });

            SelectWrapper<Video> wrapper = Video.InitWrapper();




            setSortOrder(wrapper, random);

            toLimit(wrapper);
            wrapper.Select(SelectFields);
            if (extraWrapper != null) wrapper.Join(extraWrapper);

            string sql = VideoMapper.BASE_SQL;


            // todo 如果搜索框选中了标签，搜索出来的结果不一致
            SearchField searchType = (SearchField)GlobalConfig.Main.SearchSelectedIndex;
            if (Searching)
            {
                if (searchType == SearchField.ActorName)
                    sql += VideoMapper.ACTOR_JOIN_SQL;
                else if (searchType == SearchField.LabelName)
                    sql += VideoMapper.LABEL_JOIN_SQL;
            }
            else if (!string.IsNullOrEmpty(ClickFilterType))
            {
                if (ClickFilterType == "Label")
                {
                    sql += VideoMapper.LABEL_JOIN_SQL;
                }
                else if (ClickFilterType == "Actor")
                {
                    sql += VideoMapper.ACTOR_JOIN_SQL;
                }
                else
                {

                }

            }


            // 标记
            bool allFalse = TagStamps.All(item => item.Selected == false);
            if (allFalse)
            {
                wrapper.IsNull("TagID");
                sql += VideoMapper.TAGSTAMP_LEFT_JOIN_SQL;
            }
            else
            {
                bool allTrue = TagStamps.All(item => item.Selected == true);
                if (!allTrue)
                {
                    wrapper.In("metadata_to_tagstamp.TagID", TagStamps.Where(item => item.Selected == true).Select(item => item.TagID.ToString()));
                    sql += VideoMapper.TAGSTAMP_JOIN_SQL;
                }
            }


            // 右侧菜单的一些筛选项

            // 1. 仅显示分段视频
            if (Properties.Settings.Default.OnlyShowSubSection)
                wrapper.NotEq("SubSection", "");

            // 2. 视频类型
            List<MenuItem> allMenus = main.VideoTypeMenuItem.Items.OfType<MenuItem>().ToList();
            List<MenuItem> checkedMenus = new List<MenuItem>();

            App.Current.Dispatcher.Invoke(() =>
            {
                checkedMenus = allMenus.Where(t => t.IsChecked).ToList();
            });

            if (checkedMenus.Count > 0 && checkedMenus.Count < 4)
            {
                // VideoType = 0 or VideoType = 1 or VideoType=2

                if (checkedMenus.Count == 1)
                {
                    int idx = allMenus.IndexOf(checkedMenus[0]);
                    wrapper.Eq("VideoType", idx);
                }
                else if (checkedMenus.Count == 2)
                {
                    int idx1 = allMenus.IndexOf(checkedMenus[0]);
                    int idx2 = allMenus.IndexOf(checkedMenus[1]);
                    wrapper.Eq("VideoType", idx1).LeftBacket().Or().Eq("VideoType", idx2).RightBacket();
                }
                else if (checkedMenus.Count == 3)
                {
                    int idx1 = allMenus.IndexOf(checkedMenus[0]);
                    int idx2 = allMenus.IndexOf(checkedMenus[1]);
                    int idx3 = allMenus.IndexOf(checkedMenus[2]);
                    wrapper.Eq("VideoType", idx1).LeftBacket().Or().Eq("VideoType", idx2).Or().Eq("VideoType", idx3).RightBacket();
                }
            }




            // 图片显示模式
            if (GlobalConfig.Settings.PictureIndexCreated && PictureTypeIndex > 0)
            {
                sql += VideoMapper.COMMON_PICTURE_EXIST_JOIN_SQL;
                long pathType = GlobalConfig.Settings.PicPathMode;
                int.TryParse(Properties.Settings.Default.ShowImageMode, out int imageType);
                if (imageType > 1) imageType = 0;
                wrapper.Eq("common_picture_exist.PathType", pathType).Eq("common_picture_exist.ImageType", imageType).Eq("common_picture_exist.Exist", PictureTypeIndex - 1);
            }
            // 是否可播放
            if (GlobalConfig.Settings.PlayableIndexCreated && DataExistIndex > 0)
                wrapper.Eq("metadata.PathExist", DataExistIndex - 1);


            string count_sql = "select count(DISTINCT metadata.DataID) " + sql + wrapper.toWhere(false);
            TotalCount = metaDataMapper.selectCount(count_sql);


            WrapperEventArg<Video> arg = new WrapperEventArg<Video>();
            arg.Wrapper = wrapper;
            arg.SQL = sql;
            RenderSqlChanged?.Invoke(null, arg);

            sql = wrapper.toSelect(false) + sql + wrapper.toWhere(false) + wrapper.toOrder() + wrapper.toLimit();
            // 只能手动设置页码，很奇怪
            App.Current.Dispatcher.Invoke(() => { main.pagination.Total = TotalCount; });
            RenderCurrentVideo(sql);
        }



        public void RenderCurrentVideo(string sql)
        {
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            List<Video> Videos = metaDataMapper.toEntity<Video>(list, typeof(Video).GetProperties(), false);

            VideoList = new List<Video>();
            if (Videos == null) Videos = new List<Video>();
            VideoList.AddRange(Videos);
            CurrentCount = VideoList.Count;
            render();

        }






        public async void render()
        {
            if (CurrentVideoList == null) CurrentVideoList = new ObservableCollection<Video>();
            int.TryParse(Properties.Settings.Default.ShowImageMode, out int imageMode);
            for (int i = 0; i < VideoList.Count; i++)
            {
                try { renderVideoCT.ThrowIfCancellationRequested(); }
                catch (OperationCanceledException) { renderVideoCTS?.Dispose(); break; }
                rendering = true;
                Video video = VideoList[i];
                if (video == null) continue;
                SetImage(ref video, imageMode);
                Video.setTagStamps(ref video);// 设置标签戳
                Video.handleEmpty(ref video);// 设置标题和发行日期
                // 设置关联
                HashSet<long> set = associationMapper.getAssociationDatas(video.DataID);
                if (set != null)
                {
                    video.HasAssociation = set.Count > 0;
                    video.AssociationList = set.ToList();
                }


                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadVideoDelegate(LoadVideo), video, i);
            }

            // 清除
            for (int i = CurrentVideoList.Count - 1; i > VideoList.Count - 1; i--)
            {
                CurrentVideoList.RemoveAt(i);
            }

            if (renderVideoCT.IsCancellationRequested) refreshVideoRenderToken();
            rendering = false;
            //if (pageQueue.Count > 0) pageQueue.Dequeue();
            PageChangedCompleted?.Invoke(this, null);
        }





        #endregion















        /// <summary>
        /// 统计：加载时间 <70ms (15620个信息)
        /// </summary>
        public void Statistic()
        {
            Task.Run(() =>
            {
                long dbid = GlobalConfig.Main.CurrentDBId;
                AllVideoCount = metaDataMapper.selectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0));
                appDatabaseMapper.updateFieldById("Count", AllVideoCount.ToString(), dbid);

                FavoriteVideoCount = metaDataMapper.selectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Gt("Grade", 0));


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


                AllActorCount = actorMapper.selectCount(actor_count_sql);

                string label_count_sql = "SELECT COUNT(DISTINCT LabelName) as Count  from metadata_to_label " +
                                        "join metadata on metadata_to_label.DataID=metadata.DataID " +
                                         $"WHERE metadata.DBId={dbid} and metadata.DataType={0} ";


                AllLabelCount = metaDataMapper.selectCount(label_count_sql);
                DateTime date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays);
                DateTime date2 = DateTime.Now;
                RecentWatchCount = metaDataMapper.selectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Between("ViewDate", DateHelper.toLocalDate(date1), DateHelper.toLocalDate(date2)));


            });
        }


        public void LoadViewAssoData(long dataID)
        {
            if (ViewAssociationDatas == null) ViewAssociationDatas = new ObservableCollection<Video>();
            ViewAssociationDatas.Clear();
            GC.Collect();
            Video currentVideo = CurrentVideoList.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (currentVideo.AssociationList == null || currentVideo.AssociationList.Count <= 0) return;
            SelectWrapper<Video> wrapper = Video.InitWrapper();
            wrapper.In("metadata.DataID", currentVideo.AssociationList.Select(arg => arg.ToString()));
            wrapper.Select(SelectFields);

            string sql = VideoMapper.BASE_SQL;

            sql = wrapper.toSelect(false) + sql + wrapper.toWhere(false);

            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            List<Video> Videos = metaDataMapper.toEntity<Video>(list, typeof(Video).GetProperties(), false);

            if (Videos == null) return;


            for (int i = 0; i < Videos.Count; i++)
            {
                Video video = Videos[i];
                if (video == null) continue;
                BitmapImage smallimage = ReadImageFromFile(video.getSmallImage());
                BitmapImage bigimage = ReadImageFromFile(video.getBigImage());
                if (smallimage == null) smallimage = DefaultSmallImage;
                if (bigimage == null) bigimage = smallimage;
                video.BigImage = bigimage;
                Video.setTagStamps(ref video);// 设置标签戳
                Video.handleEmpty(ref video);// 设置标题和发行日期


                if (GlobalConfig.Settings.AutoGenScreenShot)
                {
                    string path = video.getScreenShot();
                    if (Directory.Exists(path))
                    {
                        string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (array.Length > 0)
                        {

                            Video.SetImage(ref video, array[array.Length / 2]);
                            video.BigImage = null;
                            video.BigImage = video.ViewImage;
                        }
                    }
                }

                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new LoadViewAssoVideoDelegate(LoadViewAssoVideo), video, i);
            }

            // 清除
            for (int i = ViewAssociationDatas.Count - 1; i > Videos.Count - 1; i--)
            {
                ViewAssociationDatas.RemoveAt(i);
            }
        }

        public void LoadAssoMetaData()
        {


            string searchText = AssoSearchText.ToProperSql();

            if (AssociationDatas == null) AssociationDatas = new ObservableCollection<Video>();

            if (string.IsNullOrEmpty(searchText))
            {

                AssoSearchTotalCount = 0;
                return;
            }


            SelectWrapper<Video> wrapper = Video.InitWrapper();
            wrapper.Like("Title", searchText).LeftBacket()
                .Or().Like("Path", searchText)
                .Or().Like("VID", searchText)
                .RightBacket();

            toAssoSearchLimit(wrapper);
            wrapper.Select(SelectFields);

            string sql = VideoMapper.BASE_SQL;

            string count_sql = "select count(*) " + sql + wrapper.toWhere(false);
            AssoSearchTotalCount = metaDataMapper.selectCount(count_sql);
            main.assoPagination.Total = AssoSearchTotalCount;

            sql = wrapper.toSelect(false) + sql + wrapper.toWhere(false) + wrapper.toOrder() + wrapper.toLimit();

            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            List<Video> Videos = metaDataMapper.toEntity<Video>(list, typeof(Video).GetProperties(), false);

            if (Videos == null) return;



            for (int i = 0; i < Videos.Count; i++)
            {
                Video video = Videos[i];
                if (video == null) continue;
                BitmapImage smallimage = ReadImageFromFile(video.getSmallImage());
                BitmapImage bigimage = ReadImageFromFile(video.getBigImage());
                if (smallimage == null) smallimage = DefaultSmallImage;
                if (bigimage == null) bigimage = smallimage;
                video.BigImage = bigimage;
                Video.handleEmpty(ref video);// 设置标题和发行日期
                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new LoadAssoVideoDelegate(LoadAssoVideo), video, i);
            }

            // 清除
            for (int i = AssociationDatas.Count - 1; i > Videos.Count - 1; i--)
            {
                AssociationDatas.RemoveAt(i);
            }
            LoadAssoMetaDataCompleted?.Invoke(null, null);
        }

        public void toAssoSearchLimit<T>(IWrapper<T> wrapper)
        {

            int row_count = AssoSearchPageSize;
            long offset = AssoSearchPageSize * (CurrentAssoSearchPage - 1);
            wrapper.Limit(offset, row_count);
        }



        public void LoadExistAssociationDatas(long dataID)
        {
            ExistAssociationDatas = new ObservableCollection<Video>();
            CurrentExistAssoData = new List<Video>();
            // 遍历邻接表，找到所有关联的 id
            HashSet<long> set = associationMapper.getAssociationDatas(dataID);
            if (set?.Count > 0)
            {
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Select("VID", "metadata.DataID", "Title", "MVID").In("metadata.DataID", set.Select(x => x.ToString()));
                string sql = VideoMapper.BASE_SQL;
                sql = wrapper.toSelect(false) + sql + wrapper.toWhere(false);
                List<Dictionary<string, object>> list = metaDataMapper.select(sql);
                CurrentExistAssoData = metaDataMapper.toEntity<Video>(list, typeof(Video).GetProperties(), false);
                if (CurrentExistAssoData != null)
                    CurrentExistAssoData.ForEach(t => ExistAssociationDatas.Add(t));
                else
                    CurrentExistAssoData = new List<Video>();
            }
        }


        public List<long> SaveAssociation(long dataID)
        {
            List<Association> toInsert = new List<Association>();
            List<long> toDelete = new List<long>();// 删除比较特殊，要删除所有该 id 的
            List<long> dataList = new List<long>();
            if (ExistAssociationDatas != null && ExistAssociationDatas.Count > 0)
            {
                dataList = AssociationSelectedDatas.Union(ExistAssociationDatas).ToHashSet().Select(arg => arg.DataID).ToList();
                foreach (long id in dataList)
                    toInsert.Add(new Association(dataID, id));
            }
            else
            {
                if (AssociationSelectedDatas.Count > 0)
                {
                    foreach (Video item in AssociationSelectedDatas)
                        toInsert.Add(new Association(dataID, item.DataID));
                }
            }


            // 删除
            List<long> list = CurrentExistAssoData.Select(arg => arg.DataID).Except(dataList).ToList();
            foreach (long id in list)
                toDelete.Add(id);




            if (toInsert.Count > 0)
                associationMapper.insertBatch(toInsert, InsertMode.Ignore);
            if (toDelete.Count > 0)
            {
                foreach (long id in toDelete)
                {
                    string sql = $"delete from common_association where MainDataID='{id}' or SubDataID='{id}'";
                    associationMapper.executeNonQuery(sql);
                }
            }

            return toDelete;

        }
    }
}
