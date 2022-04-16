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
        public static Jvedio.Core.WindowConfig.Edit Edit = Jvedio.Core.WindowConfig.Edit.createInstance();
        public static Jvedio.Core.WindowConfig.Detail Detail = Jvedio.Core.WindowConfig.Detail.createInstance();
        public static Jvedio.Core.WindowConfig.MetaData MetaData = Jvedio.Core.WindowConfig.MetaData.createInstance();

        static GlobalConfig()
        {
            StartUp.Read();
            Main.Read();
            Edit.Read();
            Detail.Read();
            MetaData.Read();
        }
    }
}
