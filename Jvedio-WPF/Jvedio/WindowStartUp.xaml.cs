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

using Jvedio.Core.pojo;

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
            string[] oldFIles = { "info.sqlite", "AI.sqlite", "Magnets.sqlite", "Translate.sqlite" };
            foreach (var item in oldFIles)
            {
                FileHelper.TryMoveFile(item, Path.Combine(GlobalVariable.CurrentUserFolder, item));
            }
            string ScanPathConfig = Path.Combine(oldDataPath, "ScanPathConfig");
            if (File.Exists(ScanPathConfig))
            {
                FileHelper.TryMoveFile(ScanPathConfig, GlobalVariable.ScanConfigPath);
            }
            string ServersConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServersConfig");
            if (File.Exists(ServersConfigPath))
            {
                FileHelper.TryMoveFile(ServersConfigPath, GlobalVariable.ServersConfigPath);
            }

            string[] files = FileHelper.TryScanDIr(oldDataPath, "*.sqlite", SearchOption.TopDirectoryOnly);
            if (files != null)
            {
                foreach (var item in files)
                {
                    if (File.Exists(item))
                    {
                        string name = Path.GetFileName(item);
                        string newFileName = Path.Combine(GlobalVariable.VideoDataPath, name);
                        bool success = MoveOldData(item, newFileName);
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
        }




        /// <summary>
        /// 移动旧数据库到新数据库
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool MoveOldData(string origin, string target)
        {
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                VideoConnection newConnection = new VideoConnection(target);
                newConnection.InitTables();
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from movie");
                if (sr != null)
                {
                    while (sr.Read())
                    {
                        DetailMovie detailMovie = new DetailMovie()
                        {
                            id = sr["id"].ToString(),
                            title = sr["title"].ToString(),
                            filepath = sr["filepath"].ToString(),
                            subsection = sr["subsection"].ToString(),
                            scandate = sr["scandate"].ToString(),
                            releasedate = sr["releasedate"].ToString(),
                            director = sr["director"].ToString(),
                            genre = sr["genre"].ToString(),
                            tag = sr["tag"].ToString(),
                            actor = sr["actor"].ToString(),
                            actorid = sr["actorid"].ToString(),
                            studio = sr["studio"].ToString(),
                            chinesetitle = sr["chinesetitle"].ToString(),
                            label = sr["label"].ToString(),
                            plot = sr["plot"].ToString(),
                            outline = sr["outline"].ToString(),
                            country = sr["country"].ToString(),
                            otherinfo = sr["otherinfo"].ToString(),
                            actressimageurl = sr["actressimageurl"].ToString(),
                            smallimageurl = sr["smallimageurl"].ToString(),
                            bigimageurl = sr["bigimageurl"].ToString(),
                            extraimageurl = sr["extraimageurl"].ToString(),
                            sourceurl = sr["sourceurl"].ToString(),
                            source = sr["source"].ToString()
                        };

                        double.TryParse(sr["filesize"].ToString(), out double filesize);
                        int.TryParse(sr["vediotype"].ToString(), out int vediotype);
                        int.TryParse(sr["visits"].ToString(), out int visits);
                        int.TryParse(sr["favorites"].ToString(), out int favorites);
                        int.TryParse(sr["year"].ToString(), out int year);
                        int.TryParse(sr["countrycode"].ToString(), out int countrycode);
                        int.TryParse(sr["runtime"].ToString(), out int runtime);
                        float.TryParse(sr["rating"].ToString(), out float rating);
                        detailMovie.filesize = filesize;
                        detailMovie.vediotype = vediotype;
                        detailMovie.visits = visits;
                        detailMovie.rating = rating;
                        detailMovie.favorites = favorites;
                        detailMovie.year = year;
                        detailMovie.countrycode = countrycode;
                        detailMovie.runtime = runtime;

                        //导入新数据库中
                        newConnection.insertMovie(detailMovie);
                        break; // 测试
                    }
                }

                oldSqlite.CloseDB();
                newConnection.Close();
                return true;
            }
            return false;
        }

        private void InitAppData()
        {
            try
            {
                InitDataBase();//初始化数据库
                Identify.InitFanhaoList();
                Scan.InitSearchPattern();
                GlobalVariable.InitVariable();
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

            SqliteInfo sqliteInfo = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string path = sqliteInfo.Path;
            string name = sqliteInfo.Name;
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
                        ConfigConnection.Instance.DeleteByIds(new List<int>() { sqliteInfo.ID });
                        vieModel_StartUp.ScanDatabase();
                    }
                }
            }
        }


        private void RenameSqlite(object sender, RoutedEventArgs e)
        {

            SqliteInfo sqliteInfo = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string originName = sqliteInfo.Name;
            string originPath = sqliteInfo.Path;
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
            sqliteInfo.Name = targetName;
            sqliteInfo.Path = targetPath;
            FileHelper.TryMoveFile(originPath, targetPath);
            ConfigConnection.Instance.UpdateSqliteInfoPath(sqliteInfo);
            vieModel_StartUp.ScanDatabase(); // todo 仅更新重命名的
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
            SqliteInfo info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string path = info.Path;
            int id = info.ID;
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
            ConfigConnection.Instance.IncreaseField("ViewCount", id);

            this.Close();
        }


        public void LoadDataBase(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;
            LoadDataBase();
        }

        private void SetImage(object sender, RoutedEventArgs e)
        {
            SqliteInfo info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "选择图片";
            dialog.Filter = "(jpg;jpeg;png)|*.jpg;*.jpeg;*.png";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = dialog.FileName;
                SetImage(info.ID, filename);
            }
        }

        private void SetImage(int id, string imagePath)
        {
            // 复制到 ProjectImagePath 下
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string ext = Path.GetExtension(imagePath);
            string newName = $"{id}_{name}{ext}";
            string newPath = Path.Combine(GlobalVariable.ProjectImagePath, newName);
            FileHelper.TryCopyFile(imagePath, newPath);


            vieModel_StartUp.Databases.Where(x => x.ID == id).SingleOrDefault().ImagePath = newPath;
            vieModel_StartUp.CurrentDatabases.Where(x => x.ID == id).SingleOrDefault().ImagePath = newPath;

            ConfigConnection.Instance.UpdateSqliteInfoField("ImagePath", newName, id);
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
