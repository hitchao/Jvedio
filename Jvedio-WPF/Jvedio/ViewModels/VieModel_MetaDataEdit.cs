using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.GlobalMapper;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using static Jvedio.ImageProcess;
using static Jvedio.Utils.CustomExtension;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using Jvedio.Entity;
using Jvedio.Core.SimpleORM;
using System.Windows;
using System.Windows.Threading;
using Jvedio.Entity.Data;
using Jvedio.Core.Scan;
using Jvedio.Utils;

namespace Jvedio.ViewModel
{
    class VieModel_MetaDataEdit : ViewModelBase
    {

        Window_MetaDataEdit editWindow = GetWindowByName("Window_MetaDataEdit") as Window_MetaDataEdit;


        public MetaData _CurrentData;

        public MetaData CurrentData
        {
            get { return _CurrentData; }
            set
            {
                _CurrentData = value;
                RaisePropertyChanged();
            }
        }
        public Picture _CurrentPicture;

        public Picture CurrentPicture
        {
            get { return _CurrentPicture; }
            set
            {
                _CurrentPicture = value;
                RaisePropertyChanged();
            }
        }
        public Comic _CurrentComic;

        public Comic CurrentComic
        {
            get { return _CurrentComic; }
            set
            {
                _CurrentComic = value;
                RaisePropertyChanged();
            }
        }
        public Game _CurrentGame;

        public Game CurrentGame
        {
            get { return _CurrentGame; }
            set
            {
                _CurrentGame = value;
                RaisePropertyChanged();
            }
        }


        public bool _MoreExpanded = GlobalConfig.Edit.MoreExpanded;

        public bool MoreExpanded
        {
            get { return _MoreExpanded; }
            set
            {
                _MoreExpanded = value;
                RaisePropertyChanged();
            }
        }


        public long _DataID;

        public long DataID
        {
            get { return _DataID; }
            set
            {
                _DataID = value;
                RaisePropertyChanged();
            }
        }


        private BitmapSource _CurrentImage;
        public BitmapSource CurrentImage
        {
            get { return _CurrentImage; }
            set
            {
                _CurrentImage = value;
                RaisePropertyChanged();
            }
        }


        private List<ActorInfo> actorlist;
        public List<ActorInfo> ActorList
        {
            get { return actorlist; }
            set
            {
                actorlist = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<ActorInfo> _CurrentActorList;


        public ObservableCollection<ActorInfo> CurrentActorList
        {
            get { return _CurrentActorList; }
            set
            {
                _CurrentActorList = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _CurrentLabelList;


        public ObservableCollection<string> CurrentLabelList
        {
            get { return _CurrentLabelList; }
            set
            {
                _CurrentLabelList = value;
                RaisePropertyChanged();
            }
        }




        private int _TabControlSelectedIndex = 0;
        public int TabControlSelectedIndex
        {
            get { return _TabControlSelectedIndex; }
            set
            {
                _TabControlSelectedIndex = value;
                RaisePropertyChanged();
            }
        }





        private string _LabelText = String.Empty;
        public string LabelText
        {
            get { return _LabelText; }
            set
            {
                _LabelText = value;
                RaisePropertyChanged();
                getLabels();
            }
        }









        public VieModel_MetaDataEdit(long dataid)
        {
            if (dataid <= 0) return;
            DataID = dataid;
            TabControlSelectedIndex = (int)editWindow.CurrentDataType - 1;
            Reset();
        }


        private List<string> oldLabels;

        public void Reset()
        {
            if (CurrentDataType == Core.Enums.DataType.Picture)
            {
                TabControlSelectedIndex = 0;
                CurrentPicture = null;
                CurrentPicture = GlobalMapper.pictureMapper.SelectByID(DataID);
                oldLabels = CurrentPicture.LabelList?.Select(arg => arg).ToList();
                CurrentData = CurrentPicture.toMetaData();
            }
            else if (CurrentDataType == Core.Enums.DataType.Game)
            {
                TabControlSelectedIndex = 1;
                CurrentGame = null;
                CurrentGame = GlobalMapper.gameMapper.SelectByID(DataID);
                oldLabels = CurrentGame.LabelList?.Select(arg => arg).ToList();
                CurrentData = CurrentGame.toMetaData();
                //if (File.Exists(CurrentGame.BigImagePath))
                //    CurrentImage = ImageProcess.ReadImageFromFile(CurrentGame.BigImagePath);
            }
            else if (CurrentDataType == Core.Enums.DataType.Comics)
            {
                TabControlSelectedIndex = 2;
                CurrentComic = null;
                CurrentComic = GlobalMapper.comicMapper.SelectByID(DataID);
                oldLabels = CurrentComic.LabelList?.Select(arg => arg).ToList();
                CurrentData = CurrentComic.toMetaData();
            }
            getLabels();
        }


        private bool loadingLabel = false;
        public async void getLabels()
        {
            if (loadingLabel) return;
            loadingLabel = true;
            string like_sql = "";

            string search = LabelText.ToProperSql().Trim();
            if (!string.IsNullOrEmpty(search))
                like_sql = $" and LabelName like '%{search}%' ";


            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label " +
                "JOIN metadata on metadata.DataID=metadata_to_label.DataID " +
                $"where metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0}" + like_sql +
                $" GROUP BY LabelName ORDER BY Count DESC";
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            foreach (Dictionary<string, object> item in list)
            {
                string LabelName = item["LabelName"].ToString();
                long.TryParse(item["Count"].ToString(), out long count);
                labels.Add($"{LabelName}({count})");
            }
            CurrentLabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadLabel), labels[i]);
            }
            loadingLabel = false;
        }

        private delegate void LoadLabelDelegate(string str);
        private void LoadLabel(string str) => CurrentLabelList.Add(str);

        public bool Save()
        {
            if (CurrentData == null) return false;
            int update1 = metaDataMapper.updateById(CurrentData);
            metaDataMapper.SaveLabel(CurrentData, oldLabels);// 标签
            int update2 = 0;
            if (CurrentDataType == Core.Enums.DataType.Picture)
            {
                if (CurrentPicture == null) return false;
                update2 = pictureMapper.updateById(CurrentPicture);
            }
            else if (CurrentDataType == Core.Enums.DataType.Game)
            {
                if (CurrentGame == null) return false;
                update2 = gameMapper.updateById(CurrentGame);
            }
            else if (CurrentDataType == Core.Enums.DataType.Comics)
            {
                if (CurrentComic == null) return false;
                update2 = comicMapper.updateById(CurrentComic);
            }
            return update1 > 0 & update2 > 0;
        }







    }
}
