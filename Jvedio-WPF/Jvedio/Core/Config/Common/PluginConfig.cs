using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Config.Base;
using Jvedio.Core.Crawler;
using Jvedio.Core.Global;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class PluginConfig : AbstractConfig
    {
        private PluginConfig() : base("PluginConfig")
        {
            PluginList = string.Empty;
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
            RequestHeader header = CrawlerHeader.GitHub;
            Task.Run(async () =>
            {
                HttpResult httpResult = await HttpClient.Get(UrlManager.PLUGIN_LIST_URL, header);
                if (httpResult.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
                {
                    // 更新插件
                    string json = httpResult.SourceCode;
                    if (!json.Equals(ConfigManager.PluginConfig.PluginList))
                    {
                        ConfigManager.PluginConfig.PluginList = json;
                        ConfigManager.PluginConfig.Save();
                        onRefresh?.Invoke();
                    }
                }
            });
        }
    }
}
