using SuperUtils.Reflections;
using SuperUtils.WPF.VisualTools;
using System;
using System.Reflection;
using System.Windows;

namespace Jvedio.VisualTools
{
    public static class WindowHelper
    {
        // todo 有些窗体名字没改，打不开
        // todo 检视
        public static void OpenWindowByName(string name, Window parent = null)
        {
            if (string.IsNullOrEmpty(name)) return;
            bool dialog = name.StartsWith("Dialog_");

            Window window = GetWindowByName(name);
            if (window == null)
            {
                // 反射加载
                Type[] typelist = VisualHelper.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Jvedio");
                foreach (Type type in typelist)
                {
                    if (name.Equals(type.Name))
                    {
                        object instance = null;
                        if (dialog)
                            instance = ReflectionHelper.TryCreateInstance(type, new object[] { parent });
                        else
                            instance = ReflectionHelper.TryCreateInstance(type, null);
                        if (instance != null) window = instance as Window;
                        break;
                    }
                }
            }

            if (window != null)
            {
                if (dialog)
                {
                    if (!window.IsVisible)
                        window.ShowDialog();
                }
                else
                {
                    window.Show();
                    window.BringIntoView();
                    window.Focus();
                }
            }
        }

        public static Window GetWindowByName(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }

            return null;
        }
    }
}
