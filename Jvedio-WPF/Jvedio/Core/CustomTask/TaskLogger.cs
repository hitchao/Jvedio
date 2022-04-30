using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomTask
{
    public class TaskLogger
    {
        private List<string> Logs { get; set; }
        public TaskLogger(List<string> logs)
        {
            Logs = logs;
        }
        public void Error(string str)
        {
            Logs.Add($"[{DateHelper.Now()}] [Error] => {str}");
        }
        public void Info(string str)
        {
            Logs.Add($"[{DateHelper.Now()}] [Info] => {str}");
        }
    }
}
