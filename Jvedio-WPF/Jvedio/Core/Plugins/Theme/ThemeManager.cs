using ExCSS;
using Jvedio.Core.Plugins;
using Jvedio.Entity;
using Jvedio.Logs;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Utils.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Jvedio.Core
{
    public static class ThemeManager
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


        public static List<PluginMetaData> PluginMetaDatas { get; set; }
        public static List<Theme> Themes { get; set; }

        public static readonly string ThemePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins\\Themes");


        /// <summary>
        /// 获得所有主题并解析
        /// </summary>
        /// <returns></returns>
        public static void LoadAllThemes()
        {
            // 获得当前的支持的颜色
            Themes = new List<Theme>();
            // 添加默认皮肤
            Theme black = new Theme();
            black.Colors = Theme.DEFAULT_BALCK_COLORS;
            black.ViewBrush = black.GetViewBrush();
            Theme white = new Theme();
            white.Colors = Theme.DEFAULT_WHITE_COLORS;
            white.ViewBrush = white.GetViewBrush();
            Themes.Add(black);
            Themes.Add(white);

            PluginMetaDatas = new List<PluginMetaData>();
            string[] paths = Directory.GetDirectories(ThemePath);
            foreach (var path in paths)
            {
                string jsonPath = Path.Combine(path, "main.json");
                if (!File.Exists(jsonPath))
                {
                    Logger.Warning($"解析皮肤资源失败 => 不存在 {jsonPath}");
                    continue;
                }
                (Theme theme, PluginMetaData data) = ParseJson(jsonPath);
                if (theme == null || data == null)
                {
                    Logger.Warning($"解析皮肤资源失败 => {jsonPath}");
                    continue;
                }
                string ID = Path.GetFileName(path);
                theme.ID = ID;
                theme.ViewImage = theme.GetViewImage();
                Themes.Add(theme);
                data.Installed = true;
                data.SetPluginID(PluginType.Theme, Path.GetFileName(path));
                PluginMetaDatas.Add(data);
            }
        }

        private static (Theme, PluginMetaData) ParseJson(string jsonPath)
        {
            string content = FileHelper.TryReadFile(jsonPath);
            if (string.IsNullOrEmpty(content)) return (null, null);
            Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(content);
            if (!dict.ContainsKey("Data") || !dict.ContainsKey("PluginMetaData")) return (null, null);
            Theme theme = null;
            PluginMetaData data = null;
            try
            {
                theme = Theme.Parse(dict["Data"]);
                data = PluginMetaData.ParseByPath(jsonPath);
                if (data != null)
                    theme.Desc = data.ReleaseNotes.Desc;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
            }
            return (theme, data);
        }


    }




}