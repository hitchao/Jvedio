using Jvedio.Utils.Common;
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
            return ClassUtils.toString(this);
        }
    }
}
