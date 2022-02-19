using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.pojo
{
    public class Servers
    {

        public Server Bus { get; set; }
        public Server BusEurope { get; set; }
        public Server Library { get; set; }
        public Server FC2 { get; set; }
        public Server Jav321 { get; set; }
        public Server DMM { get; set; }
        public Server DB { get; set; }
        public Server MOO { get; set; }


        public Servers()
        {
            Bus = new Server("Bus");
            BusEurope = new Server("BusEurope");
            Library = new Server("Library");
            FC2 = new Server("FC2");
            Jav321 = new Server("Jav321");
            DMM = new Server("DMM");
            MOO = new Server("MOO");
        }

        public void Save()
        {
            ServerConfig.Instance.SaveServer(Bus);
            ServerConfig.Instance.SaveServer(BusEurope);
            ServerConfig.Instance.SaveServer(Library);
            ServerConfig.Instance.SaveServer(FC2);
            ServerConfig.Instance.SaveServer(Jav321);
            ServerConfig.Instance.SaveServer(DMM);
            ServerConfig.Instance.SaveServer(DB);
            ServerConfig.Instance.SaveServer(MOO);
        }

        /// <summary>
        /// 检查是否启用服务器源且地址不为空
        /// </summary>
        /// <returns></returns>
        public bool IsProper()
        {
            return Jav321.IsEnable && !string.IsNullOrEmpty(Jav321.Url)
                                || Bus.IsEnable && !string.IsNullOrEmpty(Bus.Url)
                                || BusEurope.IsEnable && !string.IsNullOrEmpty(BusEurope.Url)
                                || Library.IsEnable && !string.IsNullOrEmpty(Library.Url)
                                || DB.IsEnable && !string.IsNullOrEmpty(DB.Url)
                                || FC2.IsEnable && !string.IsNullOrEmpty(FC2.Url)
                                || DMM.IsEnable && !string.IsNullOrEmpty(DMM.Url)
                                || MOO.IsEnable && !string.IsNullOrEmpty(MOO.Url);
        }

    }

}
