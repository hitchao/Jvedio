using Jvedio.Core.Crawler;
using Jvedio.Core.SimpleORM;
using Jvedio.Core.WindowConfig;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class ServerConfig : AbstractConfig
    {
        private ServerConfig() : base("Servers")
        {
        }

        private static ServerConfig instance = null;

        public static ServerConfig createInstance()
        {
            if (instance == null) instance = new ServerConfig();
            return instance;
        }

        public List<CrawlerServer> CrawlerServers { get; set; }



        public override void Read()
        {
            CrawlerServers = new List<CrawlerServer>();
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            wrapper.Eq("ConfigName", ConfigName);
            AppConfig appConfig = GlobalMapper.appConfigMapper.selectOne(wrapper);
            if (appConfig == null || appConfig.ConfigId == 0) return;
            List<Dictionary<object, object>> dicts = JsonConvert.DeserializeObject<List<Dictionary<object, object>>>(appConfig.ConfigValue);

            if (dicts == null) return;
            foreach (Dictionary<object, object> d in dicts)
            {
                CrawlerServer server = new CrawlerServer();
                server.ServerName = d["ServerName"].ToString();
                server.Name = d["Name"].ToString();
                if (!Global.Plugins.Crawlers.Where(arg => arg.ServerName.ToString().ToLower().Equals(server.ServerName.ToLower())).Any())
                    continue;
                server.Url = d["Url"].ToString();
                server.Cookies = d["Cookies"].ToString();
                server.Enabled = d["Enabled"].ToString().ToLower() == "true" ? true : false;
                server.LastRefreshDate = d["LastRefreshDate"].ToString();
                server.Headers = d["Headers"].ToString();
                int.TryParse(d["Available"].ToString(), out int available);
                server.Available = available;
                CrawlerServers.Add(server);


            }
            Console.WriteLine();
        }
        public override void Save()
        {
            if (CrawlerServers != null)
            {
                AppConfig appConfig = new AppConfig();
                appConfig.ConfigName = ConfigName;
                appConfig.ConfigValue = JsonConvert.SerializeObject(CrawlerServers); ;
                Console.WriteLine();
                GlobalMapper.appConfigMapper.insert(appConfig, Enums.InsertMode.Replace);
            }
        }



    }
}
