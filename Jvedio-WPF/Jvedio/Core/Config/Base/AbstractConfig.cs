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

        protected string ConfigName { get; set; }

        public AbstractConfig(string configName)
        {
            ConfigName = configName;
        }

        public virtual void Read()
        {
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            wrapper.Eq("ConfigName", ConfigName);
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

        public virtual void Save()
        {
            System.Reflection.PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            Dictionary<string, object> dictionary = propertyInfos.ToList().ToDictionary(x => x.Name, x => x.GetValue(this));
            AppConfig appConfig = new AppConfig();
            appConfig.ConfigName = ConfigName;
            appConfig.ConfigValue = JsonConvert.SerializeObject(dictionary); ;
            Console.WriteLine();
            GlobalMapper.appConfigMapper.insert(appConfig, Enums.InsertMode.Replace);
        }
    }
}
