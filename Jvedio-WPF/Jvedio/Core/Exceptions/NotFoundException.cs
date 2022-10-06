using System;

namespace Jvedio.Core.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string fileName)
            : base($"{SuperControls.Style.LangManager.GetValueByKey("NotFound")} {fileName}")
        {
        }
    }
}
