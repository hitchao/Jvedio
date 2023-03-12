using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class CrawlerNotSupportException : Exception
    {
        public CrawlerNotSupportException() : base(LangManager.GetValueByKey("NotSupportCrawler"))
        {
        }
    }
}
