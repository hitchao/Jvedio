using Jvedio.Core.Exceptions;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
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
using static Jvedio.App;

namespace Jvedio.Core.FFmpeg
{
    public class ScreenShot
    {
        private const int MAX_THREAD_NUM = 10;
        private const int DEFAULT_THREAD_NUM = 1;
        private const int DEFAULT_GIF_WIDTH = 280;
        private const int DEFAULT_GIF_HEIGHT = 170;
        private const int DEFAULT_DURATION = 3;

        #region "事件"


        public event EventHandler onProgress;
        public event EventHandler onError;


        #endregion

        #region "属性"

        private object ErrorLock { get; set; } = new object();

        private Video CurrentVideo { get; set; }

        private int TotalCount { get; set; } = (int)ConfigManager.FFmpegConfig.ScreenShotNum;
        private int TimeOut { get; set; } = (int)ConfigManager.FFmpegConfig.TimeOut;
        private string FFmpegPath { get; set; } = ConfigManager.FFmpegConfig.Path;
        private bool SkipExistScreenShot { get; set; } = ConfigManager.FFmpegConfig.SkipExistScreenShot;

        private List<string> saveFileNames { get; set; } = new List<string>();

        public int TotalTaskCount { get; set; }

        public int CurrentTaskCount { get; set; }

        private object TaskCountLock = new object();

        private StringBuilder outputs { get; set; } = new StringBuilder();

        private CancellationToken Token { get; set; }

        #endregion
        public ScreenShot(Video video, CancellationToken token)
        {
            CurrentVideo = video;
            Token = token;
        }


