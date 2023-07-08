using Jvedio.Core.Config.Base;

namespace Jvedio.Core.WindowConfig
{
    public class Detail : AbstractConfig
    {
        private Detail() : base($"WindowConfig.Detail")
        {
        }

        private static Detail _instance = null;

        public static Detail CreateInstance()
        {
            if (_instance == null)
                _instance = new Detail();

            return _instance;
        }

        public bool ShowScreenShot { get; set; }

        public long InfoSelectedIndex { get; set; }
    }
}
