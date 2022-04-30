using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class CrawlerNotFoundException : Exception
    {
        public CrawlerNotFoundException() : base("无对应刮削器刮削") { }
    }
}
