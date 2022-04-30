using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Jvedio.Core.Plugins.Crawler
{
    /// <summary>
    /// 加载爬虫插件
    /// </summary>
    public class CrawlerLoader
    {
        /**
         * 文件类型：DLL 文件，或者 cs 文件
         * 执行方式：反射加载
         * 
         */

        private static string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "crawlers");

        private static List<string> BaseDll = new List<string>() { "HtmlAgilityPack.dll", "CommonNet.dll" };


        public static void loadAllCrawlers()
        {
            // 扫描
            List<string> list = FileHelper.TryGetAllFiles(BaseDir, "*.dll").ToList();
            list.RemoveAll(arg => BaseDll.Contains(Path.GetFileName(arg)));
            foreach (string dllPath in list)
            {

                Assembly dll = Assembly.LoadFrom(dllPath);
                Type classType = getPublicType(dll.GetTypes());
                if (classType == null) continue;
                Dictionary<string, string> info = getInfo(classType);
                if (info == null || !info.ContainsKey("ServerName") || !info.ContainsKey("Name")) continue;
                if (string.IsNullOrEmpty(info["ServerName"]) || string.IsNullOrEmpty(info["Name"])) continue;
                PluginInfo pluginInfo = PluginInfo.ParseDict(info);
                if (pluginInfo != null)
                {
                    pluginInfo.Path = dllPath;
                    Global.Plugins.Crawlers.Add(pluginInfo);
                }
            }
            setPluginEnabled();
            GlobalConfig.ServerConfig.Read();// 必须在加载所有爬虫插件后在初始化
        }


        private static void setPluginEnabled()
        {
            if (Global.Plugins.Crawlers != null && Global.Plugins.Crawlers.Count > 0
                && !string.IsNullOrEmpty(GlobalConfig.Settings.PluginEnabledJson))
            {
                Dictionary<string, bool> dict = null; ;
                string json = GlobalConfig.Settings.PluginEnabledJson;
                try
                {
                    dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
                }
                catch (Exception ex) { Console.WriteLine(ex); }
                if (dict == null || dict.Count <= 0) return;

                foreach (PluginInfo plugin in Global.Plugins.Crawlers)
                {
                    string uid = plugin.getUID();
                    if (dict.ContainsKey(uid))
                        plugin.Enabled = dict[uid];
                }
            }
        }



        private static Type getPublicType(Type[] types)
        {
            if (types == null || types.Length == 0) return null;
            foreach (Type type in types)
            {
                if (type.IsPublic) return type;
            }
            return null;
        }

        public static Dictionary<string, string> getInfo(Type type)
        {
            FieldInfo fieldInfo = type.GetField("Infos");
            if (fieldInfo != null)
            {
                object value = fieldInfo.GetValue(null);
                if (value != null)
                {
                    try
                    {
                        return (Dictionary<string, string>)value;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                }
            }
            return null;
        }




    }
}
