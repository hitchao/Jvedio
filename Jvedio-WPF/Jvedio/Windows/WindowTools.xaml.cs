using Jvedio.Style;
using Jvedio.Utils;
using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using static Jvedio.FileProcess;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class WindowTools : BaseWindow
    {
        public const string GrowlToken = "ToolsGrowl";
        public CancellationTokenSource cts;
        public CancellationToken ct;
        public bool Running;
        VieModel_Tools vieModel;

        public WindowTools()
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            //WinState = 0;//每次重新打开窗体默认为Normal
            vieModel = new VieModel_Tools();
            this.DataContext = vieModel;
            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_CancelCurrentTask, GrowlToken); });
            ct = cts.Token;
            Running = false;
            TabControl.SelectedIndex = Properties.Settings.Default.ToolsIndex;

            StatusTextBlock.Visibility = Visibility.Hidden;
            LoadingStackPanel.Visibility = Visibility.Hidden;
        }

        public void ShowAccessPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = $"{Jvedio.Language.Resources.Choose} Access {Jvedio.Language.Resources.File}";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = $"Access {Jvedio.Language.Resources.File}(*.mdb)| *.mdb";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AccessPathTextBox.Text = OpenFileDialog1.FileName;
            }
        }


        public void ShowUNCPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                UNCPathTextBox.Text = path;
            }
        }

        public void ShowNFOPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = $"{Jvedio.Language.Resources.Choose} NFO {Jvedio.Language.Resources.File}";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = $"NFO {Jvedio.Language.Resources.File}(*.nfo)| *.nfo";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NFOPathTextBox.Text = OpenFileDialog1.FileName;
            }
        }



        public void AddPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (!vieModel.ScanPath.Contains(path) && !vieModel.ScanPath.IsIntersectWith(path))
                {
                    vieModel.ScanPath.Add(path);
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                }



            }




        }

        public void DelPath(object sender, RoutedEventArgs e)
        {
            if (PathListBox.SelectedIndex != -1)
            {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {
            vieModel.ScanPath.Clear();
        }



        public void AddEuPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (!vieModel.ScanEuPath.Contains(path) && !vieModel.ScanEuPath.IsIntersectWith(path))
                {
                    vieModel.ScanEuPath.Add(path);
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                }

            }




        }

        public void DelEuPath(object sender, RoutedEventArgs e)
        {
            if (EuropePathListBox.SelectedIndex != -1)
            {
                for (int i = EuropePathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.ScanEuPath.Remove(EuropePathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearEuPath(object sender, RoutedEventArgs e)
        {
            vieModel.ScanEuPath.Clear();
        }






        public void AddNFOPath(object sender, RoutedEventArgs e)
        {
            string path = FileHelper.SelectPath(this);
            //path = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "DownLoad\\NFO") ? AppDomain.CurrentDomain.BaseDirectory + "DownLoad\\NFO" : AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(path))
            {
                if (!vieModel.NFOScanPath.Contains(path) && !vieModel.NFOScanPath.IsIntersectWith(path))
                {
                    vieModel.NFOScanPath.Add(path);
                }
                else
                {
                    HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                }

            }




        }


        public async void StartRun(object sender, RoutedEventArgs e)
        {

            if (Running)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_StopAndTry, GrowlToken);
                return;
            }

            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_CancelCurrentTask, GrowlToken); });
            ct = cts.Token;

            int index = TabControl.SelectedIndex;
            Running = true;
            switch (index)
            {
                case 0:
                    //扫描
                    double totalnum = 0;//扫描出的视频总数
                    double insertnum = 0;//导入的视频总数
                    try
                    {
                        //全盘扫描
                        if ((bool)ScanAll.IsChecked)
                        {
                            LoadingStackPanel.Visibility = Visibility.Visible;
                            await Task.Run(() =>
                            {
                                ct.ThrowIfCancellationRequested();

                                List<string> filepaths = Scan.ScanAllDrives();
                                totalnum = filepaths.Count;
                                insertnum = Scan.InsertWithNfo(filepaths, ct);
                            });
                        }
                        else
                        {
                            if (vieModel.ScanPath.Count == 0) { break; }
                            LoadingStackPanel.Visibility = Visibility.Visible;



                            await Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();

                            StringCollection stringCollection = new StringCollection();
                            foreach (var item in vieModel.ScanPath)
                            {
                                if (Directory.Exists(item)) { stringCollection.Add(item); }
                            }
                            List<string> filepaths = Scan.ScanPaths(stringCollection, ct);
                            totalnum = filepaths.Count;
                            insertnum = Scan.InsertWithNfo(filepaths, ct);
                        }, cts.Token);

                        }

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_ScanNum} {totalnum}  {Jvedio.Language.Resources.ImportNumber} {insertnum}", GrowlToken);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }


                    break;
                case 1:
                    //Access
                    LoadingStackPanel.Visibility = Visibility.Visible;
                    string AccessPath = AccessPathTextBox.Text;
                    if (!File.Exists(AccessPath))
                    {
                        HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist} {AccessPath}", GrowlToken);

                        break;
                    }
                    try
                    {
                        await Task.Run(() =>
                    {
                        DataBase.InsertFromAccess(AccessPath);
                    });
                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, GrowlToken);
                        }

                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }
                    break;
                case 2:
                    //NFO
                    if (NFOTabControl.SelectedIndex == 0)
                    {
                        if (vieModel.NFOScanPath.Count == 0) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_CanNotBeNull, GrowlToken); }
                    }
                    else { if (!File.Exists(NFOPathTextBox.Text)) { HandyControl.Controls.Growl.Warning($"{Jvedio.Language.Resources.Message_FileNotExist} {NFOPathTextBox.Text}", GrowlToken); } }


                    Running = true;

                    try
                    {
                        List<string> nfoFiles = new List<string>();
                        if (NFOTabControl.SelectedIndex == 1)
                        {
                            nfoFiles.Add(NFOPathTextBox.Text);
                        }
                        else
                        {
                            //扫描所有nfo文件
                            await Task.Run(() =>
                            {
                                this.Dispatcher.Invoke((Action)delegate
                                {
                                    StatusTextBlock.Visibility = Visibility.Visible;
                                    StatusTextBlock.Text = Jvedio.Language.Resources.BeginScan;
                                });

                                StringCollection stringCollection = new StringCollection();
                                foreach (var item in vieModel.NFOScanPath)
                                {
                                    if (Directory.Exists(item)) { stringCollection.Add(item); }
                                }
                                nfoFiles = Scan.ScanNFO(stringCollection, ct, (filepath) =>
                                {
                                    this.Dispatcher.Invoke((Action)delegate { StatusTextBlock.Text = filepath; });
                                });
                            }, cts.Token);
                        }


                        //记录日志
                        Logger.LogScanInfo(Environment.NewLine + $"-----【" + DateTime.Now.ToString() + $"】NFO {Jvedio.Language.Resources.Scan}-----");
                        Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.Scan}{Jvedio.Language.Resources.Number} => {nfoFiles.Count}  ");


                        //导入所有 nfo 文件信息
                        double total = 0;
                        bool importpic = (bool)NFOCopyPicture.IsChecked;
                        await Task.Run(() =>
                        {

                            nfoFiles.ForEach(item =>
                            {
                                if (File.Exists(item))
                                {
                                    Movie movie = GetInfoFromNfo(item);
                                    if (movie != null && !string.IsNullOrEmpty(movie.id))
                                    {

                                        DataBase.InsertFullMovie(movie);
                                        //复制并覆盖所有图片
                                        if (importpic) CopyPicToPath(movie.id, item);
                                        total += 1;
                                        Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNumber} => {item}  ");
                                    }

                                }
                            });


                        });
                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNumber} {total} ");
                            HandyControl.Controls.Growl.Success(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNumber} {total} ", GrowlToken);
                        }

                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }


                    break;

                case 3:
                    //欧美扫描
                    if (vieModel.ScanEuPath.Count == 0) { break; }
                    LoadingStackPanel.Visibility = Visibility.Visible;
                    totalnum = 0;
                    insertnum = 0;

                    try
                    {
                        await Task.Run(() =>
                        {
                            StringCollection stringCollection = new StringCollection();
                            foreach (var item in vieModel.ScanEuPath) if (Directory.Exists(item)) { stringCollection.Add(item); }
                            List<string> filepaths = Scan.ScanPaths(stringCollection, ct);
                            totalnum = filepaths.Count;
                            insertnum = Scan.InsertWithNfo(filepaths, ct, IsEurope: true);
                        });

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Scan}{Jvedio.Language.Resources.Number} {totalnum}  {Jvedio.Language.Resources.ImportNumber} {insertnum} ", GrowlToken);
                        }
                    }
                    finally

                    {
                        cts.Dispose();
                        Running = false;
                    }

                    break;

                case 4:

                    break;

                case 5:
                    //网络驱动器
                    LoadingStackPanel.Visibility = Visibility.Visible;

                    string path = UNCPathTextBox.Text;
                    if (path == "") { break; }

                    bool CanScan = true;
                    //检查权限
                    await Task.Run(() =>
                    {
                        try { var tl = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly); }
                        catch { CanScan = false; }
                    });

                    if (!CanScan) { LoadingStackPanel.Visibility = Visibility.Hidden; HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.InsufficientPermissions, GrowlToken); break; }


                    bool IsEurope = !(bool)ScanTypeRadioButton.IsChecked;

                    totalnum = 0;
                    insertnum = 0;
                    try
                    {
                        await Task.Run(() =>
                        {
                            StringCollection stringCollection = new StringCollection();
                            stringCollection.Add(path);
                            List<string> filepaths = Scan.ScanPaths(stringCollection, ct);
                            totalnum = filepaths.Count;
                            insertnum = Scan.InsertWithNfo(filepaths, ct, IsEurope: IsEurope);
                        });

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested) { HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Scan}{Jvedio.Language.Resources.Number} {totalnum}  {Jvedio.Language.Resources.ImportNumber} {insertnum} ", GrowlToken); }
                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }
                    break;

                default:

                    break;

            }
            Running = false;

        }



        public bool IsDownLoading()
        {
            bool result = false;
            Main main = null;
            Window window = GetWindowByName("Main");
            if (window != null) main = (Main)window;


            if (main?.DownLoader != null)
            {
                if (main.DownLoader.State == DownLoadState.DownLoading | main.DownLoader.State == DownLoadState.Pause)
                {
                    Console.WriteLine("main.DownLoader.State   " + main.DownLoader.State);
                    result = true;
                }


            }


            return result;
        }




        public void ShowRunInfo(object sender, RoutedEventArgs e)
        {
            int index = TabControl.SelectedIndex;
            string filepath = "";
            if (index == 1)
            {
                filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\DataBase\\{DateTime.Now.ToString("yyyy -MM-dd")}.log";
            }
            else if (index == 4)
            {

            }
            else
            {
                filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            }
            if (filepath == "")
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.NoLog, GrowlToken);
            else
                FileHelper.TryOpenSelectPath(filepath, GrowlToken);
        }

        public void DelNFOPath(object sender, RoutedEventArgs e)
        {
            if (NFOPathListBox.SelectedIndex != -1)
            {
                for (int i = NFOPathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.NFOScanPath.Remove(NFOPathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearNFOPath(object sender, RoutedEventArgs e)
        {
            vieModel.NFOScanPath.Clear();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Hide();
        }


        public void StartScan(object sender, RoutedEventArgs e)
        {

        }

        public void CopyPicToPath(string id, string path)
        {
            string fatherpath = new FileInfo(path).DirectoryName;
            string[] files = null;
            try
            {
                files = Directory.GetFiles(fatherpath, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Logger.LogE(e);
            }

            string ImageExt = "bmp;gif;ico;jpe;jpeg;jpg;png";
            List<string> ImageExtList = new List<string>(); foreach (var item in ImageExt.Split(';')) { ImageExtList.Add('.' + item); }

            //识别图片
            if (files != null)
            {
                var piclist = files.Where(s => ImageExtList.Contains(Path.GetExtension(s))).ToList();
                if (piclist.Count <= 0) return;
                foreach (var item in piclist)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (item.ToLower().IndexOf("poster") >= 0 || item.ToLower().IndexOf($"{id.ToLower()}_s") >= 0)
                        {
                            FileHelper.TryCopyFile(item, GlobalVariable.BasePicPath + $"SmallPic\\{id}.jpg", true);


                        }
                        else if (item.ToLower().IndexOf("fanart") >= 0 || item.ToLower().IndexOf($"{id.ToLower()}_b") >= 0)
                        {
                            FileHelper.TryCopyFile(item, GlobalVariable.BasePicPath + $"BigPic\\{id}.jpg", true);
                        }
                    }

                }




            }
        }



        private void DownloadMany(object sender, RoutedEventArgs e)
        {
            if (Running) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.OtherTaskIsRunning, GrowlToken); return; }
            if (IsDownLoading()) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, GrowlToken); return; }



        }



        private void InsertOneMovie(object sender, RoutedEventArgs e)
        {
            Window window = GetWindowByName("WindowEdit");
            WindowEdit windowEdit;
            if (window != null) { windowEdit = (WindowEdit)window; windowEdit.Close(); }
            windowEdit = new WindowEdit();
            windowEdit.Show();

        }

        private void CancelRun(object sender, RoutedEventArgs e)
        {
            if (!cts.IsCancellationRequested) cts.Cancel();
            LoadingStackPanel.Visibility = Visibility.Hidden;

            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Cancel, GrowlToken);
            Running = false;
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    if (!vieModel.ScanPath.Contains(dragdropFile) && !vieModel.ScanPath.IsIntersectWith(dragdropFile))
                    {
                        vieModel.ScanPath.Add(dragdropFile);
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                    }
                }
            }
        }

        private void AccessPathTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void AccessPathTextBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (IsFile(dragdropFile))
                {
                    if (new FileInfo(dragdropFile).Extension == ".mdb")
                    {
                        AccessPathTextBox.Text = dragdropFile;
                        break;
                    }
                }
            }
        }

        private void NFOPathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void NFOPathListBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    if (!vieModel.NFOScanPath.Contains(dragdropFile) && !vieModel.NFOScanPath.IsIntersectWith(dragdropFile))
                    {
                        vieModel.NFOScanPath.Add(dragdropFile);
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                    }
                }
            }
        }

        private void SingleNFOBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void SingleNFOBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (IsFile(dragdropFile))
                {
                    if (new FileInfo(dragdropFile).Extension == ".nfo")
                    {
                        NFOPathTextBox.Text = dragdropFile;
                        break;
                    }
                }
            }
        }

        private void EuropePathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void EuropePathListBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    if (!vieModel.ScanEuPath.Contains(dragdropFile) && !vieModel.ScanEuPath.IsIntersectWith(dragdropFile))
                    {
                        vieModel.ScanEuPath.Add(dragdropFile);
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.FilePathIntersection, GrowlToken);
                    }
                }
            }
        }

        private void UNCPathBorder_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void UNCPathBorder_DragOver(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    UNCPathTextBox.Text = dragdropFile;
                    break;
                }
            }
        }


        private void Jvedio_BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.ToolsIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void LoadingStackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (LoadingStackPanel.Visibility == Visibility.Visible)
            {
                TabControl.IsEnabled = false;
            }
            else
            {
                TabControl.IsEnabled = true;
            }
        }
    }

}
