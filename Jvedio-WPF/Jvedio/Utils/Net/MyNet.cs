using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jvedio.Utils;
using static Jvedio.GlobalVariable;
using Jvedio.Utils.Net;

namespace Jvedio
{

    /// <summary>
    /// 每次访问一个网址都实例化一个网络对象
    /// </summary>
    public class MyNet : Net
    {
        public MyNet() : base(new int[] {
                                 Properties.Settings.Default.Timeout_tcp,
                                 Properties.Settings.Default.Timeout_forcestop,
                                  Properties.Settings.Default.Timeout_http * 1000,
                                  Properties.Settings.Default.Timeout_download * 1000,
                                  Properties.Settings.Default.Timeout_stream * 1000
            })
        {

        }

        public override void Log(string str)
        {
            Logger.LogN(str);
        }

        /// <summary>
        /// 异步下载图片
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="imageType"></param>
        /// <param name="ID"></param>
        /// <param name="Cookie"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> DownLoadImage(string Url, ImageType imageType, string ID, string Cookie = "", Action<int> callback = null)
        {
            //如果文件存在则不下载
            string filepath = BasePicPath;
            if (imageType == ImageType.SmallImage)
            {
                filepath = Path.Combine(filepath, "SmallPic", ID + ".jpg");
            }
            else if (imageType == ImageType.BigImage)
            {
                filepath = Path.Combine(filepath, "BigPic", ID + ".jpg");
            }
            if (File.Exists(filepath)) return (true, "");

            if (!Url.IsProperUrl()) return (false, "");
            HttpResult httpResult = await new MyNet().DownLoadFile(Url.Replace("\"", "'"), setCookie: Cookie);

            bool result = false;
            string cookie = "";


            if (httpResult == null)
            {
                Logger.LogN($" {Jvedio.Language.Resources.DownLoadImageFail}：{Url}");
                callback?.Invoke((int)HttpStatusCode.Forbidden);
                result = false;
            }
            else
            {
                if (httpResult.Headers?.SetCookie != null)
                    cookie = httpResult.Headers.SetCookie.Split(';')[0];
                else
                    cookie = Cookie;
                result = true;
                ImageProcess.SaveImage(ID, httpResult.FileByte, imageType, Url);
            }
            return (result, cookie);
        }



        public static async Task<bool> DownLoadActress(string ID, string Name, Action<string> callback)
        {
            bool result = false;
            string Url = JvedioServers.Bus.Url + $"star/{ID}";
            HttpResult httpResult = null;
            httpResult = await new MyNet().Http(Url);
            string error = "";
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != "")
            {
                //id搜索
                BusParse busParse = new BusParse(ID, httpResult.SourceCode, VedioType.骑兵);
                Actress actress = busParse.ParseActress();
                if (actress == null && string.IsNullOrEmpty(actress.birthday) && actress.age == 0 && string.IsNullOrEmpty(actress.birthplace))
                {
                    error = $"{Jvedio.Language.Resources.NoActorInfo}：{Url}";

                }
                else
                {
                    actress.sourceurl = Url;
                    actress.source = "javbus";
                    actress.id = ID;
                    actress.name = Name;
                    //保存信息
                    DataBase.InsertActress(actress);
                    result = true;
                }
            }
            else if (httpResult != null)
            {
                error = httpResult.StatusCode.ToStatusMessage();
            }
            else
            {
                error = Jvedio.Language.Resources.HttpFail;
            }
            Console.WriteLine(error);
            callback.Invoke(error);
            Logger.LogN($"URL={Url},Message-{error}");
            return result;
        }


        public async static Task<(bool, string)> DownLoadSmallPic(DetailMovie dm, bool overWrite = false)
        {
            if (overWrite) return await DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id);
            //不存在才下载
            if (!File.Exists(GlobalVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg"))
            {
                return await DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id);
            }
            else return (false, "");

        }


        public async static Task<(bool, string)> DownLoadBigPic(DetailMovie dm, bool overWrite = false)
        {

            if (overWrite) return await DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);


