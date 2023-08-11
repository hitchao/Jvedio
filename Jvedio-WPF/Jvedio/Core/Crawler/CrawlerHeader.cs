using SuperUtils.NetWork.Entity;
using System.Collections.Generic;

namespace Jvedio.Core.Crawler
{
    public static class CrawlerHeader
    {
        private static Dictionary<string, string> DEFAULT_HEADERS { get; set; } =
            new Dictionary<string, string>() {
                {
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36"
                },
            };

        public static RequestHeader GitHub { get; set; }

        public static RequestHeader Default { get; set; }

        public static System.Net.IWebProxy WebProxy { get; set; }

        static CrawlerHeader()
        {
            Init();
        }

        public static void Init()
        {
            WebProxy = ConfigManager.ProxyConfig.GetWebProxy();
            Default = new SuperUtils.NetWork.Crawler.CrawlerHeader(WebProxy).Default;
            GitHub = Default;
        }
    }
}
