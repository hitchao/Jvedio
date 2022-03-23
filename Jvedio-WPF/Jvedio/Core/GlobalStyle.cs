using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Jvedio
{
    public static class GlobalStyle
    {
        public static class Common
        {
            public static class HighLight
            {
                public static SolidColorBrush Background = (SolidColorBrush)Application.Current.Resources["Common.HighLight.Background"];
                public static SolidColorBrush BorderBrush = (SolidColorBrush)Application.Current.Resources["Common.HighLight.BorderBrush"];
            }

        }
    }
}
