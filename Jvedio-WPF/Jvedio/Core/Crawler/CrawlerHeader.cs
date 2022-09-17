using Jvedio.CommonNet.Entity;
using System.Collections.Generic;

namespace Jvedio.Core.Crawler
{
    public static class CrawlerHeader
    {
        private static Dictionary<string, string> DEFAULT_HEADERS = new Dictionary<string, string>()
        {
            {"User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36" },
        };



        public static RequestHeader GitHub { get; set; }

        public static RequestHeader Default { get; set; }

        static CrawlerHeader()
        {
            GitHub = new RequestHeader();
            GitHub.Method = System.Net.Http.HttpMethod.Get;
            GitHub.WebProxy = ConfigManager.ProxyConfig.GetWebProxy();
            GitHub.Headers = DEFAULT_HEADERS;
            Default = GitHub;
        }
    }
}
