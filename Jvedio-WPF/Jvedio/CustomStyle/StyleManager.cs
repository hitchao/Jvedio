using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jvedio
{
    public static class StyleManager
    {
        public static BitmapSource BackgroundImage { get; set; }

        public static FontFamily GlobalFont { get; set; }

        public static string[] FontExt { get; set; } = new[] { ".otf", ".ttf" };

        public static class Common
        {
            public static class HighLight
            {
                public static SolidColorBrush Background =
                    (SolidColorBrush)Application.Current.Resources["Common.HighLight.Background"];
                public static SolidColorBrush BorderBrush =
                    (SolidColorBrush)Application.Current.Resources["Common.HighLight.BorderBrush"];
            }
        }
    }
}
