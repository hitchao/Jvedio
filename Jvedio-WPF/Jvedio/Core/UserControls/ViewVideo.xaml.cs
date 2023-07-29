using Jvedio.Core.Enums;
using Jvedio.Core.Media;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using SuperControls.Style;
using SuperUtils.WPF.VieModel;
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

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// ViewVideo.xaml 的交互逻辑
    /// </summary>
    public partial class ViewVideo : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ViewVideo()
        {
            InitializeComponent();
        }

        private bool canShowDetails { get; set; }

        #region "Event"
        public static readonly RoutedEvent ShowDetailEvent =
            EventManager.RegisterRoutedEvent("ShowDetail", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ViewVideo));

        public event RoutedEventHandler ShowDetail {
            add => AddHandler(ShowDetailEvent, value);
            remove => RemoveHandler(ShowDetailEvent, value);
        }


        public static readonly RoutedEvent ImageMouseEnterEvent =
            EventManager.RegisterRoutedEvent("ImageMouseEnter", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ViewVideo));

        public event RoutedEventHandler ImageMouseEnter {
            add => AddHandler(ImageMouseEnterEvent, value);
            remove => RemoveHandler(ImageMouseEnterEvent, value);
        }


        public static readonly RoutedEvent ImageMouseLeaveEvent =
            EventManager.RegisterRoutedEvent("ImageMouseLeave", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ViewVideo));

        public event RoutedEventHandler ImageMouseLeave {
            add => AddHandler(ImageMouseLeaveEvent, value);
            remove => RemoveHandler(ImageMouseLeaveEvent, value);
        }


        public static readonly RoutedEvent OnPlayVideoEvent =
            EventManager.RegisterRoutedEvent("OnPlayVideo", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ViewVideo));

        public event RoutedEventHandler OnPlayVideo {
            add => AddHandler(OnPlayVideoEvent, value);
            remove => RemoveHandler(OnPlayVideoEvent, value);
        }
        #endregion

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ImageMouseEnterEvent, this));
        }



        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ImageMouseLeaveEvent, this));
        }

        private void CanShowDetails(object sender, MouseButtonEventArgs e)
        {
            canShowDetails = true;
        }

        private void ShowDetails(object sender, MouseButtonEventArgs e)
        {
            if (canShowDetails)
                RaiseEvent(new RoutedEventArgs(ShowDetailEvent) { Source = this });
            canShowDetails = false;
        }

        private void ViewAssocDatas(object sender, RoutedEventArgs e)
        {

        }

        private void ShowSubSection(object sender, RoutedEventArgs e)
        {

        }



        public void PlayVideo(object sender, MouseButtonEventArgs e)
        {
            if (!EditMode)
                RaiseEvent(new RoutedEventArgs(OnPlayVideoEvent) { Source = this });
        }

        private void CopyVID(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
        {

        }

        private void OnRateMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Rate_ValueChanged(object sender, SuperControls.Style.FunctionEventArgs<double> e)
        {

        }

        #region "属性"

        public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
nameof(ImageWidth), typeof(int), typeof(ViewVideo), new PropertyMetadata((int)ConfigManager.VideoConfig.GlobalImageWidth));

        public int ImageWidth {
            get { return (int)GetValue(ImageWidthProperty); }
            set {
                SetValue(ImageWidthProperty, value);
            }
        }

        public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
nameof(ImageHeight), typeof(int), typeof(ViewVideo), new PropertyMetadata((int)ConfigManager.VideoConfig.GlobalImageWidth));

        public int ImageHeight {
            get {
                return (int)GetValue(ImageHeightProperty);
            }
            set {
                SetValue(ImageHeightProperty, value);
            }
        }

        public static readonly DependencyProperty ImageModeProperty = DependencyProperty.Register(
nameof(ImageMode), typeof(int), typeof(ViewVideo), new PropertyMetadata(1));

        public int ImageMode {
            get { return (int)GetValue(ImageModeProperty); }
            set {
                SetValue(ImageModeProperty, value);
            }
        }

        public bool _EditMode;
        public bool EditMode {
            get { return _EditMode; }

            set {
                _EditMode = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "对外方法"
        public void SetBorderBrush(SolidColorBrush brush)
        {
            rootBorder.BorderBrush = brush;
        }
        public void SetBackground(SolidColorBrush brush)
        {
            rootBorder.Background = brush;
        }

        public void SetEditMode(bool mode)
        {
            this.EditMode = mode;
        }


        public static int GetImageHeight(int mode, int width)
        {
            if (mode == 0) {
                return (int)((double)width * (200 / 147));
            } else if (mode == 1) {
                return (int)(width * 540f / 800f);
            } else if (mode == 2) {
                return (int)(width * 540f / 800f);
            }
            return width;
        }

        #endregion
    }
}
