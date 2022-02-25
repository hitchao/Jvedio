using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Newtonsoft.Json;
using Jvedio.Core;
using System.Windows.Controls.Primitives;
using ChaoControls.Style;

using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Core.SqlMapper;
using Jvedio.Test;
using Jvedio.Core.Sql;
using Jvedio.Entity.CommonSQL;

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



        private void testMapper()
        {

            string demo_path = @"D:\Jvedio\Jvedio\Jvedio-WPF\Jvedio\Sql\demo.sqlite";



            AppDatabaseMapper appDatabaseMapper = new AppDatabaseMapper(demo_path);
            TestMapper.insertAndSelect(appDatabaseMapper);

            //TranslationMapper mapper = new TranslationMapper(demo_path);
            //TestMapper.insertAndSelect(mapper);
            //ComImagesMapper imagesMapper = new ComImagesMapper(demo_path);
            //TestMapper.insertAndSelect(imagesMapper);




            this.Close();
        }



        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalVariable.InitVariable();// 初始化全局变量
            EnsureSettings();//修复设置错误
            EnsureFileExists(); //判断文件是否存在
            EnsureDirExists();//创建文件夹
            MoveOldFiles();//迁移旧文件并迁移到新数据库

            Jvedio.Core.ThemeLoader.loadAllThemes(); //加载主题
            if (GlobalFont != null) this.FontFamily = GlobalFont;
            SetSkin(Properties.Settings.Default.Themes);//设置皮肤
            InitAppData();// 初始化应用数据
            InitClean();//清理文件

            vieModel_StartUp = new VieModel_StartUp();
            this.DataContext = vieModel_StartUp;

            //默认打开某个数据库
            if (Properties.Settings.Default.OpenDataBaseDefault && File.Exists(Properties.Settings.Default.DataBasePath))
            {
                OpenDefaultDatabase();
                //启动主窗口
                Main main = new Main();
                try
                {
                    await main.InitMovie();
                }
                catch (Exception ex)
                {
                    Logger.LogE(ex);
                }
                main.Show();
                this.Close();
            }
            else
            {
                vieModel_StartUp.InitCompleted = true;
            }
        }



        private void MoveOldFiles()
        {
            // 迁移公共数据
            Jvedio4ToJvedio5.MoveAI();
            Jvedio4ToJvedio5.MoveMagnets();
            Jvedio4ToJvedio5.MoveTranslate();

            string[] files = FileHelper.TryScanDIr(oldDataPath, "*.sqlite", SearchOption.TopDirectoryOnly);
            Jvedio4ToJvedio5.MoveScanPathConfig(files);
            Jvedio4ToJvedio5.MoveServersConfig();
            // 迁移库

            if (files != null)
            {
                foreach (var item in files)
                {
                    if (File.Exists(item))
                    {
                        string name = Path.GetFileName(item);
                        string newFileName = Path.Combine(GlobalVariable.VideoDataPath, name);
                        bool success = Jvedio4ToJvedio5.MoveOldData(item, newFileName);
                        if (success)
                        {
                            //FileHelper.TryDeleteFile(item);
                        }
                    }
                }
                //files = FileHelper.TryScanDIr(oldDataPath, "*.sqlite", SearchOption.TopDirectoryOnly);
                //if (files != null && files.Length == 0) FileHelper.TryDeleteDir(oldDataPath);
                //迁移新数据

            }
            Jvedio4ToJvedio5.MoveRecentWatch(); //todo MoveRecentWatch
        }






        private void InitAppData()
        {
            try
            {
                InitDataBase();//初始化数据库
                Identify.InitFanhaoList();
                Scan.InitSearchPattern();

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

            try
            {
                //备份文件
                BackUp(GlobalVariable.MagnetsDataBasePath);
                BackUp(GlobalVariable.AIDataBasePath);
                BackUp(GlobalVariable.TranslateDataBasePath);
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
            try
            {
                if (Properties.Settings.Default.ScanGivenPath)
                {
                    // todo 开启是扫描文件夹
                    await Task.Run(() =>
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //statusText.Text = Jvedio.Language.Resources.Status_ScanDir; 
                        }), System.Windows.Threading.DispatcherPriority.Render);
                        List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath)), ct);
                        Scan.InsertWithNfo(filepaths, ct);
                    }, cts.Token);

                }
            }
            catch (Exception ex)
            {
                Logger.LogE(ex);
            }
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

            if (!File.Exists("BusActress.sqlite"))
            {
                MessageBox.Show($"{Jvedio.Language.Resources.Missing} BusActress.sqlite", "Jvedio");
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

        private void InitDataBase()
        {
            //GlobalConnection.Init();
            if (!File.Exists(AIDataBasePath))
            {
                MySqlite db = new MySqlite(AIDataBasePath);
                db.CreateTable(DataBase.SQLITETABLE_BAIDUAI);
                db.CloseDB();
            }
            else
            {
                //是否具有表结构
                MySqlite db = new MySqlite(AIDataBasePath);
                if (!db.IsTableExist("baidu")) db.CreateTable(DataBase.SQLITETABLE_BAIDUAI);
                db.CloseDB();
            }


            if (!File.Exists(TranslateDataBasePath))
            {
                MySqlite db = new MySqlite(TranslateDataBasePath);
                db.CreateTable(DataBase.SQLITETABLE_YOUDAO);
                db.CreateTable(DataBase.SQLITETABLE_BAIDUTRANSLATE);
                db.CloseDB();
            }
            else
            {
                //是否具有表结构
                MySqlite db = new MySqlite(TranslateDataBasePath);
                if (!db.IsTableExist("youdao")) db.CreateTable(DataBase.SQLITETABLE_YOUDAO);
                if (!db.IsTableExist("baidu")) db.CreateTable(DataBase.SQLITETABLE_BAIDUTRANSLATE);
                db.CloseDB();
            }

            if (!File.Exists(MagnetsDataBasePath))
            {
                MySqlite db = new MySqlite(MagnetsDataBasePath);
                db.CreateTable(DataBase.SQLITETABLE_MAGNETS);
                db.CloseDB();
            }

        }




        private void ImportDatabase(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseDataBase;
            OpenFileDialog1.Filter = $"Sqlite {Jvedio.Language.Resources.File}|*.sqlite";
            OpenFileDialog1.Multiselect = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] names = OpenFileDialog1.FileNames;
                foreach (var item in names)
                {
                    string name = Path.GetFileNameWithoutExtension(item);
                    if (!DataBase.IsProperSqlite(item))
                    {
                        MessageCard.Show("不支持该文件：" + item);
                        continue;
                    }
                    string targetPath = Path.Combine(GlobalVariable.DataPath, GlobalVariable.CurrentInfoType.ToString(), name + ".sqlite");
                    if (File.Exists(targetPath))
                    {
                        if (new Msgbox(this, $"{Jvedio.Language.Resources.Message_AlreadyExist} {name} {Jvedio.Language.Resources.IsToOverWrite} ？").ShowDialog() == true)
                        {
                            FileHelper.TryCopyFile(item, targetPath, true);
                            vieModel_StartUp.ScanDatabase();
                        }
                    }
                    else
                    {
                        FileHelper.TryCopyFile(item, targetPath, true);
                        vieModel_StartUp.ScanDatabase();
                    }
                }
            }
        }




        private void DelSqlite(object sender, RoutedEventArgs e)
        {

            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string path = info.Path;
            string name = info.Name;
            if (new Msgbox(this, $"{Jvedio.Language.Resources.IsToDelete} {name}?").ShowDialog() == true)
            {
                string dirpath = DateTime.Now.ToString("yyyyMMddHHmm");
                string backupPath = Path.Combine(GlobalVariable.BackupPath, dirpath);
                FileHelper.TryCreateDir(backupPath);
                if (File.Exists(path))
                {
                    //备份
                    FileHelper.TryCopyFile(path, Path.Combine(backupPath, name + ".sqlite"), true);
                    //删除
                    if (FileHelper.TryDeleteFile(path))
                    {
                        GlobalVariable.AppDatabaseMapper.deleteById(info.DBId);
                        vieModel_StartUp.ScanDatabase();
                    }
                }
            }
        }


        private void RenameSqlite(object sender, RoutedEventArgs e)
        {

            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string originName = info.Name;
            string originPath = info.Path;
            DialogInput input = new DialogInput(this, Jvedio.Language.Resources.Rename, originName);
            if (input.ShowDialog() == false) return;
            string targetName = input.Text;
            if (targetName == originName) return;
            if (string.IsNullOrEmpty(targetName) || targetName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
            {
                MessageCard.Show("名称非法！");
                return;
            }
            string targetPath = Path.Combine(GlobalVariable.DataPath, GlobalVariable.CurrentInfoType.ToString(), targetName + ".sqlite");
            if (File.Exists(targetPath))
            {
                MessageCard.Show(Jvedio.Language.Resources.Message_AlreadyExist);
                return;
            }
            info.Name = targetName;
            info.Path = targetPath;
            FileHelper.TryMoveFile(originPath, targetPath);
            GlobalVariable.AppDatabaseMapper.updateById(info);

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
            if (string.IsNullOrEmpty(targetName) || targetName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
            {
                MessageCard.Show("名称非法！");
                return;
            }
            if (vieModel_StartUp.Databases.Where(x => x.Name == targetName).Any())
            {
                MessageCard.Show(Jvedio.Language.Resources.Message_AlreadyExist);
                return;
            }

            Jvedio.Core.Command.Sqlite.CreateVideoDataBase.Execute(targetName);
            vieModel_StartUp.ScanDatabase();
        }

        private void RefreshDatabase(object sender, MouseButtonEventArgs e)
        {
            vieModel_StartUp.ScanDatabase();
        }



        private void SideRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StackPanel stackPanel = radioButton.Parent as StackPanel;
            int idx = stackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            vieModel_StartUp.CurrentSideIdx = idx;
            GlobalVariable.CurrentInfoType = (InfoType)idx;
            vieModel_StartUp.ScanDatabase(GlobalVariable.CurrentInfoType);
        }

        public async void LoadDataBase()
        {

            //加载数据库
            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string path = info.Path;
            long id = info.DBId;
            Properties.Settings.Default.DataBasePath = path;
            if (!File.Exists(Properties.Settings.Default.DataBasePath)) return;
            vieModel_StartUp.InitCompleted = false;
            if (Properties.Settings.Default.ScanGivenPath)
            {
                try
                {
                    await Task.Run(() =>
                    {

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //statusText.Text = Jvedio.Language.Resources.Status_ScanDir; 
                        }), System.Windows.Threading.DispatcherPriority.Background);
                        List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath)), ct);
                        Scan.InsertWithNfo(filepaths, ct);
                    }, cts.Token);
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                }

            }


            //启动主窗口
            Main main = new Main();
            //statusText.Text = Jvedio.Language.Resources.Status_InitMovie;
            try
            {
                await main.InitMovie();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.LogE(ex);
            }
            main.Show();

            // 次数+1
            GlobalVariable.AppDatabaseMapper.increaseFieldById("ViewCount", id);

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
            string ext = Path.GetExtension(imagePath);
            string newName = $"{id}_{name}{ext}";
            string newPath = Path.Combine(GlobalVariable.ProjectImagePath, newName);
            FileHelper.TryCopyFile(imagePath, newPath);


            vieModel_StartUp.Databases.Where(x => x.DBId == id).SingleOrDefault().ImagePath = newPath;
            vieModel_StartUp.CurrentDatabases.Where(x => x.DBId == id).SingleOrDefault().ImagePath = newPath;
            GlobalVariable.AppDatabaseMapper.updateFieldById("ImagePath", newName, id);
        }

        private void LoadDataBase(object sender, RoutedEventArgs e)
        {
            LoadDataBase();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
