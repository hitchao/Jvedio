using Jvedio.Core.Config.Base;

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
            if (instance == null)
                instance = new JavaServerConfig();
            return instance;
        }


        private long _Port { get; set; }
        public long Port {
            get { return _Port; }
            set {
                _Port = value;
                RaisePropertyChanged();
            }
        }


    }
}
