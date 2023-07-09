using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static Jvedio.App;

namespace Jvedio.Core.Config.Base
{
    /// <summary>
    ///
    /// </summary>
    public abstract class AbstractConfig : IConfig, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


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
            AppConfig appConfig = MapperManager.appConfigMapper.SelectOne(wrapper);
            Logger.Info($"read config: {ConfigName}");
            if (appConfig == null || appConfig.ConfigId == 0)
                return;
            Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(appConfig.ConfigValue);
            if (dict == null)
                return;
            Type type = this.GetType();
            foreach (string key in dict.Keys) {
                if (string.IsNullOrEmpty(key))
                    continue;
                object value = dict[key];
                var prop = type.GetProperty(key);
                if (prop == null || value == null)
                    continue;
                try {
                    prop.SetValue(this, value, null);
                } catch {
                    continue;
                }
            }
        }

        public virtual void Save()
        {
            System.Reflection.PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            if (propertyInfos == null)
                return;
            Dictionary<string, object> dictionary = propertyInfos.ToList().ToDictionary(x => x.Name, x => x.GetValue(this));
            AppConfig appConfig = new AppConfig();
            appConfig.ConfigName = ConfigName;
            appConfig.ConfigValue = JsonConvert.SerializeObject(dictionary);        // 不为 null
            MapperManager.appConfigMapper.Insert(appConfig, InsertMode.Replace);
            Logger.Info($"save config: {ConfigName}");
        }
    }
}
