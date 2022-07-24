using Jvedio.Logs;
using Jvedio.Utils.Common;
using Jvedio.Utils.IO;
using Jvedio.Utils.Reflections;
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
    public class CrawlerManager
    {
        /**
         * 文件类型：DLL 文件，或者 cs 文件
         * 执行方式：反射加载
         * 
         */


        public static List<PluginMetaData> PluginMetaDatas { get; set; }

        private static string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "crawlers");


        // todo DLL 签名验证
        public static void LoadAllCrawlers()
        {
            // 扫描
            List<string> list = DirHelper.TryGetDirList(BaseDir).ToList();
            PluginMetaDatas = new List<PluginMetaData>();
            foreach (string crawler_dir in list)
            {
                string[] arr = FileHelper.TryGetAllFiles(crawler_dir, "*.dll");
                if (arr.Length <= 0) continue;
                string dllPath = arr[0];

                // 校验
                PluginMetaData data = GetPluginData(dllPath);
                if (data == null) continue;
                data.PluginID = PluginType.Cralwer.ToString() + Path.GetFileName(crawler_dir);
                CrawlerInfo info = new CrawlerInfo();
                info.Path = dllPath;
                PluginMetaDatas.Add(data);
                // 校验并复制
                bool copy = NeedToCopy(dllPath);
                string target = Path.Combine(BaseDir, Path.GetFileName(dllPath));
                if (copy) FileHelper.TryCopyFile(dllPath, target, true);
            }
            //SetPluginEnabled();
            GlobalConfig.ServerConfig.Read();// 必须在加载所有爬虫插件后在初始化
        }


        public static bool NeedToCopy(string dllPath)
        {
            string target = Path.Combine(BaseDir, Path.GetFileName(dllPath));
            if (!File.Exists(target)) return true;
            // 检查 Md5
            string m1 = JvedioLib.Security.Encrypt.GetFileMD5(dllPath);
            string m2 = JvedioLib.Security.Encrypt.GetFileMD5(target);
            return !m1.Equals(m2);
        }


        private static PluginMetaData GetPluginData(string dllPath)
        {
            string json_path = Path.Combine(Path.GetDirectoryName(dllPath), "main.json");
            PluginMetaData data = null;
            string jsonPath = GetCrawlerJsonPath(json_path);
            if (!File.Exists(jsonPath)) return null;
            data = PluginMetaData.Parse(jsonPath);
            if (data == null) return null;
            Assembly dll = ReflectionHelper.TryLoadAssembly(dllPath);
            if (dll == null) return null;
            Type classType = getPublicType(dll.GetTypes());
            if (classType == null) return null;
            data.Installed = true;


            return data;
        }


        private static string GetCrawlerJsonPath(string dllPath)
        {
            string dir = Path.GetDirectoryName(dllPath);
            string name = Path.GetFileNameWithoutExtension(dllPath);
            return Path.Combine(dir, name + ".json");
        }


        private static void SetPluginEnabled()
        {
            //if (Global.Plugins.Crawlers != null && Global.Plugins.Crawlers.Count > 0
            //    && !string.IsNullOrEmpty(GlobalConfig.Settings.PluginEnabledJson))
            //{

            //    string json = GlobalConfig.Settings.PluginEnabledJson;
            //    if (string.IsNullOrEmpty(json)) return;
            //    Dictionary<string, bool> dict = JsonUtils.TryDeserializeObject<Dictionary<string, bool>>(json);
            //    if (dict == null || dict.Count <= 0) return;
            //    foreach (PluginMetaData plugin in Global.Plugins.Crawlers)
            //    {
            //        string uid = plugin.getUID();
            //        if (string.IsNullOrEmpty(uid)) continue;
            //        if (dict.ContainsKey(uid))
            //            plugin.Enabled = dict[uid];
            //    }
            //}
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

        public static Dictionary<string, string> GetInfo(Type type)
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
                        Logger.Error(ex);
                        return null;
                    }
                }
                else
                {
                    Logger.Warning("Infos 字段没有值");
                }
            }
            else
            {
                Logger.Warning("DLL 无 Infos 字段");
            }
            return null;
        }




    }
}
