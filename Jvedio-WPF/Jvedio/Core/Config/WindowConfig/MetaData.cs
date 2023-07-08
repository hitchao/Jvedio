using Jvedio.Core.Config.Base;

namespace Jvedio.Core.WindowConfig
{
    public class MetaData : AbstractConfig
    {
        private MetaData() : base($"WindowConfig.MetaData")
        {
        }

        private static MetaData _instance = null;

        public static MetaData CreateInstance()
        {
            if (_instance == null)
                _instance = new MetaData();

            return _instance;
        }

        public long SearchSelectedIndex { get; set; }

        public long ClassifySelectedIndex { get; set; }

        public double SideGridWidth { get; set; }

        public bool SortDescending { get; set; }

        public long SortIndex { get; set; }

        public long PageSize { get; set; }
    }
}
