using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using SuperControls.Style;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.Core.UserControls.ObjectEventArgs;

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


        #region "事件"

        public static Action onStatistic;
        public static Action<long, long> onTagStampRemove;


        public static readonly RoutedEvent OnGradeChangeEvent =
            EventManager.RegisterRoutedEvent("OnGradeChange", RoutingStrategy.Bubble,
                typeof(ObjectEventArgsHandler), typeof(ViewVideo));

        public event ObjectEventArgsHandler OnGradeChange {
            add => AddHandler(OnGradeChangeEvent, value);
            remove => RemoveHandler(OnGradeChangeEvent, value);
        }

        public static readonly RoutedEvent OnViewAssoDataEvent =
            EventManager.RegisterRoutedEvent("OnViewAssoData", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ViewVideo));

        public event RoutedEventHandler OnViewAssoData {
            add => AddHandler(OnViewAssoDataEvent, value);
            remove => RemoveHandler(OnViewAssoDataEvent, value);
        }


        public static readonly RoutedEvent OnTagStampRemoveEvent =
            EventManager.RegisterRoutedEvent("OnTagStampRemove", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ViewVideo));

        public event RoutedEventHandler OnTagStampRemove {
            add => AddHandler(OnTagStampRemoveEvent, value);
            remove => RemoveHandler(OnTagStampRemoveEvent, value);
        }

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


        #region "属性"
        private bool canShowDetails { get; set; }
        private bool CanRateChange { get; set; }

        #endregion


        #region "控件属性"

        public static readonly DependencyProperty AssoVisibleProperty = DependencyProperty.Register(
nameof(AssoVisible), typeof(bool), typeof(ViewVideo), new PropertyMetadata(true));

        public bool AssoVisible {
            get { return (bool)GetValue(AssoVisibleProperty); }
            set {
                SetValue(AssoVisibleProperty, value);
            }
        }

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



        public ViewVideo()
        {
            InitializeComponent();
        }



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
            RaiseEvent(new RoutedEventArgs(OnViewAssoDataEvent) { Source = this });
        }

        private void ShowSubSection(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            Video video = GetVideo(this);
            if (video == null)
                return;

            ContextMenu contextMenu = element.ContextMenu;
            contextMenu.Items.Clear();

            if (video != null && video.SubSectionList?.Count > 0) {
                for (int i = 0; i < video.SubSectionList.Count; i++) {
                    string filepath = video.SubSectionList[i].Value; // 这样可以，放在  PlayVideoWithPlayer 就超出索引
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = i + 1;
                    menuItem.Click += (s, _) => {
                        Video.PlayVideoWithPlayer(filepath, video.DataID);
                    };
                    contextMenu.Items.Add(menuItem);
                }

                contextMenu.IsOpen = true;
            }
        }


        private Video GetVideo(FrameworkElement ele)
        {
            if (ele != null && ele.Tag != null &&
                ele.Tag is Video video) {
                return video;
            }
            return null;
        }


        public void PlayVideo(object sender, MouseButtonEventArgs e)
        {
            if (!EditMode)
                RaiseEvent(new RoutedEventArgs(OnPlayVideoEvent) { Source = this });
        }

        private void CopyText(object sender, MouseButtonEventArgs e)
        {
            string text = "";
            if (sender is TextBlock textBlock) {
                text = textBlock.Text;
            } else if (sender is TextBox textBox) {
                text = textBox.Text;
            }
            SuperUtils.IO.ClipBoard.TrySetDataObject(text);
            MessageNotify.Success($"{LangManager.GetValueByKey("Message_Copied")} {text}");
        }

        private void DeleteVideoTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Border border = (menuItem.Parent as ContextMenu).PlacementTarget as Border;
            long.TryParse(border.Tag.ToString(), out long tagID);
            if (tagID <= 0)
                return;

            Video video = GetVideo(this);
            if (video == null)
                return;

            long dataID = video.DataID;

            ItemsControl itemsControl = border.FindParentOfType<ItemsControl>();
            if (itemsControl == null)
                return;

            ObservableCollection<TagStamp> tagStamps = itemsControl.ItemsSource as ObservableCollection<TagStamp>;
            if (tagStamps == null)
                return;
            TagStamp tagStamp = tagStamps.FirstOrDefault(arg => arg.TagID.Equals(tagID));
            if (tagStamp != null) {
                tagStamps.Remove(tagStamp);
                string sql = $"delete from metadata_to_tagstamp where TagID='{tagID}' and DataID='{dataID}'";
                MapperManager.tagStampMapper.ExecuteNonQuery(sql);
                Video newVideo = MapperManager.videoMapper.SelectVideoByID(dataID);
                video.TagIDs = video.TagIDs;
                // 通知上层数目变化了
                RaiseEvent(new RoutedEventArgs(OnTagStampRemoveEvent) { Source = this });
                onTagStampRemove?.Invoke(dataID, tagID);
            }
        }

        private void OnRateMouseDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }

        private void Rate_ValueChanged(object sender, SuperControls.Style.FunctionEventArgs<double> e)
        {
            if (!CanRateChange)
                return;

            if (sender is Rate rate &&
                rate.Tag != null &&
                long.TryParse(rate.Tag.ToString(), out long DataID) && DataID > 0) {
                MapperManager.metaDataMapper.UpdateFieldById("Grade", rate.Value.ToString(), DataID);
                onStatistic?.Invoke();
                RaiseEvent(new ObjectEventArgs((float)rate.Value, OnGradeChangeEvent, sender));
            }
            CanRateChange = false;
        }

        #region "对外方法"
        public void SetBorderBrush(SolidColorBrush brush)
        {
            rootBorder.BorderBrush = brush;
        }
        public void SetBackground(SolidColorBrush brush)
        {
            // rootBorder.Background = brush;
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
