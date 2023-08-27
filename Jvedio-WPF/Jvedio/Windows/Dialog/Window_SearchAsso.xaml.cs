using Jvedio.Core.Media;
using Jvedio.Entity;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.App;
using static SuperUtils.WPF.VisualTools.VisualHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_SearchAsso.xaml 的交互逻辑
    /// </summary>
    public partial class Window_SearchAsso : BaseDialog
    {

        #region "事件"
        public Action<long> OnDataRefresh;
        public Action OnSelectData;
        #endregion

        #region "静态属性"
        public static string SearchText { get; set; } = "";

        #endregion

        #region "属性"
        private VieModel_SearchAsso vieModel { get; set; }

        /// <summary>
        /// 当前正在关联的影片的 dataID
        /// </summary>
        private long CurrentAssocDataID { get; set; }

        #endregion

        public Window_SearchAsso(List<Video> videos = null)
        {
            InitializeComponent();

            vieModel = new VieModel_SearchAsso(this);
            this.DataContext = vieModel;


            Init(videos);
        }

        public void Init(List<Video> videos)
        {
            if (vieModel == null)
                return;

            if (videos != null)
                vieModel.SelectedVideo = videos;

            CurrentAssocDataID = vieModel.SelectedVideo[0].DataID;
            vieModel.LoadExistAssociationDatas(vieModel.SelectedVideo[0].DataID);

            // 将剩余的作为需要关联的
            for (int i = 1; i < vieModel.SelectedVideo.Count; i++) {
                Video video = vieModel.SelectedVideo[i];
                if (!vieModel.ExistAssociationDatas.Contains(video))
                    vieModel.AssociationSelectedDatas.Add(vieModel.SelectedVideo[i]);
            }


            Logger.Info("open window search assoc");
        }



        private void searchDataBox_Search(object sender, RoutedEventArgs e)
        {
            SearchBox box = sender as SearchBox;
            if (!box.IsLoaded)
                return;

            string searchText = box.Text;
            vieModel.AssoSearchText = searchText;
            vieModel.LoadAssoMetaData();
            SearchText = vieModel.AssoSearchText;
        }


        private void RemoveAssociation(object sender, RoutedEventArgs e)
        {
            Grid grid = (sender as FrameworkElement).Parent as Grid;
            if (grid == null || grid.Tag == null)
                return;
            long.TryParse(grid.Tag.ToString(), out long dataID);
            if (dataID <= 0)
                return;
            Logger.Info($"remove assoc by id[{dataID}]");
            Video video = vieModel.AssociationSelectedDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (video != null) {
                vieModel.AssociationSelectedDatas.Remove(video);
                SetAssocSelected();
            }
        }

        private void RemoveExistAssociation(object sender, RoutedEventArgs e)
        {
            Grid grid = (sender as FrameworkElement).Parent as Grid;
            if (grid == null || grid.Tag == null)
                return;
            long.TryParse(grid.Tag.ToString(), out long dataID);
            if (dataID <= 0)
                return;
            Logger.Info($"remove exist assoc by id[{dataID}]");
            Video video = vieModel.ExistAssociationDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (video != null)
                vieModel.ExistAssociationDatas.Remove(video);
        }


        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            if (CurrentAssocDataID <= 0)
                return;
            List<long> toDelete = vieModel.SaveAssociation(CurrentAssocDataID);

            // 刷新关联的影片
            HashSet<long> set = MapperManager.associationMapper.GetAssociationDatas(CurrentAssocDataID);
            set.Add(CurrentAssocDataID);
            foreach (var item in toDelete)
                set.Add(item);
            foreach (var item in set) {
                OnDataRefresh?.Invoke(item);
            }
            base.Confirm(sender, e);
        }

        private void AssoSearchPageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.AssoSearchPageSize = pagination.PageSize;
            vieModel.LoadAssoMetaData();
        }

        private void AssoSearchPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentAssoSearchPage = pagination.CurrentPage;
            vieModel.LoadAssoMetaData();
        }

        private void AddToAssociation(object sender, MouseButtonEventArgs e)
        {
            long dataID = GetDataID(sender as FrameworkElement);
            Video video = vieModel.AssociationDatas.Where(arg => arg.DataID.Equals(dataID)).FirstOrDefault();
            if (vieModel.ExistAssociationDatas.Contains(video) || dataID.Equals(CurrentAssocDataID))
                return;
            if (!vieModel.AssociationSelectedDatas.Contains(video))
                vieModel.AssociationSelectedDatas.Add(video);
            else
                vieModel.AssociationSelectedDatas.Remove(video);
            SetAssocSelected();
        }

        private void SetAssocSelected()
        {
            ItemsControl itemsControl = assoSearchItemsControl;
            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null)
                    continue;
                Border border = FindElementByName<Border>(presenter, "rootBorder");
                if (border == null)
                    continue;
                long dataID = GetDataID(border);
                border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                border.BorderBrush = Brushes.Transparent;
                if (dataID > 0 && vieModel.AssociationSelectedDatas?.Count > 0) {
                    if (vieModel.AssociationSelectedDatas.Where(arg => arg.DataID == dataID).Any()) {
                        border.Background = StyleManager.Common.HighLight.Background;
                        border.BorderBrush = StyleManager.Common.HighLight.BorderBrush;
                    }
                }
            }
        }

        public void AssoBorderMouseEnter(object sender, MouseEventArgs e)
        {
            GifImage image = sender as GifImage;
            SimplePanel grid = image.FindParentOfType<SimplePanel>("rootGrid");
            if (grid == null || grid.Children.Count <= 0)
                return;
            Border border = grid.Children[0] as Border;
            if (border != null)
                border.BorderBrush = StyleManager.Common.HighLight.BorderBrush;
        }

        private long GetDataID(UIElement o, bool findParent = true)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element == null)
                return -1;

            FrameworkElement target = element;
            if (findParent)
                target = element.FindParentOfType<SimplePanel>("rootGrid");

            if (target != null &&
                target.Tag != null &&
                target.Tag.ToString() is string tag &&
                long.TryParse(target.Tag.ToString(), out long id))
                return id;

            return -1;
        }

        public void AssoBorderMouseLeave(object sender, MouseEventArgs e)
        {
            GifImage image = sender as GifImage;
            if (image == null)
                return;
            long dataID = GetDataID(image);
            SimplePanel grid = image.FindParentOfType<SimplePanel>("rootGrid");
            if (grid == null || grid.Children.Count <= 0)
                return;
            Border border = grid.Children[0] as Border;
            if (border == null || vieModel.AssociationSelectedDatas == null)
                return;
            if (vieModel.AssociationSelectedDatas.Where(arg => arg.DataID == dataID).Any())
                border.BorderBrush = StyleManager.Common.HighLight.BorderBrush;
            else
                border.BorderBrush = Brushes.Transparent;
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            vieModel.AssoSearchText = SearchText;
            searchDataBox.Text = SearchText;
            vieModel.LoadAssoMetaData();
        }
    }
}
