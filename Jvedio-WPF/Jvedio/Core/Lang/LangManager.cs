using System;
using System.Windows;
using static Jvedio.App;

namespace Jvedio.Core.Lang
{
    public static class LangManager
    {
        public static bool SetLang(string lang)
        {
            if (!SuperControls.Style.LangManager.SupportLanguages.Contains(lang)) {
                Logger.Warn($"lang not support: {lang}");
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
                        Logger.Error(ex);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
