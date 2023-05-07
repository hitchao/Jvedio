using Jvedio.Core.Config.Base;
using static Jvedio.LogManager;
using SuperUtils.Security;
using MihaZupan;
using SuperUtils.NetWork;
using System;
using System.Net;
using System.ComponentModel;

namespace Jvedio.Core.Config
{
    public class DownloadConfig : AbstractConfig
    {

        private DownloadConfig() : base("DownloadConfig")
        {

        }

        private static DownloadConfig _instance = null;

        public static DownloadConfig CreateInstance()
        {
            if (_instance == null) _instance = new DownloadConfig();

            return _instance;
        }
        [DefaultValue(true)]
        public bool DownloadInfo { get; set; }
        [DefaultValue(true)]
        public bool DownloadThumbNail { get; set; }
        [DefaultValue(true)]
        public bool DownloadPoster { get; set; }

        [DefaultValue(false)]
        public bool DownloadPreviewImage { get; set; }

        [DefaultValue(true)]
        public bool DownloadActor { get; set; }

    }
}
