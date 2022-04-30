using DynamicData.Annotations;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Jvedio.Core.Enums;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalMapper;

namespace Jvedio.ViewModel
{
    public class VieModel_StartUp : ViewModelBase
    {


        public bool Sort = GlobalConfig.StartUp.Sort;
        public string SortType = GlobalConfig.StartUp.SortType;
        public string CurrentSearch = "";
        public long CurrentSideIdx = GlobalConfig.StartUp.SideIdx;
        public long CurrentDBID = GlobalConfig.StartUp.CurrentDBID;

        #region "属性"



        private bool _Tile = GlobalConfig.StartUp.Tile;

        /// <summary>
        /// 是否平铺
        /// </summary>
        public bool Tile
        {
            get { return _Tile; }
            set
            {
                _Tile = value;
                RaisePropertyChanged();
            }
        }

        private bool _Loading = true;
        public bool Loading
        {
            get { return _Loading; }
            set
            {
                _Loading = value;
                RaisePropertyChanged();
            }
        }

        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
        }

        private bool _ShowHideItem = GlobalConfig.StartUp.ShowHideItem;

        /// <summary>
        /// 是否平铺
        /// </summary>
        public bool ShowHideItem
        {
            get { return _ShowHideItem; }
            set
            {
                _ShowHideItem = value;
                RaisePropertyChanged();
            }
        }




        private ObservableCollection<AppDatabase> _Databases;
        public ObservableCollection<AppDatabase> Databases
        {
            get { return _Databases; }
            set
            {
                _Databases = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<AppDatabase> _CurrentDatabases;
        public ObservableCollection<AppDatabase> CurrentDatabases
        {
            get { return _CurrentDatabases; }
            set
            {
                _CurrentDatabases = value;
                RaisePropertyChanged();
            }
        }
        private string _LoadingText;
        public string LoadingText
        {
            get { return _LoadingText; }
            set
            {
                _LoadingText = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        public VieModel_StartUp()
        {
            ReadFromDataBase();
        }

        public void ReadFromDataBase()
        {
            Databases = new ObservableCollection<AppDatabase>();
            CurrentDatabases = new ObservableCollection<AppDatabase>();
            List<AppDatabase> appDatabases = new List<AppDatabase>();
            SelectWrapper<AppDatabase> wrapper = new SelectWrapper<AppDatabase>();
            wrapper.Eq("DataType", GlobalConfig.StartUp.SideIdx);
            appDatabases = appDatabaseMapper.selectList(wrapper);

            appDatabases.ForEach(item => Databases.Add(item));
            Search();
        }

        public void refreshItem(AppDatabase data)
        {
            ReadFromDataBase();
        }




        public void Search()
        {
            CurrentDatabases = null;
            if (string.IsNullOrEmpty(CurrentSearch)) CurrentDatabases = Databases;
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            if (!ShowHideItem)
            {
                Databases.ToList().Where(item => item.Hide == 0).Where(item => item.Name.IndexOf(CurrentSearch) >= 0).ToList().ForEach
                (item => temp.Add(item));
            }
            else
            {
                Databases.ToList().Where(item => item.Hide >= 0).Where(item => item.Name.IndexOf(CurrentSearch) >= 0).ToList().ForEach
                (item => temp.Add(item));
            }
            CurrentDatabases = temp;
            SortDataBase();
        }

        public void SortDataBase()
        {
            if (Databases == null || Databases.Count == 0)
            {
                CurrentDatabases = new ObservableCollection<AppDatabase>();
                return;
            }
            if (CurrentDatabases == null) CurrentDatabases = Databases;
            List<AppDatabase> infos = CurrentDatabases.ToList();
            CurrentDatabases = null;
            switch (SortType)
            {
                case "名称":
                    infos = infos.OrderBy(x => x.Name).ToList();
                    break;
                case "创建时间":
                    infos = infos.OrderBy(x => x.CreateDate).ToList();
                    break;
                case "数据数目":
                    infos = infos.OrderBy(x => x.Count).ToList();
                    break;

                case "访问频率":
                    infos = infos.OrderBy(x => x.ViewCount).ToList();
                    break;

                default:
                    break;
            }
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            if (Sort) infos.Reverse();
            infos.ForEach(item => temp.Add(item));
            CurrentDatabases = temp;
        }

    }
}
