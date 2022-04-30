using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Exceptions;
using Jvedio.Entity;
using Jvedio.Utils.ImageAndVideo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalVariable;

namespace Jvedio.Core.FFmpeg
{

    //TODO 线程池
    public class ScreenShot
    {

        private static int MAX_THREAD_NUM = 10;

        private object ScreenShotLockObject = 0;

        private Video CurrentVideo { get; set; }

        private int TotalCount = (int)GlobalConfig.FFmpegConfig.ScreenShotNum;
        private int TimeOut = (int)GlobalConfig.FFmpegConfig.TimeOut;
        private string FFmpegPath = GlobalConfig.FFmpegConfig.Path;
        private bool SkipExistScreenShot = GlobalConfig.FFmpegConfig.SkipExistScreenShot;

        private List<string> saveFileNames = new List<string>();


        public int TotalTaskCount { get; set; }
        public int CurrentTaskCount { get; set; }
        private object TaskCountLock = new object();

        private StringBuilder outputs = new StringBuilder();

        private CancellationToken Token;

        public ScreenShot(Video video, CancellationToken token)
        {
            CurrentVideo = video;
            Token = token;
        }



        public async void BeginScreenShot()
        {

            //SingleScreenShotCompleted?.Invoke(this, new ScreenShotEventArgs(str, output, error));
            //lock (ScreenShotLockObject) { ScreenShotCurrent += 1; }
        }



        public event EventHandler onProgress;
        public event EventHandler onError;
        private object ErrorLock = new object();



        public async Task<string> AsyncScreenShot()
        {
            if (!File.Exists(FFmpegPath))
                throw new NotFoundException(FFmpegPath);

            string[] cutoffArray = MediaParse.GetCutOffArray(CurrentVideo.Path); //获得需要截图的视频进度
            if (cutoffArray.Length == 0)
                throw new MediaCutOutOfRangeException();
            int threadNum = (int)GlobalConfig.FFmpegConfig.ThreadNum;// 截图线程
            if (threadNum > MAX_THREAD_NUM || threadNum <= 0) threadNum = 1;

            string outputDir = CurrentVideo.getScreenShot();
            if (SkipExistScreenShot && Directory.Exists(outputDir))
            {
                outputs.Append($"跳过截图，因为文件夹存在（如需关闭，请在设置中修改） => {outputDir}");
                return outputs.ToString();
            }
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);



            // 生成截图命令
            List<string> ffmpegParams = new List<string>();
            string originPath = CurrentVideo.Path;
            for (int i = 0; i < cutoffArray.Count(); i++)
            {
                string saveFileName = Path.Combine(outputDir, $"ScreenShot-{i.ToString().PadLeft(2, '0')}.jpg");
                saveFileNames.Add(saveFileName);
                string cutoffTime = cutoffArray[i];
                if (string.IsNullOrEmpty(cutoffTime)) continue;
                //string command = $"\"{ffmpegPath}\" -y -threads 1 -ss {cutoffTime} -i \"{originPath}\" -f image2 -frames:v 1 \"{saveFileName}\"";
                string ffmpegParam = $"-y -threads 1 -ss {cutoffTime} -i \"{originPath}\" -f image2 -frames:v 1 \"{saveFileName}\"";
                ffmpegParams.Add(ffmpegParam);
            }

            // 放到线程池里运行
            TotalTaskCount = ffmpegParams.Count;
            ThreadPool.SetMaxThreads(threadNum, threadNum);
            for (int i = 0; i < TotalTaskCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RunFFmpeg), ffmpegParams[i]);
            }

            // 等待所有任务完成
            while (CurrentTaskCount < TotalTaskCount)
            {
                await Task.Delay(50);
                Console.WriteLine("等待截图完成");
                if (Token.IsCancellationRequested) break;
            }
            return outputs.ToString();
        }


        public void RunFFmpeg(object arg)
        {
            string ffmpegParam = arg.ToString();
            StringBuilder currentOutput = new StringBuilder();
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFmpegPath,
                    Arguments = ffmpegParam,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,

                    RedirectStandardError = true
                },
                EnableRaisingEvents = true

            };
            try
            {

                process.Start();
                string processOutput = "";
                while ((processOutput = process.StandardError.ReadLine()) != null)
                {
                    currentOutput.Append(processOutput);
                    currentOutput.AppendLine();
                }
                if (Token.IsCancellationRequested)
                    throw new TaskCanceledException();
            }
            catch (Exception ex)
            {
                lock (ErrorLock)
                {
                    onError?.Invoke(this, new MessageCallBackEventArgs(ex.Message));
                }
            }
            finally
            {
                process.Dispose();
                lock (TaskCountLock)
                {
                    CurrentTaskCount++;
                    onProgress?.Invoke(this, null);
                    outputs.Append(currentOutput.ToString());
                }
            }


            //if (token.IsCancellationRequested)
            //    break;

        }





        public async Task<string> AsyncGenrateGif()
        {
            if (!File.Exists(FFmpegPath))
                throw new NotFoundException(FFmpegPath);

            string[] cutoffArray = MediaParse.GetCutOffArray(CurrentVideo.Path); //获得需要截图的视频进度
            if (cutoffArray.Length == 0)
                throw new MediaCutOutOfRangeException();
            string filePath = CurrentVideo.Path;
            string saveFileName = CurrentVideo.getGifPath();

            if (GlobalConfig.FFmpegConfig.SkipExistGif && File.Exists(saveFileName))
            {
                outputs.Append($"跳过已截取的 GIF： {saveFileName}");
                return outputs.ToString();
            }


            string outputDir = Path.GetDirectoryName(saveFileName);
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string cutofftime = cutoffArray[new Random().Next(cutoffArray.Length - 1)];
            if (string.IsNullOrEmpty(cutofftime))
                throw new MediaCutOutOfRangeException();

            int duration = (int)GlobalConfig.FFmpegConfig.GifDuration;
            int width = (int)GlobalConfig.FFmpegConfig.GifWidth;
            int height = (int)GlobalConfig.FFmpegConfig.GifHeight;

            if (GlobalConfig.FFmpegConfig.GifAutoHeight)
            {
                (double w, double h) = MediaParse.GetWidthHeight(filePath);
                if (w != 0) height = (int)(h / w * (double)width);
            }

            if (width <= 0) width = 280;
            if (height <= 0) height = 170;


            string command = $"-y -t {duration} -ss {cutofftime} -i \"{filePath}\" -s {width}x{height}  \"{saveFileName}\"";
            TotalCount = 1;
            RunFFmpeg(command);
            while (CurrentTaskCount < TotalTaskCount)
            {
                await Task.Delay(50);
                Console.WriteLine("等待 gif 生成");
                if (Token.IsCancellationRequested) break;
            }
            return outputs.ToString();
        }
    }


    public class ScreenShotEventArgs : EventArgs
    {
        public string FFmpegCommand = "";
        public string FilePath = "";
        public string Error = "";

        public ScreenShotEventArgs(string _FFmpegCommand, string filepath, string error = "")
        {
            FFmpegCommand = _FFmpegCommand;
            FilePath = filepath;
            Error = error;
        }
    }
}
