using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class PrimaryKeyTypeException : Exception
    {
        public PrimaryKeyTypeException() : base("主键未设置") { }
    }
}
