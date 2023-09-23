using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class FFmpegConfig : AbstractConfig
    {
        public const int DEFAULT_THREAD_NUM = 2;
        public const int DEFAULT_SCREEN_SHOT_NUM = 5;
        public const int DEFAULT_SCREEN_SHOT_IGNORE_START = 1;
        public const int DEFAULT_SCREEN_SHOT_IGNORE_END = 1;
        public const int DEFAULT_GIF_WIDTH = 300;
        public const int DEFAULT_GIF_HEIGHT = 168;
        public const int DEFAULT_GIF_DURATION = 3;

        private FFmpegConfig() : base("FFmpegConfig")
        {
            ThreadNum = DEFAULT_THREAD_NUM;
            ScreenShotNum = DEFAULT_SCREEN_SHOT_NUM;
            ScreenShotIgnoreStart = DEFAULT_SCREEN_SHOT_IGNORE_START;
            ScreenShotIgnoreEnd = DEFAULT_SCREEN_SHOT_IGNORE_END;
            GifAutoHeight = true;
            GifWidth = DEFAULT_GIF_WIDTH;
            GifHeight = DEFAULT_GIF_HEIGHT;
            GifDuration = DEFAULT_GIF_DURATION;
            SkipExistScreenShot = true;
            SkipExistGif = true;
            ScreenShotAfterImport = true;
        }

        private static FFmpegConfig _instance = null;

        public static FFmpegConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new FFmpegConfig();

            return _instance;
        }

        public string Path { get; set; }

        public long ThreadNum { get; set; }

        public long TimeOut { get; set; }

        public long ScreenShotNum { get; set; }

        public long ScreenShotIgnoreStart { get; set; }

        public long ScreenShotIgnoreEnd { get; set; }

        public bool SkipExistScreenShot { get; set; }
        public bool ScreenShotAfterImport { get; set; }

        public bool SkipExistGif { get; set; }

        public bool GifAutoHeight { get; set; }

        public long GifWidth { get; set; }

        public long GifHeight { get; set; }

        public long GifDuration { get; set; }
    }
}
