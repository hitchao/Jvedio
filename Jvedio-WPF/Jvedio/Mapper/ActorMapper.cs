using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class ActorMapper : BaseMapper<ActorInfo>
    {
        public ActorMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
