using Jvedio.Core.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class SqlException : Exception
    {
        public SqlException(string sql, Exception ex) : base($"执行 sql 命令错误")
        {
            Logger.Error("执行 sql 命令错误：=> " + sql);
            Logger.Error(ex);
        }
    }
}
