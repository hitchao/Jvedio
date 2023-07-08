using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class ThemeConfig : AbstractConfig
    {
        private ThemeConfig() : base("ThemeConfig")
        {
            ThemeIndex = 0;
            ThemeID = string.Empty;
        }

        private static ThemeConfig _instance = null;

        public static ThemeConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new ThemeConfig();

            return _instance;
        }

        public string ThemeID { get; set; }

        public long ThemeIndex { get; set; }
    }
}
