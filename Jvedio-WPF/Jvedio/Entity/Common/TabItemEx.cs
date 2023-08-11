using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Jvedio.Entity.Common
{
    public enum TabType
    {
        Default,
        GeoVideo,
        GeoStar,
        GeoRecentPlay,
        GeoActor,
        GeoLabel,
        GeoClassify,
        GeoRandom,
        GeoAsso,
        GeoTask,
        TabTypeMax,
    }

    public enum TaskType
    {
        ScreenShot,
        Download,
        Scan
    }


    public class TabItemEx : ViewModelBase
    {

        private const string DEFAULT_PATH_KEY = "GeoDefault";

        #region "属性"

        public static Dictionary<TabType, Geometry> PATH_TABLE { get; set; }


        private TabType _TabType;
        public TabType TabType {
            get { return _TabType; }
            private set { _TabType = value; RaisePropertyChanged(); }
        }



        private string _UUID;
        public string UUID {
            get { return _UUID; }
            private set { _UUID = value; RaisePropertyChanged(); }
        }

        private string _Name;
        public string Name {
            get { return _Name; }
            set { _Name = value; RaisePropertyChanged(); }
        }

        private bool _Selected;
        public bool Selected {
            get { return _Selected; }
            set { _Selected = value; RaisePropertyChanged(); }
        }

        private bool _Pinned;
        public bool Pinned {
            get { return _Pinned; }
            set { _Pinned = value; RaisePropertyChanged(); }
        }
        private bool _Loading;
        public bool Loading {
            get { return _Loading; }
            set { _Loading = value; RaisePropertyChanged(); }
        }

        private Geometry _TabGeometry = PATH_TABLE[TabType.Default];
        public Geometry TabGeometry {
            get { return _TabGeometry; }
            set { _TabGeometry = value; RaisePropertyChanged(); }
        }


        #endregion

        static TabItemEx()
        {
            PATH_TABLE = new Dictionary<TabType, Geometry>();
            System.Windows.ResourceDictionary resources = App.Current.Resources;

            Geometry geometry = null;
            if (resources.Contains(DEFAULT_PATH_KEY))
                geometry = resources[DEFAULT_PATH_KEY] as Geometry;

            PATH_TABLE.Add(TabType.Default, geometry);


            for (TabType type = TabType.GeoVideo; type < TabType.TabTypeMax; type++) {
                geometry = null;
                string key = type.ToString();
                if (resources.Contains(key))
                    geometry = resources[key] as Geometry;
                PATH_TABLE.Add(type, geometry);
            }
        }



        public TabItemEx(string name, TabType type, bool selected = false, bool pinned = false)
        {
            Name = name;
            Selected = selected;
            Pinned = pinned;
            TabType = type;
            TabGeometry = PATH_TABLE[type];
            UUID = System.Guid.NewGuid().ToString();
        }



        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            TabItemEx tabItemEx = obj as TabItemEx;
            if (tabItemEx == null)
                return false;

            return this.UUID.Equals(tabItemEx.UUID);
        }

        public override int GetHashCode()
        {
            return this.UUID.GetHashCode();
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }
    }
}
