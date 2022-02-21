using Jvedio.Core.Attributes;
using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{

    [Table(tableName: "common_transaltions")]
    public class ComTranslationMapper : BaseMapper<Translation>
    {
        public ComTranslationMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
