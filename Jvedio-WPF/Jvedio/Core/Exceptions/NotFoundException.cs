using System;

namespace Jvedio.Core.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string fileName)
            : base($"{Jvedio.Language.Resources.NotFound} {fileName}")
        {
        }
    }
}
