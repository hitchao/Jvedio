using GalaSoft.MvvmLight.Command;
using System.Windows;
using static Jvedio.VisualTools.WindowHelper;

namespace Jvedio.Core.Command
{
    public class OpenWindow
    {
        public static RelayCommand Settings { get; set; }

        public static RelayCommand<Window> About { get; set; }

        public static RelayCommand<Window> Upgrade { get; set; }

        public static RelayCommand<Window> Thanks { get; set; }

        static OpenWindow()
        {
            Settings = new RelayCommand(() => OpenWindowByName("Window_Settings"));
            About = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_About", parent));
            Upgrade = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Upgrade", parent));
            Thanks = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Thanks", parent));
        }
    }
}
