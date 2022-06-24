using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Jvedio.Utils.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config.Base
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractConfig : IConfig
    {

        protected string ConfigName { get; set; }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AbstractConfig(string configName)
        {
            // configName 不为 null
            if (string.IsNullOrEmpty(configName))
                throw new ArgumentNullException("AbstractConfig.ConfigName");
            ConfigName = configName;
        }

        public virtual void Read()
        {
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            wrapper.Eq("ConfigName", ConfigName);
            AppConfig appConfig = GlobalMapper.appConfigMapper.selectOne(wrapper);
            if (appConfig == null || appConfig.ConfigId == 0) return;
            Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(appConfig.ConfigValue);
            if (dict == null) return;
            Type type = this.GetType();
            foreach (string key in dict.Keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                object value = dict[key];
                var prop = type.GetProperty(key);
                if (prop == null || value == null) continue;
                try
                {
                    prop.SetValue(this, value, null);
                }
                catch { continue; }

            }
        }

        public virtual void Save()
        {
            System.Reflection.PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            if (propertyInfos == null) return;
            Dictionary<string, object> dictionary = propertyInfos.ToList().ToDictionary(x => x.Name, x => x.GetValue(this));
            AppConfig appConfig = new AppConfig();
            appConfig.ConfigName = ConfigName;
            appConfig.ConfigValue = JsonConvert.SerializeObject(dictionary);        // 不为 null
            GlobalMapper.appConfigMapper.insert(appConfig, Enums.InsertMode.Replace);
        }
    }
}
