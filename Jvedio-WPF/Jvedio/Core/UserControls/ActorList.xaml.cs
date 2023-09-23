using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.WPF.VisualTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.VisualHelper;

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// ActorsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ActorList : UserControl, ITabItemControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "事件"

        public Action<long, float> onGradeChange;
        public event EventHandler PageChangedCompleted;
        public Action<long> onShowSameActor;
        public static Action onStatistic;


        private delegate void LoadActorDelegate(ActorInfo actor, int idx);



        #endregion

        #region "静态属性"



        public static Dictionary<int, string> SortDict { get; set; } = new Dictionary<int, string>();


        public static List<string> SortDictList { get; set; } = new List<string>()
        {
            "actor_info.Grade",
            "actor_info.ActorName",
            "Count",
            "actor_info.Country",
            "Nation",
            "BirthPlace",
            "Birthday",
            "BloodType",
            "Height",
            "Weight",
            "Gender",
            "Hobby",
            "Cup",
            "Chest",
            "Waist",
            "Hipline",
            "actor_info.Grade",
            "Age",
        };


        public static string[] SelectedField { get; set; } = new string[]
        {
            "count(ActorName) as Count",
            "actor_info.ActorID",
            "actor_info.ActorName",
            "actor_info.Country",
            "Nation",
            "BirthPlace",
            "Birthday",
            "BloodType",
            "Height",
            "Weight",
            "Gender",
            "Hobby",
            "Cup",
            "Chest",
            "Age",
            "Waist",
            "Hipline",
            "WebType",
            "WebUrl",
            "actor_info.Grade",
            "actor_info.ExtraInfo",
            "actor_info.CreateDate",
            "actor_info.UpdateDate",
        };
        
        public static string[] SelectedFieldUnion { get; set; }  = SelectedField.ToArray();


        public static Dictionary<string, string> Actor_SELECT_TYPE { get; set; } = new Dictionary<string, string>()
        {
            { "All", "  " },
            { "Favorite", "  " },
        };



        #endregion

        #region "属性"

        private Queue<int> PageQueue { get; set; } = new Queue<int>();
        private int firstIdx { get; set; } = -1;
        private int secondIdx { get; set; } = -1;
        private int actorFirstIdx { get; set; } = -1;
        private int actorSecondIdx { get; set; } = -1;

        public CancellationTokenSource RenderCTS { get; set; }

        public CancellationToken RenderCT { get; set; }

        private bool _Nothing = true;

        public bool Nothing {
            get { return _Nothing; }

            set {
                _Nothing = value;
                RaisePropertyChanged();
            }
        }
        private bool _ShowTable = false;

        public bool ShowTable {
            get { return _ShowTable; }

            set {
                _ShowTable = value;
                RaisePropertyChanged();
            }
        }
        private string _SearchText = string.Empty;

        public string SearchText {
            get { return _SearchText; }

            set {
                _SearchText = value;
                RaisePropertyChanged();

                // BeginSearch();
            }
        }

        private int _PageSize = 40;

        public int PageSize {
            get { return _PageSize; }

            set {
                _PageSize = value;
                RaisePropertyChanged();
            }
        }

        private long _TotalCount = 0;

        public long TotalCount {
            get { return _TotalCount; }

            set {
                _TotalCount = value;
                RaisePropertyChanged();
            }
        }


        private int _CurrentPage = 1;

        public int CurrentPage {
            get { return _CurrentPage; }

            set {
                _CurrentPage = value;

                RaisePropertyChanged();
            }
        }

        private List<ActorInfo> _Actors;

        public List<ActorInfo> Actors {
            get { return _Actors; }

            set {
                _Actors = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ActorInfo> _CurrentList;

        public ObservableCollection<ActorInfo> CurrentList {
            get { return _CurrentList; }

            set {
                _CurrentList = value;
                RaisePropertyChanged();
            }
        }


        public int _CurrentCount = 0;

        public int CurrentCount {
            get { return _CurrentCount; }

            set {
                _CurrentCount = value;
                RaisePropertyChanged();
            }
        }

        private List<ActorInfo> _SelectedActors = new List<ActorInfo>();

        public List<ActorInfo> SelectedActors {
            get { return _SelectedActors; }

            set {
                _SelectedActors = value;
                RaisePropertyChanged();
            }
        }


        private int _RenderProgress;

        public int RenderProgress {
            get { return _RenderProgress; }

            set {
                _RenderProgress = value;
                RaisePropertyChanged();
            }
        }


        private bool _rendering;
        public bool Rendering {
            get { return _rendering; }
            set {
                _rendering = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "控件属性"

        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
nameof(ViewMode), typeof(bool), typeof(ActorList), new PropertyMetadata(false));

        public bool ViewMode {
            get { return (bool)GetValue(ViewModeProperty); }
            set {
                SetValue(ViewModeProperty, value);
            }
        }

        #endregion


        public ActorList()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            this.DataContext = this;
            ConfigManager.VideoConfig.ActorEditMode = false;

            SetDataGrid();
        }

        static ActorList()
        {
            for (int i = 0; i < SortDictList.Count; i++) {
                SortDict.Add(i, SortDictList[i]);
            }
            SelectedFieldUnion[0] = "CASE WHEN count(ActorName) = 1 THEN 0 ELSE 1 END as Count";
        }


        private void ActorList_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (sender is ActorList actorList && actorList.IsLoaded) {
                RefreshActorRenderToken();
                BindingEvent();
                Select();
            }
        }

        public void Refresh(int page = -1)
        {
            if (page > 0 && CurrentPage != page) {
                CurrentPage = page;
                pagination.CurrentPage = page;
            } else {
                Select();
            }
        }

        public void RefreshGrade(long actorID, float grade)
        {
            if (actorID <= 0)
                return;
            for (int i = 0; i < CurrentList.Count; i++) {
                if (CurrentList[i].ActorID == actorID) {
                    CurrentList[i].Grade = grade;
                    break;
                }
            }
        }

        public void BindingEvent()
        {
            // 设置演员排序类型
            int actorSortType = (int)ConfigManager.VideoConfig.ActorSortType;
            var SortMenuItems = SortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < SortMenuItems.Count; i++) {
                SortMenuItems[i].Click += SortMenu_Click;
                SortMenuItems[i].IsCheckable = true;
                if (i == actorSortType)
                    SortMenuItems[i].IsChecked = true;
            }


            // 设置演员显示模式
            var arbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            for (int i = 0; i < arbs.Count; i++) {
                arbs[i].Click += SetViewMode;
                if (i == ConfigManager.VideoConfig.ActorViewMode)
                    arbs[i].IsChecked = true;
            }


            this.PageChangedCompleted += (s, ev) => {
                //if (ConfigManager.VideoConfig.ActorEditMode)
                //    SetSelected();
                scrollViewer.ScrollToTop();
            };
        }

        public void SetViewMode(object sender, RoutedEventArgs e)
        {
            PathRadioButton radioButton = sender as PathRadioButton;
            if (radioButton == null)
                return;
            var rbs = ViewModeStackPanel.Children.OfType<PathRadioButton>().ToList();
            int idx = rbs.IndexOf(radioButton);
            ConfigManager.VideoConfig.ActorViewMode = idx;
            ConfigManager.VideoConfig.ActorEditMode = false;
            ConfigManager.VideoConfig.Save();
            SetDataGrid();
        }

        public void SetDataGrid()
        {
            if (ConfigManager.VideoConfig.ActorViewMode == 2) {
                ShowTable = true;
            } else {
                ShowTable = false;
            }
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++) {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem) {
                    item.IsChecked = true;
                    if (i == ConfigManager.VideoConfig.ActorSortType) {
                        ConfigManager.VideoConfig.ActorSortDescending = !ConfigManager.VideoConfig.ActorSortDescending;
                    }

                    ConfigManager.VideoConfig.ActorSortType = i;
                } else
                    item.IsChecked = false;
            }

            Select();
        }


        public void RefreshActorRenderToken()
        {
            RenderCTS = new CancellationTokenSource();
            RenderCTS.Token.Register(() => { Logger.Warn("cancel load actor page task"); });
            RenderCT = RenderCTS.Token;
        }

        public async void Select()
        {
            if (ConfigManager.Main == null)
                return;

            // 判断当前获取的队列
            while (PageQueue.Count > 1) {
                int page = PageQueue.Dequeue();
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (Rendering) {
                RenderCTS?.Cancel(); // 取消加载
                await Task.Delay(100);
            }

            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            SetSortOrder(wrapper);

            bool search = !string.IsNullOrEmpty(SearchText);
            string searchText = SearchText.ToProperSql();

            string count_sql = "SELECT count(*) as Count " +
                         "from (SELECT actor_info.ActorID FROM actor_info join metadata_to_actor " +
                         "on metadata_to_actor.ActorID=actor_info.ActorID " +
                         "join metadata " +
                         "on metadata_to_actor.DataID=metadata.DataID " +
                         $"WHERE metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                         $"{(search ? $"and actor_info.ActorName like '%{searchText}%' " : string.Empty)} " +
                         "GROUP BY actor_info.ActorID " +
                         "UNION " +
                         "select actor_info.ActorID  " +
                         "FROM actor_info WHERE NOT EXISTS " +
                         "(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID ) " +
                         $"{(search ? $"and actor_info.ActorName like '%{searchText}%' " : string.Empty)} " +
                         "GROUP BY actor_info.ActorID)";


            TotalCount = actorMapper.SelectCount(count_sql);

            string filedSelect = new SelectWrapper<ActorInfo>().Select(SelectedField).ToSelect(false);
            string unionSelect = new SelectWrapper<ActorInfo>().Select(SelectedFieldUnion).ToSelect(false);

            string sql = $"{filedSelect} FROM actor_info " +
                $"join metadata_to_actor on metadata_to_actor.ActorID=actor_info.ActorID " +
                $"join metadata on metadata_to_actor.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                $"{(search ? $"and actor_info.ActorName like '%{searchText}%' " : string.Empty)} " +
                $"GROUP BY actor_info.ActorID " +
                "UNION " +
                // 显示只有演员名，没有数量的演员
                $"{unionSelect} FROM actor_info " +
                "WHERE NOT EXISTS(SELECT 1 from metadata_to_actor where metadata_to_actor.ActorID=actor_info.ActorID )" +
                $"{(search ? $"and actor_info.ActorName like '%{searchText}%' " : string.Empty)}  GROUP BY actor_info.ActorID " +
                wrapper.ToOrder() + ActorToLimit();

            onPageChange(TotalCount);
            Render(sql);
        }

        public void onPageChange(long total)
        {
            App.Current.Dispatcher.Invoke(() => pagination.Total = TotalCount);
        }



        public void SetSortOrder<T>(IWrapper<T> wrapper)
        {
            if (wrapper == null ||
                SortDict == null ||
                ConfigManager.VideoConfig == null ||
                ConfigManager.VideoConfig.ActorSortType >= SortDict.Count)
                return;
            string sortField = SortDict[(int)ConfigManager.VideoConfig.ActorSortType];
            if (ConfigManager.VideoConfig.ActorSortDescending)
                wrapper.Desc(sortField);
            else
                wrapper.Asc(sortField);
        }

        public string ActorToLimit()
        {
            int row_count = PageSize;
            long offset = PageSize * (CurrentPage - 1);
            return $" LIMIT {offset},{row_count}";
        }


        public void Render(string sql)
        {
            List<Dictionary<string, object>> list = actorMapper.Select(sql);
            List<ActorInfo> actors = actorMapper.ToEntity<ActorInfo>(list, typeof(ActorInfo).GetProperties(), false);
            Actors = new List<ActorInfo>();
            if (actors == null)
                actors = new List<ActorInfo>();
            Actors.AddRange(actors);


            CurrentCount = Actors.Count;

            RenderActor();
        }

        public async void RenderActor()
        {
            if (CurrentList == null) {
                CurrentList = new ObservableCollection<ActorInfo>();
                Nothing = true;
                CurrentList.CollectionChanged += (s, e) => {
                    Nothing = CurrentList.Count == 0;
                };
            }

            for (int i = 0; i < Actors.Count; i++) {
                try {
                    RenderCT.ThrowIfCancellationRequested();
                } catch (OperationCanceledException) {
                    RenderCTS?.Dispose();
                    break;
                }

                Rendering = true;
                ActorInfo actorInfo = Actors[i];
                ActorInfo.SetImage(ref actorInfo);
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo, i);
                RenderProgress = (int)(100 * (i + 1) / (float)Actors.Count);
            }

            // 清除
            for (int i = CurrentList.Count - 1; i > Actors.Count - 1; i--) {
                CurrentList.RemoveAt(i);
            }

            if (RenderCT.IsCancellationRequested)
                RefreshActorRenderToken();
            Rendering = false;

            // if (pageQueue.Count > 0) pageQueue.Dequeue();
            PageChangedCompleted?.Invoke(this, null);
        }

        private void LoadActor(ActorInfo actor, int idx)
        {
            if (RenderCT.IsCancellationRequested)
                return;
            if (CurrentList.Count < PageSize) {
                if (idx < CurrentList.Count) {
                    LoadActor(idx, actor);
                } else {
                    CurrentList.Add(actor);
                }
            } else {
                LoadActor(idx, actor);
            }
        }

        private void LoadActor(int idx, ActorInfo actorInfo)
        {
            if (CurrentList[idx].ActorID == actorInfo.ActorID) {
                // 不知为啥，如果 2 个对象相等，则不会触发 notify
                ActorInfo temp = CurrentList[idx];
                RefreshData(ref temp, actorInfo);
            } else {
                CurrentList[idx] = actorInfo;
            }
        }

        private void RefreshData(ref ActorInfo origin, ActorInfo target)
        {
            System.Reflection.PropertyInfo[] propertyInfos = target.GetType().GetProperties();
            foreach (var item in propertyInfos) {
                object v = item.GetValue(target);
                if (v != null) {
                    item.SetValue(origin, v);
                }
            }
        }

        private void Page_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right) {
                // 末页
                //SetSelected();
            } else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left) {
                CurrentPage = 1;
                //SetSelected();

            }
        }

        private void SetActorSelectMode(object sender, RoutedEventArgs e)
        {
            SelectedActors.Clear();
            //SetSelected();
        }

        private void CurrentActorPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;

            if (!pagination.IsLoaded)
                return;


            CurrentPage = pagination.CurrentPage;
            PageQueue.Enqueue(pagination.CurrentPage);
            Select();
        }

        private void ActorPageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            if (!pagination.IsLoaded)
                return;
            PageSize = pagination.PageSize;
            Select();
        }

        public void RefreshActor(long actorID)
        {
            if (CurrentList?.Count <= 0)
                return;
            for (int i = 0; i < CurrentList.Count; i++) {
                if (CurrentList[i]?.ActorID == actorID) {
                    long count = CurrentList[i].Count;
                    CurrentList[i] = new ActorInfo();
                    CurrentList[i] = ActorInfo.GetById(actorID);
                    CurrentList[i].Count = count;
                    break;
                }
            }
        }

        private void DeleteActors(object sender, RoutedEventArgs e)
        {
            if (new MsgBox("即将删除演员信息，是否继续？").ShowDialog() == true) {
                MenuItem mnu = sender as MenuItem;
                ContextMenu contextMenu = mnu.Parent as ContextMenu;

                // FrameworkElement image = contextMenu.PlacementTarget as FrameworkElement;
                long.TryParse(contextMenu.Tag.ToString(), out long actorID);
                if (actorID <= 0)
                    return;

                if (!ConfigManager.VideoConfig.ActorEditMode)
                    SelectedActors.Clear();
                ActorInfo actor = CurrentList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
                if (!SelectedActors.Where(arg => arg.ActorID == actorID).Any())
                    SelectedActors.Add(actor);

                foreach (ActorInfo actorInfo in SelectedActors) {
                    actorMapper.DeleteById(actorInfo.ActorID);
                    string sql = $"delete from metadata_to_actor where metadata_to_actor.ActorID='{actorInfo.ActorID}'";
                    actorMapper.ExecuteNonQuery(sql);
                }

                Select();
            }
        }

        // todo 演员信息下载
        public void DownLoadSelectedActor(object sender, RoutedEventArgs e)
        {
            // if (downLoadActress?.State == DownLoadState.DownLoading)
            // {
            //    msgCard.Info(SuperControls.Style.LangManager.GetValueByKey("Message_WaitForDownload")); return;
            // }

            // if (!ConfigManager.VideoConfig.ActorEditMode) SelectedActress.Clear();
            // StackPanel sp = null;
            // if (sender is MenuItem mnu)
            // {
            //    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            //    string name = sp.Tag.ToString();
            //    Actress CurrentActress = GetActressFromVieModel(name);
            //    if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
            //    StartDownLoadActor(SelectedActress);

            // }
            // if (!ConfigManager.VideoConfig.ActorEditMode) SelectedActress.Clear();
        }

        private void OpenActorImagePath(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = mnu.Parent as ContextMenu;
            long.TryParse(contextMenu.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;
            ActorInfo actorInfo = actorMapper.SelectById(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            string path = Path.GetFullPath(actorInfo.GetImagePath());
            FileHelper.TryOpenSelectPath(path);
        }


        private void SideActorRate_ValueChanged(object sender, EventArgs e)
        {
            Rate rate = sender as Rate;
            if (rate.Tag == null)
                return;
            long.TryParse(rate.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;
            actorMapper.UpdateFieldById("Grade", rate.Value.ToString(), actorID);
        }
        private void EditActor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;

            Window_EditActor window_EditActor = new Window_EditActor(actorID);
            window_EditActor.ShowDialog();
        }

        private void ShowSameActor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long.TryParse(button.Tag.ToString(), out long actorID);
            if (actorID <= 0)
                return;

            onShowSameActor?.Invoke(actorID);
        }

        private void NewActor(object sender, RoutedEventArgs e)
        {
            bool? success = new Window_EditActor(0, true).ShowDialog();
            if ((bool)success) {
                MessageNotify.Success(LangManager.GetValueByKey("AddSuccess"));
                onStatistic?.Invoke();
                Refresh();
            }
        }


        public void SetSearchFocus()
        {
            searchBox.SetFocus();
        }

        private void searchBox_Search(object sender, RoutedEventArgs e)
        {
            if (sender is SearchBox searchBox && searchBox.IsLoaded)
                Select();
        }

        private void Border_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewMode && sender is FrameworkElement ele && ele.Tag != null &&
                long.TryParse(ele.Tag.ToString(), out long actorID))
                onShowSameActor?.Invoke(actorID);
        }

        public void NextPage()
        {
            pagination.NextPage();
        }

        public void PreviousPage()
        {
            pagination.PrevPage();
        }

        public void GoToTop()
        {

        }

        public void GoToBottom()
        {

        }

        public void FirstPage()
        {
            pagination.FirstPage();
        }

        public void LastPage()
        {
            pagination.LastPage();
        }

        private void SideActorRate_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            if (sender is Rate rate && rate.Tag != null &&
                long.TryParse(rate.Tag.ToString(), out long actorID) &&
                actorID > 0) {
                actorMapper.UpdateFieldById("Grade", rate.Value.ToString(), actorID);
                onGradeChange?.Invoke(actorID, (float)rate.Value);
            }

        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (CurrentList != null && tableData.SelectedIndex is int idx &&
                idx >= 0 && idx < CurrentList.Count &&
                CurrentList[idx] is ActorInfo info && info.ActorID is long id) {
                onShowSameActor?.Invoke(id);
            }
        }
    }
}
