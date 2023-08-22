using System.Windows;

namespace Jvedio.Core.UserControls
{
    public class ObjectEventArgs : RoutedEventArgs
    {

        public delegate void ObjectEventArgsHandler(object sender, ObjectEventArgs e);

        public object Data { get; set; }

        public ObjectEventArgs(object data, RoutedEvent e, object source) : base(e, source)
        {
            Data = data;
        }
    }
}
