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
            DllPath = dllPath;
            MethodName = methodName;
            Params = @params;
        }

        public object invokeMethod()
        {
            if (File.Exists(DllPath))
            {
                Assembly dll = Assembly.LoadFrom(DllPath);
                Type classType = getPublicType(dll.GetTypes());
                if (classType != null)
                {
                    Dictionary<string, string> infos = getInfo(classType);
                    var instance = Activator.CreateInstance(classType, Params);
                    MethodInfo methodInfo = classType.GetMethod(MethodName);
                    if (methodInfo != null)
                    {
                        return methodInfo.Invoke(instance, null);
                    }
                }
            }
            return null;
        }
        public async Task<object> InvokeAsyncMethod()
        {
            if (File.Exists(DllPath))
            {
                Assembly dll = Assembly.LoadFrom(DllPath);
                Type classType = getPublicType(dll.GetTypes());
                if (classType != null)
                {
                    Dictionary<string, string> infos = getInfo(classType);
                    var instance = Activator.CreateInstance(classType, Params);
                    MethodInfo methodInfo = classType.GetMethod(MethodName);
                    if (methodInfo != null)
                    {
                        try
                        {
                            return await (Task<Dictionary<string, object>>)methodInfo.Invoke(instance, null);
                            //return result;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return null;
                        }

                    }
                }
            }
            return null;
        }
        public async Task<object> AsyncInvokeMethod()
        {
            if (File.Exists(DllPath))
            {
                Assembly dll = Assembly.LoadFrom(DllPath);
                Type classType = getPublicType(dll.GetTypes());
                if (classType != null)
                {
                    Dictionary<string, string> infos = getInfo(classType);
                    var instance = Activator.CreateInstance(classType, Params);
                    MethodInfo methodInfo = classType.GetMethod(MethodName);
                    if (methodInfo != null)
                    {
                        return await Task.Run(() =>
                        {
                            return methodInfo.Invoke(instance, null);
                        });
                    }
                }
            }
            return null;
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

        private Dictionary<string, string> getInfo(Type type)
        {
            FieldInfo fieldInfo = type.GetField("Infos");
            if (fieldInfo != null)
            {
                object value = fieldInfo.GetValue(null);
                if (value != null)
                {
                    try
                    {
                        return (Dictionary<string, string>)value;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                }
            }
            return null;
        }

    }
}
