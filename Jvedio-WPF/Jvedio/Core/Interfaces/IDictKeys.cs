using System.Collections.Generic;

namespace Jvedio.Core.Interfaces
{
    public interface IDictKeys
    {
        /// <summary>
        /// 类的所有属性是否都在字典中
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        bool HasAllKeys(Dictionary<object, object> d);
    }
}
