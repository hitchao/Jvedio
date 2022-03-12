using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public static class Log
    {
        public static Level LogLevel { get; set; }
        public enum Level
        {
            Error,
            Warning,
            Information
        }

        static Log()
        {
            LogLevel = Level.Information;
        }

        public static void Error(string str)
        {
            if (LogLevel >= Level.Error)
                Console.WriteLine($"{DateHelper.Now()} => [Error] {str}");
        }

        public static void Info(string str)
        {
            if (LogLevel >= Level.Information)
                Console.WriteLine($"{DateHelper.Now()} => [Info] {str}");
        }

        public static void Warn(string str)
        {
            if (LogLevel >= Level.Warning)
                Console.WriteLine($"{DateHelper.Now()} => [Warn] {str}");
        }
    }
}
