using System;
using System.Windows;

namespace Jvedio.Core.Lang
{
    public static class LangManager
    {
        public static bool SetLang(string lang)
        {
            if (!SuperControls.Style.LangManager.SupportLanguages.Contains(lang)) {
                Console.WriteLine("不支持的语言：" + lang);
                return false;
            }

            string format = "pack://application:,,,/Jvedio;Component/Core/Lang/{0}.xaml";
            format = string.Format(format, lang);
            foreach (ResourceDictionary mergedDictionary in Application.Current.Resources.MergedDictionaries) {
                if (mergedDictionary.Source != null && mergedDictionary.Source.OriginalString.Contains("Jvedio;Component/Core/Lang")) {
                    try {
                        bool flag = Application.Current.Resources.MergedDictionaries.Remove(mergedDictionary);
                        mergedDictionary.Source = new Uri(format, UriKind.RelativeOrAbsolute);
                        Application.Current.Resources.MergedDictionaries.Add(mergedDictionary);
                        return true;
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
