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
using System.Windows.Threading;

namespace Jvedio.Core.Tasks
{
    public class ScanManager : BaseManager
    {

        protected ScanManager() { }

        public new static ScanManager Instance { get; set; }

        public new static ScanManager CreateInstance()
        {
            if (Instance == null)
                Instance = new ScanManager();
            return Instance;
        }

        public override void AddToDispatcher(AbstractTask task)
        {

        }

        public override void ClearDispatcher()
        {

        }
    }
}