            if (!File.Exists(GlobalVariable.BasePicPath + $"BigPic\\{dm.id}.jpg"))
            {
                return await DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);
            }
            else
            {
                return (false, "");
            }
        }

        public static async Task<bool> ParseSpecifiedInfo(WebSite webSite, string id, string url)
        {
            HttpResult httpResult = null;

            if (webSite == WebSite.Bus) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
            else if (webSite == WebSite.BusEu) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.BusEurope.Cookie });
            else if (webSite == WebSite.Library) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Library.Cookie });
            else if (webSite == WebSite.Jav321) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Jav321.Cookie });
            else if (webSite == WebSite.FC2) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.FC2.Cookie });
            else if (webSite == WebSite.DB) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.DB.Cookie });
            else if (webSite == WebSite.DMM) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.DMM.Cookie });
            else if (webSite == WebSite.MOO) httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.MOO.Cookie });
            else httpResult = await new MyNet().Http(url);

            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != "")
            {
                string content = httpResult.SourceCode;
                Dictionary<string, string> Info = new Dictionary<string, string>();

                if (webSite == WebSite.Bus)
                {
                    Info = new BusParse(id, content, Identify.GetVideoType(id)).Parse();
                    Info.Add("source", "javbus");
                }
                else if (webSite == WebSite.BusEu)
                {
                    Info = new BusParse(id, content, VedioType.欧美).Parse();
                    Info.Add("source", "javbus");
                }
                else if (webSite == WebSite.DB)
                {
                    Info = new JavDBParse(id, content, url.Split('/').Last()).Parse();
                    Info.Add("source", "javdb");
                }
                else if (webSite == WebSite.Library)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "javlibrary");
                }
                else if (webSite == WebSite.Jav321)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "Jav321");
                }
                else if (webSite == WebSite.DMM)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "DMM");
                }
                else if (webSite == WebSite.MOO)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "MOO");
                }
                else if (webSite == WebSite.FC2)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "FC2");
                }
                Info.Add("sourceurl", url);
                if (Info.Count > 2)
                {
                    FileProcess.SaveInfo(Info, id);
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// 从网络上下载信息
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        public static async Task<HttpResult> DownLoadFromNet(Movie movie)
        {
            HttpResult httpResult = null;
            string message = "";

            if (movie.vediotype == (int)VedioType.欧美)
            {
                if (JvedioServers.BusEurope.IsEnable)
                    httpResult = await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl();
                //else if (supportServices.BusEu.IsProperUrl() && !serviceEnables.BusEu) 
                //    message = Jvedio.Language.Resources.UrlEuropeNotset;
            }
            else
            {
                //FC2 影片
                if (movie.id.ToUpper().IndexOf("FC2") >= 0)
                {
                    //优先从 db 下载
                    if (JvedioServers.DB.IsEnable)
                        httpResult = await new DBCrawler(movie.id).Crawl();
                    //else if (supportServices.DB.IsProperUrl() && !serviceEnables.DB) 
                    //    message = Jvedio.Language.Resources.UrlDBNotset;

                    //db 未下载成功则去 fc2官网
                    if (httpResult == null)
                    {
                        if (JvedioServers.FC2.IsEnable)
                            httpResult = await new FC2Crawler(movie.id).Crawl();
                        //else if (supportServices.FC2.IsProperUrl() && !serviceEnables.FC2)
                        //    message = Jvedio.Language.Resources.UrlFC2Notset;
                    }
                }
                else
                {
                    //非FC2 影片
                    //优先从 Bus 下载
                    if (JvedioServers.Bus.IsEnable)
                        httpResult = await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl();
                    //else if (supportServices.Bus.IsProperUrl() && !serviceEnables.Bus)
                    //    message = Jvedio.Language.Resources.UrlBusNotset;

                    //Bus 未下载成功则去 library
                    if (httpResult == null)
                    {
                        if (JvedioServers.Library.IsEnable)
                            httpResult = await new LibraryCrawler(movie.id).Crawl();
                        //else if (supportServices.Library.IsProperUrl() && !serviceEnables.Library)
                        //    message = Jvedio.Language.Resources.UrlLibraryNotset;
                    }

                    //library 未下载成功则去 DB
                    if (httpResult == null)
                    {
                        if (JvedioServers.DB.IsEnable)
                            httpResult = await new DBCrawler(movie.id).Crawl();
                        //else if (supportServices.DB.IsProperUrl() && !serviceEnables.DB)
                        //    message = Jvedio.Language.Resources.UrlDBNotset;
                    }

                    //DB未下载成功则去 FANZA
                    if (httpResult == null)
                    {
                        if (JvedioServers.DMM.IsEnable)
                            httpResult = await new FANZACrawler(movie.id).Crawl();
                        //else if (supportServices.DMM.IsProperUrl() && !serviceEnables.DMM)
                        //    message = Jvedio.Language.Resources.UrlDMMNotset;
                    }

                    //FANZA 未下载成功则去 MOO
                    if (httpResult == null)
                    {
                        if (JvedioServers.MOO.IsEnable)
                            httpResult = await new MOOCrawler(movie.id).Crawl();
                        //else if (supportServices.MOO.IsProperUrl() && !serviceEnables.MOO)
                        //    message = Jvedio.Language.Resources.UrlMOONotset;
                    }

                    //MOO 未下载成功则去 JAV321
                    if (httpResult == null)
                    {
                        if (JvedioServers.Jav321.IsEnable)
                            httpResult = await new Jav321Crawler(movie.id).Crawl();
                        //else if (supportServices.Jav321.IsProperUrl() && !serviceEnables.Jav321)
                        //    message = Jvedio.Language.Resources.UrlJAV321Notset;
                    }

                }

            }

            Movie newMovie = DataBase.SelectMovieByID(movie.id);
            if (newMovie != null && newMovie.title != "" && httpResult != null && httpResult.Error == "") httpResult.Success = true;
            if (httpResult == null && message != "") httpResult = new HttpResult() { Error = message, Success = false };
            return httpResult;
        }
    }
}
