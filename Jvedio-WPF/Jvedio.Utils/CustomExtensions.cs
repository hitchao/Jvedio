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

namespace Jvedio.Utils
{
    /// <summary>
    /// 自定义扩展类
    /// </summary>
    public static class CustomExtension
    {
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


        public static string ToProperFileSize(this long filesize)
        {
            double result = (double)filesize / 1024 / 1024;//MB
            if (filesize >= 0.9)
                return $"{Math.Round(result, 2)} MB";
            else
                return $"{Math.Ceiling(result * 1024)} KB";//KB
        }

        public static bool IsLetter(this char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                return true;
            else
                return false;
        }

        public static bool IsIntersectWith(this ObservableCollection<string> collections, string str)
        {
            foreach (var item in collections)
            {
                if (item.IndexOf(str) >= 0 || str.IndexOf(item) >= 0) return true;
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




        public static string ToSqlString(this string str)
        {
            string result = "";

            if (str == "标签")
            {
                result = "label";
            }
            else if (str == "系列")
            {
                result = "tag";
            }
            else if (str == "发行商")
            {
                result = "studio";
            }
            else if (str == "导演")
            {
                result = "director";
            }

            return result;


        }

        public static string ToSqlString(this Sort sort)
        {
            string result;
            if (sort == Sort.识别码)
            {
                result = "id";
            }
            else if (sort == Sort.文件大小)
            {
                result = "filesize";
            }
            else if (sort == Sort.导入时间)
            {
                result = "otherinfo";
            }
            else if (sort == Sort.创建时间)
            {
                result = "scandate";
            }
            else if (sort == Sort.喜爱程度)
            {
                result = "favorites";
            }
            else if (sort == Sort.名称)
            {
                result = "title";
            }
            else if (sort == Sort.访问次数)
            {
                result = "visits";
            }
            else if (sort == Sort.发行日期)
            {
                result = "releasedate";
            }
            else if (sort == Sort.评分)
            {
                result = "rating";
            }
            else if (sort == Sort.时长)
            {
                result = "runtime";
            }
            else if (sort == Sort.演员)
            {
                result = "actor";
            }
            else
            {
                result = "id";
            }

            return result;


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

        public static string ToProperUrl(this string url)
        {
            url = url.ToLower();
            if (string.IsNullOrEmpty(url)) return "";
            if (url.IndexOf("http") < 0) url = "https://" + url;
            if (!url.EndsWith("/")) url += "/";
            return url;
        }





        public static bool IsProperUrl(this string source) => Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);


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
