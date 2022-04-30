using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using static Jvedio.ImageProcess;
using static Jvedio.FileProcess;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using DynamicData;
using System.Windows.Media.Imaging;
using Jvedio.Utils;
using Jvedio.Entity;
using Jvedio.Core.SimpleORM;
using System.Windows.Threading;

namespace Jvedio.ViewModel
{
    public class VieModel_Details : ViewModelBase
    {
        WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
        public event EventHandler QueryCompleted;
        public VieModel_Details()
        {

        }

        private bool _TeenMode = GlobalConfig.Settings.TeenMode;

        public bool TeenMode
        {
            get { return _TeenMode; }
            set
            {
                _TeenMode = value;
                RaisePropertyChanged();
            }
        }
        private int _SelectImageIndex = 0;

        public int SelectImageIndex
        {
            get { return _SelectImageIndex; }
            set
            {
                _SelectImageIndex = value;
                RaisePropertyChanged();
            }
        }
        private int _InfoSelectedIndex = (int)GlobalConfig.Detail.InfoSelectedIndex;

        public int InfoSelectedIndex
        {
            get { return _InfoSelectedIndex; }
            set
            {
                _InfoSelectedIndex = value;
                if (value == 1 && VideoInfo == null)
                    LoadVideoInfo();
                RaisePropertyChanged();
            }
        }

        private bool _ShowScreenShot = GlobalConfig.Detail.ShowScreenShot;

        public bool ShowScreenShot
        {
            get { return _ShowScreenShot; }
            set
            {
                _ShowScreenShot = value;
                RaisePropertyChanged();
            }
        }



        private string _SwitchInfo = "影片信息";
        public string SwitchInfo
        {
            get { return _SwitchInfo; }
            set
            {
                _SwitchInfo = value;
                RaisePropertyChanged();
            }
        }



        private Video _CurrentVideo;

        public Video CurrentVideo
        {
            get { return _CurrentVideo; }
            set
            {
                _CurrentVideo = value;
                RaisePropertyChanged();
            }
        }


        private VideoInfo _VideoInfo;

        public VideoInfo VideoInfo
        {
            get { return _VideoInfo; }
            set
            {
                _VideoInfo = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> labellist;
        public ObservableCollection<string> LabelList
        {
            get { return labellist; }
            set
            {
                labellist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<MyListItem> _MyList;


        public ObservableCollection<MyListItem> MyList
        {
            get { return _MyList; }
            set
            {
                _MyList = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<ActorInfo> _CurrentActorList = new ObservableCollection<ActorInfo>();


        public ObservableCollection<ActorInfo> CurrentActorList
        {
            get { return _CurrentActorList; }
            set
            {
                _CurrentActorList = value;
                RaisePropertyChanged();
            }
        }

        public void CleanUp()
        {
            MessengerInstance.Unregister(this);
        }


        public void LoadVideoInfo()
        {
            // todo 分段视频
            VideoInfo = MediaParse.GetMediaInfo(CurrentVideo.Path);
        }



        public void SaveLove()
        {
            metaDataMapper.updateFieldById("Grade", CurrentVideo.Grade.ToString(), CurrentVideo.DataID);
        }



        public void LoadTagStamp(ref Video video)
        {

        }


        public void Load(long dataID)
        {
            //释放图片内存
            if (CurrentVideo != null)
            {
                CurrentVideo.SmallImage = null;
                CurrentVideo.BigImage = null;
                for (int i = 0; i < CurrentVideo.PreviewImageList.Count; i++)
                {
                    CurrentVideo.PreviewImageList[i] = null;
                }
            }
            if (CurrentActorList != null)
            {
                for (int i = 0; i < CurrentActorList.Count; i++)
                {
                    CurrentActorList[i].SmallImage = null;
                }
            }

            GC.Collect();

            windowDetails.DataID = dataID;
            //((WindowDetails)FileProcess.GetWindowByName("WindowDetails")).SetStatus(false);

            // todo 事务下导致阻塞
            metaDataMapper.increaseFieldById("ViewCount", dataID); //访问次数+1
            Video video = videoMapper.SelectVideoByID(dataID);
            Video.setTagStamps(ref video);// 设置标签戳
            Video.handleEmpty(ref video);
            CurrentVideo = video;
            // 磁力
            List<Magnet> magnets = magnetsMapper.selectList(new SelectWrapper<Magnet>().Eq("DataID", dataID));
            CurrentVideo.Magnets = magnets.OrderByDescending(arg => arg.Size).ThenByDescending(arg => arg.Releasedate).ThenByDescending(arg => string.Join(" ", arg.Tags).Length).ToList(); ;



            BitmapImage image = ImageProcess.BitmapImageFromFile(CurrentVideo.getBigImage());
            if (image == null) image = DefaultBigImage;
            CurrentVideo.BigImage = image;
            //MySqlite db = new MySqlite("Translate");
            ////加载翻译结果
            //if (Properties.Settings.Default.TitleShowTranslate)
            //{
            //    string translate_title = db.GetInfoBySql($"select translate_title from youdao where id='{CurrentVideo.id}'");
            //    if (translate_title != "") CurrentVideo.title = translate_title;
            //}

            //if (Properties.Settings.Default.PlotShowTranslate)
            //{
            //    string translate_plot = db.GetInfoBySql($"select translate_plot from youdao where id='{CurrentVideo.id}'");
            //    if (translate_plot != "") CurrentVideo.plot = translate_plot;
            //}
            //db.CloseDB();
            //if (string.IsNullOrEmpty(CurrentVideo.title)) CurrentVideo.title = Path.GetFileNameWithoutExtension(CurrentVideo.filepath);

            if (InfoSelectedIndex == 1) LoadVideoInfo();
            QueryCompleted?.Invoke(this, new EventArgs());

        }



    }


}
