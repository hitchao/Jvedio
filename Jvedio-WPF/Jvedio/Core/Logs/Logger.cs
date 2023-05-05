using Jvedio.Core.Enums;
using SuperUtils.Framework.Logger;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using static SuperUtils.Framework.Logger.AbstractLogger;

namespace Jvedio.Core.Logs
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public class Logger : AbstractLogger
    {

        private static string FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private static object LogLock { get; set; }

        private Logger() { }

        public static Logger Instance { get; }

        static Logger()
        {
            Instance = new Logger();
            Instance.LogLevel = Level.Debug;
            LogLock = new object();
        }

        public override void LogPrint(string str)
        {
            Console.Write(str);
            if (!Directory.Exists(FilePath))
                SuperUtils.IO.DirHelper.TryCreateDirectory(FilePath);
            string filepath = System.IO.Path.Combine(FilePath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            lock (LogLock)
            {
                FileHelper.TryAppendToFile(FilePath, str);
            }
        }

    }
}
