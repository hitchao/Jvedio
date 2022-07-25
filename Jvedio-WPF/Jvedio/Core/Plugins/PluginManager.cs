using Jvedio.Core.Plugins.Crawler;
using Jvedio.Utils.Common;
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


        private static void MergeAllPlugin()
        {
            PluginList.AddRange(ThemeManager.PluginMetaDatas);
            PluginList.AddRange(CrawlerManager.PluginMetaDatas);

        }

        public static void Init()
        {
            MergeAllPlugin();
            SetPluginEnabled();

        }


        private static void SetPluginEnabled()
        {
            if (PluginList?.Count > 0
                && !string.IsNullOrEmpty(GlobalConfig.Settings.PluginEnabledJson))
            {

                string json = GlobalConfig.Settings.PluginEnabledJson;
                if (string.IsNullOrEmpty(json)) return;
                Dictionary<string, bool> dict = JsonUtils.TryDeserializeObject<Dictionary<string, bool>>(json);
                if (dict == null || dict.Count <= 0) return;
                foreach (PluginMetaData plugin in PluginList)
                {
                    string pluginID = plugin.PluginID;
                    if (string.IsNullOrEmpty(pluginID)) continue;
                    if (dict.ContainsKey(pluginID))
                        plugin.Enabled = dict[pluginID];
                }
            }
        }
    }
}
