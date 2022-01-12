using DynamicData.Annotations;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Jvedio.Core.pojo.data;
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
        public RelayCommand ListDatabaseCommand { get; set; }

        public bool Sort = false;

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

        


        private ObservableCollection<SqliteInfo> _Databases;
        public ObservableCollection<SqliteInfo> Databases
        {
            get { return _Databases; }
            set
            {
                _Databases = value;
                RaisePropertyChanged();
            }
        }        
        private ObservableCollection<SqliteInfo> _CurrentDatabases;
        public ObservableCollection<SqliteInfo> CurrentDatabases
        {
            get { return _CurrentDatabases; }
            set
            {
                _CurrentDatabases = value;
                RaisePropertyChanged();
            }
        }


        public VieModel_StartUp()
        {
            ListDatabaseCommand = new RelayCommand(ListDatabase);
            ListDatabase();
        }

        public void Search(string search)
        {
            CurrentDatabases = null;
            if (search == null) CurrentDatabases = Databases;
            ObservableCollection<Core.pojo.data.SqliteInfo> temp = new ObservableCollection<Core.pojo.data.SqliteInfo>();
            Databases.ToList().Where(item => item.Name.IndexOf(search) >= 0 || item.Path.IndexOf(search) >= 0).ToList().ForEach
                (item => temp.Add(item));
            CurrentDatabases = temp;
        }

        public void SortDataBase(string header)
        {
            Sort = !Sort;
            List<Core.pojo.data.SqliteInfo> sqliteInfos = Databases.ToList();
            CurrentDatabases = null;
            switch (header)
            {
                case "名称":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Name).ToList();
                    break;
                case "创建时间":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.CreateDate).ToList();
                    break;
                case "数量":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Count).ToList();
                    break;
                case "路径":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Path).ToList();
                    break;

                default:
                    break;
            }
            ObservableCollection<Core.pojo.data.SqliteInfo> temp = new ObservableCollection<Core.pojo.data.SqliteInfo>();
            if (Sort) sqliteInfos.Reverse();
            sqliteInfos.ForEach(item => temp.Add(item));
            CurrentDatabases = temp;
        }


        public void ListDatabase()
        {
            Databases = new ObservableCollection<SqliteInfo>();
            string[] files = FileHelper.TryScanDIr(GlobalVariable.DataPath, "*.sqlite", SearchOption.TopDirectoryOnly);
            LoadDatabase(files);
        }

        public void LoadDatabase(string[] files)
        {
            if (files != null && files.Length > 0)
            {
                foreach (var item in files)
                {
                    SqliteInfo sqliteInfo = ParseInfo(item);
                    if (sqliteInfo != null && !Databases.Contains(sqliteInfo)) Databases.Add(sqliteInfo);
                }
            }
            CurrentDatabases = Databases;
        }

        public SqliteInfo ParseInfo(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name)) return null;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            SqliteInfo sqliteInfo = new SqliteInfo();
            sqliteInfo.Name = name;
            sqliteInfo.Path = path;
            sqliteInfo.CreateDate = fileInfo.CreationTime.ToLongDateString();
            sqliteInfo.Count = 10;
            return sqliteInfo;
        }
    }
}
