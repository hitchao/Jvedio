using SuperUtils.Framework.Logger;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.IO;
using System.Text;
using System.Web.UI.WebControls;

namespace Jvedio.Core.Logs
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public class Logger : AbstractLogger
    {

        private static string FilePath { get; set; } =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private static object LogLock { get; set; }
        public static Logger Instance { get; }

        private Logger() { }

        static Logger()
        {
            Instance = new Logger();
            Instance.LogLevel = Level.Debug;
            LogLock = new object();
        }

        public override void LogPrint(string str)
        {
            if (str == null)
                str = "";
            Console.Write(str);
            DirHelper.TryCreateDirectory(FilePath);
            string filepath =
                System.IO.Path.Combine(FilePath, DateHelper.NowDate() + ".log");
            lock (LogLock) {
                try {
                    File.AppendAllText(filepath, str);
                } catch (Exception ex) {
                    // 防止递归
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
