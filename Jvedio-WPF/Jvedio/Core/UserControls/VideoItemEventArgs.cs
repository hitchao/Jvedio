using System.Windows;

namespace Jvedio.Core.UserControls
{
    public class VideoItemEventArgs : RoutedEventArgs
    {

        public delegate void VideoItemEventHandler(object sender, VideoItemEventArgs e);

        public long DataID { get; set; }

        public VideoItemEventArgs(long dataID, RoutedEvent e, object source) : base(e, source)
        {
            DataID = dataID;
        }
    }
}
