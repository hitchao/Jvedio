using System;

namespace Jvedio.Core.Exceptions
{
    public class DirCreateFailedException : Exception
    {
        public DirCreateFailedException(string dir) : base($"文件夹创建失败：{dir}") { }
    }
}
