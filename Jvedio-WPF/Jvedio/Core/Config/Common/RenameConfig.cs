using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config
{
    public class RenameConfig : AbstractConfig
    {

        public const string DEFAULT_OUT_SPLIT = "[null]";
        public const string DEFAULT_IN_SPLIT = DEFAULT_OUT_SPLIT;


        public static string DEFAULT_NULL_STRING { get; set; }

        static RenameConfig()
        {
            DEFAULT_NULL_STRING = "NULL";
        }

        private RenameConfig() : base("RenameConfig")
        {
            OutSplit = DEFAULT_OUT_SPLIT;
            InSplit = DEFAULT_IN_SPLIT;
            FormatString = string.Empty;
        }

        private static RenameConfig _instance = null;

        public static RenameConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new RenameConfig();

            return _instance;
        }

        public bool RemoveTitleSpace { get; set; }

        public bool AddRenameTag { get; set; }

        public string OutSplit { get; set; }

        public string InSplit { get; set; }

        public string FormatString { get; set; }
    }
}
