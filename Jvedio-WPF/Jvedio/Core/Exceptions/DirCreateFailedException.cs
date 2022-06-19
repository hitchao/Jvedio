using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class DirCreateFailedException : Exception
    {
        public DirCreateFailedException(string dir) : base($"文件夹创建失败：{dir}") { }
    }
}
