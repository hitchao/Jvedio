using ExCSS;
using Jvedio.Core.pojo;
using Jvedio.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Jvedio.Core
{
    /// <summary>
    /// 主题加载
    /// </summary>
    public static class ThemeLoader
    {
        /**
         * 【初始化的过程】：
         * 1. 扫描 Themes 下的所有目录，每个目录即为一个主题
         * 2. 验证每个主题的合法性
         * 3. 加载：键值对 => key=主题名，value=目录名
         *
         */

        /**
         * 【主题的目录结构】：
         * Themes
         *      - 主题1
         *          ReadMe.md
         *          index.json ：用于描述主题的元信息，包括 主题名称、来源、发布日期、作者、文件夹大小、版本
         *          theme.css：主题的核心文件
         *          fonts
         *              - fonts1.otf：字体文件
         *      - 主题2
         *      - ...
         */


        public static List<Theme> Themes { get; set; }

        private static readonly string ThemePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins\\Themes");



        /// <summary>
        /// 获得所有主题并解析
        /// </summary>
        /// <returns></returns>
        public static void loadAllThemes()
        {
            Themes = new List<Theme>();
            List<string> result = new List<string>();
            string[] paths = Directory.GetDirectories(ThemePath);
            foreach (var path in paths)
            {
                Theme theme = getThemeFromPath(path);
                Themes.Add(theme);
            }
        }

        public static Theme loadTheme(string name)
        {
            string path = Path.Combine(ThemePath, name);
            if (Directory.Exists(path))
            {
                return getThemeFromPath(path);
            }
            return new Theme();
        }

        private static Theme getThemeFromPath(string path)
        {
            Theme theme = new Theme();
            string cssPath = Path.Combine(path, "theme.css");
            DisplayProperty displayProperty = parseCSS(cssPath);
            theme.DisplayProperty = displayProperty;
            theme.Name = path.Split('\\').Last();
            string fontPath = Path.Combine(path, "fonts");
            if (Directory.Exists(fontPath))
            {
                string[] fontfiles = FileHelper.TryGetAllFiles(fontPath, "*.*");
                if (fontfiles.Length > 0)
                {
                    foreach (var fontfile in fontfiles)
                    {
                        if (fontfile.IndexOfAnyString(GlobalVariable.FontExt) > 0)
                        {
                            theme.Font = Path.Combine(fontPath, fontfile);
                        }
                    }

                }

            }

            parseJson(ref theme, Path.Combine(path, "index.json"));
            return theme;
        }

        private static void parseJson(ref Theme theme, string jsonPath)
        {
            try
            {
                string jsonContent = FileHelper.TryLoadFile(jsonPath);
                Theme t = JsonConvert.DeserializeObject<Theme>(jsonContent);
                theme.Author = t.Author;
                theme.Url = t.Url;
                theme.Date = t.Date;
                theme.Version = t.Version;
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

        }

        private static DisplayProperty parseCSS(string cssPath)
        {
            string cssContent = FileHelper.TryLoadFile(cssPath);
            var parser = new StylesheetParser();
            var stylesheet = parser.Parse(cssContent);

            DisplayProperty displayProperty = new DisplayProperty();
            displayProperty.Title = new DisplayStyle();
            displayProperty.Side = new DisplayStyle();
            displayProperty.Display = new DisplayStyle();
            displayProperty.Tools = new DisplayStyle();
            displayProperty.Search = new DisplayStyle();
            displayProperty.Menu = new DisplayStyle();
            displayProperty.Global = new DisplayStyle();
            Type t1 = displayProperty.GetType();

            //在样式表中自上而下寻找
            foreach (StyleRule rule in stylesheet.StyleRules)
            {
                string selectors = rule.SelectorText;
                string color = rule.Style.Color;
                string bg = rule.Style.Background;
                string fontsize = rule.Style.FontSize;
                int.TryParse(Regex.Match(fontsize, @"\d+").Value, out int fs);

                //获得选择器
                string[] selectTexts = selectors.Split('.');
                string s1 = selectTexts[1].Trim();
                string s2 = selectTexts[2].Trim();

                //反射
                DisplayStyle style = (DisplayStyle)t1.GetProperty(s1).GetValue(displayProperty);
                if (s2 == "Main")
                {
                    style.MainFontSize = fs;
                    style.MainBackground = getColorFromRGBSring(bg);
                    style.MainForeground = getColorFromRGBSring(color);
                }
                else
                {
                    style.SubFontSize = fs;
                    style.SubBackground = getColorFromRGBSring(bg);
                    style.SubForeground = getColorFromRGBSring(color);
                }
                t1.GetProperty(s1).SetValue(displayProperty, style);
            }

            return displayProperty;
        }

        private static System.Windows.Media.Color getColorFromRGBSring(string rgb)
        {
            if (rgb.IndexOf("rgb(") < 0 || rgb.Split(',').Count() != 3) return System.Windows.Media.Color.FromRgb(0, 0, 0);

            rgb = rgb.Replace("rgb(", "").Replace(")", "").Replace(" ", "");
            string[] colors = rgb.Split(',');
            byte.TryParse(colors[0], out byte r);
            byte.TryParse(colors[1], out byte g);
            byte.TryParse(colors[2], out byte b);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

    }




}