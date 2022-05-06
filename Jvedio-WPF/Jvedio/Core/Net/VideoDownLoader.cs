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
using Jvedio.Utils.Net;
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
        public event EventHandler InfoUpdate;
        public event EventHandler MessageCallBack;
        public DownLoadProgress downLoadProgress;
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



        public (string url, string code) getUrlAndCode(CrawlerServer server)
        {
            string baseUrl = server.Url;
            string serverName = server.ServerName.ToUpper();
            string url = baseUrl;
            string code = "";
            string vid = CurrentVideo.VID;
            if (serverName.Equals("BUS"))
            {
                url = $"{baseUrl}{vid}";
                code = vid;
            }
            else if (serverName.Equals("DB"))
            {
                url = baseUrl;
                IWrapper<UrlCode> wrapper = new SelectWrapper<UrlCode>()
                    .Eq("WebType", "db").Eq("ValueType", "video").Eq("LocalValue", vid);
                UrlCode urlCode = GlobalMapper.urlCodeMapper.selectOne(wrapper);
                if (urlCode != null)
                    code = urlCode.RemoteValue;
            }
            else if (serverName.Equals("FC"))
            {
                // 后面必须要有 /
                url = $"{baseUrl}article/{vid.Replace("FC2-", "")}/";
            }
            else if (serverName.Equals("BUS"))
            {

            }
            else if (serverName.Equals("BUS"))
            {

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
                    .Where(arg => arg.Enabled && arg.ServerName.ToLower().Equals(pluginInfo.ServerName.ToLower())
                    && arg.Available == 1 && !string.IsNullOrEmpty(arg.Url)).ToList();

                if (crawlers != null && crawlers.Count > 0) break;
            }
            if (crawlers == null || crawlers.Count == 0) throw new CrawlerNotFoundException();
            // todo 爬虫调度器
            crawlers = crawlers.OrderBy(arg => arg.ServerName).ToList();
            CrawlerServer crawler = crawlers[0];
            return (crawler, pluginInfo);
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



        private async void DownLoad(object o)
        {

            //下载信息
            //    Movie movie = o as Movie;
            //    if (movie.id.ToUpper().StartsWith("FC2"))
            //        SemaphoreFC2.WaitOne();
            //    else
            //        Semaphore.WaitOne();//阻塞
            //    if (Cancel || string.IsNullOrEmpty(movie.id))
            //    {
            //        if (movie.id.ToUpper().StartsWith("FC2"))
            //            SemaphoreFC2.Release();
            //        else
            //            Semaphore.Release();
            //        return;
            //    }

            //    //下载信息
            //    State = DownLoadState.DownLoading;
            //    if (movie.IsToDownLoadInfo() || enforce)
            //    {
            //        //满足一定条件才下载信息
            //        HttpResult httpResult = await HTTP.DownLoadFromNet(movie);
            //        if (httpResult != null)
            //        {
            //            if (httpResult.Success)
            //            {
            //                InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = movie, progress = downLoadProgress.value, Success = httpResult.Success });//委托到主界面显示
            //            }
            //            else
            //            {
            //                string error = httpResult.Error != "" ? httpResult.Error : httpResult.StatusCode.ToStatusMessage();
            //                MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($" {movie.id} {Jvedio.Language.Resources.DownloadMessageFailFor}：{error}"));
            //            }
            //        }
            //    }
            //    DetailMovie dm = DataBase.SelectDetailMovieById(movie.id);

            //    if (dm == null)
            //    {
            //        if (movie.id.ToUpper().StartsWith("FC2"))
            //            SemaphoreFC2.Release();
            //        else
            //            Semaphore.Release();
            //        return;
            //    }

            //    if (!File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") || enforce)
            //    {
            //        await HTTP.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);//下载大图
            //    }



            //    //fc2 没有缩略图
            //    if (dm.id.IndexOf("FC2") >= 0)
            //    {
            //        //复制海报图作为缩略图
            //        if (File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") && !File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg"))
            //        {
            //            FileHelper.TryCopyFile(BasePicPath + $"BigPic\\{dm.id}.jpg", BasePicPath + $"SmallPic\\{dm.id}.jpg");
            //        }

            //    }
            //    else
            //    {
            //        if (!File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg") || enforce)
            //        {
            //            await HTTP.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id); //下载小图
            //        }
            //    }
            //    dm.smallimage = ImageProcess.GetBitmapImage(dm.id, "SmallPic");
            //    InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State });//委托到主界面显示
            //    dm.bigimage = ImageProcess.GetBitmapImage(dm.id, "BigPic");
            //    lock (downLoadProgress.lockobject) downLoadProgress.value += 1;//完全下载完一个影片
            //    InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State, Success = true });//委托到主界面显示
            //    Task.Delay(Delay.MEDIUM).Wait();//每个线程之间暂停
            //    //取消阻塞
            //    if (movie.id.ToUpper().IndexOf("FC2") >= 0)
            //        SemaphoreFC2.Release();
            //    else
            //        Semaphore.Release();
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
