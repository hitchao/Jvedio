using Jvedio.Core.Config.Base;
using System.ComponentModel;

namespace Jvedio.Core.Config
{
    public class RenameConfig : AbstractConfig
    {
        public static string DEFAULT_NULL_STRING { get; set; }

        static RenameConfig()
        {
            DEFAULT_NULL_STRING = "NULL";
        }

        public const string DEFAULT_OUT_SPLIT = "[null]";
        public const string DEFAULT_IN_SPLIT = DEFAULT_OUT_SPLIT;

        private RenameConfig() : base("RenameConfig")
        {
            FormatString = string.Empty;
        }

        private static RenameConfig _instance = null;

        public static RenameConfig CreateInstance()
        {
            if (_instance == null) _instance = new RenameConfig();

            return _instance;
        }

        public bool RemoveTitleSpace { get; set; }

        public bool AddRenameTag { get; set; }

        [DefaultValue(DEFAULT_OUT_SPLIT)]

        public string OutSplit { get; set; }


        [DefaultValue(DEFAULT_IN_SPLIT)]
        public string InSplit { get; set; }

        public string FormatString { get; set; }
    }
}
