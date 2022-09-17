using Jvedio.Core.CustomTask;
using Jvedio.Core.FFmpeg;

namespace Jvedio.Global
{
    public static class FFmpegManager
    {
        public static TaskDispatcher<ScreenShotTask> Dispatcher { get; set; }

        static FFmpegManager()
        {
            Dispatcher = TaskDispatcher<ScreenShotTask>.createInstance(0);
        }
    }
}
