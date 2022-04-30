using Jvedio.Core.WindowConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class RenameConfig : AbstractConfig
    {


        private RenameConfig() : base("RenameConfig")
        {
        }

        private static RenameConfig _instance = null;

        public static RenameConfig createInstance()
        {
            if (_instance == null) _instance = new RenameConfig();

            return _instance;
        }




        public bool RemoveTitleSpace { get; set; }
        public bool AddRenameTag { get; set; }
        public string _OutSplit = "[null]";
        public string OutSplit
        {
            get { return _OutSplit; }
            set { _OutSplit = value; }
        }
        public string _InSplit = "[null]";
        public string InSplit
        {
            set { _InSplit = value; }
            get { return _InSplit; }
        }
        public string _FormatString = "";
        public string FormatString
        {
            get { return _FormatString; }
            set { _FormatString = value; }
        }

    }
}
