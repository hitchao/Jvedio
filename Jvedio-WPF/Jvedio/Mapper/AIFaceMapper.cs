using Jvedio.Core.SqlMapper;
using Jvedio.Entity.CommonSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class AIFaceMapper : BaseMapper<AIFaceInfo>
    {
        public AIFaceMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
