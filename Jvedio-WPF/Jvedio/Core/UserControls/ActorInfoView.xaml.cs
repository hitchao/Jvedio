using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.IO;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// ActorInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class ActorInfoView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #region "事件"
        public event Action Close;

        #endregion


        #region "属性"




        private ActorInfo _CurrentActorInfo;

        public ActorInfo CurrentActorInfo {
            get { return _CurrentActorInfo; }

            set {
                _CurrentActorInfo = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        public ActorInfoView()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void OpenActorPath(object sender, RoutedEventArgs e)
        {
            if (CurrentActorInfo != null)
                FileHelper.TryOpenSelectPath(CurrentActorInfo.GetImagePath());
        }

        // todo 演员信息下载
        private void BeginDownLoadActress(object sender, MouseButtonEventArgs e)
        {
            // List<Actress> actresses = new List<Actress>();
            // actresses.Add(vieModel.Actress);
            // DownLoadActress downLoadActress = new DownLoadActress(actresses);
            // downLoadActress.BeginDownLoad();
            // downLoadActress.InfoUpdate += (s, ev) =>
            // {
            //    ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
            //    try
            //    {
            //        Dispatcher.Invoke((Action)delegate ()
            //        {
            //            vieModel.Actress = null;
            //            vieModel.Actress = actressUpdateEventArgs.Actress;
            //            downLoadActress.State = DownLoadState.Completed;
            //        });
            //    }
            //    catch (TaskCanceledException ex) { Logger.LogE(ex); }

            // };

            // downLoadActress.MessageCallBack += (s, ev) =>
            // {
            //    MessageCallBackEventArgs actressUpdateEventArgs = ev as MessageCallBackEventArgs;
            //    msgCard.Info(actressUpdateEventArgs.Message);

            // };
        }



        private void Image_Drop(object sender, DragEventArgs e)
        {
            // string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            // string file = dragdropFiles[0];

            // if (IsFile(file))
            // {
            //    FileInfo fileInfo = new FileInfo(file);
            //    if (fileInfo.Extension.ToLower() == ".jpg")
            //    {
            //        FileHelper.TryCopyFile(fileInfo.FullName, BasePicPath + $"Actresses\\{vieModel.Actress.name}.jpg", true);
            //        Actress actress = vieModel.Actress;
            //        actress.smallimage = null;
            //        actress.smallimage = GetActorImage(actress.name);
            //        vieModel.Actress = null;
            //        vieModel.Actress = actress;

            // if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

            // for (int i = 0; i < vieModel.ActorList.Count; i++)
            //        {
            //            if (vieModel.ActorList[i].name == actress.name)
            //            {
            //                vieModel.ActorList[i] = actress;
            //                break;
            //            }
            //        }

            // }
            //    else
            //    {
            //        msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_OnlySupportJPG"));
            //    }
            // }
        }

        private void EditActress(object sender, MouseButtonEventArgs e)
        {

        }

        private void LoadActorOtherMovie(object sender, MouseButtonEventArgs e)
        {

        }

        private void ActorRate_ValueChanged(object sender, EventArgs e)
        {
            Rate rate = (Rate)sender;
            actorMapper.UpdateFieldById("Grade", rate.Value.ToString(), CurrentActorInfo.ActorID);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void HideActressGrid(object sender, RoutedEventArgs e)
        {
            Close?.Invoke();
        }
    }
}
