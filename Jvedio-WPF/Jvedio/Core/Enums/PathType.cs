using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Enums
{

    /// <summary>
    /// 0-绝对路径 
    /// 1-相对于Jvedio路径 
    /// 2-相对于影片路径 
    /// 3-网络绝对路径
    /// </summary>
    public enum PathType
    {
        Absolute,
        RelativeToApp,
        RelativeToData,
        AbsoluteUrl
    }
}
