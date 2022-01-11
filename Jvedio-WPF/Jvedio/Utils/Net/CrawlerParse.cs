
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Jvedio.Utils.Net;

namespace Jvedio
{

    //TODO 封装成 DLL
    public abstract class InfoParse
    {
        protected string HtmlText { get; set; }
        public string ID { get; set; }
        protected VedioType VedioType { get; set; }

        public InfoParse(string htmlText, string id = "", VedioType vedioType = VedioType.步兵)
        {
            ID = id;
            HtmlText = htmlText;
            VedioType = vedioType;
        }

        public InfoParse()
        {

        }


        public abstract Dictionary<string, string> Parse();

    }
    public class DouBanParse : InfoParse
    {
        public DouBanParse(string id, string htmlText, VedioType vedioType) : base(htmlText, id, vedioType) { }


        public DouBanParse()
        {

        }

        public override Dictionary<string, string> Parse()
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(HtmlText)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);
            //基本信息
            HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//span[@class='header']");
            if (headerNodes != null)
            {
                foreach (HtmlNode headerNode in headerNodes)
                {
                    if (headerNode == null) continue;
                    string headerText = headerNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNode linkNode = null;
                    switch (headerText)
                    {
                        case "發行日期:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("releasedate", Regex.Match(content, "[0-9]{4}-[0-9]{2}-[0-9]{2}").Value);
                            result.Add("year", Regex.Match(content, "[0-9]{4}").Value);
                            break;
                        case "長度:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("runtime", Regex.Match(content, "[0-9]+").Value);
                            break;
                        case "製作商:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("studio", content);
                            break;
                        case "系列:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("tag", content);
                            break;
                        case "導演:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("director", content);
                            break;
                        default:
                            break;
                    }
                }
            }

            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h3");
            if (titleNodes != null && titleNodes.Count > 0)
            {
                if (VedioType == VedioType.欧美)
                    result.Add("title", titleNodes[0].InnerText.Replace(ID, ""));
                else
                {
                    string title = titleNodes[0].InnerText.ToUpper().Replace(ID.ToUpper(), "");
                    if (title.StartsWith(" "))
                        result.Add("title", title.Substring(1));
                    else
                        result.Add("title", title);
                }

            }


            //类别、演员

            List<string> actors = new List<string>();
            List<string> actorsid = new List<string>();
            HtmlNodeCollection actorsNodes = doc.DocumentNode.SelectNodes("//span[@class='genre']/a");
            if (actorsNodes != null)
            {
                foreach (HtmlNode actorsNode in actorsNodes)
                {
                    if (actorsNode == null) continue;
                    HtmlNode node = actorsNode.ParentNode; if (node == null) continue;

                    if (node.Attributes["onmouseover"] != null)
                    {
                        actors.Add(actorsNode.InnerText);//演员
                        string link = actorsNode.Attributes["href"]?.Value;
                        if (!string.IsNullOrEmpty(link) && link.IndexOf("/") >= 0)
                            actorsid.Add(link.Split('/').Last());
                    }
                }

            }
            List<string> genres = new List<string>();
            HtmlNodeCollection genreNodes = doc.DocumentNode.SelectNodes("//span[@class='genre']/label/a");
            if (genreNodes != null)
            {
                foreach (HtmlNode genreNode in genreNodes)
                {
                    genres.Add(genreNode.InnerText);
                }
            }
            result.Add("genre", string.Join(" ", genres));

