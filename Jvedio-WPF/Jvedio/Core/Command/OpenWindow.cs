using GalaSoft.MvvmLight.Command;
using Jvedio.Core.Crawler;
using Jvedio.Core.Global;
using SuperControls.Style.Upgrade;
using System.Windows;
using static Jvedio.VisualTools.WindowHelper;

namespace Jvedio.Core.Command
{
    public class OpenWindow
    {
        public static RelayCommand Settings { get; set; }
        public static RelayCommand Plugin { get; set; }

        public static RelayCommand<Window> About { get; set; }

        public static RelayCommand<Window> Upgrade { get; set; }

        public static RelayCommand<Window> Thanks { get; set; }

        static OpenWindow()
        {
            Settings = new RelayCommand(() => OpenWindowByName("Window_Settings"));
            Plugin = new RelayCommand(() => OpenWindowByName("Window_Plugin"));
            About = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_About", parent));
            //Upgrade = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Upgrade", parent));

            Thanks = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Thanks", parent));

            Upgrade = new RelayCommand<Window>(parent =>
            {
                SuperUpgrader upgrader = new SuperUpgrader();
                upgrader.InfoUrl = UrlManager.UpdateUrl;
                upgrader.FileListUrl = UrlManager.UpdateFileListUrl;
                upgrader.FilePathUrl = UrlManager.UpdateFilePathUrl;
                upgrader.ReleaseUrl = UrlManager.ReleaseUrl;
                upgrader.UpgradeSource = UrlManager.UpgradeSource;
                upgrader.Language = ConfigManager.Settings.CurrentLanguage;
                upgrader.Header = CrawlerHeader.GitHub;
                upgrader.Logger = null;//todo
                SuperControls.Style.Upgrade.Dialog_Upgrade dialog_Upgrade =
                new SuperControls.Style.Upgrade.Dialog_Upgrade(parent, false, "", "", "", upgrader);
                dialog_Upgrade.LocalVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                dialog_Upgrade.ShowDialog();
            });
        }
    }
}
