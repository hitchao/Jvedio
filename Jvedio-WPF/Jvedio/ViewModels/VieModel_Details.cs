using Jvedio.Core.Media;
using Jvedio.Core.UserControls.ViewModels;
using Jvedio.Entity;
using Jvedio.Mapper;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;
using static SuperUtils.Media.ImageHelper;

namespace Jvedio.ViewModel
{
    public class VieModel_Details : ViewModelBase
    {

        #region "事件"

        public event EventHandler QueryCompleted;
        private delegate void LoadLabelDelegate(string str);
        private void LoadLabel(string str) => CurrentLabelList.Add(str);

        private delegate void LoadViewAssocVideoDelegate(Video video, int idx);
        private void LoadViewAssocVideo(Video video, int idx) => ViewAssociationDatas.Add(video);


        #endregion


        #region "属性"

        private int _ScreenShotCount;
        public int ScreenShotCount {
            get {
                return _ScreenShotCount;
            }
            set {
                _ScreenShotCount = value;
                RaisePropertyChanged();
            }
        }
        private int _PreviewImageCount;
        public int PreviewImageCount {
            get {
                return _PreviewImageCount;
            }
            set {
                _PreviewImageCount = value;
                RaisePropertyChanged();
            }
        }
        private bool _LoadingVideoInfo;
        public bool LoadingVideoInfo {
            get {
                return _LoadingVideoInfo;
            }
            set {
                _LoadingVideoInfo = value;
                RaisePropertyChanged();
            }
        }

        private Window_Details WindowDetails { get; set; }

        public bool LoadingData { get; set; }
        private bool LoadingLabel { get; set; }

        private bool _TeenMode = ConfigManager.Settings.TeenMode;

        public bool TeenMode {
            get { return _TeenMode; }

            set {
                _TeenMode = value;
                RaisePropertyChanged();
            }
        }

        private int _SelectImageIndex = 0;

        public int SelectImageIndex {
            get { return _SelectImageIndex; }

            set {
                _SelectImageIndex = value;
                RaisePropertyChanged();
            }
        }

        private int _InfoSelectedIndex = (int)ConfigManager.Detail.InfoSelectedIndex;

        public int InfoSelectedIndex {
            get { return _InfoSelectedIndex; }

            set {
                _InfoSelectedIndex = value;
                if (value == 1 && VideoInfo == null)
                    LoadVideoInfo();
                RaisePropertyChanged();
            }
        }

        private bool _ShowScreenShot = ConfigManager.Detail.ShowScreenShot;

        public bool ShowScreenShot {
            get { return _ShowScreenShot; }

            set {
                _ShowScreenShot = value;
                RaisePropertyChanged();
            }
        }

        private Video _CurrentVideo;

        public Video CurrentVideo {
            get { return _CurrentVideo; }

            set {
                _CurrentVideo = value;
                RaisePropertyChanged();
            }
        }

        private VideoInfo _VideoInfo;

