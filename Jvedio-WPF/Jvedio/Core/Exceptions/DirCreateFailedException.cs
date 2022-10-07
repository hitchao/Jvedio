using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class DirCreateFailedException : Exception
    {
        public DirCreateFailedException(string dir) : base($"{LangManager.GetValueByKey("CreateDirFailed")} {dir}")
        {
        }
    }
}
