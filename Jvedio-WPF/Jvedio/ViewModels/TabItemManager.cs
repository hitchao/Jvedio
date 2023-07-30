using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Jvedio.ViewModels
{

    public class TabItemManager
    {

        private TabItemManager()
        {
        }

        private static TabItemManager instance { get; set; }

        private VieModel_Main vieModel { get; set; }
        private SimplePanel TabPanel { get; set; }

        public static TabItemManager CreateInstance(VieModel_Main vieModel, SimplePanel tabPanel)
        {
            if (instance == null) {
                instance = new TabItemManager();
                instance.vieModel = vieModel;
                instance.TabPanel = tabPanel;
            }
            return instance;
        }

        public void Add(TabType type, string tabName, object tabData)
        {
            if (vieModel == null) {
                return;
            }

            if (vieModel.TabItems == null)
                vieModel.TabItems = new ObservableCollection<TabItemEx>();

            TabItemEx tabItem = vieModel.TabItems.FirstOrDefault(arg => arg.Name.Equals(tabName));

            if (tabItem != null) {
                // 移除
                RemovePanel(tabItem);
                RemoveTabItem(vieModel.TabItems.IndexOf(tabItem));
            }


            tabItem = new TabItemEx(tabName, type);
            vieModel.TabItems.Add(tabItem);


            int idx = -1;
            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                if (vieModel.TabItems[i].Name.Equals(tabName)) {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
                SetTabSelected(idx);

            onAddData(tabItem, tabData);
        }

        private void OnItemClick(object sender, VideoItemEventArgs e)
        {
            long dataID = e.DataID;
            onShowDetailData(dataID);
        }

        public void onShowDetailData(long dataID)
        {
            Window_Details windowDetails = new Window_Details(dataID);
            windowDetails.Show();
        }

        private void onAddData(TabItemEx tabItem, object tabData)
        {
            switch (tabItem.TabType) {
                case TabType.GeoVideo:
                case TabType.GeoStar:
                case TabType.GeoRecentPlay:
                    SelectWrapper<Video> ExtraWrapper = tabData as SelectWrapper<Video>;

                    VideoList videoList = new VideoList(ExtraWrapper, tabItem);
                    videoList.Uid = tabItem.UUID;
                    videoList.OnItemClick += OnItemClick;

                    TabPanel.Children.Add(videoList);


                    break;

                default:
                    break;
            }
        }

        public void RemovePanel(TabItemEx tabItem)
        {
            int idx = -1;
            foreach (UIElement item in TabPanel.Children) {
                idx++;
                string uid = item.Uid;
                if (string.IsNullOrEmpty(uid))
                    continue;
                if (uid.Equals(tabItem.UUID)) {
                    break;
                }
            }
            if (idx >= 0 && idx < TabPanel.Children.Count) {
                TabPanel.Children.RemoveAt(idx);
            }
        }

        public void RemoveTabItem(int idx)
        {
            if (vieModel.TabItems == null)
                return;
            if (idx >= 0 && idx < vieModel.TabItems.Count) {

                // 移除对应的 panel
                RemovePanel(vieModel.TabItems[idx]);
                vieModel.TabItems[idx].Pinned = false;
                vieModel.TabItems.RemoveAt(idx);
            }
            // 默认选中左边的
            int selectIndex = idx - 1;
            if (selectIndex < 0)
                selectIndex = 0;

            if (vieModel.TabItems.Count > 0)
                vieModel.TabItemManager?.SetTabSelected(selectIndex);
        }

        public void SetTabSelected(int idx)
        {
            if (vieModel.TabItems == null || idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                vieModel.TabItems[i].Selected = false;
            }
            vieModel.TabItems[idx].Selected = true;
            List<VideoList> videoLists = TabPanel.Children.OfType<VideoList>().ToList();

            int target = -1;
            for (int i = 0; i < videoLists.Count; i++) {
                videoLists[i].Visibility = System.Windows.Visibility.Hidden;
                if (videoLists[i].TabItemEx.Equals(vieModel.TabItems[idx])) {
                    target = i;
                }
            }
            if (target >= 0 && target < videoLists.Count)
                videoLists[target].Visibility = System.Windows.Visibility.Visible;
        }

        public void PinByIndex(int idx)
        {

            if (idx < 0)
                return;

            if (vieModel == null || vieModel.TabItems == null || vieModel.TabItems.Count == 0 ||
                idx >= vieModel.TabItems.Count)
                return;
            TabItemEx tabItem = vieModel.TabItems[idx];
            if (tabItem.Pinned) {
                // 取消固定
                int targetIndex = vieModel.TabItems.Count;

                for (int i = vieModel.TabItems.Count - 1; i >= 0; i--) {
                    if (targetIndex == vieModel.TabItems.Count && vieModel.TabItems[i].Pinned)
                        targetIndex = i;

                    if (targetIndex < vieModel.TabItems.Count && idx >= 0)
                        break;
                }

                if (targetIndex == vieModel.TabItems.Count)
                    targetIndex = 0;
                tabItem.Pinned = false;
                // 移动到前面
                vieModel.TabItems.Move(idx, targetIndex);
            } else {
                // 固定
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (targetIndex < 0 && !vieModel.TabItems[i].Pinned)
                        targetIndex = i;

                    if (targetIndex >= 0 && idx >= 0)
                        break;
                }
                if (targetIndex < 0)
                    return;
                tabItem.Pinned = true;
                // 移动到前面
                vieModel.TabItems.Move(idx, targetIndex);
            }
        }

        public void MoveToLast(int originIdx)
        {
            if (vieModel.TabItems[originIdx].Pinned) {
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Pinned)
                        targetIndex = i;
                }
                vieModel.TabItems.Move(originIdx, targetIndex);
            } else {
                vieModel.TabItems.Move(originIdx, vieModel.TabItems.Count - 1);
            }
        }
        public void MoveToFirst(int originIdx)
        {
            if (vieModel.TabItems[originIdx].Pinned) {
                // 如果已经固定，则移动到所有固定的前面
                vieModel.TabItems.Move(originIdx, 0);
            } else {
                // 如果没有固定，则找到最后一个固定的
                bool hasPinned = false;
                int targetIndex = -1;
                for (int i = 0; i < vieModel.TabItems.Count; i++) {
                    if (vieModel.TabItems[i].Pinned) {
                        hasPinned = true;
                        targetIndex = i;
                    }
                }

                if (targetIndex < 0 || targetIndex + 1 >= vieModel.TabItems.Count)
                    targetIndex = 0;
                if (hasPinned && targetIndex + 1 < vieModel.TabItems.Count)
                    vieModel.TabItems.Move(originIdx, targetIndex + 1);
                else
                    vieModel.TabItems.Move(originIdx, 0);
            }
        }

        public void RemoveRange(int start, int end)
        {
            if (vieModel.TabItems == null || vieModel.TabItems.Count == 0)
                return;

            int total = vieModel.TabItems.Count;

            if (start < 0 || start >= total || end < 0 || end >= total || start > end)
                return;

            for (int i = end; i >= start; i--) {
                if (vieModel.TabItems[i].Pinned)
                    continue;
                vieModel.TabItems.RemoveAt(i);
            }
        }
    }
}
