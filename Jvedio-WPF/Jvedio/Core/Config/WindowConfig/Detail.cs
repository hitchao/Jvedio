using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Detail : AbstractConfig
    {
        private Detail() : base($"WindowConfig.Detail")
        {
        }

        private static Detail _instance = null;

        public static Detail createInstance()
        {
            if (_instance == null) _instance = new Detail();

            return _instance;
        }
        public bool ShowScreenShot { get; set; }
        public long InfoSelectedIndex { get; set; }


    }
}
