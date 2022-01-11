using DynamicData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class MediaParse
    {


        /// <summary>
        /// 获得影片长度（wmv  10ms，其他  100ms）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetVideoDuration(string path)
        {
            string result = "00:00:00";
            try
            {
                MediaInfo mediaInfo = new MediaInfo();
                mediaInfo.Open(path);
                string Duration = mediaInfo.Get(0, 0, "Duration/String3");
                result = Duration.Substring(0, Duration.LastIndexOf("."));
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

            return result;
        }


        /// <summary>
        /// 生成截图的时间节点（wmv  10ms，其他  100ms）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetCutOffArray(string path)
        {
            if (Properties.Settings.Default.ScreenShotNum <= 0 || Properties.Settings.Default.ScreenShotNum > 30) Properties.Settings.Default.ScreenShotNum = 10;
            string[] result = new string[Properties.Settings.Default.ScreenShotNum];
            string Duration = GetVideoDuration(path);
            double Second = DurationToSecond(Duration);
            Second = GetProperSecond(Second);
            if (Second < result.Length)
            {
                if (Second > 0)
                {
                    result = new string[(int)Second];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = SecondToDuration(i);
                    }
                    return result;
                }
                else
                {
                    //掐头去尾发现过少，则不截图
                    return new string[0];
                }

            }
            else
            {
                // 按照秒 n 等分
                uint splitLength = (uint)(Second / Properties.Settings.Default.ScreenShotNum);
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = SecondToDuration(Properties.Settings.Default.ScreenShotIgnoreStart * 60 + splitLength * i);//加上跳过开头的部分
                }
                return result;
            }


        }

        /// <summary>
        /// 去掉开头和结尾的秒数
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public static double GetProperSecond(double second)
        {
            double Second = second;
            if (Properties.Settings.Default.ScreenShotIgnoreStart > 0)
            {
                Second -= Properties.Settings.Default.ScreenShotIgnoreStart * 60;
            }
            if (Properties.Settings.Default.ScreenShotIgnoreEnd > 0)
            {
                Second -= Properties.Settings.Default.ScreenShotIgnoreEnd * 60;
            }
            return Second;
        }

        public static double DurationToSecond(string Duration)
        {
            if (string.IsNullOrEmpty(Duration) || Duration.Split(':').Count() < 3) return 0;
            double Hour = double.Parse(Duration.Split(':')[0]);
            double Minutes = double.Parse(Duration.Split(':')[1]);
            double Seconds = double.Parse(Duration.Split(':')[2]);
            return Hour * 3600 + Minutes * 60 + Seconds;
        }

        public static string SecondToDuration(double Second)
        {
            // 36000 10h
            if (Second == 0) return "00:00:00";
            TimeSpan timeSpan = TimeSpan.FromSeconds(Second);
            return $"{timeSpan.Hours.ToString().PadLeft(2, '0')}:{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}";
        }


        public static (double, double) GetWidthHeight(string vediopath)
        {
            string width = "0";
            string height = "0";
            if (!File.Exists(vediopath)) return (0, 0);
            try
            {
                MediaInfo MI = new MediaInfo();
                MI.Open(vediopath);
                width = MI.Get(StreamKind.Video, 0, "Width");
                height = MI.Get(StreamKind.Video, 0, "Height");
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            int.TryParse(width, out int w);
            int.TryParse(height, out int h);
            return (w, h);

        }


        /// <summary>
        /// 获取视频信息 （wmv  10ms，其他  100ms）
        /// </summary>
        /// <param name="VideoName"></param>
        /// <returns></returns>
        public static VideoInfo GetMediaInfo(string videoPath)
        {
            VideoInfo videoInfo = new VideoInfo();
            if (File.Exists(videoPath))
            {
                MediaInfo MI = null;
                try
                {
                    MI = new MediaInfo();
                    MI.Open(videoPath);
                    //全局
                    string format = MI.Get(StreamKind.General, 0, "Format");
                    string bitrate = MI.Get(StreamKind.General, 0, "BitRate/String");
                    string duration = MI.Get(StreamKind.General, 0, "Duration/String1");
                    string fileSize = MI.Get(StreamKind.General, 0, "FileSize/String");
                    //视频
                    string vid = MI.Get(StreamKind.Video, 0, "ID");
                    string video = MI.Get(StreamKind.Video, 0, "Format");
                    string vBitRate = MI.Get(StreamKind.Video, 0, "BitRate/String");
                    string vSize = MI.Get(StreamKind.Video, 0, "StreamSize/String");
                    string width = MI.Get(StreamKind.Video, 0, "Width");
                    string height = MI.Get(StreamKind.Video, 0, "Height");
                    string risplayAspectRatio = MI.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");
                    string risplayAspectRatio2 = MI.Get(StreamKind.Video, 0, "DisplayAspectRatio");
                    string frameRate = MI.Get(StreamKind.Video, 0, "FrameRate/String");
                    string bitDepth = MI.Get(StreamKind.Video, 0, "BitDepth/String");
                    string pixelAspectRatio = MI.Get(StreamKind.Video, 0, "PixelAspectRatio");
                    string encodedLibrary = MI.Get(StreamKind.Video, 0, "Encoded_Library");
                    string encodeTime = MI.Get(StreamKind.Video, 0, "Encoded_Date");
                    string codecProfile = MI.Get(StreamKind.Video, 0, "Codec_Profile");
                    string frameCount = MI.Get(StreamKind.Video, 0, "FrameCount");

                    //音频
                    string aid = MI.Get(StreamKind.Audio, 0, "ID");
                    string audio = MI.Get(StreamKind.Audio, 0, "Format");
                    string aBitRate = MI.Get(StreamKind.Audio, 0, "BitRate/String");
                    string samplingRate = MI.Get(StreamKind.Audio, 0, "SamplingRate/String");
                    string channel = MI.Get(StreamKind.Audio, 0, "Channel(s)");
                    string aSize = MI.Get(StreamKind.Audio, 0, "StreamSize/String");

                    string audioInfo = MI.Get(StreamKind.Audio, 0, "Inform") + MI.Get(StreamKind.Audio, 1, "Inform") + MI.Get(StreamKind.Audio, 2, "Inform") + MI.Get(StreamKind.Audio, 3, "Inform");
                    string vi = MI.Get(StreamKind.Video, 0, "Inform");

                    videoInfo = new VideoInfo()
                    {
                        Format = format,
                        BitRate = vBitRate,
                        Duration = duration,
                        FileSize = fileSize,
                        Width = width,
                        Height = height,

                        DisplayAspectRatio = risplayAspectRatio,
                        FrameRate = frameRate,
                        BitDepth = bitDepth,
                        PixelAspectRatio = pixelAspectRatio,
                        Encoded_Library = encodedLibrary,
                        FrameCount = frameCount,
                        AudioFormat = audio,
                        AudioBitRate = aBitRate,
                        AudioSamplingRate = samplingRate,
                        Channel = channel
                    };
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                }
                finally
                {
                    MI?.Close();
                }


            }
            if (!string.IsNullOrEmpty(videoInfo.Width) && !string.IsNullOrEmpty(videoInfo.Height)) videoInfo.Resolution = videoInfo.Width + "x" + videoInfo.Height;
            if (!string.IsNullOrEmpty(videoPath))
            {
                videoInfo.Extension = Path.GetExtension(videoPath)?.ToUpper().Replace(".", "");
                videoInfo.FileName = Path.GetFileNameWithoutExtension(videoPath);
            }
            return videoInfo;
        }

    }
}