        public async Task<string> AsyncScreenShot()
        {
            if (!File.Exists(FFmpegPath))
                throw new NotFoundException("ffmpeg.exe");
            string originPath = CurrentVideo.Path;
            if (!File.Exists(originPath))
                throw new NotFoundException(originPath);

            // 获得需要截图的视频进度
            string[] cutoffArray =
                MediaParse.GetCutOffArray(originPath,
                ConfigManager.FFmpegConfig.ScreenShotNum,
                ConfigManager.FFmpegConfig.ScreenShotIgnoreStart,
                ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd);

            if (cutoffArray == null || cutoffArray.Length == 0)
                throw new MediaCutOutOfRangeException();

            int threadNum = (int)ConfigManager.FFmpegConfig.ThreadNum; // 截图线程
            if (threadNum > MAX_THREAD_NUM || threadNum <= 0)
                threadNum = DEFAULT_THREAD_NUM;

            string outputDir = CurrentVideo.GetScreenShot();
            if (SkipExistScreenShot && Directory.Exists(outputDir)) {
                outputs.Append($"{LangManager.GetValueByKey("SkipScreenShotForDirExists")} => {outputDir}");
                return outputs.ToString();
            }

            DirHelper.TryCreateDirectory(outputDir, (ex) => {
                throw new DirCreateFailedException(outputDir);
            });

            // 生成截图命令
            List<string> ffmpegParams = new List<string>();
            for (int i = 0; i < cutoffArray.Count(); i++) {
                string saveFileName = Path.Combine(outputDir, $"ScreenShot-{i.ToString().PadLeft(2, '0')}.jpg");
                saveFileNames.Add(saveFileName);
                string cutoffTime = cutoffArray[i];
                if (string.IsNullOrEmpty(cutoffTime))
                    continue;
                string ffmpegParam = $"-y -threads 1 -ss {cutoffTime} -i \"{originPath}\" -f image2 -frames:v 1 \"{saveFileName}\"";
                ffmpegParams.Add(ffmpegParam);
            }

            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine();
            cmd.AppendLine();
            cmd.Append($"#### ffmpeg commands ####{Environment.NewLine}");
            foreach (var item in ffmpegParams)
                cmd.Append($"ffmpeg {item}{Environment.NewLine}");
            cmd.AppendLine();
            cmd.AppendLine();
            outputs.Append(cmd.ToString());

            // 放到线程池里运行
            TotalTaskCount = ffmpegParams.Count;
            ThreadPool.SetMaxThreads(threadNum, threadNum);
            for (int i = 0; i < TotalTaskCount; i++) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RunFFmpeg), ffmpegParams[i]);
            }

            // 等待所有任务完成
            while (CurrentTaskCount < TotalTaskCount) {
                await Task.Delay(50);
                Console.WriteLine("wait for screen done");
                if (Token.IsCancellationRequested)
                    break;
            }

            return outputs.ToString();
        }

        public void RunFFmpeg(object ffmpegParam)
        {
            StringBuilder currentOutput = new StringBuilder();
            Process process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = FFmpegPath,
                    Arguments = ffmpegParam.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    RedirectStandardError = true,
                },
                EnableRaisingEvents = true,
            };
            try {
                Logger.Info($"run ffmpeg cmd: {ffmpegParam}");
                process.Start();
                string processOutput = string.Empty;
                while ((processOutput = process.StandardError.ReadLine()) != null) {
                    currentOutput.Append(processOutput);
                    currentOutput.AppendLine();
                }

                if (Token.IsCancellationRequested)
                    throw new TaskCanceledException();
            } catch (Exception ex) {
                lock (ErrorLock) {
                    onError?.Invoke(this, new MessageCallBackEventArgs(ex.Message));
                }
            } finally {
                process.Dispose();
                lock (TaskCountLock) {
                    CurrentTaskCount++;
                    onProgress?.Invoke(this, null);
                    outputs.Append(currentOutput.ToString());
                }
            }
        }

        public async Task<string> AsyncGenerateGif()
        {
            if (!File.Exists(FFmpegPath))
                throw new NotFoundException("ffmpeg.exe");
            string originPath = CurrentVideo.Path;
            if (!File.Exists(originPath))
                throw new NotFoundException(originPath);

            // 获得需要截图的视频进度
            string[] cutoffArray =
                MediaParse.GetCutOffArray(originPath,
                ConfigManager.FFmpegConfig.ScreenShotNum,
                ConfigManager.FFmpegConfig.ScreenShotIgnoreStart,
                ConfigManager.FFmpegConfig.ScreenShotIgnoreEnd);

            if (cutoffArray.Length == 0)
                throw new MediaCutOutOfRangeException();

            string saveFileName = CurrentVideo.GetGifPath();
            if (string.IsNullOrEmpty(saveFileName))
                throw new NotFoundException(saveFileName);

            if (ConfigManager.FFmpegConfig.SkipExistGif && File.Exists(saveFileName)) {
                outputs.Append($"{LangManager.GetValueByKey("SkipGif")} {saveFileName}");
                return outputs.ToString();
            }

            string outputDir = Path.GetDirectoryName(saveFileName);
            DirHelper.TryCreateDirectory(outputDir, (ex) => {
                throw new DirCreateFailedException(outputDir);
            });

            string time = cutoffArray[new Random().Next(cutoffArray.Length - 1)];
            if (string.IsNullOrEmpty(time))
                throw new MediaCutOutOfRangeException();

            int duration = (int)ConfigManager.FFmpegConfig.GifDuration;
            int width = (int)ConfigManager.FFmpegConfig.GifWidth;
            int height = (int)ConfigManager.FFmpegConfig.GifHeight;
            if (width <= 0)
                width = DEFAULT_GIF_WIDTH;

            if (ConfigManager.FFmpegConfig.GifAutoHeight) {
                (double w, double h) = MediaParse.GetWidthHeight(originPath);
                if (w != 0)
                    height = (int)(h / w * (double)width);
            }

            if (width <= 0)
                width = DEFAULT_GIF_WIDTH;
            if (height <= 0)
                height = DEFAULT_GIF_HEIGHT;
            if (duration <= 0)
                duration = DEFAULT_DURATION;

            string ffmpegParam = $"-y -t {duration} -ss {time} -i \"{originPath}\" -s {width}x{height}  \"{saveFileName}\"";
            TotalCount = 1;

            outputs.Append($"{Environment.NewLine}#### ffmpeg commands ####{Environment.NewLine}");
            outputs.Append($"ffmpeg {ffmpegParam}{Environment.NewLine}");
            outputs.Append($"{Environment.NewLine}{Environment.NewLine}");

            RunFFmpeg(ffmpegParam);
            while (CurrentTaskCount < TotalTaskCount) {
                await Task.Delay(50);
                Console.WriteLine("等待 gif 生成");
                if (Token.IsCancellationRequested)
                    break;
            }

            return outputs.ToString();
        }
    }
}
