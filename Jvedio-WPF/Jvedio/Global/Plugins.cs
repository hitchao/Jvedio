using Jvedio.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Global
{
    public static class Plugins
    {
        public static HashSet<PluginInfo> Crawlers { get; set; }


        static Plugins()
        {
            Crawlers = new HashSet<PluginInfo>();
        }


    }
}