            if (actors.Count > 0 && actorsid.Count > 0)
            {
                result.Add("actor", string.Join("/", actors));
                result.Add("actorid", string.Join("/", actorsid));
                List<string> url_a = new List<string>();//演员头像地址
                foreach (var item in actorsid)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    if (VedioType == VedioType.骑兵)
                        url_a.Add($"{JvedioServers.Bus.Url}pics/actress/{item}_a.jpg");
                    else if (VedioType == VedioType.欧美)
                        url_a.Add(JvedioServers.BusEurope.Url.Replace("www", "images") + "actress/" + item + "_a.jpg");//https://images.javbus.one/actress/41r_a.jpg
                    else if (VedioType == VedioType.步兵)
                        url_a.Add($"{JvedioServers.Bus.Url}imgs/actress/{item}.jpg");
                }
                result.Add("actressimageurl", string.Join(";", url_a));
            }

            //大图
            string movieid = ""; string bigimageurl = "";
            HtmlNodeCollection bigimgeNodes = doc.DocumentNode.SelectNodes("//a[@class='bigImage']");
            if (bigimgeNodes != null && bigimgeNodes.Count > 0)
            {
                bigimageurl = bigimgeNodes[0].Attributes["href"]?.Value;
                if (!string.IsNullOrEmpty(bigimageurl))
                {
                    if (bigimageurl.IndexOf("http") < 0) bigimageurl = JvedioServers.Bus.Url + bigimageurl.Substring(1);
                    result.Add("bigimageurl", bigimageurl);
                    // => /pics/cover/89co_b.jpg
                    movieid = bigimageurl.Split('/').Last().Split('.').First().Replace("_b", "");
                }


            }

            //小图
            if (!string.IsNullOrEmpty(bigimageurl))
            {
                if (bigimageurl.IndexOf("pics.dmm.co.jp") >= 0)
                    result.Add("smallimageurl", bigimageurl.Replace("pl.jpg", "ps.jpg"));
                else if (!string.IsNullOrEmpty(movieid))
                {
                    if ((int)VedioType == 2)
                        result.Add("smallimageurl", $"{JvedioServers.Bus.Url}pics/thumb/{movieid}.jpg");
                    else if ((int)VedioType == 1)
                        result.Add("smallimageurl", $"{JvedioServers.Bus.Url}imgs/thumbs/{movieid}.jpg");
                    else if ((int)VedioType == 3)
                        result.Add("smallimageurl", $"{JvedioServers.BusEurope.Url}thumb/" + movieid + ".jpg");
                }
            }

            //预览图
            List<string> url_e = new List<string>();
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//a[@class='sample-box']");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    url_e.Add(extrapicNode.Attributes["href"].Value);
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }
            return result;
        }





        public async Task<List<Magnet>> ParseMagnet(string bigimage)
        {
            List<Magnet> result = new List<Magnet>();
            if (string.IsNullOrEmpty(HtmlText)) return result;


            //通过正则获得 gid
            Regex gidRex = new Regex(@"var gid = \d+");
            Regex ucRex = new Regex(@"var uc = \d+");
            var gidMatch = gidRex.Match(HtmlText);
            var ucMatch = ucRex.Match(HtmlText);
            if (gidMatch != null && gidMatch.Length > 0 && ucMatch != null && ucMatch.Length > 0)
            {
                string gid = gidMatch.Value.Replace("var gid = ", "");
                string uc = ucMatch.Value.Replace("var uc = ", "");
                string url = $"{JvedioServers.Bus.Url}ajax/uncledatoolsbyajax.php?gid={gid}&lang=zh&img={bigimage}&uc={uc}";

                HttpResult httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
                if (httpResult != null && httpResult.SourceCode != "")
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(httpResult.SourceCode);

                    //基本信息
                    HtmlNodeCollection magnetNodes = doc.DocumentNode.SelectNodes("//tr");
                    foreach (var magnetNode in magnetNodes)
                    {

                        HtmlNodeCollection tdNodes = magnetNode.SelectNodes("td");

                        if (tdNodes != null && tdNodes.Count == 3)
                        {
                            Magnet magnet = new Magnet(ID);
                            //名称和 tag
                            HtmlNodeCollection linkNodes = tdNodes[0].SelectNodes("a");
                            magnet.title = linkNodes[0].InnerText.CleanSqlString();
                            magnet.link = linkNodes[0].Attributes["href"]?.Value;

                            for (int i = 1; i < linkNodes.Count; i++)
                            {
                                magnet.tag.Add(linkNodes[i].InnerText);
                            }

                            //大小
                            string size = tdNodes[1].SelectSingleNode("a").InnerText.CleanSqlString();
                            double filesize = 0;
                            if (size.EndsWith("GB"))
                            {
                                size = size.Replace("GB", "");
                                double.TryParse(size, out filesize);
                                filesize = filesize * 1024;//转为 MB
                            }
                            else if (size.EndsWith("MB"))
                            {
                                size = size.Replace("MB", "");
                                double.TryParse(size, out filesize);
                            }
                            magnet.size = filesize;
                            //发行日期
                            magnet.releasedate = tdNodes[2].SelectSingleNode("a").InnerText.CleanSqlString();
                            if (magnet.link.IndexOf("&") > 0) magnet.link = magnet.link.Split('&')[0];
                            result.Add(magnet);
                        }
                    }
                }
            }
            return result;

        }



        public static List<Movie> GetMoviesFromPage(string sourceCode)
        {
            List<Movie> result = new List<Movie>();
            if (string.IsNullOrEmpty(sourceCode)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sourceCode);
            HtmlNodeCollection movieNodes = doc.DocumentNode.SelectNodes("//a[@class='movie-box']");
            if (movieNodes == null) return result;
            foreach (HtmlNode movieNode in movieNodes)
            {
                Movie movie = new Movie();
                if (movieNode.Attributes["href"]?.Value != "") movie.sourceurl = movieNode.Attributes["href"].Value;

                HtmlNode smallimage = movieNode.SelectSingleNode("div/img");
                HtmlNodeCollection htmlNodes = movieNode.SelectNodes("div/span/date");
                HtmlNode title = movieNode.SelectSingleNode("div/span");
                HtmlNodeCollection itemtags = movieNode.SelectNodes("div/span/div/button");
                List<string> tags = new List<string>();
                //标签
                if (itemtags != null && itemtags.Count > 0)
                {
                    foreach (var item in itemtags)
                    {
                        tags.Add(item.InnerText);
                    }
                }
                string titletext = "";
                if (smallimage != null && smallimage.Attributes["src"]?.Value != "") movie.smallimageurl = smallimage.Attributes["src"].Value;
                if (title != null && title.InnerText != "") titletext = title.InnerText.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
                //id 和发行日期
                if (htmlNodes != null && htmlNodes.Count == 2)
                {
                    movie.id = htmlNodes[0].InnerText;
                    movie.releasedate = htmlNodes[1].InnerText;
                }

                foreach (var item in tags)
                {
                    titletext = titletext.Replace(item, "").Replace($"{movie.id}/{movie.releasedate}", "");
                }
                movie.title = titletext.Replace("'", "");

                if (!string.IsNullOrEmpty(movie.id))
                    result.Add(movie);
            }


            return result;
        }

    }

    public class BusParse : InfoParse
    {
        public BusParse(string id, string htmlText, VedioType vedioType) : base(htmlText, id, vedioType) { }


        public BusParse()
        {

        }

        public override Dictionary<string, string> Parse()
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(HtmlText)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);
            //基本信息
            HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//span[@class='header']");
            if (headerNodes != null)
            {
                foreach (HtmlNode headerNode in headerNodes)
                {
                    if (headerNode == null) continue;
                    string headerText = headerNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNode linkNode = null;
                    switch (headerText)
                    {
                        case "發行日期:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("releasedate", Regex.Match(content, "[0-9]{4}-[0-9]{2}-[0-9]{2}").Value);
                            result.Add("year", Regex.Match(content, "[0-9]{4}").Value);
                            break;
                        case "長度:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("runtime", Regex.Match(content, "[0-9]+").Value);
                            break;
                        case "製作商:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("studio", content);
                            break;
                        case "系列:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("tag", content);
                            break;
                        case "導演:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("director", content);
                            break;
                        default:
                            break;
                    }
                }
            }

            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h3");
            if (titleNodes != null && titleNodes.Count > 0)
            {
                if (VedioType == VedioType.欧美)
                    result.Add("title", titleNodes[0].InnerText.Replace(ID, ""));
                else
                {
                    string title = titleNodes[0].InnerText.ToUpper().Replace(ID.ToUpper(), "");
                    if (title.StartsWith(" "))
                        result.Add("title", title.Substring(1));
                    else
                        result.Add("title", title);
                }

            }


            //类别、演员

            List<string> actors = new List<string>();
            List<string> actorsid = new List<string>();
            HtmlNodeCollection actorsNodes = doc.DocumentNode.SelectNodes("//span[@class='genre']/a");
            if (actorsNodes != null)
            {
                foreach (HtmlNode actorsNode in actorsNodes)
                {
                    if (actorsNode == null) continue;
                    HtmlNode node = actorsNode.ParentNode; if (node == null) continue;

                    if (node.Attributes["onmouseover"] != null)
                    {
                        actors.Add(actorsNode.InnerText);//演员
                        string link = actorsNode.Attributes["href"]?.Value;
                        if (!string.IsNullOrEmpty(link) && link.IndexOf("/") >= 0)
                            actorsid.Add(link.Split('/').Last());
                    }
                }

            }
            List<string> genres = new List<string>();
            HtmlNodeCollection genreNodes = doc.DocumentNode.SelectNodes("//span[@class='genre']/label/a");
            if (genreNodes != null)
            {
                foreach (HtmlNode genreNode in genreNodes)
                {
                    genres.Add(genreNode.InnerText);
                }
            }
            result.Add("genre", string.Join(" ", genres));

            if (actors.Count > 0 && actorsid.Count > 0)
            {
                result.Add("actor", string.Join("/", actors));
                result.Add("actorid", string.Join("/", actorsid));
                List<string> url_a = new List<string>();//演员头像地址
                foreach (var item in actorsid)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    if (VedioType == VedioType.骑兵)
                        url_a.Add($"{JvedioServers.Bus.Url}pics/actress/{item}_a.jpg");
                    else if (VedioType == VedioType.欧美)
                        url_a.Add(JvedioServers.BusEurope.Url.Replace("www", "images") + "actress/" + item + "_a.jpg");//https://images.javbus.one/actress/41r_a.jpg
                    else if (VedioType == VedioType.步兵)
                        url_a.Add($"{JvedioServers.Bus.Url}imgs/actress/{item}.jpg");//步兵没有 _a
                }
                result.Add("actressimageurl", string.Join(";", url_a));
            }

            //大图
            string movieid = ""; string bigimageurl = "";
            HtmlNodeCollection bigimgeNodes = doc.DocumentNode.SelectNodes("//a[@class='bigImage']");
            if (bigimgeNodes != null && bigimgeNodes.Count > 0)
            {
                bigimageurl = bigimgeNodes[0].Attributes["href"]?.Value;
                if (!string.IsNullOrEmpty(bigimageurl))
                {
                    if (bigimageurl.IndexOf("http") < 0) bigimageurl = JvedioServers.Bus.Url + bigimageurl.Substring(1);
                    result.Add("bigimageurl", bigimageurl);
                    // => /pics/cover/89co_b.jpg
                    movieid = bigimageurl.Split('/').Last().Split('.').First().Replace("_b", "");
                }


            }

            //小图
            if (!string.IsNullOrEmpty(bigimageurl))
            {
                if (bigimageurl.IndexOf("pics.dmm.co.jp") >= 0)
                    result.Add("smallimageurl", bigimageurl.Replace("pl.jpg", "ps.jpg"));
                else if (!string.IsNullOrEmpty(movieid))
                {
                    if (VedioType == VedioType.骑兵)
                        result.Add("smallimageurl", $"{JvedioServers.Bus.Url}pics/thumb/{movieid}.jpg");
                    else if (VedioType == VedioType.步兵)
                        result.Add("smallimageurl", $"{JvedioServers.Bus.Url}imgs/thumbs/{movieid}.jpg");
                    else if (VedioType == VedioType.欧美)
                        result.Add("smallimageurl", $"{JvedioServers.BusEurope.Url}thumb/" + movieid + ".jpg");
                }
            }

            //预览图
            List<string> url_e = new List<string>();
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//a[@class='sample-box']");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    url_e.Add(extrapicNode.Attributes["href"].Value);
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }
            return result;
        }


        public Actress ParseActress()
        {
            if (string.IsNullOrEmpty(HtmlText)) return null;
            Actress result = new Actress();

            string info;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //基本信息
            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//div[@class='photo-info']/p");
            if (infoNodes != null)
            {
                foreach (HtmlNode infoNode in infoNodes)
                {
                    try
                    {
                        info = infoNode.InnerText;
                        if (info.IndexOf("生日") >= 0)
                        {
                            result.birthday = info.Replace("生日: ", "");
                        }
                        else if (info.IndexOf("年齡") >= 0)
                        {
                            int.TryParse(info.Replace("年齡: ", ""), out int age);
                            result.age = age;
                        }
                        else if (info.IndexOf("身高") >= 0)
                        {
                            int h = 0;
                            if (Regex.Match(info, @"[0-9]+") != null)
                                int.TryParse(Regex.Match(info, @"[0-9]+").Value, out h);
                            result.height = h;
                        }
                        else if (info.IndexOf("罩杯") >= 0)
                        {
                            result.cup = info.Replace("罩杯: ", "");
                        }
                        else if (info.IndexOf("胸圍") >= 0)
                        {
                            result.chest = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("腰圍") >= 0)
                        {
                            result.waist = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("臀圍") >= 0)
                        {
                            result.hipline = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("愛好") >= 0)
                        {
                            result.hobby = info.Replace("愛好: ", "");
                        }
                        else if (info.IndexOf("出生地") >= 0)
                        {
                            result.birthplace = info.Replace("出生地: ", "");
                        }
                    }
                    catch { continue; }
                }
            }
            return result;
        }



        public async Task<List<Magnet>> ParseMagnet(string bigimage)
        {

            /*
             * 
             * <script>
	                var gid = 45244082959;
	                var uc = 0;
	                var img = 'https://pics.javcdn.net/cover/8110_b.jpg';
                </script>


            */
            List<Magnet> result = new List<Magnet>();
            if (string.IsNullOrEmpty(HtmlText)) return result;


            //通过正则获得 gid
            Regex gidRex = new Regex(@"var gid = \d+");
            Regex ucRex = new Regex(@"var uc = \d+");
            var gidMatch = gidRex.Match(HtmlText);
            var ucMatch = ucRex.Match(HtmlText);
            if (gidMatch != null && gidMatch.Length > 0 && ucMatch != null && ucMatch.Length > 0)
            {
                string gid = gidMatch.Value.Replace("var gid = ", "");
                string uc = ucMatch.Value.Replace("var uc = ", "");
                string url = $"{JvedioServers.Bus.Url}ajax/uncledatoolsbyajax.php?gid={gid}&lang=zh&img={bigimage}&uc={uc}";

                HttpResult httpResult = await new MyNet().Http(url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
                if (httpResult != null && httpResult.SourceCode != "")
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(httpResult.SourceCode);

                    //基本信息
                    HtmlNodeCollection magnetNodes = doc.DocumentNode.SelectNodes("//tr");
                    foreach (var magnetNode in magnetNodes)
                    {

                        HtmlNodeCollection tdNodes = magnetNode.SelectNodes("td");

                        if (tdNodes != null && tdNodes.Count == 3)
                        {
                            Magnet magnet = new Magnet(ID);
                            //名称和 tag
                            HtmlNodeCollection linkNodes = tdNodes[0].SelectNodes("a");
                            magnet.title = linkNodes[0].InnerText.CleanSqlString();
                            magnet.link = linkNodes[0].Attributes["href"]?.Value;

                            for (int i = 1; i < linkNodes.Count; i++)
                            {
                                magnet.tag.Add(linkNodes[i].InnerText);
                            }

                            //大小
                            string size = tdNodes[1].SelectSingleNode("a").InnerText.CleanSqlString();
                            double filesize = 0;
                            if (size.EndsWith("GB"))
                            {
                                size = size.Replace("GB", "");
                                double.TryParse(size, out filesize);
                                filesize = filesize * 1024;//转为 MB
                            }
                            else if (size.EndsWith("MB"))
                            {
                                size = size.Replace("MB", "");
                                double.TryParse(size, out filesize);
                            }
                            magnet.size = filesize;
                            //发行日期
                            magnet.releasedate = tdNodes[2].SelectSingleNode("a").InnerText.CleanSqlString();
                            if (magnet.link.IndexOf("&") > 0) magnet.link = magnet.link.Split('&')[0];
                            result.Add(magnet);
                        }
                    }
                }
            }
            return result;

        }



        public static List<Movie> GetMoviesFromPage(string sourceCode)
        {
            List<Movie> result = new List<Movie>();
            if (string.IsNullOrEmpty(sourceCode)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sourceCode);
            HtmlNodeCollection movieNodes = doc.DocumentNode.SelectNodes("//a[@class='movie-box']");
            if (movieNodes == null) return result;
            foreach (HtmlNode movieNode in movieNodes)
            {
                Movie movie = new Movie();
                if (movieNode.Attributes["href"]?.Value != "") movie.sourceurl = movieNode.Attributes["href"].Value;

                HtmlNode smallimage = movieNode.SelectSingleNode("div/img");
                HtmlNodeCollection htmlNodes = movieNode.SelectNodes("div/span/date");
                HtmlNode title = movieNode.SelectSingleNode("div/span");
                HtmlNodeCollection itemtags = movieNode.SelectNodes("div/span/div/button");
                List<string> tags = new List<string>();
                //标签
                if (itemtags != null && itemtags.Count > 0)
                {
                    foreach (var item in itemtags)
                    {
                        tags.Add(item.InnerText);
                    }
                }
                string titletext = "";
                if (smallimage != null && smallimage.Attributes["src"]?.Value != "") movie.smallimageurl = smallimage.Attributes["src"].Value;
                if (title != null && title.InnerText != "") titletext = title.InnerText.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
                //id 和发行日期
                if (htmlNodes != null && htmlNodes.Count == 2)
                {
                    movie.id = htmlNodes[0].InnerText;
                    movie.releasedate = htmlNodes[1].InnerText;
                }

                foreach (var item in tags)
                {
                    titletext = titletext.Replace(item, "").Replace($"{movie.id}/{movie.releasedate}", "");
                }
                movie.title = titletext.Replace("'", "");

                if (!string.IsNullOrEmpty(movie.id))
                    result.Add(movie);
            }


            return result;
        }

    }

    public class LibraryParse : InfoParse
    {
        public LibraryParse(string id, string htmlText, VedioType vedioType = 0) : base(htmlText, id, vedioType) { }

        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") return result;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);
            string id = "";
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h3[@class='post-title text']/a");
            if (titleNode != null)
            {
                id = titleNode.InnerText.Split(' ')[0].ToUpper();
                result.Add("title", titleNode.InnerText.ToUpper().Replace(id, "").Substring(1));
            }

            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//div[@id='video_info']/div/table/tr");
            if (infoNodes != null)
            {
                foreach (HtmlNode infoNode in infoNodes)
                {
                    if (infoNode == null) continue;
                    string header = infoNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNodeCollection nodes = null;
                    if (header.IndexOf("发行日期") >= 0)
                    {
                        nodes = infoNode.SelectNodes("td"); if (nodes == null || nodes.Count == 0) continue;
                        content = nodes[1].InnerText;
                        result.Add("releasedate", content);
                    }
                    else if (header.IndexOf("长度") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/span"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("runtime", content);
                    }
                    else if (header.IndexOf("导演") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/span/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("director", content);
                    }
                    else if (header.IndexOf("发行商") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/span/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("studio", content);
                    }

                    else if (header.IndexOf("使用者评价:") >= 0)
                    {
                        node = doc.DocumentNode.SelectSingleNode("//span[@class='score']");
                        if (node == null) continue;
                        content = node.InnerText;
                        Match match = Regex.Match(content, @"([0-9]|\.)+");
                        if (match == null) continue;
                        double.TryParse(match.Value, out double rating);
                        result.Add("rating", Math.Ceiling(rating * 10).ToString());
                    }
                    else if (header.IndexOf("类别") >= 0)
                    {
                        HtmlNodeCollection genreNodes = infoNode.SelectNodes("td/span/a");
                        if (genreNodes != null)
                        {
                            List<string> genres = new List<string>();
                            foreach (HtmlNode genreNode in genreNodes)
                            {
                                genres.Add(genreNode.InnerText);
                            }
                            result.Add("genre", string.Join(" ", genres));
                        }

                    }
                    else if (header.IndexOf("演员") >= 0)
                    {
                        HtmlNodeCollection actressNodes = infoNode.SelectNodes("td/span/span/a");
                        if (actressNodes != null)
                        {
                            List<string> actress = new List<string>();
                            foreach (HtmlNode actressNode in actressNodes)
                            {
                                actress.Add(actressNode.InnerText);
                            }
                            result.Add("actor", string.Join("/", actress));
                        }

                    }
                }
            }

            // library 小图地址与大图地址无规律
            HtmlNode bigimageNode = doc.DocumentNode.SelectSingleNode("//img[@id='video_jacket_img']");
            if (bigimageNode != null)
            {
                result.Add("bigimageurl", "http:" + bigimageNode.Attributes["src"].Value);
                result.Add("smallimageurl", result["bigimageurl"].Replace("pl.jpg", "ps.jpg")); //如果地址来自于dmm则替换，否则大小图一致
            }



            //预览图
            //标准预览图是：https://pics.dmm.co.jp/digital/video/1star00319/1star00319jp-17.jpg
            //小的预览图时：https://pics.dmm.co.jp/digital/video/1star00319/1star00319-17.jpg
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//div[@class='previewthumbs']/img");
            if (extrapicNodes != null)
            {
                List<string> extraImage = new List<string>();
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    string link = "https:" + extrapicNode.Attributes["src"].Value;
                    if (link.IsProperUrl())
                    {
                        string name = System.IO.Path.GetFileName(new Uri(link).LocalPath);
                        string path = link.Replace(name, "");
                        string[] names = name.Split('-');
                        if (names.Length >= 2)
                        {
                            if (names.First().EndsWith("jp"))
                                extraImage.Add(link);
                            else
                            {
                                names[0] = names[0] + "jp";
                                extraImage.Add(path + string.Join("-", names));
                            }

                        }
                    }

                }
                result.Add("extraimageurl", string.Join(";", extraImage));
            }

            return result;
        }

    }

    public class JavDBParse : InfoParse
    {
        protected string MovieCode { get; set; }

        public JavDBParse(string id, string htmlText, string movieCode) : base(htmlText)
        {
            ID = id;
            HtmlText = htmlText;
            MovieCode = movieCode;
        }

        public JavDBParse()
        {

        }





        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h2[@class='title is-4']/strong");
            if (titleNode != null)
            {
                result.Add("title", titleNode.InnerText.Replace(ID, " ").Substring(1));
            }

            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//nav[@class='panel movie-panel-info']/div");
            if (infoNodes != null)
            {

                foreach (HtmlNode infoNode in infoNodes)
                {
                    HtmlNode node = null;
                    string content = "";
                    if (infoNode == null) continue;
                    string headerText = infoNode.InnerText;
                    if (headerText.IndexOf("時間") >= 0 || headerText.IndexOf("日期") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("releasedate", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("時長") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A")
                        {
                            Match match = Regex.Match(content, "[0-9]+");
                            if (match != null) result.Add("runtime", match.Value);
                        }
                    }
                    else if (infoNode.InnerText.IndexOf("賣家") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span/a"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("director", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("評分") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A")
                        {
                            Match match = Regex.Match(content, @"([0-9]|\.)+分");
                            if (match != null)
                            {
                                string rating = match.Value.Replace("分", "");
                                double.TryParse(rating, out double rate);
                                result.Add("rating", Math.Ceiling(rate * 20).ToString());
                            }
                        }
                    }
                    else if (infoNode.InnerText.IndexOf("類別") >= 0)
                    {
                        HtmlNodeCollection genreNodes = infoNode.SelectNodes("span/a");
                        if (genreNodes != null && genreNodes.Count > 0)
                        {
                            List<string> genres = new List<string>();
                            foreach (HtmlNode genreNode in genreNodes)
                            {
                                if (genreNode != null)
                                    genres.Add(genreNode.InnerText);
                            }
                            result.Add("genre", string.Join(" ", genres));
                        }

                    }
                    else if (infoNode.InnerText.IndexOf("片商") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span/a"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("studio", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("系列") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span/a"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("tag", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("演員") >= 0)
                    {
                        HtmlNodeCollection actressNodes = infoNode.SelectNodes("span/a");
                        if (actressNodes != null)
                        {
                            List<string> actress = new List<string>();
                            foreach (HtmlNode actressNode in actressNodes)
                            {
                                if (actressNode != null)
                                    actress.Add(actressNode.InnerText);
                            }
                            result.Add("actor", string.Join("/", actress));
                        }
                    }
                }

            }
            //大小图
            HtmlNode bigimageNode = doc.DocumentNode.SelectSingleNode("//img[@class='video-cover']");
            if (bigimageNode != null) { result.Add("bigimageurl", bigimageNode.Attributes["src"].Value); }

            string smallimageurl = "https://jdbimgs.com/thumbs/" + MovieCode.ToLower().Substring(0, 2) + "/" + MovieCode + ".jpg";
            result.Add("smallimageurl", smallimageurl);

            //预览图
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//a[@class='tile-item']");
            if (extrapicNodes != null)
            {
                List<string> extraimage = new List<string>();
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    string link = "";
                    link = extrapicNode.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(link) && link.IndexOf("/v/") < 0)
                        extraimage.Add(link);
                }
                result.Add("extraimageurl", string.Join(";", extraimage));
            }

            return result;
        }


        public List<Magnet> ParseMagnet()
        {
            List<Magnet> result = new List<Magnet>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //基本信息
            HtmlNodeCollection magnetNodes = doc.DocumentNode.SelectNodes("//div[@id='magnets-content']/table/tr");
            if (magnetNodes == null) return result;
            foreach (var magnetNode in magnetNodes)
            {

                HtmlNodeCollection tdNodes = magnetNode.SelectNodes("td");


                if (tdNodes != null && tdNodes.Count == 3)
                {

                    Magnet magnet = new Magnet(ID);
                    //磁力
                    HtmlNode linkNode = tdNodes[0].SelectSingleNode("a");

                    magnet.link = linkNode.Attributes["href"]?.Value;
                    HtmlNodeCollection titleNodes = linkNode.SelectNodes("span");
                    magnet.title = titleNodes[0].InnerText;

                    List<string> taginfos = new List<string>();
                    for (int i = 1; i < titleNodes.Count; i++)
                    {
                        taginfos.Add(titleNodes[i].InnerText);
                    }

                    List<string> taglist = new List<string>();

                    //名称、 tag
                    foreach (var item in taginfos)
                    {
                        if (item.IndexOf("GB") > 0)
                        {
                            Regex regex = new Regex(@"\d+\.?\d+GB");
                            var size = regex.Match(item);
                            if (size.Success && size.Value.Length > 0)
                            {
                                double.TryParse(size.Value.Replace("GB", ""), out double filesize);
                                magnet.size = filesize * 1024;
                            }

                        }
                        else if (item.IndexOf("MB") > 0)
                        {
                            Regex regex = new Regex(@"\d+\.?\d+MB");
                            var size = regex.Match(item);
                            if (size.Success && size.Value.Length > 0)
                            {
                                double.TryParse(size.Value.Replace("MB", ""), out double filesize);
                                magnet.size = filesize;
                            }
                        }
                        else
                        {
                            magnet.tag.Add(item);
                        }
                    }



                    //发行日期
                    magnet.releasedate = tdNodes[1].SelectSingleNode("span").InnerText;
                    if (magnet.link.IndexOf("&") > 0) magnet.link = magnet.link.Split('&')[0];
                    result.Add(magnet);
                }
            }


            return result;

        }


        public static List<Movie> GetMoviesFromPage(string sourceCode)
        {
            List<Movie> result = new List<Movie>();
            if (string.IsNullOrEmpty(sourceCode)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sourceCode);
            HtmlNodeCollection movieNodes = doc.DocumentNode.SelectNodes("//div[@class='grid-item column']/a");
            if (movieNodes == null || movieNodes.Count == 0)
                movieNodes = doc.DocumentNode.SelectNodes("//div[@class='grid-item column horz-cover']/a");
            if (movieNodes == null || movieNodes.Count == 0) return result;
            foreach (HtmlNode movieNode in movieNodes)
            {
                Movie movie = new Movie();
                if (movieNode.Attributes["href"]?.Value != "") movie.sourceurl = JvedioServers.DB.Url + movieNode.Attributes["href"].Value.Substring(1);

                HtmlNode id = movieNode.SelectSingleNode("div[@class='uid']");
                HtmlNode title = movieNode.SelectSingleNode("div[@class='video-title']");
                HtmlNode meta = movieNode.SelectSingleNode("div[@class='meta']");

                if (id != null && id.InnerText != "") movie.id = id.InnerText;
                if (title != null && title.InnerText != "") movie.title = title.InnerText;
                if (meta != null && meta.InnerText != "") movie.releasedate = meta.InnerText;

                if (!string.IsNullOrEmpty(movie.id))
                    result.Add(movie);
            }


            return result;
        }
    }

    public class Fc2ClubParse : InfoParse
    {
        public Fc2ClubParse(string id, string htmlText, VedioType vedioType = 0) : base(htmlText, id, vedioType) { }


        public override Dictionary<string, string> Parse()
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            string content; string title;
            //string id = "";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //基本信息
            HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//h5");
            if (headerNodes != null)
            {
                foreach (HtmlNode headerNode in headerNodes)
                {
                    try
                    {
                        title = headerNode.InnerText;
                        //Console.WriteLine(title);
                        if (title.IndexOf("影片评分") >= 0)
                        {
                            content = title;
                            result.Add("rating", Regex.Match(content, "[0-9]+").Value);
                        }
                        else if (title.IndexOf("资源参数") >= 0)
                        {
                            content = title;
                            if (content.IndexOf("无码") > 0) { result.Add("vediotype", "1"); }
                            else { result.Add("vediotype", "2"); }
                        }
                        else if (title.IndexOf("卖家信息") >= 0)
                        {
                            content = headerNode.SelectSingleNode("a").InnerText;
                            result.Add("director", content.Replace("\n", "").Replace("\r", ""));
                            result.Add("studio", content.Replace("\n", "").Replace("\r", ""));
                        }
                        else if (title.IndexOf("影片标签") >= 0)
                        {
                            HtmlNodeCollection genreNodes = headerNode.SelectNodes("a");
                            if (genreNodes != null)
                            {
                                string genre = "";
                                foreach (HtmlNode genreNode in genreNodes)
                                {
                                    genre = genre + genreNode.InnerText + " ";
                                }
                                if (genre.Length > 0) { result.Add("genre", genre.Substring(0, genre.Length - 1)); }
                            }

                        }
                        else if (title.IndexOf("女优名字") >= 0)
                        {
                            content = title;
                            result.Add("actor", content.Replace("女优名字：", "").Replace("\n", "").Replace("\r", "").Replace("/", " "));
                        }
                    }
                    catch { continue; }
                }
            }

            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h3");
            if (titleNodes != null)
            {
                foreach (HtmlNode titleNode in titleNodes)
                {
                    try
                    {
                        if (titleNode.InnerText.IndexOf("FC2优质资源推荐") < 0)
                        {
                            //id = titleNode.InnerText.Split(' ')[0];
                            //result.Add("id", id);
                            result.Add("title", titleNode.InnerText.Replace(ID, "").Substring(1).Replace("\n", "").Replace("\r", ""));
                            break;
                        }
                    }
                    catch { continue; }
                }

            }

            //预览图
            string url_e = "";
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//ul[@class='slides']/li/img");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    try
                    {
                        url_e = url_e + "https://fc2club.com" + extrapicNode.Attributes["src"].Value + ";";
                    }
                    catch { continue; }
                }
                result.Add("extraimageurl", url_e);
            }

            //大图小图
            if (url_e.IndexOf(';') > 0)
            {
                result.Add("bigimageurl", url_e.Split(';')[0]);
                result.Add("smallimageurl", url_e.Split(';')[0]);
            }

            //发行日期和发行年份
            if (url_e.IndexOf(";") > 0)
            {
                // / uploadfile / 2018 / 1213 / 20181213104511782.jpg
                string url = url_e.Split(';')[0];
                string datestring = Regex.Match(url, "[0-9]{4}/[0-9]{4}").Value;

                result.Add("releasedate", datestring.Substring(0, 4) + "-" + datestring.Substring(5, 2) + "-" + datestring.Substring(7, 2));
                result.Add("year", datestring.Substring(0, 4));
            }

            return result;
        }

    }


    public class FC2Parse : InfoParse
    {
        public FC2Parse(string id, string htmlText) : base(htmlText, id) { }


        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") return result;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);


            //发行商、评分
            Dictionary<string, string> studio_rating = GetUrlInfo(ref doc);
            foreach (var item in studio_rating)
            {
                if (!result.ContainsKey(item.Key))
                    result.Add(item.Key, item.Value);
            }



            //长度
            HtmlNodeCollection runtimeNodes = doc.DocumentNode.SelectNodes("//div[@class='items_article_MainitemThumb']/span/p");
            if (runtimeNodes != null && runtimeNodes.Count > 0)
            {
                string runtime = runtimeNodes[0].InnerText;
                runtime = RuntimeToMinute(runtime);
                result.Add("runtime", runtime);
            }

            //日期
            HtmlNodeCollection dateNodes = doc.DocumentNode.SelectNodes("//div[@class='items_article_Releasedate']/p");
            if (dateNodes != null && dateNodes.Count > 0)
            {
                string date = dateNodes[0].InnerText.Replace("上架时间 : ", "").Replace("販売日 : ", "").Replace("/", "-");
                result.Add("releasedate", date);
                result.Add("year", Regex.Match(date, "[0-9]{4}").Value);
            }


            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//section[@class='items_article_headerTitleInArea']/div[@class='items_article_headerInfo']/h3");
            if (titleNodes != null && titleNodes.Count > 0)
            {
                result.Add("title", titleNodes[0].InnerText);
            }


            //类别、演员
            List<string> genres = new List<string>();
            HtmlNodeCollection genreNodes = doc.DocumentNode.SelectNodes("//a[@class='tag tagTag']");
            if (genreNodes != null)
            {
                foreach (HtmlNode genreNode in genreNodes)
                {
                    if (genreNode != null)
                        genres.Add(genreNode.InnerText);

                }
                result.Add("genre", string.Join(" ", genres));
            }

            //大图
            string bigimageurl = "";
            HtmlNodeCollection bigimgeNodes = doc.DocumentNode.SelectNodes("//section[@class='items_article_headerTitleInArea']/div[@class='items_article_MainitemThumb']/span/img");
            if (bigimgeNodes != null && bigimgeNodes.Count > 0)
            {
                bigimageurl = bigimgeNodes[0].Attributes["src"]?.Value;
                if (!string.IsNullOrEmpty(bigimageurl))
                {
                    result.Add("bigimageurl", "https:" + bigimageurl.Replace("'", "\""));//下载的时候双引号替换为单引号
                    result.Add("smallimageurl", "https:" + bigimageurl.Replace("'", "\""));
                }
            }


            //预览图
            List<string> url_e = new List<string>();
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//ul[@class='items_article_SampleImagesArea']/li/a");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    url_e.Add(extrapicNode.Attributes["href"].Value);
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }
            return result;
        }

        private Dictionary<string, string> GetUrlInfo(ref HtmlDocument doc)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string studio = "";
            double rating = 0;
            HtmlNode node = null;
            HtmlNode studioNode = null;
            HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='items_article_headerInfo']/ul/li");
            if (htmlNodes != null && htmlNodes.Count > 0)
            {
                int num = htmlNodes.Count;
                switch (num)
                {
                    case 1:
                        node = htmlNodes[0].SelectSingleNode("//a");
                        if (node != null)
                            studio = node.InnerText;
                        break;

                    case 2:
                        node = htmlNodes[0].SelectSingleNode("//a[@class='items_article_Stars']");
                        if (node != null)
                        {
                            string spanClass = node.Attributes["class"].Value;
                            if (!string.IsNullOrEmpty(spanClass))
                            {
                                Match match = Regex.Match(spanClass, @"\d");
                                if (match != null)
                                    double.TryParse(match.Value, out rating);
                            }
                        }

                        studioNode = htmlNodes[1].SelectSingleNode("//a");
                        if (studioNode != null)
                            studio = studioNode.InnerText;

                        break;

                    case 3:
                        node = htmlNodes[1].SelectSingleNode("//a/p/span[1]");
                        if (node != null)
                        {
                            string spanClass = node.Attributes["class"].Value;
                            if (!string.IsNullOrEmpty(spanClass))
                            {
                                Match match = Regex.Match(spanClass, @"\d");
                                if (match != null)
                                    double.TryParse(match.Value, out rating);
                            }
                        }
                        studioNode = htmlNodes[2].SelectSingleNode("a");
                        if (studioNode != null)
                            studio = studioNode.InnerText;

                        break;


                    default:
                        break;
                }
            }
            rating *= 20;
            result.Add("rating", rating.ToString());
            result.Add("studio", studio);
            return result;

        }


        private string RuntimeToMinute(string Runtime)
        {
            int result = 0;
            if (Runtime.IndexOf(":") > 0)
            {
                List<string> runtimes = Runtime.Split(':').ToList();
                if (runtimes.Count == 3)
                    result = int.Parse(runtimes[0]) * 60 + int.Parse(runtimes[1]);
                else if (runtimes.Count == 2)
                {
                    if (runtimes[0] == "00")
                        result = 0;
                    else
                        result = int.Parse(runtimes[0]);
                }
            }
            return result.ToString();
        }

    }


    public class FanzaParse : InfoParse
    {
        public FanzaParse(string id, string htmlText, VedioType vedioType = 0) : base(htmlText, id, vedioType) { }

        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //标题
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h1[@id='title']");
            if (titleNode != null)
                result.Add("title", titleNode.InnerText);

            //摘要

            HtmlNode plotNode = doc.DocumentNode.SelectSingleNode("//div[@class='mg-b20 lh4']/p");
            if (plotNode != null)
                result.Add("plot", plotNode.InnerText.Replace("\n", ""));



            //基本信息
            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//table[@class='mg-b20']/tr");
            if (infoNodes != null)
            {
                foreach (HtmlNode infoNode in infoNodes)
                {
                    if (infoNode == null) continue;
                    string header = infoNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNodeCollection nodes = null;
                    if (header.IndexOf("貸出開始日") >= 0 || header.IndexOf("発売日") >= 0)
                    {
                        nodes = infoNode.SelectNodes("td"); if (nodes == null || nodes.Count == 0) continue;
                        content = nodes[1].InnerText;
                        result.Add("releasedate", content.Replace("/", "-"));
                    }
                    else if (header.IndexOf("収録時間") >= 0)
                    {
                        nodes = infoNode.SelectNodes("td"); if (nodes == null || nodes.Count == 0) continue;
                        content = nodes[1].InnerText;
                        int.TryParse(content.Replace("分", ""), out int runtime);
                        result.Add("runtime", runtime.ToString());
                    }
                    else if (header.IndexOf("監督") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("director", content);
                    }
                    else if (header.IndexOf("メーカー") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("studio", content);
                    }
                    else if (header.IndexOf("シリーズ") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("tag", content);
                    }
                    else if (header.IndexOf("平均評価") >= 0)
                    {
                        ////p.dmm.co.jp/p/ms/review/3.gif
                        node = doc.DocumentNode.SelectSingleNode("//img[@class='mg-r6 middle']");
                        if (node != null)
                        {
                            string src = node.Attributes["src"]?.Value;
                            if (!string.IsNullOrEmpty(src) && src.IndexOf(".gif") >= 0)
                            {
                                content = src.Split('/').Last().Split('.').First().Replace("_", ".");
                                //转为 10 分制
                                float.TryParse(content, out float rating);
                                result.Add("rating", (rating * 2).ToString());
                            }
                        }
                    }
                    else if (header.IndexOf("ジャンル") >= 0)
                    {
                        HtmlNodeCollection genreNodes = infoNode.SelectNodes("td/a");
                        if (genreNodes != null)
                        {
                            List<string> genres = new List<string>();
                            foreach (HtmlNode genreNode in genreNodes)
                            {
                                genres.Add(genreNode.InnerText);
                            }
                            result.Add("genre", string.Join(" ", genres));
                        }

                    }
                    else if (header.IndexOf("出演者") >= 0)
                    {
                        HtmlNodeCollection actressNodes = infoNode.SelectNodes("td/span/a");
                        if (actressNodes != null)
                        {
                            List<string> actress = new List<string>();
                            foreach (HtmlNode actressNode in actressNodes)
                            {
                                actress.Add(actressNode.InnerText);
                            }
                            result.Add("actor", string.Join("/", actress));
                        }

                    }
                }
            }

            //小图
            HtmlNode smallimageNode = doc.DocumentNode.SelectSingleNode("//div[@id='sample-video']/div/a/img");
            if (smallimageNode != null)
            {
                string src = smallimageNode.Attributes["src"]?.Value;
                if (!string.IsNullOrEmpty(src))
                    result.Add("smallimageurl", src);
            }
            //大图
            HtmlNode bigimageNode = doc.DocumentNode.SelectSingleNode("//div[@id='sample-video']/div/a");
            if (bigimageNode != null)
            {
                string src = bigimageNode.Attributes["href"]?.Value;
                if (!string.IsNullOrEmpty(src))
                    result.Add("bigimageurl", src);
            }

            //预览图
            //大：https://pics.dmm.co.jp/digital/video/mmus00032/mmus00032jp-4.jpg 
            //小：https://pics.dmm.co.jp/digital/video/mmus00032/mmus00032-4.jpg
            HtmlNodeCollection extraImageNodes = doc.DocumentNode.SelectNodes("//div[@id='sample-image-block']/a/img");
            List<string> url_e = new List<string>();
            if (extraImageNodes != null)
            {
                for (int i = 0; i < extraImageNodes.Count; i++)
                {
                    HtmlNode imgNode = extraImageNodes[i];
                    if (imgNode != null)
                    {
                        string src = imgNode.Attributes["src"]?.Value;
                        if (!string.IsNullOrEmpty(src) && src.IndexOf($"-{i + 1}.jpg") > 0)
                        {
                            url_e.Add(src.Replace($"-{i + 1}.jpg", $"jp-{i + 1}.jpg"));
                        }
                    }
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }

            return result;
        }
    }


    public class MOOParse : InfoParse
    {
        public MOOParse(string id, string htmlText) : base(htmlText, id) { }


        public override Dictionary<string, string> Parse()
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(HtmlText)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);
            string id = "";
            //基本信息
            HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//span[@class='header']");
            if (headerNodes != null)
            {
                foreach (HtmlNode headerNode in headerNodes)
                {
                    if (headerNode == null) continue;
                    string headerText = headerNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNode linkNode = null;
                    switch (headerText)
                    {
                        case "识别码:":
                            node = headerNode.ParentNode; if (node == null) break;
                            HtmlNodeCollection spanNodes = node.SelectNodes("span"); if (spanNodes == null || spanNodes.Count < 2) break;
                            id = spanNodes[1].InnerText;
                            break;
                        case "发行时间:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("releasedate", Regex.Match(content, "[0-9]{4}-[0-9]{2}-[0-9]{2}").Value);
                            result.Add("year", Regex.Match(content, "[0-9]{4}").Value);
                            break;
                        case "长度:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("runtime", Regex.Match(content, "[0-9]+").Value);
                            break;
                        case "导演:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("director", content);
                            break;
                        default:
                            break;
                    }
                }
            }

            //带连接的信息
            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//div[@class='col-md-3 info']/p");
            if (infoNodes != null)
            {
                for (int i = 0; i < infoNodes.Count; i++)
                {
                    string text = infoNodes[i].InnerText;
                    if (text.IndexOf("制作商") >= 0 && i + 1 < infoNodes.Count)
                    {
                        HtmlNode htmlNode = infoNodes[i + 1].SelectSingleNode("a");
                        if (htmlNode != null) result.Add("studio", htmlNode.InnerText);
                    }
                    else if (text.IndexOf("系列") >= 0 && i + 1 < infoNodes.Count)
                    {
                        HtmlNode htmlNode = infoNodes[i + 1].SelectSingleNode("a");
                        if (htmlNode != null) result.Add("tag", htmlNode.InnerText);
                    }
                }
            }


            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h3");
            if (titleNodes != null && titleNodes.Count > 0)
            {
                string title = titleNodes[0].InnerText.Replace(id, "");
                if (title.StartsWith(" "))
                    result.Add("title", title.Substring(1));
                else
                    result.Add("title", title);
            }


            //类别、演员
            List<string> genres = new List<string>();

            HtmlNodeCollection genreNodes = doc.DocumentNode.SelectNodes("//span[@class='genre']/a");
            if (genreNodes != null)
            {
                foreach (HtmlNode genreNode in genreNodes)
                {
                    if (genreNode == null) continue;
                    genres.Add(genreNode.InnerText);//类别
                }
                result.Add("genre", string.Join(" ", genres));
            }
            List<string> actors = new List<string>();
            List<string> url_a = new List<string>();//演员头像地址
            HtmlNodeCollection actorNodes = doc.DocumentNode.SelectNodes("//div[@id='avatar-waterfall']/a");
            if (actorNodes != null)
            {
                foreach (HtmlNode actorNode in actorNodes)
                {
                    HtmlNode nameNode = actorNode.SelectSingleNode("span");
                    HtmlNode imgNode = actorNode.SelectSingleNode("div/img");
                    if (nameNode != null) actors.Add(nameNode.InnerText);
                    if (imgNode != null)
                    {
                        string src = imgNode.Attributes["src"]?.Value;
                        if (!string.IsNullOrEmpty(src)) url_a.Add(src);
                    }

                }
                result.Add("actor", string.Join("/", actors));
                result.Add("actressimageurl", string.Join(";", url_a));
            }

            //大小图
            HtmlNodeCollection bigimgeNodes = doc.DocumentNode.SelectNodes("//a[@class='bigImage']");
            if (bigimgeNodes != null && bigimgeNodes.Count > 0)
            {
                string bigimageurl = bigimgeNodes[0].Attributes["href"]?.Value;
                if (!string.IsNullOrEmpty(bigimageurl))
                {
                    result.Add("bigimageurl", bigimageurl);
                    result.Add("smallimageurl", bigimageurl.Replace("pl.jpg", "ps.jpg"));
                }
            }

            //https://jp.netcdn.space/digital/video/nsps00978/nsps00978jp-1.jpg
            //https://jp.netcdn.space/digital/video/nsps00978/nsps00978-1.jpg

            //预览图
            List<string> url_e = new List<string>();
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//div[@id='sample-waterfall']/a/div/img");
            if (extrapicNodes != null)
            {
                for (int i = 0; i < extrapicNodes.Count; i++)
                {
                    string link = extrapicNodes[i].Attributes["src"]?.Value;
                    if (!string.IsNullOrEmpty(link) && link.IndexOf($"-{i + 1}.jpg") >= 0)
                    {
                        link = link.Replace($"-{i + 1}.jpg", $"jp-{i + 1}.jpg");
                        url_e.Add(link);
                    }
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }
            return result;
        }


        public Actress ParseActress()
        {
            if (string.IsNullOrEmpty(HtmlText)) return null;
            Actress result = new Actress();

            string info;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //基本信息
            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//div[@class='photo-info']/p");
            if (infoNodes != null)
            {
                foreach (HtmlNode infoNode in infoNodes)
                {
                    try
                    {
                        info = infoNode.InnerText;
                        if (info.IndexOf("生日") >= 0)
                        {
                            result.birthday = info.Replace("生日: ", "");
                        }
                        else if (info.IndexOf("年齡") >= 0)
                        {
                            int.TryParse(info.Replace("年齡: ", ""), out int age);
                            result.age = age;
                        }
                        else if (info.IndexOf("身高") >= 0)
                        {
                            int h = 0;
                            if (Regex.Match(info, @"[0-9]+") != null)
                                int.TryParse(Regex.Match(info, @"[0-9]+").Value, out h);
                            result.height = h;
                        }
                        else if (info.IndexOf("罩杯") >= 0)
                        {
                            result.cup = info.Replace("罩杯: ", "");
                        }
                        else if (info.IndexOf("胸圍") >= 0)
                        {
                            result.chest = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("腰圍") >= 0)
                        {
                            result.waist = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("臀圍") >= 0)
                        {
                            result.hipline = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("愛好") >= 0)
                        {
                            result.hobby = info.Replace("愛好: ", "");
                        }
                        else if (info.IndexOf("出生地") >= 0)
                        {
                            result.birthplace = info.Replace("出生地: ", "");
                        }
                    }
                    catch { continue; }
                }
            }
            return result;
        }


    }




    /// <summary>
    /// 傻逼JAV321，网页写的跟屎一样，类别也没有，预览图还加载的是原图，CNMD
    /// </summary>
    public class Jav321Parse : InfoParse
    {
        public Jav321Parse(string id, string htmlText) : base(htmlText, id) { }


        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //标题
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h3");
            HtmlNode smalltitleNode = doc.DocumentNode.SelectSingleNode("//h3/small");
            string title = "";
            if (titleNode != null && smalltitleNode != null)
                title = titleNode.InnerText.Replace(smalltitleNode.InnerText, "").Replace("\r\n", "");

            for (int i = 0; i < 20; i++)
            {
                title = title.Replace("  ", " ");
            }

            result.Add("title", title);



            //演员信息及头像
            List<string> actors = new List<string>();
            List<string> actorImages = new List<string>();
            HtmlNodeCollection actorNodes = doc.DocumentNode.SelectNodes("//div[@class='col-md-12']/div[@class='col-xs-6 col-md-3']/div[@class='thumbnail']/a");
            if (actorNodes != null)
            {
                foreach (HtmlNode actorNode in actorNodes)
                {
                    string actorlink = actorNode.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(actorlink) && actorlink.IndexOf("/star/") >= 0)
                    {
                        HtmlNode imgNode = actorNode.SelectSingleNode("img");
                        actors.Add(actorNode.InnerText);
                        if (imgNode != null && !string.IsNullOrEmpty(imgNode.Attributes["src"]?.Value))
                            actorImages.Add(imgNode.Attributes["src"].Value);
                    }

                }
            }

            result.Add("actor", string.Join("/", actors));
            result.Add("actressimageurl", string.Join(";", actorImages));

            //基本信息

            HtmlNode colNode = doc.DocumentNode.SelectSingleNode("//div[@class='col-md-9']");
            if (colNode != null)
            {
                HtmlNodeCollection htmlNodes = colNode.ChildNodes;
                if (htmlNodes != null)
                {
                    for (int i = 0; i < htmlNodes.Count; i++)
                    {
                        string text = htmlNodes[i].InnerText;
                        if (!string.IsNullOrEmpty(text) && text != " &nbsp;")
                        {
                            if (text.IndexOf("メーカー") >= 0)
                            {
                                i += 2;
                                HtmlNode nextNode = htmlNodes[i];
                                if (!string.IsNullOrEmpty(nextNode.Attributes["href"]?.Value))
                                    result.Add("studio", nextNode.InnerText);
                            }
                            else if (text.IndexOf("平均評価") >= 0)
                            {
                                i += 2;
                                HtmlNode nextNode = htmlNodes[i];
                                string src = nextNode.Attributes["data-original"]?.Value;
                                if (!string.IsNullOrEmpty(src) && src.IndexOf("/") >= 0)
                                {
                                    string r = src.Split('/').Last().Split('.').First();
                                    float.TryParse(r, out float rating);
                                    result.Add("rating", (rating / 10 * 2).ToString());
                                }
                            }
                            else if (text.IndexOf("配信開始日") >= 0)
                            {
                                i += 1;
                                HtmlNode nextNode = htmlNodes[i];
                                string date = nextNode.InnerText;
                                if (!string.IsNullOrEmpty(date))
                                {
                                    result.Add("releasedate", date.Replace(": ", ""));
                                }
                            }
                            else if (text.IndexOf("収録時間") >= 0)
                            {
                                i += 1;
                                HtmlNode nextNode = htmlNodes[i];
                                string date = nextNode.InnerText;
                                if (!string.IsNullOrEmpty(date))
                                {
                                    result.Add("runtime", date.Replace(": ", "").Replace(" minutes", ""));
                                }
                            }
                            else if (text.IndexOf("収録時間") >= 0)
                            {
                                i += 1;
                                HtmlNode nextNode = htmlNodes[i];
                                string date = nextNode.InnerText;
                                if (!string.IsNullOrEmpty(date))
                                {
                                    result.Add("runtime", date.Replace(": ", "").Replace(" minutes", ""));
                                }
                            }
                            else if (text.IndexOf("出演者") >= 0 && actors.Count <= 0)
                            {
                                i += 1;
                                HtmlNode nextNode = htmlNodes[i];
                                string actor = nextNode.InnerText;
                                if (!string.IsNullOrEmpty(actor))
                                {
                                    actor = actor.Replace(" &nbsp;", "").Replace(": ", "");
                                    foreach (var item in actor.Split(' '))
                                    {
                                        if (!string.IsNullOrEmpty(item) && item.Length > 0)
                                        {
                                            actors.Add(item);
                                        }
                                    }
                                    result["actor"] = string.Join("/", actors);
                                }
                            }
                        }
                    }
                }
            }



            //摘要
            HtmlNodeCollection plotNodes = doc.DocumentNode.SelectNodes("//div[@class='panel-body']/div[@class='row']/div[@class='col-md-12']");
            if (plotNodes != null)
            {
                foreach (HtmlNode plotNode in plotNodes)
                {
                    if (!string.IsNullOrEmpty(plotNode.InnerText))
                    {
                        result.Add("plot", plotNode.InnerText);
                        break;
                    }

                }
            }


            //缩略图
            HtmlNode smallimageNode = doc.DocumentNode.SelectSingleNode("//div[@class='panel-body']/div/div/img");
            if (smallimageNode != null) { result.Add("smallimageurl", smallimageNode.Attributes["src"].Value); }

            //海报图
            HtmlNodeCollection imageNodes = doc.DocumentNode.SelectNodes("//div[@class='col-md-3']/div/p/a/img");
            if (imageNodes != null)
            {
                result.Add("bigimageurl", imageNodes[0].Attributes["src"].Value);
                string extraimage = "";
                for (int i = 1; i < imageNodes.Count; i++)
                {
                    HtmlNode htmlNode = imageNodes[i];
                    try { extraimage = extraimage + htmlNode.Attributes["src"].Value + ";"; }
                    catch { continue; }
                }
                result.Add("extraimageurl", extraimage);
            }


            return result;
        }

    }
}
