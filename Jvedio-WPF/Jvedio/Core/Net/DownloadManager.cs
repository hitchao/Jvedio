using Jvedio.Core.CustomTask;
using Jvedio.Core.Net;

namespace Jvedio.Global
{
    public static class DownloadManager
    {
        public static TaskDispatcher<DownLoadTask> Dispatcher { get; set; }

        static DownloadManager()
        {
            Dispatcher = TaskDispatcher<DownLoadTask>.createInstance(taskDelay: 3000, enableLongTaskDelay: true);
        }


    }
}
