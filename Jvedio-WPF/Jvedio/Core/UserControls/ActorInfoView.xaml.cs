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


        private void OpenActorPath(object sender, RoutedEventArgs e)
        {
            if (CurrentActorInfo != null)
                FileHelper.TryOpenSelectPath(CurrentActorInfo.GetImagePath());
        }

        // todo 演员信息下载
        private void BeginDownLoadActress(object sender, MouseButtonEventArgs e)
        {
            MessageNotify.Info("开发中");
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
        private void EditActress(object sender, MouseButtonEventArgs e)
        {
            if (CurrentActorInfo != null) {
                Window_EditActor window_EditActor = new Window_EditActor(CurrentActorInfo.ActorID);
                window_EditActor.ShowDialog();
            }
        }

        private void LoadActorOtherMovie(object sender, MouseButtonEventArgs e)
        {
            MessageNotify.Info("开发中");
        }

        private void ActorRate_ValueChanged(object sender, EventArgs e)
        {
            Rate rate = (Rate)sender;
            if (CurrentActorInfo != null)
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
