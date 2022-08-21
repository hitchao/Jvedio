using Jvedio.Core.Exceptions;
using Jvedio.Utils.Reflections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Plugins
{
    public class Plugin
    {
        public string DllPath { get; set; }
        public string MethodName { get; set; }
        public object[] Params { get; set; }

        public Plugin(string dllPath, string methodName, object[] @params)
        {
            if (!File.Exists(dllPath) || string.IsNullOrEmpty(methodName))
                throw new CrawlerNotFoundException();
            DllPath = dllPath;
            MethodName = methodName;
            Params = @params;
            if (Params == null || Params.Length != 3)
                throw new ArgumentException("Params Length must be 3");
        }

        public async Task<object> InvokeAsyncMethod()
        {
            if (!File.Exists(DllPath)) throw new DllLoadFailedException(DllPath);
            Type classType = null;
            object instance = null;
            try
            {
                Assembly dll = ReflectionHelper.TryLoadAssembly(DllPath);
                classType = getPublicType(dll.GetTypes());
                instance = ReflectionHelper.TryCreateInstance(classType, new object[] { Params[1], Params[2] });
            }
            catch (Exception)
            {
                throw new DllLoadFailedException(DllPath);
            }
            if (classType == null || instance == null) throw new DllLoadFailedException(DllPath);
            MethodInfo methodInfo = classType.GetMethod(MethodName);
            if (methodInfo == null) throw new DllLoadFailedException(DllPath);
            try
            {
                return await (Task<Dictionary<string, object>>)methodInfo.
                    Invoke(instance, new object[] { (bool)Params[0] });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Type getPublicType(Type[] types)
        {
            if (types == null || types.Length == 0) return null;
            foreach (Type type in types)
            {
                if (type.IsPublic) return type;
            }
            return null;
        }

    }
}
