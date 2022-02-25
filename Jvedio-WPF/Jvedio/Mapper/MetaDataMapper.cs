using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class MetaDataMapper : BaseMapper<MetaData>
    {
        public MetaDataMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
