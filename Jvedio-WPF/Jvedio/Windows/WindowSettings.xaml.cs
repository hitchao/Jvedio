using ChaoControls.Style;
using FontAwesome.WPF;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Plugins;
using Jvedio.Core.SimpleMarkDown;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Style;
using Jvedio.Utils;

using Jvedio.Utils.FileProcess;
using Jvedio.ViewModel;
using JvedioLib.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Windows.Media;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : ChaoControls.Style.BaseWindow
    {
        private Main windowMain = GetWindowByName("Main") as Main;
        //public const string ffmpeg_url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";
        public const string ffmpeg_url = "https://www.gyan.dev/ffmpeg/builds/";
        public static string GrowlToken = "SettingsGrowl";
        public Video SampleVideo = new Video()
        {
            VID = "IRONMAN-01",
            Title = Jvedio.Language.Resources.SampleMovie_Title,
            VideoType = VideoType.Normal,
            ReleaseDate = "2020-01-01",
            Director = Jvedio.Language.Resources.SampleMovie_Director,
            Genre = Jvedio.Language.Resources.SampleMovie_Genre,
            Series = Jvedio.Language.Resources.SampleMovie_Tag,
            ActorNames = Jvedio.Language.Resources.SampleMovie_Actor,
            Studio = Jvedio.Language.Resources.SampleMovie_Studio,
            Rating = 9.0f,
            Label = Jvedio.Language.Resources.SampleMovie_Label,
            ReleaseYear = 2020,
            Duration = 126,

            Country = Jvedio.Language.Resources.SampleMovie_Country
        };
        public VieModel_Settings vieModel;
        public Settings()
        {
            InitializeComponent();
            if (GlobalFont != null) this.FontFamily = GlobalFont;
            vieModel = new VieModel_Settings();
            this.DataContext = vieModel;
            //绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                item.Click += AddToRename;
            }

            if (windowMain == null)
            {
                vieModel.MainWindowVisiblie = false;
            }
            else
            {
                vieModel.MainWindowVisiblie = true;
            }
        }



        #region "热键"





        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift)
            {
                if (!funcKeys.Contains(currentKey)) funcKeys.Add(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9))
            {
                key = currentKey;
            }
            else
            {
                //Console.WriteLine("不支持");
            }

            string singleKey = key.ToString();
            if (key.ToString().Length > 1)
            {
                singleKey = singleKey.ToString().Replace("D", "");
            }

            if (funcKeys.Count > 0)
            {
                if (key == Key.None)
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys);
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = Key.None;
                }
                else
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys) + "+" + singleKey;
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = key;
                }

            }
            else
            {
                if (key != Key.None)
                {
                    hotkeyTextBox.Text = singleKey;
                    _funcKeys = new List<Key>();
                    _key = key;
                }
            }




        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift)
            {
                if (funcKeys.Contains(currentKey)) funcKeys.Remove(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12))
            {
                if (currentKey == key)
                {
                    key = Key.None;
                }

            }


        }

        private void ApplyHotKey(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _funcKeys.Contains(Key.LeftAlt) | _funcKeys.Contains(Key.LeftCtrl) | _funcKeys.Contains(Key.LeftShift) | _funcKeys.Contains(Key.CapsLock);


            if (!containsFunKey | _key == Key.None)
            {
                ChaoControls.Style.MessageCard.Error("必须为 功能键 + 数字/字母");
            }
            else
            {
                //注册热键
                if (_key != Key.None & IsProperFuncKey(_funcKeys))
                {
                    uint fsModifiers = (uint)Modifiers.None;
                    foreach (Key key in _funcKeys)
                    {
                        if (key == Key.LeftCtrl) fsModifiers = fsModifiers | (uint)Modifiers.Control;
                        if (key == Key.LeftAlt) fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                        if (key == Key.LeftShift) fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                    }
                    VK = (uint)KeyInterop.VirtualKeyFromKey(_key);


                    UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                    bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, fsModifiers, VK);
                    if (!success) { MessageBox.Show("热键冲突！", "热键冲突"); }
                    {
                        //保存设置
                        Properties.Settings.Default.HotKey_Modifiers = fsModifiers;
                        Properties.Settings.Default.HotKey_VK = VK;
                        Properties.Settings.Default.HotKey_Enable = true;
                        Properties.Settings.Default.HotKey_String = hotkeyTextBox.Text;
                        Properties.Settings.Default.Save();
                        MessageCard.Success("设置热键成功");
                    }

                }



            }
        }

        #endregion





        public void AddPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (vieModel.ScanPath == null)
                    vieModel.ScanPath = new ObservableCollection<string>();
                if (!vieModel.ScanPath.Contains(path) && !vieModel.ScanPath.IsIntersectWith(path))
                    vieModel.ScanPath.Add(path);
                else

                    MessageCard.Error(Jvedio.Language.Resources.FilePathIntersection);
            }

        }

        public async void TestAI(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];
            if (checkBox.Content.ToString() == Jvedio.Language.Resources.BaiduFaceRecognition)
            {

                string base64 = GlobalVariable.AIBaseImage64;
                System.Drawing.Bitmap bitmap = ImageProcess.Base64ToBitmap(base64);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = await TestBaiduAI(bitmap);
                if (result != null && int32Rect != Int32Rect.Empty)
                {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));
                    string clientId = Properties.Settings.Default.Baidu_API_KEY.Replace(" ", "");
                    string clientSecret = Properties.Settings.Default.Baidu_SECRET_KEY.Replace(" ", "");
                    SaveKeyValue(clientId, clientSecret, "BaiduAI.key");
                }
                else
                {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public static Task<(Dictionary<string, string>, Int32Rect)> TestBaiduAI(System.Drawing.Bitmap bitmap)
        {
            return Task.Run(() =>
            {
                string token = AccessToken.getAccessToken();
                string FaceJson = FaceDetect.faceDetect(token, bitmap);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = FaceParse.Parse(FaceJson);
                return (result, int32Rect);
            });

        }

        public async void TestTranslate(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];

            if (checkBox.Content.ToString() == "百度翻译")
            {

            }
            else if (checkBox.Content.ToString() == Jvedio.Language.Resources.Youdao)
            {
                string result = await Translate.Youdao("のマ○コに");
                if (result != "")
                {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));

                    string Youdao_appKey = Properties.Settings.Default.TL_YOUDAO_APIKEY.Replace(" ", "");
                    string Youdao_appSecret = Properties.Settings.Default.TL_YOUDAO_SECRETKEY.Replace(" ", "");

                    //成功，保存在本地
                    SaveKeyValue(Youdao_appKey, Youdao_appSecret, "youdao.key");
                }
                else
                {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }


        }

        public void SaveKeyValue(string key, string value, string filename)
        {
            string v = Encrypt.AesEncrypt(key + " " + value, EncryptKeys[0]);
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, append: false))
                {
                    sw.Write(v);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }

        public void DelPath(object sender, RoutedEventArgs e)
        {
            if (PathListBox.SelectedIndex >= 0)
            {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }

        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {
            vieModel.ScanPath?.Clear();
        }





        private void Restore(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, Jvedio.Language.Resources.Message_IsToReset).ShowDialog() == true)
            {
                Properties.Settings.Default.Reset();
                GlobalConfig.Main.FirstRun = false;
                Properties.Settings.Default.Save();
            }

        }

        private void DisplayNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 500)
                {
                    Properties.Settings.Default.DisplayNumber = num;
                    Properties.Settings.Default.Save();
                }
            }

        }








        private void ListenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsVisible == false) return;
            if ((bool)checkBox.IsChecked)
            {
                //测试是否能监听
                if (!TestListen())
                    checkBox.IsChecked = false;
                else
                    ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.RebootToTakeEffect);
            }
        }


        FileSystemWatcher[] watchers;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool TestListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            watchers = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++)
            {
                try
                {

                    if (drives[i] == @"C:\") { continue; }
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                    watchers[i] = watcher;
                    watcher.Dispose();
                }
                catch
                {
                    ChaoControls.Style.MessageCard.Error($"{Jvedio.Language.Resources.NoPermissionToListen} {drives[i]}");
                    return false;
                }
            }
            return true;
        }

        private void SetVediaPlaterPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.Choose;
            OpenFileDialog1.Filter = "exe|*.exe";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = OpenFileDialog1.FileName;
                if (File.Exists(exePath))
                    Properties.Settings.Default.VedioPlayerPath = exePath;

            }
        }


        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            //if (Properties.Settings.Default.Opacity_Main >= 0.5)
            //    App.Current.Windows[0].Opacity = Properties.Settings.Default.Opacity_Main;
            //else
            //    App.Current.Windows[0].Opacity = 1;

            // 保存扫描库

            if (DatabaseComboBox.ItemsSource != null && DatabaseComboBox.SelectedItem != null)
            {
                AppDatabase db = DatabaseComboBox.SelectedItem as AppDatabase;
                List<string> list = new List<string>();
                if (vieModel.ScanPath != null) list = vieModel.ScanPath.ToList();
                db.ScanPath = JsonConvert.SerializeObject(list);
                GlobalMapper.appDatabaseMapper.updateById(db);
                int idx = DatabaseComboBox.SelectedIndex;

                List<AppDatabase> appDatabases = windowMain?.vieModel.DataBases.ToList();
                if (appDatabases != null && idx < windowMain.vieModel.DataBases.Count)
                {
                    windowMain.vieModel.DataBases[idx] = db;
                }
            }

            bool success = vieModel.SaveServers((msg) =>
             {
                 MessageCard.Error(msg);
             });
            if (success)
            {
                GlobalVariable.InitVariable();
                ScanHelper.InitSearchPattern();
                savePath();
                saveSettings();

                ChaoControls.Style.MessageCard.Success(Jvedio.Language.Resources.Message_Success);
            }





        }

        private void savePath()
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)vieModel.PicPaths[PathType.RelativeToData.ToString()];
            dict["BigImagePath"] = vieModel.BigImagePath;
            dict["SmallImagePath"] = vieModel.SmallImagePath;
            dict["PreviewImagePath"] = vieModel.PreviewImagePath;
            dict["ScreenShotPath"] = vieModel.ScreenShotPath;
            dict["ActorImagePath"] = vieModel.ActorImagePath;
            vieModel.PicPaths[PathType.RelativeToData.ToString()] = dict;
            GlobalConfig.Settings.PicPathJson = JsonConvert.SerializeObject(vieModel.PicPaths);

        }





        private void SetFFMPEGPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseFFmpeg;
            OpenFileDialog1.FileName = "ffmpeg.exe";
            OpenFileDialog1.Filter = "ffmpeg.exe|*.exe";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = OpenFileDialog1.FileName;
                if (File.Exists(exePath))
                {
                    if (new FileInfo(exePath).Name.ToLower() == "ffmpeg.exe")
                        vieModel.FFMPEG_Path = exePath;
                }
            }
        }

        private void SetSkin(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            OnSetSkin();
        }

        private void OnSetSkin()
        {
            Main main = GetWindowByName("Main") as Main;
            main.SetSkin();
            main?.SetSelected();
            main?.ActorSetSelected();
        }



        public void SetLanguage()
        {
            //https://blog.csdn.net/fenglailea/article/details/45888799

            long language = vieModel.SelectedLanguage;
            string hint = "";
            if (language == 1)
                hint = "Take effect after restart";
            else if (language == 2)
                hint = "再起動後に有効になります";
            else
                hint = "重启后生效";
            MessageCard.Success(hint);
            SetLanguageDictionary();
        }

        private void SetLanguageDictionary()
        {
            //设置语言
            long language = GlobalConfig.Settings.SelectedLanguage;
            switch (language)
            {

                case 0:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case 1:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;

                case 2:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("ja-JP");
                    break;
                default:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
            }
            Jvedio.Language.Resources.Culture.ClearCachedData();
            Properties.Settings.Default.SelectedLanguage = vieModel.SelectedLanguage;
            Properties.Settings.Default.Save();
        }

        private void Border_MouseLeftButtonUp1(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(Properties.Settings.Default.Selected_BorderBrush);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.Selected_BorderBrush = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                Properties.Settings.Default.Save();
            }


        }

        private void Border_MouseLeftButtonUp2(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(Properties.Settings.Default.Selected_BorderBrush);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.Selected_Background = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                Properties.Settings.Default.Save();
            }

        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            AppDatabase db = e.AddedItems[0] as AppDatabase;
            vieModel.LoadScanPath(db);
        }



        private void setScanDatabases()
        {
            List<AppDatabase> appDatabases = windowMain?.vieModel.DataBases.ToList();
            AppDatabase db = windowMain?.vieModel.CurrentAppDataBase;
            if (appDatabases != null)
            {
                DatabaseComboBox.ItemsSource = appDatabases;
                for (int i = 0; i < appDatabases.Count; i++)
                {
                    if (appDatabases[i].Equals(db))
                    {
                        DatabaseComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            //初次启动后不给设置默认打开上一次库，否则会提示无此数据库
            if (GlobalConfig.Settings.DefaultDBID <= 0)
                openDefaultCheckBox.IsEnabled = false;



            // 设置 crawlerIndex
            serverListBox.SelectedIndex = (int)GlobalConfig.Settings.CrawlerSelectedIndex;

            //设置当前数据库
            setScanDatabases();




            //if (vieModel.DataBases?.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;

            ShowViewRename(GlobalConfig.RenameConfig.FormatString);

            SetCheckedBoxChecked();
            foreach (ComboBoxItem item in OutComboBox.Items)
            {
                if (item.Content.ToString().Equals(GlobalConfig.RenameConfig.OutSplit))
                {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }
            if (OutComboBox.SelectedIndex < 0) OutComboBox.SelectedIndex = 0;

            foreach (ComboBoxItem item in InComboBox.Items)
            {
                if (item.Content.ToString().Equals(GlobalConfig.RenameConfig.InSplit))
                {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }
            if (InComboBox.SelectedIndex < 0) OutComboBox.SelectedIndex = 0;

            //设置主题选中
            bool findTheme = false;
            foreach (var item in SkinWrapPanel.Children.OfType<RadioButton>())
            {
                if (item.Content.ToString() == Properties.Settings.Default.Themes)
                {
                    item.IsChecked = true;
                    findTheme = true;
                    break;
                }
            }
            if (!findTheme)
            {
                for (int i = 0; i < ThemesDataGrid.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)ThemesDataGrid.ItemContainerGenerator.ContainerFromItem(ThemesDataGrid.Items[i]);
                    if (row != null)
                    {
                        var cell = ThemesDataGrid.Columns[0];
                        var cp = (ContentPresenter)cell?.GetCellContent(row);
                        RadioButton rb = (RadioButton)cp?.ContentTemplate.FindName("rb", cp);
                        if (rb != null && rb.Content.ToString() == Properties.Settings.Default.Themes)
                        {
                            rb.IsChecked = true;

                            break;
                        }
                    }

                }
            }

            // 设置代理选中
            List<RadioButton> proxies = proxyStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxies.Count; i++)
            {
                if (i == GlobalConfig.ProxyConfig.ProxyMode) proxies[i].IsChecked = true;
                int idx = i;
                proxies[i].Click += (s, ev) =>
                {
                    GlobalConfig.ProxyConfig.ProxyMode = idx;
                };
            }
            List<RadioButton> proxyTypes = proxyTypesStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < proxyTypes.Count; i++)
            {
                if (i == GlobalConfig.ProxyConfig.ProxyType) proxyTypes[i].IsChecked = true;
                int idx = i;
                proxyTypes[i].Click += (s, ev) =>
                {
                    GlobalConfig.ProxyConfig.ProxyType = idx;
                };
            }
            // 设置代理密码
            passwordBox.Password = vieModel.ProxyPwd;

            passwordBox.PasswordChanged += (s, ev) =>
            {
                vieModel.ProxyPwd = Encrypt.AesEncrypt(passwordBox.Password, AesKey.PROXY);

            };
            adjustPluginViewListBox();

            // 设置插件排序
            var MenuItems = pluginSortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Click += SortMenu_Click;
                MenuItems[i].IsCheckable = true;
            }

            // 同步远程插件
            GlobalConfig.PluginConfig.FetchPluginInfo(() =>
            {
                // 更新插件状态
                Dispatcher.Invoke(() => { setRemotePluginInfo(); });
            });

            vieModel.setPlugins();
            setRemotePluginInfo();



        }


        private List<PluginInfo> parsePluginInfoFromJson(string pluginList)
        {
            List<PluginInfo> result = new List<PluginInfo>();
            try
            {
                List<Dictionary<string, object>> list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(pluginList);
                if (list != null && list.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in list)
                    {
                        if (!dictionary.ContainsKey("Type") || !dictionary.ContainsKey("Data")) continue;
                        string type = dictionary["Type"].ToString().ToLower();
                        if ("crawler".Equals(type))
                        {
                            JArray Data = (JArray)dictionary["Data"];
                            List<Dictionary<string, string>> datas = Data.ToObject<List<Dictionary<string, string>>>();
                            foreach (Dictionary<string, string> dict in datas)
                            {
                                if (!dict.ContainsKey("ServerName") || !dict.ContainsKey("Name") || !dict.ContainsKey("Version")) continue;
                                PluginInfo pluginInfo = PluginInfo.ParseDict(dict);
                                result.Add(pluginInfo);
                            }

                        }
                        else if ("theme".Equals(type))
                        {

                        }

                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return null;
        }

        private void setRemotePluginInfo()
        {
            // 未安装创建
            vieModel.AllFreshPlugins = new List<PluginInfo>();
            string pluginList = GlobalConfig.PluginConfig.PluginList;
            if (!string.IsNullOrEmpty(pluginList))
            {
                List<PluginInfo> pluginInfos = parsePluginInfoFromJson(pluginList);
                if (pluginInfos != null && pluginInfos.Count > 0)
                {
                    foreach (PluginInfo info in pluginInfos)
                    {

                        PluginInfo installed = vieModel.InstalledPlugins.Where(arg => arg.getUID().Equals(info.getUID())).FirstOrDefault();
                        if (installed == null)
                        {
                            // 新插件

                            vieModel.AllFreshPlugins.Add(info);
                        }
                        else
                        {
                            // 检查更新
                            if (installed.Version.CompareTo(info.Version) < 0)
                            {
                                installed.HasNewVersion = true;
                                installed.NewVersion = info.Version;
                                installed.FileName = info.FileName;
                                PluginInfo currentInstalled = vieModel.InstalledPlugins.Where(arg => arg.getUID().Equals(info.getUID())).FirstOrDefault();
                                if (currentInstalled != null) currentInstalled = installed;
                            }
                        }
                    }
                }

            }


            vieModel.CurrentFreshPlugins = new ObservableCollection<PluginInfo>();
            foreach (var item in vieModel.getSortResult(vieModel.AllFreshPlugins))
                vieModel.CurrentFreshPlugins.Add(item);

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
                    if (i == vieModel.PluginSortIndex)
                    {
                        vieModel.PluginSortDesc = !vieModel.PluginSortDesc;
                    }
                    vieModel.PluginSortIndex = i;

                }
                else item.IsChecked = false;
            }
            vieModel.setPlugins();
        }

        private void SetCheckedBoxChecked()
        {
            foreach (ToggleButton item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                if (GlobalConfig.RenameConfig.FormatString.IndexOf(Video.ToSqlField(item.Content.ToString())) >= 0)
                {
                    item.IsChecked = true;
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(YoudaoUrl);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(BaiduUrl);
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            if (vieModel.ScanPath == null) vieModel.ScanPath = new ObservableCollection<string>();
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles)
            {
                if (!FileHelper.IsFile(item))
                {
                    if (!vieModel.ScanPath.Contains(item) && !vieModel.ScanPath.IsIntersectWith(item))
                        vieModel.ScanPath.Add(item);
                    else
                        MessageCard.Error(Jvedio.Language.Resources.FilePathIntersection);

                }
            }
        }

        private void SelectNfoPath(object sender, RoutedEventArgs e)
        {
            //选择NFO存放位置
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (!path.EndsWith("\\")) path = path + "\\";
                vieModel.NFOSavePath = path;
            }
            else
            {
                MessageCard.Error(Jvedio.Language.Resources.Message_CanNotBeNull);
            }
        }


        private void NewServer(object sender, RoutedEventArgs e)
        {
            string serverType = getCurrentServerType();
            if (string.IsNullOrEmpty(serverType)) return;
            CrawlerServer server = new CrawlerServer()
            {
                Enabled = true,
                Url = "https://www.baidu.com/",
                Cookies = "",
                Available = 0,
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[serverType];
            if (list == null) list = new ObservableCollection<CrawlerServer>();
            list.Add(server);
            vieModel.CrawlerServers[serverType] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }



        private string getCurrentServerType()
        {
            int idx = serverListBox.SelectedIndex;
            if (idx < 0 || vieModel.CrawlerServers == null || vieModel.CrawlerServers.Count == 0) return null;
            return vieModel.CrawlerServers.Keys.ToList()[idx];
        }


        private int CurrentRowIndex = 0;
        private void TestServer(object sender, RoutedEventArgs e)
        {


            int idx = CurrentRowIndex;
            string serverType = getCurrentServerType();
            if (string.IsNullOrEmpty(serverType)) return;
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[serverType];
            CrawlerServer server = list[idx];

            if (!server.isHeaderProper())
            {
                MessageCard.Error("Header 不合理");
                return;
            }


            server.Available = 2;
            ServersDataGrid.IsEnabled = false;
            CheckUrl(server, (s) =>
            {
                ServersDataGrid.IsEnabled = true;
                list[idx].LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            });
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {

            string serverType = getCurrentServerType();
            if (string.IsNullOrEmpty(serverType)) return;
            Console.WriteLine(CurrentRowIndex);
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[serverType];
            list.RemoveAt(CurrentRowIndex);
            vieModel.CrawlerServers[serverType] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }


        private void SetCurrentRowIndex(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            CurrentRowIndex = dgr.GetIndex();
        }

        private async void CheckUrl(CrawlerServer server, Action<int> callback)
        {
            // library 需要保证 Cookies 和 UserAgent完全一致
            RequestHeader header = CrawlerServer.parseHeader(server);
            try
            {
                string title = await HttpHelper.AsyncGetWebTitle(server.Url, header);
                if (string.IsNullOrEmpty(title))
                {
                    server.Available = -1;
                }
                else
                {
                    server.Available = 1;
                }
                await Dispatcher.BeginInvoke((Action)delegate
                {
                    ServersDataGrid.Items.Refresh();
                    if (!string.IsNullOrEmpty(title))
                        MessageCard.Success(title);
                });
                callback.Invoke(0);

            }
            catch (WebException ex)
            {
                MessageCard.Error(ex.Message);
                server.Available = -1;
                await Dispatcher.BeginInvoke((Action)delegate
                {
                    ServersDataGrid.Items.Refresh();
                });
                callback.Invoke(0);
            }


        }




        public static T GetVisualChild<T>(Visual parent) where T : Visual

        {

            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++)

            {

                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                child = v as T;

                if (child == null)

                {

                    child = GetVisualChild<T>

                    (v);

                }

                if (child != null)

                {

                    break;

                }

            }

            return child;

        }


        private void SetServerEnable(object sender, MouseButtonEventArgs e)
        {
            //bool enable = !(bool)((CheckBox)sender).IsChecked;
            //vieModel.Servers[CurrentRowIndex].IsEnable = enable;
            //ServerConfig.Instance.SaveServer(vieModel.Servers[CurrentRowIndex]);
            //InitVariable();
            //ServersDataGrid.Items.Refresh();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //注册热键
            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (modifier != 0 && vk != 0)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success)
                {
                    ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.BossKeyError);
                    Properties.Settings.Default.HotKey_Enable = false;
                }
            }
        }

        private void Unregister_HotKey(object sender, RoutedEventArgs e)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
        }


        private void ReplaceWithValue(string property)
        {
            string inSplit = GlobalConfig.RenameConfig.InSplit.Equals("[null]") ? "" : GlobalConfig.RenameConfig.InSplit;
            PropertyInfo[] PropertyList = SampleVideo.GetType().GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(SampleVideo);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "ActorNames" || property == "Genre" || property == "Label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (vieModel.RemoveTitleSpace && property.Equals("Title"))
                            value = value.Trim();

                        if (property == "VideoType")
                        {
                            int v = 0;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = Jvedio.Language.Resources.Uncensored;
                            else if (v == 2)
                                value = Jvedio.Language.Resources.Censored;
                            else if (v == 3)
                                value = Jvedio.Language.Resources.Europe;
                        }
                        vieModel.ViewRenameFormat = vieModel.ViewRenameFormat.Replace("{" + property + "}", value);
                    }
                    break;
                }
            }
        }


        private void SetRenameFormat()
        {

            List<ToggleButton> toggleButtons = CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList();
            List<string> names = toggleButtons.Where(arg => (bool)arg.IsChecked).Select(arg => arg.Content.ToString()).ToList();

            if (names.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                string sep = GlobalConfig.RenameConfig.OutSplit.Equals("[null]") ? "" : GlobalConfig.RenameConfig.OutSplit;
                List<string> formatNames = new List<string>();
                foreach (string name in names)
                {
                    formatNames.Add($"{{{Video.ToSqlField(name)}}}");
                }
                vieModel.FormatString = string.Join(sep, formatNames);
            }
            else
                vieModel.FormatString = "";
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            SetRenameFormat();
            ShowViewRename(vieModel.FormatString);
        }

        private char getSplit(string formatstring)
        {
            int idx = vieModel.FormatString.IndexOf(formatstring);
            if (idx > 0)
                return vieModel.FormatString[idx - 1];
            else
                return '\0';

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel == null) return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            ShowViewRename(txt);
        }

        private void ShowViewRename(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                vieModel.ViewRenameFormat = "";
                return;
            }
            MatchCollection matches = Regex.Matches(txt, "\\{[a-zA-Z]+\\}");
            if (matches != null && matches.Count > 0)
            {
                vieModel.ViewRenameFormat = txt;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    ReplaceWithValue(property);
                }
            }
            else
            {
                vieModel.ViewRenameFormat = "";
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            GlobalConfig.RenameConfig.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            GlobalConfig.RenameConfig.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            SetRenameFormat();
            ShowViewRename(vieModel.FormatString);
        }

        private void SetBackgroundImage(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.Choose;
            OpenFileDialog1.FileName = "background.jpg";
            OpenFileDialog1.Filter = "(jpg;jpeg;png)|*.jpg;*.jpeg;*.png";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = OpenFileDialog1.FileName;
                if (File.Exists(path))
                {
                    //设置背景
                    GlobalVariable.BackgroundImage = null;
                    GC.Collect();
                    GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);
                    Properties.Settings.Default.BackgroundImage = path;
                    (GetWindowByName("Main") as Main)?.SetSkin();
                    (GetWindowByName("WindowDetails") as WindowDetails)?.SetSkin();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            saveSettings();
            GlobalConfig.Settings.Save();
            GlobalConfig.ProxyConfig.Save();
            GlobalConfig.ScanConfig.Save();
            GlobalConfig.FFmpegConfig.Save();
            GlobalConfig.RenameConfig.Save();
        }



        private void saveSettings()
        {
            GlobalConfig.Settings.TabControlSelectedIndex = vieModel.TabControlSelectedIndex;
            GlobalConfig.Settings.OpenDataBaseDefault = vieModel.OpenDataBaseDefault;
            GlobalConfig.Settings.AutoGenScreenShot = vieModel.AutoGenScreenShot;
            GlobalConfig.Settings.TeenMode = vieModel.TeenMode;
            GlobalConfig.Settings.CloseToTaskBar = vieModel.CloseToTaskBar;
            GlobalConfig.Settings.SelectedLanguage = vieModel.SelectedLanguage;
            GlobalConfig.Settings.SaveInfoToNFO = vieModel.SaveInfoToNFO;
            GlobalConfig.Settings.NFOSavePath = vieModel.NFOSavePath;
            GlobalConfig.Settings.OverriteNFO = vieModel.OverriteNFO;
            GlobalConfig.Settings.AutoHandleHeader = vieModel.AutoHandleHeader;
            GlobalConfig.Settings.AutoCreatePlayableIndex = vieModel.AutoCreatePlayableIndex;

            GlobalConfig.Settings.PicPathMode = vieModel.PicPathMode;
            GlobalConfig.Settings.DownloadPreviewImage = vieModel.DownloadPreviewImage;
            GlobalConfig.Settings.OverrideInfo = vieModel.OverrideInfo;
            GlobalConfig.Settings.AutoBackup = vieModel.AutoBackup;
            GlobalConfig.Settings.AutoBackupPeriodIndex = vieModel.AutoBackupPeriodIndex;

            // 代理
            GlobalConfig.ProxyConfig.Server = vieModel.ProxyServer;
            GlobalConfig.ProxyConfig.Port = vieModel.ProxyPort;
            GlobalConfig.ProxyConfig.UserName = vieModel.ProxyUserName;
            GlobalConfig.ProxyConfig.Password = vieModel.ProxyPwd;
            GlobalConfig.ProxyConfig.HttpTimeout = vieModel.HttpTimeout;



            // 扫描
            GlobalConfig.ScanConfig.MinFileSize = vieModel.MinFileSize;
            GlobalConfig.ScanConfig.ScanOnStartUp = vieModel.ScanOnStartUp;
            GlobalConfig.ScanConfig.CopyNFOPicture = vieModel.CopyNFOPicture;

            // ffmpeg
            GlobalConfig.FFmpegConfig.Path = vieModel.FFMPEG_Path;
            GlobalConfig.FFmpegConfig.ThreadNum = vieModel.ScreenShot_ThreadNum;
            GlobalConfig.FFmpegConfig.TimeOut = vieModel.ScreenShot_TimeOut;
            GlobalConfig.FFmpegConfig.ScreenShotNum = vieModel.ScreenShotNum;
            GlobalConfig.FFmpegConfig.ScreenShotIgnoreStart = vieModel.ScreenShotIgnoreStart;
            GlobalConfig.FFmpegConfig.ScreenShotIgnoreEnd = vieModel.ScreenShotIgnoreEnd;
            GlobalConfig.FFmpegConfig.SkipExistGif = vieModel.SkipExistGif;
            GlobalConfig.FFmpegConfig.SkipExistScreenShot = vieModel.SkipExistScreenShot;
            GlobalConfig.FFmpegConfig.GifAutoHeight = vieModel.GifAutoHeight;
            GlobalConfig.FFmpegConfig.GifWidth = vieModel.GifWidth;
            GlobalConfig.FFmpegConfig.GifHeight = vieModel.GifHeight;
            GlobalConfig.FFmpegConfig.GifDuration = vieModel.GifDuration;

            // 重命名

            GlobalConfig.RenameConfig.AddRenameTag = vieModel.AddRenameTag;
            GlobalConfig.RenameConfig.RemoveTitleSpace = vieModel.RemoveTitleSpace;
            GlobalConfig.RenameConfig.FormatString = vieModel.FormatString;



        }

        private void CopyFFmpegUrl(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenUrl(ffmpeg_url);
        }

        private void LoadTranslate(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("youdao.key")) return;
            string v = GetValueKey("youdao.key");
            if (v.Split(' ').Length == 2)
            {
                Properties.Settings.Default.TL_YOUDAO_APIKEY = v.Split(' ')[0];
                Properties.Settings.Default.TL_YOUDAO_SECRETKEY = v.Split(' ')[1];
            }
        }


        public string GetValueKey(string filename)
        {
            string v = "";
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    v = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            if (v != "")
                return Encrypt.AesDecrypt(v, EncryptKeys[0]);
            else
                return "";
        }

        private void LoadAI(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("BaiduAI.key")) return;
            string v = GetValueKey("BaiduAI.key");
            if (v.Split(' ').Length == 2)
            {
                Properties.Settings.Default.Baidu_API_KEY = v.Split(' ')[0];
                Properties.Settings.Default.Baidu_SECRET_KEY = v.Split(' ')[1];
            }
        }

        private int GetRowIndex(RoutedEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null)
                return -1;
            else
                return dgr.GetIndex();
        }



        private void SetScanRe(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ScanRe = (sender as TextBox).Text.Replace("；", ";");
        }

        private void OpenDIY(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(ThemeDIY);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariable.LoadBgImage();
            OnSetSkin();
        }

        private void SetBasePicPath(object sender, MouseButtonEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (!path.EndsWith("\\")) path += "\\";
                vieModel.BasePicPath = path;
            }
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            if (idx < 0) return;
            if (vieModel.CrawlerServers != null && vieModel.CrawlerServers.Count > 0)
            {
                string serverType = vieModel.CrawlerServers.Keys.ToList()[idx];
                int index = serverType.IndexOf('.');
                string serverName = serverType.Substring(0, index);
                string name = serverType.Substring(index + 1);
                PluginInfo pluginInfo = Global.Plugins.Crawlers.Where(arg => arg.ServerName.Equals(serverName) && arg.Name.Equals(name)).FirstOrDefault();
                if (pluginInfo != null && pluginInfo.Enabled) vieModel.PluginEnabled = true;
                else vieModel.PluginEnabled = false;

                ServersDataGrid.ItemsSource = null;
                ServersDataGrid.ItemsSource = vieModel.CrawlerServers[serverType];
                GlobalConfig.Settings.CrawlerSelectedIndex = idx;

            }
        }

        private void ShowCrawlerHelp(object sender, MouseButtonEventArgs e)
        {
            MessageCard.Info("左侧是支持的信息刮削器，右侧需要自行填入刮削器对应的网址，Jvedio 不提供任何网站地址！");
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



        private void pluginViewListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            object item = pluginViewListBox.SelectedItem;
            if (item == null) return;
            PluginInfo pluginInfo = item as PluginInfo;
            vieModel.CurrentPlugin = vieModel.InstalledPlugins.Where(arg => arg.FileHash.Equals(pluginInfo.FileHash)).FirstOrDefault();
            richTextBox.Document = MarkDown.parse(vieModel.CurrentPlugin.MarkDown);
            pluginDetailGrid.Visibility = Visibility.Visible;
        }

        private void freshViewListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            object item = freshViewListBox.SelectedItem;
            if (item == null) return;
            PluginInfo pluginInfo = item as PluginInfo;
            vieModel.CurrentPlugin = vieModel.AllFreshPlugins.Where(arg => arg.getUID().Equals(pluginInfo.getUID())).FirstOrDefault();
            richTextBox.Document = MarkDown.parse(vieModel.CurrentPlugin.MarkDown);
            pluginDetailGrid.Visibility = Visibility.Visible;
        }



        private void ImageSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ComboBox).SelectedIndex;
            if (idx >= 0 && vieModel != null && idx < vieModel.PIC_PATH_MODE_COUNT)
            {
                PathType type = (PathType)idx;
                if (type != PathType.RelativeToData)
                    vieModel.BasePicPath = vieModel.PicPaths[type.ToString()].ToString();
            }
        }

        private void SavePluginEnabled(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Settings.PluginEnabled = new Dictionary<string, bool>();
            bool enabled = (bool)(sender as ChaoControls.Style.Switch).IsChecked;
            foreach (PluginInfo plugin in Global.Plugins.Crawlers)
            {
                if (plugin.getUID().Equals(vieModel.CurrentPlugin.getUID()))
                    plugin.Enabled = enabled;
                GlobalConfig.Settings.PluginEnabled.Add(plugin.getUID(), plugin.Enabled);
            }
            GlobalConfig.Settings.PluginEnabledJson = JsonConvert.SerializeObject(GlobalConfig.Settings.PluginEnabled);
            vieModel.setServers();
        }

        private void url_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchBox searchBox = sender as SearchBox;
            string cookies = searchBox.Text;
            DialogInput dialogInput = new DialogInput(this, "请填入 cookie", cookies);
            if (dialogInput.ShowDialog() == true)
            {
                searchBox.Text = dialogInput.Text;
            }
        }

        CrawlerServer currentCrawlerServer;
        private void url_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            setHeaderPopup.IsOpen = true;
            SearchBox searchBox = sender as SearchBox;
            string headers = searchBox.Text;
            if (!string.IsNullOrEmpty(headers))
            {
                try
                {
                    Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(headers);
                    if (dict != null && dict.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (string key in dict.Keys)
                        {
                            builder.Append($"{key}: {dict[key]}{Environment.NewLine}");
                        }
                        inputTextbox.Text = builder.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }


            //currentHeaderBox = sender as SearchBox;
            //parsedTextbox.Text = currentHeaderBox.Text;
            string serverType = getCurrentServerType();
            currentCrawlerServer = vieModel.CrawlerServers[serverType][ServersDataGrid.SelectedIndex];
        }

        private void CancelHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
        }

        private void ConfirmHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            if (currentCrawlerServer != null)
            {
                currentCrawlerServer.Headers = parsedTextbox.Text.Replace("{" + Environment.NewLine + "    ", "{")
                    .Replace(Environment.NewLine + "}", "}")
                    .Replace($"\",{Environment.NewLine}    \"", "\",\"");
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(currentCrawlerServer.Headers);

                if (dict.ContainsKey("cookie")) currentCrawlerServer.Cookies = dict["cookie"];
            }



        }

        private void InputHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = parse((sender as TextBox).Text);
        }

        private string parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] array = text.Split(Environment.NewLine.ToCharArray());
            foreach (string item in array)
            {
                int idx = item.IndexOf(':');
                if (idx <= 0 || idx >= item.Length - 1) continue;
                string key = item.Substring(0, idx).Trim().ToLower();
                string value = item.Substring(idx + 1).Trim();


                if (!data.ContainsKey(key)) data.Add(key, value);
            }

            //if (vieModel.AutoHandleHeader)
            //{
            data.Remove("content-encoding");
            data.Remove("accept-encoding");
            data.Remove("host");

            data = data.Where(arg => arg.Key.IndexOf(" ") < 0).ToDictionary(x => x.Key, y => y.Value);

            //}




            string json = JsonConvert.SerializeObject(data);
            if (json.Equals("{}"))
                return json;

            return json.Replace("{", "{" + Environment.NewLine + "    ")
                .Replace("}", Environment.NewLine + "}")
                .Replace("\",\"", $"\",{Environment.NewLine}    \"");
        }

        private void SetAutoHeader(object sender, RoutedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = parse(inputTextbox.Text);
        }

        private async void TestProxy(object sender, RoutedEventArgs e)
        {
            vieModel.TestProxyStatus = TaskStatus.Running;
            saveSettings();
            Button button = sender as Button;
            button.IsEnabled = false;
            string url = textProxyUrl.Text;
            //string url = "https://www.baidu.com";
            //string url = "https://www.google.com";



            //WebProxy proxy = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RequestHeader header = new RequestHeader();
            IWebProxy proxy = GlobalConfig.ProxyConfig.GetWebProxy();
            header.TimeOut = GlobalConfig.ProxyConfig.HttpTimeout * 1000;// 转为 ms
            header.WebProxy = proxy;

            HttpResult httpResult = await HttpClient.Get(url, header);
            if (httpResult != null)
            {
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    MessageCard.Success($"成功，延时：{stopwatch.ElapsedMilliseconds} ms");
                    vieModel.TestProxyStatus = TaskStatus.RanToCompletion;
                }
                else
                {
                    MessageCard.Error(httpResult.Error);
                    vieModel.TestProxyStatus = TaskStatus.Canceled;
                }
            }
            else
            {
                MessageCard.Error("失败");
                vieModel.TestProxyStatus = TaskStatus.Canceled;
            }

            stopwatch.Stop();
            button.IsEnabled = true;
        }

        private void ShowHeaderHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("根据之前的方法，打开网页的 cookie 所在处，把 request header 下所有的内容复制进来即可");
        }

        private void ShowScanReHelp(object sender, MouseButtonEventArgs e)
        {
            //MessageCard.Info("在扫描时，对于视频 VID 的识别，例如填写正则为 .*钢铁侠.* 则只要文件名含有钢铁侠，");
        }

        private void ShowRenameHelp(object sender, MouseButtonEventArgs e)
        {
            MessageCard.Info(Jvedio.Language.Resources.Attention_Rename);
        }

        private void BaseWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            adjustPluginViewListBox();
        }

        private void adjustPluginViewListBox()
        {
            if (this.ActualHeight > 0)
            {
                freshViewListBox.MaxHeight = this.ActualHeight - 220;
                pluginViewListBox.MaxHeight = this.ActualHeight - 220;

            }

        }

        private void SearchPlugin(object sender, RoutedEventArgs e)
        {
            SearchBox searchBox = sender as SearchBox;
            vieModel.PluginSearch = searchBox.Text;
            vieModel.setPlugins();
        }

        private void DownloadPlugin(object sender, RoutedEventArgs e)
        {

            PluginInfo pluginInfo = new PluginInfo();
            pluginInfo.Name = vieModel.CurrentPlugin.Name;
            pluginInfo.FileName = vieModel.CurrentPlugin.FileName;
            pluginInfo.Type = vieModel.CurrentPlugin.Type;
            //https://hitchao.github.io/Jvedio-Plugin/plugins/crawlers/testFile.txt
            string remoteUrl = getPluginPath(GlobalVariable.PLUGIN_LIST_BASE_URL, pluginInfo).Replace("\\", "/");
            if (!remoteUrl.IsProperUrl())
            {
                MessageCard.Error("地址不合理 => " + remoteUrl);
                return;
            }
            Button button = sender as Button;
            button.IsEnabled = false;
            DownloadPlugin(remoteUrl, pluginInfo);
        }


        public string getPluginPath(string basePath, PluginInfo pluginInfo)
        {
            if (pluginInfo == null || pluginInfo.FileName == null) return "";
            return Path.Combine(basePath, "plugins", pluginInfo.Type.ToString().ToLower() + "s", pluginInfo.FileName);
        }



        private void DownloadPlugin(string url, PluginInfo pluginInfo)
        {
            Task.Run(async () =>
            {
                HttpResult httpResult = await HttpHelper.AsyncDownLoadFile(url, CrawlerHeader.GitHub);
                if (httpResult.StatusCode == HttpStatusCode.OK && httpResult.FileByte != null)
                {
                    byte[] fileByte = httpResult.FileByte;
                    string saveFileName = getPluginPath(AppDomain.CurrentDomain.BaseDirectory, pluginInfo);
                    if (string.IsNullOrEmpty(saveFileName)) return;

                    string tempDir = Path.Combine(Path.GetDirectoryName(saveFileName), "temp");
                    if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                    string tempFileName = Path.Combine(tempDir, Path.GetFileName(saveFileName));
                    bool success = FileProcess.ByteArrayToFile(fileByte, tempFileName, (error) =>
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageCard.Error(error);
                            });
                        }
                    });
                    if (success) Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"{pluginInfo.Name} 下载成功，即将重启", "插件下载");
                        //执行命令
                        string arg = $"xcopy /y/e \"{tempFileName}\" \"{saveFileName}*\"&TIMEOUT /T 1&start \"\" \"jvedio.exe\" &exit";
                        StreamHelper.TryWrite("upgrade-plugins.bat", arg, true, Encoding.GetEncoding("GB2312"));
                        FileHelper.TryOpenFile("upgrade-plugins.bat");
                        Application.Current.Shutdown();
                    });
                }
            });

        }

        private async void CreatePlayableIndex(object sender, RoutedEventArgs e)
        {
            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() =>
              {
                  List<MetaData> metaDatas = GlobalMapper.metaDataMapper.selectList();
                  total = metaDatas.Count;
                  if (total <= 0) return false;
                  StringBuilder builder = new StringBuilder();
                  List<string> list = new List<string>();
                  for (int i = 0; i < total; i++)
                  {
                      MetaData metaData = metaDatas[i];
                      if (!File.Exists(metaData.Path))
                          builder.Append($"update metadata set PathExist=0 where DataID='{metaData.DataID}';");
                      if (IndexCanceled) return false;
                      App.Current.Dispatcher.Invoke(() =>
                      {
                          indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                      });
                  }
                  string sql = $"begin;update metadata set PathExist=1;{builder};commit;";// 因为大多数资源都是存在的，默认先设为1
                  GlobalMapper.videoMapper.executeNonQuery(sql);
                  return true;
              });
            GlobalConfig.Settings.PlayableIndexCreated = true;
            vieModel.IndexCreating = false;
            if (result)
                MessageCard.Success($"成功建立 {total} 个资源的索引");
        }

        private async void CreatePictureIndex(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, $"当前图片模式为：{((PathType)GlobalConfig.Settings.PicPathMode).ToString()}，仅对当前图片模式生效，是否继续？")
                .ShowDialog() == false)
            {
                return;
            }


            vieModel.IndexCreating = true;
            IndexCanceled = false;
            long total = 0;
            bool result = await Task.Run(() =>
            {
                string sql = VideoMapper.BASE_SQL;
                IWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Select("metadata.DataID", "Path", "VID", "Hash");
                sql = wrapper.toSelect(false) + sql;
                List<Dictionary<string, object>> temp = GlobalMapper.metaDataMapper.select(sql);
                List<Video> videos = GlobalMapper.metaDataMapper.toEntity<Video>(temp, typeof(Video).GetProperties(), true);
                total = videos.Count;
                if (total <= 0) return false;
                List<string> list = new List<string>();
                long pathType = GlobalConfig.Settings.PicPathMode;
                for (int i = 0; i < total; i++)
                {
                    Video video = videos[i];
                    // 小图
                    list.Add($"({video.DataID},{pathType},0,{(File.Exists(video.getSmallImage()) ? 1 : 0)})");
                    // 大图
                    list.Add($"({video.DataID},{pathType},1,{(File.Exists(video.getBigImage()) ? 1 : 0)})");
                    if (IndexCanceled) return false;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        indexCreatingProgressBar.Value = Math.Round(((double)i + 1) / total * 100, 2);
                    });
                }
                string insertSql = $"begin;insert or replace into common_picture_exist(DataID,PathType,ImageType,Exist) values {string.Join(",", list)};commit;";
                GlobalMapper.videoMapper.executeNonQuery(insertSql);
                return true;
            });
            if (result)
                MessageCard.Success($"成功建立 {total} 个资源的索引");
            GlobalConfig.Settings.PictureIndexCreated = true;
            vieModel.IndexCreating = false;



        }


        private bool IndexCanceled = false;

        private void CancelCreateIndex(object sender, RoutedEventArgs e)
        {
            IndexCanceled = true;
        }
    }


}
