using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Exceptions;
using Jvedio.Entity;
using SuperUtils.IO;
using SuperUtils.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.FFmpeg
{

    //TODO 线程池
    public class ScreenShot
    {
        private const int MAX_THREAD_NUM = 10;
        private const int DEFAULT_THREAD_NUM = 1;
        private const int DEFAULT_GIF_WIDTH = 280;
        private const int DEFAULT_GIF_HEIGHT = 170;
        private const int DEFAULT_DURATION = 3;
        private object ScreenShotLockObject = new object();

        private Video CurrentVideo { get; set; }

        private int TotalCount = (int)ConfigManager.FFmpegConfig.ScreenShotNum;
        private int TimeOut = (int)ConfigManager.FFmpegConfig.TimeOut;
        private string FFmpegPath = ConfigManager.FFmpegConfig.Path;
        private bool SkipExistScreenShot = ConfigManager.FFmpegConfig.SkipExistScreenShot;

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


        public event EventHandler onProgress;

        public event EventHandler onError;

        private object ErrorLock = new object();



        public async Task<string> AsyncScreenShot()
        {
            if (!File.Exists(FFmpegPath))
                throw new NotFoundException("ffmpeg.exe");
            string originPath = CurrentVideo.Path;
            if (!File.Exists(originPath))
                throw new NotFoundException(originPath);
            string[] cutoffArray = MediaParse.GetCutOffArray(originPath, ConfigManager.FFmpegConfig.ScreenShotNum, ConfigManager.FFmpegConfig.ScreenShotIgnoreStart, ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd); //获得需要截图的视频进度
            if (cutoffArray.Length == 0)
                throw new MediaCutOutOfRangeException();
            int threadNum = (int)ConfigManager.FFmpegConfig.ThreadNum;// 截图线程
            if (threadNum > MAX_THREAD_NUM || threadNum <= 0) threadNum = DEFAULT_THREAD_NUM;

            string outputDir = CurrentVideo.getScreenShot();
            if (SkipExistScreenShot && Directory.Exists(outputDir))
            {
                outputs.Append($"跳过截图，因为文件夹存在（可在设置中关闭） => {outputDir}");
                return outputs.ToString();
            }

            DirHelper.TryCreateDirectory(outputDir, (ex) =>
            {
                throw new DirCreateFailedException(outputDir);
            });

            // 生成截图命令
            List<string> ffmpegParams = new List<string>();
            for (int i = 0; i < cutoffArray.Count(); i++)
            {
                string saveFileName = Path.Combine(outputDir, $"ScreenShot-{i.ToString().PadLeft(2, '0')}.jpg");
                saveFileNames.Add(saveFileName);
                string cutoffTime = cutoffArray[i];
                if (string.IsNullOrEmpty(cutoffTime)) continue;
                string ffmpegParam = $"-y -threads 1 -ss {cutoffTime} -i \"{originPath}\" -f image2 -frames:v 1 \"{saveFileName}\"";
                ffmpegParams.Add(ffmpegParam);
            }

            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine();
            cmd.AppendLine();
            cmd.Append($"#### ffmpeg commads ####{Environment.NewLine}");
            foreach (var item in ffmpegParams)
                cmd.Append($"ffmpeg {item}{Environment.NewLine}");
            cmd.AppendLine();
            cmd.AppendLine();
            outputs.Append(cmd.ToString());

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


        public void RunFFmpeg(object ffmpegParam)
        {
            StringBuilder currentOutput = new StringBuilder();
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFmpegPath,
                    Arguments = ffmpegParam.ToString(),
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
        }





        public async Task<string> AsyncGenrateGif()
        {
            if (!File.Exists(FFmpegPath))
                throw new NotFoundException("ffmpeg.exe");
            string originPath = CurrentVideo.Path;
            if (!File.Exists(originPath))
                throw new NotFoundException(originPath);

            string[] cutoffArray = MediaParse.GetCutOffArray(originPath, ConfigManager.FFmpegConfig.ScreenShotNum, ConfigManager.FFmpegConfig.ScreenShotIgnoreStart, ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd); //获得需要截图的视频进度
            if (cutoffArray.Length == 0)
                throw new MediaCutOutOfRangeException();


            string saveFileName = CurrentVideo.getGifPath();
            if (string.IsNullOrEmpty(saveFileName))
                throw new NotFoundException(saveFileName);

            if (ConfigManager.FFmpegConfig.SkipExistGif && File.Exists(saveFileName))
            {
                outputs.Append($"跳过已截取的 GIF： {saveFileName}");
                return outputs.ToString();
            }

            string outputDir = Path.GetDirectoryName(saveFileName);
            DirHelper.TryCreateDirectory(outputDir, (ex) =>
            {
                throw new DirCreateFailedException(outputDir);
            });

            string cutofftime = cutoffArray[new Random().Next(cutoffArray.Length - 1)];
            if (string.IsNullOrEmpty(cutofftime))
                throw new MediaCutOutOfRangeException();

            int duration = (int)ConfigManager.FFmpegConfig.GifDuration;
            int width = (int)ConfigManager.FFmpegConfig.GifWidth;
            int height = (int)ConfigManager.FFmpegConfig.GifHeight;
            if (width <= 0) width = DEFAULT_GIF_WIDTH;


            if (ConfigManager.FFmpegConfig.GifAutoHeight)
            {
                (double w, double h) = MediaParse.GetWidthHeight(originPath);
                if (w != 0) height = (int)(h / w * (double)width);
            }

            if (width <= 0) width = DEFAULT_GIF_WIDTH;
            if (height <= 0) height = DEFAULT_GIF_HEIGHT;
            if (duration <= 0) duration = DEFAULT_DURATION;

            string ffmpegParam = $"-y -t {duration} -ss {cutofftime} -i \"{originPath}\" -s {width}x{height}  \"{saveFileName}\"";
            TotalCount = 1;

            outputs.Append($"{Environment.NewLine}#### ffmpeg commads ####{Environment.NewLine}");
            outputs.Append($"ffmpeg {ffmpegParam}{Environment.NewLine}");
            outputs.Append($"{Environment.NewLine}{Environment.NewLine}");

            RunFFmpeg(ffmpegParam);
            while (CurrentTaskCount < TotalTaskCount)
            {
                await Task.Delay(50);
                Console.WriteLine("等待 gif 生成");
                if (Token.IsCancellationRequested) break;
            }
            return outputs.ToString();
        }
    }
}
