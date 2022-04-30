using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{

    /// <summary>
    /// 日志记录静态类
    /// </summary>
    public static class Logger
    {
        private static readonly object NetWorkLock = new object();
        private static readonly object DataBaseLock = new object();
        private static readonly object ExceptionLock = new object();
        private static readonly object ScanLogLock = new object();

        private static readonly object ErrorLock = new object();



        public static void LogE(Exception e)
        {
            Console.WriteLine(e.StackTrace);
            Console.WriteLine(e.Message);
            string path = AppDomain.CurrentDomain.BaseDirectory + "Log";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            string filepath = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd") + ".log");

            string content;
            content = Environment.NewLine + "-----【" + DateTime.Now.ToString() + "】-----";
            content += Environment.NewLine + $"Message=>{ e.Message}";
            content += Environment.NewLine + $"StackTrace=>{Environment.NewLine }{ GetAllFootprints(e)}";

            lock (ExceptionLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try { sr.Write(content); } catch { }
                }
            }
        }


        public static void LogF(Exception e)
        {
            Console.WriteLine(e.StackTrace);
            Console.WriteLine(e.Message);
            string path = AppDomain.CurrentDomain.BaseDirectory + "Log\\File";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            string filepath = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string content;
            content = Environment.NewLine + "-----【" + DateTime.Now.ToString() + "】-----";
            content += $"{Environment.NewLine }Message=>{ e.Message}";
            content += $"{Environment.NewLine }StackTrace=>{Environment.NewLine }{ GetAllFootprints(e)}";

            lock (ExceptionLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try { sr.Write(content); } catch { }
                }
            }
        }



        public static void LogN(string NetWorkStatus)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Log\\NetWork";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            string filepath = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string content = Environment.NewLine + "【" + DateTime.Now.ToString() + $"】=>{NetWorkStatus}";
            lock (NetWorkLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try { sr.Write(content); } catch { }
                }
            }

        }


        public static void LogD(Exception e)
        {
            Console.WriteLine(e.StackTrace);
            Console.WriteLine(e.Message);
            string path = AppDomain.CurrentDomain.BaseDirectory + "Log\\DataBase";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            string filepath = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string content;
            content = Environment.NewLine + "-----【" + DateTime.Now.ToString() + "】-----";
            content += $"{Environment.NewLine}Message=>{ e.Message}";
            content += $"{Environment.NewLine}StackTrace=>{Environment.NewLine}{ GetAllFootprints(e)}";
            lock (DataBaseLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try { sr.Write(content); } catch { }
                }
            }

        }

        public static string GetAllFootprints(Exception x)
        {
            if (x == null) return "";
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
                        traceString.Append($"    {Jvedio.Language.Resources.File}: {frame.GetFileName()}");
                        traceString.Append($" {Jvedio.Language.Resources.Method}: {frame.GetMethod().Name}");
                        traceString.Append($" {Jvedio.Language.Resources.RowNumber}: {frame.GetFileLineNumber()}{Environment.NewLine}");
                    }
                }

                return traceString.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }


        public static void LogScanInfo(string content)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "log/scanlog";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            string filepath = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            lock (ScanLogLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try { sr.Write(content); } catch { }
                }
            }
        }


        public static void Error(string str)
        {
            str += Environment.NewLine;
            Console.WriteLine(str);
            string path = AppDomain.CurrentDomain.BaseDirectory + "Log\\Error";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            string filepath = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            lock (DataBaseLock)
            {
                using (StreamWriter sr = new StreamWriter(filepath, true))
                {
                    try { sr.Write(str); } catch { }
                }
            }
        }



    }
}
