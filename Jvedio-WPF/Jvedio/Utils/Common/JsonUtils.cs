using Jvedio.Logs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Common
{
    public static class JsonUtils
    {
        public static T TryDeserializeObject<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return default(T);
            }
        }

        public static string ClasstoJson(TypeInfo[] types)
        {
            if (types.Length <= 0) return "";
            string result = "";
            // 广度优先
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsNestedPrivate) continue;
                FieldInfo[] fieldInfos = types[i].GetTypeInfo().DeclaredFields.ToArray();// Field
                TypeInfo[] nestedTypes = types[i].GetTypeInfo().DeclaredNestedTypes.ToArray();// Class
                StringBuilder sb = new StringBuilder();
                foreach (var item in fieldInfos)
                {
                    if (!item.IsStatic || !item.IsPublic) continue;
                    sb.Append($"\"{item.Name}\":\"{item.GetValue(null)}\",");
                }
                if (sb.Length >= 1 && sb[sb.Length - 1].Equals(',')) sb.Remove(sb.Length - 1, 1);
                string fieldString = sb.ToString();
                string innerStr = ClasstoJson(nestedTypes);
                if (innerStr.Length > 0) fieldString += "," + innerStr;
                result += "\"" + types[i].Name + "\":{" + fieldString + "},";
            }
            if (result.EndsWith(",")) result = result.Substring(0, result.Length - 1);
            return result;
        }

        private static bool isBasicType(Type type)
        {
            return type == typeof(bool)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                ;
        }
    }


}
