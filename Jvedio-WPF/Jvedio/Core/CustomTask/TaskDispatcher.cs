using Jvedio.Core.CustomTask;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomTask
{
    public class TaskDispatcher<T> where T : AbstractTask
    {

        // 优先级
        private static int MAX_PRIORITY = 5;
        private static int NORMAL_PRIORITY = 3;
        private static int MIN_PRIORITY = 1;
        private const int DEFAULT_TASKDELAY = 3000;

        private static int MAX_TASK_COUNT = 3;// 每次同时下载的任务数量

        private static int CHECK_PERIOD = 1000;  // 调度器运行周期


        public bool Working = false;// 调度器是否在工作中
        public bool Cancel = false;// 调度器是否被取消了

        public double Progress { get; set; }// 总的工作进度
        private int TaskDelay { get; set; }// 每一批次任务完成后暂停的时间

        public event EventHandler onWorking;

        // 具有优先级的队列
        public static SimplePriorityQueue<T> WaitingQueue = new SimplePriorityQueue<T>();
        public static List<T> WorkingList = new List<T>();
        public static List<T> DoneList = new List<T>();

        private static TaskDispatcher<T> instance = null;

        private TaskDispatcher(int taskDelay)
        {
            TaskDelay = taskDelay;
        }


        public static TaskDispatcher<T> createInstance(int taskDelay = DEFAULT_TASKDELAY)
        {
            if (instance == null) instance = new TaskDispatcher<T>(taskDelay);


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
        }


        public void BeginWork()
        {
            Working = true;
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
                        if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Canceled)
                        {
                            DoneList.Add(task);
                            WorkingList.RemoveAt(i);
                        }
                    }
                    if (WorkingList.Count != 0 && TaskDelay > 0) await Task.Delay(TaskDelay);
                    // 将等待队列中的下载任务添加到工作队列
                    while (WorkingList.Count < MAX_TASK_COUNT && WaitingQueue.Count > 0)
                    {
                        T task = WaitingQueue.Dequeue();
                        if (task.Status == TaskStatus.WaitingToRun)
                            WorkingList.Add(task);
                        else
                            DoneList.Add(task);
                    }

                    foreach (T task in WorkingList)
                    {
                        if (!task.Running && task.Status != TaskStatus.Canceled)
                        {
                            task.Start();
                        }
                    }

                    float totalcount = DoneList.Count + WaitingQueue.Count + WorkingList.Count;
                    this.Progress = Math.Round((float)DoneList.Count / totalcount * 100, 2);
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
