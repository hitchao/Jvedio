using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Common
{
    public static class DateHelper
    {
        public static string Now()
        {
            return DateTime.Now.toLocalDate();
        }
        public static string toLocalDate(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string toReadableTime(long ms)
        {
            string result = "";
            try
            {
                TimeSpan t = TimeSpan.FromMilliseconds(ms);
                result = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return result;
        }

        public static string toLocalDate(string str)
        {
            DateTime date = DateTime.Now;
            DateTime.TryParse(str, out date);
            return date.ToString("yyyy-MM-dd");
        }
    }
}
