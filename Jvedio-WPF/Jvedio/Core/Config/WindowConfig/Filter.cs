using Jvedio.Core.Config.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Filter : AbstractConfig
    {
        private Filter() : base($"WindowConfig.Filter")
        {
            Width = 400;
            Height = 600;
        }

        private static Filter _instance = null;

        public static Filter createInstance()
        {
            if (_instance == null) _instance = new Filter();

            return _instance;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }


    }
}
