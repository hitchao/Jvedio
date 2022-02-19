using DynamicData.Annotations;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Jvedio.Core.pojo;

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

        #endregion


        public VieModel_StartUp()
        {
            ScanDatabase();
        }

        public void Search()
        {
            CurrentDatabases = null;
            if (string.IsNullOrEmpty(CurrentSearch)) CurrentDatabases = Databases;
            ObservableCollection<SqliteInfo> temp = new ObservableCollection<SqliteInfo>();
            Databases.ToList().Where(item => item.Name.IndexOf(CurrentSearch) >= 0 || item.Path.IndexOf(CurrentSearch) >= 0).ToList().ForEach
                (item => temp.Add(item));
            CurrentDatabases = temp;
            SortDataBase();
        }

        public void SortDataBase()
        {
            if (Databases == null || Databases.Count == 0)
            {
                CurrentDatabases = new ObservableCollection<SqliteInfo>();
                return;
            }
            if (CurrentDatabases == null) CurrentDatabases = Databases;
            List<SqliteInfo> sqliteInfos = CurrentDatabases.ToList();
            CurrentDatabases = null;
            switch (SortType)
            {
                case "名称":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Name).ToList();
                    break;
                case "创建时间":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.CreateDate).ToList();
                    break;
                case "数据数目":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Count).ToList();
                    break;

                case "访问频率":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.ViewCount).ToList();
                    break;

                case "文件大小":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Size).ToList();
                    break;
                case "路径":
                    sqliteInfos = sqliteInfos.OrderBy(x => x.Path).ToList();
                    break;

                default:
                    break;
            }
            ObservableCollection<SqliteInfo> temp = new ObservableCollection<SqliteInfo>();
            if (Sort) sqliteInfos.Reverse();
            sqliteInfos.ForEach(item => temp.Add(item));
            CurrentDatabases = temp;
        }


        public void ScanDatabase(InfoType type = InfoType.Video)
        {
            Databases = new ObservableCollection<SqliteInfo>();
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
                    SqliteInfo sqliteInfo = ParseInfo(item);
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
            List<SqliteInfo> sqliteInfos = ConfigConnection.Instance.SelectSqliteInfo();
            foreach (var item in Databases)
            {
                if (sqliteInfos.Contains(item))
                {
                    SqliteInfo data = sqliteInfos[sqliteInfos.IndexOf(item)];
                    //更新
                    item.ID = data.ID;
                    item.Count = data.Count;
                    item.ViewCount = data.ViewCount;
                    item.ViewCount = data.ViewCount;
                    string imagePath = Path.Combine(GlobalVariable.ProjectImagePath, data.ImagePath);
                    if (File.Exists(imagePath)) item.ImagePath = imagePath;

                    data.Size = item.Size;
                    ConfigConnection.Instance.UpdateSqliteInfo(data);
                }
                else
                {
                    // 新增
                    ConfigConnection.Instance.InsertSqliteInfo(item);
                }
            }

            // 删除
            List<SqliteInfo> toDelete = sqliteInfos.Except(Databases).ToList();
            ConfigConnection.Instance.DeleteByIds(toDelete.Select(item => item.ID).ToList());
        }

        public SqliteInfo ParseInfo(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name)) return null;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            SqliteInfo sqliteInfo = new SqliteInfo();
            sqliteInfo.Name = name;
            sqliteInfo.Path = path;
            sqliteInfo.Size = fileInfo.Length;
            sqliteInfo.CreateDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            return sqliteInfo;
        }

    }
}
