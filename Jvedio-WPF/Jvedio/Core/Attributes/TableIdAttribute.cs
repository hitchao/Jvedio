using Jvedio.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Attributes
{

    [System.AttributeUsage(System.AttributeTargets.Property)]
    internal class TableIdAttribute : System.Attribute
    {
        public IdType type;

        public TableIdAttribute(IdType type)
        {
            this.type = type;
        }
    }
}
