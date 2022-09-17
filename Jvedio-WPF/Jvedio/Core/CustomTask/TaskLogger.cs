using SuperUtils.Time;
using System.Collections.Generic;

namespace Jvedio.Core.CustomTask
{
    public class TaskLogger
    {
        private List<string> Logs { get; set; }

        public TaskLogger(List<string> logs)
        {
            Logs = logs;
            if (Logs == null) Logs = new List<string>();
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
