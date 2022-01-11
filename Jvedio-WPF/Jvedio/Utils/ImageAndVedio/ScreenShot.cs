using Jvedio.Utils.ImageAndVedio;
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

namespace Jvedio
{

    //TODO 线程池
    public class ScreenShot
    {
        public event EventHandler SingleScreenShotCompleted;
        private Semaphore SemaphoreScreenShot;
        private int ScreenShotCurrent = 0;
        private object ScreenShotLockObject = 0;
        public async void BeginScreenShot(object o)
        {
            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string i = list[1] as string;
            string filePath = list[2] as string;
            string ScreenShotPath = list[3] as string;
            string output = $"{ScreenShotPath}\\ScreenShot-{i.PadLeft(2, '0')}.jpg";

            if (string.IsNullOrEmpty(cutoffTime)) return;
            SemaphoreScreenShot.WaitOne();

            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -threads 1 -ss {cutoffTime} -i \"{filePath}\" -f image2 -frames:v 1 \"{output}\"";
            string error = await new FFmpegHelper(str).Run();
            SemaphoreScreenShot.Release();
            SingleScreenShotCompleted?.Invoke(this, new ScreenShotEventArgs(str, output, error));
            lock (ScreenShotLockObject) { ScreenShotCurrent += 1; }
        }


        public async Task<bool> BeginGenGif(object o)
        {
            return await Task.Run(async () =>
            {
                object[] list = o as object[];
                string cutoffTime = list[0] as string;
                string filePath = list[1] as string;
                string GifPath = list[2] as string;

                int duration = Properties.Settings.Default.Gif_Duration;

                int width = Properties.Settings.Default.Gif_Width;
                int height = Properties.Settings.Default.Gif_Height;
                if (Properties.Settings.Default.Gif_AutoHeight)
                {
                    (double w, double h) = MediaParse.GetWidthHeight(filePath);
                    if (w != 0) height = (int)(h / w * (double)width);
                }



                if (width <= 0 || width > 1980) width = 280;
                if (height <= 0 || height > 1080) height = 170;

                if (string.IsNullOrEmpty(cutoffTime)) return false;
                string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -t {duration} -ss {cutoffTime} -i \"{filePath}\" -s {width}x{height}  \"{GifPath}\"";
                string error = await new FFmpegHelper(str, duration * 5).Run();
                SingleScreenShotCompleted?.Invoke(this, new ScreenShotEventArgs(str, GifPath, error));
                return true;
            });
        }




        public async Task<(bool, string)> AsyncScreenShot(Movie movie)
        {
            bool result = true;
            string message = "";
            List<string> outputPath = new List<string>();
            await Task.Run(() =>
            {

                if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { result = false; message = Jvedio.Language.Resources.Message_SetFFmpeg; return; }

                int num = Properties.Settings.Default.ScreenShot_ThreadNum;// n 个线程截图
                string ScreenShotPath = "";
                ScreenShotPath = BasePicPath + "ScreenShot\\" + movie.id;

                if (!Directory.Exists(ScreenShotPath)) Directory.CreateDirectory(ScreenShotPath);

                string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
                if (cutoffArray.Length == 0) { result = false; message = Jvedio.Language.Resources.FailToCutOffVideo; return; }
                if (!File.Exists(movie.filepath)) { result = false; message = Jvedio.Language.Resources.NotExists; return; }
                int SemaphoreNum = cutoffArray.Length > 10 ? 10 : cutoffArray.Length;//最多 10 个线程截图
                SemaphoreScreenShot = new Semaphore(SemaphoreNum, SemaphoreNum);



                ScreenShotCurrent = 0;
                int ScreenShotTotal = cutoffArray.Count();
                ScreenShotLockObject = new object();

                for (int i = 0; i < cutoffArray.Count(); i++)
                {
                    outputPath.Add($"{ScreenShotPath}\\ScreenShot-{i.ToString().PadLeft(2, '0')}.jpg");
                    List<object> list = new List<object>() { cutoffArray[i], i.ToString(), movie.filepath, ScreenShotPath };
                    Thread threadObject = new Thread(BeginScreenShot);
                    threadObject.Start(list);
                }

                //等待直到所有线程结束
                while (ScreenShotCurrent != ScreenShotTotal)
                {
                    Task.Delay(100).Wait();
                }
            });
            foreach (var item in outputPath)
            {
                if (!File.Exists(item))
                {
                    result = false;
                    message = $"{Jvedio.Language.Resources.FailToGenerate} {item}";
                    break;
                }
            }
            return (result, message);
        }


        public async Task<(bool, string)> AsyncGenrateGif(Movie movie)
        {
            bool result = true;
            string message = "";
            string GifPath = BasePicPath + "Gif\\" + movie.id + ".gif";
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { result = false; message = Jvedio.Language.Resources.Message_SetFFmpeg; return (result, message); }
            if (!Directory.Exists(BasePicPath + "Gif\\")) Directory.CreateDirectory(BasePicPath + "Gif\\");
            if (!File.Exists(movie.filepath)) { result = false; message = Jvedio.Language.Resources.NotExists; return (result, message); }
            string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
            if (cutoffArray.Length == 0) { result = false; message = Jvedio.Language.Resources.FailToCutOffVideo; return (result, message); }
            string cutofftime = cutoffArray[new Random().Next(cutoffArray.Length - 1)];
            object[] list = new object[] { cutofftime, movie.filepath, GifPath };

            await BeginGenGif(list);

            if (!File.Exists(GifPath))
            {
                result = false;
                message = $"{Jvedio.Language.Resources.FailToGenerate} {GifPath}";
            }

            return (result, message);
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
