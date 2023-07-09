using SuperControls.Style;
using System;
using static Jvedio.App;

namespace Jvedio.Core.Exceptions
{
    public class SqlException : Exception
    {
        public SqlException(string sql, Exception ex) : base(LangManager.GetValueByKey("ExeSqlError"))
        {
            Logger.Warn($"{LangManager.GetValueByKey("ExeSqlError")} => {sql}");
            Logger.Error(ex);
        }
    }
}
