using ChaoControls.Style;
using DynamicData;
using Jvedio.Core;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Entity.Data;
using Jvedio.Utils.Common;
using Jvedio.Utils.Visual;
using Jvedio.ViewModel;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Jvedio.GlobalMapper;
using static Jvedio.Main;
using static Jvedio.Main.Msg;
using static Jvedio.Utils.Visual.VisualHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_Images.xaml 的交互逻辑
    /// </summary>
    public partial class Window_MetaDatas : BaseWindow
    {


        public VieModel_MetaData vieModel;
        public static Msg msgCard = new Msg();
        public static bool CheckingScanStatus = false;
        public bool CanRateChange = false;

        public SelectWrapper<MetaData> CurrentWrapper;
        public string CurrentSQL;

        public Window_MetaDataEdit editWindow;



        public int firstidx = -1;
        public int secondidx = -1;
        public Window_MetaDatas()
        {
            InitializeComponent();
            vieModel = new VieModel_MetaData();
            this.DataContext = vieModel;
            BindingEvents();
        }

        private void BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            setDataBases();
            BindingEventAfterRender();
            vieModel.Select();
            vieModel.Statistic();
        }
        public void BindingEvents()
        {
            //设置排序类型
            var MenuItems = SortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Click += SortMenu_Click;
                MenuItems[i].IsCheckable = true;
                if (i == GlobalConfig.MetaData.SortIndex) MenuItems[i].IsChecked = true;
            }

            vieModel.PageChangedCompleted += (s, ev) =>
            {
                // todo 需要引入 virtual wrapper，否则内存占用率一直很高，每页 40 个 => 1.3 G 左右
                //GC.Collect();
                if (vieModel.EditMode) SetSelected();

                //vieModel.canRender = true;
            };

            vieModel.RenderSqlChanged += (s, ev) =>
            {
                WrapperEventArg<MetaData> arg = ev as WrapperEventArg<MetaData>;
                CurrentWrapper = arg.Wrapper as SelectWrapper<MetaData>;
                CurrentSQL = arg.SQL;

            };

            // 绑定消息
            msgCard.MsgShown += (s, ev) =>
            {
                MessageEventArg eventArg = ev as MessageEventArg;
                vieModel.Message.Add(eventArg.Message);
            };

        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++)
            {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem)
                {
                    item.IsChecked = true;
                    if (i == GlobalConfig.MetaData.SortIndex)
                    {
                        GlobalConfig.MetaData.SortDescending = !GlobalConfig.MetaData.SortDescending;
                    }
                    GlobalConfig.MetaData.SortIndex = i;

                }
                else item.IsChecked = false;
            }
            vieModel.Reset();
        }

        private void SideBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }


        private void GoToStartUp(object sender, MouseButtonEventArgs e)
        {
            WindowStartUp windowStartUp = new WindowStartUp();
            Application.Current.MainWindow = windowStartUp;
            windowStartUp.Show();
            this.Close();
        }

        public void setDataBases()
        {
            List<AppDatabase> appDatabases =
                appDatabaseMapper.selectList(new SelectWrapper<AppDatabase>().Eq("DataType", (int)GlobalVariable.CurrentDataType));
            ObservableCollection<AppDatabase> temp = new ObservableCollection<AppDatabase>();
            appDatabases.ForEach(db => temp.Add(db));
            vieModel.DataBases = temp;
            if (temp.Count > 0)
                vieModel.CurrentAppDataBase = temp[0];
        }

        public void setComboboxID()
        {
            int idx = vieModel.DataBases.ToList().FindIndex(arg => arg.DBId == GlobalConfig.Main.CurrentDBId);
            if (idx < 0 || idx > DatabaseComboBox.Items.Count) idx = 0;
            DatabaseComboBox.SelectedIndex = idx;
        }



        private void BindingEventAfterRender()
        {
            setComboboxID();
            DatabaseComboBox.SelectionChanged += DatabaseComboBox_SelectionChanged;

            // 搜索框事件
            searchBox.TextChanged += RefreshCandiadte;
            searchTabControl.SelectionChanged += (s, e) =>
            {
                if (GlobalConfig.Main.SearchSelectedIndex == searchTabControl.SelectedIndex) return;
                GlobalConfig.Main.SearchSelectedIndex = searchTabControl.SelectedIndex;
                RefreshCandiadte(null, null);
            };

            pagination.PageSizeChange += Pagination_PageSizeChange;
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            //AppDatabase database = 
            vieModel.CurrentAppDataBase = (AppDatabase)e.AddedItems[0];
            GlobalConfig.Main.CurrentDBId = vieModel.CurrentAppDataBase.DBId;
            //切换数据库

            vieModel.Statistic();
            vieModel.Reset();
            vieModel.initCurrentTagStamps();

            //vieModel.InitLettersNavigation();
            //vieModel.GetFilterInfo();
            AllRadioButton.IsChecked = true;



        }

        private async void RefreshCandiadte(object sender, TextChangedEventArgs e)
        {
            List<string> list = await vieModel.GetSearchCandidate();
            int idx = (int)GlobalConfig.Main.SearchSelectedIndex;
            TabItem tabItem = searchTabControl.Items[idx] as TabItem;
            addOrRefreshItem(tabItem, list);
        }

        private void addOrRefreshItem(TabItem tabItem, List<string> list)
        {
            ListBox listBox;
            if (tabItem.Content == null)
            {
                listBox = new ListBox();
                tabItem.Content = listBox;
            }
            else
            {
                listBox = tabItem.Content as ListBox;
            }
            listBox.Margin = new Thickness(0, 0, 0, 5);
            listBox.Style = (System.Windows.Style)App.Current.Resources["NormalListBox"];
            listBox.ItemContainerStyle = (System.Windows.Style)this.Resources["SearchBoxListItemContainerStyle"];
            listBox.Background = Brushes.Transparent;
            listBox.ItemsSource = list;
            vieModel.Searching = true;
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            vieModel.ScanStatus = "Scanning";
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> files = new List<string>();
            List<string> paths = new List<string>();

            foreach (var item in dragdropFiles)
            {
                if (FileHelper.IsFile(item))
                    files.Add(item);
                else
                    paths.Add(item);
            }
            ScanTask scanTask = ScanFactory.produceScanner(GlobalVariable.CurrentDataType, paths, files);
            scanTask.onCanceled += (s, ev) =>
            {
                //msgCard.Warning("取消扫描任务");
            };
            scanTask.onError += (s, ev) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    msgCard.Error((ev as MessageCallBackEventArgs).Message);
                });

            };
            vieModel.ScanTasks.Add(scanTask);
            scanTask.Start();
            setScanStatus();
        }

        private void setScanStatus()
        {
            if (!CheckingScanStatus)
            {
                CheckingScanStatus = true;
                Task.Run(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("检查状态");
                        if (vieModel.ScanTasks.All(arg =>
                         arg.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                         arg.Status == System.Threading.Tasks.TaskStatus.RanToCompletion
                        ))
                        {
                            vieModel.ScanStatus = "Complete";
                            CheckingScanStatus = false;
                            break;
                        }
                        else
                        {
                            Task.Delay(1000).Wait();
                        }

                    }
                });
            }
        }

        private void ShowMsgScanPopup(object sender, MouseButtonEventArgs e)
        {
            scanStatusPopup.IsOpen = true;

        }

        private void HideScanPopup(object sender, MouseButtonEventArgs e)
        {
            scanStatusPopup.IsOpen = false;
        }

        private void ClearScanTasks(object sender, MouseButtonEventArgs e)
        {

            for (int i = vieModel.ScanTasks.Count - 1; i >= 0; i--)
            {
                Core.Scan.ScanTask scanTask = vieModel.ScanTasks[i];
                if (scanTask.Status == System.Threading.Tasks.TaskStatus.Canceled ||
                    scanTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion
                    )
                {
                    vieModel.ScanTasks.RemoveAt(i);
                }
            }
            vieModel.ScanStatus = "None";
        }

        private void CancelScanTask(object sender, RoutedEventArgs e)
        {
            string createTime = (sender as Button).Tag.ToString();
            ScanTask scanTask = vieModel.ScanTasks.Where(arg => arg.CreateTime.Equals(createTime)).FirstOrDefault();
            scanTask.Cancel();
        }

        private void ShowScanDetail(object sender, RoutedEventArgs e)
        {
            string createTime = (sender as Button).Tag.ToString();
            ScanTask scanTask = vieModel.ScanTasks.Where(arg => arg.CreateTime.Equals(createTime)).FirstOrDefault();
            if (scanTask.Status != System.Threading.Tasks.TaskStatus.Running)
            {
                Window_ScanDetail scanDetail = new Window_ScanDetail(scanTask.ScanResult);
                scanDetail.Show();
            }
        }

        private void ShowMessage(object sender, MouseButtonEventArgs e)
        {
            msgPopup.IsOpen = true;
        }

        private void HideMsgPopup(object sender, MouseButtonEventArgs e)
        {
            msgPopup.IsOpen = false;
        }

        private void ClearMsg(object sender, MouseButtonEventArgs e)
        {
            vieModel.Message.Clear();
        }

        private void ShowContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
            e.Handled = true;
        }

        private void Pagination_CurrentPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentPage = pagination.CurrentPage;
            //if (!vieModel.canRender) return;
            VieModel_Main.pageQueue.Enqueue(pagination.CurrentPage);
            vieModel.LoadData();
        }

        private void Pagination_PageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.PageSize = pagination.PageSize;
            vieModel.LoadData();
        }

        private long getDataID(UIElement o)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element == null) return -1;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (grid != null && grid.Tag != null)
            {
                long.TryParse(grid.Tag.ToString(), out long result);
                return result;
            }
            return -1;
        }

        private void ShowVideo(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            long dataID = getDataID(button);
            if (dataID <= 0) return;

            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.Items.Clear();

            MetaData data = vieModel.DataList.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (data != null)
            {
                for (int i = 0; i < data.AttachedVideos.Count; i++)
                {
                    string filepath = data.AttachedVideos[i];//这样可以，放在  PlayVideoWithPlayer 就超出索引
                    MenuItem menuItem = new MenuItem();

                    menuItem.Header = System.IO.Path.GetFileName(filepath);
                    menuItem.Click += (s, _) =>
                    {
                        PlayVideoWithPlayer(filepath, dataID);
                    };
                    contextMenu.Items.Add(menuItem);
                }
                contextMenu.IsOpen = true;
            }
        }

        public void PlayVideoWithPlayer(string filepath, long dataID = 0, string token = "")
        {

            if (File.Exists(filepath))
            {
                bool success = false;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.VedioPlayerPath) && File.Exists(Properties.Settings.Default.VedioPlayerPath))
                {
                    success = FileHelper.TryOpenFile(Properties.Settings.Default.VedioPlayerPath, filepath, token);
                }
                else
                {
                    //使用默认播放器
                    success = FileHelper.TryOpenFile(filepath, token);
                }

                if (success && dataID > 0)
                {
                    metaDataMapper.updateFieldById("ViewDate", DateHelper.Now(), dataID);
                    vieModel.Statistic();
                }
            }
            else
            {
                msgCard.Error(Jvedio.Language.Resources.Message_OpenFail + "：" + filepath);
            }
        }

        public void runGame(string exePath, long dataID)
        {
            if (File.Exists(exePath))
            {
                bool success = FileHelper.TryOpenFile(exePath);
                if (success && dataID > 0)
                {
                    metaDataMapper.updateFieldById("ViewDate", DateHelper.Now(), dataID);
                    vieModel.Statistic();
                }
            }
            else
            {
                msgCard.Error(Jvedio.Language.Resources.Message_OpenFail + "：" + exePath);
            }
        }

        private void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Properties.Settings.Default.ShowImageMode == "0")
            {
                Properties.Settings.Default.SmallImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.SmallImage_Height = (int)((double)Properties.Settings.Default.SmallImage_Width * (200 / 147));

            }
            else if (Properties.Settings.Default.ShowImageMode == "1")
            {
                Properties.Settings.Default.BigImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.BigImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
            //else if (Properties.Settings.Default.ShowImageMode == "2")
            //{
            //    Properties.Settings.Default.ExtraImage_Width = Properties.Settings.Default.GlobalImageWidth;
            //    Properties.Settings.Default.ExtraImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            //}
            else if (Properties.Settings.Default.ShowImageMode == "2")
            {
                Properties.Settings.Default.GifImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.GifImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalConfig.MetaData.PageSize = vieModel.PageSize;
            GlobalConfig.MetaData.Save();
        }

        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (!CanRateChange) return;
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            StackPanel stackPanel = rate.Parent as StackPanel;
            long id = getDataID(stackPanel);
            metaDataMapper.updateFieldById("Grade", rate.Value.ToString(), id);
            vieModel.Statistic();
            CanRateChange = false;
        }

        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }

        private void DataMouseEnter(object sender, MouseEventArgs e)
        {
            if (vieModel.EditMode)
            {
                GifImage image = sender as GifImage;
                Grid grid = image.FindParentOfType<Grid>("rootGrid");
                Border border = grid.Children[0] as Border;
                border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
            }
        }

        private void DataMouseLeave(object sender, MouseEventArgs e)
        {
            if (vieModel.EditMode)
            {
                GifImage image = sender as GifImage;
                long dataID = getDataID(image);
                Grid grid = image.FindParentOfType<Grid>("rootGrid");
                Border border = grid.Children[0] as Border;
                if (vieModel.SelectedData.Where(arg => arg.DataID == dataID).Any())
                {
                    border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;
                }
                else
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        private bool canShowDetails = false;
        private void CanShowDetails(object sender, MouseButtonEventArgs e)
        {
            canShowDetails = true;
        }

        private void ShowDetails(object sender, MouseButtonEventArgs e)
        {
            if (!canShowDetails) return;
            FrameworkElement element = sender as FrameworkElement;//点击 border 也能选中
            long ID = getDataID(element);
            if (vieModel.EditMode)
            {
                MetaData data = vieModel.CurrentDataList.Where(arg => arg.DataID == ID).FirstOrDefault();
                int selectIdx = vieModel.CurrentDataList.IndexOf(data);

                // 多选
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (firstidx == -1)
                        firstidx = selectIdx;
                    else
                        secondidx = selectIdx;
                }


                Console.WriteLine("firstidx=" + firstidx);
                Console.WriteLine("secondidx=" + secondidx);
                if (firstidx >= 0 && secondidx >= 0)
                {
                    if (firstidx > secondidx)
                    {
                        //交换一下顺序
                        int temp = firstidx;
                        firstidx = secondidx - 1;
                        secondidx = temp - 1;
                    }

                    for (int i = firstidx + 1; i <= secondidx; i++)
                    {
                        MetaData m = vieModel.CurrentDataList[i];
                        if (vieModel.SelectedData.Contains(m))
                            vieModel.SelectedData.Remove(m);
                        else
                            vieModel.SelectedData.Add(m);
                    }
                    firstidx = -1;
                    secondidx = -1;
                }
                else
                {
                    if (vieModel.SelectedData.Contains(data))
                        vieModel.SelectedData.Remove(data);
                    else
                        vieModel.SelectedData.Add(data);
                }


                SetSelected();

            }
            else
            {
                //windowDetails?.Close();
                //windowDetails = new WindowDetails(ID);
                //windowDetails.Show();
                //VieModel_Main.PreviousPage = vieModel.CurrentPage;
                //VieModel_Main.PreviousOffset = MovieScrollViewer.VerticalOffset;
            }
            canShowDetails = false;
        }

        public void SetSelected()
        {
            ItemsControl itemsControl;

            itemsControl = MovieItemsControl;


            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "rootBorder");
                if (border == null) continue;
                Grid grid = border.Parent as Grid;
                long dataID = getDataID(border);
                if (border != null)
                {

                    border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
                    border.BorderBrush = Brushes.Transparent;
                    if (vieModel.EditMode && vieModel.SelectedData.Where(arg => arg.DataID == dataID).Any())
                    {
                        border.Background = GlobalStyle.Common.HighLight.Background;
                        border.BorderBrush = GlobalStyle.Common.HighLight.BorderBrush;

                    }
                }

            }

        }

        private void SetSelectMode(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedData.Clear();
            SetSelected();
        }

        private void ShowRunMenu(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                FrameworkElement el = sender as FrameworkElement;
                long dataid = getDataID(el);
                if (dataid > 0)
                {
                    Game game = vieModel.GameList.Where(item => item.DataID == dataid).First();
                    if (game != null && game.DataID > 0)
                    {
                        runGame(game.Path, game.DataID);
                    }
                }

            }
            e.Handled = true;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            Border border = grid.FindName("runBorder") as Border;
            if (GlobalVariable.CurrentDataType == Core.Enums.DataType.Game && !vieModel.EditMode)
            {
                border.Visibility = Visibility.Visible;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            Border border = grid.FindName("runBorder") as Border;
            border.Visibility = Visibility.Hidden;
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (vieModel.IsLoadingMovie)
            {
                e.Handled = true;
                return;
            }


            // 标记
            GifImage gifImage = e.Source as GifImage;
            if (gifImage == null) return;
            long dataID = getDataID(gifImage);
            ContextMenu contextMenu = gifImage.ContextMenu;
            foreach (MenuItem item in contextMenu.Items)
            {
                if (item.Name == "TagMenuItems")
                {
                    item.Items.Clear();
                    GlobalVariable.TagStamps.ForEach(arg =>
                    {
                        MenuItem menu = new MenuItem()
                        {
                            Header = arg.TagName
                        };
                        menu.Click += (s, ev) =>
                        {
                            string sql = $"insert or replace into metadata_to_tagstamp (DataID,TagID)  values ({dataID},{arg.TagID})";
                            tagStampMapper.executeNonQuery(sql);
                            initTagStamp();
                            for (int i = 0; i < vieModel.CurrentDataList.Count; i++)
                            {
                                if (vieModel.CurrentDataList[i].DataID == dataID)
                                {
                                    MetaData data = vieModel.CurrentDataList[i];
                                    refreshTagStamp(ref data, arg.TagID);
                                    vieModel.CurrentDataList[i] = null;
                                    vieModel.CurrentDataList[i] = data;

                                }
                            }

                        };
                        item.Items.Add(menu);

                    });
                }
            }

        }

        private void initTagStamp()
        {
            GlobalVariable.TagStamps = tagStampMapper.getAllTagStamp();
            vieModel.initCurrentTagStamps();
        }

        private void refreshTagStamp(ref MetaData data, long newTagID)
        {
            string tagIDs = data.TagIDs;
            if (string.IsNullOrEmpty(tagIDs))
            {
                data.TagStamp = new ObservableCollection<TagStamp>();
                data.TagStamp.Add(GlobalVariable.TagStamps.Where(arg => arg.TagID == newTagID).FirstOrDefault());
            }
            else
            {
                List<string> list = tagIDs.Split(',').ToList();
                if (!list.Contains(newTagID.ToString()))
                {
                    list.Add(newTagID.ToString());
                    data.TagIDs = String.Join(",", list);
                    data.TagStamp = new ObservableCollection<TagStamp>();
                    foreach (var arg in list)
                    {
                        long.TryParse(arg, out long id);
                        data.TagStamp.Add(GlobalVariable.TagStamps.Where(item => item.TagID == id).FirstOrDefault());
                    }
                }
            }
        }

        private void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteInfo);
                if (menuItem != null) DeleteID(menuItem, new RoutedEventArgs());
            }
            else if (e.Key == Key.T)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteFile);
                if (menuItem != null) DeleteFile(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.E)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_EditInfo);
                if (menuItem != null) EditInfo(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.W)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_OpenWebSite);
                if (menuItem != null) OpenWeb(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.C)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_CopyFile);
                if (menuItem != null) CopyFile(menuItem, new RoutedEventArgs());

            }
            contextMenu.IsOpen = false;
        }

        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            foreach (MenuItem item in contextMenu.Items)
            {
                if (item.Header.ToString() == header)
                {
                    return item;
                }
            }
            return null;
        }

        private void EditInfo(object sender, RoutedEventArgs e)
        {
            editWindow?.Close();
            editWindow = new Window_MetaDataEdit(GetIDFromMenuItem(sender), GlobalVariable.CurrentDataType);
            editWindow.ShowDialog();
        }

        private void OpenWeb(object sender, RoutedEventArgs e)
        {

        }

        public void CopyFile(object sender, RoutedEventArgs e)
        {
            handleMenuSelected((sender));
            StringCollection paths = new StringCollection();
            DataType dataType = GlobalVariable.CurrentDataType;
            foreach (var item in vieModel.SelectedData)
            {
                string path = item.Path;
                if (dataType == DataType.Game)
                {
                    path = System.IO.Path.GetDirectoryName(item.Path);
                }
                if (Directory.Exists(path))
                    paths.Add(path);
            }

            if (paths.Count <= 0)
            {
                msgCard.Warning($"需要复制文件的个数为 0，文件可能不存在");
                return;
            }
            bool success = ClipBoard.TrySetFileDropList(paths, (error) => { msgCard.Error(error); });

            if (success)
                msgCard.Success($"{Jvedio.Language.Resources.Message_Copied} {paths.Count}/{vieModel.SelectedData.Count}");


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedData.Clear();
        }

        private void DeleteID(object sender, RoutedEventArgs e)
        {
            handleMenuSelected(sender);
            if (vieModel.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToDelete).ShowDialog() == false) { return; }
            deleteIDs(vieModel.SelectedData, false);
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            handleMenuSelected((sender));
            if (vieModel.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToDelete).ShowDialog() == false) { return; }
            int num = 0;
            int totalCount = vieModel.SelectedData.Count;


            DataType dataType = GlobalVariable.CurrentDataType;

            foreach (var item in vieModel.SelectedData)
            {
                string path = item.Path;
                if (dataType == DataType.Game)
                {
                    path = System.IO.Path.GetDirectoryName(item.Path);
                }
                try
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    num++;
                }
                catch (Exception ex)
                {
                    msgCard.Error(ex.Message);
                    Logger.LogF(ex);
                }

            }

            msgCard.Info($"已删除到回收站 {num}/{totalCount}");

            if (num > 0 && Properties.Settings.Default.DelInfoAfterDelFile)
                deleteIDs(vieModel.SelectedData, false);

            if (!vieModel.EditMode) vieModel.SelectedData.Clear();
        }

        private void handleMenuSelected(object sender, int depth = 0)
        {
            long dataID = GetIDFromMenuItem(sender, depth);
            if (!vieModel.EditMode) vieModel.SelectedData.Clear();
            MetaData data = vieModel.CurrentDataList.Where(arg => arg.DataID == dataID).FirstOrDefault();
            if (!vieModel.SelectedData.Where(arg => arg.DataID == dataID).Any()) vieModel.SelectedData.Add(data);
        }

        private long GetIDFromMenuItem(object sender, int depth = 0)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu contextMenu = null;
            if (depth == 0)
            {
                contextMenu = mnu.Parent as ContextMenu;
            }
            else
            {
                MenuItem _mnu = mnu.Parent as MenuItem;
                contextMenu = _mnu.Parent as ContextMenu;
            }
            GifImage gifImage = contextMenu.PlacementTarget as GifImage;
            return getDataID(gifImage);
        }

        public async void deleteIDs(List<MetaData> to_delete, bool fromDetailWindow = true)
        {
            if (!fromDetailWindow)
            {
                vieModel.CurrentDataList.RemoveMany(to_delete);
                vieModel.DataList.RemoveMany(to_delete);
            }
            else
            {
                // 影片只有单个
                MetaData data = to_delete[0];
                int idx = -1;
                for (int i = 0; i < vieModel.CurrentDataList.Count; i++)
                {
                    if (vieModel.CurrentDataList[i].DataID == data.DataID)
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx >= 0)
                {
                    vieModel.CurrentDataList.RemoveAt(idx);
                    vieModel.DataList.RemoveAt(idx);
                }

            }


            // todo FilterMovieList
            //vieModel.FilterMovieList.Remove(arg);
            int count = metaDataMapper.deleteDataByIds(to_delete.Select(arg => arg.DataID.ToString()).ToList());

            // todo 关闭详情窗口
            //if (!fromDetailWindow && FileProcess.GetWindowByName("WindowDetails") is Window window)
            //{
            //    WindowDetails windowDetails = (WindowDetails)window;
            //    foreach (var item in to_delete)
            //    {
            //        if (windowDetails.DataID == item.DataID)
            //        {
            //            windowDetails.Close();
            //            break;
            //        }
            //    }
            //}

            msgCard.Info($"{Jvedio.Language.Resources.SuccessDelete} {count}/{to_delete.Count} ");
            //修复数字显示
            vieModel.CurrentCount -= to_delete.Count;
            vieModel.TotalCount -= to_delete.Count;

            to_delete.Clear();
            vieModel.Statistic();

            await Task.Delay(1000);
            vieModel.EditMode = false;
            vieModel.SelectedData.Clear();
            SetSelected();
        }

        public void RefreshData(long dataID)
        {
            for (int i = 0; i < vieModel.CurrentDataList.Count; i++)
            {
                if (vieModel.CurrentDataList[i]?.DataID == dataID)
                {
                    MetaData data = null;
                    if (GlobalVariable.CurrentDataType == DataType.Picture)
                    {
                        vieModel.PictureList[i] = pictureMapper.SelectByID(dataID);
                        data = vieModel.PictureList[i].toMetaData();

                    }
                    else if (GlobalVariable.CurrentDataType == DataType.Game)
                    {
                        vieModel.GameList[i] = gameMapper.SelectByID(dataID);
                        data = vieModel.GameList[i].toMetaData();
                    }
                    else if (GlobalVariable.CurrentDataType == DataType.Comics)
                    {
                        vieModel.ComicList[i] = comicMapper.SelectByID(dataID);
                        data = vieModel.ComicList[i].toMetaData();
                    }
                    if (data != null)
                    {
                        vieModel.setData(ref data);
                        //vieModel.CurrentDataList[i].ViewImage = null;
                        //vieModel.CurrentDataList[i].ViewImage = data.ViewImage; 
                        vieModel.CurrentDataList[i] = null;
                        vieModel.CurrentDataList[i] = data;
                    }

                    break;
                }
            }
        }

    }
}
