using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalVariable;
using System.Data;
using System.Windows.Controls.Primitives;
using FontAwesome.WPF;
using System.ComponentModel;
using Jvedio.Utils;
using System.Threading;
using Jvedio.Style;
using Jvedio.Utils.Net;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class WindowBatch : BaseWindow
    {
        public VieModel_Batch vieModel;

        public CancellationTokenSource cts;
        public CancellationToken ct;

        //TODO
        //修复批量错误

        private bool running = false;
        public bool Running
        {
            get => running;
            set
            {
                running = value;
                if (running)
                    SetEnable(false);
                else
                    SetEnable(true);

                if (running) ProgressBar.Visibility = Visibility.Visible;
            }
        }





        public bool Pause = false;



        public WindowBatch()
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Cancel, "BatchGrowl"); });
            ct = cts.Token;
            ProgressBar.Visibility = Visibility.Hidden;
            TabControl.SelectedIndex = Properties.Settings.Default.BatchIndex;
        }

        private void ShowStatus(string str, bool newline = true)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    OutputPanel.Visibility = Visibility.Visible;
                    OutputPanel.AppendText(str, newline);
                    OutputPanel.ScrollToEnd();
                });
            }

        }


        private int ScreenShotNumber;
        public Semaphore SemaphoreScreenShot;
        public object lockobject = new object();




        public async Task<bool> ScreenShot(string id)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) return false;

                Movie movie = DataBase.SelectMovieByID(id);
                int SemaphoreNum = vieModel.ScreenShot_Num;
                string ScreenShotPath = "";
                ScreenShotPath = BasePicPath + "ScreenShot\\" + movie.id;

                if (!Directory.Exists(ScreenShotPath)) Directory.CreateDirectory(ScreenShotPath);



                string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
                if (cutoffArray.Length == 0) return false;
                SemaphoreScreenShot = new Semaphore(SemaphoreNum, SemaphoreNum);
                int total = cutoffArray.Count();
                ScreenShotNumber = 0;


                for (int i = 0; i < cutoffArray.Count(); i++)
                {
                    List<object> list = new List<object>() { cutoffArray[i], i.ToString(), movie.filepath, ScreenShotPath };
                    Thread threadObject = new Thread(BeginScreenShot);
                    threadObject.Start(list);
                }

                //等待直到所有线程完成
                while (ScreenShotNumber < total)
                {
                    Task.Delay(500).Wait();
                }
                return true;
            });

        }


        public void BeginScreenShot(object o)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                return;
            }

            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string i = list[1] as string;
            string filePath = list[2] as string;
            string ScreenShotPath = list[3] as string;

            SemaphoreScreenShot.WaitOne();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -threads 1 -ss {cutoffTime} -i \"{filePath}\" -f image2 -frames:v 1 \"{ScreenShotPath}\\ScreenShot-{i.PadLeft(2, '0')}.jpg\"";
            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            _ = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            lock (lockobject) { ScreenShotNumber++; }
            ShowStatus(">", false);
            SemaphoreScreenShot.Release();
        }












        private async Task<bool> GenGif(string id)
        {
            return await Task.Run(() =>
            {
                Movie movie = DataBase.SelectMovieByID(id);
                string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
                if (cutoffArray.Count() < 2) return false;
                string cutoffTime = cutoffArray[new Random().Next(1, cutoffArray.Count() - 1)];
                string filePath = movie.filepath;
                string GifSavePath = BasePicPath + "Gif\\";
                if (!Directory.Exists(GifSavePath)) Directory.CreateDirectory(GifSavePath);
                GifSavePath += id + ".gif";
                if (string.IsNullOrEmpty(cutoffTime)) return false;
                if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) return false;

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                int width = vieModel.Gif_Width;
                int height = vieModel.Gif_Height;

                string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -t {vieModel.Gif_Length} -ss {cutoffTime} -i \"{filePath}\" -s {width}x{height}  \"{GifSavePath}\"";
                Console.WriteLine(str);
                p.StandardInput.WriteLine(str + "&exit");
                p.StandardInput.AutoFlush = true;
                _ = p.StandardOutput.ReadToEnd();
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();

                return true;
            });
        }

        private void SetEnable(bool enable)
        {
            TabControl.IsEnabled = enable;
        }


        private async Task<bool> DownLoad(string id)
        {
            Movie movie = DataBase.SelectMovieByID(id);
            bool downloadinfo = vieModel.Info_ForceDownload || movie.IsToDownLoadInfo();
            if (!downloadinfo)
                ShowStatus($"{id}：{Jvedio.Language.Resources.Skip} {Jvedio.Language.Resources.SyncInfo}");



            //下载信息
            if (downloadinfo)
            {
                HttpResult httpResult = await MyNet.DownLoadFromNet(movie);
                if (httpResult != null && httpResult.Success)
                {
                    ShowStatus($"{id}：{Jvedio.Language.Resources.SyncInfo} {Jvedio.Language.Resources.Message_Success}");
                }
                else if (httpResult != null)
                {
                    string error = httpResult.Error != "" ? httpResult.Error : httpResult.StatusCode.ToStatusMessage();
                    ShowStatus($"{id}：{Jvedio.Language.Resources.SyncInfo}  {Jvedio.Language.Resources.Message_Fail} ，{Jvedio.Language.Resources.Reason} ：{error}");
                }
                else
                {
                    ShowStatus($"{id}：{Jvedio.Language.Resources.SyncInfo}  {Jvedio.Language.Resources.Message_Fail} ，{Jvedio.Language.Resources.Reason} ：{Jvedio.Language.Resources.HttpFail}");
                }
                Task.Delay(vieModel.Timeout_Medium).Wait();
            }






            //加载预览图
            movie = DataBase.SelectMovieByID(id);
            List<string> extraImageList = new List<string>();
            if (!string.IsNullOrEmpty(movie.extraimageurl) && movie.extraimageurl.IndexOf(";") > 0)
            {
                extraImageList = movie.extraimageurl.Split(';').ToList().Where(arg => !string.IsNullOrEmpty(arg) && arg.IndexOf("http") >= 0).ToList();
            }

            if (CheckPause()) return false;

            bool success = false;
            string resultMessage = "";
            //同步缩略图
            if (vieModel.DownloadSmallPic)
            {
                string path = Path.Combine(BasePicPath, "SmallPic", movie.id + ".jpg");
                if (!File.Exists(path))
                {
                    (success, resultMessage) = await MyNet.DownLoadImage(movie.smallimageurl, ImageType.SmallImage, movie.id);
                    ShowStatus($"{Jvedio.Language.Resources.Download} {Jvedio.Language.Resources.Thumbnail}：{(success ? Jvedio.Language.Resources.Message_Success : Jvedio.Language.Resources.Message_Fail)}");
                    if (success) Task.Delay(vieModel.Timeout_Medium).Wait();
                }
                else
                {
                    ShowStatus($"{Jvedio.Language.Resources.Thumbnail} {Jvedio.Language.Resources.Message_AlreadyExist} {Jvedio.Language.Resources.Skip}");
                }
            }
            if (CheckPause()) return false;

            //同步海报图
            if (vieModel.DownloadBigPic)
            {
                string path = Path.Combine(BasePicPath, "SmallPic", movie.id + ".jpg");
                if (!File.Exists(path))
                {

                    (success, resultMessage) = await MyNet.DownLoadImage(movie.bigimageurl, ImageType.BigImage, movie.id);
                    ShowStatus($"{Jvedio.Language.Resources.Download} {Jvedio.Language.Resources.Poster}：{(success ? Jvedio.Language.Resources.Message_Success : Jvedio.Language.Resources.Message_Fail)}");
                    if (success) Task.Delay(vieModel.Timeout_Medium).Wait();
                }
                else
                {
                    ShowStatus($"{Jvedio.Language.Resources.Poster} {Jvedio.Language.Resources.Message_AlreadyExist} {Jvedio.Language.Resources.Skip}");
                }
            }
            if (CheckPause()) return false;

            //同步预览图
            if (vieModel.DownloadExtraPic)
            {
                string cookies = "";
                bool extraImageSuccess = false;
                string filepath = "";

                for (int i = 0; i < extraImageList.Count; i++)
                {
                    if (CheckPause()) return false;
                    if (extraImageList[i].Length > 0)
                    {
                        filepath = Path.Combine(BasePicPath, "ExtraPic", movie.id, Path.GetFileName(new Uri(extraImageList[i]).LocalPath));
                        if (!File.Exists(filepath))
                        {
                            (extraImageSuccess, cookies) = await Task.Run(() => { return MyNet.DownLoadImage(extraImageList[i], ImageType.ExtraImage, movie.id, Cookie: cookies); });
                            if (extraImageSuccess)
                                ShowStatus($"{Jvedio.Language.Resources.Download} {Jvedio.Language.Resources.Preview} {Jvedio.Language.Resources.Message_Success} {i + 1}/{extraImageList.Count}");
                            else
                                ShowStatus($"{Jvedio.Language.Resources.Download} {Jvedio.Language.Resources.Preview} {Jvedio.Language.Resources.Message_Fail} {i + 1}/{extraImageList.Count}");
                            Task.Delay(vieModel.Timeout_Medium).Wait();
                        }
                        else
                        {
                            ShowStatus($"{Jvedio.Language.Resources.Message_AlreadyExist} {Jvedio.Language.Resources.Skip}  {i + 1}/{extraImageList.Count}");
                        }
                    }
                }

            }
            vieModel.CurrentNum += 1;
            return true;
        }

        private void ShowResultMessage(bool result, int i, string message = "")
        {
            if (result)
                ShowStatus($"{i + 1}/{vieModel.TotalNum} => {Jvedio.Language.Resources.Message_Success}");
            else
            {
                if (message != "")
                {
                    ShowStatus($"{i + 1}/{vieModel.TotalNum} =>  {Jvedio.Language.Resources.Message_Fail}  {Jvedio.Language.Resources.Reason} ：{message}");
                }
                else
                {
                    ShowStatus($"{i + 1}/{vieModel.TotalNum} => {Jvedio.Language.Resources.Message_Fail}");
                }
            }

            vieModel.CurrentNum += 1;
        }

        private bool CheckPause()
        {
            try
            {
                if (Pause)
                {
                    ShowStatus($"{Jvedio.Language.Resources.Pause}……");
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        WaitingPanel.Visibility = Visibility.Collapsed;
                    });
                }
                while (Pause)
                {
                    Task.Delay(500).Wait();
                }
                ct.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
                //ShowStatus($"------------{Jvedio.Language.Resources.Cancel}------------");
                App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });
                return true;
            }
            return false;
        }




        private async void SyncInfo()
        {
            ShowStatus($"------------{Jvedio.Language.Resources.BeginAsync}------------");
            await Task.Run(async () =>
           {
               //单个单个下载
               for (int i = 0; i < vieModel.TotalNum; i++)
               {
                   if (CheckPause()) break;
                   await DownLoad(vieModel.Movies[i]);

               }
           }, ct);
            ShowStatus($"------------{Jvedio.Language.Resources.Complete}------------");
            cts.Dispose();
            Running = false;
        }


        private async void GenerateGif()
        {
            ShowStatus($"------------{Jvedio.Language.Resources.Batch_Gif}------------");
            await Task.Run(async () =>
            {
                for (int i = 0; i < vieModel.TotalNum; i++)
                {
                    if (CheckPause()) break;
                    bool result = await GenGif(vieModel.Movies[i]);
                    string message = "";
                    if (!result) message = Jvedio.Language.Resources.NoScreenShotDuration;
                    ShowResultMessage(result, i, message);
                }
            }, ct);
            ShowStatus($"------------{Jvedio.Language.Resources.Complete}------------");
            cts.Dispose();
            Running = false;
        }

        private async void GenerateScreenShot()
        {
            ShowStatus($"------------{Jvedio.Language.Resources.ScreenShot}------------");
            await Task.Run(async () =>
            {
                for (int i = 0; i < vieModel.TotalNum; i++)
                {
                    if (CheckPause()) break;
                    bool result = await ScreenShot(vieModel.Movies[i]);
                    string message = "";
                    if (!result) message = Jvedio.Language.Resources.NoScreenShotDuration;
                    ShowResultMessage(result, i, message);

                }
            }, ct);
            ShowStatus($"------------{Jvedio.Language.Resources.Complete}------------");
            cts.Dispose();
            Running = false;
        }


        private async void Rename()
        {
            if (Properties.Settings.Default.RenameFormat.IndexOf("{") < 0)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetRenameRule);
                cts.Dispose();
                Running = false;
                return;
            }

            ShowStatus($"------------{Jvedio.Language.Resources.Rename}------------");
            await Task.Run(() =>
            {
                for (int i = 0; i < vieModel.TotalNum; i++)
                {
                    if (CheckPause()) break;
                    (bool result, string message) = Rename(vieModel.Movies[i]);
                    ShowResultMessage(result, i, message);
                }
            }, ct);
            ShowStatus($"------------{Jvedio.Language.Resources.Complete}------------");
            cts.Dispose();
            Running = false;
        }



        private async void StartTask(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.OtherTaskIsRunning, "BatchGrowl");
                return;
            }

            int idx = TabControl.SelectedIndex;

            if ((idx == 1 || idx == 2) && !File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetFFmpeg, "BatchGrowl");
                return;
            }

            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_CancelCurrentTask, "BatchGrowl"); });
            ct = cts.Token;


            Running = true;

            OutputPanel.Clear();

            PauseButton.Content = Jvedio.Language.Resources.Pause;
            vieModel.TotalNum = 0;
            vieModel.CurrentNum = 0;
            vieModel.Progress = 0;

            bool success = await ResetTask(cts);
            if (success && Running && vieModel.Movies != null && vieModel.TotalNum != 0)
            {
                WaitingPanel.Visibility = Visibility.Hidden;
                PauseButton.IsEnabled = true;
                if (idx == 0)
                {
                    SyncInfo();
                }
                else if (idx == 1)
                {
                    GenerateGif();
                }

                else if (idx == 2)
                {
                    GenerateScreenShot();
                }

                else if (idx == 3)
                {
                    Rename();
                }
            }
            else
            {
                Running = false;

            }
        }



        private (bool, string) Rename(string id)
        {
            DetailMovie detailMovie = DataBase.SelectDetailMovieById(id);

            if (!File.Exists(detailMovie.filepath)) return (false, $"{Jvedio.Language.Resources.Message_FileNotExist} {detailMovie.filepath}");
            DetailMovie movie = DataBase.SelectDetailMovieById(id);
            string[] newPath = movie.ToFileName();
            if (movie.hassubsection)
            {
                for (int i = 0; i < newPath.Length; i++)
                {
                    if (File.Exists(newPath[i])) return (false, $"{Jvedio.Language.Resources.NewFileExists}：{newPath[i]}，{Jvedio.Language.Resources.SourceFile}：{detailMovie.filepath}");
                }

                for (int i = 0; i < newPath.Length; i++)
                {
                    try
                    {
                        File.Move(movie.subsectionlist[i], newPath[i]);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogF(ex);
                        continue;
                    }
                }
                movie.filepath = newPath[0];
                movie.subsection = string.Join(";", newPath);
                DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                DataBase.UpdateMovieByID(movie.id, "subsection", movie.subsection, "string");//保存

            }
            else
            {
                if (File.Exists(newPath[0])) return (false, $"{Jvedio.Language.Resources.NewFileExists}：{newPath[0]}，{Jvedio.Language.Resources.NewFileExists}：{detailMovie.filepath}");
                File.Move(movie.filepath, newPath[0]);
                movie.filepath = newPath[0];
                DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
            }
            return (true, "");
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (Pause)
            {
                Pause = false;
                button.Content = Jvedio.Language.Resources.Pause;
                WaitingPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                Pause = true;
                button.Content = Jvedio.Language.Resources.Continue;
                WaitingPanel.Visibility = Visibility.Visible;
            }




        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                Pause = false;
                try
                {
                    cts?.Cancel();
                    WaitingPanel.Visibility = Visibility.Visible;
                }
                catch { }
                PauseButton.IsEnabled = false;
                WaitingPanel.Visibility = Visibility.Hidden;
                Running = false;
            }
        }

        private void Jvedio_BaseWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Running) { cts.Cancel(); }
            Properties.Settings.Default.BatchIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private async Task<bool> ResetTask(CancellationTokenSource cts = null)
        {
            int idx = TabControl.SelectedIndex;
            WaitingPanel.Visibility = Visibility.Visible;
            return await Task.Run(() =>
            {
                return vieModel.Reset(idx, (message) =>
                {
                    Dispatcher.BeginInvoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });
                }, cts);
            });
        }


        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            OutputPanel.Clear();
        }


        private void Jvedio_BaseWindow_ContentRendered(object sender, EventArgs e)
        {

            vieModel = new VieModel_Batch();
            this.DataContext = vieModel;
        }



        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.TextDecorations = TextDecorations.Underline;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.TextDecorations = null;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Settings settings = (Settings)Jvedio.FileProcess.GetWindowByName("Settings");
            if (settings == null) settings = new Settings();
            settings.Show();
            settings.Activate();

            if (TabControl.SelectedIndex == 1)
            {
                settings.TabControl.SelectedIndex = 7;
            }
            else if (TabControl.SelectedIndex == 2)
            {
                settings.TabControl.SelectedIndex = 7;
            }
            else if (TabControl.SelectedIndex == 3)
            {
                settings.TabControl.SelectedIndex = 9;
            }

        }
    }



}
