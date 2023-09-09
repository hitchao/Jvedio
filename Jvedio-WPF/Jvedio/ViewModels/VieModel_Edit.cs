using Jvedio.Entity;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.WPF.VieModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.ViewModel
{
    class VieModel_Edit : ViewModelBase
    {

        #region "事件"
        private delegate void LoadLabelDelegate(string str);

        private void LoadLabel(string str) => CurrentLabelList.Add(str);
        #endregion

        #region "属性"
        private Window_Edit WindowEdit { get; set; }

        private List<string> OldLabels { get; set; }

        private bool LoadingLabel { get; set; }

        private bool _Saving;

        public bool Saving {
            get { return _Saving; }

            set {
                _Saving = value;
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

        private bool _MoreExpanded = ConfigManager.Edit.MoreExpanded;

        public bool MoreExpanded {
            get { return _MoreExpanded; }

            set {
                _MoreExpanded = value;
                RaisePropertyChanged();
            }
        }

        private long _DataID;

        public long DataID {
            get { return _DataID; }

            set {
                _DataID = value;
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

        private ObservableCollection<ActorInfo> _ViewActors;

        /// <summary>
        /// 用户可见的 ActorLIst
        /// </summary>
        public ObservableCollection<ActorInfo> ViewActors {
            get { return _ViewActors; }

            set {
                _ViewActors = value;
                RaisePropertyChanged();
            }
        }

        private int _ActorPageSize = 10;

        public int ActorPageSize {
            get { return _ActorPageSize; }

            set {
                _ActorPageSize = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentActorCount = 0;

        public int CurrentActorCount {
            get { return _CurrentActorCount; }

            set {
                _CurrentActorCount = value;
                RaisePropertyChanged();
            }
        }

        private long _ActorTotalCount = 0;

        public long ActorTotalCount {
            get { return _ActorTotalCount; }

            set {
                _ActorTotalCount = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentActorPage = 1;

        public int CurrentActorPage {
            get { return _CurrentActorPage; }

            set {
                _CurrentActorPage = value;
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

        private string _ActorName = string.Empty;

        public string ActorName {
            get { return _ActorName; }

            set {
                _ActorName = value;
                RaisePropertyChanged();
            }
        }

        private long _ActorID;

        public long ActorID {
            get { return _ActorID; }

            set {
                _ActorID = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _CurrentImage;

        public BitmapSource CurrentImage {
            get { return _CurrentImage; }

            set {
                _CurrentImage = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        public VieModel_Edit(long dataId, Window_Edit windowEdit)
        {
            WindowEdit = windowEdit;
            if (dataId <= 0) {
                Logger.Error("data id must > 0");
                return;
            }
            DataID = dataId;
            Init();
        }

        public override void Init()
        {
            CurrentVideo = null;
            CurrentVideo = MapperManager.videoMapper.SelectVideoByID(DataID);
            OldLabels = CurrentVideo.LabelList?.Select(arg => arg.Value).ToList();
            ViewActors = new ObservableCollection<ActorInfo>();
            foreach (ActorInfo info in CurrentVideo.ActorInfos)
                ViewActors.Add(info);

            GetLabels();

            Logger.Info("init ok");
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

        public async Task<bool> Save()
        {
            return await Task.Run(() => {
                if (CurrentVideo == null)
                    return false;
                MetaData data = CurrentVideo.toMetaData();
                if (data == null)
                    return false;

                data.DataID = DataID;
                int update1 = MapperManager.metaDataMapper.UpdateById(data);
                int update2 = MapperManager.videoMapper.UpdateById(CurrentVideo);

                Logger.Info($"save metadata ret[{update1}], video ret[{update2}]");

                // 标签
                MapperManager.metaDataMapper.SaveLabel(data);
                Logger.Info("save label");

                // 演员
                MapperManager.videoMapper.SaveActor(CurrentVideo, ViewActors.ToList());
                Logger.Info("save actors");

                return update1 > 0 & update2 > 0;
            });
        }



    }
}
