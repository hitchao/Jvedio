using DynamicData.Annotations;
using Jvedio.Core;
using Newtonsoft.Json.Linq;
using SuperUtils.Media;
using SuperUtils.WPF.VisualTools;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    public class ThemeImage
    {
        public string Background { get; set; }

        public string Big { get; set; }

        public string Small { get; set; }

        public string Normal { get; set; }
    }

    public class Theme : INotifyPropertyChanged
    {
        public string ID { get; set; }

        public ThemeImage Images { get; set; }

        public string FontName { get; set; }

        public string FontPath { get; set; }

        public string Desc { get; set; }

        private float _BgColorOpacity = 1;

        public float BgColorOpacity
        {
            get { return _BgColorOpacity; }

            set
            {
                if (value < 0)
                    _BgColorOpacity = 0;
                else if (value > 1)
                    _BgColorOpacity = 1;
                else
                    _BgColorOpacity = value;
            }
        }

        public Dictionary<string, string> Colors { get; set; }

        public Brush _ViewBrush;

        public Brush ViewBrush
        {
            get { return _ViewBrush; }

            set
            {
                _ViewBrush = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage _ViewImage;

        public BitmapImage ViewImage
        {
            get { return _ViewImage; }

            set
            {
                _ViewImage = value;
                OnPropertyChanged();
            }
        }

        public static string ViewBrushKey = "Window.Title.Background";

        public static Dictionary<string, string> DEFAULT_BALCK_COLORS = new Dictionary<string, string>()
        {
    { "Window.Background", "#1E1E1E" },
    { "Window.Foreground", "#FFFFFF" },
    { "Window.Sub.Background", "#1E1E1E" },
    { "Window.Sub.Foreground", "#C5C5C5" },
    { "Window.Side.Background", "#252526" },
    { "Window.Side.Foreground", "#FFFFFF" },
    { "Window.Side.Hover.Background", "#B237373D" },
    { "Window.Side.Hover.Foreground", "#B2FFFFFF" },
    { "Window.Detail.Background", "#1B1B1F" },
    { "Window.Detail.Foreground", "#FFFFFF" },
    { "Window.InnerDialog.Background", "#1B1B1F" },
    { "Window.InnerDialog.Foreground", "#FFFFFF" },
    { "Window.ToolsBar.Background", "#383838" },
    { "Window.ToolsBar.Foreground", "#FFFFFF" },
    { "Window.Title.Background", "#323233" },
    { "Window.Title.Foreground", "#FFFFFF" },
    { "Window.Title.Hover.Background", "#B23F3F41" },
    { "Window.Title.Hover.Foreground", "#B2FFFFFF" },
    { "Window.Title.Hover.Deep.Background", "#FF0000" },
    { "Window.Title.Hover.Deep.Foreground", "#B2FFFFFF" },
    { "Window.StatusBar.Background", "#007ACC" },
    { "Window.StatusBar.Foreground", "#FFFFFF" },
    { "Window.StatusBar.Hover.Background", "#B23F3F41" },
    { "Window.StatusBar.Hover.Foreground", "#B2FFFFFF" },
    { "Control.Background", "#2D2D30" },
    { "Control.Foreground", "#FFFFFF" },
    { "Control.Hover.Background", "#2D2D30" },
    { "Control.Hover.Foreground", "#FFFFFF" },
    { "Control.Disabled.Background", "#808080" },
    { "Control.Disabled.Foreground", "#2D2D30" },
    { "Control.Disabled.BorderBrush", "#2D2D30" },
    { "Button.Selected.Background", "#326BF1" },
    { "Button.Selected.BorderBrush", "#007FD4" },
    { "Button.Selected.Foreground", "#FFFFFF" },
    { "Button.Selected.NotActive.Background", "#37373D" },
    { "Button.Selected.NotActive.Foreground", "#37373D" },
    { "Button.Hover.Background", "#B22D2D30" },
    { "Button.Hover.BorderBrush", "#FFFFFF" },
    { "Button.Fill.Background", "#1A97ED" },
    { "Button.Fill.Foreground", "#FFFFFF" },
    { "ScollViewer.Background", "#00000000" },
    { "ScollViewer.Thumb.Background", "#424242" },
    { "ScollViewer.Thumb.Hover.Background", "#4F4F4F" },
    { "Menu.Background", "#212122" },
    { "Menu.Foreground", "#FFFFFF" },
    { "Menu.Hover.Background", "#1F466E" },
    { "Menu.Hover.Foreground", "#FFFFFF" },
    { "Menu.Hover.Deep.Background", "#FF0000" },
    { "Menu.Hover.Deep.Foreground", "#FFFF00" },
    { "Popup.Background", "#252526" },
    { "Popup.Foreground", "#FFFFFF" },
    { "ListBoxItem.Background", "#101013" },
    { "ListBoxItem.Foreground", "#CCCCCC" },
    { "ListBoxItem.Hover.Background", "#2D2D30" },
    { "ListBoxItem.Hover.Foreground", "#FFFFFF" },
    { "ListBoxItem.Selected.Active.Background", "#094771" },
    { "ListBoxItem.Selected.Active.Foreground", "#FFFFFF" },
    { "ListBoxItem.Selected.Active.BorderBrush", "#007FD4" },
    { "ListBoxItem.Selected.NotActive.Background", "#686868" },
    { "TabItem.Background", "#2D2D2D" },
    { "TabItem.Foreground", "#FFFFFF" },
    { "TabItem.Hover.Background", "#3B3C3C" },
    { "TabItem.Hover.Foreground", "#FFFFFF" },
    { "TabItem.Selected.Background", "#1E1E1E" },
    { "TabItem.Selected.Foreground", "#FFFFFF" },
    { "TabItem.Selected.Deep.Background", "#ED7547" },
    { "TabItem.Selected.Deep.Foreground", "#FFFFFF" },
    { "DataGrid.Header.Background", "#222222" },
    { "DataGrid.Header.Foreground", "#FFFFFF" },
    { "DataGrid.Header.Hover.Background", "#2A2D2E" },
    { "DataGrid.Header.Hover.Foreground", "#FFFFFF" },
    { "DataGrid.Row.Even.Background", "#1E1E1E" },
    { "DataGrid.Row.Even.Foreground", "#FFFFFF" },
    { "DataGrid.Row.Odd.Background", "#252525" },
    { "DataGrid.Row.Odd.Foreground", "#FFFFFF" },
    { "DataGrid.Row.Hover.Background", "#2A2D2E" },
    { "DataGrid.Row.Hover.Foreground", "#FFFFFF" },
    { "Common.HighLight.Background", "#094771" },
    { "Common.HighLight.Deep.Background", "#FF7B3B" },
    { "Common.HighLight.BorderBrush", "#007FD4" },
    { "Common.Status.Running.Background", "#007FD4" },
    { "Common.Status.Complete.Background", "#20B759" },
    { "CheckBox.Background", "#FFFFFF" },
    { "CheckBox.Checked.Background", "#38C550" },
    { "CheckBox.Checked.Foreground", "#FFFFFF" },
    { "CheckBox.Checked.Hover.Background", "#008000" },
    { "TextBox.PlaceHolder.Foreground", "#808080" },
};

        public static Dictionary<string, string> DEFAULT_WHITE_COLORS = new Dictionary<string, string>()
        {
    { "Window.Background", "#FFFFFF" },
    { "Window.Foreground", "#242424" },
    { "Window.Sub.Background", "#FFFFFF" },
    { "Window.Sub.Foreground", "#000000" },
    { "Window.Side.Background", "#F3F3F3" },
    { "Window.Side.Foreground", "#242424" },
    { "Window.Side.Hover.Background", "#B2E8E8E8" },
    { "Window.Side.Hover.Foreground", "#B2242424" },
    { "Window.Detail.Background", "#FFFFFF" },
    { "Window.Detail.Foreground", "#242424" },
    { "Window.InnerDialog.Background", "#FFFFFF" },
    { "Window.InnerDialog.Foreground", "#242424" },
    { "Window.ToolsBar.Background", "#ECECEC" },
    { "Window.ToolsBar.Foreground", "#242424" },
    { "Window.Title.Background", "#DDDDDD" },
    { "Window.Title.Foreground", "#242424" },
    { "Window.Title.Hover.Background", "#B2C6C6C6" },
    { "Window.Title.Hover.Foreground", "#B2242424" },
    { "Window.Title.Hover.Deep.Background", "#FF0000" },
    { "Window.Title.Hover.Deep.Foreground", "#B2242424" },
    { "Window.StatusBar.Background", "#007ACC" },
    { "Window.StatusBar.Foreground", "#242424" },
    { "Window.StatusBar.Hover.Background", "#B23F3F41" },
    { "Window.StatusBar.Hover.Foreground", "#B2242424" },
    { "Control.Background", "#E1E1E1" },
    { "Control.Foreground", "#242424" },
    { "Control.Hover.Background", "#E1E1E1" },
    { "Control.Hover.Foreground", "#242424" },
    { "Control.Disabled.Background", "#E1E1E1" },
    { "Control.Disabled.Foreground", "#242424" },
    { "Control.Disabled.BorderBrush", "#808080" },
    { "Button.Selected.Background", "#326BF1" },
    { "Button.Selected.BorderBrush", "#007FD4" },
    { "Button.Selected.Foreground", "#242424" },
    { "Button.Selected.NotActive.Background", "#E4E6F1" },
    { "Button.Selected.NotActive.Foreground", "#E4E6F1" },
    { "Button.Hover.Background", "#B2E9E9E9" },
    { "Button.Hover.BorderBrush", "#242424" },
    { "Button.Fill.Background", "#1A97ED" },
    { "Button.Fill.Foreground", "#242424" },
    { "ScollViewer.Background", "#00000000" },
    { "ScollViewer.Thumb.Background", "#BABABA" },
    { "ScollViewer.Thumb.Hover.Background", "#BABABA" },
    { "Menu.Background", "#FFFFFF" },
    { "Menu.Foreground", "#000000" },
    { "Menu.Hover.Background", "#0060C0" },
    { "Menu.Hover.Foreground", "#242424" },
    { "Menu.Hover.Deep.Background", "#FF0000" },
    { "Menu.Hover.Deep.Foreground", "#FFFF00" },
    { "Popup.Background", "#FF0000" },
    { "Popup.Foreground", "#242424" },
    { "ListBoxItem.Background", "#FFFFFF" },
    { "ListBoxItem.Foreground", "#242424" },
    { "ListBoxItem.Hover.Background", "#EEEEEE" },
    { "ListBoxItem.Hover.Foreground", "#242424" },
    { "ListBoxItem.Selected.Active.Background", "#326CF3" },
    { "ListBoxItem.Selected.Active.Foreground", "#242424" },
    { "ListBoxItem.Selected.Active.BorderBrush", "#007FD4" },
    { "ListBoxItem.Selected.NotActive.Background", "#F5F5F5" },
    { "TabItem.Background", "#EEEEEE" },
    { "TabItem.Foreground", "#242424" },
    { "TabItem.Hover.Background", "#EEEEEE" },
    { "TabItem.Hover.Foreground", "#242424" },
    { "TabItem.Selected.Background", "#EEEEEE" },
    { "TabItem.Selected.Foreground", "#242424" },
    { "TabItem.Selected.Deep.Background", "#ED7547" },
    { "TabItem.Selected.Deep.Foreground", "#242424" },
    { "DataGrid.Header.Background", "#EEEEEE" },
    { "DataGrid.Header.Foreground", "#242424" },
    { "DataGrid.Header.Hover.Background", "#2A2D2E" },
    { "DataGrid.Header.Hover.Foreground", "#242424" },
    { "DataGrid.Row.Even.Background", "#FFFFFF" },
    { "DataGrid.Row.Even.Foreground", "#242424" },
    { "DataGrid.Row.Odd.Background", "#F9F9F9" },
    { "DataGrid.Row.Odd.Foreground", "#242424" },
    { "DataGrid.Row.Hover.Background", "#2A2D2E" },
    { "DataGrid.Row.Hover.Foreground", "#242424" },
    { "Common.HighLight.Background", "#094771" },
    { "Common.HighLight.Deep.Background", "#FF7B3B" },
    { "Common.HighLight.BorderBrush", "#007FD4" },
    { "Common.Status.Running.Background", "#007FD4" },
    { "Common.Status.Complete.Background", "#20B759" },
    { "CheckBox.Background", "#C0C0C0" },
    { "CheckBox.Checked.Background", "#38C550" },
    { "CheckBox.Checked.Foreground", "#FFFFFF" },
    { "CheckBox.Checked.Hover.Background", "#008000" },
    { "TextBox.PlaceHolder.Foreground", "#4B4B4B" },
};

        public static List<string> ColorKeys = DEFAULT_BALCK_COLORS.Keys.ToList();

        public Theme()
        {
            ID = string.Empty;
        }

        public Brush GetViewBrush()
        {
            string hexColor = DEFAULT_BALCK_COLORS[ViewBrushKey];
            if (Colors.ContainsKey(ViewBrushKey))
            {
                hexColor = Colors[ViewBrushKey];
            }

            return VisualHelper.HexStringToBrush(hexColor);
        }

        public BitmapImage GetViewImage()
        {
            if (Images == null || string.IsNullOrEmpty(Images.Small)) return null;
            string path = Path.GetFullPath(Path.Combine(GetThemePath(), Images.Small));
            if (File.Exists(path))
            {
                return ImageHelper.BitmapImageFromFile(path);
            }

            return null;
        }

        public string GetThemePath()
        {
            if (string.IsNullOrEmpty(ID)) return string.Empty;
            return Path.Combine(ThemeManager.ThemePath, ID);
        }

        public static Theme Parse(object o)
        {
            JObject dict = o as JObject;
            if (!dict.ContainsKey("Colors")) return null;

            Theme theme = new Theme();
            if (dict.ContainsKey("Images") && dict["Images"] is JObject jObject)
            {
                theme.Images = new ThemeImage();
                if (jObject.ContainsKey("Background")) theme.Images.Background = jObject["Background"].ToString();
                if (jObject.ContainsKey("Big")) theme.Images.Big = jObject["Big"].ToString();
                if (jObject.ContainsKey("Small")) theme.Images.Small = jObject["Small"].ToString();
                if (jObject.ContainsKey("Normal")) theme.Images.Normal = jObject["Normal"].ToString();
            }

            if (dict.ContainsKey("FontName")) theme.FontName = dict["FontName"].ToString();
            if (dict.ContainsKey("Font")) theme.FontPath = dict["Font"].ToString();
            if (dict.ContainsKey("BgColorOpacity") && float.TryParse(dict["BgColorOpacity"].ToString(), out float opacity))
            {
                theme.BgColorOpacity = opacity;
            }

            if (dict["Colors"] is JObject d)
            {
                theme.Colors = new Dictionary<string, string>();
                foreach (var key in ColorKeys)
                {
                    if (d.ContainsKey(key))
                    {
                        theme.Colors.Add(key, d[key].ToString());
                    }
                    else
                    {
                        theme.Colors.Add(key, DEFAULT_BALCK_COLORS[key]);
                    }
                }

                theme.ViewBrush = VisualHelper.HexStringToBrush(theme.Colors[ViewBrushKey]);
            }

            return theme;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
