using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Exceptions
{
    public class NFOParseException : Exception
    {
        public NFOParseException(string path) : base($"无法从该文件识别出有效的 NFO 信息：{path}") { }
    }
}
