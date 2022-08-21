using Jvedio.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Common
{
    public static class CommonDataHelper
    {
        public static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonUtils.TrySerializeObject(obj);
            var dictionary = JsonUtils.TryDeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public static object TryGetMaxCountKey<T>(Dictionary<T, int> dict)
        {
            if (dict == null || dict.Count == 0) return null;
            try
            {
                return dict.FirstOrDefault(x => x.Value == dict.Values.Max()).Key;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
    }
}
