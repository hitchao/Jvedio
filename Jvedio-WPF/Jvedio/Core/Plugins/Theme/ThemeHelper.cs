using System;
using System.Windows;

namespace Jvedio.Core.Plugins.Theme
{
    public class ThemeHelper
    {
        public static void SetSkin(string themeName)
        {
            if ("白色".Equals(themeName))
            {
                Application.Current.Resources.MergedDictionaries[2].Source = new Uri("pack://application:,,,/SuperControls.Style;Component/XAML/Skin/White.xaml", UriKind.RelativeOrAbsolute);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries[2].Source = new Uri("pack://application:,,,/SuperControls.Style;Component/XAML/Skin/DefaultColor.xaml", UriKind.RelativeOrAbsolute);
            }

            // Theme theme = ThemeLoader.loadTheme(themeName);

            ////设置字体
            // GlobalFont = new FontFamily("微软雅黑");
            // if (theme.Font != null)
            // {
            //    var fonts = Fonts.GetFontFamilies(new Uri(theme.Font));
            //    if (fonts != null && fonts.Count >= 1) GlobalFont = fonts.First();
            // }
            // foreach (Window window in App.Current.Windows)
            // {
            //    window.FontFamily = GlobalFont;
            // }

            // Console.WriteLine(fonts);
        }
    }
}
