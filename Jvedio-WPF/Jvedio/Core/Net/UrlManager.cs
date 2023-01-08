using SuperControls.Style.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Core.Global
{
    public static class UrlManager
    {

        public static readonly string ProjectUrl = "https://github.com/hitchao/Jvedio";
        public static readonly string WebPage = "https://hitchao.github.io/JvedioWebPage/";
        public static readonly string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public static readonly string UpgradeSource = "https://hitchao.github.io/";
        public static Dictionary<string, UpgradeSource> UpgradeSourceDict = new Dictionary<string, UpgradeSource>()
        {
            {"Github",new UpgradeSource(UpgradeSource,ReleaseUrl,"jvedioupdate") },
            {"Github加速",new UpgradeSource("https://cdn.jsdelivr.net/gh/hitchao/",ReleaseUrl,"jvedioupdate") },
        };

        public static List<string> UpgradeSourceKeys = UpgradeSourceDict.Keys.ToList();
        private static int RemoteIndex = (int)ConfigManager.Settings.RemoteIndex; // 用户切换源的时候存储起来
        private static string DonateJsonBasePath = "SuperSudio-Donate";

        public static int GetRemoteIndex()
        {
            return RemoteIndex;
        }
        public static void SetRemoteIndex(int idx)
        {
            RemoteIndex = idx;
        }
        public static string GetRemoteBasePath()
        {
            if (RemoteIndex < 0 || RemoteIndex >= UpgradeSourceKeys.Count)
                RemoteIndex = 0;
            return UpgradeSourceDict[UpgradeSourceKeys[RemoteIndex]].BaseUrl;
        }

        public static string GetDonateJsonUrl()
        {
            return $"{GetRemoteBasePath()}{DonateJsonBasePath}/config.json";
        }



        // *************** 网址 ***************
        public static readonly string NoticeUrl = "https://hitchao.github.io/jvedioupdate/notice.json";
        public static readonly string FeedBackUrl = "https://github.com/hitchao/Jvedio/issues";
        public static readonly string WikiUrl = "https://github.com/hitchao/Jvedio/wiki/02_Beginning";
        public static readonly string WebPageUrl = "https://hitchao.github.io/JvedioWebPage/";

        public static readonly string ThemeDIY = "https://hitchao.github.io/JvedioWebPage/theme.html";
        public static readonly string PLUGIN_LIST_URL = "https://hitchao.github.io/Jvedio-Plugin/pluginlist.json";
        public static readonly string PLUGIN_LIST_BASE_URL = "https://hitchao.github.io/Jvedio-Plugin/";
        public static readonly string FFMPEG_URL = "https://www.gyan.dev/ffmpeg/builds/";
        public static readonly string PLUGIN_UPLOAD_HELP = "https://github.com/hitchao/Jvedio/wiki/08_Plugin";

        // public static readonly string FFMPEG_URL = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";
        [Obsolete]
        public static readonly string YoudaoUrl = "https://github.com/hitchao/Jvedio/wiki";
        [Obsolete]
        public static readonly string BaiduUrl = "https://github.com/hitchao/Jvedio/wiki";
    }
}
