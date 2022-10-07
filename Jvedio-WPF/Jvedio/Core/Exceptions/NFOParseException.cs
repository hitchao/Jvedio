using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class NFOParseException : Exception
    {
        public NFOParseException(string path) : base($"{LangManager.GetValueByKey("ParseNfoInfoFailFromFile")} {path}")
        {
        }
    }
}
