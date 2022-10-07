using Jvedio.Core;
using SuperControls.Style;
using SuperUtils.IO;
using SuperUtils.WPF.VisualTools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_SetSkinLang : SuperControls.Style.BaseDialog
    {
        public Dialog_SetSkinLang(Window owner) : base(owner, false)
        {
            InitializeComponent();
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            int.TryParse(radioButton.Tag.ToString(), out int idx);
            ConfigManager.ThemeConfig.ThemeIndex = idx;
            Entity.Theme theme = ThemeManager.Themes[idx];
            ConfigManager.ThemeConfig.ThemeID = theme.ID;
            ConfigManager.ThemeConfig.Save();
            if (theme.Colors != null && theme.Colors.Count > 0)
            {
                foreach (string key in theme.Colors.Keys)
                {
                    Application.Current.Resources[key] = VisualHelper.HexStringToBrush(theme.Colors[key]);
                }
            }
        }

        private void SetLang(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            string lang = radioButton.Tag.ToString();
            SuperControls.Style.LangManager.SetLang(lang);
            Jvedio.Core.Lang.LangManager.SetLang(lang);
            ConfigManager.Settings.CurrentLanguage = lang;
            ConfigManager.Settings.Save();
        }
    }
}