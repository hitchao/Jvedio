using Jvedio.Core.Global;
using SuperControls.Style.Upgrade;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Crawler;
using System.Threading.Tasks;
using System.Windows;
using static Jvedio.App;

namespace Jvedio.Upgrade
{
    public static class UpgradeHelper
    {
        public const int AUTO_CHECK_UPGRADE_DELAY = 60 * 1000;

        #region "属性"

        private static bool WindowClosed { get; set; }
        private static Window Window { get; set; }

        private static SuperUpgrader Upgrader { get; set; }
        private static Dialog_Upgrade dialog_Upgrade { get; set; }
        #endregion

        public static void Init(Window parent)
        {
            Upgrader = new SuperUpgrader();
            Upgrader.UpgradeSourceDict = UrlManager.UpgradeSourceDict;
            Upgrader.UpgradeSourceIndex = UrlManager.GetRemoteIndex();
            Upgrader.Language = "zh-CN";
            Upgrader.Header = new CrawlerHeader(SuperWebProxy.SystemWebProxy).Default;
            Upgrader.Logger = null;//todo Upgrader.Logger
            Upgrader.BeforeUpdateDelay = 5;
            Upgrader.AfterUpdateDelay = 1;
            Upgrader.UpDateFileDir = "TEMP";
            Upgrader.AppName = "Jvedio.exe";
            Window = parent;
            WindowClosed = true;
            Logger.Info("init upgrade ok");
        }

        public static void CreateDialog_Upgrade()
        {
            dialog_Upgrade = new Dialog_Upgrade(Upgrader);
            dialog_Upgrade.LocalVersion = App.GetLocalVersion(false);
            dialog_Upgrade.OnSourceChanged += (s, e) => {
                // 保存当前选择的地址
                int index = e.NewValue;
                UrlManager.SetRemoteIndex(index);
                ConfigManager.Settings.RemoteIndex = index;
                ConfigManager.Settings.Save();
            };
            dialog_Upgrade.Closed += (s, e) => {
                WindowClosed = true;
            };

            dialog_Upgrade.OnExitApp += () => {
                Application.Current.Shutdown();
            };

            WindowClosed = false;
            Logger.Info("create upgrade dialog ok");
        }



        public static void OpenWindow()
        {
            if (WindowClosed)
                CreateDialog_Upgrade();

            Logger.Info("open upgrade dialog");
            dialog_Upgrade?.ShowDialog();
        }

        public static async Task<(string LatestVersion, string ReleaseDate, string ReleaseNote)> GetUpgradeInfo()
        {
            if (Upgrader == null)
                return (null, null, null);
            return await Upgrader.GetUpgradeInfo();
        }


    }
}
