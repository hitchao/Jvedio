using Jvedio.Core.SqlMapper;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class ComImagesMapper : BaseMapper<CommonImage>
    {
        public ComImagesMapper(string sqlitePath) : base(sqlitePath)
        {
        }
    }
}
