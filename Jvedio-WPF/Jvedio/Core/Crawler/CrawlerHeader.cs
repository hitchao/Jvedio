﻿using Jvedio.CommonNet.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Crawler
{
    public static class CrawlerHeader
    {

        public static RequestHeader GitHub { get; set; }
        static CrawlerHeader()
        {
            GitHub = new RequestHeader();
            GitHub.Method = System.Net.Http.HttpMethod.Get;
            GitHub.WebProxy = GlobalConfig.ProxyConfig.GetWebProxy();
            GitHub.Headers = new Dictionary<string, string>()
            {
                {"User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36" },
            };
        }
    }
}
