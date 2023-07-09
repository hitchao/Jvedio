using SuperUtils.NetWork;
using System;
using System.Windows;
using System.Windows.Navigation;
using static Jvedio.App;

namespace Jvedio.Pages
{
    public class ActorNavigator
    {
        public static NavigationService NavigationService { get; set; } =
            (Application.Current.MainWindow as Main).actorFrame.NavigationService;

        public static void Navigate(string path, object param = null)
        {
            try {
                NavigationService?.Navigate(new Uri(path, UriKind.RelativeOrAbsolute), param);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public static void GoBack()
        {
            NavigationService?.GoBack();
        }

        public static void GoForward()
        {
            NavigationService?.GoForward();
        }
    }
}
