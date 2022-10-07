using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class CrawlerNotFoundException : Exception
    {
        public CrawlerNotFoundException() : base(LangManager.GetValueByKey("NoCrawler"))
        {
        }
    }
}
