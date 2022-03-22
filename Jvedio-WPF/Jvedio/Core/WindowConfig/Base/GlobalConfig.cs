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
        public static Jvedio.Core.WindowConfig.Main Main = Jvedio.Core.WindowConfig.Main.createInstance();

        static GlobalConfig()
        {
            StartUp.Read();
            Main.Read();
        }
    }
}
