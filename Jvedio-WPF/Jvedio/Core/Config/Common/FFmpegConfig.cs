using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class FFmpegConfig : AbstractConfig
    {


        private FFmpegConfig() : base("FFmpegConfig")
        {
            ThreadNum = 2;
            ScreenShotNum = 5;
            ScreenShotIgnoreStart = 1;
            ScreenShotIgnoreEnd = 1;
            GifAutoHeight = true;
            GifWidth = 300;
            SkipExistScreenShot = true;
        }

        private static FFmpegConfig _instance = null;

        public static FFmpegConfig createInstance()
        {
            if (_instance == null) _instance = new FFmpegConfig();

            return _instance;
        }


        public string Path { get; set; }

        public long ThreadNum { get; set; }

        public long TimeOut { get; set; }

        public long ScreenShotNum { get; set; }

        public long ScreenShotIgnoreStart { get; set; }

        public long ScreenShotIgnoreEnd { get; set; }

        public bool SkipExistScreenShot { get; set; }

        public bool SkipExistGif { get; set; }

        public bool GifAutoHeight { get; set; }

        public long GifWidth { get; set; }

        public long GifHeight { get; set; }

        public long GifDuration { get; set; }


    }
}
