using System;

namespace Jvedio.Core.Global
{
    public static class UrlManager
    {
        // *************** 网址 ***************
        public static readonly string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public static readonly string UpgradeSource = "https://hitchao.github.io";
        public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/latest.json";

        // public static readonly string UpdateUrl = "https://hitchao.github.io:444/jvedioupdate/latest.json";   // 旧版本
        // public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/Version";           // 旧版本
        public static readonly string UpdateExeVersionUrl = "https://hitchao.github.io/jvedioupdate/update";
        public static readonly string UpdateExeUrl = "https://hitchao.github.io/jvedioupdate/JvedioUpdate.exe";

        // public static readonly string NoticeUrl = "https://hitchao.github.io/JvedioWebPage/notice";           // 旧版本
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
