using Jvedio.Plot.Bar;
using Jvedio.Style;
using Jvedio.Utils;
using Jvedio.ViewModel;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static Jvedio.FileProcess;
namespace Jvedio
{

    public partial class Window_DBManagement : BaseWindow
    {
        private string srcToCopy = "";
        private string dstToCopy = "";
        private static string GrowlToken = "DBManageGrowl";

        private CancellationTokenSource cts;
        private CancellationToken ct;
        private VieModel_DBManagement vieModel_DBManagement;
        private Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance = null;



        public Window_DBManagement()
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            vieModel_DBManagement = new VieModel_DBManagement();
            vieModel_DBManagement.ListDatabase();

            this.DataContext = vieModel_DBManagement;
            if (File.Exists(Properties.Settings.Default.DataBasePath))
            {
                vieModel_DBManagement.CurrentDataBase = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);
                for (int i = 0; i < vieModel_DBManagement.DataBases.Count; i++)
                {
                    if (vieModel_DBManagement.DataBases[i] == vieModel_DBManagement.CurrentDataBase)
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }



            this.SizedChangedCompleted += delegate { ShowStatistic(); };
        }

        private void Jvedio_BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            //设置当前数据库
            for (int i = 0; i < vieModel_DBManagement.DataBases.Count; i++)
            {
                if (vieModel_DBManagement.DataBases[i].ToLower() == Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel_DBManagement.DataBases.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;
        }




        public void RefreshMain()
        {
            //刷新主界面
            string name = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);
            Main main = GetWindowByName("Main") as Main;
            main.vieModel.LoadDataBaseList();
            main.vieModel.DatabaseSelectedIndex = main.vieModel.DataBases.IndexOf(name);
        }

        public void EditDataBase(object sender, MouseButtonEventArgs e)
        {
            string name = "";
            Border border = sender as Border;
            Grid grid = border.Parent as Grid;
            Grid grid1 = grid.Parent as Grid;
            TextBlock textBlock = grid1.Children[1] as TextBlock;
            name = textBlock.Text.ToLower();
            vieModel_DBManagement.CurrentDataBase = name;
            for (int i = 0; i < vieModel_DBManagement.DataBases.Count; i++)
            {
                if (vieModel_DBManagement.DataBases[i] == name)
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }

            var brush = new SolidColorBrush(Colors.Red);
            NameBorder.Background = brush;
            Color TargColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundMain"].ToString())).Color;
            var ca = new ColorAnimation(TargColor, TimeSpan.FromSeconds(0.75));
            brush.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }


        public void DelDataBase(object sender, MouseButtonEventArgs e)
        {
            //删除数据库

            Border border = sender as Border;
            string name = border.Tag.ToString();
            if (new Msgbox(this, $"{Jvedio.Language.Resources.IsToDelete} {name}?").ShowDialog() == true)
            {
                string dirpath = DateTime.Now.ToString("yyyyMMddHHss");
                FileHelper.TryCreateDir($"BackUp\\{dirpath}");
                if (File.Exists($"DataBase\\{name}.sqlite"))
                {
                    //备份
                    FileHelper.TryCopyFile($"DataBase\\{name}.sqlite", $"BackUp\\{dirpath}\\{name}.sqlite", true);
                    //删除
                    if (FileHelper.TryDeleteFile($"DataBase\\{name}.sqlite"))
                    {
                        vieModel_DBManagement.DataBases.Remove(name);
                        RefreshMain();
                        if (vieModel_DBManagement.DataBases.Count == 0)
                        {
                            Properties.Settings.Default.DataBasePath = "";
                            Properties.Settings.Default.Save();
                        }

                    }
                }
            }

        }







