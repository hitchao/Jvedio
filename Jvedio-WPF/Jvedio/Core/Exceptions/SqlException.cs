using Jvedio.Core.Logs;
using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class SqlException : Exception
    {
        public SqlException(string sql, Exception ex) : base(LangManager.GetValueByKey("ExeSqlError"))
        {
            Logger.Warning($"{LangManager.GetValueByKey("ExeSqlError")} => {sql}");
            Logger.Error(ex);
        }
    }
}
