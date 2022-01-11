
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jvedio.Utils;
using static Jvedio.GlobalVariable;
using System.IO;
using System.Net;
using Jvedio.Utils.Net;

namespace Jvedio
{

    /// <summary>
    /// 该演员曾出演过的影片
    /// </summary>
    public class ActorSearch
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public string Link { get; set; }
        public string Img { get; set; }
        public string Tag { get; set; }

        public ActorSearch(string name)
        {
            Name = name;
            ID = 0;
            Link = "";
            Img = "";
            Tag = "";
        }

        public ActorSearch() : this("") { }


    }

    public abstract class ActorCrawler
    {
        protected string Url;//网址
        protected CrawlerHeader headers;
        protected HttpResult httpResult;

        public string Name { get; set; }//必须给出演员名字

        public ActorCrawler(string name)
        {
            Name = name;
            Url = "";
            headers = new CrawlerHeader();
            httpResult = null;
        }

        public ActorCrawler() : this("") { }


        protected abstract void InitHeaders();
        protected abstract Dictionary<string, string> GetInfo();

        protected abstract void ParseCookies(string SetCookie);

        public abstract Task<HttpResult> Crawl();

    }


    public class BusActorCrawler : ActorCrawler
    {


        public VedioType VedioType { get; set; }
        public BusActorCrawler(string Id) : base(Id)
        {


        }

        public override async Task<HttpResult> Crawl()
        {
            if (Url.IsProperUrl()) InitHeaders();
            httpResult = await new MyNet().Http(Url, headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {

                httpResult.Success = true;
                ParseCookies(httpResult.Headers.SetCookie);
            }
            return httpResult;
        }


        //TODO 并发问题
        protected override void ParseCookies(string SetCookie)
        {
            if (string.IsNullOrEmpty(SetCookie)) return;
            List<string> Cookies = new List<string>();
            var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
            foreach (var item in values)
            {
                if (item.IndexOf('=') < 0) continue;
                string key = item.Split('=')[0];
                string value = item.Split('=')[1];
                if (key == "__cfduid" || key == "PHPSESSID" || key == "existmag") Cookies.Add(key + "=" + value);
            }
            string cookie = string.Join(";", Cookies);
            if (VedioType == VedioType.欧美)
                JvedioServers.BusEurope.Cookie = cookie;
            else
                JvedioServers.Bus.Cookie = cookie;
            JvedioServers.Save();
        }


        protected override void InitHeaders()
        {
            headers = new CrawlerHeader()
            {
                Cookies = VedioType == VedioType.欧美 ? JvedioServers.BusEurope.Cookie : JvedioServers.Bus.Cookie
            };
        }


        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }

    }

    public class FC2ActorCrawler : ActorCrawler
    {

        public FC2ActorCrawler(string Id) : base(Id)
        {

        }


        protected override void InitHeaders()
        {
            headers = new CrawlerHeader()
            {
                Cookies = JvedioServers.FC2.Cookie
            };
        }

        public override async Task<HttpResult> Crawl()
        {
            return null;
        }


        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }

        protected override void ParseCookies(string SetCookie)
        {
            if (SetCookie == null) return;
            if (JvedioServers.FC2.Cookie != "") return;
            List<string> Cookies = new List<string>();
            var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
            foreach (var item in values)
            {
                if (item.IndexOf('=') < 0) continue;
                string key = item.Split('=')[0];
                string value = item.Split('=')[1];
                if (key == "CONTENTS_FC2_PHPSESSID" || key == "contents_mode" || key == "contents_func_mode") Cookies.Add(key + "=" + value);
            }
            string cookie = string.Join(";", Cookies);
            JvedioServers.FC2.Cookie = cookie;
            JvedioServers.Save();
        }
    }
    public class DBActorCrawler : ActorCrawler
    {

        protected string MovieCode;
        public DBActorCrawler(string Id) : base(Id)
        {

        }
        protected override void InitHeaders()
        {
            headers = new CrawlerHeader()
            {
                Cookies = JvedioServers.DB.Cookie
            };
        }

        public async Task<string> GetMovieCode(Action<string> callback = null)
        {
            return null;
        }

        protected string GetMovieCodeFromSearchResult(string content)
        {
            return null;
        }



        public override async Task<HttpResult> Crawl()
        {
            return null;

        }

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }


        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }


    }

    public class LibraryActorCrawler : ActorCrawler
    {
        protected string MovieCode;
        public LibraryActorCrawler(string Id) : base(Id)
        {

        }

        protected async Task<string> GetMovieCode()
        {
            return null;
        }

        private string GetMovieCodeFromSearchResult(string html)
        {
            return null;

        }

        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() { Cookies = JvedioServers.Library.Cookie };
        }


        public override async Task<HttpResult> Crawl()
        {
            return null;
        }

        protected override void ParseCookies(string SetCookie)
        {
            if (SetCookie == null) return;
            List<string> Cookies = new List<string>();
            var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
            foreach (var item in values)
            {
                if (item.IndexOf('=') < 0) continue;
                string key = item.Split('=')[0];
                string value = item.Split('=')[1];
                if (key == "__cfduid" || key == "__qca") Cookies.Add(key + "=" + value);
            }
            Cookies.Add("over18=18");
            string cookie = string.Join(";", Cookies);
            JvedioServers.Library.Cookie = cookie;
            JvedioServers.Save();
        }

        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }


    }

    public class FANZAActorCrawler : ActorCrawler
    {

        protected string MovieCode = "";
        public FANZAActorCrawler(string Id) : base(Id)
        {

        }

        protected async Task<string> GetMovieCode()
        {
            return null;
        }

        private string GetLinkFromSearchResult(string html)
        {
            return null;

        }




        public override async Task<HttpResult> Crawl()
        {
            return null;
        }

        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }

        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() { Cookies = JvedioServers.DMM.Cookie };
        }

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }
    }


    public class MOOActorCrawler : ActorCrawler
    {

        protected string MovieCode;
        public MOOActorCrawler(string Id) : base(Id)
        {

        }
        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() { Cookies = JvedioServers.MOO.Cookie };
        }

        public async Task<string> GetMovieCode(Action<string> callback = null)
        {

            //从网络获取
            HttpResult result = await new MyNet().Http(Url, headers, allowRedirect: false);
            //if (result != null && result.StatusCode == HttpStatusCode.Redirect) callback?.Invoke(Jvedio.Language.Resources.SearchTooFrequent);
            if (result != null && result.SourceCode != "")
                return GetMovieCodeFromSearchResult(result.SourceCode);

            //未找到

            //搜索太频繁

            return "";
        }

        protected string GetMovieCodeFromSearchResult(string content)
        {

            return null;
        }



        public override async Task<HttpResult> Crawl()
        {
            return null;

        }

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }


        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }


    }

    public class Jav321ActorCrawler : ActorCrawler
    {
        protected string MovieCode = "";
        public Jav321ActorCrawler(string Id) : base(Id)
        {
            Url = JvedioServers.Jav321.Url + $"search";
        }

        protected void InitHeaders(string postdata)
        {
            //sn=pppd-093
            if (!Url.IsProperUrl()) return;
            Uri uri = new Uri(Url);
            headers = new CrawlerHeader()
            {

                ContentLength = postdata.Length + 3,
                Origin = uri.Scheme + "://" + uri.Host,
                ContentType = "application/x-www-form-urlencoded",
                Referer = uri.Scheme + "://" + uri.Host,
                Method = "POST",
                Cookies = JvedioServers.Jav321.Cookie
            };
        }

        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() { Cookies = JvedioServers.Jav321.Cookie };
        }

        public async Task<string> GetMovieCode(Action<string> callback = null)
        {
            return null;
        }

        public override async Task<HttpResult> Crawl()
        {
            return null;
        }




        protected override Dictionary<string, string> GetInfo()
        {
            return null;
        }


        protected override void ParseCookies(string SetCookie)
        {
            if (SetCookie == null) return;
            List<string> Cookies = new List<string>();
            var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
            foreach (var item in values)
            {
                if (item.IndexOf('=') < 0) continue;
                string key = item.Split('=')[0];
                string value = item.Split('=')[1];
                if (key == "__cfduid" || key == "is_loyal") Cookies.Add(key + "=" + value);
            }
            string cookie = string.Join(";", Cookies);
            JvedioServers.Jav321.Cookie = cookie;
            JvedioServers.Save();
        }
    }



}
