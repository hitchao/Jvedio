using SuperControls.Style;
using SuperControls.Style.Plugin;
using SuperUtils.Common;
using SuperUtils.IO;
using SuperUtils.Reflections;
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

        public static string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "crawlers");


        private static void MoveDir(string baseDir)
        {

        }


        // todo DLL 签名验证
        public static void Init(bool moveFile)
        {
            // 扫描
            List<string> list = DirHelper.TryGetDirList(BaseDir).ToList();
            PluginMetaDatas = new List<PluginMetaData>();
            foreach (string crawler_dir in list) {
                // 移动并删除 temp
                if (moveFile) {
                    string tempPath = Path.Combine(crawler_dir, "temp");
                    if (Directory.Exists(tempPath))
                        DirHelper.TryMoveDir(tempPath, crawler_dir, true);
                }
                string[] arr = FileHelper.TryGetAllFiles(crawler_dir, "*.dll");
                if (arr == null || arr.Length <= 0)
                    continue;
                string dllPath = arr[0];

                // 校验
                PluginMetaData data = GetPluginData(dllPath);
                if (data == null)
                    continue;
                data.SetPluginID(PluginType.Crawler, Path.GetFileName(crawler_dir));
                //data.Enabled = true;
                CrawlerInfo info = new CrawlerInfo();
                info.Path = dllPath;
                PluginMetaDatas.Add(data);

                // 校验并复制
                bool copy = NeedToCopy(dllPath);
                string target = Path.Combine(BaseDir, Path.GetFileName(dllPath));
                if (copy)
                    FileHelper.TryCopyFile(dllPath, target, true);
            }

            ConfigManager.ServerConfig.Read(); // 必须在加载所有爬虫插件后在初始化
            PluginMetaData.BaseDir = BaseDir;
        }

        public static bool NeedToCopy(string dllPath)
        {
            string target = Path.Combine(BaseDir, Path.GetFileName(dllPath));
            if (!File.Exists(target))
                return true;

            // 检查 Md5
            string m1 = SuperUtils.Security.Encrypt.TryGetFileMD5(dllPath);
            string m2 = SuperUtils.Security.Encrypt.TryGetFileMD5(target);
            return !m1.Equals(m2);
        }

        private static PluginMetaData GetPluginData(string dllPath)
        {
            string basePath = Path.GetDirectoryName(dllPath);
            string json_path = Path.Combine(basePath, "main.json");
            PluginMetaData data = null;
            string jsonPath = GetCrawlerJsonPath(json_path);
            if (!File.Exists(jsonPath))
                return null;
            data = PluginMetaData.ParseByPath(jsonPath);
            if (data == null)
                return null;
            // 必须加载
            Assembly dll = ReflectionHelper.TryLoadAssembly(dllPath);
            if (dll == null)
                return null;
            Type classType = getPublicType(dll);
            if (classType == null)
                return null;
            data.Installed = true;

            // 读取配置
            string configPath = Path.Combine(basePath, "config.json");
            if (File.Exists(configPath) && FileHelper.TryReadFile(configPath) is string configString
                && !string.IsNullOrEmpty(configString)) {
                Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(configString);
                if (dict != null) {
                    dict.TryGetValue("enabled", out object enabledString);
                    if (enabledString is bool enabled)
                        data.Enabled = enabled;
                }
            }



            return data;
        }

        private static string GetCrawlerJsonPath(string dllPath)
        {
            string dir = Path.GetDirectoryName(dllPath);
            string name = Path.GetFileNameWithoutExtension(dllPath);
            return Path.Combine(dir, name + ".json");
        }

        //public static bool IsPluginInUsed(string pluginId)
        //{
        //    // 如果在刮削，则不允许删除任何刮削器插件

        //}

        private static Type getPublicType(Assembly dll)
        {
            Type[] types = null;
            try {
                types = dll.GetTypes();
            } catch (Exception ex) {
                MessageCard.Error(ex.Message);
                Console.WriteLine(ex.Message);
                return null;
            }

            if (types == null || types.Length == 0)
                return null;
            foreach (Type type in types) {
                if (type.IsPublic)
                    return type;
            }

            return null;
        }
    }
}
