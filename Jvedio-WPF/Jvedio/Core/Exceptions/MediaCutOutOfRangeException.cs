using System;

namespace Jvedio.Core.Exceptions
{
    public class MediaCutOutOfRangeException : Exception
    {
        public MediaCutOutOfRangeException() : base("需要获取的截图数量超出了视频的总帧数")
        {
        }
    }
}
