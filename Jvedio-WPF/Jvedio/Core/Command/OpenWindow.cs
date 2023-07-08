using SuperUtils.WPF.VieModel;
using System.Windows;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio.Core.Command
{
    public class OpenWindow
    {
        public static RelayCommand<Window> Settings { get; set; }
        // public static RelayCommand<Window> Plugin { get; set; }

        //public static RelayCommand<Window> About { get; set; }

        //public static RelayCommand<Window> Upgrade { get; set; }

        public static RelayCommand<Window> Thanks { get; set; }

        static OpenWindow()
        {
            Settings = new RelayCommand<Window>(parent => OpenWindowByName("Window_Settings", App.Current.Windows, "Jvedio"));
            // Plugin = new RelayCommand<Window>(parent => OpenWindowByName("Window_Plugin"));
            //About = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_About", parent));
            //Upgrade = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Upgrade", parent));

            Thanks = new RelayCommand<Window>(parent => OpenWindowByName("Dialog_Thanks", App.Current.Windows, "Jvedio", parent));

            //Upgrade = new RelayCommand<Window>(parent =>
            //{
            //    SuperUpgrader upgrader = new SuperUpgrader();
            //    upgrader.InfoUrl = UrlManager.UpdateUrl;
            //    upgrader.FileListUrl = UrlManager.UpdateFileListUrl;
            //    upgrader.FilePathUrl = UrlManager.UpdateFilePathUrl;
            //    upgrader.ReleaseUrl = UrlManager.ReleaseUrl;
            //    upgrader.UpgradeSource = UrlManager.UpgradeSource;
            //    upgrader.Language = ConfigManager.Settings.CurrentLanguage;
            //    upgrader.Header = CrawlerHeader.GitHub;
            //    upgrader.Logger = null;//todo
            //                           // 写入配置文件
            //    upgrader.BeforeUpdateDelay = 5;
            //    upgrader.AfterUpdateDelay = 1;
            //    upgrader.UpDateFileDir = "TEMP";
            //    upgrader.AppName = "Jvedio.exe";

            //    SuperControls.Style.Upgrade.Dialog_Upgrade dialog_Upgrade = new SuperControls.Style.Upgrade.Dialog_Upgrade(parent, upgrader);
            //    dialog_Upgrade.LocalVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //    dialog_Upgrade.ShowDialog();
            //});
        }
    }
}
