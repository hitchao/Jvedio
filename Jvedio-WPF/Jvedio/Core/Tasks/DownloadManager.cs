
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Net;
using SuperUtils.Framework.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using static Jvedio.App;
using System.Linq;

namespace Jvedio.Core.Tasks
{
    public class DownloadManager : BaseManager
    {


        #region "事件"


        public event EventHandler onLongDelay;

        #endregion


        private static TaskDispatcher<DownLoadTask> Dispatcher { get; set; }

        static DownloadManager()
        {
            Dispatcher = TaskDispatcher<DownLoadTask>.CreateInstance(taskDelay: 3000, enableLongTaskDelay: true);
            Dispatcher.onWorking += (s, e) => {
                App.Current.Dispatcher.Invoke(() => {
                    Instance.onRunning?.Invoke();
                    Instance.Progress = (int)Dispatcher.Progress;
                });
            };
            Dispatcher.onLongDelay += (s, e) => {
                Instance.onLongDelay?.Invoke(s, e);
            };
            Dispatcher.onComplete += (s, e) => {
                Instance.Progress = 100;
            };
            //start();
        }

        public void Start()
        {
            Dispatcher.BeginWork();
        }

        private DownloadManager() { }

        public new static DownloadManager Instance { get; set; }

        public new static DownloadManager CreateInstance()
        {
            if (Instance == null)
                Instance = new DownloadManager();
            return Instance;
        }


        public override void AddToDispatcher(AbstractTask task)
        {
            Dispatcher.Enqueue(task as DownLoadTask);
            Dispatcher.BeginWork();
        }

        public override void ClearDispatcher()
        {
            Dispatcher.ClearDoneList();
        }
    }
}
