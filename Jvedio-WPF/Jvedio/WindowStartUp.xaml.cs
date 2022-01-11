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

namespace Jvedio
{

    public partial class WindowStartUp : Window
    {

        private CancellationTokenSource cts;
        private CancellationToken ct;
        private VieModel_StartUp vieModel_StartUp;
        private Image optionImage;
        private string beforeRename = "";


        public WindowStartUp()
        {

            InitializeComponent();
            vieModel_StartUp = new VieModel_StartUp();
            vieModel_StartUp.ListDatabase();
            this.DataContext = vieModel_StartUp;

            cts = new CancellationTokenSource();
            cts.Token.Register(() => Console.WriteLine("取消任务"));
            ct = cts.Token;

            FileHelper.TryDeleteFile("upgrade.bat");
            FileHelper.TryDeleteDir("Temp");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            statusText.Text = Jvedio.Language.Resources.Status_UpdateConfig;
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

            //复制原有的 info.sqlite
            FileHelper.TryCreateDir("DataBase");
            if (File.Exists("info.sqlite"))
            {
                FileHelper.TryCopyFile("info.sqlite", "DataBase\\info.sqlite");
                FileHelper.TryDeleteFile("info.sqlite");
            }



            CheckFile(); //判断文件是否存在
            CheckSettings();//修复设置错误
            CreateDir();//创建文件夹
            Jvedio.Core.ThemeLoader.loadAllThemes();
            SetSkin(Properties.Settings.Default.Themes);

            if (GlobalFont != null) this.FontFamily = GlobalFont;
            try
            {
                statusText.Text = Jvedio.Language.Resources.Status_InitDatabase;
                InitDataBase();//初始化数据库
                Identify.InitFanhaoList();
                Scan.InitSearchPattern();
                InitVariable();
            }
            catch (Exception ex)
            {
                Logger.LogE(ex);
            }

            try
            {
                statusText.Text = Jvedio.Language.Resources.Status_ClearRecentWatch;
                ClearDateBefore(-10);
                statusText.Text = Jvedio.Language.Resources.Status_ClearLog;
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

                BackUp("Magnets.sqlite");//备份文件
                BackUp("AI.sqlite");//备份文件
                BackUp("Translate.sqlite");//备份文件
            }
            catch (Exception ex)
            {
                Logger.LogE(ex);
            }


            //默认打开某个数据库
            if (Jvedio.Core.Settings.Common.OpenDataBaseDefault && File.Exists(Properties.Settings.Default.DataBasePath))
            {

                OpenDefaultDatabase();
                //启动主窗口
                Main main = new Main();
                statusText.Text = Jvedio.Language.Resources.Status_InitMovie;
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




        public async void LoadDataBase(object sender, MouseButtonEventArgs e)
        {
            //加载数据库
            StackPanel stackPanel = sender as StackPanel;
            TextBox TextBox = stackPanel.Children[1] as TextBox;
            if (!TextBox.IsReadOnly) return;

            string name = TextBox.Text;
            if (name == Jvedio.Language.Resources.NewLibrary)
            {
                //重命名
                TextBox.IsReadOnly = false;
                TextBox.Text = Jvedio.Language.Resources.MyLibrary;
                TextBox.Focus();
                TextBox.SelectAll();
                TextBox.Cursor = Cursors.IBeam;
                return;
            }
            else
                Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"\\DataBase\\{name}.sqlite";
            if (!File.Exists(Properties.Settings.Default.DataBasePath)) return;
            vieModel_StartUp.InitCompleted = false;
            if (Properties.Settings.Default.ScanGivenPath)
            {
                try
                {
                    await Task.Run(() =>
                {

                    this.Dispatcher.BeginInvoke(new Action(() => { statusText.Text = Jvedio.Language.Resources.Status_ScanDir; }), System.Windows.Threading.DispatcherPriority.Background);
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
            statusText.Text = Jvedio.Language.Resources.Status_InitMovie;
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
            this.Close();
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

                    await Task.Run(() =>
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => { statusText.Text = Jvedio.Language.Resources.Status_ScanDir; }), System.Windows.Threading.DispatcherPriority.Render);
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

        public void CheckSettings()
        {
            statusText.Text = Jvedio.Language.Resources.Status_RepairConfig;
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

        public void CreateDir()
        {
            statusText.Text = Jvedio.Language.Resources.Status_CreateDir;
            foreach (var item in InitDirs)
            {
                FileHelper.TryCreateDir(item);
            }
            foreach (var item in PicPaths)
            {
                FileHelper.TryCreateDir(Path.Combine(BasePicPath, item));
            }

        }


        public void CheckFile()
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

        private void BackUp(string filename)
        {
            FileHelper.TryCreateDir("BackUp");
            if (File.Exists(filename))
            {
                string src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                string target = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackUp", filename);
                if (!File.Exists(target))
                {
                    FileHelper.TryCopyFile(src, target);
                }
                else if (new FileInfo(target).Length < new FileInfo(src).Length)
                {
                    FileHelper.TryCopyFile(src, target, true);
                }
            }
        }

        private void InitDataBase()
        {
            if (!File.Exists(AIDataBasePath))
            {
                MySqlite db = new MySqlite("AI");
                db.CreateTable(DataBase.SQLITETABLE_BAIDUAI);
                db.CloseDB();
            }
            else
            {
                //是否具有表结构
                MySqlite db = new MySqlite("AI");
                if (!db.IsTableExist("baidu")) db.CreateTable(DataBase.SQLITETABLE_BAIDUAI);
                db.CloseDB();
            }


            if (!File.Exists(TranslateDataBasePath))
            {
                MySqlite db = new MySqlite("Translate");
                db.CreateTable(DataBase.SQLITETABLE_YOUDAO);
                db.CreateTable(DataBase.SQLITETABLE_BAIDUTRANSLATE);
                db.CloseDB();
            }
            else
            {
                //是否具有表结构
                MySqlite db = new MySqlite("Translate");
                if (!db.IsTableExist("youdao")) db.CreateTable(DataBase.SQLITETABLE_YOUDAO);
                if (!db.IsTableExist("baidu")) db.CreateTable(DataBase.SQLITETABLE_BAIDUTRANSLATE);
                db.CloseDB();
            }

            if (!File.Exists("Magnets.sqlite"))
            {
                MySqlite db = new MySqlite("Magnets");
                db.CreateTable(DataBase.SQLITETABLE_MAGNETS);
                db.CloseDB();
            }

        }





        private void MoveWindow(object sender, MouseEventArgs e)
        {
            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
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
                    if (name == Jvedio.Language.Resources.NewLibrary) continue;
                    if (!DataBase.IsProperSqlite(item)) continue;
                    if (File.Exists($"DataBase\\{name}.sqlite"))
                    {
                        if (new Msgbox(this, $"{Jvedio.Language.Resources.Message_AlreadyExist} {name} {Jvedio.Language.Resources.IsToOverWrite} ？").ShowDialog() == true)
                        {
                            FileHelper.TryCopyFile(item, $"DataBase\\{name}.sqlite", true);
                            if (!vieModel_StartUp.DataBases.Contains(name)) vieModel_StartUp.DataBases.Add(name);
                        }
                    }
                    else
                    {
                        FileHelper.TryCopyFile(item, $"DataBase\\{name}.sqlite", true);
                        if (!vieModel_StartUp.DataBases.Contains(name)) vieModel_StartUp.DataBases.Add(name);
                    }

                }
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void OpenPopup(object sender, MouseButtonEventArgs e)
        {
            optionImage = (sender as Border).Child as Image;
            OptionPopup.IsOpen = true;
        }

        private void DelSqlite(object sender, RoutedEventArgs e)
        {
            Border border = optionImage.Parent as Border;
            Grid grid = border.Parent as Grid;
            StackPanel stackPanel = grid.Children.OfType<StackPanel>().First();
            TextBox TextBox = stackPanel.Children[1] as TextBox;
            string name = TextBox.Text;
            if (name == Jvedio.Language.Resources.NewLibrary) return;

            if (new Msgbox(this, $"{Jvedio.Language.Resources.IsToDelete} {name}?").ShowDialog() == true)
            {
                string dirpath = DateTime.Now.ToString("yyyyMMddHHss");
                Directory.CreateDirectory($"BackUp\\{dirpath}");
                if (File.Exists($"DataBase\\{name}.sqlite"))
                {
                    //备份
                    FileHelper.TryCopyFile($"DataBase\\{name}.sqlite", $"BackUp\\{dirpath}\\{name}.sqlite", true);
                    //删除
                    if (FileHelper.TryDeleteFile($"DataBase\\{name}.sqlite"))
                    {
                        vieModel_StartUp.DataBases.Remove(name);
                    }
                }
            }
        }



        private void RenameSqlite(object sender, RoutedEventArgs e)
        {
            Border border = optionImage.Parent as Border;
            Grid grid = border.Parent as Grid;
            StackPanel stackPanel = grid.Children.OfType<StackPanel>().First();
            TextBox TextBox = stackPanel.Children[1] as TextBox;
            //重命名
            OptionPopup.IsOpen = false;
            TextBox.IsReadOnly = false;
            TextBox.Focus();
            TextBox.SelectAll();
            TextBox.Cursor = Cursors.IBeam;
            beforeRename = TextBox.Text;
        }


        private void Rename(TextBox textBox)
        {
            string name = textBox.Text;

            //不修改
            if (name == beforeRename)
            {
                textBox.IsReadOnly = true;
                textBox.Cursor = Cursors.Hand;
                beforeRename = "";
                return;
            }


            //新建一个数据库
            if (beforeRename == "")
            {
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrEmpty(name) && !IsItemInList(name, vieModel_StartUp.DataBases) && name.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
                {
                    //新建
                    MySqlite db = new MySqlite("DataBase\\" + name);
                    db.CreateTable(DataBase.SQLITETABLE_MOVIE);
                    db.CreateTable(DataBase.SQLITETABLE_ACTRESS);
                    db.CreateTable(DataBase.SQLITETABLE_LIBRARY);
                    db.CreateTable(DataBase.SQLITETABLE_JAVDB);
                    db.CloseDB();

                    if (vieModel_StartUp.DataBases.Contains(Jvedio.Language.Resources.NewLibrary))
                        vieModel_StartUp.DataBases.Remove(Jvedio.Language.Resources.NewLibrary);
                    textBox.IsReadOnly = true;
                    textBox.Cursor = Cursors.Hand;

                    vieModel_StartUp.DataBases.Add(name);
                    vieModel_StartUp.DataBases.Add(Jvedio.Language.Resources.NewLibrary);
                }
                else
                {
                    textBox.Text = Jvedio.Language.Resources.NewLibrary;
                }
            }
            else
            {
                //重命名
                if (IsItemInList(name, vieModel_StartUp.DataBases))
                {
                    textBox.Text = beforeRename; //重复的
                }
                else
                {
                    //重命名
                    if (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
                    {
                        try
                        {
                            File.Move(AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{beforeRename}.sqlite",
                                AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogE(ex);
                        }
                        for (int i = 0; i < vieModel_StartUp.DataBases.Count; i++)
                        {
                            if (vieModel_StartUp.DataBases[i].ToLower() == beforeRename.ToLower())
                            {
                                vieModel_StartUp.DataBases[i] = name;
                                break;
                            }
                        }
                        itemsControl.Items.Refresh();
                    }
                    else
                    {
                        textBox.Text = beforeRename;
                    }


                }
                beforeRename = "";
            }
            textBox.IsReadOnly = true;
            textBox.Cursor = Cursors.Hand;
            textBox.TextAlignment = TextAlignment.Left;
        }


        /// <summary>
        /// 忽略大小写
        /// </summary>
        /// <param name="str"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        private bool IsItemInList(string str, Collection<string> collection)
        {
            foreach (var item in collection)
            {
                if (item?.ToLower() == str.ToLower()) return true;
            }
            return false;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Rename(textBox);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                LoadButton.Focus();
            }
            else if (e.Key == Key.Escape)
            {
                textBox.IsReadOnly = true;
                textBox.Cursor = Cursors.Hand;
            }
        }

        private void SetAsDefault(object sender, RoutedEventArgs e)
        {
            OptionPopup.IsOpen = false;
            Border border = optionImage.Parent as Border;
            Grid grid = border.Parent as Grid;
            StackPanel stackPanel = grid.Children.OfType<StackPanel>().First();
            TextBox TextBox = stackPanel.Children[1] as TextBox;
            string name = TextBox.Text;
            Properties.Settings.Default.OpenDataBaseDefault = true;
            Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite";

            LoadDataBase(stackPanel, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            OptionPopup.IsOpen = false;
            Border border = optionImage.Parent as Border;
            Grid grid = border.Parent as Grid;
            StackPanel stackPanel = grid.Children.OfType<StackPanel>().First();
            TextBox TextBox = stackPanel.Children[1] as TextBox;
            string name = TextBox.Text;
            string path = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite";
            FileHelper.TryOpenSelectPath(path);
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(e.AddedItems[0].ToString());
        }
    }
}
