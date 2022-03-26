using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using DynamicData;
using System.Windows.Media.Imaging;
using Jvedio.Utils;
using Jvedio.Entity;
using Jvedio.Core.SimpleORM;

namespace Jvedio.ViewModel
{
    public class VieModel_Details : ViewModelBase
    {

        public event EventHandler QueryCompleted;
        public VieModel_Details()
        {

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

        public void CleanUp()
        {
            MessengerInstance.Unregister(this);
        }

        public void GetLabelList()
        {
            //TextType = "标签";
            List<string> labels = DataBase.SelectLabelByVedioType(VedioType.所有);

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LabelList = new ObservableCollection<string>();
                LabelList.AddRange(labels);
            });
        }





        public void SaveLove()
        {

            //DataBase.UpdateMovieByID(CurrentVideo.id, "favorites", CurrentVideo.favorites, "string");
        }

        public void SaveLabel()
        {
            //DataBase.UpdateMovieByID(CurrentVideo.id, "label", string.Join(" ", CurrentVideo.labellist), "string");
        }


        public void onLabelChanged()
        {
            RaisePropertyChanged("CurrentVideo");
        }



        public void Load(long dataID)
        {
            //((WindowDetails)FileProcess.GetWindowByName("WindowDetails")).SetStatus(false);
            metaDataMapper.increaseFieldById("ViewCount", dataID); //访问次数+1
            CurrentVideo = videoMapper.SelectVideoByID(dataID);

            // 磁力
            List<Magnet> magnets = magnetsMapper.selectList(new SelectWrapper<Magnet>().Eq("DataID", dataID));
            CurrentVideo.Magnets = magnets.OrderByDescending(arg => arg.Size).ThenByDescending(arg => arg.Releasedate).ThenByDescending(arg => string.Join(" ", arg.Tags).Length).ToList(); ;

            // 演员：懒加载模式
            string[] actorNames = CurrentVideo.ActorNames.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries);
            string[] nameFlags = CurrentVideo.NameFlags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (actorNames.Length == nameFlags.Length)
            {
                CurrentVideo.ActorInfos = new List<ActorInfo>();
                for (int i = 0; i < actorNames.Length; i++)
                {
                    string actorName = actorNames[i];
                    string nameFlag = nameFlags[i];
                    IWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>().Eq("ActorName", actorName).Eq("NameFlag", nameFlag);
                    ActorInfo actorInfo = actorMapper.selectOne(wrapper);
                    CurrentVideo.ActorInfos.Add(actorInfo);
                }
            }

            //释放图片内存
            //CurrentVideo.SmallImage = null;
            //CurrentVideo.BigImage = null;
            //for (int i = 0; i < CurrentVideo.PreviewImageList.Count; i++)
            //{
            //    CurrentVideo.PreviewImageList[i] = null;
            //}
            // todo 释放演员头像

            //GC.Collect();



            CurrentVideo.BigImage = ImageProcess.BitmapImageFromFile(Video.parseImagePath(CurrentVideo.BigImagePath));
            if (CurrentVideo.BigImage == null) CurrentVideo.BigImage = DefaultBigImage;
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

            //CurrentVideo = CurrentVideo;
            //CurrentVideo.tagstamps = "";
            //FileProcess.addTag(ref CurrentVideo);
            //if (string.IsNullOrEmpty(CurrentVideo.title)) CurrentVideo.title = Path.GetFileNameWithoutExtension(CurrentVideo.filepath);
            QueryCompleted?.Invoke(this, new EventArgs());

        }
    }


}
