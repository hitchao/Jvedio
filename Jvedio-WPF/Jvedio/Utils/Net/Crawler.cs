
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;
using System.IO;
using System.Net;
using Jvedio.Utils;
using Jvedio.Utils.Net;
using static Jvedio.Utils.Net.Net;

namespace Jvedio
{
    public abstract class Crawler
    {

        protected string Url = "";//网址
        protected CrawlerHeader headers = new CrawlerHeader();
        protected HttpResult httpResult = null;



        public string ID { get; set; }//必须给出视频 ID

        public Crawler(string Id)
        {
            ID = Id;

        }


        /// <summary>
        /// 初始化请求头
        /// </summary>
        protected abstract void InitHeaders();

        /// <summary>
        /// 表示如何解析 html 文件或者从网络传输中获取信息
        /// </summary>
        /// <returns>键值对</returns>
        protected abstract Dictionary<string, string> GetInfo();

        /// <summary>
        /// 持久化保存 Cookies
        /// </summary>
        /// <param name="SetCookie"></param>
        protected abstract void ParseCookies(string SetCookie);

        /// <summary>
        /// 爬取逻辑
        /// </summary>
        /// <returns>Http 结果</returns>
        public abstract Task<HttpResult> Crawl();

    }
    public class DouBanCrawler : Crawler
    {


        public VedioType VedioType { get; set; }
        public DouBanCrawler(string Id, VedioType vedioType) : base(Id)
        {
            VedioType = vedioType;
            if ((int)vedioType == 3)
            {
                Url = JvedioServers.BusEurope.Url + ID.Replace(".", "-");
            }
            else
            {
                Url = JvedioServers.Bus.Url + ID.ToUpper();
            }

        }

