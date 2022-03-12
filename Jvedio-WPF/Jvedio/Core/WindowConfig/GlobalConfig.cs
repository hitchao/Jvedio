using Jvedio.Core.WindowConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class GlobalConfig
    {
        public static StartUp StartUp = StartUp.createInstance();

        static GlobalConfig()
        {
            StartUp.Read();
        }
    }
}
