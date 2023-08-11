using SuperControls.Style.Upgrade;
using System.Collections.Generic;
using System.Linq;
using static Jvedio.App;

namespace Jvedio.Core.Global
{
    public static class UrlManager
    {
        #region "const"
        private const string DonateJsonBasePath = "SuperStudio-Donate";

        public const string ServerHelpUrl = "https://github.com/hitchao/Jvedio/wiki";
        public const string ProjectUrl = "https://github.com/hitchao/Jvedio";
        public const string WebPage = "https://hitchao.github.io/JvedioWebPage/";
        public const string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public const string UpgradeSource = "https://hitchao.github.io/";
        public const string ServerUrl = "https://hitchao.github.io/hitchao/jvedio-server/jvedio-server.jar";


        public const string NoticeUrl = "https://hitchao.github.io/jvedioupdate/notice.json";
        public const string FeedBackUrl = "https://github.com/hitchao/Jvedio/issues";
        public const string WikiUrl = "https://github.com/hitchao/Jvedio/wiki/02_Beginning";
        public const string WebPageUrl = "https://hitchao.github.io/JvedioWebPage/";
        public const string ThemeDIY = "https://hitchao.github.io/JvedioWebPage/theme.html";
        public const string PLUGIN_LIST_URL = "https://hitchao.github.io/Jvedio-Plugin/pluginlist.json";
        public const string PLUGIN_LIST_BASE_URL = "https://hitchao.github.io/Jvedio-Plugin/";
        public const string FFMPEG_URL = "https://www.gyan.dev/ffmpeg/builds/";
        public const string PLUGIN_UPLOAD_HELP = "https://github.com/hitchao/Jvedio/wiki/08_Plugin";
        public const string HEADER_HELP = "https://github.com/hitchao/Jvedio/wiki/05_Headers";


        #endregion

        #region "属性"

        public static Dictionary<string, UpgradeSource> UpgradeSourceDict { get; set; } = new Dictionary<string, UpgradeSource>()
        {
            {"Github",new UpgradeSource(UpgradeSource,ReleaseUrl,"jvedioupdate") },
            {"Github加速",new UpgradeSource("https://cdn.jsdelivr.net/gh/hitchao/",ReleaseUrl,"jvedioupdate") },
            {"StormKit",new UpgradeSource("https://divealpine-ab8zhe--77466901127398.stormkit.dev/",ReleaseUrl,"") },
        };

        public static List<string> UpgradeSourceKeys { get; set; } = UpgradeSourceDict.Keys.ToList();

        /// <summary>
        /// 用户切换源的时候存储起来
        /// </summary>
        private static int RemoteIndex { get; set; } = (int)ConfigManager.Settings.RemoteIndex;

        #endregion

        public static int GetRemoteIndex()
        {
            return RemoteIndex;
        }
        public static void SetRemoteIndex(int idx)
        {
            RemoteIndex = idx;
            Logger.Info($"set remote index: {RemoteIndex}");
        }
        public static string GetRemoteBasePath()
        {
            if (RemoteIndex < 0 || RemoteIndex >= UpgradeSourceKeys.Count)
                RemoteIndex = 0;

            if (RemoteIndex >= UpgradeSourceKeys.Count)
                return "";

            return UpgradeSourceDict[UpgradeSourceKeys[RemoteIndex]].BaseUrl;
        }

        public static string GetDonateJsonUrl()
        {
            return $"{GetRemoteBasePath()}{DonateJsonBasePath}/config.json";
        }

        public static string GetPluginUrl()
        {
            return PLUGIN_LIST_BASE_URL;
        }
    }
}
