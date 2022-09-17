using System;

namespace Jvedio.Core.Exceptions
{
    public class PrimaryKeyTypeException : Exception
    {
        public PrimaryKeyTypeException() : base("主键未设置")
        {
        }
    }
}
