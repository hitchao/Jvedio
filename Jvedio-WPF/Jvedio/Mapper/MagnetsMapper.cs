using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class MagnetsMapper : BaseMapper<Magnet>
    {
        public MagnetsMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
