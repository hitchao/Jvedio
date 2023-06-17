using Jvedio.Core.Config.Base;
using Jvedio.Core.Crawler;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Core.Config
{
    public class JavaServerConfig : AbstractConfig
    {
        private const long DEFAULT_PORT = 9527;
        private JavaServerConfig() : base("JavaServer")
        {
            Port = DEFAULT_PORT;
        }



        private static JavaServerConfig instance = null;

        public static JavaServerConfig CreateInstance()
        {
            if (instance == null) instance = new JavaServerConfig();
            return instance;
        }


        private long _Port { get; set; }
        public long Port
        {
            get { return _Port; }
            set
            {
                _Port = value;
                RaisePropertyChanged();
            }
        }


    }
}
