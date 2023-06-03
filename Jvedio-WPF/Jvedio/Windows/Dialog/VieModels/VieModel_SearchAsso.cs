using Jvedio.Core;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Plugins;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using SuperControls.Style.Plugin;
using SuperUtils.Common;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static SuperUtils.WPF.VisualTools.WindowHelper;
using static Jvedio.LogManager;
using Jvedio.Entity.Common;
using System.ComponentModel;
using Jvedio.Core.WindowConfig;
using Jvedio.Mapper;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;
using Jvedio.Core.Media;
using SuperControls.Style;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.MapperManager;
using static SuperUtils.Media.ImageHelper;
using Jvedio.Entity.CommonSQL;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.IO;
using System.IO;

namespace Jvedio.ViewModel
{
    public class VieModel_SearchAsso : ViewModelBase
    {
        private static Window_SearchAsso window { get; set; }

        public VieModel_SearchAsso(Window_SearchAsso win)
        {
            window = win;
            SelectedVideo = new List<Video>();
            CurrentExistAssocData = new List<Video>();

        }

        public Video CurrentVideo { get; set; }// 当前正在关联的影片的 dataID


        public List<Video> SelectedVideo { get; set; }


        public long _AssocSearchTotalCount;

        public long AssoSearchTotalCount
        {
            get { return _AssocSearchTotalCount; }

            set
            {
                _AssocSearchTotalCount = value;
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

        public int _AssocSearchPageSize = 20;

        public int AssoSearchPageSize
        {
            get { return _AssocSearchPageSize; }

            set
            {
                _AssocSearchPageSize = value;
                RaisePropertyChanged();
            }
        }





        private string _AssoSearchText = string.Empty;

        public string AssoSearchText
        {
            get { return _AssoSearchText; }

            set
            {
                _AssoSearchText = value;
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

        private List<Video> CurrentExistAssocData { get; set; }

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

        private delegate void LoadAssoVideoDelegate(Video video, int idx);

        private void LoadAssoVideo(Video video, int idx)
        {
            if (AssociationDatas.Count < window.assoPagination.PageSize)
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

            if (string.IsNullOrEmpty(searchText))
            {
                AssoSearchTotalCount = 0;
                return;
            }

            SelectWrapper<Video> wrapper = Video.InitWrapper();
            wrapper.Like("Title", searchText).LeftBracket()
                .Or().Like("Path", searchText)
                .Or().Like("VID", searchText)
                .RightBracket();

            ToAssoSearchLimit(wrapper);
            wrapper.Select(VieModel_Main.SelectFields);
            string sql = VideoMapper.BASE_SQL;

            string count_sql = "select count(*) " + sql + wrapper.ToWhere(false);
            AssoSearchTotalCount = metaDataMapper.SelectCount(count_sql);
            window.assoPagination.Total = AssoSearchTotalCount;

            sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false) + wrapper.ToOrder() + wrapper.ToLimit();

            List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
            List<Video> videos = metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

            if (videos == null) return;

            for (int i = 0; i < videos.Count; i++)
            {
                Video video = videos[i];
                if (video == null) continue;
                BitmapImage smallimage = ReadImageFromFile(video.GetSmallImage(), Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
                BitmapImage bigimage = ReadImageFromFile(video.GetBigImage(), Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
                if (smallimage == null) smallimage = Jvedio.Entity.MetaData.DefaultSmallImage;
                if (bigimage == null) bigimage = smallimage;
                video.BigImage = bigimage;
                Video.HandleEmpty(ref video); // 设置标题和发行日期
                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new LoadAssoVideoDelegate(LoadAssoVideo), video, i);
            }

            // 清除
            for (int i = AssociationDatas.Count - 1; i > videos.Count - 1; i--)
            {
                AssociationDatas.RemoveAt(i);
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

        private delegate void LoadViewAssoVideoDelegate(Video video, int idx);

        private void LoadViewAssoVideo(Video video, int idx) => ViewAssociationDatas.Add(video);



        public void LoadExistAssociationDatas(long dataID)
        {
            ExistAssociationDatas = new ObservableCollection<Video>();
            CurrentExistAssocData = new List<Video>();

            // 遍历邻接表，找到所有关联的 id
            HashSet<long> set = associationMapper.GetAssociationDatas(dataID);
            if (set?.Count > 0)
            {
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Select("VID", "metadata.DataID", "Title", "MVID", "Path").In("metadata.DataID", set.Select(x => x.ToString()));
                string sql = VideoMapper.BASE_SQL;
                sql = wrapper.ToSelect(false) + sql + wrapper.ToWhere(false);
                List<Dictionary<string, object>> list = metaDataMapper.Select(sql);
                CurrentExistAssocData = metaDataMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);

                if (CurrentExistAssocData == null) return;

                for (int i = 0; i < CurrentExistAssocData.Count; i++)
                {
                    Video video = CurrentExistAssocData[i];
                    if (video == null)
                        continue;
                    Video.HandleEmpty(ref video); // 设置标题和发行日期
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
            List<long> list = CurrentExistAssocData.Select(arg => arg.DataID).Except(dataList).ToList();
            foreach (long id in list)
                toDelete.Add(id);

            if (toInsert.Count > 0)
                associationMapper.InsertBatch(toInsert, InsertMode.Ignore);
            if (toDelete.Count > 0)
            {
                foreach (long id in toDelete)
                {
                    string sql = $"delete from common_association where MainDataID='{id}' or SubDataID='{id}'";
                    associationMapper.ExecuteNonQuery(sql);
                }
            }

            return toDelete;
        }


        public bool SaveAssociations(List<Video> videoList)
        {
            if (videoList.Count == 1)
                return false;
            List<Association> toInsert = new List<Association>();
            long baseId = videoList[0].DataID;
            foreach (Video item in videoList)
                toInsert.Add(new Association(baseId, item.DataID));
            int count = associationMapper.InsertBatch(toInsert, InsertMode.Ignore);
            return count > 0;
        }





    }
}
