
using Jvedio.Core.FFmpeg;
using SuperUtils.Framework.Tasks;

namespace Jvedio.Global
{
    public static class FFmpegManager
    {
        public static TaskDispatcher<ScreenShotTask> Dispatcher { get; set; }

        static FFmpegManager()
        {
            Dispatcher = TaskDispatcher<ScreenShotTask>.CreateInstance(0);
        }
    }
}
