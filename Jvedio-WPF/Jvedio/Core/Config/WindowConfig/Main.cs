using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Main : AbstractConfig
    {
        private Main() : base($"WindowConfig.Main")
        {
            Width = SystemParameters.WorkArea.Width * 0.8;
            Height = SystemParameters.WorkArea.Height * 0.8;
        }

        private static Main _instance = null;

        public static Main createInstance()
        {
            if (_instance == null) _instance = new Main();

            return _instance;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public long WindowState { get; set; }
        public long CurrentDBId { get; set; }
        public long SearchSelectedIndex { get; set; }
        public long ClassifySelectedIndex { get; set; }
        public double _SideGridWidth = 200;
        public double SideGridWidth
        {
            get { return _SideGridWidth; }
            set { _SideGridWidth = value; }
        }

        public bool _FirstRun = true;
        public bool FirstRun
        {
            get { return _FirstRun; }
            set { _FirstRun = value; }
        }

    }
}
