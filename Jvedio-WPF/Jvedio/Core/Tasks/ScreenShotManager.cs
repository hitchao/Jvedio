using Jvedio.Core.Enums;
using Jvedio.Core.FFmpeg;
using Jvedio.Core.Net;
using SuperUtils.Framework.Tasks;
using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Tasks
{
    public class ScreenShotManager : BaseManager
    {

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
            Dispatcher = TaskDispatcher<ScreenShotTask>.CreateInstance(0);
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
