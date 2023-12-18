using Jvedio.Core.Config;
using Jvedio.Core.DataBase;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Core.Global;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Upgrade;
using Jvedio.ViewModel;
using Jvedio.Windows;
using SuperControls.Style;
using SuperControls.Style.CSFile.Interfaces;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.IO;
using SuperUtils.Systems;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Jvedio.App;
using static Jvedio.Core.Global.PathManager;
using static Jvedio.MapperManager;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    public partial class WindowStartUp : BaseWindow, IBaseWindow
    {
        /// <summary>
        /// 日志保存期限
        /// </summary>
        private const int CLEAR_LOG_DAY = -10;

        /// <summary>
        /// 备份文件期限
        /// </summary>
        private const int CLEAR_BACKUP_DAY = -30;

        /// <summary>
        /// 标题栏高度
        /// </summary>
        private const int DEFAULT_TITLE_HEIGHT = 30;

        #region "属性"

        private CancellationTokenSource cts { get; set; }
        private CancellationToken ct { get; set; }
        private VieModel_StartUp vieModel { get; set; }

        private ScanTask ScanTask { get; set; }

        private bool EnteringDataBase { get; set; }

        private bool CancelScanTask { get; set; }

        #endregion

        public WindowStartUp()
        {
            InitializeComponent();
            Init();
        }


        public void Init()
        {
            this.Width = SystemParameters.PrimaryScreenWidth * 2 / 3;
            this.Height = SystemParameters.PrimaryScreenHeight * 2 / 3;

            cts = new CancellationTokenSource();
            cts.Token.Register(() => Logger.Warn("cancel task"));
            ct = cts.Token;
        }

        public void Dispose()
        {
            if (ConfigManager.StartUp == null)
                return;
            ConfigManager.StartUp.Tile = vieModel.Tile;
            ConfigManager.StartUp.ShowHideItem = vieModel.ShowHideItem;
            ConfigManager.StartUp.SideIdx = vieModel.CurrentSideIdx;

            ConfigManager.StartUp.Sort = vieModel.Sort;
            ConfigManager.StartUp.SortType =
                string.IsNullOrEmpty(vieModel.SortType) ? LangManager.GetValueByKey("Title") : vieModel.SortType;
            ConfigManager.StartUp.Save();

            Main main = GetWindowByName("Main", App.Current.Windows) as Main;
            if (main != null && !main.IsActive && !EnteringDataBase) {
                ConfigManager.SaveAll();
                Application.Current.Shutdown();
            }
        }

        public void InitContext()
        {
            vieModel = new VieModel_StartUp();
            this.DataContext = vieModel;

            List<RadioButton> radioButtons = SidePanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < radioButtons.Count; i++) {
                if (i == vieModel.CurrentSideIdx)
                    radioButtons[i].IsChecked = true;
                else
                    radioButtons[i].IsChecked = false;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ************************************
            // ************* 以下顺序不可移动 ******
            // ************************************

            EnsureFileExists();
            EnsureDirExists();

            InitMapper(); // 初始化数据库
            ConfigManager.Init(() => SetLang()); // 从数据库加载应用配置

            await MoveOldFiles();
            InitAppData();
            DeleteDirs();
            await BackupData();
            await MovePlugins();
            await DeletePlugins();
            CrawlerManager.Init(true);
            ConfigManager.ServerConfig.Read();

            InitContext();
            UtilsManager.OnUtilSettingChange(); // 初始化 SuperUtils 的配置
            InitFirstRun();
            InitMainWindow();
            InitBinding();
        }

        private void InitMainWindow()
        {
            Main.CurrentDataType = (DataType)ConfigManager.StartUp.SideIdx;
            if (!Main.ClickGoBackToStartUp && ConfigManager.Settings.OpenDataBaseDefault) {
                tabControl.SelectedIndex = 0;
                LoadDataBase();
            } else {
                tabControl.SelectedIndex = 1;
                vieModel.Loading = false;
            }
        }

        private void InitBinding()
        {

        }


        private void InitFirstRun()
        {
#if DEBUG
            //ConfigManager.Main.FirstRun = true;
#endif
            if (ConfigManager.Main.FirstRun) {
                Dialog_SetSkinLang skinLang = new Dialog_SetSkinLang();
                skinLang.SetThemeConfig(ConfigManager.ThemeConfig.ThemeIndex,
                    ConfigManager.ThemeConfig.ThemeID);
                skinLang.InitThemes();
                Dialog_SetSkinLang.OnLangChanged += (lang) => {
                    Jvedio.Core.Lang.LangManager.SetLang(lang);
                    ConfigManager.Settings.CurrentLanguage = lang;
                    ConfigManager.Settings.Save();
                };
                Dialog_SetSkinLang.OnThemeChanged += (ThemeIdx, ThemeID) => {
                    ConfigManager.ThemeConfig.ThemeIndex = ThemeIdx;
                    ConfigManager.ThemeConfig.ThemeID = ThemeID;
                    ConfigManager.ThemeConfig.Save();
                };
                skinLang.ShowDialog(this);
            }
        }



        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        private void InitMapper()
        {
            if (!MapperManager.Init()) {
                MsgBox.Show($"{LangManager.GetValueByKey("LibraryInitFailed")}");
                App.Current.Shutdown();
            }
        }


        private void SetLang()
        {
            // 设置语言
            if (!string.IsNullOrEmpty(ConfigManager.Settings.CurrentLanguage)
                && SuperControls.Style.LangManager.SupportLanguages.Contains(ConfigManager.Settings.CurrentLanguage)) {
                SuperControls.Style.LangManager.SetLang(ConfigManager.Settings.CurrentLanguage);
                Jvedio.Core.Lang.LangManager.SetLang(ConfigManager.Settings.CurrentLanguage);
            }
        }

        private async Task<bool> MovePlugins()
        {
            await Task.Delay(1);
            string path = Path.Combine(PathManager.BasePluginsPath, "temp");
            bool success = DirHelper.TryCopy(path, PathManager.BasePluginsPath);
            if (success)
                DirHelper.TryDelete(path);
            return true;
        }

        private async Task<bool> DeletePlugins()
        {
            await Task.Delay(1);
            List<string> list = JsonUtils.TryDeserializeObject<List<string>>(ConfigManager.PluginConfig.DeleteList);
            if (list != null && list.Count > 0) {
                for (int i = list.Count - 1; i >= 0; i--) {
                    string pluginId = list[i];
                    string[] paths = pluginId.Split(new char[] { '/', '\\' });
                    if (paths.Length <= 1)
                        continue;
                    string type = paths[0];
                    if (!type.EndsWith("s"))
                        type += "s";
                    paths[0] = type;
                    string targetPath = Path.GetFullPath(Path.Combine(PathManager.BasePluginsPath, paths.Aggregate(Path.Combine)));
                    if (Directory.Exists(targetPath)) {
                        DirHelper.TryDelete(targetPath);
                        list.RemoveAt(i);
                    }
                }
                ConfigManager.PluginConfig.DeleteList = JsonUtils.TrySerializeObject(list);
                ConfigManager.PluginConfig.Save();
            }
            return true;
        }

        private async Task<bool> BackupData()
        {
            if (ConfigManager.Settings.AutoBackup) {
                int period = Jvedio.Core.WindowConfig.Settings.BackUpPeriods[(int)ConfigManager.Settings.AutoBackupPeriodIndex];
                bool backup = false;
                string[] arr = DirHelper.TryGetDirList(PathManager.BackupPath);
                if (arr != null && arr.Length > 0) {
                    string dirname = arr[arr.Length - 1];
                    if (Directory.Exists(dirname)) {
                        string dirName = Path.GetFileName(dirname);
                        DateTime before = DateTime.Now.AddDays(1);
                        DateTime now = DateTime.Now;
                        DateTime.TryParse(dirName, out before);
                        if (now.CompareTo(before) < 0 || (now - before).TotalDays > period) {
                            backup = true;
                        }
                    }
                } else {
                    backup = true;
                }

                if (backup) {
                    string dir = Path.Combine(BackupPath, DateHelper.NowDate());
                    DirHelper.TryCreateDirectory(dir, (Action<Exception>)((err) => {
                        Logger.Error(err);
                        return;
                    }));
                    string target1 = Path.Combine(dir, "app_configs.sqlite");
                    string target2 = Path.Combine(dir, "app_datas.sqlite");
                    string target3 = Path.Combine(dir, "image");
                    FileHelper.TryCopyFile(SqlManager.DEFAULT_SQLITE_CONFIG_PATH, target1);
                    FileHelper.TryCopyFile(SqlManager.DEFAULT_SQLITE_PATH, target2);
                    string origin = Path.Combine(CurrentUserFolder, "image");
                    DirHelper.TryCopy(origin, target3);
                }
            }

            await Task.Delay(1);
            return false;
        }

        private async Task<bool> MoveOldFiles()
        {
            // 迁移公共数据
            Jvedio4ToJvedio5.MoveAI();
            string[] files = FileHelper.TryScanDIr(oldDataPath, "*.sqlite", SearchOption.TopDirectoryOnly);
            bool success = await Jvedio4ToJvedio5.MoveDatabases(files);
            if (success && files != null && files.Length > 0) {
                Jvedio4ToJvedio5.MoveRecentWatch();
                Jvedio4ToJvedio5.MoveMagnets();
                Jvedio4ToJvedio5.MoveTranslate();
                Jvedio4ToJvedio5.MoveMyList();
                Jvedio4ToJvedio5.MoveScanPathConfig(files);
                ConfigManager.Settings.OpenDataBaseDefault = false;
            }

            // 移动文件
            string targetDir = Path.Combine(AllOldDataPath, "DataBase");
            if (Directory.Exists(targetDir))
                DirHelper.TryDelete(targetDir);
            DirHelper.TryMoveDir(oldDataPath, targetDir); // 移动 DataBase
            string[] moveFiles =
            {
                "SearchHistory", "ServersConfig", "RecentWatch",
                "AI.sqlite", "Magnets.sqlite", "Translate.sqlite", "mylist.sqlite",
            };

            foreach (string filename in moveFiles) {
                string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                if (File.Exists(origin))
                    FileHelper.TryMoveFile(origin, Path.Combine(AllOldDataPath, filename));
            }

            return true;
        }

        private void InitAppData()
        {
            try {
                VideoParser.InitSearchPattern();
                // 读取配置文件，设置 debug
                ReadConfig();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void ReadConfig()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (!File.Exists(path))
                return;
            string value = FileHelper.TryReadFile(path);
            foreach (var item in value.Split('\n')) {
                if (!string.IsNullOrEmpty(item) && item.IndexOf("=") > 0 && item.Length >= 3) {
                    string p = item.Split('=')[0];
                    string v = item.Split('=')[1].Replace("\r", string.Empty);
                    if ("debug".Equals(p) && "true".Equals(v.ToLower())) {
                        ConfigManager.Settings.Debug = true;
                    } else {
                        ConfigManager.Settings.Debug = false;
                    }
                }
            }
            Logger.Info($"debug mode: {ConfigManager.Settings.Debug}");
        }


        /// <summary>
        /// 删除
        /// </summary>
        private void DeleteDirs()
        {
            try {
                // 清除日志
                ClearLogBefore(CLEAR_LOG_DAY, PathManager.LogPath);
                // 清除备份文件
                DeleteDirBefore(CLEAR_BACKUP_DAY, PathManager.BackupPath);

                if (!ConfigManager.Settings.Debug) {
                    FileHelper.TryDeleteFile("upgrade.bat");
                    FileHelper.TryDeleteFile("upgrade-plugins.bat");
                    FileHelper.TryDeleteDir("Temp");
                }

                FileHelper.TryDeleteDir(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "themes", "temp"));

            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public void ClearLogBefore(int day, string filepath)
        {
            DateTime dateTime = DateTime.Now.AddDays(day);
            filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filepath);
            if (!Directory.Exists(filepath))
                return;
            try {
                string[] files = Directory.GetFiles(filepath, "*.log", SearchOption.TopDirectoryOnly);
                foreach (var file in files) {
                    DateTime.TryParse(file.Split('\\').Last().Replace(".log", string.Empty), out DateTime date);
                    if (date < dateTime)
                        FileHelper.TryDeleteFile(file);
                }
            } catch (Exception e) {
                Logger.Error(e);
            }
        }

        public void DeleteDirBefore(int day, string dir)
        {
            DateTime dateTime = DateTime.Now.AddDays(day);
            if (!Directory.Exists(dir))
                return;
            try {
                string[] dirs = Directory.GetDirectories(dir);
                foreach (var dirName in dirs) {
                    DateTime.TryParse(dirName.Split('\\').Last(), out DateTime date);
                    if (date < dateTime)
                        DirHelper.TryDelete(dirName);
                }
            } catch (Exception e) {
                Logger.Error(e);
            }
        }

        public void EnsureDirExists()
        {
            foreach (var item in InitDirs) {
                FileHelper.TryCreateDir(item);
            }

            foreach (var item in PicPaths) {
                FileHelper.TryCreateDir(Path.Combine(PicPath, item));
            }
        }

        public void EnsureFileExists()
        {
            foreach (var file in ReferenceDllPaths) {
                if (!File.Exists(file)) {
                    MsgBox.Show($"{SuperControls.Style.LangManager.GetValueByKey("Missing")} {file}");
                    Application.Current.Shutdown();
                }
            }
        }

        private void DelSqlite(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel.CurrentDatabases.Count || listBox.SelectedIndex < 0) {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase database = vieModel.CurrentDatabases[listBox.SelectedIndex];
            MsgBox msgbox = new MsgBox(
                $"{LangManager.GetValueByKey("IsToDelete")} {database.Name} {LangManager.GetValueByKey("And")} {database.Count} {LangManager.GetValueByKey("TagLabelAndOtherInfo")}");
            if (msgbox.ShowDialog() == true) {
                try {
                    database.deleteByID(database.DBId);
                    RefreshDatabase();
                    Main main = GetWindowByName("Main", App.Current.Windows) as Main;
                    if (main != null) {
                        // 重置当前
                        //main.vieModel.Select(); // todo tab
                        main.vieModel.Statistic();
                    }
                } catch (SqlException ex) {
                    MessageCard.Error(ex.Message);
                }
            }
        }

        private void RenameSqlite(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel.CurrentDatabases.Count || listBox.SelectedIndex < 0) {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase info = vieModel.CurrentDatabases[listBox.SelectedIndex];
            string originName = info.Name;
            DialogInput input = new DialogInput(SuperControls.Style.LangManager.GetValueByKey("Rename"), originName);
            if (input.ShowDialog(this) == false)
                return;
            string targetName = input.Text;
            if (string.IsNullOrEmpty(targetName))
                return;
            if (targetName == originName)
                return;
            info.Name = targetName;
            appDatabaseMapper.UpdateById(info);

            // 仅更新重命名的
            vieModel.refreshItem();
        }

        private void SortDatabases(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                string header = menuItem.Header.ToString();
                vieModel.SortType = header;
                vieModel.Sort = !vieModel.Sort;
                vieModel.Search();
            }
        }

        private void SearchText_Changed(object sender, RoutedEventArgs e)
        {
            SuperControls.Style.SearchBox textBox = sender as SuperControls.Style.SearchBox;
            if (textBox == null)
                return;
            vieModel.CurrentSearch = textBox.Text;
            vieModel.Search();
        }

        private void NewDatabase(object sender, RoutedEventArgs e)
        {
            vieModel.CurrentSearch = string.Empty;
            vieModel.Sort = true;
            vieModel.SortType = LangManager.GetValueByKey("CreatedDate");
            DialogInput input = new DialogInput(SuperControls.Style.LangManager.GetValueByKey("NewLibrary"));
            if (input.ShowDialog(this) == false)
                return;
            string targetName = input.Text;
            if (string.IsNullOrEmpty(targetName))
                return;

            // 创建新数据库
            AppDatabase appDatabase = new AppDatabase();
            appDatabase.Name = targetName;
            appDatabase.DataType = (DataType)vieModel.CurrentSideIdx;
            appDatabaseMapper.Insert(appDatabase);
            RefreshDatabase();
        }


        public void RefreshDatabase()
        {
            vieModel.ReadFromDataBase();
        }

        private void ChangeDataType(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StackPanel stackPanel = radioButton.Parent as StackPanel;
            int idx = stackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            vieModel.CurrentSideIdx = idx;
            Main.CurrentDataType = (DataType)idx;
            ConfigManager.StartUp.SideIdx = idx;
            vieModel.ReadFromDataBase();
        }

        public async void LoadDataBase()
        {
            if (vieModel.CurrentDatabases == null || vieModel.CurrentDatabases.Count <= 0) {
                ConfigManager.Settings.OpenDataBaseDefault = false;
                vieModel.Loading = false;
                tabControl.SelectedIndex = 1;
                return;
            }

            List<AppDatabase> appDatabases = appDatabaseMapper.SelectList();

            // 加载数据库
            long id = ConfigManager.Main.CurrentDBId;
            AppDatabase database = null;
            if (Main.ClickGoBackToStartUp || !ConfigManager.Settings.OpenDataBaseDefault) {
                if (listBox.SelectedIndex >= 0 && listBox.SelectedIndex < vieModel.CurrentDatabases.Count) {
                    database = vieModel.CurrentDatabases[listBox.SelectedIndex];
                    id = database.DBId;
                    ConfigManager.Settings.DefaultDBID = id;
                } else {
                    return;
                }
            } else {
                // 默认打开上一次的库
                id = ConfigManager.Settings.DefaultDBID;

                if (appDatabases != null || appDatabases.Count > 0)
                    database = appDatabases.Where(arg => arg.DBId == id).FirstOrDefault();
            }

            Main main = GetWindowByName("Main", App.Current.Windows) as Main;
            // 检测该 id 是否在数据库中存在
            if (database == null) {
                MessageNotify.Error(LangManager.GetValueByKey("CancelOpenDefault"));
                ConfigManager.Settings.OpenDataBaseDefault = false;
                vieModel.Loading = false;
                tabControl.SelectedIndex = 1;
                return;
            } else {
                vieModel.Loading = false;

                // 次数+1
                appDatabaseMapper.IncreaseFieldById("ViewCount", id);
                ConfigManager.Main.CurrentDBId = id;

                // 是否需要扫描
                if (main == null &&
                    Main.CurrentDataType == DataType.Video &&
                    ConfigManager.ScanConfig.ScanOnStartUp) {
                    // 未打开过 main 的时候才会扫描
                    if (!string.IsNullOrEmpty(database.ScanPath)) {
                        tabControl.SelectedIndex = 0;
                        List<string> toScan = JsonUtils.TryDeserializeObject<List<string>>(database.ScanPath);
                        try {
                            ScanTask = new ScanTask(toScan, null, ScanTask.VIDEO_EXTENSIONS_LIST);
                            ScanTask.onScanning += (s, ev) => {
                                Dispatcher.Invoke(() => {
                                    vieModel.LoadingText = (ev as MessageCallBackEventArgs).Message;
                                });
                            };
                            ScanTask.Start();
                            while (ScanTask.Running) {
                                await Task.Delay(100);
                                if (CancelScanTask)
                                    break;
                            }
                        } catch (Exception ex) {
                            Logger.Error(ex);
                            MsgBox.Show(ex.Message);
                        }
                    } else {
                        tabControl.SelectedIndex = 1;
                    }
                }
            }

            TagStamp.Init(); // 初始化标签戳

            // 启动主窗口

            if (main == null) {
                main = new Main();
                Application.Current.MainWindow = main;
            } else {
                main.InitDataBases();
                main.SetComboboxID();
                main.LoadAll();
            }

            main.Show();
            if (ScanTask != null)
                App.ScanManager.CurrentTasks.Add(ScanTask);

            // 设置当前状态为：进入库
            EnteringDataBase = true;
            this.Close();
        }

        private void SetImage(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel.CurrentDatabases.Count || listBox.SelectedIndex < 0) {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase info = vieModel.CurrentDatabases[listBox.SelectedIndex];

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFile");
            dialog.Filter = "(jpg;jpeg;png)|*.jpg;*.jpeg;*.png";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string filename = dialog.FileName;
                SetImage(info.DBId, filename);
            }
        }

        private void SetImage(long id, string imagePath)
        {
            if (id <= 0 || !File.Exists(imagePath))
                return;

            // 复制到 ProjectImagePath 下
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string ext = Path.GetExtension(imagePath).ToLower();
            string newName = $"{id}_{name}{ext}";
            string newPath = Path.Combine(PathManager.ProjectImagePath, newName);
            FileHelper.TryCopyFile(imagePath, newPath);

            AppDatabase app1 = vieModel.Databases.Where(x => x.DBId == id).SingleOrDefault();
            AppDatabase app2 = vieModel.CurrentDatabases.Where(x => x.DBId == id).SingleOrDefault();
            if (app1 != null)
                app1.ImagePath = newPath;
            if (app2 != null)
                app2.ImagePath = newPath;
            appDatabaseMapper.UpdateFieldById("ImagePath", newName, id);
        }

        private void LoadDataBase(object sender, RoutedEventArgs e)
        {
            LoadDataBase();
        }

        private void ShowHideDataBase(object sender, RoutedEventArgs e)
        {
            vieModel.ReadFromDataBase();
        }

        private void HideDataBase(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel.CurrentDatabases.Count || listBox.SelectedIndex < 0) {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase info = vieModel.CurrentDatabases[listBox.SelectedIndex];
            if (info == null)
                return;
            info.Hide = info.Hide == 0 ? 1 : 0;
            appDatabaseMapper.UpdateById(info);
            vieModel.ReadFromDataBase();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadDataBase();
        }

        private void CancelScan(object sender, RoutedEventArgs e)
        {
            ScanTask?.Cancel();
            CancelScanTask = true;
            tabControl.SelectedIndex = 1;
        }

        private async void RestoreDatabase(object sender, RoutedEventArgs e)
        {
            if (new MsgBox(LangManager.GetValueByKey("IsToRestore")).ShowDialog(this) == true) {
                vieModel.Restoring = true;

                // todo 多数据库
                MapperManager.Dispose();
                await Task.Delay(2000);
                bool success = FileHelper.TryDeleteFile(SqlManager.DEFAULT_SQLITE_PATH, (error) => {
                    MessageCard.Error(error.Message);
                });
                if (success) {

                    MapperManager.ResetInitState();
                    bool loaded = MapperManager.Init();    // 初始化数据库连接
                    await Task.Delay(3000);
                    if (!loaded) {
                        MsgBox.Show($"{LangManager.GetValueByKey("LibraryInitFailed")}");
                        App.Current.Shutdown();
                    }

                    MessageNotify.Success(LangManager.GetValueByKey("Success"));
                }
                vieModel.Restoring = false;
                vieModel.ReadFromDataBase();
            }
        }

        private void ShowUpgradeWindow(object sender, RoutedEventArgs e)
        {
            UpgradeHelper.OpenWindow();
        }



        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            App.ShowAbout();
        }

        private void RefreshDatabase(object sender, RoutedEventArgs e)
        {
            RefreshDatabase();
        }

        private void Window_StartUp_ContentRendered(object sender, EventArgs e)
        {
            //new Window_Progress().ShowDialog();
        }
    }
}
