using FontAwesome.WPF;
using Jvedio.Style;
using Jvedio.Utils;
using Jvedio.Utils.Encrypt;
using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
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
    public partial class Settings : BaseWindow
    {

        public const string ffmpeg_url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";
        public static string GrowlToken = "SettingsGrowl";
        public DetailMovie SampleMovie = new DetailMovie()
        {
            id = "AAA-001",
            title = Jvedio.Language.Resources.SampleMovie_Title,
            vediotype = 1,
            releasedate = "2020-01-01",
            director = Jvedio.Language.Resources.SampleMovie_Director,
            genre = Jvedio.Language.Resources.SampleMovie_Genre,
            tag = Jvedio.Language.Resources.SampleMovie_Tag,
            actor = Jvedio.Language.Resources.SampleMovie_Actor,
            studio = Jvedio.Language.Resources.SampleMovie_Studio,
            rating = 9.0f,
            chinesetitle = Jvedio.Language.Resources.SampleMovie_TranslatedTitle,
            label = Jvedio.Language.Resources.SampleMovie_Label,
            year = 2020,
            runtime = 126,
            country = Jvedio.Language.Resources.SampleMovie_Country
        };
        public VieModel_Settings vieModel_Settings;
        public Settings()
        {
            InitializeComponent();
            if (GlobalFont != null) this.FontFamily = GlobalFont;
            vieModel_Settings = new VieModel_Settings();


            this.DataContext = vieModel_Settings;
            vieModel_Settings.Reset();



            //绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                item.Click += AddToRename;
            }
            if (Properties.Settings.Default.SettingsIndex == 2)
                TabControl.SelectedIndex = 0;
            else
                TabControl.SelectedIndex = Properties.Settings.Default.SettingsIndex;

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
                HandyControl.Controls.Growl.Error("必须为 功能键 + 数字/字母", GrowlToken);
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
                        HandyControl.Controls.Growl.Success("设置热键成功", GrowlToken);
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
                if (vieModel_Settings.ScanPath == null) { vieModel_Settings.ScanPath = new ObservableCollection<string>(); }
                if (!vieModel_Settings.ScanPath.Contains(path) && !vieModel_Settings.ScanPath.IsIntersectWith(path))
                {
                    vieModel_Settings.ScanPath.Add(path);
                    //保存
                    FileProcess.SaveScanPathToConfig(vieModel_Settings.DataBase, vieModel_Settings.ScanPath?.ToList());
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                }


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

                string base64 = Resource_String.BaseImage64;
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
            if (PathListBox.SelectedIndex != -1)
            {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel_Settings.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
            if (vieModel_Settings.ScanPath != null)
                SaveScanPathToConfig(vieModel_Settings.DataBase, vieModel_Settings.ScanPath.ToList());

        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {

            vieModel_Settings.ScanPath?.Clear();
            SaveScanPathToConfig(vieModel_Settings.DataBase, new List<string>());
        }





        private void Restore(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, Jvedio.Language.Resources.Message_IsToReset).ShowDialog() == true)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.FirstRun = false;
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

        private void FlowTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 30)
                {
                    Properties.Settings.Default.FlowNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ActorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 50)
                {
                    Properties.Settings.Default.ActorDisplayNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ScreenShotNumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 20)
                {
                    Properties.Settings.Default.ScreenShotNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ScanMinFileSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num >= 0 & num <= 2000)
                {
                    Properties.Settings.Default.ScanMinFileSize = num;
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
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.RebootToTakeEffect, GrowlToken);
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
                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NoPermissionToListen} {drives[i]}", GrowlToken);
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

        private void SetBasePicPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (path.Substring(path.Length - 1, 1) != "\\") { path = path + "\\"; }
                Properties.Settings.Default.BasePicPath = path;
            }
            else
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_CanNotBeNull, GrowlToken);
            }
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Opacity_Main >= 0.5)
                App.Current.Windows[0].Opacity = Properties.Settings.Default.Opacity_Main;
            else
                App.Current.Windows[0].Opacity = 1;

            ////UpdateServersEnable();

            GlobalVariable.InitVariable();
            Scan.InitSearchPattern();
            HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, GrowlToken);
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
                        Properties.Settings.Default.FFMPEG_Path = exePath;
                }
            }
        }

        private void SetSkin(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            Main main = GetWindowByName("Main") as Main;
            main.SetSkin();
            main?.SetSelected();
            main?.ActorSetSelected();
        }



        private void SetLanguage(object sender, RoutedEventArgs e)
        {
            //https://blog.csdn.net/fenglailea/article/details/45888799
            Properties.Settings.Default.Language = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            string language = Properties.Settings.Default.Language;
            string hint = "";
            if (language == "English")
                hint = "Take effect after restart";
            else if (language == "日本語")
                hint = "再起動後に有効になります";
            else
                hint = "重启后生效";
            HandyControl.Controls.Growl.Success(hint, GrowlToken);


            //SetLanguageDictionary();


        }

        private void SetLanguageDictionary()
        {
            //设置语言
            string language = Jvedio.Properties.Settings.Default.Language;
            switch (language)
            {
                case "日本語":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("ja-JP");
                    break;
                case "中文":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case "English":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
                default:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
            }
            //Jvedio.Language.Resources.Culture.ClearCachedData();
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
            vieModel_Settings.DataBase = e.AddedItems[0].ToString();
            vieModel_Settings.Reset();




        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //设置当前数据库
            for (int i = 0; i < vieModel_Settings.DataBases.Count; i++)
            {
                if (vieModel_Settings.DataBases[i].ToLower() == Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel_Settings.DataBases.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;

            ShowViewRename(Properties.Settings.Default.RenameFormat);

            SetCheckedBoxChecked();


            foreach (ComboBoxItem item in OutComboBox.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.OutSplit)
                {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }


            foreach (ComboBoxItem item in InComboBox.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.InSplit)
                {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }

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

        }

        private void SetCheckedBoxChecked()
        {
            foreach (ToggleButton item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                if (Properties.Settings.Default.RenameFormat.IndexOf(item.Content.ToString().ToSqlField()) >= 0)
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
            if (vieModel_Settings.ScanPath == null) { vieModel_Settings.ScanPath = new ObservableCollection<string>(); }
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles)
            {
                if (!IsFile(item))
                {
                    if (!vieModel_Settings.ScanPath.Contains(item) && !vieModel_Settings.ScanPath.IsIntersectWith(item))
                    {
                        vieModel_Settings.ScanPath.Add(item);
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                    }
                }

            }
            //保存
            FileProcess.SaveScanPathToConfig(vieModel_Settings.DataBase, vieModel_Settings.ScanPath.ToList());

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //选择NFO存放位置
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (path.Substring(path.Length - 1, 1) != "\\") { path = path + "\\"; }
                Properties.Settings.Default.NFOSavePath = path;

            }
            else
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_CanNotBeNull, GrowlToken);
            }

        }


        private void NewServer(object sender, RoutedEventArgs e)
        {
            if (vieModel_Settings.Servers.Count >= 10) return;
            vieModel_Settings.Servers.Add(new Server()
            {
                IsEnable = true,
                Url = "https://",
                Cookie = Jvedio.Language.Resources.Nothing,
                Available = 0,
                Name = "",
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }); ;
        }


        private int CurrentRowIndex = 0;
        private void TestServer(object sender, RoutedEventArgs e)
        {
            int rowIndex = CurrentRowIndex;
            vieModel_Settings.Servers[rowIndex].LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            vieModel_Settings.Servers[rowIndex].Available = 2;
            ServersDataGrid.IsEnabled = false;
            CheckUrl(vieModel_Settings.Servers[rowIndex], (s) =>
            {
                ServersDataGrid.IsEnabled = true;
            });
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {
            Server server = vieModel_Settings.Servers[CurrentRowIndex];
            ServerConfig.Instance.DeleteByName(server.Name);
            vieModel_Settings.Servers.RemoveAt(CurrentRowIndex);
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

        private async void CheckUrl(Server server, Action<int> callback)
        {
            bool enablecookie = false;
            if (server.Name == "DMM" || server.Name == "DB" || server.Name == "MOO") enablecookie = true;
            (bool result, string title) = await new MyNet().TestAndGetTitle(server.Url, enablecookie, server.Cookie, server.Name);
            if (!result && title.IndexOf("DB") >= 0)
            {
                await Dispatcher.BeginInvoke((Action)delegate
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_TestError, GrowlToken);
                });
                callback.Invoke(0);
            }
            if (result && title != "")
            {
                server.Available = 1;
                if (title.IndexOf("JavBus") >= 0 && title.IndexOf("歐美") < 0)
                {
                    server.Name = "Bus";
                }
                else if (title.IndexOf("JavBus") >= 0 && title.IndexOf("歐美") >= 0)
                {
                    server.Name = "BusEurope";
                }
                else if (title.IndexOf("JavDB") >= 0)
                {
                    server.Name = "DB";
                }
                else if (title.IndexOf("JavLibrary") >= 0)
                {
                    server.Name = "Library";
                }
                else if (title.IndexOf("FANZA") >= 0)
                {
                    server.Name = "DMM";
                    if (server.Url.EndsWith("top/")) server.Url = server.Url.Replace("top/", "");
                }
                else if (title.IndexOf("FC2コンテンツマーケット") >= 0 || title.IndexOf("FC2电子市场") >= 0)
                {
                    server.Name = "FC2";
                }
                else if (title.IndexOf("JAV321") >= 0)
                {
                    server.Name = "Jav321";
                }
                else if (title.IndexOf("AVMOO") >= 0)
                {
                    server.Name = "MOO";
                }
                else
                {
                    server.Name = title;
                }
            }
            else
            {
                server.Available = -1;
            }
            await Dispatcher.BeginInvoke((Action)delegate
            {
                ServersDataGrid.Items.Refresh();
            });


            if (NeedCookie.Contains(server.Name))
            {
                //是否包含 cookie
                if (server.Cookie == Jvedio.Language.Resources.Nothing || server.Cookie == "")
                {
                    server.Available = -1;
                    await Dispatcher.BeginInvoke((Action)delegate
                    {
                        new Msgbox(this, Jvedio.Language.Resources.Message_NeedCookies).ShowDialog();
                    });

                }
                else
                {
                    ServerConfig.Instance.SaveServer(server);//保存覆盖
                }
            }
            else
            {
                ServerConfig.Instance.SaveServer(server);//保存覆盖
            }
            callback.Invoke(0);
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
            bool enable = !(bool)((CheckBox)sender).IsChecked;
            vieModel_Settings.Servers[CurrentRowIndex].IsEnable = enable;
            ServerConfig.Instance.SaveServer(vieModel_Settings.Servers[CurrentRowIndex]);
            InitVariable();
            ServersDataGrid.Items.Refresh();
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
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.BossKeyError, GrowlToken);
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
            string inSplit = InComboBox.Text.Replace(Jvedio.Language.Resources.Nothing, "");
            PropertyInfo[] PropertyList = SampleMovie.GetType().GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(SampleMovie);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "actor" || property == "genre" || property == "label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (property == "vediotype")
                        {
                            int v = 1;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = Jvedio.Language.Resources.Uncensored;
                            else if (v == 2)
                                value = Jvedio.Language.Resources.Censored;
                            else if (v == 3)
                                value = Jvedio.Language.Resources.Europe;
                        }
                        vieModel_Settings.ViewRenameFormat = vieModel_Settings.ViewRenameFormat.Replace("{" + property + "}", value);
                    }
                    break;
                }
            }
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            string text = toggleButton.Content.ToString();
            bool ischecked = (bool)toggleButton.IsChecked;
            string formatstring = "{" + text.ToSqlField() + "}";

            string split = OutComboBox.Text.Replace(Jvedio.Language.Resources.Nothing, "");


            if (ischecked)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.RenameFormat))
                {
                    Properties.Settings.Default.RenameFormat += formatstring;
                }
                else
                {
                    Properties.Settings.Default.RenameFormat += split + formatstring;
                }
            }
            else
            {
                int idx = Properties.Settings.Default.RenameFormat.IndexOf(formatstring);
                if (idx == 0)
                {
                    Properties.Settings.Default.RenameFormat = Properties.Settings.Default.RenameFormat.Replace(formatstring, "");
                }
                else
                {
                    Properties.Settings.Default.RenameFormat = Properties.Settings.Default.RenameFormat.Replace(getSplit(formatstring) + formatstring, "");
                }
            }
        }

        private char getSplit(string formatstring)
        {
            int idx = Properties.Settings.Default.RenameFormat.IndexOf(formatstring);
            if (idx > 0)
                return Properties.Settings.Default.RenameFormat[idx - 1];
            else
                return '\0';

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel_Settings == null) return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            ShowViewRename(txt);
        }

        private void ShowViewRename(string txt)
        {

            MatchCollection matches = Regex.Matches(txt, "\\{[a-z]+\\}");
            if (matches != null && matches.Count > 0)
            {
                vieModel_Settings.ViewRenameFormat = txt;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    ReplaceWithValue(property);
                }
            }
            else
            {
                vieModel_Settings.ViewRenameFormat = "";
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Properties.Settings.Default.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Properties.Settings.Default.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
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
            Properties.Settings.Default.SettingsIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void CopyFFmpegUrl(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenUrl(ffmpeg_url, GrowlToken);
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CurrentRowIndex = GetRowIndex(e);
            HandyControl.Controls.TextBox textBox = sender as HandyControl.Controls.TextBox;
            textBox.Background = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];
            textBox.Foreground = (SolidColorBrush)Application.Current.Resources["BackgroundMenu"];
            textBox.CaretBrush = (SolidColorBrush)Application.Current.Resources["BackgroundMenu"];
        }


        //修改地址
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.TextBox textBox = sender as HandyControl.Controls.TextBox;
            textBox.Background = Brushes.Transparent;
            textBox.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundGlobal"];

            if (CurrentRowIndex >= vieModel_Settings.Servers.Count || CurrentRowIndex < 0) return;

            if (textBox.Name == "url")
                vieModel_Settings.Servers[CurrentRowIndex].Url = textBox.Text;
            else
                vieModel_Settings.Servers[CurrentRowIndex].Cookie = textBox.Text;
        }


        private void SetScanRe(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ScanRe = (sender as TextBox).Text.Replace("；", ";");
        }

        private void OpenDIY(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(ThemeDIY, GrowlToken);
        }
    }


}
