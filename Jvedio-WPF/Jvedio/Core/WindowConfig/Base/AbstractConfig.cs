using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.WindowConfig
{
    public abstract class AbstractConfig : IConfig
    {
        public void Read()
        {
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            string configName = $"WindowConfig.{this.GetType().Name}";
            wrapper.Eq("ConfigName", configName);
            AppConfig appConfig = GlobalMapper.appConfigMapper.selectOne(wrapper);
            if (appConfig == null || appConfig.ConfigId == 0) return;
            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(appConfig.ConfigValue);
            Type type = this.GetType();
            foreach (string key in dict.Keys)
            {
                object value = dict[key];
                var prop = type.GetProperty(key);
                if (prop == null || value == null) continue;
                prop.SetValue(this, value, null);
            }
        }

        public void Save()
        {
            System.Reflection.PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            Dictionary<string, object> dictionary = propertyInfos.ToList().ToDictionary(x => x.Name, x => x.GetValue(this));
            AppConfig appConfig = new AppConfig();
            string configName = $"WindowConfig.{this.GetType().Name}";
            appConfig.ConfigName = configName;
            appConfig.ConfigValue = JsonConvert.SerializeObject(dictionary); ;
            Console.WriteLine();
            GlobalMapper.appConfigMapper.insert(appConfig, Enums.InsertMode.Replace);
        }
    }
}
