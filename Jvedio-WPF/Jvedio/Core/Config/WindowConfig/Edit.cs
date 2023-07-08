using Jvedio.Core.Config.Base;

namespace Jvedio.Core.WindowConfig
{
    public class Edit : AbstractConfig
    {
        private Edit() : base($"WindowConfig.Edit")
        {
        }

        private static Edit _instance = null;

        public static Edit CreateInstance()
        {
            if (_instance == null)
                _instance = new Edit();

            return _instance;
        }

        public bool MoreExpanded { get; set; }
    }
}
