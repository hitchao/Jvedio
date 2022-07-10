using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace Jvedio.Utils
{
    /// <summary>
    /// 自定义扩展类
    /// </summary>
    public static class CustomExtension
    {





        public static T GetQueryOrDefault<T>(this BitmapMetadata metadata, string query, T defaultValue)
        {
            if (metadata.ContainsQuery(query))
                return (T)Convert.ChangeType(metadata.GetQuery(query), typeof(T));
            return defaultValue;
        }



        public static string CleanSqlString(this string str)
        {
            return str.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Replace("'", "");
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException(Jvedio.Language.Resources.TO);
                }
            }
        }


        public static string ToProperFileSize(this long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        public static bool IsLetter(this char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                return true;
            else
                return false;
        }


        // todo
        // 2.1 和 2.14 会产生交集
        // d:/abc/2.1
        // d:/abc/2.14
        // d:/2.145
        public static bool IsIntersectWith(this ICollection<string> collections, string dir)
        {
            if (collections?.Count <= 0 || string.IsNullOrEmpty(dir)) return false;
            List<string> names = collections.Select(arg => Path.GetFileName(arg).ToLower()).ToList();
            string fatherPath = Path.GetDirectoryName(dir).ToLower();
            string name = Path.GetFileNameWithoutExtension(dir);
            foreach (var item in collections)
            {
                if (item.IndexOf(dir) >= 0 || dir.IndexOf(item) >= 0)
                {
                    string fp = Path.GetDirectoryName(item).ToLower();
                    if (fp.Equals(fatherPath) && !names.Contains(name)) return false;
                    return true;
                }
            }

            return false;
        }

        public static int IndexOfAnyString(this string str, string[] parameter)
        {
            foreach (var item in parameter)
            {
                int idx = str.IndexOf(item, StringComparison.CurrentCultureIgnoreCase);
                if (idx >= 0) return idx;
            }
            return -1;
        }

        /// <summary>
        /// 根据文件的实际数字大小排序而不是 1,10,100,1000
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<string> CustomSort(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();

            return list.Select(s => new
            {
                OrgStr = s,
                SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
            })
            .OrderBy(x => x.SortStr)
            .Select(x => x.OrgStr);
        }


        public static string ToProperSql(this string sql, bool toUpper = true)
        {
            if (toUpper)
                return sql.Replace("%", "").Replace("'", "").ToUpper();
            else
                return sql.Replace("%", "").Replace("'", "");
        }

        public static string ToProperFileName(this string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }
    }
}
