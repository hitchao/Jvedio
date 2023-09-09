using Jvedio.Core.CustomEventArgs;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Wrapper;
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
using System.Windows.Media;
using System.Windows.Threading;
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


        private const long MB_TO_B = 1024 * 1024;


        #region "事件"

        public event Action Close;

        public static Action<long> onTagStampDelete { get; set; }
        public static Action<long> onTagStampRefresh { get; set; }

        public event EventHandler OnApplyWrapper;

        private delegate void AsyncLoadItemDelegate(UIElementCollection collection, UIElement item);

        private void AsyncLoadItem(UIElementCollection collection, UIElement item) => collection.Add(item);


        #endregion

        #region "静态属性"

        private static Main MainWindow { get; set; }

        private static List<int> TimeList { get; set; } =
            new List<int>() { 0, 30, 60, 120, 240, 360 };

        /// <summary>
        /// 单位 MB
        /// </summary>
        private static List<long> SizeList { get; set; } =
            new List<long>() { 0L * MB_TO_B, 500L * MB_TO_B, 1000L * MB_TO_B, 2000L * MB_TO_B, 3000L * MB_TO_B };

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


        // ************************************
        // ************* 记忆是否展开 *********
        // ************************************


        private bool _ExpandTag;
        public bool ExpandTag {
            get { return _ExpandTag; }
            set {
                _ExpandTag = value;
                RaisePropertyChanged();
                if (ConfigManager.FilterConfig != null)
                    ConfigManager.FilterConfig.ExpandTag = value;
            }
        }

        private bool _ExpandCommon;
        public bool ExpandCommon {
            get { return _ExpandCommon; }
            set {
                _ExpandCommon = value;
                RaisePropertyChanged();
                if (ConfigManager.FilterConfig != null)
                    ConfigManager.FilterConfig.ExpandCommon = value;
            }
        }

        private bool _ExpandGenre;
        public bool ExpandGenre {
            get { return _ExpandGenre; }
            set {
                _ExpandGenre = value;
                RaisePropertyChanged();
                if (ConfigManager.FilterConfig != null)
                    ConfigManager.FilterConfig.ExpandGenre = value;
            }
        }

        private bool _ExpandSeries;
        public bool ExpandSeries {
            get { return _ExpandSeries; }
            set {
                _ExpandSeries = value;
                RaisePropertyChanged();
                if (ConfigManager.FilterConfig != null)
                    ConfigManager.FilterConfig.ExpandSeries = value;
            }
        }
        private bool _ExpandDirector;
        public bool ExpandDirector {
            get { return _ExpandDirector; }
            set {
                _ExpandDirector = value;
                RaisePropertyChanged();
                if (ConfigManager.FilterConfig != null)
                    ConfigManager.FilterConfig.ExpandDirector = value;
            }
        }
        private bool _ExpandStudio;
        public bool ExpandStudio {
            get { return _ExpandStudio; }
            set {
                _ExpandStudio = value;
                RaisePropertyChanged();
                if (ConfigManager.FilterConfig != null)
                    ConfigManager.FilterConfig.ExpandStudio = value;
            }
        }


        #endregion


        static Filter()
        {
            MainWindow = SuperUtils.WPF.VisualTools.WindowHelper.GetWindowByName("Main", App.Current.Windows) as Main;
        }


        public Filter()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            InitProp();
            LoadAll();
            BindEvent();
        }

        private void BindEvent()
        {
            Window_Details.onRemoveTagStamp += onRemoveTagStamp;
        }


        private void onRemoveTagStamp()
        {
            this.InitTagStamp();
        }


        /// <summary>
        /// 用户控件的属性不能直接使用 ConfigManager
        /// </summary>
        public void InitProp()
        {
            ExpandTag = ConfigManager.FilterConfig.ExpandTag;
            ExpandCommon = ConfigManager.FilterConfig.ExpandCommon;
            ExpandGenre = ConfigManager.FilterConfig.ExpandGenre;
            ExpandSeries = ConfigManager.FilterConfig.ExpandSeries;
            ExpandDirector = ConfigManager.FilterConfig.ExpandDirector;
            ExpandStudio = ConfigManager.FilterConfig.ExpandStudio;
        }

        public void LoadAll()
        {
            if (ExpandTag)
                InitTagStamp();
            if (ExpandCommon)
                SetCommonFilter();
            if (ExpandGenre)
                LoadGenre();
            if (ExpandSeries)
                LoadSeries();
            if (ExpandDirector)
                LoadDirector();
            if (ExpandStudio)
                LoadStudio();

        }

        /// <summary>
        /// 加载默认
        /// </summary>
        private void SetCommonFilter()
        {
            if (CommonLoad == LoadState.Loaded)
                return;
            CommonLoad = LoadState.Loading;
            LoadYearMonth();
        }

        private void LoadTagStamp(List<TagStamp> beforeTagStamps = null)
        {
            Task.Run(async () => {
                await Task.Delay(200);

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
        private void LoadYearMonth()
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
            AddItem(months, monthWrapPanel, () => CommonLoad = LoadState.Loaded);
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

        private void HideGrid(object sender, RoutedEventArgs e)
        {
            Close?.Invoke();
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
            ApplyFilter();
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
                onTagStampDelete?.Invoke(tagStamp.TagID);
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
            ApplyFilter();
        }


        private void TagStamp_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsLoaded && panel.IsExpanded)
                InitTagStamp();
        }

        private void Common_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsLoaded && panel.IsExpanded)
                SetCommonFilter();
        }

        private void Genre_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsLoaded && panel.IsExpanded)
                LoadGenre();

        }

        private void Series_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsLoaded && panel.IsExpanded && SeriesLoad != LoadState.Loaded)
                LoadSeries();
        }

        private void Director_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsLoaded && panel.IsExpanded && DirectorLoad != LoadState.Loaded)
                LoadDirector();

        }

        private void Studio_Expand(object sender, EventArgs e)
        {
            if (sender is TogglePanel panel && panel.IsLoaded && panel.IsExpanded && StudioLoad != LoadState.Loaded)
                LoadStudio();
        }

        public void LoadGenre()
        {
            LoadSingleDataFromMetaData(genreWrapPanel, "Genre"); // 类别
        }
        public void LoadSeries()
        {
            LoadSingleData(seriesWrapPanel, "Series", () => SeriesLoad = LoadState.Loading, () => SeriesLoad = LoadState.Loaded, (value) => SeriesProgress = value); // 系列
        }
        public void LoadDirector()
        {
            LoadSingleData(directorWrapPanel, "Director", () => DirectorLoad = LoadState.Loading, () => DirectorLoad = LoadState.Loaded, (value) => DirectorProgress = value); // 系列
        }
        public void LoadStudio()
        {
            LoadSingleData(studioWrapPanel, "Studio", () => StudioLoad = LoadState.Loading, () => StudioLoad = LoadState.Loaded, (value) => StudioProgress = value); // 系列
        }


        private void SetAllSelected(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton panel &&
                panel.Parent is FrameworkElement ele &&
                ele.Parent is DockPanel dockPanel &&
                dockPanel.Children.OfType<WrapPanel>().Last() is WrapPanel wrapPanel) {
                List<ToggleButton> list = wrapPanel.Children.OfType<ToggleButton>().ToList();

                bool all = (bool)panel.IsChecked;

                if (list != null) {
                    foreach (ToggleButton item in list) {
                        item.IsChecked = all;
                    }
                }


            }
        }

        private void ApplyFilter(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void SetPlayable(object sender, RoutedEventArgs e)
        {
            if (!ConfigManager.Settings.PlayableIndexCreated) {
                MessageNotify.Error(LangManager.GetValueByKey("PleaseSetExistsIndex"));
                return;
            }
            ApplyFilter();
        }
        private void SetPictureType(object sender, RoutedEventArgs e)
        {
            if (!ConfigManager.Settings.PictureIndexCreated) {
                MessageNotify.Error(LangManager.GetValueByKey("PleaseSetImageIndex"));
                return;
            }
            SetAllChecked(sender, e);
        }


        public void ApplyFilter()
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            // 1.标签戳

            string sql = "";



            // 标记
            if (TagStamps != null && TagStamps.Count > 0) {
                bool allFalse = TagStamps.All(item => item.Selected == false);
                if (allFalse) {
                    wrapper.IsNull("TagID");
                    sql += VideoMapper.SQL_LEFT_JOIN_TAGSTAMP;
                } else {
                    bool allTrue = TagStamps.All(item => item.Selected == true);
                    if (!allTrue) {
                        wrapper.In("metadata_to_tagstamp.TagID", TagStamps.Where(item => item.Selected == true).Select(item => item.TagID.ToString()));
                        sql += VideoMapper.SQL_JOIN_TAGSTAMP;
                    }
                }
            }


            // 分段视频

            // 是否可播放
            if (ConfigManager.Settings.PlayableIndexCreated) {
                List<RadioButton> plays = playWrapPanel.Children.OfType<RadioButton>().ToList();
                int idx = 0;
                for (int i = 0; i < plays.Count; i++) {
                    if ((bool)plays[i].IsChecked) {
                        idx = i;
                        break;
                    }
                }
                if (idx > 0) {
                    wrapper.Eq("metadata.PathExist", idx - 1);
                }
            }

            // 视频类型
            List<ToggleButton> allMenus = videoTypeWrapPanel.Children.OfType<ToggleButton>().ToList();
            List<ToggleButton> checkedMenus = allMenus.Where(t => (bool)t.IsChecked).ToList();
            int checkedCount = checkedMenus.Count;

            string field = "";

            if (checkedCount > 0 && checkedCount < 4) {
                field = "VideoType";
                if (checkedCount == 1) {
                    int idx = allMenus.IndexOf(checkedMenus[0]) - 1;
                    if (idx >= 0)
                        wrapper.Eq(field, idx);
                } else if (checkedCount == 2) {
                    int idx1 = allMenus.IndexOf(checkedMenus[0]) - 1;
                    int idx2 = allMenus.IndexOf(checkedMenus[1]) - 1;
                    if (idx1 >= 0 && idx2 >= 0)
                        wrapper.Eq(field, idx1).LeftBracket().Or().Eq(field, idx2).RightBracket();
                } else if (checkedCount == 3) {
                    int idx1 = allMenus.IndexOf(checkedMenus[0]) - 1;
                    int idx2 = allMenus.IndexOf(checkedMenus[1]) - 1;
                    int idx3 = allMenus.IndexOf(checkedMenus[2]) - 1;
                    if (idx1 >= 0 && idx2 >= 0 && idx3 >= 0)
                        wrapper.Eq(field, idx1).LeftBracket().Or().Eq(field, idx2).Or().Eq(field, idx3).RightBracket();
                }
            }


            // 1. 仅显示分段视频
            if ((bool)OnlyShowSubsection.IsChecked)
                wrapper.NotEq("SubSection", string.Empty);

            // 图片显示模式
            if (ConfigManager.Settings.PictureIndexCreated) {
                List<RadioButton> plays = pictureWrapPanel.Children.OfType<RadioButton>().ToList();
                int idx = 0;
                for (int i = 0; i < plays.Count; i++) {
                    if ((bool)plays[i].IsChecked) {
                        idx = i;
                        break;
                    }
                }
                if (idx > 0) {
                    int exists = (bool)pictureReverse.IsChecked ? 0 : 1;
                    sql += VideoMapper.SQL_JOIN_COMMON_PICTURE_EXIST;
                    wrapper.Eq("common_picture_exist.PathType", ConfigManager.Settings.PicPathMode)
                        .Eq("common_picture_exist.ImageType", idx - 1)
                        .Eq("common_picture_exist.Exist", exists);
                }
            }

            // 时长
            List<ToggleButton> timeList = timeWrapPanel.Children.OfType<ToggleButton>().ToList();
            ToggleButton timeButton = timeList.FirstOrDefault(item => (bool)item.IsChecked);
            if (timeButton != null && timeList.IndexOf(timeButton) is int timeIndex && timeIndex > 0) {
                field = "Duration";
                if (timeIndex < 5) {
                    wrapper.Ge(field, TimeList[timeIndex - 1]).Le(field, TimeList[timeIndex]);
                } else if (timeIndex == 5) {
                    wrapper.Ge(field, TimeList[timeIndex]);
                }
            }

            // 文件大小
            List<RadioButton> sizeList = sizeWrapPanel.Children.OfType<RadioButton>().ToList();
            RadioButton sizeButton = sizeList.FirstOrDefault(item => (bool)item.IsChecked);
            if (sizeButton != null && sizeList.IndexOf(sizeButton) is int sizeIndex && sizeIndex > 0) {
                field = "Size";
                if (sizeIndex < 5) {
                    wrapper.Ge(field, SizeList[sizeIndex - 1]).Le(field, SizeList[sizeIndex]);
                } else if (sizeIndex == 5) {
                    wrapper.Ge(field, SizeList[sizeIndex - 1]);
                }
            }

            // 评分
            double minRate = rateSlider.MinValue;
            double maxRate = rateSlider.MaxValue;
            if (minRate != rateSlider.Minimum || maxRate != rateSlider.Maximum) {
                field = "Grade";
                if (minRate == maxRate) {
                    wrapper.Eq(field, minRate);
                } else {
                    wrapper.Ge(field, minRate);
                    wrapper.Le(field, maxRate);
                }
            }

            // 年份

            List<ToggleButton> yearList = yearWrapPanel.Children.OfType<ToggleButton>().Where(item => (bool)item.IsChecked).ToList();
            if (yearList.Count > 0 && yearList.Count != yearWrapPanel.Children.Count) {
                field = "ReleaseYear";
                int count = yearList.Count;
                List<int> list = yearList.Select(item => int.Parse(item.Content.ToString())).ToList();
                wrapper.Eq(field, list[0]).LeftBracket().Or();
                for (int i = 1; i < count - 1; i++) {
                    wrapper.Eq(field, list[i]).Or();
                }
                wrapper.Eq(field, list[count - 1]).RightBracket();

            }

            // 月份
            //List<ToggleButton> monthList = monthWrapPanel.Children.OfType<ToggleButton>().Where(item => (bool)item.IsChecked).ToList();
            //if (monthList.Count > 0 && monthList.Count != monthWrapPanel.Children.Count) {
            //    field = "ReleaseDate";
            //    int count = monthList.Count;
            //    List<int> list = monthList.Select(item => int.Parse(item.Content.ToString())).ToList();
            //    wrapper.Eq(field, list[0]).LeftBracket().Or();
            //    for (int i = 1; i < count - 1; i++) {
            //        wrapper.Eq(field, list[i]).Or();
            //    }
            //    wrapper.Eq(field, list[count - 1]).RightBracket();

            //}

            // 类别
            List<ToggleButton> genreList = genreWrapPanel.Children.OfType<ToggleButton>().Where(item => (bool)item.IsChecked).ToList();
            if (genreList.Count > 0 && genreList.Count != genreWrapPanel.Children.Count) {
                field = "Genre";
                int count = genreList.Count;
                List<string> list = genreList.Select(item => item.Content.ToString()).ToList();
                wrapper.Like(field, list[0]).LeftBracket().Or();
                for (int i = 1; i < count - 1; i++) {
                    wrapper.Like(field, list[i]).Or();
                }
                wrapper.Like(field, list[count - 1]).RightBracket();
            }

            // 系列
            List<ToggleButton> seriesList = seriesWrapPanel.Children.OfType<ToggleButton>().Where(item => (bool)item.IsChecked).ToList();
            if (seriesList.Count > 0 && seriesList.Count != seriesWrapPanel.Children.Count) {
                field = "Series";
                int count = seriesList.Count;
                List<string> list = seriesList.Select(item => item.Content.ToString()).ToList();
                wrapper.Like(field, list[0]).LeftBracket().Or();
                for (int i = 1; i < count - 1; i++) {
                    wrapper.Like(field, list[i]).Or();
                }
                wrapper.Like(field, list[count - 1]).RightBracket();
            }

            // 导演
            List<ToggleButton> directorList = directorWrapPanel.Children.OfType<ToggleButton>().Where(item => (bool)item.IsChecked).ToList();
            if (directorList.Count > 0 && directorList.Count != directorWrapPanel.Children.Count) {
                field = "Director";
                int count = directorList.Count;
                List<string> list = directorList.Select(item => item.Content.ToString()).ToList();
                wrapper.Like(field, list[0]).LeftBracket().Or();
                for (int i = 1; i < count - 1; i++) {
                    wrapper.Like(field, list[i]).Or();
                }
                wrapper.Like(field, list[count - 1]).RightBracket();
            }

            // 系列
            List<ToggleButton> studioList = studioWrapPanel.Children.OfType<ToggleButton>().Where(item => (bool)item.IsChecked).ToList();
            if (studioList.Count > 0 && studioList.Count != studioWrapPanel.Children.Count) {
                field = "Studio";
                int count = studioList.Count;
                List<string> list = studioList.Select(item => item.Content.ToString()).ToList();
                wrapper.Like(field, list[0]).LeftBracket().Or();
                for (int i = 1; i < count - 1; i++) {
                    wrapper.Like(field, list[i]).Or();
                }
                wrapper.Like(field, list[count - 1]).RightBracket();
            }

            WrapperEventArg<Video> arg = new WrapperEventArg<Video>(wrapper);
            arg.SQL = sql;

            OnApplyWrapper?.Invoke(this, arg);
        }

        private void SetAllChecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.Parent is WrapPanel panel &&
                panel.Children.OfType<ToggleButton>().ToList() is List<ToggleButton> list &&
                list.IndexOf(toggleButton) is int idx &&
                idx >= 0) {
                if (idx == 0) {
                    list.ForEach((arg) => arg.IsChecked = false);
                    toggleButton.IsChecked = true;
                } else {
                    list[0].IsChecked = !list.Any(arg => (bool)arg.IsChecked);
                }
            }
        }

        private WrapPanel GetWrapPanel(FrameworkElement ele)
        {
            if (ele.Parent is DockPanel panel &&
                panel.Parent is StackPanel stackPanel &&
                stackPanel.Children.OfType<ScrollViewer>().Last() is ScrollViewer viewer &&
                viewer.Content is WrapPanel wrapPanel)
                return wrapPanel;
            return null;
        }

        private void SetAllLabelChecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button &&
                GetWrapPanel(button) is WrapPanel wrapPanel &&
                wrapPanel.Children.OfType<ToggleButton>().ToList() is List<ToggleButton> list
               ) {
                bool isChecked = (bool)button.IsChecked;
                list.ForEach(arg => arg.IsChecked = isChecked);
            }
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
                return Visibility.Collapsed;
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