        public VideoInfo VideoInfo {
            get { return _VideoInfo; }

            set {
                _VideoInfo = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ActorInfo> _CurrentActorList = new ObservableCollection<ActorInfo>();

        public ObservableCollection<ActorInfo> CurrentActorList {
            get { return _CurrentActorList; }

            set {
                _CurrentActorList = value;
                RaisePropertyChanged();
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

        private ObservableCollection<string> _CurrentLabelList;

        public ObservableCollection<string> CurrentLabelList {
            get { return _CurrentLabelList; }

            set {
                _CurrentLabelList = value;
                RaisePropertyChanged();
            }
        }

        private string _LabelText = string.Empty;

        public string LabelText {
            get { return _LabelText; }

            set {
                _LabelText = value;
                RaisePropertyChanged();
                GetLabels();
            }
        }

        #endregion

        public VieModel_Details(Window_Details windowDetails)
        {
            this.WindowDetails = windowDetails;

            Init();
        }


        public override void Init()
        {

        }

        public async void LoadVideoInfo()
        {
            // 异步加载
            if (LoadingVideoInfo)
                return;

            await Task.Run(() => {
                LoadingVideoInfo = true;
                VideoInfo = Video.GetMediaInfo(CurrentVideo.Path);
                return true;
            });
            LoadingVideoInfo = false;
        }

        public void SaveLove()
        {
            bool ok = metaDataMapper.UpdateFieldById("Grade", CurrentVideo.Grade.ToString(), CurrentVideo.DataID);
            Logger.Info($"update data[{CurrentVideo.DataID}] grade[{CurrentVideo.Grade}] ret[{ok}] ");
        }

        public void Load(long dataID)
        {
            if (LoadingData)
                return;
            LoadingData = true;
            Logger.Info($"begin load data[{dataID}]");
            // 释放图片内存
            if (CurrentVideo != null) {
                CurrentVideo.SmallImage = null;
                CurrentVideo.BigImage = null;
                for (int i = 0; i < CurrentVideo.PreviewImageList.Count; i++) {
                    CurrentVideo.PreviewImageList[i] = null;
                }
            }

            if (CurrentActorList != null) {
                for (int i = 0; i < CurrentActorList.Count; i++) {
                    CurrentActorList[i].SmallImage = null;
                }
            }

            GC.Collect();

            WindowDetails.DataID = dataID;

            // todo 事务下导致阻塞
            metaDataMapper.IncreaseFieldById("ViewCount", dataID); // 访问次数+1
            Logger.Info($"view count ++");

            Video video = Video.GetById(dataID);
            if (video == null) {
                LoadingData = false;
                return;
            }

            CurrentVideo = video;
            // 磁力
            List<Magnet> magnets = magnetsMapper.SelectList(new SelectWrapper<Magnet>().Eq("DataID", dataID));
            if (magnets?.Count > 0) {
                try {
                    CurrentVideo.Magnets = magnets.OrderByDescending(arg => arg.Size)
                        .ThenByDescending(arg => arg.Releasedate)
                        .ThenByDescending(arg => string.Join(" ", arg.Tags).Length).ToList();
                    Logger.Info($"set magnets");
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            if (InfoSelectedIndex == 1)
                LoadVideoInfo();
            QueryCompleted?.Invoke(this, new EventArgs());
            Logger.Info($"load complete");
        }

        public void LoadViewAssocData()
        {
            if (ViewAssociationDatas == null)
                ViewAssociationDatas = new ObservableCollection<Video>();
            ViewAssociationDatas.Clear();
            GC.Collect();
            if (CurrentVideo.AssociationList == null || CurrentVideo.AssociationList.Count <= 0)
                return;
            SelectWrapper<Video> wrapper = Video.InitWrapper();
            wrapper.In("metadata.DataID", CurrentVideo.AssociationList.Select(arg => arg.ToString()));
            wrapper.Select(VieModel_VideoList.SelectFields);

            string sql = VideoMapper.SQL_BASE;

            sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false);

            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            List<Video> videos = metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

            if (videos == null)
                return;

            for (int i = 0; i < videos.Count; i++) {
                Video video = videos[i];
                if (video == null)
                    continue;
                BitmapImage smallimage = ImageCache.Get(video.GetSmallImage());
                BitmapImage bigimage = ImageCache.Get(video.GetBigImage());
                if (smallimage == null)
                    smallimage = MetaData.DefaultSmallImage;
                if (bigimage == null)
                    bigimage = smallimage;
                video.BigImage = bigimage;
                Video.SetTagStamps(ref video); // 设置标签戳
                Video.SetTitleAndDate(ref video); // 设置标题和发行日期

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
                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new LoadViewAssocVideoDelegate(LoadViewAssocVideo), video, i);
            }

            // 清除
            for (int i = ViewAssociationDatas.Count - 1; i > videos.Count - 1; i--) {
                ViewAssociationDatas.RemoveAt(i);
            }
        }


        public async void GetLabels()
        {
            if (LoadingLabel) {
                Logger.Warn("label is loading");
                return;
            }

            LoadingLabel = true;
            string like_sql = string.Empty;

            string search = LabelText.ToProperSql().Trim();
            if (!string.IsNullOrEmpty(search))
                like_sql = $" and LabelName like '%{search}%' ";

            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label " +
                "JOIN metadata on metadata.DataID=metadata_to_label.DataID " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}" + like_sql +
                $" GROUP BY LabelName ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            if (list != null) {
                foreach (Dictionary<string, object> item in list) {
                    if (!item.ContainsKey("LabelName") || !item.ContainsKey("Count") ||
                        item["LabelName"] == null || item["Count"] == null)
                        continue;
                    string labelName = item["LabelName"].ToString();
                    long.TryParse(item["Count"].ToString(), out long count);
                    labels.Add($"{labelName}({count})");
                }
            }

            CurrentLabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++) {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadLabel), labels[i]);
            }

            LoadingLabel = false;
        }

    }
}
