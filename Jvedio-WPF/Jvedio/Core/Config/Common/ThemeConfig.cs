using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class ThemeConfig : AbstractConfig
    {

        private ThemeConfig() : base("ThemeConfig")
        {
            ThemeIndex = 0;
            ThemeID = "";
        }

        private static ThemeConfig _instance = null;

        public static ThemeConfig createInstance()
        {
            if (_instance == null) _instance = new ThemeConfig();

            return _instance;
        }

        public string ThemeID { get; set; }

        public int ThemeIndex { get; set; }
    }
}
