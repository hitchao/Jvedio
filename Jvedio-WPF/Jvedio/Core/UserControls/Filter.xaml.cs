using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using SuperControls.Style;
using SuperUtils.IO;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
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

        #endregion



        public Filter()
        {
            InitializeComponent();
        }

        private void Filter_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }


        public void Init()
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

        private void AddItem(ICollection<string> list, WrapPanel panel)
        {
            panel.Children.Clear();
            foreach (string item in list)
                panel.Children.Add(buildToggleButton(item));
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


            LoadSingleDataFromMetaData(genreWrapPanel, "Genre"); // 类别
            LoadSingleData(seriesWrapPanel, "Series"); // 系列
            LoadSingleData(directorWrapPanel, "Director"); // 导演
            LoadSingleData(studioWrapPanel, "Studio"); // 制作商
            //LoadSingleData(publisherWrapPanel, "Publisher"); // 发行商
        }
        private void LoadSingleDataFromMetaData(WrapPanel wrapPanel, string field)
        {
            string sql = $"SELECT DISTINCT {field} FROM metadata " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<string> dataList = list.Select(x => x[field].ToString())
                 .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            HashSet<string> set = new HashSet<string>();
            foreach (string item in dataList)
                foreach (string data in item.Split(SuperUtils.Values.ConstValues.Separator))
                    set.Add(data);
            AddItem(set, wrapPanel);
        }

        private void LoadSingleData(WrapPanel wrapPanel, string field)
        {
            string sql = $"SELECT DISTINCT {field} FROM metadata_video join metadata on metadata.DataID=metadata_video.DataID " +
                    $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0}";

            List<Dictionary<string, object>> list = MapperManager.metaDataMapper.Select(sql);
            List<string> dataList = list.Select(x => x[field].ToString())
                 .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            HashSet<string> set = new HashSet<string>();
            foreach (string item in dataList)
                foreach (string data in item.Split(SuperUtils.Values.ConstValues.Separator))
                    set.Add(data);
            AddItem(set, wrapPanel);
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
            Init();
        }

        private void PathCheckButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditTagStamp(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteTagStamp(object sender, RoutedEventArgs e)
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
            Main.TagStamps = tagStampMapper.GetAllTagStamp();
            if (tagStamps != null && tagStamps.Count > 0) {
                foreach (var item in Main.TagStamps) {
                    TagStamp tagStamp = tagStamps.FirstOrDefault(arg => arg.TagID == item.TagID);
                    if (tagStamp != null)
                        item.Selected = tagStamp.Selected;
                }
            }
            LoadTagStamp(tagStamps);
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

    }
}
