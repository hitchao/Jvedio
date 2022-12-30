
using SuperUtils.NetWork.Entity;
using Jvedio.Core.Interfaces;
using Jvedio.Core.Logs;
using Newtonsoft.Json;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jvedio.Core.Crawler
{
    /// <summary>
    /// 服务器源
    /// </summary>
    public class CrawlerServer : INotifyPropertyChanged, IDictKeys
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CrawlerServer()
        {
            PluginID = string.Empty;
        }

        public string PluginID { get; set; }

        private string _Url;

        public string Url
        {
            get
            {
                return _Url;
            }

            set
            {
                _Url = value;
                RaisePropertyChanged();
            }
        }

        private bool _Enabled;

        public bool Enabled
        {
            get
            {
                return _Enabled;
            }

            set
            {
                _Enabled = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// -1 不可用，1-可用，2-测试中
        /// </summary>
        private int _Available;

        public int Available
        {
            get
            {
                return _Available;
            }

            set
            {
                _Available = value;
                RaisePropertyChanged();
            }
        }

        public string LastRefreshDate { get; set; }

        private string _Cookies;

        public string Cookies
        {
            get
            {
                return _Cookies;
            }

            set
            {
                _Cookies = value;
                RaisePropertyChanged();
            }
        }

        private string _Headers { get; set; }

        public string Headers
        {
            get
            {
                return _Headers;
            }

            set
            {
                _Headers = value;
                RaisePropertyChanged();
            }
        }

        public bool isHeaderProper()
        {
            if (string.IsNullOrEmpty(Headers)) return true;
            try
            {
                Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Headers);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static RequestHeader parseHeader(CrawlerServer server)
        {
            if (server == null) return CrawlerHeader.Default;
            RequestHeader result = new RequestHeader();
            result.WebProxy = ConfigManager.ProxyConfig.GetWebProxy();
            result.TimeOut = ConfigManager.ProxyConfig.HttpTimeout * 1000; // 转为 ms
            string header = server.Headers;
            if (string.IsNullOrEmpty(header)) return CrawlerHeader.Default;
            Dictionary<string, string> dict = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(header);
            if (dict != null && dict.Count > 0)
            {
                if (!dict.ContainsKey("cookie") && !string.IsNullOrEmpty(server.Cookies))
                    dict.Add("cookie", server.Cookies);
                result.Headers = dict;
            }

            return result;
        }

        public bool HasAllKeys(Dictionary<object, object> dict)
        {
            if (dict == null || dict.Count == 0) return false;
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (var item in propertyInfos)
            {
                if (!dict.ContainsKey(item.Name)) return false;
                if (dict[item.Name] == null) return false;
            }

            return true;
        }
    }
}
