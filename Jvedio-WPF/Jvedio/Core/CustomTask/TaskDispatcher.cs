using Jvedio.Core.CustomEventArgs;
using Priority_Queue;
using SuperUtils.Maths;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomTask
{
    public class TaskDispatcher<T> where T : AbstractTask
    {
        // 优先级
        private static int MAX_PRIORITY = 5;
        private static int NORMAL_PRIORITY = 3;

        private const int DEFAULT_TASKDELAY = 3 * 1000;         // 默认暂停时间
        private const int DEFAULT_LONG_TASKDELAY = 60 * 1000;   // 长任务的暂停默认时间 +- 随机数
        private const int DEFAULT_LONG_TASK_COUNT = 10;         // 运行多少个任务后开始长暂停

        private static int MAX_TASK_COUNT = 3;      // 每次同时下载的任务数量
        private static int CHECK_PERIOD = 1000;     // 调度器运行周期
        private long beforeTaskCount = 0;           // 上一次长暂停的 DoneList 数目，避免重复长暂停

        public bool Working { get; set; }               // 调度器是否在工作中

        public bool Cancel { get; set; }                // 调度器是否被取消了

        public double Progress { get; set; }            // 总的工作进度

        private int TaskDelay { get; set; }             // 每一批次任务完成后暂停的时间

        private int LongTaskDelay { get; set; }         // 每一批次任务完成后暂停的时间

        private bool EnableLongTaskDelay { get; set; }  // 每一批次任务完成后暂停的时间

        public event EventHandler onWorking;

        public event EventHandler onLongDelay;

        /* 具有优先级的队列，懒加载 */
        protected static SimplePriorityQueue<T> WaitingQueue { get; set; }

        protected static List<T> WorkingList { get; set; }

        protected static List<T> DoneList { get; set; }

        protected static List<T> CanceldList { get; set; }

        private static TaskDispatcher<T> instance = null;

        private TaskDispatcher(int taskDelay, int longTaskDelay, bool enableLongTaskDelay)
        {
            TaskDelay = taskDelay;
            LongTaskDelay = longTaskDelay;
            EnableLongTaskDelay = enableLongTaskDelay;
        }

        public static TaskDispatcher<T> createInstance(int taskDelay = DEFAULT_TASKDELAY, int longTaskDelay = DEFAULT_LONG_TASKDELAY, bool enableLongTaskDelay = false)
        {
            WaitingQueue = new SimplePriorityQueue<T>();
            WorkingList = new List<T>();
            DoneList = new List<T>();
            CanceldList = new List<T>();
            if (instance == null) instance = new TaskDispatcher<T>(taskDelay, longTaskDelay, enableLongTaskDelay);
            return instance;
        }

        public void Enqueue(T task)
        {
            if (!WaitingQueue.Contains(task))
                WaitingQueue.Enqueue(task, NORMAL_PRIORITY);
        }

        public void CancelWork()
        {
            Cancel = true;
            Working = false;
            foreach (T task in WorkingList)
            {
                task.Cancel();
            }
        }

        public void ClearDoneList()
        {
            DoneList.Clear();
            CanceldList.Clear();
        }

        public void BeginWork()
        {
            Working = true;
            beforeTaskCount = 0;
            Task.Run(async () =>
            {
                while (true && !Cancel)
                {
                    Console.WriteLine("调度器工作中...");
                    // 检查工作队列中的任务是否完成
                    for (int i = WorkingList.Count - 1; i >= 0; i--)
                    {
                        if (Cancel) return;
                        T task = WorkingList[i];
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            DoneList.Add(task);
                            WorkingList.RemoveAt(i);
                        }
                        else if (task.Status == TaskStatus.Canceled)
                        {
                            CanceldList.Add(task);
                            WorkingList.RemoveAt(i);
                        }
                    }

                    // 长暂停
                    if (EnableLongTaskDelay && DoneList.Count > 0 &&
                    beforeTaskCount != DoneList.Count && DoneList.Count % DEFAULT_LONG_TASK_COUNT == 0)
                    {
                        beforeTaskCount = DoneList.Count;
                        int delay = NumberHelper.GenerateRandomMS(LongTaskDelay);
                        Console.WriteLine("开始长暂停 " + delay);
                        onLongDelay?.Invoke(this, new MessageCallBackEventArgs(delay.ToString()));
                        await Task.Delay(delay);
                    }
                    else
                    {
                        // 短暂停
                        if (WorkingList.Count != 0 && TaskDelay > 0) await Task.Delay(NumberHelper.GenerateRandomMS(TaskDelay, 1));
                    }

                    onLongDelay?.Invoke(this, new MessageCallBackEventArgs("0"));// 隐藏提示

                    // 扫描一遍 doneList 和 cancelList，把标记为重新开始的任务添加到工作队列
                    for (int i = DoneList.Count - 1; i >= 0; i--)
                    {
                        if (DoneList[i].Status == TaskStatus.WaitingToRun)
                        {
                            WaitingQueue.Enqueue(DoneList[i], MAX_PRIORITY);// 重新开始的任务优先值最高
                            DoneList.RemoveAt(i);
                        }
                    }

                    for (int i = CanceldList.Count - 1; i >= 0; i--)
                    {
                        if (CanceldList[i].Status == TaskStatus.WaitingToRun)
                        {
                            WaitingQueue.Enqueue(CanceldList[i], MAX_PRIORITY);// 重新开始的任务优先值最高
                            CanceldList.RemoveAt(i);
                        }
                    }

                    // 将等待队列中的下载任务添加到工作队列
                    while (WorkingList.Count < MAX_TASK_COUNT && WaitingQueue.Count > 0)
                    {
                        T task = WaitingQueue.Dequeue();
                        if (task.Status == TaskStatus.WaitingToRun)
                            WorkingList.Add(task);
                        else if (task.Status == TaskStatus.Canceled)
                            CanceldList.Add(task);
                        else if (task.Status == TaskStatus.RanToCompletion)
                            DoneList.Add(task);
                    }

                    foreach (T task in WorkingList)
                    {
                        if (task.Status == TaskStatus.WaitingToRun)
                        {
                            task.Start();
                        }
                    }

                    float totalcount = CanceldList.Count + DoneList.Count + WaitingQueue.Count + WorkingList.Count;
                    this.Progress = Math.Round((float)(CanceldList.Count + DoneList.Count) / totalcount * 100, 2);
                    onWorking?.Invoke(this, null);
                    await Task.Delay(CHECK_PERIOD);

                    if (WorkingList.Count == 0 && WaitingQueue.Count == 0)
                    {
                        Working = false;
                        break;
                    }
                }
            });
        }
    }
}
