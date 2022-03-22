using Jvedio.Utils;
using System.Windows;

namespace Jvedio.Entity
{
    public class WindowProperty
    {
        public WindowProperty()
        {
            Location = new Point(0, 0);
            Size = new Size(0, 0);
            WinState = Jvedio.Core.Enums.WindowState.Normal;
        }
        public Point Location { get; set; }
        public Size Size { get; set; }

        public Jvedio.Core.Enums.WindowState WinState { get; set; }
    }

}
