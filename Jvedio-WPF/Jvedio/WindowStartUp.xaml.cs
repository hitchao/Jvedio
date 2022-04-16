using ChaoControls.Style;
using Jvedio.Core;
using Jvedio.Entity;
using Jvedio.Utils;
using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using Jvedio.Utils.FileProcess;
using Jvedio.Windows;
using Jvedio.Core.Enums;
using System.Windows.Media.Animation;

namespace Jvedio
{

    public partial class WindowStartUp : ChaoControls.Style.BaseWindow
    {

        private CancellationTokenSource cts;
        private CancellationToken ct;
        private VieModel_StartUp vieModel_StartUp;
        private Image optionImage;


        public WindowStartUp()
        {

            InitializeComponent();
            //testMapper();

            this.Width = SystemParameters.PrimaryScreenWidth * 2 / 3;
            this.Height = SystemParameters.PrimaryScreenHeight * 2 / 3;


            cts = new CancellationTokenSource();
            cts.Token.Register(() => Console.WriteLine("取消任务"));
            ct = cts.Token;

            FileHelper.TryDeleteFile("upgrade.bat");
            FileHelper.TryDeleteDir("Temp");
        }





        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnsureSettings();//修复设置错误
            EnsureFileExists(); //判断文件是否存在
            EnsureDirExists();//创建文件夹

            GlobalVariable.InitVariable();// 初始化全局变量
            GlobalMapper.Init();// 初始化数据库连接


            await MoveOldFiles();//迁移旧文件并迁移到新数据库
            ThemeLoader.loadAllThemes(); //加载主题

            if (GlobalFont != null) this.FontFamily = GlobalFont;
            SetSkin(Properties.Settings.Default.Themes);//设置皮肤
            InitAppData();// 初始化应用数据
            InitClean();//清理文件

            vieModel_StartUp = new VieModel_StartUp();
            this.DataContext = vieModel_StartUp;

            List<RadioButton> radioButtons = SidePanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < radioButtons.Count; i++)
            {
                if (i == vieModel_StartUp.CurrentSideIdx) radioButtons[i].IsChecked = true;
                else radioButtons[i].IsChecked = false;
            }
            GlobalVariable.CurrentDataType = (DataType)GlobalConfig.StartUp.SideIdx;

            if (!Properties.Settings.Default.OpenDataBaseDefault)
            {
                vieModel_StartUp.Loading = false;
                this.TitleHeight = 30;
            }

