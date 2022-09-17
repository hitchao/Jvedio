using System;

namespace Jvedio.Core.Exceptions
{
    public class CrawlerNotFoundException : Exception
    {
        public CrawlerNotFoundException() : base("无对应刮削器刮削")
        {
        }
    }
}
