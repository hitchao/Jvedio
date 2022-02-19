using Jvedio.Utils;
using System.Windows;

namespace Jvedio.Core.pojo
{
    public class WindowProperty
    {
        public WindowProperty()
        {
            Location = new Point(0, 0);
            Size = new Size(0, 0);
            WinState = JvedioWindowState.Normal;
        }
        public Point Location { get; set; }
        public Size Size { get; set; }

        public JvedioWindowState WinState { get; set; }
    }

}
