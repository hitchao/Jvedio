using Jvedio;
using SuperUtils.Reflections;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace SuperUtils.Visual
{
    public static class VisualHelper
    {
        private static System.Windows.Media.BrushConverter converter =
            new System.Windows.Media.BrushConverter();
        public static Brush HexStringToBrush(string hexString)
        {
            try
            {
                return (Brush)converter.ConvertFromString(hexString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;

        }

        private static System.Windows.Media.Color GetColorFromRGBSring(string rgb)
        {
            if (rgb.IndexOf("rgb(") < 0 || rgb.Split(',').Count() != 3) return System.Windows.Media.Color.FromRgb(0, 0, 0);

            rgb = rgb.Replace("rgb(", "").Replace(")", "").Replace(" ", "");
            string[] colors = rgb.Split(',');
            byte.TryParse(colors[0], out byte r);
            byte.TryParse(colors[1], out byte g);
            byte.TryParse(colors[2], out byte b);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

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
                Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Jvedio");
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

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public static Window GetWindowByName(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }
            return null;
        }

        public static T FindParentOfType<T>(this FrameworkElement child, string name = "") where T : FrameworkElement
        {
            if (string.IsNullOrEmpty(name) && child == null) return null;
            FrameworkElement parentDepObj = child;
            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj) as FrameworkElement;
                T parent = parentDepObj as T;
                if (string.IsNullOrEmpty(name) && parent != null) return parent;
                if (parent != null && name.Equals(parent.Name)) return parent;
            }
            while (parentDepObj != null);
            return null;
        }

        public static SolidColorBrush RGBAToBrush(string[] args)
        {
            if (args == null || args.Length < 3) return null;

            byte.TryParse(args[0].Trim(), out byte r);
            byte.TryParse(args[1].Trim(), out byte g);
            byte.TryParse(args[2].Trim(), out byte b);

            byte a = 255;
            if (args.Length == 4)
                byte.TryParse(args[3].Trim(), out a);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public static string SerilizeBrush(SolidColorBrush brush)
        {
            if (brush == null) return "0,0,0,0";
            Color color = brush.Color;
            return $"{color.R},{color.G},{color.B},{color.A}";
        }

        public static T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            if (element == null) return childElement;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }

            return childElement;
        }
    }
}
