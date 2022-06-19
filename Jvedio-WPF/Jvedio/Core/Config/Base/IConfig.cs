using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config.Base
{
    public interface IConfig
    {
        void Save();

        void Read();
    }


}
