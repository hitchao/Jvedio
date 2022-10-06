using Jvedio.Core.Logs;
using System;

namespace Jvedio.Core.Exceptions
{
    public class SqlException : Exception
    {
        public SqlException(string sql, Exception ex) : base($"执行 sql 命令错误")
        {
            Logger.Warning("执行 sql 命令错误：=> " + sql);
            Logger.Error(ex);
        }
    }
}
