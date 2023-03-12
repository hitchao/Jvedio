
using Jvedio.Core.Net;
using SuperUtils.Framework.Tasks;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Jvedio.Global
{
    public static class DownloadManager
    {
        public static TaskDispatcher<DownLoadTask> Dispatcher { get; set; }

        static DownloadManager()
        {
            Dispatcher = TaskDispatcher<DownLoadTask>.createInstance(taskDelay: 3000, enableLongTaskDelay: true);
            //start();
        }

        public static void start()
        {
            Task.Run(async () =>
            {
                while (true)
                {


                    await Task.Delay(1000);
                    Dispatcher.Working = true;
                    Debug.Print("下载中...");
                }
            });
        }
    }
}
