using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Common
{
    public static class ClassUtils
    {
        public static string toString<T>(T entity, bool newLine = false)
        {
            System.Reflection.PropertyInfo[] propertyInfos = entity.GetType().GetProperties();
            StringBuilder builder = new StringBuilder();
            builder.Append(entity.GetType().Name + " => ");
            foreach (var item in propertyInfos)
            {
                builder.Append($"{item.Name}={item.GetValue(entity)}{(newLine ? Environment.NewLine : ",")}");
            }
            return builder.ToString();
        }
    }
}
