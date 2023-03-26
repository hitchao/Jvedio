using Jvedio.Core.Config;
using Jvedio.Core.Config.Base;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Logs;
using Jvedio.Core.WindowConfig;
using Newtonsoft.Json;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jvedio
{
    public static class ConfigManager
    {
        public const string RELEASE_DATE = "2023-03-26";
        public static StartUp StartUp = StartUp.createInstance();
        public static Core.WindowConfig.Main Main = Core.WindowConfig.Main.createInstance();
        public static Filter Filter = Core.WindowConfig.Filter.createInstance();
        public static Edit Edit = Edit.createInstance();
        public static Detail Detail = Detail.createInstance();
        public static MetaData MetaData = MetaData.createInstance();
        public static Settings Settings = Settings.createInstance();

        public static Jvedio.Core.Config.ServerConfig ServerConfig = Jvedio.Core.Config.ServerConfig.createInstance();
        public static Jvedio.Core.Config.ProxyConfig ProxyConfig = Jvedio.Core.Config.ProxyConfig.createInstance();
        public static Jvedio.Core.Config.ScanConfig ScanConfig = Jvedio.Core.Config.ScanConfig.createInstance();
        public static Jvedio.Core.Config.FFmpegConfig FFmpegConfig = Jvedio.Core.Config.FFmpegConfig.createInstance();
        public static Jvedio.Core.Config.RenameConfig RenameConfig = Jvedio.Core.Config.RenameConfig.createInstance();
        public static Jvedio.Core.Config.PluginConfig PluginConfig = Jvedio.Core.Config.PluginConfig.createInstance();
        public static Jvedio.Core.Config.ThemeConfig ThemeConfig = Jvedio.Core.Config.ThemeConfig.createInstance();
        public static Jvedio.Core.Config.DownloadConfig DownloadConfig = Jvedio.Core.Config.DownloadConfig.createInstance();

        public static void InitConfig(Action callback)
        {
            System.Reflection.FieldInfo[] fieldInfos = typeof(ConfigManager).GetFields();

            foreach (var item in fieldInfos)
            {
                AbstractConfig config = item.GetValue(null) as AbstractConfig;
                if (config == null)
                {
                    Logger.Error(new Exception("无法识别的 AbstractConfig"));
                    continue;
                }

                config.Read();
                if (item.Name.Equals("Settings"))
                    callback?.Invoke();
            }
        }

        public static void Init()
        {
            // 配置 ffmpeg 路径
            if (!File.Exists(ConfigManager.FFmpegConfig.Path) && File.Exists("ffmpeg.exe"))
            {
                ConfigManager.FFmpegConfig.Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            }


        }

        public static void EnsurePicPaths()
        {
            if (string.IsNullOrEmpty(Settings.PicPathJson))
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(PathType.Absolute.ToString(), PathManager.PicPath);
                dict.Add(PathType.RelativeToApp.ToString(), "./Pic");

                Dictionary<string, string> d = new Dictionary<string, string>();
                d.Add("BigImagePath", "./fanart");
                d.Add("SmallImagePath", "./poster");
                d.Add("PreviewImagePath", "./.preview");
                d.Add("ScreenShotPath", "./.screenshot");
                d.Add("ActorImagePath", "./.actor");
                dict.Add(PathType.RelativeToData.ToString(), d);
                Settings.PicPathJson = JsonConvert.SerializeObject(dict);
                Settings.PicPaths = dict;
            }
            else
            {
                Dictionary<string, object> dictionary = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(Settings.PicPathJson);
                if (dictionary == null) return;
                string str = dictionary[PathType.RelativeToData.ToString()].ToString();
                dictionary[PathType.RelativeToData.ToString()] = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(str);
                Settings.PicPaths = dictionary;
            }
        }
    }
}
