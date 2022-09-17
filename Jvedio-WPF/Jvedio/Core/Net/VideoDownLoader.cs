using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Core.Plugins;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public class VideoDownLoader
    {
        public DownLoadState State = DownLoadState.DownLoading;

        private bool Canceld { get; set; }

        private CancellationToken cancellationToken { get; set; }

        public Video CurrentVideo { get; set; }

        public RequestHeader Header { get; set; }

        public List<CrawlerServer> CrawlerServers { get; set; } //该资源支持的爬虫刮削器

        public VideoDownLoader(Video video, CancellationToken token)
        {
            CurrentVideo = video;
            cancellationToken = token;
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
            (CrawlerServer crawler, PluginMetaData PluginMetaData) = getCrawlerServer();
            (string url, string code) = getUrlAndCode(crawler);
            Header = CrawlerServer.parseHeader(crawler);
            callBack?.Invoke(Header);
            Dictionary<string, string> dataInfo = CurrentVideo.ToDictionary();
            if (!dataInfo.ContainsKey("DataCode"))
                dataInfo.Add("DataCode", code);
            else
                dataInfo["DataCode"] = code;
            dataInfo["url"] = url;
            // 路径就是 pluginID 组合
            Plugin plugin = new Plugin(PluginMetaData.GetFilePath(), "GetInfo", new object[] { false, Header, dataInfo });
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
            string url = baseUrl;
            string code = string.Empty;
            string vid = CurrentVideo.VID;
            //if ("BUS".Equals(serverName))
            //{
            //    url = $"{baseUrl}{vid}";
            //    code = vid;
            //}
            //else if ("DB".Equals(serverName))
            //{
            //    url = baseUrl;
            //    IWrapper<UrlCode> wrapper = new SelectWrapper<UrlCode>()
            //        .Eq("WebType", "db").Eq("ValueType", "video").Eq("LocalValue", vid);
            //    UrlCode urlCode = GlobalMapper.urlCodeMapper.selectOne(wrapper);
            //    if (urlCode != null)
            //        code = urlCode.RemoteValue;
            //}
            //else if ("FC".Equals(serverName))
            //{
            //    // 后面必须要有 /
            //    url = $"{baseUrl}article/{vid.Replace("FC2-", "")}/";
            //}
            return (url, code);
        }

        public (CrawlerServer, PluginMetaData) getCrawlerServer()
        {
            // 获取信息类型，并设置爬虫类型

            if (ConfigManager.ServerConfig.CrawlerServers.Count == 0 || CrawlerManager.PluginMetaDatas?.Count == 0)
                throw new CrawlerNotFoundException();
            List<PluginMetaData> PluginMetaDatas = CrawlerManager.PluginMetaDatas.Where(arg => arg.Enabled).ToList();
            if (PluginMetaDatas.Count == 0)
                throw new CrawlerNotFoundException();

            PluginMetaData PluginMetaData = null;
            List<CrawlerServer> crawlers = null;
            for (int i = 0; i < PluginMetaDatas.Count; i++)
            {
                // 一组支持刮削的网址列表
                PluginMetaData = PluginMetaDatas[i];
                crawlers = ConfigManager.ServerConfig.CrawlerServers
                    .Where(arg => arg.Enabled && !string.IsNullOrEmpty(arg.PluginID) &&
                    arg.PluginID.ToLower().Equals(PluginMetaData.PluginID.ToLower())
                    && arg.Available == 1 && !string.IsNullOrEmpty(arg.Url)).ToList();

                if (crawlers != null && crawlers.Count > 0) break;
            }
            if (crawlers == null || crawlers.Count == 0)
                throw new CrawlerNotFoundException();
            // todo 爬虫调度器
            crawlers = crawlers.OrderBy(arg => arg.PluginID).ToList();
            CrawlerServer crawler = crawlers[0];        // 如果有多个可用的网址，默认取第一个
            return (crawler, PluginMetaData);
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
