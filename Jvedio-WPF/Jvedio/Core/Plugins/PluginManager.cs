using Jvedio.Core.Plugins.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Plugins
{
    public class PluginManager
    {
        public static List<PluginMetaData> PluginList = new List<PluginMetaData>();


        public static void MergeAllPlugin()
        {
            PluginList.AddRange(ThemeManager.PluginMetaDatas);
            PluginList.AddRange(CrawlerManager.PluginMetaDatas);
            Console.WriteLine();
        }
    }
}
