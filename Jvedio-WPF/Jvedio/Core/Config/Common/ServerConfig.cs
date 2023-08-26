using Jvedio.Core.Config.Base;
using Jvedio.Core.Crawler;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Core.Config
{
    public class ServerConfig : AbstractConfig
    {
        private ServerConfig() : base("Servers")
        {
            //DownloadInfo = true;
            //DownloadThumbNail = true;
            //DownloadPoster = true;
            //DownloadPreviewImage = false;
            //DownloadActor = true;
        }

        //public bool DownloadInfo { get; set; }
        //public bool DownloadThumbNail { get; set; }
        //public bool DownloadPoster { get; set; }
        //public bool DownloadPreviewImage { get; set; }
        //public bool DownloadActor { get; set; }

        private static ServerConfig instance = null;

        public static ServerConfig CreateInstance()
        {
            if (instance == null)
                instance = new ServerConfig();
            return instance;
        }

        public List<CrawlerServer> CrawlerServers { get; set; }

        public override void Read()
        {
            CrawlerServers = new List<CrawlerServer>();
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            wrapper.Eq("ConfigName", ConfigName);
            AppConfig appConfig = MapperManager.appConfigMapper.SelectOne(wrapper);
            if (appConfig == null || appConfig.ConfigId == 0)
                return;
            List<Dictionary<object, object>> dicts = JsonUtils.TryDeserializeObject<List<Dictionary<object, object>>>(appConfig.ConfigValue);

            if (dicts == null || CrawlerManager.PluginMetaDatas == null)
                return;
            foreach (Dictionary<object, object> d in dicts) {
                CrawlerServer server = new CrawlerServer();
                //if (!server.HasAllKeys(d))
                //    continue;
                if (d.ContainsKey("PluginID"))
                    server.PluginID = d["PluginID"].ToString();
                if (string.IsNullOrEmpty(server.PluginID))
                    continue;
                if (!CrawlerManager.PluginMetaDatas.Where(arg => arg.PluginID.Equals(server.PluginID)).Any())
                    continue;
                if (d.ContainsKey("Url") && d["Url"] is string url)
                    server.Url = url;
                if (d.ContainsKey("Cookies") && d["Cookies"] is string Cookies)
                    server.Cookies = Cookies;
                if (d.ContainsKey("Enabled") && d["Enabled"] is bool enabled)
                    server.Enabled = enabled;
                if (d.ContainsKey("LastRefreshDate") && d["LastRefreshDate"] is string LastRefreshDate)
                    server.LastRefreshDate = LastRefreshDate;
                if (d.ContainsKey("Headers") && d["Headers"] is string Headers)
                    server.Headers = Headers;
                if (d.ContainsKey("Available") && int.TryParse(d["Available"].ToString(), out int available))
                    server.Available = available;
                CrawlerServers.Add(server);
            }
        }

        public override void Save()
        {
            if (CrawlerServers != null) {
                AppConfig appConfig = new AppConfig();
                appConfig.ConfigName = ConfigName;
                appConfig.ConfigValue = JsonConvert.SerializeObject(CrawlerServers);
                MapperManager.appConfigMapper.Insert(appConfig, InsertMode.Replace);
            }
        }
    }
}
