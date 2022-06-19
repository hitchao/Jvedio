using DynamicData.Annotations;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Jvedio.Entity;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Core.Crawler;
using Jvedio.Core.Exceptions;
using Jvedio.Core.Plugins;
using Jvedio.CommonNet.Entity;
using Jvedio.CommonNet.Crawler;
using Jvedio.CommonNet;
using Jvedio.Core.SimpleORM;
using System.Net;

namespace Jvedio.Core.Net
{
    public class VideoDownLoader
    {
        public DownLoadState State = DownLoadState.DownLoading;


        private bool Canceld { get; set; }
        private CancellationToken cancellationToken { get; set; }

        public Video CurrentVideo { get; set; }
        public string InfoType { get; set; }
        public RequestHeader Header { get; set; }

        public List<CrawlerServer> CrawlerServers { get; set; } //该资源支持的爬虫刮削器
        public VideoDownLoader(Video video, CancellationToken token)
        {
            CurrentVideo = video;
            cancellationToken = token;
            InfoType = CurrentVideo.getServerInfoType().ToLower();
        }


        /// <summary>
        /// 取消下载
        /// </summary>
        public void Cancel()
        {
            Canceld = true;
            State = DownLoadState.Fail;
        }


        public async Task<Dictionary<string, object>> GetInfo(Action<RequestHeader> callBack)
        {
            //下载信息
            State = DownLoadState.DownLoading;
            Dictionary<string, object> result = new Dictionary<string, object>();
            (CrawlerServer crawler, PluginInfo pluginInfo) = getCrawlerServer();
            (string url, string code) = getUrlAndCode(crawler);
            Header = CrawlerServer.parseHeader(crawler);
            callBack?.Invoke(Header);

            Dictionary<string, string> dataInfo = CurrentVideo.toDictionary();
            if (!dataInfo.ContainsKey("DataCode"))
                dataInfo.Add("DataCode", code);
            else
                dataInfo["DataCode"] = code;

            Plugin plugin = new Plugin(pluginInfo.Path, "GetInfo", new object[] { url, Header, dataInfo });
            // 等待很久
            object o = await plugin.InvokeAsyncMethod();
            if (o is Dictionary<string, object> d)
            {
                return d;
            }
            return result;

        }


        public (string url, string code) getUrlAndCode(CrawlerServer server)
        {
            // server != NULL
            // server.ServerName != NULL
            string baseUrl = server.Url;
            string serverName = server.ServerName.ToUpper();
            string url = baseUrl;
            string code = "";
            string vid = CurrentVideo.VID;
            if ("BUS".Equals(serverName))
            {
                url = $"{baseUrl}{vid}";
                code = vid;
            }
            else if ("DB".Equals(serverName))
            {
                url = baseUrl;
                IWrapper<UrlCode> wrapper = new SelectWrapper<UrlCode>()
                    .Eq("WebType", "db").Eq("ValueType", "video").Eq("LocalValue", vid);
                UrlCode urlCode = GlobalMapper.urlCodeMapper.selectOne(wrapper);
                if (urlCode != null)
                    code = urlCode.RemoteValue;
            }
            else if ("FC".Equals(serverName))
            {
                // 后面必须要有 /
                url = $"{baseUrl}article/{vid.Replace("FC2-", "")}/";
            }
            return (url, code);
        }


        public (CrawlerServer, PluginInfo) getCrawlerServer()
        {
            // 获取信息类型，并设置爬虫类型

            if (string.IsNullOrEmpty(InfoType) || GlobalConfig.ServerConfig.CrawlerServers.Count == 0
                || Global.Plugins.Crawlers.Count == 0
                )
                throw new CrawlerNotFoundException();

            List<PluginInfo> pluginInfos = Global.Plugins.Crawlers.Where(arg => arg.Enabled && arg.InfoType.Split(',')
                                           .Select(item => item.ToLower()).Contains(InfoType)).ToList();
            if (pluginInfos.Count == 0)
                throw new CrawlerNotFoundException();

            PluginInfo pluginInfo = null;
            List<CrawlerServer> crawlers = null;
            for (int i = 0; i < pluginInfos.Count; i++)
            {
                // 一组支持刮削的网址列表
                pluginInfo = pluginInfos[i];
                crawlers = GlobalConfig.ServerConfig.CrawlerServers
                    .Where(arg => arg.Enabled && !string.IsNullOrEmpty(arg.ServerName) &&
                    arg.ServerName.ToLower().Equals(pluginInfo.ServerName.ToLower())
                    && arg.Available == 1 && !string.IsNullOrEmpty(arg.Url)).ToList();

                if (crawlers != null && crawlers.Count > 0) break;
            }
            if (crawlers == null || crawlers.Count == 0) throw new CrawlerNotFoundException();
            // todo 爬虫调度器
            crawlers = crawlers.OrderBy(arg => arg.ServerName).ToList();
            CrawlerServer crawler = crawlers[0];        // 如果有多个可用的网址，默认取第一个
            return (crawler, pluginInfo);
        }



        public async Task<byte[]> DownloadImage(string url, RequestHeader header, Action<string> onError = null)
        {
            try
            {
                HttpResult httpResult = await HttpHelper.AsyncDownLoadFile(url, header);
                return httpResult.FileByte;
            }
            catch (WebException ex)
            {
                onError?.Invoke(ex.Message);
            }
            return null;
        }



        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }
    }













}
