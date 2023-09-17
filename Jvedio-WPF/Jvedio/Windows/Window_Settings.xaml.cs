using Jvedio.Core.Config;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using Jvedio.Mapper;
using Jvedio.ViewModel;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperControls.Style.Plugin;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Entity;
using SuperUtils.Systems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.Core.Global.UrlManager;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Settings : SuperControls.Style.BaseWindow
    {

        private const string DEFAULT_TEST_URL = "https://www.example.com/";

        #region "事件"


        #endregion

        #region "静态属性"

        private List<string> RenameList { get; set; } = new List<string>();

        public static Video SampleVideo { get; set; }

        public static string SupportVideoFormat { get; set; }

        public static string SupportPictureFormat { get; set; } // bmp,gif,ico,jpe,jpeg,jpg,png

        #endregion

        #region "属性"
        public VieModel_Settings vieModel { get; set; }
        private Main MainWindow { get; set; }
        private int CurrentRowIndex { get; set; }

        private CrawlerServer currentCrawlerServer { get; set; }

        private bool IndexCanceled { get; set; } = false;


        #endregion


        #region "热键"
        public const int HOTKEY_ID = 2415;
        public static uint VK { get; set; }
        public static IntPtr WindowHandle { get; set; }
        public static HwndSource HSource { get; set; }

        /// <summary>
        /// 功能键 [1,3] 个
        /// </summary>
        public static List<Key> FuncKeys { get; set; } = new List<Key>();

        /// <summary>
        /// 基础键 1 个
        /// </summary>
        public static Key BasicKey { get; set; } = Key.None;
        public static List<Key> _FuncKeys { get; set; } = new List<Key>();
        public static Key _BasicKey { get; set; } = Key.None;

        public enum Modifiers
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008,
        }

        public static bool IsProperFuncKey(List<Key> keyList)
        {
            bool result = true;
            List<Key> keys = new List<Key>() { Key.LeftCtrl, Key.LeftAlt, Key.LeftShift };

            foreach (Key item in keyList) {
                if (!keys.Contains(item)) {
                    result = false;
                    break;
                }
            }

            return result;
        }

        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift) {
                if (!FuncKeys.Contains(currentKey))
                    FuncKeys.Add(currentKey);
            } else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9)) {
                BasicKey = currentKey;
            } else {
                // Console.WriteLine("不支持");
            }

            string singleKey = BasicKey.ToString();
            if (BasicKey.ToString().Length > 1) {
                singleKey = singleKey.ToString().Replace("D", string.Empty);
            }

            if (FuncKeys.Count > 0) {
                if (BasicKey == Key.None) {
                    hotkeyTextBox.Text = string.Join("+", FuncKeys);
                    _FuncKeys = new List<Key>();
                    _FuncKeys.AddRange(FuncKeys);
                    _BasicKey = Key.None;
                } else {
                    hotkeyTextBox.Text = string.Join("+", FuncKeys) + "+" + singleKey;
                    _FuncKeys = new List<Key>();
                    _FuncKeys.AddRange(FuncKeys);
                    _BasicKey = BasicKey;
                }
            } else {
                if (BasicKey != Key.None) {
                    hotkeyTextBox.Text = singleKey;
                    _FuncKeys = new List<Key>();
                    _BasicKey = BasicKey;
                }
            }
        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift) {
                if (FuncKeys.Contains(currentKey))
                    FuncKeys.Remove(currentKey);
            } else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12)) {
                if (currentKey == BasicKey) {
                    BasicKey = Key.None;
                }
            }
        }

        private void ApplyHotKey(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _FuncKeys.Contains(Key.LeftAlt) | _FuncKeys.Contains(Key.LeftCtrl) | _FuncKeys.Contains(Key.LeftShift) | _FuncKeys.Contains(Key.CapsLock);

            if (!containsFunKey | _BasicKey == Key.None) {
                SuperControls.Style.MessageCard.Error(LangManager.GetValueByKey("HotKeyWarning"));
            } else {
                // 注册热键
                if (_BasicKey != Key.None & IsProperFuncKey(_FuncKeys)) {
                    uint fsModifiers = (uint)Modifiers.None;
                    foreach (Key key in _FuncKeys) {
                        if (key == Key.LeftCtrl)
                            fsModifiers = fsModifiers | (uint)Modifiers.Control;
                        if (key == Key.LeftAlt)
                            fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                        if (key == Key.LeftShift)
                            fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                    }

                    VK = (uint)KeyInterop.VirtualKeyFromKey(_BasicKey);

                    Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
                    bool success = Win32Helper.RegisterHotKey(WindowHandle, HOTKEY_ID, fsModifiers, VK);
                    if (!success) {
                        new MsgBox(LangManager.GetValueByKey("HotKeyConflict")).ShowDialog(this);
                    }

                    {
                        // 保存设置
                        ConfigManager.Settings.HotKeyModifiers = fsModifiers;
                        ConfigManager.Settings.HotKeyVK = VK;
                        ConfigManager.Settings.HotKeyEnable = true;
                        ConfigManager.Settings.HotKeyString = hotkeyTextBox.Text;
                        ConfigManager.Settings.Save();
                        MessageNotify.Success(LangManager.GetValueByKey("HotKeySetSuccess"));
                    }
                }
            }
        }


        private void Unregister_HotKey(object sender, RoutedEventArgs e)
        {
            Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
        }


        #endregion

        static Window_Settings()
        {
            SampleVideo = new Video() {
                VID = "IRONMAN-01",
                Title = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Title"),
                VideoType = VideoType.Normal,
                ReleaseDate = "2020-01-01",
                Director = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Director"),
                Genre = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Genre"),
                Series = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Tag"),
                ActorNames = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Actor"),
                Studio = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Studio"),
                Rating = 9.0f,
                Label = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Label"),
                ReleaseYear = 2020,
                Duration = 126,
                Country = SuperControls.Style.LangManager.GetValueByKey("SampleMovie_Country"),
            };

            SupportVideoFormat =
                $"{SuperControls.Style.LangManager.GetValueByKey("NormalVideo")}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{SuperControls.Style.LangManager.GetValueByKey("OtherVedio")}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{SuperControls.Style.LangManager.GetValueByKey("AllFile")} (*.*)|*.*";
            SupportPictureFormat = $"图片(*.bmp, *.jpe, *.jpeg, *.jpg, *.png)|*.bmp;*.jpe;*.jpeg;*.jpg;*.png";
        }

        public Window_Settings()
        {
            InitializeComponent();

            vieModel = new VieModel_Settings();
            this.DataContext = vieModel;

            Init();

        }

        public void Init()
        {
            MainWindow = GetWindowByName("Main", App.Current.Windows) as Main;

            // 绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList()) {
                item.Click += AddToRename;
            }
            vieModel.MainWindowVisible = MainWindow != null;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitIndex();
            InitScanDatabases();
            InitViewRename(ConfigManager.RenameConfig.FormatString);
            InitCheckedBoxChecked();
            InitRenameCombobox();
            InitProxy();
            InitLang();
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        private void InitLang()
        {
            int langIdx = 0;
            if (!string.IsNullOrEmpty(ConfigManager.Settings.CurrentLanguage)) {
                for (int i = 0; i < langComboBox.Items.Count; i++) {
                    ComboBoxItem item = langComboBox.Items[i] as ComboBoxItem;
                    if (item.Tag.ToString().Equals(ConfigManager.Settings.CurrentLanguage)) {
                        langIdx = i;
                        break;
                    }
                }
            }
            langComboBox.SelectedIndex = langIdx;
            langComboBox.SelectionChanged += (s, ev) => {
                if (ev.AddedItems?.Count > 0) {
                    ComboBoxItem comboBoxItem = ev.AddedItems[0] as ComboBoxItem;
                    string lang = comboBoxItem.Tag.ToString();
                    SuperControls.Style.LangManager.SetLang(lang);
                    Jvedio.Core.Lang.LangManager.SetLang(lang);
                    vieModel.CurrentLanguage = lang;
                }
            };
        }

        /// <summary>
        /// 设置代理选中
        /// </summary>
        private void InitProxy()
        {
            List<RadioButton> proxies = proxyStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxies.Count; i++) {
                if (i == ConfigManager.ProxyConfig.ProxyMode)
                    proxies[i].IsChecked = true;
                int idx = i;
                proxies[i].Click += (s, ev) => {
                    ConfigManager.ProxyConfig.ProxyMode = idx;
                    MessageCard.Info(LangManager.GetValueByKey("RebootToTakeEffect"));
                };
            }

            List<RadioButton> proxyTypes = proxyTypesStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxyTypes.Count; i++) {
                if (i == ConfigManager.ProxyConfig.ProxyType)
                    proxyTypes[i].IsChecked = true;
                int idx = i;
                proxyTypes[i].Click += (s, ev) => {
                    ConfigManager.ProxyConfig.ProxyType = idx;
                };
            }

            // 设置代理密码
            passwordBox.Password = vieModel.ProxyPwd;

            passwordBox.PasswordChanged += (s, ev) => {
                if (!string.IsNullOrEmpty(passwordBox.Password))
                    vieModel.ProxyPwd = JvedioLib.Security.Encrypt.AesEncrypt(passwordBox.Password, 0);
            };
        }

        private void InitIndex()
        {
            // 初次启动后不给设置默认打开上一次库，否则会提示无此数据库
            if (ConfigManager.Settings.DefaultDBID <= 0)
                openDefaultCheckBox.IsEnabled = false;

            // 设置 crawlerIndex
            serverListBox.SelectedIndex = (int)ConfigManager.Settings.CrawlerSelectedIndex;

        }


        private void InitRenameCombobox()
        {
            foreach (ComboBoxItem item in OutComboBox.Items) {
                if (item.Content.ToString().Equals(ConfigManager.RenameConfig.OutSplit)) {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }

            if (OutComboBox.SelectedIndex < 0)
                OutComboBox.SelectedIndex = 0;

            foreach (ComboBoxItem item in InComboBox.Items) {
                if (item.Content.ToString().Equals(ConfigManager.RenameConfig.InSplit)) {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }

            if (InComboBox.SelectedIndex < 0)
                OutComboBox.SelectedIndex = 0;
        }

        public void AddPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (vieModel.ScanPath == null)
                    vieModel.ScanPath = new ObservableCollection<string>();
                if (!vieModel.ScanPath.Contains(path) && !vieModel.ScanPath.IsIntersectWith(path))
                    vieModel.ScanPath.Add(path);
                else
                    MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("FilePathIntersection"));
            }
        }


        public void DelPath(object sender, RoutedEventArgs e)
        {
            if (PathListBox.SelectedIndex >= 0) {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--) {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {
            vieModel.ScanPath?.Clear();
        }


        #region "文件监听"


        private FileSystemWatcher[] watchers { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool TestListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            watchers = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++) {
                try {
                    if (drives[i] == @"C:\") {
                        continue;
                    }

                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                    watchers[i] = watcher;
                    watcher.Dispose();
                } catch {
                    SuperControls.Style.MessageNotify.Error($"{SuperControls.Style.LangManager.GetValueByKey("NoPermissionToListen")} {drives[i]}");
                    return false;
                }
            }

            return true;
        }
        #endregion

        private void SetVideoPlayerPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("Choose");
            openFileDialog1.Filter = "exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string exePath = openFileDialog1.FileName;
                if (File.Exists(exePath))
                    ConfigManager.Settings.VideoPlayerPath = exePath;
            }
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            ApplySettings(null, null);
            this.Close();
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            // 保存扫描库
            ConfigManager.DownloadConfig.Save();

            if (DatabaseComboBox.ItemsSource != null && DatabaseComboBox.SelectedItem != null) {
                AppDatabase db = DatabaseComboBox.SelectedItem as AppDatabase;
                List<string> list = new List<string>();
                if (vieModel.ScanPath != null)
                    list = vieModel.ScanPath.ToList();
                db.ScanPath = JsonConvert.SerializeObject(list);
                MapperManager.appDatabaseMapper.UpdateById(db);
                int idx = DatabaseComboBox.SelectedIndex;

                List<AppDatabase> appDatabases = MainWindow?.vieModel.DataBases.ToList();
                if (appDatabases != null && idx < MainWindow.vieModel.DataBases.Count) {
                    MainWindow.vieModel.DataBases[idx].ScanPath = db.ScanPath;
                }
            }

            bool success = vieModel.SaveServers((msg) => {
                MessageCard.Error(msg);
            });
            if (success) {
                VideoParser.InitSearchPattern();
                SavePath();
                SaveSettings();

                SuperControls.Style.MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
            }
            UtilsManager.OnUtilSettingChange();


            SaveNFOParseValues();
        }


        /// <summary>
        /// 保存 NFO 解析
        /// </summary>
        public void SaveNFOParseValues()
        {
            vieModel.SaveNFOParseData();
        }

        private void SavePath()
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)vieModel.PicPaths[PathType.RelativeToData.ToString()];
            dict["BigImagePath"] = vieModel.BigImagePath;
            dict["SmallImagePath"] = vieModel.SmallImagePath;
            dict["PreviewImagePath"] = vieModel.PreviewImagePath;
            dict["ScreenShotPath"] = vieModel.ScreenShotPath;
            dict["ActorImagePath"] = vieModel.ActorImagePath;
            vieModel.PicPaths[PathType.RelativeToData.ToString()] = dict;
            ConfigManager.Settings.PicPathJson = JsonConvert.SerializeObject(vieModel.PicPaths);
        }

        private void SetFFMPEGPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFFmpeg");
            openFileDialog1.FileName = "ffmpeg.exe";
            openFileDialog1.Filter = "ffmpeg.exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string exePath = openFileDialog1.FileName;
                if (File.Exists(exePath)) {
                    if (new FileInfo(exePath).Name.ToLower() == "ffmpeg.exe")
                        vieModel.FFMPEG_Path = exePath;
                }
            }
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            AppDatabase db = e.AddedItems[0] as AppDatabase;
            vieModel.LoadScanPath(db);
        }


        /// <summary>
        /// 设置当前数据库
        /// </summary>
        private void InitScanDatabases()
        {
            List<AppDatabase> appDatabases = MainWindow?.vieModel.DataBases.ToList();
            AppDatabase db = MainWindow?.vieModel.CurrentAppDataBase;
            if (appDatabases != null) {
                DatabaseComboBox.ItemsSource = appDatabases;
                for (int i = 0; i < appDatabases.Count; i++) {
                    if (appDatabases[i].Equals(db)) {
                        DatabaseComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void InitCheckedBoxChecked()
        {
            List<ToggleButton> toggleButtons = CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList();
            List<string> list = toggleButtons.Select(arg => Video.ToSqlField(arg.Content.ToString())).ToList();

            // 按照顺序
            string formatString = ConfigManager.RenameConfig.FormatString;
            if (!string.IsNullOrEmpty(formatString)) {
                int left = formatString.IndexOf("{"), right = formatString.IndexOf("}");
                while (right > 0 && right < formatString.Length) {
                    string name = formatString.Substring(left + 1, right - left - 1);
                    if (list.Contains(name)) {
                        RenameList.Add(name);
                        toggleButtons[list.IndexOf(name)].IsChecked = true;
                    }
                    left = formatString.IndexOf("{", left + 1);
                    right = formatString.IndexOf("}", right + 1);
                    Console.WriteLine();
                }
            }
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        // 检视
        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            if (vieModel.ScanPath == null)
                vieModel.ScanPath = new ObservableCollection<string>();
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles) {
                if (!FileHelper.IsFile(item)) {
                    if (!vieModel.ScanPath.Contains(item) && !vieModel.ScanPath.IsIntersectWith(item))
                        vieModel.ScanPath.Add(item);
                    else
                        MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("FilePathIntersection"));
                }
            }
        }

        private void SelectNfoPath(object sender, RoutedEventArgs e)
        {
            // 选择NFO存放位置
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (!path.EndsWith("\\"))
                    path = path + "\\";
                vieModel.NFOSavePath = path;
            } else {
                MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_CanNotBeNull"));
            }
        }

        private void NewServer(object sender, RoutedEventArgs e)
        {
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            CrawlerServer server = new CrawlerServer() {
                PluginID = pluginID,
                Enabled = true,
                Url = DEFAULT_TEST_URL,
                Cookies = string.Empty,
                Available = 0,
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            if (list == null)
                list = new ObservableCollection<CrawlerServer>();
            list.Add(server);
            vieModel.CrawlerServers[pluginID] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }

        private string GetPluginID()
        {
            int idx = serverListBox.SelectedIndex;
            if (idx < 0 || vieModel.CrawlerServers?.Count == 0)
                return null;
            return vieModel.CrawlerServers.Keys.ToList()[idx];
        }


        private void TestServer(object sender, RoutedEventArgs e)
        {
            int idx = CurrentRowIndex;
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            CrawlerServer server = list[idx];

            if (!server.IsHeaderProper()) {
                MessageNotify.Error(LangManager.GetValueByKey("HeaderNotProper"));
                return;
            }

            server.Available = 2;
            ServersDataGrid.IsEnabled = false;
            CheckUrl(server, (s) => {
                ServersDataGrid.IsEnabled = true;
                list[idx].LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            });
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {
            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            Console.WriteLine(CurrentRowIndex);
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[pluginID];
            list.RemoveAt(CurrentRowIndex);
            vieModel.CrawlerServers[pluginID] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }

        private void SetCurrentRowIndex(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null) {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }

            if (dgr == null) {
                return;
            }

            CurrentRowIndex = dgr.GetIndex();
        }

        private async void CheckUrl(CrawlerServer server, Action<int> callback)
        {
            // library 需要保证 Cookies 和 UserAgent完全一致
            RequestHeader header = CrawlerServer.ParseHeader(server);
            try {
                string title = await HttpHelper.AsyncGetWebTitle(server.Url, header);
                if (string.IsNullOrEmpty(title)) {
                    server.Available = -1;
                } else {
                    server.Available = 1;
                }

                await Dispatcher.BeginInvoke((Action)delegate {
                    ServersDataGrid.Items.Refresh();
                    if (!string.IsNullOrEmpty(title))
                        MessageCard.Success(title);
                });
                callback.Invoke(0);
            } catch (WebException ex) {
                MessageCard.Error(ex.Message);
                server.Available = -1;
                await Dispatcher.BeginInvoke((Action)delegate {
                    ServersDataGrid.Items.Refresh();
                });
                callback.Invoke(0);
            }
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual

        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                child = v as T;

                if (child == null) {
                    child = GetVisualChild<T>

                    (v);
                }

                if (child != null) {
                    break;
                }
            }

            return child;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // 注册热键
            uint modifier = (uint)ConfigManager.Settings.HotKeyModifiers;
            uint vk = (uint)ConfigManager.Settings.HotKeyVK;

            if (modifier != 0 && vk != 0) {
                Win32Helper.UnregisterHotKey(WindowHandle, HOTKEY_ID); // 取消之前的热键
                bool success = Win32Helper.RegisterHotKey(WindowHandle, HOTKEY_ID, modifier, vk);
                if (!success) {
                    SuperControls.Style.MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("BossKeyError"));
                    ConfigManager.Settings.HotKeyEnable = false;
                }
            }
        }

        private void ReplaceWithValue(string property)
        {
            string inSplit = ConfigManager.RenameConfig.InSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.InSplit;
            PropertyInfo[] propertyList = SampleVideo.GetType().GetProperties();
            foreach (PropertyInfo item in propertyList) {
                string name = item.Name;
                if (name == property) {
                    object o = item.GetValue(SampleVideo);
                    if (o != null) {
                        string value = o.ToString();

                        if (property == "ActorNames" || property == "Genre" || property == "Label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (vieModel.RemoveTitleSpace && property.Equals("Title"))
                            value = value.Trim();

                        if (property == "VideoType") {
                            int v = 0;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = SuperControls.Style.LangManager.GetValueByKey("Uncensored");
                            else if (v == 2)
                                value = SuperControls.Style.LangManager.GetValueByKey("Censored");
                            else if (v == 3)
                                value = SuperControls.Style.LangManager.GetValueByKey("Europe");
                        }

                        vieModel.ViewRenameFormat = vieModel.ViewRenameFormat.Replace("{" + property + "}", value);
                    }

                    break;
                }
            }
        }



        private void SetRenameFormat()
        {
            string format = vieModel.FormatString;
            if (RenameList.Count > 0) {

                StringBuilder builder = new StringBuilder();
                string sep = ConfigManager.RenameConfig.OutSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.OutSplit;
                List<string> formatNames = new List<string>();
                foreach (string name in RenameList) {
                    formatNames.Add($"{{{name}}}");
                }
                vieModel.FormatString = string.Join(sep, formatNames);
            } else
                vieModel.FormatString = string.Empty;
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton != null) {
                string sep = ConfigManager.RenameConfig.OutSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.OutSplit;
                string format = vieModel.FormatString;
                string value = Video.ToSqlField(toggleButton.Content.ToString());
                if ((bool)toggleButton.IsChecked) {

                    if (format.IndexOf($"{{{value}}}") < 0) {
                        // 加到最后
                        if (format.Length > 0 && !string.IsNullOrEmpty(sep) && !format[format.Length - 1].Equals(sep.ToCharArray()[0]))
                            format += sep;
                        format += $"{{{value}}}";
                        RenameList.Add(value);
                    }
                } else {
                    // 移除所有
                    format = format.Replace($"{sep}{{{value}}}", "");
                    format = format.Replace($"{{{value}}}", "");
                    RenameList.Remove(value);
                }
                vieModel.FormatString = format;
            }
            SetRenameFormat();
            InitViewRename(vieModel.FormatString);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel == null)
                return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            InitViewRename(txt);
        }

        private void InitViewRename(string txt)
        {
            if (string.IsNullOrEmpty(txt)) {
                vieModel.ViewRenameFormat = string.Empty;
                return;
            }

            MatchCollection matches = Regex.Matches(txt, "\\{[a-zA-Z]+\\}");
            if (matches != null && matches.Count > 0) {
                vieModel.ViewRenameFormat = txt;
                foreach (Match match in matches) {
                    string property = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);
                    ReplaceWithValue(property);
                }
            } else {
                vieModel.ViewRenameFormat = string.Empty;
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ConfigManager.RenameConfig.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ConfigManager.RenameConfig.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
            InitViewRename(vieModel.FormatString);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
            ConfigManager.Settings.Save();
            ConfigManager.ProxyConfig.Save();
            ConfigManager.ScanConfig.Save();
            ConfigManager.FFmpegConfig.Save();
            ConfigManager.RenameConfig.Save();
        }

        private void SaveSettings()
        {
            ConfigManager.Main.ShowSearchHistory = vieModel.ShowSearchHistory;

            ConfigManager.Settings.TabControlSelectedIndex = vieModel.TabControlSelectedIndex;
            ConfigManager.Settings.OpenDataBaseDefault = vieModel.OpenDataBaseDefault;
            ConfigManager.Settings.AutoGenScreenShot = vieModel.AutoGenScreenShot;
            ConfigManager.Settings.CloseToTaskBar = vieModel.CloseToTaskBar;
            ConfigManager.Settings.CurrentLanguage = vieModel.CurrentLanguage;
            ConfigManager.Settings.SaveInfoToNFO = vieModel.SaveInfoToNFO;
            ConfigManager.Settings.NFOSavePath = vieModel.NFOSavePath;
            ConfigManager.Settings.OverwriteNFO = vieModel.OverwriteNFO;
            ConfigManager.Settings.AutoHandleHeader = vieModel.AutoHandleHeader;

            ConfigManager.Settings.PicPathMode = vieModel.PicPathMode;
            ConfigManager.Settings.SkipExistImage = vieModel.SkipExistImage;
            ConfigManager.Settings.DownloadWhenTitleNull = vieModel.DownloadWhenTitleNull;
            ConfigManager.Settings.IgnoreCertVal = vieModel.IgnoreCertVal;
            ConfigManager.Settings.AutoBackup = vieModel.AutoBackup;
            ConfigManager.Settings.AutoBackupPeriodIndex = vieModel.AutoBackupPeriodIndex;

            // 代理
            ConfigManager.ProxyConfig.Server = vieModel.ProxyServer;
            ConfigManager.ProxyConfig.Port = vieModel.ProxyPort;
            ConfigManager.ProxyConfig.UserName = vieModel.ProxyUserName;
            ConfigManager.ProxyConfig.Password = vieModel.ProxyPwd;
            ConfigManager.ProxyConfig.HttpTimeout = vieModel.HttpTimeout;

            // 扫描
            ConfigManager.ScanConfig.MinFileSize = vieModel.MinFileSize;
            ConfigManager.ScanConfig.FetchVID = vieModel.FetchVID;
            ConfigManager.ScanConfig.LoadDataAfterScan = vieModel.LoadDataAfterScan;
            ConfigManager.ScanConfig.DataExistsIndexAfterScan = vieModel.DataExistsIndexAfterScan;
            ConfigManager.ScanConfig.ImageExistsIndexAfterScan = vieModel.ImageExistsIndexAfterScan;
            ConfigManager.ScanConfig.ScanOnStartUp = vieModel.ScanOnStartUp;
            ConfigManager.ScanConfig.CopyNFOOverwriteImage = vieModel.CopyNFOOverwriteImage;
            ConfigManager.ScanConfig.CopyNFOPicture = vieModel.CopyNFOPicture;
            ConfigManager.ScanConfig.CopyNFOActorPicture = vieModel.CopyNFOActorPicture;
            ConfigManager.ScanConfig.CopyNFOPreview = vieModel.CopyNFOPreview;
            ConfigManager.ScanConfig.CopyNFOScreenShot = vieModel.CopyNFOScreenShot;
            ConfigManager.ScanConfig.CopyNFOActorPath = vieModel.CopyNFOActorPath;
            ConfigManager.ScanConfig.CopyNFOPreviewPath = vieModel.CopyNFOPreviewPath;
            ConfigManager.ScanConfig.CopyNFOScreenShotPath = vieModel.CopyNFOScreenShotPath;

            // ffmpeg
            ConfigManager.FFmpegConfig.Path = vieModel.FFMPEG_Path;
            ConfigManager.FFmpegConfig.ThreadNum = vieModel.ScreenShot_ThreadNum;
            ConfigManager.FFmpegConfig.TimeOut = vieModel.ScreenShot_TimeOut;
            ConfigManager.FFmpegConfig.ScreenShotNum = vieModel.ScreenShotNum;
            ConfigManager.FFmpegConfig.ScreenShotIgnoreStart = vieModel.ScreenShotIgnoreStart;
            ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd = vieModel.ScreenShotIgnoreEnd;
            ConfigManager.FFmpegConfig.SkipExistGif = vieModel.SkipExistGif;
            ConfigManager.FFmpegConfig.SkipExistScreenShot = vieModel.SkipExistScreenShot;
            ConfigManager.FFmpegConfig.ScreenShotAfterImport = vieModel.ScreenShotAfterImport;
            ConfigManager.FFmpegConfig.GifAutoHeight = vieModel.GifAutoHeight;
            ConfigManager.FFmpegConfig.GifWidth = vieModel.GifWidth;
            ConfigManager.FFmpegConfig.GifHeight = vieModel.GifHeight;
            ConfigManager.FFmpegConfig.GifDuration = vieModel.GifDuration;

            // 重命名
            ConfigManager.RenameConfig.AddRenameTag = vieModel.AddRenameTag;
            ConfigManager.RenameConfig.RemoveTitleSpace = vieModel.RemoveTitleSpace;
            ConfigManager.RenameConfig.FormatString = vieModel.FormatString;

            // 监听
            ConfigManager.Settings.ListenEnabled = vieModel.ListenEnabled;
            ConfigManager.Settings.ListenPort = vieModel.ListenPort;
        }

        private void CopyFFmpegUrl(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenUrl(FFMPEG_URL);
        }

        private void ImageSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ComboBox).SelectedIndex;
            if (idx >= 0 && vieModel != null && idx < VieModel_Settings.PIC_PATH_MODE_COUNT) {
                PathType type = (PathType)idx;
                if (type != PathType.RelativeToData)
                    vieModel.BasePicPath = vieModel.PicPaths[type.ToString()].ToString();
            }
        }

        private void url_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            setHeaderPopup.IsOpen = true;
            SearchBox searchBox = sender as SearchBox;
            string headers = searchBox.Text;
            if (!string.IsNullOrEmpty(headers)) {
                try {
                    Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(headers);
                    if (dict != null && dict.Count > 0) {
                        StringBuilder builder = new StringBuilder();
                        foreach (string key in dict.Keys) {
                            builder.Append($"{key}: {dict[key]}{Environment.NewLine}");
                        }

                        inputTextbox.Text = builder.ToString();
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            string pluginID = GetPluginID();
            if (string.IsNullOrEmpty(pluginID))
                return;
            currentCrawlerServer = vieModel.CrawlerServers[pluginID][ServersDataGrid.SelectedIndex];
        }

        private void CancelHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
        }

        private void ConfirmHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            if (currentCrawlerServer != null) {
                currentCrawlerServer.Headers = parsedTextbox.Text.Replace("{" + Environment.NewLine + "    ", "{")
                    .Replace(Environment.NewLine + "}", "}")
                    .Replace($"\",{Environment.NewLine}    \"", "\",\"");
                Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(currentCrawlerServer.Headers);

                if (dict != null && dict.ContainsKey("cookie"))
                    currentCrawlerServer.Cookies = dict["cookie"];
            }
        }

        private void InputHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = Parse((sender as TextBox).Text);
        }

        private string Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] array = text.Split(Environment.NewLine.ToCharArray());
            foreach (string item in array) {
                int idx = item.IndexOf(':');
                if (idx <= 0 || idx >= item.Length - 1)
                    continue;
                string key = item.Substring(0, idx).Trim().ToLower();
                string value = item.Substring(idx + 1).Trim();

                if (!data.ContainsKey(key))
                    data.Add(key, value);
            }

            // if (vieModel.AutoHandleHeader)
            // {
            data.Remove("content-encoding");
            data.Remove("accept-encoding");
            data.Remove("host");

            data = data.Where(arg => arg.Key.IndexOf(" ") < 0).ToDictionary(x => x.Key, y => y.Value);

            // }
            string json = JsonConvert.SerializeObject(data);
            if (json.Equals("{}"))
                return json;

            return json.Replace("{", "{" + Environment.NewLine + "    ")
                .Replace("}", Environment.NewLine + "}")
                .Replace("\",\"", $"\",{Environment.NewLine}    \"");
        }

        private async void TestProxy(object sender, RoutedEventArgs e)
        {
            vieModel.TestProxyStatus = TaskStatus.Running;
            SaveSettings();
            Button button = sender as Button;
            button.IsEnabled = false;
            string url = textProxyUrl.Text;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RequestHeader header = new RequestHeader();
            IWebProxy proxy = ConfigManager.ProxyConfig.GetWebProxy();
            header.TimeOut = ConfigManager.ProxyConfig.HttpTimeout * 1000; // 转为 ms
            header.WebProxy = proxy;
            string error = LangManager.GetValueByKey("Error");
            HttpResult httpResult = null;
            try {
                httpResult = await HttpClient.Get(url, header, SuperUtils.NetWork.Enums.HttpMode.String);
            } catch (TimeoutException ex) { error = ex.Message; } catch (Exception ex) { error = ex.Message; }
            if (httpResult != null) {
                if (httpResult.StatusCode == HttpStatusCode.OK) {
                    MessageCard.Success($"{LangManager.GetValueByKey("Success")} {LangManager.GetValueByKey("Delay")} {stopwatch.ElapsedMilliseconds} ms");
                    vieModel.TestProxyStatus = TaskStatus.RanToCompletion;
                    MessageCard.Info(LangManager.GetValueByKey("RebootToTakeEffect"));
                } else {
                    MessageCard.Error(httpResult.Error);
                    vieModel.TestProxyStatus = TaskStatus.Canceled;
                }
            } else {
                MessageCard.Error(error);
                vieModel.TestProxyStatus = TaskStatus.Canceled;
            }

            stopwatch.Stop();
            button.IsEnabled = true;
        }

        private void ShowHeaderHelp(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            FileHelper.TryOpenUrl(UrlManager.HEADER_HELP);
        }

        private async void CreatePlayableIndex(object sender, RoutedEventArgs e)
        {
            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() => {
                List<MetaData> metaDatas = MapperManager.metaDataMapper.SelectList();
                total = metaDatas.Count;
                if (total <= 0)
                    return false;
                StringBuilder builder = new StringBuilder();
                List<string> list = new List<string>();
                for (int i = 0; i < total; i++) {
                    MetaData metaData = metaDatas[i];
                    if (!File.Exists(metaData.Path))
                        builder.Append($"update metadata set PathExist=0 where DataID='{metaData.DataID}';");
                    if (IndexCanceled)
                        return false;
                    App.Current.Dispatcher.Invoke(() => {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }

                string sql = $"begin;update metadata set PathExist=1;{builder};commit;"; // 因为大多数资源都是存在的，默认先设为1
                MapperManager.videoMapper.ExecuteNonQuery(sql);
                return true;
            });
            ConfigManager.Settings.PlayableIndexCreated = true;
            vieModel.IndexCreating = false;
            if (result)
                MessageCard.Success($"{LangManager.GetValueByKey("CreateSuccess")} {total} {LangManager.GetValueByKey("DataIndex")}");
        }

        private async void CreatePictureIndex(object sender, RoutedEventArgs e)
        {
            if (new MsgBox($"{LangManager.GetValueByKey("CurrentImageType")} {((PathType)ConfigManager.Settings.PicPathMode).ToString()}，{LangManager.GetValueByKey("TakeEffectToCurrent")}")
                .ShowDialog() == false) {
                return;
            }

            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() => {
                string sql = VideoMapper.SQL_BASE;
                IWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Select("metadata.DataID", "Path", "VID", "Hash");
                sql = wrapper.ToSelect(false) + sql;
                List<Dictionary<string, object>> temp = MapperManager.metaDataMapper.Select(sql);
                List<Video> videos = MapperManager.metaDataMapper.ToEntity<Video>(temp, typeof(Video).GetProperties(), true);
                total = videos.Count;
                if (total <= 0)
                    return false;
                List<string> list = new List<string>();
                long pathType = ConfigManager.Settings.PicPathMode;
                for (int i = 0; i < total; i++) {
                    Video video = videos[i];

                    // 小图
                    list.Add($"({video.DataID},{pathType},0,{(File.Exists(video.GetSmallImage()) ? 1 : 0)})");

                    // 大图
                    list.Add($"({video.DataID},{pathType},1,{(File.Exists(video.GetBigImage()) ? 1 : 0)})");
                    if (IndexCanceled)
                        return false;

                    // todo 预览图的图片索引地址
                    //list.Add($"({video.DataID},{pathType},1,{(File.Exists(video.GetExtraImage()) ? 1 : 0)})");
                    //if (IndexCanceled)
                    //    return false;

                    // todo 影片截图的图片索引地址

                    App.Current.Dispatcher.Invoke(() => {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }

                string insertSql = $"begin;insert or replace into common_picture_exist(DataID,PathType,ImageType,Exist) values {string.Join(",", list)};commit;";
                MapperManager.videoMapper.ExecuteNonQuery(insertSql);
                return true;
            });
            if (result)
                MessageCard.Success($"{LangManager.GetValueByKey("CreateSuccess")} {total} {LangManager.GetValueByKey("DataIndex")}");
            ConfigManager.Settings.PictureIndexCreated = true;
            vieModel.IndexCreating = false;
        }

        private void CancelCreateIndex(object sender, RoutedEventArgs e)
        {
            IndexCanceled = true;
        }

        private void ViewSearchHistory(object sender, RoutedEventArgs e)
        {
        }

        private void PluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            if (idx < 0)
                return;
            if (vieModel.CrawlerServers?.Count > 0) {
                string pluginID = PluginType.Crawler.ToString().ToLower() + "/" + vieModel.DisplayCrawlerServers[idx];
                PluginMetaData pluginMetaData = CrawlerManager.PluginMetaDatas.Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
                if (vieModel.CrawlerServers.ContainsKey(pluginID)) {
                    ServersDataGrid.ItemsSource = null;
                    ServersDataGrid.ItemsSource = vieModel.CrawlerServers[pluginID];
                    vieModel.CurrentPlugin = pluginMetaData;
                    ConfigManager.Settings.CrawlerSelectedIndex = idx;
                    vieModel.ShowCurrentPlugin = true;
                }

            }
        }

        private void SavePluginSetting(object sender, RoutedEventArgs e)
        {
            // 保存插件启用情况
            vieModel.CurrentPlugin?.SaveConfig();
        }


        private void RestoreDefault(object sender, RoutedEventArgs e)
        {
            if ((bool)new MsgBox(LangManager.GetValueByKey("Restore") + "?").ShowDialog(this)) {

                ConfigManager.Restore();

                // 基本
                vieModel.OpenDataBaseDefault = false;
                vieModel.ScanOnStartUp = false;
                vieModel.CloseToTaskBar = false;

                ConfigManager.Settings.DelInfoAfterDelFile = true;
                ConfigManager.Settings.HotKeyEnable = false;
                ConfigManager.Settings.HotKeyString = "";
                langComboBox.SelectedIndex = 0;
                ConfigManager.Settings.VideoPlayerPath = "";

                // 图片
                vieModel.AutoGenScreenShot = true;

                ImageSelectComboBox.SelectedIndex = 0;
                vieModel.BasePicPath = Path.Combine(PathManager.CurrentUserFolder, "pic");

                // 扫描与导入
                vieModel.FetchVID = true;
                vieModel.LoadDataAfterScan = true;
                vieModel.MinFileSize = ScanConfig.DEFAULT_MIN_FILE_SIZE;
                vieModel.DataExistsIndexAfterScan = true;

                ConfigManager.ScanConfig.ScanNfo = false;
                ConfigManager.ScanConfig.Save();

                vieModel.CopyNFOOverwriteImage = false;
                vieModel.CopyNFOPicture = true;
                vieModel.CopyNFOActorPicture = true;
                vieModel.CopyNFOActorPath = ".actor";
                vieModel.CopyNFOPreview = true;
                vieModel.CopyNFOPreviewPath = ".preview";

                vieModel.CopyNFOScreenShot = true;
                vieModel.CopyNFOScreenShotPath = ".screenshot";

                // NFO 解析规则
                NfoParse.RestoreDefault();
                NfoParse.SaveData(NfoParse.CurrentNFOParse);
                vieModel.LoadNfoParseData();

                // 网络
                vieModel.IgnoreCertVal = true;
                vieModel.HttpTimeout = ProxyConfig.DEFAULT_TIMEOUT;
                vieModel.DownloadWhenTitleNull = true;
                vieModel.SkipExistImage = false;
                vieModel.SaveInfoToNFO = false;

                ConfigManager.DownloadConfig.DownloadThumbNail = true;
                ConfigManager.DownloadConfig.DownloadPoster = true;
                ConfigManager.DownloadConfig.DownloadPreviewImage = false;
                ConfigManager.DownloadConfig.DownloadActor = true;
                ConfigManager.DownloadConfig.OverrideInfo = false;
                ConfigManager.DownloadConfig.Save();

                ConfigManager.ProxyConfig.ProxyMode = (int)ProxyConfig.DEFAULT_PROXY_MODE;
                ConfigManager.ProxyConfig.ProxyType = (int)ProxyConfig.DEFAULT_PROXY_TYPE;
                ConfigManager.ProxyConfig.Save();

                // 显示
                ConfigManager.Main.DisplaySearchBox = true;
                ConfigManager.Main.DisplayPage = true;
                ConfigManager.Main.PaginationCombobox = true;
                ConfigManager.Main.DisplayStatusBar = true;
                ConfigManager.Main.DisplayFunBar = true;
                ConfigManager.Main.DisplayNavigation = true;
                ConfigManager.Main.DetailWindowShowAllMovie = true;
                ConfigManager.Main.ScrollSpeedFactor = 1.5;


                ConfigManager.VideoConfig.DisplayID = true;
                ConfigManager.VideoConfig.DisplayTitle = true;
                ConfigManager.VideoConfig.DisplayDate = true;
                ConfigManager.VideoConfig.DisplayStamp = true;
                ConfigManager.VideoConfig.DisplayFavorites = true;
                ConfigManager.VideoConfig.MainImageAutoMode = true;
                ConfigManager.VideoConfig.MovieOpacity = 1;

                ConfigManager.VideoConfig.ShowFileNameIfTitleEmpty = true;
                ConfigManager.VideoConfig.ShowCreateDateIfReleaseDateEmpty = true;

                // 视频处理
                vieModel.FFMPEG_Path = "";
                vieModel.ScreenShot_ThreadNum = FFmpegConfig.DEFAULT_THREAD_NUM;
                vieModel.SkipExistScreenShot = true;
                vieModel.ScreenShotAfterImport = true;
                vieModel.ScreenShotNum = FFmpegConfig.DEFAULT_SCREEN_SHOT_NUM;
                vieModel.ScreenShotIgnoreStart = FFmpegConfig.DEFAULT_SCREEN_SHOT_IGNORE_START;
                vieModel.ScreenShotIgnoreEnd = FFmpegConfig.DEFAULT_SCREEN_SHOT_IGNORE_END;
                vieModel.SkipExistGif = false;
                vieModel.GifWidth = FFmpegConfig.DEFAULT_GIF_WIDTH;
                vieModel.GifHeight = FFmpegConfig.DEFAULT_GIF_HEIGHT;
                vieModel.GifAutoHeight = true;
                vieModel.GifDuration = FFmpegConfig.DEFAULT_GIF_DURATION;

                // 重命名
                vieModel.RemoveTitleSpace = false;
                vieModel.AddRenameTag = false;
                ConfigManager.RenameConfig.OutSplit = RenameConfig.DEFAULT_OUT_SPLIT;
                ConfigManager.RenameConfig.InSplit = RenameConfig.DEFAULT_IN_SPLIT;
                vieModel.FormatString = "";
                ConfigManager.RenameConfig.Save();

                // 库
                vieModel.AutoBackup = true;
                vieModel.AutoBackupPeriodIndex = Jvedio.Core.WindowConfig.Settings.DEFAULT_BACKUP_PERIOD_INDEX;

                ConfigManager.Main.Save();
                ApplySettings(null, null);

            }
        }

        private void SetBasePicPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path)) {
                if (!path.EndsWith("\\"))
                    path += "\\";
                vieModel.BasePicPath = path;
            }
        }

        private void ShowCrawlerHelp(object sender, RoutedEventArgs e)
        {
            MessageCard.Info(LangManager.GetValueByKey("CrawlerServerHint"));
        }

        private void ClearCache(object sender, RoutedEventArgs e)
        {
            ImageCache.Clear();
            MessageNotify.Success(LangManager.GetValueByKey("Message_Success"));
        }
    }
}