            if (Properties.Settings.Default.OpenDataBaseDefault)
                LoadDataBase();


        }



        private async Task<bool> MoveOldFiles()
        {
            // 迁移公共数据
            Jvedio4ToJvedio5.MoveAI();
            string[] files = FileHelper.TryScanDIr(oldDataPath, "*.sqlite", SearchOption.TopDirectoryOnly);
            Jvedio4ToJvedio5.MoveScanPathConfig(files);
            Jvedio4ToJvedio5.MoveServersConfig();
            bool success = await Jvedio4ToJvedio5.MoveDatabases(files);
            if (success && files != null && files.Length > 0)
            {
                //videoMapper.executeNonQuery("update metadata_video set DataID=MVID;");
                Jvedio4ToJvedio5.MoveRecentWatch();
                Jvedio4ToJvedio5.MoveMagnets();
                Jvedio4ToJvedio5.MoveTranslate();
                Jvedio4ToJvedio5.MoveMyList();// 清单和 Label 合并，统一为 Label
                Jvedio4ToJvedio5.MoveSearchHistory();
            }

            // 移动文件

            DirHelper.TryMoveDir(oldDataPath, Path.Combine(AllOldDataPath, "DataBase"));// 移动 DataBase
            string[] moveFiles = { "SearchHistory", "ServersConfig", "RecentWatch",
                "AI.sqlite", "Magnets.sqlite", "Translate.sqlite", "mylist.sqlite" };

            foreach (string filename in moveFiles)
            {
                string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                if (File.Exists(origin)) FileHelper.TryMoveFile(origin, Path.Combine(AllOldDataPath, filename));
            }

            return true;
        }






        private void InitAppData()
        {
            try
            {
                Identify.InitFanhaoList();
                ScanHelper.InitSearchPattern();

            }
            catch (Exception ex)
            {
                Logger.LogE(ex);
            }
        }

        private void InitClean()
        {

            try
            {
                ClearDateBefore(-10);
                // todo 清除日志
                ClearLogBefore(-10, "log");
                ClearLogBefore(-10, "log\\NetWork");
                ClearLogBefore(-10, "log\\scanlog");
                ClearLogBefore(-10, "log\\file");
            }
            catch (Exception ex)
            {
                Logger.LogE(ex);
            }
        }




        public void ClearLogBefore(int day, string filepath)
        {
            DateTime dateTime = DateTime.Now.AddDays(day);
            filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filepath);
            if (!Directory.Exists(filepath)) return;
            try
            {
                string[] files = Directory.GetFiles(filepath, "*.log", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    DateTime.TryParse(file.Split('\\').Last().Replace(".log", ""), out DateTime date);
                    if (date < dateTime) FileHelper.TryDeleteFile(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public async void OpenDefaultDatabase()
        {
            //try
            //{
            //    if (Properties.Settings.Default.ScanGivenPath)
            //    {
            //        // todo 开启是扫描文件夹
            //        await Task.Run(() =>
            //        {
            //            this.Dispatcher.BeginInvoke(new Action(() =>
            //            {
            //                //statusText.Text = Jvedio.Language.Resources.Status_ScanDir; 
            //            }), System.Windows.Threading.DispatcherPriority.Render);
            //            List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath)), ct);
            //            Scan.InsertWithNfo(filepaths, ct);
            //        }, cts.Token);

            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogE(ex);
            //}
        }



        private double MIN_SIDE_GRID_WIDTH = 40;

        public void EnsureSettings()
        {
            try
            {
                if (Properties.Settings.Default.UpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeRequired = false;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.LogE(ex);
            }

            if (Properties.Settings.Default.SideGridWidth <= 0) Properties.Settings.Default.SideGridWidth = MIN_SIDE_GRID_WIDTH;



            try
            {
                //TODO
                //if (!Enum.IsDefined(typeof(Skin), Properties.Settings.Default.Themes))
                //{
                //    Properties.Settings.Default.Themes = Skin.黑色.ToString();
                //    Properties.Settings.Default.Save();
                //}

                //if (!Enum.IsDefined(typeof(MyLanguage), Properties.Settings.Default.Language))
                //{
                //    Properties.Settings.Default.Language = MyLanguage.中文.ToString();
                //    Properties.Settings.Default.Save();
                //}
            }
            catch (Exception ex)
            {
                Logger.LogE(ex);
            }

        }

        public void EnsureDirExists()
        {
            foreach (var item in InitDirs)
            {
                FileHelper.TryCreateDir(item);
            }
            foreach (var item in PicPaths)
            {
                FileHelper.TryCreateDir(Path.Combine(BasePicPath, item));
            }

        }


        public void EnsureFileExists()
        {
            if (!File.Exists(@"x64\SQLite.Interop.dll") || !File.Exists(@"x86\SQLite.Interop.dll"))
            {
                MessageBox.Show($"{Jvedio.Language.Resources.Missing} SQLite.Interop.dll", "Jvedio");
                this.Close();
            }
        }

        private void BackUp(string filePath)
        {
            FileHelper.TryCreateDir(GlobalVariable.BackupPath);
            if (File.Exists(filePath))
            {
                string target = Path.Combine(GlobalVariable.BackupPath, Path.GetFileName(filePath));
                if (!File.Exists(target))
                {
                    FileHelper.TryCopyFile(filePath, target);
                }
                else if (new FileInfo(target).Length < new FileInfo(filePath).Length)
                {
                    FileHelper.TryCopyFile(filePath, target, true);
                }
            }
        }

        private void DelSqlite(object sender, RoutedEventArgs e)
        {
            AppDatabase database = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            Msgbox msgbox = new Msgbox(this, $"确认删除 {database.Name}？");
            if (msgbox.ShowDialog() == true)
            {
                appDatabaseMapper.deleteById(database.DBId);
                RefreshDatabase();
            }
        }


        private void RenameSqlite(object sender, RoutedEventArgs e)
        {

            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string originName = info.Name;
            DialogInput input = new DialogInput(this, Jvedio.Language.Resources.Rename, originName);
            if (input.ShowDialog() == false) return;
            string targetName = input.Text;
            if (targetName == originName) return;
            info.Name = targetName;
            appDatabaseMapper.updateById(info);
            // 仅更新重命名的
            vieModel_StartUp.refreshItem(info);
        }


        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(e.AddedItems[0].ToString());
        }


        private void ShowSettingsPopup(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Top;
                contextMenu.IsOpen = true;
            }
            e.Handled = true;
        }




        private void ShowSortPopup(object sender, MouseButtonEventArgs e)
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

        private void SortDatabases(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string header = menuItem.Header.ToString();
                vieModel_StartUp.SortType = header;
                vieModel_StartUp.Sort = !vieModel_StartUp.Sort;
                vieModel_StartUp.Search();
            }
        }



        private void SearchText_Changed(object sender, RoutedEventArgs e)
        {
            ChaoControls.Style.SearchBox textBox = sender as ChaoControls.Style.SearchBox;
            vieModel_StartUp.CurrentSearch = textBox.Text;
            vieModel_StartUp.Search();
        }


        private void NewDatabase(object sender, RoutedEventArgs e)
        {
            vieModel_StartUp.CurrentSearch = "";
            vieModel_StartUp.Sort = true;
            vieModel_StartUp.SortType = "创建时间";
            DialogInput input = new DialogInput(this, Jvedio.Language.Resources.NewLibrary);
            if (input.ShowDialog() == false) return;
            string targetName = input.Text;
            if (string.IsNullOrEmpty(targetName)) return;
            // 创建新数据库
            AppDatabase appDatabase = new AppDatabase();
            appDatabase.Name = targetName;
            appDatabase.DataType = (DataType)vieModel_StartUp.CurrentSideIdx;
            appDatabaseMapper.insert(appDatabase);
            RefreshDatabase();
        }

        private void RefreshDatabase(object sender, MouseButtonEventArgs e)
        {
            RefreshDatabase();
        }


        public void RefreshDatabase()
        {
            vieModel_StartUp.ReadFromDataBase();
        }


        private void ChangeDataType(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StackPanel stackPanel = radioButton.Parent as StackPanel;
            int idx = stackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            vieModel_StartUp.CurrentSideIdx = idx;
            GlobalVariable.CurrentDataType = (DataType)idx;
            GlobalConfig.StartUp.SideIdx = idx;
            vieModel_StartUp.ReadFromDataBase();
        }

        public async void LoadDataBase()
        {
            //加载数据库
            long id = vieModel_StartUp.CurrentDBID;
            if (listBox.SelectedIndex < 0) return;
            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            id = info.DBId;

            vieModel_StartUp.Loading = true;
            //if (Properties.Settings.Default.ScanGivenPath)
            //{
            //    try
            //    {
            //        await Task.Run(() =>
            //        {

            //            this.Dispatcher.BeginInvoke(new Action(() =>
            //            {
            //                //statusText.Text = Jvedio.Language.Resources.Status_ScanDir; 
            //            }), System.Windows.Threading.DispatcherPriority.Background);
            //            List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath)), ct);
            //            Scan.InsertWithNfo(filepaths, ct);
            //        }, cts.Token);
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.LogF(ex);
            //    }

            //}
            vieModel_StartUp.Loading = false;
            // 次数+1
            appDatabaseMapper.increaseFieldById("ViewCount", id);
            vieModel_StartUp.CurrentDBID = id;
            GlobalConfig.StartUp.CurrentDBID = vieModel_StartUp.CurrentDBID;
            GlobalConfig.Main.CurrentDBId = id;
            //启动主窗口

            if (CurrentDataType == DataType.Video)
            {
                Main main = GetWindowByName("Main") as Main;
                if (main == null)
                {
                    main = new Main();
                    Application.Current.MainWindow = main;
                }
                else
                {
                    main.setDataBases();
                    main.setComboboxID();
                }
                main.Show();
            }
            else
            {
                Window_MetaDatas metaData = GetWindowByName("Window_MetaDatas") as Window_MetaDatas;
                if (metaData == null)
                {
                    metaData = new Window_MetaDatas();
                    metaData.Title = "Jvedio-" + CurrentDataType.ToString();
                    Application.Current.MainWindow = metaData;
                }
                else
                {
                    metaData.setDataBases();
                    metaData.setComboboxID();
                }
                metaData.Show();
            }
            this.Close();
        }


        public void LoadDataBase(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;
            LoadDataBase();
        }

        private void SetImage(object sender, RoutedEventArgs e)
        {
            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "选择图片";
            dialog.Filter = "(jpg;jpeg;png)|*.jpg;*.jpeg;*.png";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = dialog.FileName;
                SetImage(info.DBId, filename);
            }
        }

        private void SetImage(long id, string imagePath)
        {
            // 复制到 ProjectImagePath 下
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string ext = Path.GetExtension(imagePath).ToLower();
            string newName = $"{id}_{name}{ext}";
            string newPath = Path.Combine(GlobalVariable.ProjectImagePath, newName);
            FileHelper.TryCopyFile(imagePath, newPath);


            vieModel_StartUp.Databases.Where(x => x.DBId == id).SingleOrDefault().ImagePath = newPath;
            vieModel_StartUp.CurrentDatabases.Where(x => x.DBId == id).SingleOrDefault().ImagePath = newPath;
            appDatabaseMapper.updateFieldById("ImagePath", newName, id);
        }

        private void LoadDataBase(object sender, RoutedEventArgs e)
        {
            LoadDataBase();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ShowHideDataBase(object sender, RoutedEventArgs e)
        {
            vieModel_StartUp.ReadFromDataBase();
        }

        private void HideDataBase(object sender, RoutedEventArgs e)
        {
            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            info.Hide = info.Hide == 0 ? 1 : 0;
            appDatabaseMapper.updateById(info);
            vieModel_StartUp.ReadFromDataBase();
        }

        private void Window_StartUp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalConfig.StartUp.Tile = vieModel_StartUp.Tile;
            GlobalConfig.StartUp.ShowHideItem = vieModel_StartUp.ShowHideItem;
            GlobalConfig.StartUp.SideIdx = vieModel_StartUp.CurrentSideIdx;
            GlobalConfig.StartUp.CurrentDBID = vieModel_StartUp.CurrentDBID;
            GlobalConfig.StartUp.Sort = vieModel_StartUp.Sort;
            GlobalConfig.StartUp.SortType = string.IsNullOrEmpty(vieModel_StartUp.SortType) ? "名称" : vieModel_StartUp.SortType;
            GlobalConfig.StartUp.Save();

            Main main = GetWindowByName("Main") as Main;
            if (main != null && !main.IsActive) main.Close();

        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadDataBase();
        }

        private void Window_StartUp_ContentRendered(object sender, EventArgs e)
        {

        }

        private void ShowHelpPicture(object sender, MouseButtonEventArgs e)
        {
            MessageCard.Info("新建库并打开后，将含有图片的文件夹拖入（仅扫描当前文件夹，不扫描子文件夹），即可导入");
        }

        private void ShowHelpGame(object sender, MouseButtonEventArgs e)
        {
            MessageCard.Info("新建库并打开后，将含有 EXE 的文件夹拖入（仅扫描当前文件夹，不扫描子文件夹），即可导入");

        }

        private void ShowHelpComics(object sender, MouseButtonEventArgs e)
        {
            MessageCard.Info("新建库并打开后，将含有漫画图片的文件夹拖入（仅扫描当前文件夹，不扫描子文件夹），即可导入");
        }
    }
}
