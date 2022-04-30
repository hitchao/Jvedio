using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.GlobalMapper;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using static Jvedio.ImageProcess;
using static Jvedio.Utils.CustomExtension;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using Jvedio.Entity;
using Jvedio.Core.SimpleORM;
using System.Windows;
using System.Windows.Threading;
using Jvedio.Core.Enums;

namespace Jvedio.ViewModel
{
    class VieModel_Edit : ViewModelBase
    {

        WindowEdit windowEdit = GetWindowByName("WindowEdit") as WindowEdit;

        public Video _CurrentVideo;

        public Video CurrentVideo
        {
            get { return _CurrentVideo; }
            set
            {
                _CurrentVideo = value;
                RaisePropertyChanged();
            }
        }


        public bool _MoreExpanded = GlobalConfig.Edit.MoreExpanded;

        public bool MoreExpanded
        {
            get { return _MoreExpanded; }
            set
            {
                _MoreExpanded = value;
                RaisePropertyChanged();
            }
        }


        public long _DataID;

        public long DataID
        {
            get { return _DataID; }
            set
            {
                _DataID = value;
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


        private ObservableCollection<string> _CurrentLabelList;


        public ObservableCollection<string> CurrentLabelList
        {
            get { return _CurrentLabelList; }
            set
            {
                _CurrentLabelList = value;
                RaisePropertyChanged();
            }
        }





        private ObservableCollection<ActorInfo> _ViewActors;


        /// <summary>
        /// 用户可见的 ActorLIst
        /// </summary>
        public ObservableCollection<ActorInfo> ViewActors
        {
            get { return _ViewActors; }
            set
            {
                _ViewActors = value;
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


        private string _SearchText = String.Empty;
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                RaisePropertyChanged();
                SelectActor();
            }
        }

        private string _LabelText = String.Empty;
        public string LabelText
        {
            get { return _LabelText; }
            set
            {
                _LabelText = value;
                RaisePropertyChanged();
                getLabels();
            }
        }

        private string _ActorName = String.Empty;
        public string ActorName
        {
            get { return _ActorName; }
            set
            {
                _ActorName = value;
                RaisePropertyChanged();
            }
        }

        private long _ActorID;
        public long ActorID
        {
            get { return _ActorID; }
            set
            {
                _ActorID = value;
                RaisePropertyChanged();
            }
        }


        private BitmapSource _CurrentImage;
        public BitmapSource CurrentImage
        {
            get { return _CurrentImage; }
            set
            {
                _CurrentImage = value;
                RaisePropertyChanged();
            }
        }





        public VieModel_Edit(long dataid)
        {

            if (dataid <= 0) return;
            DataID = dataid;
            Reset();
        }


        private List<string> oldLabels;

        public void Reset()
        {
            CurrentVideo = null;
            CurrentVideo = GlobalMapper.videoMapper.SelectVideoByID(DataID);
            oldLabels = CurrentVideo.LabelList?.Select(arg => arg).ToList();

            ViewActors = new ObservableCollection<ActorInfo>();
            foreach (ActorInfo info in CurrentVideo.ActorInfos)
            {
                ViewActors.Add(info);
            }
            getLabels();
        }


        private bool loadingLabel = false;
        public async void getLabels()
        {
            if (loadingLabel) return;
            loadingLabel = true;
            string like_sql = "";

            string search = LabelText.ToProperSql().Trim();
            if (!string.IsNullOrEmpty(search))
                like_sql = $" and LabelName like '%{search}%' ";


            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label " +
                "JOIN metadata on metadata.DataID=metadata_to_label.DataID " +
                $"where metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0}" + like_sql +
                $" GROUP BY LabelName ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            foreach (Dictionary<string, object> item in list)
            {
                string LabelName = item["LabelName"].ToString();
                long.TryParse(item["Count"].ToString(), out long count);
                labels.Add($"{LabelName}({count})");
            }
            CurrentLabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadLabel), labels[i]);
            }
            loadingLabel = false;
        }

        private delegate void LoadLabelDelegate(string str);
        private void LoadLabel(string str) => CurrentLabelList.Add(str);

        // todo 演员
        public bool Save()
        {
            if (CurrentVideo == null) return false;
            MetaData data = CurrentVideo.toMetaData();
            data.DataID = DataID;
            int update1 = GlobalMapper.metaDataMapper.updateById(data);
            int update2 = GlobalMapper.videoMapper.updateById(CurrentVideo);

            // 标签
            GlobalMapper.metaDataMapper.SaveLabel(data, oldLabels);

            // 演员
            GlobalMapper.videoMapper.SaveActor(CurrentVideo, ViewActors.ToList());



            return update1 > 0 & update2 > 0;

        }

        private delegate void LoadActorDelegate(ActorInfo actor, int idx);
        private void LoadActor(ActorInfo actor, int idx)
        {
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


        public async void SelectActor()
        {
            string like_sql = "";

            string search = SearchText.ToProperSql().Trim();
            if (!string.IsNullOrEmpty(search))
                like_sql = $" and ActorName like '%{search}%' ";

            string count_sql = "SELECT count(*) as Count " +
                         "from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                         "on metadata_to_actor.ActorID=actor_info.ActorID " +
                         "join metadata " +
                         "on metadata_to_actor.DataID=metadata.DataID " +
                         $"WHERE metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                         like_sql + "GROUP BY actor_info.ActorID );";

            ActorTotalCount = actorMapper.selectCount(count_sql);
            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            string sql = $"{wrapper.Select(VieModel_Main.ActorSelectedField).toSelect(false)} FROM actor_info " +
                $"join metadata_to_actor on metadata_to_actor.ActorID=actor_info.ActorID " +
                $"join metadata on metadata_to_actor.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " + like_sql +
                $"GROUP BY actor_info.ActorID ORDER BY Count DESC"
                + ActorToLimit();
            // 只能手动设置页码，很奇怪
            App.Current.Dispatcher.Invoke(() => { windowEdit.actorPagination.Total = ActorTotalCount; });

            List<Dictionary<string, object>> list = actorMapper.select(sql);
            List<ActorInfo> actors = actorMapper.toEntity<ActorInfo>(list, typeof(ActorInfo).GetProperties(), false);
            ActorList = new List<ActorInfo>();
            if (actors == null) actors = new List<ActorInfo>();
            ActorList.AddRange(actors);

            if (CurrentActorList == null) CurrentActorList = new ObservableCollection<ActorInfo>();
            for (int i = 0; i < ActorList.Count; i++)
            {
                ActorInfo actorInfo = ActorList[i];
                //加载图片
                PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
                BitmapImage smallimage = null;
                if (pathType != PathType.RelativeToData)
                {
                    // 如果是相对于影片格式的，则不设置图片
                    string smallImagePath = actorInfo.getImagePath();
                    smallimage = ImageProcess.ReadImageFromFile(smallImagePath);
                }
                if (smallimage == null) smallimage = DefaultActorImage;
                actorInfo.SmallImage = smallimage;
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo, i);
            }

            // 清除
            for (int i = CurrentActorList.Count - 1; i > ActorList.Count - 1; i--)
            {
                CurrentActorList.RemoveAt(i);
            }
        }

        public string ActorToLimit()
        {

            int row_count = ActorPageSize;
            long offset = ActorPageSize * (CurrentActorPage - 1);
            return $" LIMIT {offset},{row_count}";
        }
    }
}
