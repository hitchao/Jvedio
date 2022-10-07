using SuperControls.Style;
using System;

namespace Jvedio.Core.Exceptions
{
    public class PrimaryKeyTypeException : Exception
    {
        public PrimaryKeyTypeException() : base(LangManager.GetValueByKey("PrimaryKeyNotSet"))
        {
        }
    }
}
