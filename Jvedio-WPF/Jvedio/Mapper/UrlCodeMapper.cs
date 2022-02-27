using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class UrlCodeMapper : BaseMapper<UrlCode>
    {
        public UrlCodeMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
