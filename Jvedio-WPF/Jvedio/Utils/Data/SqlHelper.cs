using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Data
{
    public static class SqlHelper
    {
        public static string Format(object obj)
        {
            if (obj == null) return "";
            return Format(obj.ToString());
        }

        public static string Format(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("'", "''");
        }

        public static string handleNewLine(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\r\n", GlobalVariable.Separator.ToString())
                .Replace('\n', GlobalVariable.Separator);
        }
    }
}