        private void NewLibrary(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, Jvedio.Language.Resources.PleaseEnter);
            if (dialogInput.ShowDialog() == true)
            {
                string name = dialogInput.Text.ToLower();


                if (vieModel_DBManagement.DataBases.Contains(name))
                {
                    new Msgbox(this, Jvedio.Language.Resources.Message_AlreadyExist).ShowDialog();
                    return;
                }

                MySqlite db = new MySqlite("DataBase\\" + name);
                db.CreateTable(DataBase.SQLITETABLE_MOVIE);
                db.CreateTable(DataBase.SQLITETABLE_ACTRESS);
                db.CreateTable(DataBase.SQLITETABLE_LIBRARY);
                db.CreateTable(DataBase.SQLITETABLE_JAVDB);


                vieModel_DBManagement.DataBases.Add(name);
                //刷新主界面
                RefreshMain();
            }


        }

        private void ImportLibrary(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseDataBase;
            OpenFileDialog1.Filter = $"Sqlite { Jvedio.Language.Resources.File}|*.sqlite";
            OpenFileDialog1.Multiselect = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] names = OpenFileDialog1.FileNames;

                foreach (var item in names)
                {
                    string name = Path.GetFileNameWithoutExtension(item).ToLower();

                    if (!DataBase.IsProperSqlite(item)) continue;

                    if (File.Exists($"DataBase\\{name}.sqlite"))
                    {
                        if (new Msgbox(this, $"{ Jvedio.Language.Resources.Message_AlreadyExist} {name}，{ Jvedio.Language.Resources.IsToOverWrite}").ShowDialog() == true)
                        {
                            FileHelper.TryCopyFile(item, $"DataBase\\{name}.sqlite", true);

                            if (!vieModel_DBManagement.DataBases.Contains(name)) vieModel_DBManagement.DataBases.Add(name);

                        }
                    }
                    else
                    {
                        FileHelper.TryCopyFile(item, $"DataBase\\{name}.sqlite", true);
                        if (!vieModel_DBManagement.DataBases.Contains(name)) vieModel_DBManagement.DataBases.Add(name);

                    }

                }



            }
            RefreshMain();
        }



        private void StatictisticComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            if (e.AddedItems[0].ToString().ToLower() != Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
            {
                Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{e.AddedItems[0].ToString()}.sqlite";
                //切换数据库
                ShowStatistic();
            }
        }

        private async void BeginTask(object sender, RoutedEventArgs e)
        {

            //数据库管理
            var cb = CheckBoxWrapPanel.Children.OfType<CheckBox>().ToList();
            string path = $"DataBase\\{vieModel_DBManagement.CurrentDataBase}";
            if (!File.Exists(path)) return;


            MySqlite db = new MySqlite(path);
            Button button = (Button)sender;
            button.IsEnabled = false;
            cts = new CancellationTokenSource();
            cts.Token.Register(() => { });
            ct = cts.Token;

            if ((bool)cb[0].IsChecked)
            {
                //重置信息
                db.DeleteTable("movie");
                db.CreateTable(DataBase.SQLITETABLE_MOVIE);
                //清空最近播放和最近创建
                ClearDateBefore(0);
                db.Vacuum();
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, GrowlToken);
            }

            if ((bool)cb[1].IsChecked)
            {
                //删除不存在影片
                long num = 0;
                await Task.Run(() =>
                {
                    var movies = db.SelectMoviesBySql("select * from movie");
                    try
                    {
                        vieModel_DBManagement.ProgressBarValue = 0;
                        List<string> idlist = new List<string>();
                        //先判断再删除
                        for (int i = 0; i < movies.Count; i++)
                        {
                            ct.ThrowIfCancellationRequested();
                            if (!File.Exists(movies[i].filepath))
                            {
                                idlist.Add("'" + movies[i].id + "'");
                                num++;
                            }
                            if (movies.Count > 0) vieModel_DBManagement.ProgressBarValue = (int)((double)(i + 1) / (double)movies.Count * 100);
                        }
                        string sql = $"delete from movie where id in ({string.Join(",", idlist)})";
                        db.ExecuteSql(sql);
                        Console.WriteLine(idlist.Count);
                        db.Vacuum();
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    }
                }, ct);

                HandyControl.Controls.Growl.Success($"{ Jvedio.Language.Resources.SuccessDelete} {num}", GrowlToken);
            }

            if ((bool)cb[2].IsChecked)
            {
                var movies = db.SelectMoviesBySql("select * from movie");
                StringCollection ScanPath = ReadScanPathFromConfig(vieModel_DBManagement.CurrentDataBase);

                long num = 0;
                await Task.Run(() =>
                {
                    try
                    {
                        vieModel_DBManagement.ProgressBarValue = 0;
                        List<string> idlist = new List<string>();
                        //先判断再删除
                        for (int i = 0; i < movies.Count; i++)
                        {
                            ct.ThrowIfCancellationRequested();
                            if (!IsPathIn(movies[i].filepath, ScanPath))
                            {
                                idlist.Add("'" + movies[i].id + "'");
                                num++;
                            }
                            if (movies.Count > 0) vieModel_DBManagement.ProgressBarValue = (int)((double)(i + 1) / (double)movies.Count * 100);
                        }
                        string sql = $"delete from movie where id in ({string.Join(",", idlist)})";
                        db.ExecuteSql(sql);
                        db.Vacuum();
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    }
                }, ct);


                HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.SuccessDelete} {num}", GrowlToken);
            }


            //TODO
            //优化一下速度
            if ((bool)cb[3].IsChecked)
            {
                if (Properties.Settings.Default.SaveInfoToNFO)
                {
                    var detailMovies = db.SelectDetailMoviesBySql("select * from movie");
                    await Task.Run(() =>
                    {
                        try
                        {
                            vieModel_DBManagement.ProgressBarValue = 0;
                            for (int i = 0; i < detailMovies.Count; i++)
                            {
                                ct.ThrowIfCancellationRequested();
                                FileProcess.SaveNfo(detailMovies[i]);
                                if (detailMovies.Count > 0) vieModel_DBManagement.ProgressBarValue = (int)((double)(i + 1) / (double)detailMovies.Count * 100);
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                        }
                    }, ct);
                    HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.Message_Success}", GrowlToken);
                }
                else
                {
                    HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.setnfo}", GrowlToken);
                }

            }
            db.CloseDB();
            cts.Dispose();
            await Task.Run(() => { Task.Delay(500).Wait(); });
            Main main = (Main)GetWindowByName("Main");
            main?.vieModel.Reset();
            button.IsEnabled = true;
        }

        public bool IsPathIn(string path, StringCollection paths)
        {
            foreach (var item in paths)
            {
                if (path.IndexOf(item) >= 0) return true;
            }
            return false;
        }



        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            WaitingPanel.Visibility = Visibility.Hidden;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowStatistic();
        }


        private async void ShowStatistic()
        {
            if (TabControl.SelectedIndex == 1)
            {
                await Task.Run(() =>
                {
                    vieModel_DBManagement.Statistic();
                    IDBarView.Datas = vieModel_DBManagement.LoadID();
                    IDBarView.Title = Jvedio.Language.Resources.ID;
                    IDBarView.Refresh();
                    Task.Delay(300).Wait();
                    ActorBarView.Datas = vieModel_DBManagement.LoadActor();
                    ActorBarView.Title = Jvedio.Language.Resources.Actor;
                    ActorBarView.Refresh();
                    Task.Delay(300).Wait();
                    GenreBarView.Datas = vieModel_DBManagement.LoadGenre();
                    GenreBarView.Title = Jvedio.Language.Resources.Genre;
                    GenreBarView.Refresh();
                    Task.Delay(300).Wait();
                    TagBarView.Datas = vieModel_DBManagement.LoadTag();
                    TagBarView.Title = Jvedio.Language.Resources.Tag;
                    TagBarView.Refresh();
                });
            }
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
                taskbarInstance.SetProgressValue((int)e.NewValue, 100, this);
                if (e.NewValue == 100) taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }
        }

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (vieModel_DBManagement.ProgressBarValue == 0 && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }
        }

        private void CancelTask(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
            vieModel_DBManagement.ProgressBarValue = 0;
            RunButton.IsEnabled = true;
        }

        private void Jvedio_BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                cts?.Cancel();
                cts_copy?.Cancel();
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            StackPanel sp = comboBox.Parent as StackPanel;
            TextBlock tb = sp.Children.OfType<TextBlock>().First();
            StackPanel topsp = sp.Parent as StackPanel;
            TextBlock textBlock = topsp.Children.OfType<TextBlock>().First();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase", e.AddedItems[0].ToString() + ".sqlite");
            double count = 0;
            long length = 0;
            if (File.Exists(path))
            {
                using (MySqlite sqlite = new MySqlite(path, true))
                {
                    count = sqlite.SelectCountByTable("movie");
                }
                length = new FileInfo(path).Length;
            }
            textBlock.Text = $"{Jvedio.Language.Resources.Number}：{count}\n{Jvedio.Language.Resources.FileSize}：{length.ToProperFileSize()}";

            if (tb.Text == Jvedio.Language.Resources.Source)
                srcToCopy = path;
            else
                dstToCopy = path;
        }



        CancellationToken ct_copy;
        CancellationTokenSource cts_copy;
        private void BeginCopy(object sender, RoutedEventArgs e)
        {
            if (srcToCopy == "" || dstToCopy == "") return;
            if (!File.Exists(srcToCopy) || !File.Exists(dstToCopy)) return;
            if (srcToCopy == dstToCopy)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.SamePathError, GrowlToken);
                return;
            }
            cts_copy = new CancellationTokenSource();
            ct_copy = cts_copy.Token;
            CopyButton.IsEnabled = false;
            bool skipnulltitle = (bool)SkipNullTitle.IsChecked;
            Task.Run(() =>
            {
                try
                {
                    DataBase.CopyDatabaseInfo(srcToCopy, dstToCopy, ct_copy, (value) =>
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                        {
                            CopyProgressBar.Value = value;
                            if (value == 100)
                            {
                                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, GrowlToken);
                                cts_copy.Dispose();
                                CopyButton.IsEnabled = true;
                            }
                        });
                    }, skipnulltitle);
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Cancel, GrowlToken);
                }

            });
        }

        private void CancelCopy(object sender, RoutedEventArgs e)
        {
            try
            {
                cts_copy?.Cancel();
                CopyButton.IsEnabled = true;
            }
            catch (ObjectDisposedException ex) { Console.WriteLine(ex.Message); }
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel sp = button.Parent as StackPanel;
            ComboBox comboBox = sp.Children.OfType<ComboBox>().First();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase", comboBox.Text + ".sqlite");
            FileHelper.TryOpenSelectPath(path, GrowlToken);
        }

        private void currentDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            vieModel_DBManagement.CurrentDataBase = e.AddedItems[0].ToString();
        }
    }

}
