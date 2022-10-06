using Jvedio.Core.Enums;
using SuperUtils.Time;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Jvedio.Core.Logs
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public static class Logger
    {

        /// <summary>
        /// 日志级别，默认 Info 级别
        /// </summary>
        public static LogType LogLevel { get; set; }

        private static object ExceptionLock { get; set; }
        private static object CommonLogLock { get; set; }


        static Logger()
        {
            LogLevel = LogType.Info;
            ExceptionLock = new object();
            CommonLogLock = new object();
        }

        private static string GetAllFootprints(Exception x)
        {
            if (x == null) return string.Empty;
            try
            {
                var st = new StackTrace(x, true);
                var frames = st.GetFrames();
                var traceString = new StringBuilder();
                StackFrame[] stackFrames = new StackTrace(x, true).GetFrames();
                if (stackFrames != null && stackFrames.Length > 0)
                {
                    foreach (var frame in stackFrames)
                    {
                        if (frame.GetFileLineNumber() < 1)
                            continue;
                        string filename = frame.GetFileName();
                        filename = string.IsNullOrEmpty(filename) ? "NULL" : filename.Replace(@"D:\Jvedio\Jvedio", string.Empty);
                        traceString.Append($"{SuperControls.Style.LangManager.GetValueByKey("File")}: {filename}");
                        traceString.Append($" {SuperControls.Style.LangManager.GetValueByKey("Method")}: {frame.GetMethod().Name}");
                        traceString.Append($" {SuperControls.Style.LangManager.GetValueByKey("RowNumber")}: {frame.GetFileLineNumber()}{Environment.NewLine}");
                    }
                }

                return traceString.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return string.Empty;
        }


        public static void Info(string content)
        {
            if (LogLevel >= LogType.Info)
                Log(LogType.Info, content);
        }

        public static void Warning(string content)
        {
            if (LogLevel >= LogType.Warning)
                Log(LogType.Warning, content);
        }

        /// <summary>
        /// 单独处理 Error 级别的日志
        /// </summary>
        /// <param name="e"></param>
        public static void Error(Exception e)
        {
            if (LogLevel < LogType.Error) return;
            string content;
            content = $"{Environment.NewLine}[{DateTime.Now.ToString()}]";
            content += $"{Environment.NewLine}[Message] {e.Message}";
            content += $"{Environment.NewLine}[StackTrace] {GetAllFootprints(e)}";
            Console.WriteLine(content);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "Error");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string filepath = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            lock (ExceptionLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try
                    {
                        sr.Write(content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static void Log(LogType logType, string content)
        {
            string output = $"{DateHelper.Now()} => [{logType}] {content} {Environment.NewLine}";
            Console.WriteLine(output);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            lock (CommonLogLock)
            {
                try
                {
                    using (StreamWriter sr = new StreamWriter(filepath, true, Encoding.UTF8))
                    {
                        sr.Write(output);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
