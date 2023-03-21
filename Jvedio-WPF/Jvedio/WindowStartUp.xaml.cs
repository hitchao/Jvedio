using Jvedio.Core;
using Jvedio.Core.Config;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.DataBase;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Core.Global;
using Jvedio.Core.Logs;
using Jvedio.Core.Plugins;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using Jvedio.Upgrade;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperControls.Style.Plugin.Themes;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.Time;
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
using static Jvedio.Core.Global.PathManager;
using static Jvedio.MapperManager;
using static Jvedio.VisualTools.WindowHelper;

namespace Jvedio
{
    public partial class WindowStartUp : SuperControls.Style.BaseWindow
    {
        private CancellationTokenSource cts;
        private CancellationToken ct;
        private VieModel_StartUp vieModel_StartUp;

        private ScanTask scanTask { get; set; }

        private bool EnteringDataBase { get; set; }

        private bool CancelScanTask { get; set; }

        private const int DEFAULT_TITLE_HEIGHT = 30;

        public WindowStartUp()
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth * 2 / 3;
            this.Height = SystemParameters.PrimaryScreenHeight * 2 / 3;

            cts = new CancellationTokenSource();
            cts.Token.Register(() => Console.WriteLine("取消任务"));
            ct = cts.Token;
            if (!Properties.Settings.Default.Debug)
            {
                FileHelper.TryDeleteFile("upgrade.bat");
                FileHelper.TryDeleteFile("upgrade-plugins.bat");
                FileHelper.TryDeleteDir("Temp");
            }

            FileHelper.TryDeleteDir(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "themes", "temp"));
        }

        // todo
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnsureSettings();           // 修复设置错误
            EnsureFileExists();         // 判断文件是否存在
            EnsureDirExists();          // 创建文件夹
            ConfigManager.Init();        // 初始化全局配置
            try
            {
                MapperManager.Init();    // 初始化数据库连接
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{LangManager.GetValueByKey("LibraryInitFailed")} {ex.Message}");
                App.Current.Shutdown();
            }

            ConfigManager.InitConfig(() =>
            {
                SetLang();
            });
            ConfigManager.EnsurePicPaths();

            await MoveOldFiles();           // 迁移旧文件并迁移到新数据库
            // if (GlobalFont != null)
            //    this.FontFamily = GlobalFont;
            InitAppData();      // 初始化应用数据
            DeleteLogs();       // 清理日志

            // GlobalConfig.PluginConfig.FetchPluginMetaData(); // 同步远程插件
            await BackupData(); // 备份文件
            await MovePlugins();
            await DeletePlugins();
            CrawlerManager.Init(true);   // 初始化爬虫

            vieModel_StartUp = new VieModel_StartUp();  // todo 检视
            this.DataContext = vieModel_StartUp;
            List<RadioButton> radioButtons = SidePanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < radioButtons.Count; i++)
            {
                if (i == vieModel_StartUp.CurrentSideIdx) radioButtons[i].IsChecked = true;
                else radioButtons[i].IsChecked = false;
            }
            UtilsManager.OnUtilSettingChange(); // 初始化 SuperUtils 的配置


#if DEBUG
            //ConfigManager.Main.FirstRun = true;
