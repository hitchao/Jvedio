
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.WPF.VieModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.ViewModel
{
    public class VieModel_StartUp : ViewModelBase
    {
        #region "属性"

        private bool _Tile = ConfigManager.StartUp.Tile;

        /// <summary>
        /// 是否平铺
        /// </summary>
        public bool Tile {
            get { return _Tile; }

            set {
                _Tile = value;
                RaisePropertyChanged();
            }
        }

        private bool _Loading = true;

        public bool Loading {
            get { return _Loading; }

            set {
                _Loading = value;
                RaisePropertyChanged();
            }
        }



        private bool _ShowHideItem = ConfigManager.StartUp.ShowHideItem;

        /// <summary>
        /// 是否平铺
        /// </summary>
        public bool ShowHideItem {
            get { return _ShowHideItem; }

            set {
                _ShowHideItem = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<AppDatabase> _Databases;

        public ObservableCollection<AppDatabase> Databases {
            get { return _Databases; }

            set {
                _Databases = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<AppDatabase> _CurrentDatabases;

        public ObservableCollection<AppDatabase> CurrentDatabases {
            get { return _CurrentDatabases; }

            set {
                _CurrentDatabases = value;
                RaisePropertyChanged();
            }
        }

        private string _LoadingText;

        public string LoadingText {
            get { return _LoadingText; }

            set {
                _LoadingText = value;
                RaisePropertyChanged();
            }
        }
        private string _Version;

        public string Version {
            get { return _Version; }

            set {
                _Version = value;
                RaisePropertyChanged();
            }
        }
        private bool _Restoring = false;

        public bool Restoring {
            get { return _Restoring; }

            set {
                _Restoring = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "其它属性"

        public bool Sort { get; set; } = ConfigManager.StartUp.Sort;
        public string SortType { get; set; } = ConfigManager.StartUp.SortType;
        public string CurrentSearch { get; set; } = string.Empty;
        public long CurrentSideIdx { get; set; } = ConfigManager.StartUp.SideIdx;

        #endregion


        public VieModel_StartUp()
        {
            Init();
        }

        public override void Init()
        {
            ReadFromDataBase();
            Version = App.GetLocalVersion();
        }

        public void ReadFromDataBase()
        {
            Databases = new ObservableCollection<AppDatabase>();
            CurrentDatabases = new ObservableCollection<AppDatabase>();
            List<AppDatabase> appDatabases = new List<AppDatabase>();
            SelectWrapper<AppDatabase> wrapper = new SelectWrapper<AppDatabase>();
            wrapper.Eq("DataType", ConfigManager.StartUp.SideIdx);
            appDatabases = appDatabaseMapper.SelectList(wrapper);
            if (appDatabases == null)
                return;
            appDatabases.ForEach(item => Databases.Add(item));
            Search();

            Logger.Info("read from database ok");
        }

        public void refreshItem()
        {
            ReadFromDataBase();
        }

        public void Search()
        {
            CurrentDatabases = null;
            if (Databases == null || Databases.Count == 0) {
                CurrentDatabases = new ObservableCollection<AppDatabase>();
                return;
            }

            if (string.IsNullOrEmpty(CurrentSearch)) {
                CurrentSearch = string.Empty;
                CurrentDatabases = Databases;
            }

            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            if (!ShowHideItem) {
                Databases.ToList().Where(item => item.Hide == 0).Where(item => !string.IsNullOrEmpty(item.Name) &&
                    item.Name.IndexOf(CurrentSearch) >= 0).ToList().ForEach
                (item => temp.Add(item));
            } else {
                Databases.ToList().Where(item => item.Hide >= 0).Where(item => !string.IsNullOrEmpty(item.Name) &&
                    item.Name.IndexOf(CurrentSearch) >= 0).ToList().ForEach
                (item => temp.Add(item));
            }

            CurrentDatabases = temp;
            SortDataBase();
        }

        public void SortDataBase()
        {
            if (Databases == null || Databases.Count == 0) {
                CurrentDatabases = new ObservableCollection<AppDatabase>();
                return;
            }

            if (CurrentDatabases == null)
                CurrentDatabases = Databases;
            List<AppDatabase> infos = CurrentDatabases.ToList();
            CurrentDatabases = null;

            if (SortType.Equals(LangManager.GetValueByKey("Title"))) {
                infos = infos.OrderBy(x => x.Name).ToList();
            } else if (SortType.Equals(LangManager.GetValueByKey("CreatedDate"))) {
                infos = infos.OrderBy(x => x.CreateDate).ToList();
            } else if (SortType.Equals(LangManager.GetValueByKey("Number"))) {
                infos = infos.OrderBy(x => x.Count).ToList();
            } else if (SortType.Equals(LangManager.GetValueByKey("ViewNumber"))) {
                infos = infos.OrderBy(x => x.ViewCount).ToList();
            }

            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            if (Sort)
                infos.Reverse();
            infos.ForEach(item => temp.Add(item));
            CurrentDatabases = temp;
        }
    }
}
