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

    public enum ProxyModeEnum
    {
        /// <summary>
        /// 无代理
        /// </summary>
        None,

        /// <summary>
        /// 系统代理
        /// </summary>
        System,

        /// <summary>
        /// 自定义代理
        /// </summary>
        Custom
    }


    public enum ProxyTypeEnum
    {
        HTTP,
        SOCKS,
    }


    public class ProxyConfig : AbstractConfig
    {
        public const int DEFAULT_TIMEOUT = 10;

        public const ProxyModeEnum DEFAULT_PROXY_MODE = ProxyModeEnum.System;
        public const ProxyTypeEnum DEFAULT_PROXY_TYPE = ProxyTypeEnum.SOCKS;

        private ProxyConfig() : base("ProxyConfig")
        {
            HttpTimeout = DEFAULT_TIMEOUT;
        }

        private static ProxyConfig _instance = null;

        public static ProxyConfig CreateInstance()
        {
            if (_instance == null) _instance = new ProxyConfig();

            return _instance;
        }

        [DefaultValue((int)DEFAULT_PROXY_MODE)]
        public long ProxyMode { get; set; }

        [DefaultValue((int)DEFAULT_PROXY_TYPE)]
        public long ProxyType { get; set; }

        public string Server { get; set; }

        public long Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        private long _HttpTimeout = DEFAULT_TIMEOUT;

        public long HttpTimeout
        {
            get
            {
                return _HttpTimeout;
            }

            set
            {
                if (value <= 0)
                    _HttpTimeout = DEFAULT_TIMEOUT;
                else
                    _HttpTimeout = value;
            }
        }

        public IWebProxy GetWebProxy()
        {
            SuperWebProxy webProxy = new SuperWebProxy();
            if (ProxyMode == 1)
            {
                webProxy.ProxyMode = SuperUtils.NetWork.Enums.ProxyMode.System;
                return webProxy.GetWebProxy();
            }
            else if (ProxyMode == 2)
            {
                webProxy.ProxyMode = SuperUtils.NetWork.Enums.ProxyMode.Custom;
                webProxy.ProxyProtocol = ProxyType == 0 ? SuperUtils.NetWork.Enums.ProxyProtocol.HTTP : SuperUtils.NetWork.Enums.ProxyProtocol.SOCKS;
                webProxy.Server = Server;
                webProxy.Port = (int)Port;
                webProxy.Pwd = GetRealPwd();
                return webProxy.GetWebProxy();
            }

            return null;
        }

        public string GetRealPwd()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                try
                {
                    return JvedioLib.Security.Encrypt.AesDecrypt(Password, 0);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return string.Empty;
        }
    }
}
