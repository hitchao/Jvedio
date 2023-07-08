using Jvedio.Core.Config.Base;
using Jvedio.Core.Crawler;
using Jvedio.Core.Global;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Entity;
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

            DownloadInfo = true;
            DownloadThumbNail = true;
            DownloadPoster = true;
            DownloadPreviewImage = false;
            DownloadActor = true;
        }

        public bool DownloadInfo { get; set; }
        public bool DownloadThumbNail { get; set; }
        public bool DownloadPoster { get; set; }
        public bool DownloadPreviewImage { get; set; }
        public bool DownloadActor { get; set; }

        private static PluginConfig _instance = null;

        public static PluginConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new PluginConfig();

            return _instance;
        }

        public string PluginList { get; set; }
        public string DeleteList { get; set; }

        public void FetchPluginMetaData(Action onRefresh = null)
        {
            RequestHeader header = CrawlerHeader.GitHub;
            Task.Run(async () => {
                HttpResult httpResult = null;
                try {
                    httpResult = await HttpClient.Get(UrlManager.PLUGIN_LIST_URL, header, SuperUtils.NetWork.Enums.HttpMode.String);
                } catch (TimeoutException) { } catch (Exception) { }
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode)) {
                    // 更新插件
                    string json = httpResult.SourceCode;
                    if (!json.Equals(ConfigManager.PluginConfig.PluginList)) {
                        ConfigManager.PluginConfig.PluginList = json;
                        ConfigManager.PluginConfig.Save();
                        onRefresh?.Invoke();
                    }
                }
            });
        }
    }
}
