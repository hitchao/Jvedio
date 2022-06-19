using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class MediaCutOutOfRangeException : Exception
    {
        public MediaCutOutOfRangeException() : base("需要获取的截图数量超出了视频的总帧数") { }
    }
}
