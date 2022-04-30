using Jvedio.Core.SimpleORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomEventArgs
{
    public class WrapperEventArg<T> : EventArgs
    {
        public IWrapper<T> Wrapper { get; set; }
        public string SQL { get; set; }
    }

}
