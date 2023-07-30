
using Jvedio.Core.Net;
using SuperUtils.Framework.Tasks;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Global
{
    public static class DownloadManager
    {
        public static TaskDispatcher<DownLoadTask> Dispatcher { get; set; }

        static DownloadManager()
        {
            Dispatcher = TaskDispatcher<DownLoadTask>.CreateInstance(taskDelay: 3000, enableLongTaskDelay: true);
            //start();
        }

        public static void start()
        {
            Task.Run(async () => {
                while (true) {
                    await Task.Delay(1000);
                    Dispatcher.Working = true;
                    Logger.Debug("downloading ...");
                }
            });
        }
    }
}
