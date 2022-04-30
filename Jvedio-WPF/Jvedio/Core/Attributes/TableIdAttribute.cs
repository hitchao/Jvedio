using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Attributes
{

    [System.AttributeUsage(System.AttributeTargets.Property)]
    internal class TableFieldAttribute : System.Attribute
    {
        public bool exist = false;
        public bool select = true;

        public TableFieldAttribute(bool exist)
        {
            this.exist = exist;
        }


        public TableFieldAttribute(bool exist, bool select) : this(exist)
        {
            this.select = select;
        }
    }
}
