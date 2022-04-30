using DynamicData.Annotations;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Plugins;
using Jvedio.Utils;
using Newtonsoft.Json;
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
    public class CrawlerServer : INotifyPropertyChanged
    {
        public CrawlerServer()
        {

        }



        public string ServerName { get; set; }
        public string Name { get; set; }
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }
        public string _Headers { get; set; }
        public string Headers
        {
            get
            {
                return _Headers;
            }

            set
            {
                _Headers = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                System.Console.WriteLine(ex.Message);
                return false;
            }

        }

        public static RequestHeader parseHeader(CrawlerServer server)
        {
            RequestHeader result = new RequestHeader();
            result.WebProxy = GlobalConfig.ProxyConfig.GetWebProxy();
            result.TimeOut = GlobalConfig.ProxyConfig.HttpTimeout * 1000;// 转为 ms
            string header = server.Headers;
            if (string.IsNullOrEmpty(header)) return null;
            try
            {
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);

                if (dict != null && dict.Count > 0)
                {
                    if (!dict.ContainsKey("cookie") && !string.IsNullOrEmpty(server.Cookies))
                        dict.Add("cookie", server.Cookies);

                    result.Headers = dict;


                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }

    }

}
