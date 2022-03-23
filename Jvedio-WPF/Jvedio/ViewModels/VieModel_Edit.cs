using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.FileProcess;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using Jvedio.Entity;
using Jvedio.Core.SimpleORM;
using System.Windows;

namespace Jvedio.ViewModel
{
    class VieModel_Edit : ViewModelBase
    {

        public Video _CurrentVideo;

        public Video CurrentVideo
        {
            get { return _CurrentVideo; }
            set
            {
                _CurrentVideo = value;
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



        public VieModel_Edit(long dataid)
        {

            if (dataid <= 0) return;
            DataID = dataid;
            CurrentVideo = GlobalMapper.videoMapper.SelectVideoByID(dataid);
        }


        public void Reset()
        {
            CurrentVideo = null;
            CurrentVideo = GlobalMapper.videoMapper.SelectVideoByID(DataID);
        }


        public bool Save()
        {
            if (CurrentVideo == null) return false;
            MetaData data = (MetaData)CurrentVideo;
            data.DataID = DataID;
            int update1 = GlobalMapper.metaDataMapper.updateById(data);
            int update2 = GlobalMapper.videoMapper.updateById(CurrentVideo);
            return update1 > 0 & update2 > 0;

        }

    }
}
