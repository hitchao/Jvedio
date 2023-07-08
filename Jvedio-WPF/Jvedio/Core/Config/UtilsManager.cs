using Jvedio.Core.Crawler;

namespace Jvedio.Core.Config
{
    public class UtilsManager
    {
        public static void OnUtilSettingChange()
        {
            SuperUtils.Config.SettingManager.HttpTimeout = (int)ConfigManager.ProxyConfig.HttpTimeout;
            SuperUtils.Config.SettingManager.IgnoreCertVal = ConfigManager.Settings.IgnoreCertVal;

            // 代理设置
            CrawlerHeader.Init();
        }
    }
}
