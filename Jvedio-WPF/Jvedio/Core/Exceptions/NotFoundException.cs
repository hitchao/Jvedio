using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string fileName)
            : base($"{Jvedio.Language.Resources.NotFound} {fileName}")
        {
        }
    }
}
