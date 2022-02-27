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
    }
}
