
using SuperControls.Style.Plugin;
using SuperUtils.Security;
using System.Collections.Generic;
using System.Reflection;
using static Jvedio.App;

namespace Jvedio.Core.Plugins
{
    public class CrawlerInfo
    {
        #region "属性"


        public string ServerName { get; set; }

        public string InfoType { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Categories { get; set; }

        public string Image { get; set; }

        // 不在 dll 内的字段
        public bool Enabled { get; set; }

        public bool HasNewVersion { get; set; }

        public bool Installed { get; set; }

        public string NewVersion { get; set; }

        public string FileHash { get; set; }

        public string FileName { get; set; }

        public string ImageUrl { get; set; }

        public PluginType Type { get; set; }

        public string _Path;

        public string Path {
            get { return _Path; }

            set {
                _Path = value;
                FileHash = Encrypt.TryGetFileMD5(value);
            }
        }
        #endregion

        public CrawlerInfo()
        {
            Enabled = true;
        }

        public static PluginMetaData ParseDict(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count <= 0)
                return null;
            PluginMetaData result = new PluginMetaData();
            PropertyInfo[] propertyInfos = typeof(PluginMetaData).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos) {
                string name = propertyInfo.Name;
                if (dict.ContainsKey(name))
                    propertyInfo.SetValue(result, dict[name]);
                else
                    Logger.Warn($"dict key[{name}] not exists");
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            CrawlerInfo other = obj as CrawlerInfo;
            if (other == null)
                return false;
            if (other.ServerName == null || other.Name == null)
                return false;
            return other.getUID().Equals(this.getUID());
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