#endif

            if (ConfigManager.Main.FirstRun)
            {
                Dialog_SetSkinLang skinLang =
                    new Dialog_SetSkinLang(this);
                skinLang.SetThemeConfig(ConfigManager.ThemeConfig.ThemeIndex,
                    ConfigManager.ThemeConfig.ThemeID);
                skinLang.InitThemes();
                Dialog_SetSkinLang.OnLangChanged += (lang) =>
                {
                    Jvedio.Core.Lang.LangManager.SetLang(lang);
                    ConfigManager.Settings.CurrentLanguage = lang;
                    ConfigManager.Settings.Save();
                };
                Dialog_SetSkinLang.OnThemeChanged += (ThemeIdx, ThemeID) =>
                {
                    ConfigManager.ThemeConfig.ThemeIndex = ThemeIdx;
                    ConfigManager.ThemeConfig.ThemeID = ThemeID;
                    ConfigManager.ThemeConfig.Save();
                };

                skinLang.ShowDialog();
            }




            Main.CurrentDataType = (DataType)ConfigManager.StartUp.SideIdx;
            if (!Main.ClickGoBackToStartUp && ConfigManager.Settings.OpenDataBaseDefault)
            {
                tabControl.SelectedIndex = 0;
                LoadDataBase();
            }
            else
            {
                tabControl.SelectedIndex = 1;
                vieModel_StartUp.Loading = false;
                //this.TitleHeight = DEFAULT_TITLE_HEIGHT;
            }
        }




        private void SetLang()
        {
            // 设置语言
            if (!string.IsNullOrEmpty(ConfigManager.Settings.CurrentLanguage)
                && SuperControls.Style.LangManager.SupportLanguages.Contains(ConfigManager.Settings.CurrentLanguage))
            {
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
            if (list != null && list.Count > 0)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    string pluginId = list[i];
                    string[] paths = pluginId.Split(new char[] { '/', '\\' });
                    if (paths.Length <= 1)
                        continue;
                    string type = paths[0];
                    if (!type.EndsWith("s"))
                        type += "s";
                    paths[0] = type;
                    string targetPath = Path.GetFullPath(Path.Combine(PathManager.BasePluginsPath, paths.Aggregate(Path.Combine)));
                    if (Directory.Exists(targetPath))
                    {
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
            if (ConfigManager.Settings.AutoBackup)
            {
                int period = Jvedio.Core.WindowConfig.Settings.BackUpPeriods[(int)ConfigManager.Settings.AutoBackupPeriodIndex];
                bool backup = false;
                string[] arr = DirHelper.TryGetDirList(PathManager.BackupPath);
                if (arr != null && arr.Length > 0)
                {
                    string dirname = arr[arr.Length - 1];
                    if (Directory.Exists(dirname))
                    {
                        string dirName = Path.GetFileName(dirname);
                        DateTime before = DateTime.Now.AddDays(1);
                        DateTime now = DateTime.Now;
                        DateTime.TryParse(dirName, out before);
                        if (now.CompareTo(before) < 0 || (now - before).TotalDays > period)
                        {
                            backup = true;
                        }
                    }
                }
                else
                {
                    backup = true;
                }

                if (backup)
                {
                    string dir = Path.Combine(BackupPath, DateHelper.NowDate());
                    DirHelper.TryCreateDirectory(dir, (Action<Exception>)((err) =>
                    {
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
            if (success && files != null && files.Length > 0)
            {
                Jvedio4ToJvedio5.MoveRecentWatch();
                Jvedio4ToJvedio5.MoveMagnets();
                Jvedio4ToJvedio5.MoveTranslate();
                Jvedio4ToJvedio5.MoveMyList();      // 清单和 Label 合并，统一为 Label

                // Jvedio4ToJvedio5.MoveSearchHistory();
                Jvedio4ToJvedio5.MoveScanPathConfig(files);
                ConfigManager.Settings.OpenDataBaseDefault = false;
            }

            // 移动文件
            string targetDir = Path.Combine(AllOldDataPath, "DataBase");
            if (Directory.Exists(targetDir)) DirHelper.TryDelete(targetDir);
            DirHelper.TryMoveDir(oldDataPath, targetDir); // 移动 DataBase
            string[] moveFiles =
            {
                "SearchHistory", "ServersConfig", "RecentWatch",
                "AI.sqlite", "Magnets.sqlite", "Translate.sqlite", "mylist.sqlite",
            };

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
                ScanHelper.InitSearchPattern();

                // 读取配置文件，设置 debug
                ReadConfig();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void ReadConfig()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (!File.Exists(path)) return;
            string value = FileHelper.TryReadFile(path);
            foreach (var item in value.Split('\n'))
            {
                if (!string.IsNullOrEmpty(item) && item.IndexOf("=") > 0 && item.Length >= 3)
                {
                    string p = item.Split('=')[0];
                    string v = item.Split('=')[1].Replace("\r", string.Empty);
                    if ("debug".Equals(p) && "true".Equals(v.ToLower()))
                    {
                        Properties.Settings.Default.Debug = true;
                    }
                    else
                    {
                        Properties.Settings.Default.Debug = false;
                    }
                }
            }

            Console.WriteLine("Properties.Settings.Default.Debug = " + Properties.Settings.Default.Debug);
        }

        private void DeleteLogs()
        {
            try
            {
                // todo 清除日志
                ClearLogBefore(-10, "log");
                ClearLogBefore(-10, "log\\NetWork");
                ClearLogBefore(-10, "log\\scanlog");
                ClearLogBefore(-10, "log\\file");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                    DateTime.TryParse(file.Split('\\').Last().Replace(".log", string.Empty), out DateTime date);
                    if (date < dateTime) FileHelper.TryDeleteFile(file);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

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
                Logger.Error(ex);
            }

            try
            {
                // TODO
                // if (!Enum.IsDefined(typeof(Skin), Properties.Settings.Default.Themes))
                // {
                //    Properties.Settings.Default.Themes = Skin.黑色.ToString();
                //    Properties.Settings.Default.Save();
                // }

                // if (!Enum.IsDefined(typeof(MyLanguage), Properties.Settings.Default.Language))
                // {
                //    Properties.Settings.Default.Language = MyLanguage.中文.ToString();
                //    Properties.Settings.Default.Save();
                // }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                FileHelper.TryCreateDir(Path.Combine(PicPath, item));
            }
        }

        public void EnsureFileExists()
        {
            if (!File.Exists(@"x64\SQLite.Interop.dll") || !File.Exists(@"x86\SQLite.Interop.dll"))
            {
                MessageBox.Show($"{SuperControls.Style.LangManager.GetValueByKey("Missing")} SQLite.Interop.dll", "Jvedio");
                this.Close();
            }
        }

        private void DelSqlite(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel_StartUp.CurrentDatabases.Count || listBox.SelectedIndex < 0)
            {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase database = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            MsgBox msgbox = new MsgBox(this,
                $"{LangManager.GetValueByKey("IsToDelete")} {database.Name} {LangManager.GetValueByKey("And")} {database.Count} {LangManager.GetValueByKey("TagLabelAndOtherInfo")}");
            if (msgbox.ShowDialog() == true)
            {
                try
                {
                    database.deleteByID(database.DBId);
                    RefreshDatabase();
                    Main main = GetWindowByName("Main") as Main;
                    if (main != null)
                    {
                        // 重置当前
                        main.vieModel.Select();
                        main.vieModel.Statistic();
                    }
                }
                catch (SqlException ex)
                {
                    MessageCard.Error(ex.Message);
                }
            }
        }

        private void RenameSqlite(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel_StartUp.CurrentDatabases.Count || listBox.SelectedIndex < 0)
            {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            string originName = info.Name;
            DialogInput input = new DialogInput(this, SuperControls.Style.LangManager.GetValueByKey("Rename"), originName);
            if (input.ShowDialog() == false) return;
            string targetName = input.Text;
            if (string.IsNullOrEmpty(targetName)) return;
            if (targetName == originName) return;
            info.Name = targetName;
            appDatabaseMapper.UpdateById(info);

            // 仅更新重命名的
            vieModel_StartUp.refreshItem();
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
            SuperControls.Style.SearchBox textBox = sender as SuperControls.Style.SearchBox;
            if (textBox == null) return;
            vieModel_StartUp.CurrentSearch = textBox.Text;
            vieModel_StartUp.Search();
        }

        private void NewDatabase(object sender, RoutedEventArgs e)
        {
            vieModel_StartUp.CurrentSearch = string.Empty;
            vieModel_StartUp.Sort = true;
            vieModel_StartUp.SortType = LangManager.GetValueByKey("CreatedDate");
            DialogInput input = new DialogInput(this, SuperControls.Style.LangManager.GetValueByKey("NewLibrary"));
            if (input.ShowDialog() == false) return;
            string targetName = input.Text;
            if (string.IsNullOrEmpty(targetName)) return;

            // 创建新数据库
            AppDatabase appDatabase = new AppDatabase();
            appDatabase.Name = targetName;
            appDatabase.DataType = (DataType)vieModel_StartUp.CurrentSideIdx;
            appDatabaseMapper.Insert(appDatabase);
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

        // todo 检视
        private void ChangeDataType(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StackPanel stackPanel = radioButton.Parent as StackPanel;
            int idx = stackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            vieModel_StartUp.CurrentSideIdx = idx;
            Main.CurrentDataType = (DataType)idx;
            ConfigManager.StartUp.SideIdx = idx;
            vieModel_StartUp.ReadFromDataBase();
        }

        public async void LoadDataBase()
        {
            if (vieModel_StartUp.CurrentDatabases == null || vieModel_StartUp.CurrentDatabases.Count <= 0)
            {
                ConfigManager.Settings.OpenDataBaseDefault = false;
                vieModel_StartUp.Loading = false;
                tabControl.SelectedIndex = 1;
                //this.TitleHeight = DEFAULT_TITLE_HEIGHT;
                return;
            }

            List<AppDatabase> appDatabases = appDatabaseMapper.SelectList();

            // 加载数据库
            long id = ConfigManager.Main.CurrentDBId;
            AppDatabase database = null;
            if (Main.ClickGoBackToStartUp || !ConfigManager.Settings.OpenDataBaseDefault)
            {
                if (listBox.SelectedIndex >= 0 && listBox.SelectedIndex < vieModel_StartUp.CurrentDatabases.Count)
                {
                    database = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
                    id = database.DBId;
                    ConfigManager.Settings.DefaultDBID = id;
                }
                else
                {
                    return;
                }
            }
            else
            {
                // 默认打开上一次的库
                id = ConfigManager.Settings.DefaultDBID;

                if (appDatabases != null || appDatabases.Count > 0)
                    database = appDatabases.Where(arg => arg.DBId == id).FirstOrDefault();
            }

            Main main = GetWindowByName("Main") as Main;
            // 检测该 id 是否在数据库中存在
            if (database == null)
            {
                MessageNotify.Error(LangManager.GetValueByKey("CancelOpenDefault"));
                ConfigManager.Settings.OpenDataBaseDefault = false;
                vieModel_StartUp.Loading = false;
                tabControl.SelectedIndex = 1;
                //this.TitleHeight = DEFAULT_TITLE_HEIGHT;
                return;
            }
            else
            {
                vieModel_StartUp.Loading = false;

                // 次数+1
                appDatabaseMapper.IncreaseFieldById("ViewCount", id);

                ConfigManager.Main.CurrentDBId = id;

                // 是否需要扫描
                if (main == null && ConfigManager.ScanConfig.ScanOnStartUp)
                {
                    // 未打开过 main 的时候才会扫描
                    if (!string.IsNullOrEmpty(database.ScanPath))
                    {
                        tabControl.SelectedIndex = 0;
                        await Task.Delay(5000);
                        //this.TitleHeight = 0;
                        List<string> toScan = JsonUtils.TryDeserializeObject<List<string>>(database.ScanPath);
                        try
                        {
                            scanTask = new ScanTask(toScan, null, ScanTask.VIDEO_EXTENSIONS_LIST);
                            scanTask.onScanning += (s, ev) =>
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    vieModel_StartUp.LoadingText = (ev as MessageCallBackEventArgs).Message;
                                });
                            };
                            scanTask.Start();
                            while (scanTask.Running)
                            {
                                await Task.Delay(100);
                                if (CancelScanTask) break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            MessageBox.Show(ex.Message);
                        }

                        //this.TitleHeight = DEFAULT_TITLE_HEIGHT;
                    }
                    else
                    {
                        tabControl.SelectedIndex = 1;
                    }
                }
            }

            // 启动主窗口
            if (Main.CurrentDataType == DataType.Video)
            {

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
                if (scanTask != null)
                {
                    if (main.vieModel.ScanTasks == null)
                        main.vieModel.ScanTasks = new System.Collections.ObjectModel.ObservableCollection<ScanTask>();
                    main.vieModel.ScanTasks.Add(scanTask);
                }
            }
            else
            {
                //Window_MetaDatas metaData = GetWindowByName("Window_MetaDatas") as Window_MetaDatas;
                //if (metaData == null)
                //{
                //    metaData = new Window_MetaDatas();
                //    metaData.Title = "Jvedio-" + Main.CurrentDataType.ToString();
                //    Application.Current.MainWindow = metaData;
                //}
                //else
                //{
                //    metaData.setDataBases();
                //    metaData.setComboboxID();
                //}

                //metaData.Show();
            }

            // 设置当前状态为：进入库
            EnteringDataBase = true;
            this.Close();
        }

        private void SetImage(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel_StartUp.CurrentDatabases.Count || listBox.SelectedIndex < 0)
            {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFile");
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
            if (id <= 0 || !File.Exists(imagePath)) return;

            // 复制到 ProjectImagePath 下
            string name = Path.GetFileNameWithoutExtension(imagePath);
            string ext = Path.GetExtension(imagePath).ToLower();
            string newName = $"{id}_{name}{ext}";
            string newPath = Path.Combine(PathManager.ProjectImagePath, newName);
            FileHelper.TryCopyFile(imagePath, newPath);

            AppDatabase app1 = vieModel_StartUp.Databases.Where(x => x.DBId == id).SingleOrDefault();
            AppDatabase app2 = vieModel_StartUp.CurrentDatabases.Where(x => x.DBId == id).SingleOrDefault();
            if (app1 != null) app1.ImagePath = newPath;
            if (app2 != null) app2.ImagePath = newPath;
            appDatabaseMapper.UpdateFieldById("ImagePath", newName, id);
        }

        private void LoadDataBase(object sender, RoutedEventArgs e)
        {
            LoadDataBase();
        }

        private void ShowHideDataBase(object sender, RoutedEventArgs e)
        {
            vieModel_StartUp.ReadFromDataBase();
        }

        private void HideDataBase(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex >= vieModel_StartUp.CurrentDatabases.Count || listBox.SelectedIndex < 0)
            {
                MessageNotify.Error(LangManager.GetValueByKey("InnerError"));
                return;
            }

            AppDatabase info = vieModel_StartUp.CurrentDatabases[listBox.SelectedIndex];
            if (info == null) return;
            info.Hide = info.Hide == 0 ? 1 : 0;
            appDatabaseMapper.UpdateById(info);
            vieModel_StartUp.ReadFromDataBase();
        }

        private void Window_StartUp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.StartUp.Tile = vieModel_StartUp.Tile;
            ConfigManager.StartUp.ShowHideItem = vieModel_StartUp.ShowHideItem;
            ConfigManager.StartUp.SideIdx = vieModel_StartUp.CurrentSideIdx;

            ConfigManager.StartUp.Sort = vieModel_StartUp.Sort;
            ConfigManager.StartUp.SortType = string.IsNullOrEmpty(vieModel_StartUp.SortType) ? LangManager.GetValueByKey("Title") : vieModel_StartUp.SortType;
            ConfigManager.StartUp.Save();

            Main main = GetWindowByName("Main") as Main;
            if (main != null && !main.IsActive && !EnteringDataBase)
            {
                Application.Current.Shutdown();

                // todo 关闭 main
                // main.Close();
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadDataBase();
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

        private void CancelScan(object sender, RoutedEventArgs e)
        {
            scanTask?.Cancel();
            CancelScanTask = true;
            tabControl.SelectedIndex = 1;
            //this.TitleHeight = DEFAULT_TITLE_HEIGHT;
        }

        private async void RestoreDatabase(object sender, RoutedEventArgs e)
        {
            if (new MsgBox(this, LangManager.GetValueByKey("IsToRestore")).ShowDialog() == true)
            {
                vieModel_StartUp.Restoring = true;

                // todo 多数据库
                MapperManager.Dispose();
                await Task.Delay(2000);
                bool success = FileHelper.TryDeleteFile(SqlManager.DEFAULT_SQLITE_PATH, (error) =>
                {
                    MessageCard.Error(error.Message);
                });
                if (success)
                {
                    try
                    {
                        MapperManager.ResetInitState();
                        MapperManager.Init();    // 初始化数据库连接
                        await Task.Delay(3000);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{LangManager.GetValueByKey("LibraryInitFailed")} {ex.Message}");
                        App.Current.Shutdown();
                    }

                    MessageNotify.Success(LangManager.GetValueByKey("Success"));
                }
                vieModel_StartUp.Restoring = false;
                vieModel_StartUp.ReadFromDataBase();
            }
        }

        private void ShowUpgradeWindow(object sender, RoutedEventArgs e)
        {
            UpgradeHelper.OpenWindow();
        }

        private void Window_StartUp_ContentRendered(object sender, EventArgs e)
        {

        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            local = local.Substring(0, local.Length);
            System.Windows.Media.Imaging.BitmapImage bitmapImage = ImageHelper.ImageFromUri("pack://application:,,,/Resources/Picture/Jvedio.png");
            About about = new About(this, bitmapImage, "Jvedio",
                "超级本地视频管理软件", local, ConfigManager.RELEASE_DATE,
                "Github", UrlManager.ProjectUrl, "Chao", "GPL-3.0");
            about.OnOtherClick += (s, ev) =>
            {
                FileHelper.TryOpenUrl(UrlManager.WebPage);
            };
            about.ShowDialog();
        }
    }
}
