using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.IO;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using static Jvedio.Core.UserControls.Filter;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// Filter.xaml 的交互逻辑
    /// </summary>
    public partial class Filter : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "事件"

        public event Action Close;

        public static Action onTagStampDelete;
        public static Action<long> onTagStampRefresh;

        private delegate void AsyncLoadItemDelegate(UIElementCollection collection, UIElement item);

        private void AsyncLoadItem(UIElementCollection collection, UIElement item) => collection.Add(item);


        #endregion

        #region "静态属性"

        private static Main MainWindow = SuperUtils.WPF.VisualTools.WindowHelper.GetWindowByName("Main", App.Current.Windows) as Main;

        private static List<string> FilterList { get; set; } = new List<string>()
         {
            "Year",
            "Month",
            "Duration",
            "Genre",
            "Grade",
            "Series",
            "Director",
            "Studio",
            "Publisher",
        };

        List<string> BaseList { get; set; } = new List<string>()
        {
            LangManager.GetValueByKey("Playable"),
            LangManager.GetValueByKey("UnPlayable"),
            LangManager.GetValueByKey("SubsectionVedio"),
            LangManager.GetValueByKey("WithMagnets"),
        };

        List<string> ImageList { get; set; } = new List<string>()
        {
            LangManager.GetValueByKey("Poster"),
            LangManager.GetValueByKey("Thumbnail"),
            LangManager.GetValueByKey("Preview"),
            LangManager.GetValueByKey("ScreenShot"),
        };

        List<string> VideoType { get; set; } = new List<string>()
        {
            LangManager.GetValueByKey("Normal"),
            LangManager.GetValueByKey("Uncensored"),
            LangManager.GetValueByKey("Censored"),
            LangManager.GetValueByKey("Europe"),
        };


        List<string> TimeList { get; set; } = new List<string>()
        {
            "<30min",
            "30min-1h",
            "1h-2h",
            ">2h"
        };

        #endregion

        #region "属性"

        private ObservableCollection<TagStamp> _TagStamps = new ObservableCollection<TagStamp>();

        public ObservableCollection<TagStamp> TagStamps {
            get { return _TagStamps; }

            set {
                _TagStamps = value;
                RaisePropertyChanged();
            }
        }


        private int _GenreProgress;
        public int GenreProgress {
            get { return _GenreProgress; }
            set {
                _GenreProgress = value;
                RaisePropertyChanged();
            }
        }
        private int _SeriesProgress;
        public int SeriesProgress {
            get { return _SeriesProgress; }
            set {
                _SeriesProgress = value;
                RaisePropertyChanged();
            }
        }
        private int _DirectorProgress;
        public int DirectorProgress {
            get { return _DirectorProgress; }
            set {
                _DirectorProgress = value;
                RaisePropertyChanged();
            }
        }
        private int _StudioProgress;
        public int StudioProgress {
            get { return _StudioProgress; }
            set {
                _StudioProgress = value;
                RaisePropertyChanged();
            }
        }


        private LoadState _CommonLoad;
        public LoadState CommonLoad {
            get { return _CommonLoad; }
            set {
                _CommonLoad = value;
                RaisePropertyChanged();
            }
        }

        private LoadState _GenreLoad;
        public LoadState GenreLoad {
            get { return _GenreLoad; }
            set {
                _GenreLoad = value;
                RaisePropertyChanged();
            }
        }

        private LoadState _SeriesLoad;
        public LoadState SeriesLoad {
            get { return _SeriesLoad; }
            set {
                _SeriesLoad = value;
                RaisePropertyChanged();
            }
        }
        private LoadState _DirectorLoad;
        public LoadState DirectorLoad {
            get { return _DirectorLoad; }
            set {
                _DirectorLoad = value;
                RaisePropertyChanged();
            }
        }
        private LoadState _StudioLoad;
        public LoadState StudioLoad {
            get { return _StudioLoad; }
            set {
                _StudioLoad = value;
                RaisePropertyChanged();
            }
        }

        #endregion




        public Filter()
        {
            InitializeComponent();
        }

        private void Filter_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTagStamp();
        }


        public void LoadAll()
        {
            SetCommonFilter();
            LoadData();
            LoadTagStamp();
        }

        /// <summary>
        /// 加载默认
        /// </summary>
        private void SetCommonFilter()
        {
            AddItem(BaseList, basicWrapPanel);
            AddItem(VideoType, videoTypeWrapPanel);
            AddItem(ImageList, imageWrapPanel);
            AddItem(TimeList, durationWrapPanel);
        }

        private void LoadTagStamp(List<TagStamp> beforeTagStamps = null)
        {
            Task.Run(async () => {
                await Task.Delay(1000);

                Dispatcher.Invoke(() => {
                    TagStamps = TagStamp.InitTagStamp(beforeTagStamps);
                    TagStampItemsControl.ItemsSource = null;
                    TagStampItemsControl.ItemsSource = TagStamps;
                });
            });

        }

        private async void AddItem(ICollection<string> list, WrapPanel panel, Action complete = null, Action<int> onProgress = null)
        {
            panel.Children.Clear();
            int idx = 0;
            int total = list.Count;
            foreach (string item in list) {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                   new AsyncLoadItemDelegate(AsyncLoadItem), panel.Children, buildToggleButton(item));
                idx++;
                float progress = ((float)idx / (float)total * 100);
                onProgress?.Invoke((int)progress);
            }
            complete?.Invoke();
        }

        private ToggleButton buildToggleButton(string content, bool isChecked = false)
        {
            ToggleButton toggleButton = new ToggleButton();

            toggleButton.Content = content;
            toggleButton.IsChecked = isChecked;
            toggleButton.Style = (System.Windows.Style)this.Resources["FilterToggleButton"];
            return toggleButton;
        }

        /// <summary>
        /// 加载过滤器
        /// </summary>
        private void LoadData()
        {
            string sql = $"SELECT DISTINCT ReleaseDate FROM metadata " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);

            // 2020-02-10
            List<string> dates = list.Select(x => x["ReleaseDate"].ToString())
                .Where(arg => !string.IsNullOrEmpty(arg) && arg.LastIndexOf('-') > 5).ToList();

            HashSet<string> years = dates.Select(arg => arg.Split('-')[0]).ToHashSet().OrderBy(x => x).Where(arg => arg != "1900" && arg != "0001").ToHashSet();
            HashSet<string> months = dates.Select(arg => arg.Split('-')[1]).ToHashSet().OrderBy(x => x).ToHashSet();

            AddItem(years, yearWrapPanel);
            AddItem(months, monthWrapPanel);

            //LoadSingleData(publisherWrapPanel, "Publisher"); // 发行商
        }
        private void LoadSingleDataFromMetaData(WrapPanel wrapPanel, string field)
        {
            if (GenreLoad == LoadState.Loaded)
                return;
            GenreLoad = LoadState.Loading;
            string sql = $"SELECT DISTINCT {field} FROM metadata " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<string> dataList = list.Select(x => x[field].ToString())
                 .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            HashSet<string> set = new HashSet<string>();
            foreach (string item in dataList)
                foreach (string data in item.Split(SuperUtils.Values.ConstValues.Separator))
                    set.Add(data);
            AddItem(set, wrapPanel, () => GenreLoad = LoadState.Loaded, (value) => GenreProgress = value);
        }

        private void LoadSingleData(WrapPanel wrapPanel, string field, Action before = null, Action complete = null, Action<int> onProgress = null)
        {
            before?.Invoke();
            string sql = $"SELECT DISTINCT {field} FROM metadata_video join metadata on metadata.DataID=metadata_video.DataID " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<string> dataList = list.Select(x => x[field].ToString())
                 .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            HashSet<string> set = new HashSet<string>();
            foreach (string item in dataList)
                foreach (string data in item.Split(SuperUtils.Values.ConstValues.Separator))
                    set.Add(data);

            AddItem(set, wrapPanel, () => complete?.Invoke(), (value) => onProgress?.Invoke(value));
        }

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void HideGrid(object sender, RoutedEventArgs e)
        {
            Close?.Invoke();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            CommonLoad = LoadState.None;
            GenreLoad = LoadState.None;
            SeriesLoad = LoadState.None;
            DirectorLoad = LoadState.None;
            StudioLoad = LoadState.None;

            LoadAll();
        }

        private void PathCheckButton_Click(object sender, RoutedEventArgs e)
        {

        }



        private void NewTagStamp(object sender, RoutedEventArgs e)
        {
            Window_TagStamp window_TagStamp = new Window_TagStamp();
            window_TagStamp.Owner = MainWindow;
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog) {
                string name = window_TagStamp.TagName;
                if (string.IsNullOrEmpty(name))
                    return;
                SolidColorBrush backgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;

                TagStamp tagStamp = new TagStamp() {
                    TagName = name,
                    Foreground = VisualHelper.SerializeBrush(ForegroundBrush),
                    Background = VisualHelper.SerializeBrush(backgroundBrush),
                };
                tagStampMapper.Insert(tagStamp);
                InitTagStamp();

            }
        }

        public void InitTagStamp()
        {
            // 记住之前的状态
            List<TagStamp> tagStamps = TagStamps.ToList();
            TagStamp.TagStamps = tagStampMapper.GetAllTagStamp();
            if (tagStamps != null && tagStamps.Count > 0) {
                foreach (var item in TagStamp.TagStamps) {
                    TagStamp tagStamp = tagStamps.FirstOrDefault(arg => arg.TagID == item.TagID);
                    if (tagStamp != null)
                        item.Selected = tagStamp.Selected;
                }
            }
            LoadTagStamp(tagStamps);
        }

        private void EditTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string tag = (contextMenu.PlacementTarget as PathCheckButton).Tag.ToString();
            long.TryParse(tag, out long id);
            if (id <= 0)
                return;

            TagStamp tagStamp = TagStamp.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();
            Window_TagStamp window_TagStamp = new Window_TagStamp(tagStamp.TagName, tagStamp.BackgroundBrush, tagStamp.ForegroundBrush);
            bool? dialog = window_TagStamp.ShowDialog();
            if ((bool)dialog) {
                string name = window_TagStamp.TagName;
                if (string.IsNullOrEmpty(name))
                    return;
                SolidColorBrush backgroundBrush = window_TagStamp.BackgroundBrush;
                SolidColorBrush ForegroundBrush = window_TagStamp.ForegroundBrush;
                tagStamp.TagName = name;
                tagStamp.Background = VisualHelper.SerializeBrush(backgroundBrush);
                tagStamp.Foreground = VisualHelper.SerializeBrush(ForegroundBrush);
                tagStampMapper.UpdateById(tagStamp);
                InitTagStamp();
                onTagStampRefresh?.Invoke(id);
            }
        }


        private void DeleteTagStamp(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string tag = (contextMenu.PlacementTarget as PathCheckButton).Tag.ToString();
            long.TryParse(tag, out long id);
            if (id <= 0)
                return;
            TagStamp tagStamp = TagStamp.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault();
            if (tagStamp.IsSystemTag()) {
                MessageNotify.Error(LangManager.GetValueByKey("CanNotDeleteDefaultTag"));
                return;
            }


            if (new MsgBox(SuperControls.Style.LangManager.GetValueByKey("IsToDelete") + $"{LangManager.GetValueByKey("TagStamp")} 【{tagStamp.TagName}】").ShowDialog() == true) {
                tagStampMapper.DeleteById(id);

                // 删除
                string sql = $"delete from metadata_to_tagstamp where TagID={tagStamp.TagID};";
                tagStampMapper.ExecuteNonQuery(sql);
                InitTagStamp();
                onTagStampDelete?.Invoke();
                // todo 更新详情窗口
            }
        }


        private void SetTagStampsSelected(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            bool allChecked = (bool)toggleButton.IsChecked;
            ItemsControl itemsControl = TagStampItemsControl;
            for (int i = 0; i < itemsControl.Items.Count; i++) {
                ContentPresenter presenter = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                if (presenter == null)
                    continue;
                PathCheckButton button = VisualHelper.FindElementByName<PathCheckButton>(presenter, "pathCheckButton");
                if (button == null)
                    continue;
                button.IsChecked = allChecked;
            }

            // todo tab
            //vieModel.LoadData();
        }

        private void TogglePanel_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsExpanded)
                SetCommonFilter();
        }

        private void Genre_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsExpanded)
                LoadSingleDataFromMetaData(genreWrapPanel, "Genre"); // 类别
        }

        private void Series_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsExpanded && SeriesLoad != LoadState.Loaded)
                LoadSingleData(seriesWrapPanel, "Series", () => SeriesLoad = LoadState.Loading, () => SeriesLoad = LoadState.Loaded, (value) => SeriesProgress = value); // 系列
        }

        private void Director_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsExpanded && DirectorLoad != LoadState.Loaded)
                LoadSingleData(directorWrapPanel, "Director", () => DirectorLoad = LoadState.Loading, () => DirectorLoad = LoadState.Loaded, (value) => DirectorProgress = value); // 系列

        }

        private void Studio_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsExpanded && StudioLoad != LoadState.Loaded)
                LoadSingleData(studioWrapPanel, "Studio", () => StudioLoad = LoadState.Loading, () => StudioLoad = LoadState.Loaded, (value) => StudioProgress = value); // 系列
        }
    }

    public enum LoadState
    {
        None,
        Loading,
        Loaded,
    }



    public class LoadStatusConverter : IValueConverter
    {
        // 数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) {
                return false;
            }

            Enum.TryParse(value.ToString(), out LoadState state);

            if (state == LoadState.Loading) {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        // 选中项地址转换为数字
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
