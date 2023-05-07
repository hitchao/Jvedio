using Jvedio.Core.Config.Base;
using System.ComponentModel;

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
        }

        private static FFmpegConfig _instance = null;

        public static FFmpegConfig CreateInstance()
        {
            if (_instance == null) _instance = new FFmpegConfig();

            return _instance;
        }

        public string Path { get; set; }

        [DefaultValue(DEFAULT_THREAD_NUM)]
        public long ThreadNum { get; set; }

        public long TimeOut { get; set; }

        [DefaultValue(DEFAULT_SCREEN_SHOT_NUM)]
        public long ScreenShotNum { get; set; }

        [DefaultValue(DEFAULT_SCREEN_SHOT_IGNORE_START)]
        public long ScreenShotIgnoreStart { get; set; }

        [DefaultValue(DEFAULT_SCREEN_SHOT_IGNORE_END)]
        public long ScreenShotIgnoreEnd { get; set; }

        [DefaultValue(true)]
        public bool SkipExistScreenShot { get; set; }

        [DefaultValue(true)]
        public bool ScreenShotAfterImport { get; set; }

        public bool SkipExistGif { get; set; }

        [DefaultValue(true)]
        public bool GifAutoHeight { get; set; }

        [DefaultValue(DEFAULT_GIF_WIDTH)]
        public long GifWidth { get; set; }

        [DefaultValue(DEFAULT_GIF_HEIGHT)]
        public long GifHeight { get; set; }

        [DefaultValue(DEFAULT_GIF_DURATION)]
        public long GifDuration { get; set; }
    }
}
