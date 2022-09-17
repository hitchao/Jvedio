using SuperUtils.Common;
using SuperUtils.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    public class Serilizable
    {
        public override string ToString()
        {
            return ClassUtils.ToString(this);
        }
    }
}
