using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Sqlite
{
    public static class SqliteHelper
    {
        public static string format(object obj)
        {
            if (obj == null) return "";
            return format(obj.ToString());
        }

        public static string format(string str)
        {
            return str.Replace("'", "''");
        }

        public static string handleNewLine(string str)
        {
            return str.Replace('\n', GlobalVariable.Separator);
        }
    }
}
