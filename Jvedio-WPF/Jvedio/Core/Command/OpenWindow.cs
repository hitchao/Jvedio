using SuperUtils.WPF.VieModel;
using System.Windows;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio.Core.Command
{
    public class OpenWindow
    {
        public static RelayCommand<Window> Settings { get; set; }

        public static RelayCommand<Window> Thanks { get; set; }

        static OpenWindow()
        {
            Settings = new RelayCommand<Window>(parent => OpenWindowByName("Window_Settings", App.Current.Windows, "Jvedio"));
            Thanks = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Thanks", App.Current.Windows, "Jvedio", parent));

        }
    }
}
