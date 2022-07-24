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
    public class PluginConfig : AbstractConfig
    {

        private PluginConfig() : base("PluginConfig")
        {
            PluginList = "";
        }

        private static PluginConfig _instance = null;

        public static PluginConfig createInstance()
        {
            if (_instance == null) _instance = new PluginConfig();

            return _instance;
        }
        public string PluginList { get; set; }


        public void FetchPluginMetaData(Action onRefresh = null)
        {
            RequestHeader Header = CrawlerHeader.GitHub;
            Task.Run(async () =>
            {
                HttpResult httpResult = await HttpClient.Get(GlobalVariable.PLUGIN_LIST_URL, Header);
                if (httpResult.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
                {
                    // 更新插件
                    string json = httpResult.SourceCode;
                    if (!json.Equals(GlobalConfig.PluginConfig.PluginList))
                    {
                        GlobalConfig.PluginConfig.PluginList = json;
                        GlobalConfig.PluginConfig.Save();
                        onRefresh?.Invoke();
                    }
                }
            });
        }
    }
}
