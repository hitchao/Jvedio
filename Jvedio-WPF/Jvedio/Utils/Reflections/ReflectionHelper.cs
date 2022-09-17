using Jvedio.Core.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperUtils.Reflections
{
    public static class ReflectionHelper
    {

        public static Assembly TryLoadAssembly(string path)
        {
            try
            {
                return Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
        public static object TryCreateInstance(Type type, params object[] args)
        {
            try
            {
                return Activator.CreateInstance(type, args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }

        }
    }
}
