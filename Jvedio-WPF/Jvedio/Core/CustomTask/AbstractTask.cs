using DynamicData.Annotations;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomTask
{
    public class AbstractTask : ITask, INotifyPropertyChanged
    {
        public Stopwatch stopwatch { get; set; }

        public bool Success { get; set; }

        public bool Canceld { get; set; }

        protected CancellationTokenSource tokenCTS;
        protected CancellationToken token;

        public event EventHandler onError;

        public event EventHandler onCanceled;

        public event EventHandler onCompleted;

        public List<string> Logs { get; set; }

        protected TaskLogger logger { get; set; }

        public static Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {
            { TaskStatus.WaitingToRun, "等待中..." },
            { TaskStatus.Running, "进行中..." },
            { TaskStatus.Canceled, "已取消" },
            { TaskStatus.RanToCompletion, "已完成" },
        };

        /// <summary>
        /// 任务调度
        /// </summary>
        public AbstractTask()
        {
            Status = System.Threading.Tasks.TaskStatus.WaitingToRun;
            CreateTime = DateHelper.Now();

            tokenCTS = new CancellationTokenSource();
            tokenCTS.Token.Register(() =>
            {
                Console.WriteLine("取消任务");
                OnCanceled(null);
            });
            token = tokenCTS.Token;

            stopwatch = new Stopwatch();
            Logs = new List<string>();
            logger = new TaskLogger(Logs);
        }

        #region "property"

        public TaskStatus _Status;

        public TaskStatus Status
        {
            get
            {
                return _Status;
            }

            set
            {
                _Status = value;
                if (STATUS_TO_TEXT_DICT.ContainsKey(value))
                    StatusText = STATUS_TO_TEXT_DICT[value];
                else StatusText = string.Empty;
                OnPropertyChanged();
            }
        }

        public string _Message;

        public string Message
        {
            get
            {
                return _Message;
            }

            set
            {
                _Message = value;
                OnPropertyChanged();
            }
        }

        public string _StatusText;

        public string StatusText
        {
            get
            {
                return _StatusText;
            }

            set
            {
                _StatusText = value;
                logger?.Info(value);
                OnPropertyChanged();
            }
        }

        public long _ElapsedMilliseconds;

        public long ElapsedMilliseconds
        {
            get
            {
                return _ElapsedMilliseconds;
            }

            set
            {
                _ElapsedMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public float _Progress;

        public float Progress
        {
            get
            {
                return _Progress;
            }

            set
            {
                _Progress = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 用于指示下载任务是否进行中
        /// </summary>
        public bool _Running;

        public bool Running
        {
            get
            {
                return _Running;
            }

            set
            {
                _Running = value;
                OnPropertyChanged();
            }
        }

        public string _CreateTime;

        public string CreateTime
        {
            get
            {
                return _CreateTime;
            }

            set
            {
                _CreateTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        protected virtual void OnError(EventArgs e)
        {
            EventHandler error = onError;
            error?.Invoke(this, e);
        }

        protected virtual void OnCanceled(EventArgs e)
        {
            EventHandler eventHandler = onCanceled;
            eventHandler?.Invoke(this, e);
        }

        protected virtual void OnCompleted(EventArgs e)
        {
            EventHandler eventHandler = onCompleted;
            eventHandler?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Cancel()
        {
            if (Status == TaskStatus.Running || Status == TaskStatus.WaitingToRun)
            {
                Status = TaskStatus.Canceled;
                Running = false;
                tokenCTS?.Cancel();
                Canceld = true;
                logger.Info("已取消");
            }
        }

        public virtual void Start()
        {
            Status = TaskStatus.Running;
            CreateTime = DateHelper.Now();
            Running = true;
            doWrok();
        }

        public virtual void FinalizeWithCancel()
        {
            Running = false;
            Status = TaskStatus.Canceled;// 抛出异常的任务都自动取消
            stopwatch?.Stop();
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            Success = false;
            logger.Info($"总计耗时：{ElapsedMilliseconds} ms");
        }

        public virtual void doWrok()
        {
            throw new NotImplementedException();
        }

        public virtual void Finished()
        {
            throw new NotImplementedException();
        }

        public virtual void Pause()
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public virtual void Restart()
        {
            this.Status = TaskStatus.WaitingToRun;// 任务重新开始
        }
    }
}
