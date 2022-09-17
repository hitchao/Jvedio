using System;

namespace Jvedio.Core.Exceptions
{
    public class DllLoadFailedException : Exception
    {
        public DllLoadFailedException() : base("DLL加载失败！")
        {
        }

        public DllLoadFailedException(string path) : base($"DLL加载失败 => {path}")
        {
        }
    }
}
