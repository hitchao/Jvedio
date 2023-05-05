using Jvedio.Core.Config.Base;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Main : AbstractConfig
    {
        public const int MAX_IMAGE_WIDTH = 800;
        private Main() : base($"WindowConfig.Main")
        {
            Width = SystemParameters.WorkArea.Width * 0.8;
            Height = SystemParameters.WorkArea.Height * 0.8;
            SideGridWidth = 200;
            FirstRun = true;
            ShowSearchHistory = true;
            SideDefaultExpanded = true;
            SideTagStampExpanded = true;
        }

        private static Main _instance = null;

        public static Main CreateInstance()
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

        public double SideGridWidth { get; set; }

        public bool FirstRun { get; set; }

        public bool ShowSearchHistory { get; set; }
        public bool SideDefaultExpanded { get; set; }
        public bool SideTagStampExpanded { get; set; }
    }
}
