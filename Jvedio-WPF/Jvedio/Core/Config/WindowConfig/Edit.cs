using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Edit : AbstractConfig
    {
        private Edit() : base($"WindowConfig.Edit")
        {

        }

        private static Edit _instance = null;

        public static Edit createInstance()
        {
            if (_instance == null) _instance = new Edit();

            return _instance;
        }
        public bool MoreExpanded { get; set; }
    }
}
