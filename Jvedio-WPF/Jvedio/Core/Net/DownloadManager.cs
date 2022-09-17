using Jvedio.Core.CustomTask;
using Jvedio.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
