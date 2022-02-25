using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class AppConfigMapper : BaseMapper<AppConfig>
    {
        public AppConfigMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
