using Jvedio.Core.Media;
using Jvedio.Core.UserControls.ViewModels;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.WPF.VieModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.ViewModel
{
    public class VieModel_SearchAsso : ViewModelBase
    {

        #region "事件"

        private delegate void LoadViewAssoVideoDelegate(Video video, int idx);

        private void LoadViewAssoVideo(Video video, int idx) => ViewAssociationDatas.Add(video);


        #endregion

        #region "静态属性"
        private static Window_SearchAsso Window { get; set; }

        #endregion

        #region "属性"
        public List<Video> SelectedVideo { get; set; }

        private List<Video> CurrentExistAssocData { get; set; }



        public ObservableCollection<Video> _ViewAssociationDatas;

        public ObservableCollection<Video> ViewAssociationDatas {
            get { return _ViewAssociationDatas; }

            set {
                _ViewAssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        private long _AssocSearchTotalCount;

        public long AssoSearchTotalCount {
            get { return _AssocSearchTotalCount; }

            set {
                _AssocSearchTotalCount = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Video> _AssociationDatas;

        /// <summary>
        /// 影片关联
        /// </summary>
        public ObservableCollection<Video> AssociationDatas {
            get { return _AssociationDatas; }

            set {
                _AssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        private int _AssocSearchPageSize = 20;

        public int AssoSearchPageSize {
            get { return _AssocSearchPageSize; }

            set {
                _AssocSearchPageSize = value;
                RaisePropertyChanged();
            }
        }


        private string _AssoSearchText = string.Empty;

        public string AssoSearchText {
            get { return _AssoSearchText; }

            set {
                _AssoSearchText = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentAssoSearchPage = 1;

        public int CurrentAssoSearchPage {
            get { return _CurrentAssoSearchPage; }

            set {
                _CurrentAssoSearchPage = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<Video> _AssociationSelectedDatas = new ObservableCollection<Video>();

        public ObservableCollection<Video> AssociationSelectedDatas {
            get { return _AssociationSelectedDatas; }

            set {
                _AssociationSelectedDatas = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Video> _ExistAssociationDatas = new ObservableCollection<Video>();

        public ObservableCollection<Video> ExistAssociationDatas {
            get { return _ExistAssociationDatas; }

            set {
                _ExistAssociationDatas = value;
                RaisePropertyChanged();
            }
        }

        private delegate void LoadAssoVideoDelegate(Video video, int idx);


        #endregion


        public VieModel_SearchAsso(Window_SearchAsso window)
        {
            Window = window;
            Init();
        }

        public override void Init()
        {
            SelectedVideo = new List<Video>();
            CurrentExistAssocData = new List<Video>();
        }
        private void LoadAssoVideo(Video video, int idx)
        {
            if (AssociationDatas.Count < Window.assoPagination.PageSize) {
                if (idx < AssociationDatas.Count) {
                    AssociationDatas[idx] = null;
                    AssociationDatas[idx] = video;
                } else {
                    AssociationDatas.Add(video);
                }
            } else {
                AssociationDatas[idx] = null;
                AssociationDatas[idx] = video;
            }
        }

        public void ToAssoSearchLimit<T>(IWrapper<T> wrapper)
        {
            int row_count = AssoSearchPageSize;
            long offset = AssoSearchPageSize * (CurrentAssoSearchPage - 1);
            wrapper.Limit(offset, row_count);
        }


        public void LoadAssoMetaData()
        {
            string searchText = AssoSearchText.ToProperSql();

            if (AssociationDatas == null)
                AssociationDatas = new ObservableCollection<Video>();

            if (string.IsNullOrEmpty(searchText)) {
                AssoSearchTotalCount = 0;
                return;
            }

            SelectWrapper<Video> wrapper = Video.InitWrapper();
            wrapper.Like("Title", searchText).LeftBracket()
                .Or().Like("Path", searchText)
                .Or().Like("VID", searchText)
                .RightBracket();

            ToAssoSearchLimit(wrapper);
            wrapper.Select(VieModel_VideoList.SelectFields);
            string sql = VideoMapper.SQL_BASE;

            string count_sql = "select count(*) " + sql + wrapper.ToWhere(false);
            AssoSearchTotalCount = metaDataMapper.SelectCount(count_sql);
            Window.assoPagination.Total = AssoSearchTotalCount;

            sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false) + wrapper.ToOrder() + wrapper.ToLimit();

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
                    smallimage = Jvedio.Entity.MetaData.DefaultSmallImage;
                if (bigimage == null)
                    bigimage = smallimage;
                video.BigImage = bigimage;
                Video.SetTitleAndDate(ref video); // 设置标题和发行日期
                Logger.Info($"load assoc video: {video.VID}");
                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new LoadAssoVideoDelegate(LoadAssoVideo), video, i);
            }

            // 清除
            for (int i = AssociationDatas.Count - 1; i > videos.Count - 1; i--) {
                AssociationDatas.RemoveAt(i);
            }
        }

        public void LoadExistAssociationDatas(long dataID)
        {
            ExistAssociationDatas = new ObservableCollection<Video>();
            CurrentExistAssocData = new List<Video>();

            // 遍历邻接表，找到所有关联的 id
            HashSet<long> set = associationMapper.GetAssociationDatas(dataID);
            if (set?.Count > 0) {
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();

                wrapper.Select("VID", "metadata.DataID", "Title", "MVID", "Path")
                    .In("metadata.DataID", set.Select(x => x.ToString()));

                string sql = VideoMapper.SQL_BASE;
                sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false);
                List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
                CurrentExistAssocData =
                    metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

                if (CurrentExistAssocData == null)
                    return;

                for (int i = 0; i < CurrentExistAssocData.Count; i++) {
                    Video video = CurrentExistAssocData[i];
                    if (video == null)
                        continue;
                    Logger.Info($"load exist assoc data: {video.VID}");
                    Video.SetTitleAndDate(ref video); // 设置标题和发行日期
                }

                if (CurrentExistAssocData != null)
                    CurrentExistAssocData.ForEach(t => ExistAssociationDatas.Add(t));
                else
                    CurrentExistAssocData = new List<Video>();
            }
        }


        /// <summary>
        /// 保存关联关系，并返回被删除的关联
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public List<long> SaveAssociation(long dataID)
        {
            List<Association> toInsert = new List<Association>();
            List<long> toDelete = new List<long>(); // 删除比较特殊，要删除所有该 id 的
            List<long> dataList = new List<long>();
            if (ExistAssociationDatas != null && ExistAssociationDatas.Count > 0) {
                dataList = AssociationSelectedDatas.Union(ExistAssociationDatas).ToHashSet().Select(arg => arg.DataID).ToList();
                foreach (long id in dataList)
                    toInsert.Add(new Association(dataID, id));
            } else {
                if (AssociationSelectedDatas.Count > 0) {
                    foreach (Video item in AssociationSelectedDatas)
                        toInsert.Add(new Association(dataID, item.DataID));
                }
            }

            // 删除
            List<long> list = CurrentExistAssocData.Select(arg => arg.DataID).Except(dataList).ToList();
            foreach (long id in list)
                toDelete.Add(id);

            if (toInsert.Count > 0)
                associationMapper.InsertBatch(toInsert, InsertMode.Ignore);
            if (toDelete.Count > 0) {
                foreach (long id in toDelete) {
                    string sql = $"delete from common_association where MainDataID='{id}' or SubDataID='{id}'";
                    associationMapper.ExecuteNonQuery(sql);
                    Logger.Info($"delete assoc id[{id}]");
                }
            }

            return toDelete;
        }
    }
}
