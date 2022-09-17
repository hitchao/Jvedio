using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Config.Base;
using Jvedio.Core.Crawler;
using Jvedio.Core.WindowConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class ThemeConfig : AbstractConfig
    {

        private ThemeConfig() : base("ThemeConfig")
        {
            ThemeIndex = 0;
            ThemeID = "";
        }

        private static ThemeConfig _instance = null;

        public static ThemeConfig createInstance()
        {
            if (_instance == null) _instance = new ThemeConfig();

            return _instance;
        }

        public string ThemeID { get; set; }
        public int ThemeIndex { get; set; }
    }
}
