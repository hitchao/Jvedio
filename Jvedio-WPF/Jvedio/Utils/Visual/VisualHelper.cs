using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Jvedio.Utils.Visual
{
    public static class VisualHelper
    {



        public static T FindParentOfType<T>(this FrameworkElement child, string name = "") where T : FrameworkElement
        {
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
