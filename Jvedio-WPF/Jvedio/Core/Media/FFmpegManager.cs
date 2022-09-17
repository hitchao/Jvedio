using Jvedio.Core.CustomTask;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