        public override async Task<HttpResult> Crawl()
        {
            if (Url.IsProperUrl()) InitHeaders();
            httpResult = await new MyNet().Http(Url, headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                FileProcess.SaveInfo(GetInfo(), ID, (int)VedioType);
                httpResult.Success = true;
                ParseCookies(httpResult.Headers.SetCookie);
                Movie movie = DataBase.SelectMovieByID(ID);
                //保存磁力
                List<Magnet> magnets = await new BusParse(ID, httpResult.SourceCode, VedioType).ParseMagnet(movie.bigimageurl);
                if (magnets.Count > 0)
                    DataBase.SaveMagnets(magnets);
            }
            return httpResult;
        }

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
            headers = new CrawlerHeader() { Cookies = (int)VedioType == 3 ? JvedioServers.BusEurope.Cookie : JvedioServers.Bus.Cookie };
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new BusParse(ID, httpResult.SourceCode, VedioType).Parse();
            if (Info.Count > 0)
            {
                Info.Add("sourceurl", Url);
                Info.Add("source", "javbus");
                Task.Delay(Delay.SHORT_3).Wait();
            }
            return Info;
        }

    }

    //https://www.imdb.com/title/tt5462900/?ref_=fn_al_tt_1
    public class IMDBBanCrawler : Crawler
    {


        public VedioType VedioType { get; set; }
        public IMDBBanCrawler(string Id, VedioType vedioType) : base(Id)
        {
            VedioType = vedioType;
            if ((int)vedioType == 3)
            {
                Url = JvedioServers.BusEurope.Url + ID.Replace(".", "-");
            }
            else
            {
                Url = JvedioServers.Bus.Url + ID.ToUpper();
            }

        }

        public override async Task<HttpResult> Crawl()
        {
            if (Url.IsProperUrl()) InitHeaders();
            httpResult = await new MyNet().Http(Url, headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                FileProcess.SaveInfo(GetInfo(), ID, (int)VedioType);
                httpResult.Success = true;
                ParseCookies(httpResult.Headers.SetCookie);
                Movie movie = DataBase.SelectMovieByID(ID);
                //保存磁力
                List<Magnet> magnets = await new BusParse(ID, httpResult.SourceCode, VedioType).ParseMagnet(movie.bigimageurl);
                if (magnets.Count > 0)
                    DataBase.SaveMagnets(magnets);
            }
            return httpResult;
        }

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
            headers = new CrawlerHeader() { Cookies = (int)VedioType == 3 ? JvedioServers.BusEurope.Cookie : JvedioServers.Bus.Cookie };
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new BusParse(ID, httpResult.SourceCode, VedioType).Parse();
            if (Info.Count > 0)
            {
                Info.Add("sourceurl", Url);
                Info.Add("source", "javbus");
                Task.Delay(Delay.SHORT_3).Wait();
            }
            return Info;
        }

    }



    public class BusCrawler : Crawler
    {


        public VedioType VedioType { get; set; }
        public BusCrawler(string Id, VedioType vedioType) : base(Id)
        {
            VedioType = vedioType;
            if (vedioType == VedioType.欧美)
            {
                Url = JvedioServers.BusEurope.Url + ID.Replace(".", "-");
            }
            else
            {
                Url = JvedioServers.Bus.Url + ID.ToUpper();
            }

        }

        public override async Task<HttpResult> Crawl()
        {
            if (Url.IsProperUrl()) InitHeaders();
            httpResult = await new MyNet().Http(Url, headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                FileProcess.SaveInfo(GetInfo(), ID, (int)VedioType);
                httpResult.Success = true;
                ParseCookies(httpResult.Headers.SetCookie);
                Movie movie = DataBase.SelectMovieByID(ID);
                //保存磁力
                List<Magnet> magnets = await new BusParse(ID, httpResult.SourceCode, VedioType).ParseMagnet(movie.bigimageurl);
                if (magnets.Count > 0)
                    DataBase.SaveMagnets(magnets);
            }
            return httpResult;
        }

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
            headers = new CrawlerHeader() { Cookies = VedioType == VedioType.欧美 ? JvedioServers.BusEurope.Cookie : JvedioServers.Bus.Cookie };
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new BusParse(ID, httpResult.SourceCode, VedioType).Parse();
            if (Info.Count > 0)
            {
                Info.Add("sourceurl", Url);
                Info.Add("source", "javbus");
                Task.Delay(Delay.SHORT_3).Wait();
            }
            return Info;
        }

    }

    public class FC2Crawler : Crawler
    {

        public FC2Crawler(string Id) : base(Id)
        {
            Url = $"{JvedioServers.FC2.Url}article/{ID.ToUpper().Replace("FC2-", "")}/";
        }


        protected override void InitHeaders()
        {
            headers = new CrawlerHeader()
            {
                Cookies = JvedioServers.FC2.Cookie
            };
        }


        //TODO 国际化
        public override async Task<HttpResult> Crawl()
        {
            if (Url.IsProperUrl()) InitHeaders();
            httpResult = await new MyNet().Http(Url, headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                if (httpResult.SourceCode.IndexOf("非常抱歉，此商品未在您的居住国家公开") < 0 && httpResult.SourceCode.IndexOf("非常抱歉，找不到您要的商品") < 0)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                }
                else if (httpResult.SourceCode.IndexOf("非常抱歉，此商品未在您的居住国家公开") > 0)
                {
                    httpResult.StatusCode = HttpStatusCode.Forbidden;
                    httpResult.Success = false;
                }
                else if (httpResult.SourceCode.IndexOf("非常抱歉，找不到您要的商品") > 0)
                {
                    httpResult.StatusCode = HttpStatusCode.NotFound;
                    httpResult.Success = false;
                }
                ParseCookies(httpResult.Headers.SetCookie);
            }
            return httpResult;
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new FC2Parse(ID, httpResult.SourceCode).Parse();
            if (Info.Count <= 0)
            {
                Logger.LogN($"{Jvedio.Language.Resources.Url}：{Url}");
            }
            else
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "fc2adult");
                Task.Delay(Delay.SHORT_3).Wait();
            }
            return Info;
        }

        protected override void ParseCookies(string SetCookie)
        {
            if (string.IsNullOrEmpty(SetCookie)) return;
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
    public class DBCrawler : Crawler
    {

        protected string MovieCode;
        public DBCrawler(string Id) : base(Id)
        {
            Url = JvedioServers.DB.Url + $"search?q={ID}&f=all";
            if (Url.IsProperUrl()) InitHeaders();
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
            //先从数据库获取
            string movieCode = DataBase.SelectInfoByID("code", "javdb", ID);
            if (movieCode == "")
            {
                //从网络获取
                HttpResult result = await new MyNet().Http(Url, headers, allowRedirect: false);
                if (result != null && result.StatusCode == HttpStatusCode.Redirect) callback?.Invoke(Jvedio.Language.Resources.SearchTooFrequent);
                if (result != null && result.SourceCode != "")
                    movieCode = GetMovieCodeFromSearchResult(result.SourceCode);

                //存入数据库
                if (movieCode != "")
                    DataBase.SaveMovieCodeByID(ID, "javdb", movieCode);



            }
            return movieCode;
        }

        protected string GetMovieCodeFromSearchResult(string content)
        {
            string result = "";
            if (string.IsNullOrEmpty(content)) return result;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);

            HtmlNodeCollection gridNodes = doc.DocumentNode.SelectNodes("//div[@class='grid columns']/div/a/div[@class='uid']");
            if (gridNodes != null)
            {
                foreach (HtmlNode gridNode in gridNodes)
                {
                    if (gridNode.InnerText.ToUpper() == ID.ToUpper())
                    {
                        result = gridNode.ParentNode.Attributes["href"].Value.Replace("/v/", "");
                        break;
                    }
                }
            }
            return result;
        }



        public override async Task<HttpResult> Crawl()
        {
            Movie movie = DataBase.SelectMovieByID(ID);

            if (!string.IsNullOrEmpty(movie.sourceurl) && movie.sourceurl.IsProperUrl())
            {
                //如果有原网址，则不用搜索了
                MovieCode = movie.sourceurl.Split('/').Last();
            }
            else
            {
                MovieCode = await GetMovieCode((error) =>
                {
                    httpResult = new HttpResult() { Error = error, Success = false };
                });
            }


            if (MovieCode != "")
            {
                Url = JvedioServers.DB.Url + $"v/{MovieCode}";
                httpResult = await new MyNet().Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                    //保存磁力
                    List<Magnet> magnets = new JavDBParse(ID, httpResult.SourceCode, MovieCode).ParseMagnet();
                    if (magnets.Count > 0)
                        DataBase.SaveMagnets(magnets);


                }
            }
            return httpResult;

        }

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new JavDBParse(ID, httpResult.SourceCode, MovieCode).Parse();
            if (Info.Count > 0)
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "javdb");
                Task.Delay(Delay.MEDIUM).Wait();
            }
            return Info;
        }


    }

    public class LibraryCrawler : Crawler
    {
        protected string MovieCode;
        public LibraryCrawler(string Id) : base(Id)
        {
            Url = JvedioServers.Library.Url + $"vl_searchbyid.php?keyword={ID}";
            if (Url.IsProperUrl()) InitHeaders();
        }

        protected async Task<string> GetMovieCode()
        {
            string movieCode = DataBase.SelectInfoByID("code", "library", ID);
            //先从数据库获取
            if (string.IsNullOrEmpty(movieCode) || movieCode.IndexOf("zh-cn") >= 0)
            {
                HttpResult result = await new MyNet().Http(Url, headers, Mode: HttpMode.RedirectGet);
                if (result != null && result.StatusCode == HttpStatusCode.Redirect) movieCode = result.Headers.Location;
                else if (result != null) movieCode = GetMovieCodeFromSearchResult(result.SourceCode);

                if (movieCode.IndexOf("=") >= 0) movieCode = movieCode.Split('=').Last();
            }

            //存入数据库
            if (movieCode != "" && movieCode.IndexOf("zh-cn") < 0)
            {
                DataBase.SaveMovieCodeByID(ID, "library", movieCode);
            }

            return movieCode;
        }

        private string GetMovieCodeFromSearchResult(string html)
        {
            string result = "";
            if (string.IsNullOrEmpty(html)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='id']");
            if (nodes != null)
            {
                HtmlNode linknode = null;
                string id = "";
                foreach (HtmlNode node in nodes)
                {
                    if (node == null) continue;
                    id = node.InnerText;
                    if (!string.IsNullOrEmpty(id) && id.ToUpper() == ID.ToUpper())
                    {
                        linknode = node.ParentNode;
                        if (linknode != null)
                        {
                            string link = linknode.Attributes["href"]?.Value;
                            if (link.IndexOf("=") > 0) result = link.Split('=').Last();
                        }
                        break;

                    }
                }
            }


            return result;

        }

        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() { Cookies = JvedioServers.Library.Cookie };
        }


        public override async Task<HttpResult> Crawl()
        {
            MovieCode = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = JvedioServers.Library.Url + $"?v={MovieCode}";
                httpResult = await new MyNet().Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                    ParseCookies(httpResult.Headers.SetCookie);
                }
            }
            return httpResult;
        }

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
                if (key == "__cfduid" || key == "__qca") Cookies.Add(key + "=" + value);
            }
            Cookies.Add("over18=18");
            string cookie = string.Join(";", Cookies);
            JvedioServers.Library.Cookie = cookie;
            JvedioServers.Save();
        }

        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new LibraryParse(ID, httpResult.SourceCode).Parse();
            if (Info.Count > 0)
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "javlibrary");
                Task.Delay(Delay.MEDIUM).Wait();
            }
            return Info;
        }


    }

    public class FANZACrawler : Crawler
    {

        protected string MovieCode = "";
        protected bool OnlyPlot;//是否仅爬取摘要
        public FANZACrawler(string Id, bool onlyPlot = false) : base(Id)
        {
            Url = $"{JvedioServers.DMM.Url}search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr={ID}&commit.x=5&commit.y=18";
            if (Url.IsProperUrl()) InitHeaders();
            OnlyPlot = onlyPlot;
        }

        protected async Task<string> GetMovieCode()
        {
            //从网络获取
            string movieCode = "";
            string link = "";
            HttpResult result = await new MyNet().Http(Url, headers, Mode: HttpMode.RedirectGet);
            if (result != null && result.StatusCode == HttpStatusCode.Redirect)
                link = result.Headers.Location;//https://www.dmm.co.jp/mono/dvd/-/search/=/searchstr=ABP-123/

            if (!string.IsNullOrEmpty(link))
            {
                HttpResult newResult = await new MyNet().Http(link, headers);
                if (newResult != null && newResult.StatusCode == HttpStatusCode.OK)
                {
                    if (newResult.SourceCode.IndexOf("に一致する商品は見つかりませんでした") > 0)
                    {
                        //不存在
                    }
                    else
                    {
                        //存在并解析
                        movieCode = GetLinkFromSearchResult(newResult.SourceCode);
                    }
                }

            }
            return movieCode;
        }

        private string GetLinkFromSearchResult(string html)
        {
            string result = "";
            if (string.IsNullOrEmpty(html)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//p[@class='tmb']/a");

            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    if (node == null) continue;
                    string link = node.Attributes["href"]?.Value;
                    if (link.IsProperUrl())
                    {
                        string fanhao = Identify.GetFanhaoFromDMMUrl(link);
                        if (Identify.GetEng(fanhao).ToUpper() == Identify.GetEng(ID).ToUpper())
                        {
                            string str1 = Identify.GetNum(fanhao);
                            string str2 = Identify.GetNum(ID);
                            int num1 = 0;
                            int num2 = 1;
                            int.TryParse(str1, out num1);
                            int.TryParse(str2, out num2);
                            if (num1 == num2)
                            {
                                result = link;
                                break;
                            }
                        }

                    }
                }
            }


            return result;

        }




        public override async Task<HttpResult> Crawl()
        {
            MovieCode = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = MovieCode;
                httpResult = await new MyNet().Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    if (!this.OnlyPlot)
                        FileProcess.SaveInfo(GetInfo(), ID);
                    else
                        FileProcess.SavePartialInfo(GetInfo(), "plot", ID);
                    httpResult.Success = true;
                }
            }
            return httpResult;
        }

        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new Dictionary<string, string>();
            Info = new FanzaParse(ID, httpResult.SourceCode).Parse();
            if (Info.Count > 0)
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "FANZA");
                Task.Delay(Delay.MEDIUM).Wait();
            }
            return Info;
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


    public class MOOCrawler : Crawler
    {

        protected string MovieCode;
        public MOOCrawler(string Id) : base(Id)
        {
            Url = JvedioServers.MOO.Url + $"search/{ID}";
            if (Url.IsProperUrl()) InitHeaders();
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

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);

            HtmlNodeCollection gridNodes = doc.DocumentNode.SelectNodes("//a[@class='movie-box']");
            if (gridNodes != null)
            {
                foreach (HtmlNode gridNode in gridNodes)
                {
                    HtmlNode htmlNode = gridNode.SelectSingleNode("div/span/date");
                    if (htmlNode != null && htmlNode.InnerText.ToUpper() == ID.ToUpper())
                    {
                        string link = gridNode.Attributes["href"]?.Value;
                        if (!string.IsNullOrEmpty(link) && link.IndexOf("/") > 0) return link.Split('/').Last();

                    }
                }
            }
            return "";
        }



        public override async Task<HttpResult> Crawl()
        {
            MovieCode = await GetMovieCode((error) =>
            {
                httpResult = new HttpResult() { Error = error, Success = false };
            });
            if (MovieCode != "")
            {
                Url = JvedioServers.MOO.Url + $"movie/{MovieCode}";
                httpResult = await new MyNet().Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                }
            }
            return httpResult;

        }

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new MOOParse(ID, httpResult.SourceCode).Parse();
            if (Info.Count > 0)
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "AVMOO");
                Task.Delay(Delay.MEDIUM).Wait();
            }
            return Info;
        }


    }

    public class Jav321Crawler : Crawler
    {
        protected string MovieCode = "";
        public Jav321Crawler(string Id) : base(Id)
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


        //TODO
        public async Task<string> GetMovieCode(Action<string> callback = null)
        {
            //从网络获取
            InitHeaders(ID);
            HttpResult result = await new MyNet().Http(Url, headers, allowRedirect: false, postString: $"sn={ID}");
            if (result != null && result.StatusCode == HttpStatusCode.MovedPermanently && !string.IsNullOrEmpty(result.Headers.Location))
            {
                return result.Headers.Location;
            }
            //未找到

            //搜索太频繁

            return "";
        }

        public override async Task<HttpResult> Crawl()
        {
            //从网络获取

            MovieCode = await GetMovieCode((error) =>
            {
                httpResult = new HttpResult() { Error = error, Success = false };
            });
            if (MovieCode.Length > 1)
            {
                InitHeaders();
                Url = JvedioServers.Jav321.Url + MovieCode.Substring(1);
                httpResult = await new MyNet().Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                    ParseCookies(httpResult.Headers.SetCookie);
                }
            }
            return httpResult;
        }




        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new Jav321Parse(ID, httpResult.SourceCode).Parse();
            if (Info.Count > 0)
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "JAV321");
                Task.Delay(Delay.MEDIUM).Wait();
            }
            return Info;
        }


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
                if (key == "__cfduid" || key == "is_loyal") Cookies.Add(key + "=" + value);
            }
            string cookie = string.Join(";", Cookies);
            JvedioServers.Jav321.Cookie = cookie;
            JvedioServers.Save();
        }
    }



}
