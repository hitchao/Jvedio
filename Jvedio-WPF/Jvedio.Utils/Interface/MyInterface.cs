using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Interface
{
    public interface ILog
    {
         void Log(Exception ex);
        void Log(string str);
    }

}
