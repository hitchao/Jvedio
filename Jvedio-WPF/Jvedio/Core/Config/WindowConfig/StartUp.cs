using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.WindowConfig
{
    public class StartUp : AbstractConfig
    {
        private StartUp() : base($"WindowConfig.StartUp")
        {
            Tile = false;
            ShowHideItem = false;
            SideIdx = 0;
            SortType = "";
            Sort = true;
        }

        private static StartUp _instance = null;

        public static StartUp createInstance()
        {
            if (_instance == null) _instance = new StartUp();

            return _instance;
        }
        public bool Tile { get; set; }
        public bool ShowHideItem { get; set; }
        public long SideIdx { get; set; }
        public long CurrentDBID { get; set; }
        public string SortType { get; set; }
        public bool Sort { get; set; }
    }
}
