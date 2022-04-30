using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.Command
{
    public class OpenWindow
    {
        public static RelayCommand Settings { get; set; }
        public static RelayCommand<Window> About { get; set; }
        public static RelayCommand<Window> Upgrade { get; set; }

        static OpenWindow()
        {
            Settings = new RelayCommand(OpenSettings);
            About = new RelayCommand<Window>(win => OpenAbout(win));
            Upgrade = new RelayCommand<Window>(win => OpenUpgrade(win));
        }

        static void OpenSettings()
        {
            new Jvedio.Settings().Show();
        }
        static void OpenAbout(Window parent)
        {
            new Dialog_About(parent, false).ShowDialog();
        }
        static void OpenUpgrade(Window parent)
        {
            new Dialog_Upgrade(parent).ShowDialog();
        }
    }
}
