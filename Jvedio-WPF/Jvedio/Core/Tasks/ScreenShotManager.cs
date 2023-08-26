using Jvedio.Core.FFmpeg;
using SuperUtils.Framework.Tasks;

namespace Jvedio.Core.Tasks
{
    public class ScreenShotManager : BaseManager
    {
        /// <summary>
        /// 每个任务的间隔 (ms)
        /// </summary>
        private const int TASK_DELAY = 0;

        /// <summary>
        /// 任务的数目到达 LongTaskCount 时暂停的间隔 (ms)
        /// </summary>
        private const int LONG_TASK_DELAY = 10 * 1000;

        /// <summary>
        /// 是否开启长暂停
        /// </summary>
        private const bool ENABLE_LONG_TASK_DELAY = false;

        /// <summary>
        /// 可同时进行任务的数目
        /// </summary>
        private const int TASK_COUNT = 5;

        /// <summary>
        /// 进行长暂停的上限
        /// </summary>
        private const int LONG_TASK_COUNT = 0;


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


        protected ScreenShotManager() { }

        public new static ScreenShotManager Instance { get; set; }

        public new static ScreenShotManager CreateInstance()
        {
            if (Instance == null)
                Instance = new ScreenShotManager();
            return Instance;
        }

        private static TaskDispatcher<ScreenShotTask> Dispatcher { get; set; }

        static ScreenShotManager()
        {
            Dispatcher = TaskDispatcher<ScreenShotTask>.CreateInstance(DEFAULT_CONFIG);
            Dispatcher.onWorking += (s, e) => {
                App.Current.Dispatcher.Invoke(() => {
                    Instance.onRunning?.Invoke();
                    Instance.Progress = (int)Dispatcher.Progress;
                });
            };
        }


        public override void AddToDispatcher(AbstractTask task)
        {
            Dispatcher.Enqueue(task as ScreenShotTask);
            Dispatcher.BeginWork();
        }

        public override void ClearDispatcher()
        {
            Dispatcher.ClearDoneList();
        }
    }
}
