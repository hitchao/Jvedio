using Jvedio.Core.Net;
using SuperUtils.Framework.Tasks;
using System;

namespace Jvedio.Core.Tasks
{
    public class DownloadManager : BaseManager
    {
        /// <summary>
        /// 每个任务的间隔 (ms)
        /// </summary>
        private const int TASK_DELAY = 3000;

        /// <summary>
        /// 任务的数目到达 LongTaskCount 时暂停的间隔 (ms)
        /// </summary>
        private const int LONG_TASK_DELAY = 10 * 1000;

        /// <summary>
        /// 是否开启长暂停
        /// </summary>
        private const bool ENABLE_LONG_TASK_DELAY = true;

        /// <summary>
        /// 可同时进行任务的数目
        /// </summary>
        private const int TASK_COUNT = 2;

        /// <summary>
        /// 进行长暂停的上限
        /// </summary>
        private const int LONG_TASK_COUNT = 5;


        /// <summary>
        /// 默认的任务配置
        /// </summary>
        private static TaskConfig DEFAULT_CONFIG { get; set; } = new TaskConfig() {
            TaskDelay = TASK_DELAY,
            TaskCount = TASK_COUNT,
            LongTaskCount = LONG_TASK_COUNT,
            LongTaskDelay = LONG_TASK_DELAY,
            EnableLongTaskDelay = ENABLE_LONG_TASK_DELAY,
        };



        #region "事件"


        public event EventHandler onLongDelay;

        #endregion


        private static TaskDispatcher<DownLoadTask> Dispatcher { get; set; }

        static DownloadManager()
        {
            Dispatcher = TaskDispatcher<DownLoadTask>.CreateInstance(DEFAULT_CONFIG);
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
