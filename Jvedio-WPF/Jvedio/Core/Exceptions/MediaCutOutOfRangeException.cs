using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class MediaCutOutOfRangeException : Exception
    {
        public MediaCutOutOfRangeException() : base(LangManager.GetValueByKey("MediaCutOutOfRangeException"))
        {
        }
    }
}
