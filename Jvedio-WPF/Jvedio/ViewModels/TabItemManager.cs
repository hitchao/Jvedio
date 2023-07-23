using Jvedio.Core.Config;
using Jvedio.Entity.Common;
using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.ViewModels
{

    public class TabItemManager
    {
        private TabItemManager()
        {
        }

        private static TabItemManager instance { get; set; }

        private VieModel_Main vieModel { get; set; }

        public static TabItemManager CreateInstance(VieModel_Main vieModel)
        {
            if (instance == null) {
                instance = new TabItemManager();
                instance.vieModel = vieModel;
            }
            return instance;
        }

        public void Add(TabType type, string tabName)
        {
            if (vieModel == null) {
                return;
            }

            if (vieModel.TabItems == null)
                vieModel.TabItems = new ObservableCollection<TabItemEx>();

            TabItemEx tabItem = vieModel.TabItems.FirstOrDefault(arg => arg.Name.Equals(tabName));
            if (tabItem == null) {
                tabItem = new TabItemEx(tabName, type);
                vieModel.TabItems.Add(tabItem);
            }

            int idx = -1;
            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                if (vieModel.TabItems[i].Name.Equals(tabName)) {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
                SetTabSelected(idx);

        }

        public void SetTabSelected(int idx)
        {
            if (vieModel.TabItems == null || idx < 0 || idx >= vieModel.TabItems.Count)
                return;

            for (int i = 0; i < vieModel.TabItems.Count; i++) {
                vieModel.TabItems[i].Selected = false;
            }
            vieModel.TabItems[idx].Selected = true;
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
