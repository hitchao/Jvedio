using DynamicData.Annotations;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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

namespace Jvedio.ViewModel
{
    public class VieModel_StartUp : ViewModelBase
    {


        public bool Sort = false;
        public string SortType = "";
        public string CurrentSearch = "";
        public int CurrentSideIdx = 0;

        #region "属性"

        private bool _initCompleted;
        public bool InitCompleted
        {
            get { return _initCompleted; }
            set
            {
                _initCompleted = value;
                RaisePropertyChanged();
            }
        }


        private bool _Tile;

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

        #endregion


        public VieModel_StartUp()
        {
            ScanDatabase();
        }

        public void refreshItem(AppDatabase data)
        {
            for (int i = 0; i < Databases.Count; i++)
            {
                if (Databases[i].DBId == data.DBId)
                {
                    Databases[i] = data;
                    break;
                }
            }

            for (int i = 0; i < CurrentDatabases.Count; i++)
            {
                if (CurrentDatabases[i].DBId == data.DBId)
                {
                    CurrentDatabases[i] = data;
                    break;
                }
            }
        }

        public void Search()
        {
            CurrentDatabases = null;
            if (string.IsNullOrEmpty(CurrentSearch)) CurrentDatabases = Databases;
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            Databases.ToList().Where(item => item.Name.IndexOf(CurrentSearch) >= 0 || item.Path.IndexOf(CurrentSearch) >= 0).ToList().ForEach
                (item => temp.Add(item));
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

                case "文件大小":
                    infos = infos.OrderBy(x => x.Size).ToList();
                    break;
                case "路径":
                    infos = infos.OrderBy(x => x.Path).ToList();
                    break;

                default:
                    break;
            }
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            if (Sort) infos.Reverse();
            infos.ForEach(item => temp.Add(item));
            CurrentDatabases = temp;
        }


        public void ScanDatabase(InfoType type = InfoType.Video)
        {
            Databases = new ObservableCollection<AppDatabase>();
            string scanDir = Path.Combine(GlobalVariable.DataPath, type.ToString());
            string[] files = FileHelper.TryScanDIr(scanDir, "*.sqlite", SearchOption.TopDirectoryOnly);
            LoadDatabase(files);
        }

        public void LoadDatabase(string[] files)
        {
            if (files != null && files.Length > 0)
            {
                foreach (var item in files)
                {
                    AppDatabase sqliteInfo = ParseInfo(item);
                    if (sqliteInfo != null && !Databases.Contains(sqliteInfo))
                    {
                        Databases.Add(sqliteInfo);
                    }
                }
            }
            JoinInfo();
            Search();
        }


        // 存储到数据库中
        private void JoinInfo()
        {
            // 1. 新增 / 更新
            // 2. 删除
            List<AppDatabase> appDatabases = GlobalVariable.AppDatabaseMapper.selectAll();
            foreach (var item in Databases)
            {
                if (appDatabases.Contains(item))
                {
                    AppDatabase data = appDatabases[appDatabases.IndexOf(item)];
                    //更新
                    item.DBId = data.DBId;
                    item.Count = data.Count;
                    item.ViewCount = data.ViewCount;
                    item.ViewCount = data.ViewCount;
                    string imagePath = Path.Combine(GlobalVariable.ProjectImagePath, data.ImagePath);
                    if (File.Exists(imagePath)) item.ImagePath = imagePath;

                    data.Size = item.Size;
                    GlobalVariable.AppDatabaseMapper.updateById(item);
                }
                else
                {
                    // 新增
                    GlobalVariable.AppDatabaseMapper.insert(item);
                }
            }
            // 删除
            List<AppDatabase> toDelete = appDatabases.Except(Databases).ToList();
            GlobalVariable.AppDatabaseMapper.deleteByIds(toDelete.Select(item => item.DBId.ToString()).ToList());
        }

        public AppDatabase ParseInfo(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name)) return null;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            AppDatabase info = new AppDatabase();
            info.Name = name;
            info.Path = path;
            info.Size = fileInfo.Length;
            info.CreateDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            return info;
        }

    }
}
