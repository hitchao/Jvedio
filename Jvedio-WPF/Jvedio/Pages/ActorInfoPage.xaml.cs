using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Mapper;
using SuperControls.Style;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Jvedio.MapperManager;

namespace Jvedio.Pages
{
    /// <summary>
    /// ActorInfoPage.xaml 的交互逻辑
    /// </summary>
    public partial class ActorInfoPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }





        private ActorInfo _CurrentActorInfo;

        public ActorInfo CurrentActorInfo
        {
            get { return _CurrentActorInfo; }

            set
            {
                _CurrentActorInfo = value;
                RaisePropertyChanged();
            }
        }

        public event Action Close;

        public ActorInfoPage()
        {
            InitializeComponent();
            this.DataContext = this;
            ActorNavigator.NavigationService.LoadCompleted -= NavigationService_LoadCompleted;
            ActorNavigator.NavigationService.LoadCompleted += NavigationService_LoadCompleted;
        }

        private void NavigationService_LoadCompleted(object sender, NavigationEventArgs e)
        {
            this.CurrentActorInfo = e.ExtraData as ActorInfo;
            this.Visibility = Visibility.Visible;
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

        // todo
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
            this.Visibility = Visibility.Collapsed;
            Close?.Invoke();
        }
    }
}
