﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Plugins
{
    public class PluginInfo
    {


        public string ServerName { get; set; }
        public string InfoType { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Categories { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string MarkDown { get; set; }
        public string License { get; set; }
        public string PublishDate { get; set; }
        public string Path { get; set; }

        // 不在 dll 内的字段
        public bool Enabled { get; set; }

        public PluginInfo()
        {
            Enabled = true;
        }


        public static PluginInfo ParseDict(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count <= 0) return null;
            PluginInfo result = new PluginInfo();
            PropertyInfo[] propertyInfos = typeof(PluginInfo).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string name = propertyInfo.Name;
                if (dict.ContainsKey(name))
                    propertyInfo.SetValue(result, dict[name]);
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PluginInfo other = obj as PluginInfo;
            if (other == null) return false;
            return other.ServerName.Equals(ServerName) && other.Name.Equals(Name);
        }

        public string getUID()
        {
            return $"{ServerName}.{Name}";
        }

        public override int GetHashCode()
        {
            return ServerName.GetHashCode() ^ Name.GetHashCode();
        }

    }
}
