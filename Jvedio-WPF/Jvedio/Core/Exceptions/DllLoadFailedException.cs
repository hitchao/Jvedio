using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class DllLoadFailedException : Exception
    {
        public DllLoadFailedException() : base(LangManager.GetValueByKey("DllLoadFailed"))
        {
        }

        public DllLoadFailedException(string path) : base($"{LangManager.GetValueByKey("DllLoadFailed")} => {path}")
        {
        }
    }
}
