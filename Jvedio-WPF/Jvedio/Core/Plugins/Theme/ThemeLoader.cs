using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Jvedio.Core.Plugins.Theme
{
    /// <summary>
    /// 加载爬虫插件
    /// </summary>
    public class ThemeLoader
    {
        /**
         * 文件类型：DLL 文件，或者 cs 文件
         * 执行方式：反射加载
         * 
         */

        private static string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "themes");


        public static void loadAllThemes()
        {
            // 扫描
            List<string> list = FileHelper.TryGetAllFiles(BaseDir, "*.json").ToList();
            foreach (string path in list)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    Dictionary<string, object> dict =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.ReadToEnd());



                }

            }

        }




    }
}
