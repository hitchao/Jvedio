using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jvedio.Utils;
using Jvedio.Utils.Interface;

namespace Jvedio.Utils.Net
{

    public class CrawlerHeader
    {
        public string Method = "GET";
        public string Host = "";
        public string Connection = "keep-alive";
        public string CacheControl = "max-age=0";
        public string UpgradeInsecureRequests = "1";
        public string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
        public string Accept = "*/*";
        public string SecFetchSite = "same-origin";
        public string SecFetchMode = "navigate";
        public string SecFetchUser = "?1";
        public string SecFetchDest = "document";
        public string AcceptEncoding = "";
        public string AcceptLanguage = "zh-CN,zh;q=0.9";
        public string Cookies = "";
        public string Referer = "";
        public string Origin = "";
        public long ContentLength = 0;
        public string ContentType = "";
    }

    public class ResponseHeaders
    {
        public string Date = "";
        public string ContentType = "";
        public string Connection = "";
        public string CacheControl = "";
        public string SetCookie = "";
        public string Location = "";
        public double ContentLength = 0;
    }

    public class HttpResult
    {
        public bool Success = false;//是否成功获取请求
        public string Error = "";//如果发生错误，则保存错误信息
        public string SourceCode = "";// html 源码
        public byte[] FileByte = null;//如果返回的是文件，则保存文件
        public string MovieCode = "";//网址中影片对应的地址
        public HttpStatusCode StatusCode = HttpStatusCode.Forbidden;
        public ResponseHeaders Headers = null;//返回体
    }


    public class Net : ILog
    {
        public int TCPTIMEOUT = 30;   // TCP 超时
        public int HTTPTIMEOUT = 30; // HTTP 超时
        public int ATTEMPTNUM = 2; // 最大尝试次数
        public int REQUESTTIMEOUT = 30000;//网站 HTML 获取超时
        public int FILE_REQUESTTIMEOUT = 30000;//图片下载超时
        public int READWRITETIMEOUT = 30000;


        public Net(int[] timeouts)
        {
            TCPTIMEOUT = timeouts[0];
            HTTPTIMEOUT = timeouts[1];
            REQUESTTIMEOUT = timeouts[2];
            FILE_REQUESTTIMEOUT = timeouts[3];
            READWRITETIMEOUT = timeouts[4];
        }

        public Net()
        {

        }



        public enum HttpMode
        {
            Normal = 0,
            RedirectGet = 1,//重定向到 Location
            Stream = 2//下载文件
        }

        /// <summary>
        /// Http 请求
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="headers"></param>
        /// <param name="Mode"></param>
        /// <param name="Proxy"></param>
        /// <param name="allowRedirect"></param>
        /// <param name="poststring"></param>
        /// <returns></returns>
        public async Task<HttpResult> Http(string Url, CrawlerHeader headers = null, HttpMode Mode = HttpMode.Normal, WebProxy Proxy = null, bool allowRedirect = true, string postString = "")
        {
            if (!Url.IsProperUrl()) return null;
            if (headers == null) headers = new CrawlerHeader();
            int trynum = 0;
            HttpResult httpResult = null;

            try
            {
                while (trynum < ATTEMPTNUM && httpResult == null)
                {
                    httpResult = await Task.Run(() =>
                    {
                        HttpWebRequest Request;
                        HttpWebResponse Response = default;
                        try
                        {
                            Request = (HttpWebRequest)HttpWebRequest.Create(Url);
                        }
                        catch (Exception e)
                        {
                            Log($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");
                            return null;
                        }
                        Uri uri = new Uri(Url);
                        Request.Host = headers.Host == "" ? uri.Host : headers.Host;
                        Request.Accept = headers.Accept;
                        Request.Timeout = HTTPTIMEOUT * 1000;
                        Request.Method = headers.Method;
                        Request.KeepAlive = true;
                        Request.AllowAutoRedirect = allowRedirect;
                        Request.Referer = uri.Scheme + "://" + uri.Host + "/";
                        Request.UserAgent = headers.UserAgent;
                        Request.Headers.Add("Accept-Language", headers.AcceptLanguage);
                        Request.Headers.Add("Upgrade-Insecure-Requests", headers.UpgradeInsecureRequests);
                        Request.Headers.Add("Sec-Fetch-Site", headers.SecFetchSite);
                        Request.Headers.Add("Sec-Fetch-Mode", headers.SecFetchMode);
                        Request.Headers.Add("Sec-Fetch-User", headers.SecFetchUser);
                        Request.Headers.Add("Sec-Fetch-Dest", headers.SecFetchDest);
                        Request.ReadWriteTimeout = READWRITETIMEOUT;
                        if (headers.Cookies != "") Request.Headers.Add("Cookie", headers.Cookies);
                        if (Mode == HttpMode.RedirectGet) Request.AllowAutoRedirect = false;
                        if (Proxy != null) Request.Proxy = Proxy;

                        try
                        {
                            if (headers.Method == "POST")
                            {
                                Request.Method = "POST";
                                Request.ContentType = headers.ContentType;
                                Request.ContentLength = headers.ContentLength;
                                Request.Headers.Add("Origin", headers.Origin);
                                byte[] bs = Encoding.UTF8.GetBytes(postString);
                                using (Stream reqStream = Request.GetRequestStream())
                                {
                                    reqStream.Write(bs, 0, bs.Length);
                                }
                            }
                            Response = (HttpWebResponse)Request.GetResponse();
                            httpResult = GetHttpResult(Response, Mode);
                            Console.WriteLine($" {Jvedio.Language.Resources.Url}：{Url} => {httpResult.StatusCode}");
                        }
                        catch (WebException e)
                        {
                            Log($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");

                            httpResult = new HttpResult()
                            {
                                Error = e.Message,
                                Success = false,
                                SourceCode = ""
                            };

                            if (e.Status == WebExceptionStatus.Timeout)
                                trynum++;
                            else
                                trynum = 2;
                        }
                        catch (Exception e)
                        {
                            Log($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");
                            trynum = 2;
                        }
                        finally
                        {
                            if (Response != null) Response.Close();
                        }
                        return httpResult;
                    }).TimeoutAfter(TimeSpan.FromSeconds(HTTPTIMEOUT));
                }
            }
            catch (TimeoutException ex)
            {
                //任务超时了
                Console.WriteLine(ex.Message);
            }
            return httpResult;
        }

        public async Task<HttpResult> Http(string Url)
        {
            return await Http(Url, null);
        }


        /// <summary>
        /// 将 HttpWebResponse 转为 HttpResult
        /// </summary>
        /// <param name="response"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public HttpResult GetHttpResult(HttpWebResponse response, HttpMode mode)
        {
            HttpResult httpResult = new HttpResult();
            WebHeaderCollection webHeaderCollection = null;
            ResponseHeaders responseHeaders = null;
            try
            {
                httpResult.StatusCode = response.StatusCode;
                webHeaderCollection = response.Headers;
            }
            catch (ObjectDisposedException ex)
            {
                Log(ex.Message);
            }
            if (webHeaderCollection != null)
            {
                //获得响应头
                responseHeaders = new ResponseHeaders()
                {
                    Location = webHeaderCollection.Get("Location"),
                    Date = webHeaderCollection.Get("Date"),
                    ContentType = webHeaderCollection.Get("Content-Type"),
                    Connection = webHeaderCollection.Get("Connection"),
                    CacheControl = webHeaderCollection.Get("Cache-Control"),
                    SetCookie = webHeaderCollection.Get("Set-Cookie"),
                };
                double.TryParse(webHeaderCollection.Get("Content-Length"), out responseHeaders.ContentLength);
                httpResult.Headers = responseHeaders;
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (mode != HttpMode.Stream)
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            httpResult.SourceCode = sr.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                    if (responseHeaders?.ContentLength == 0) responseHeaders.ContentLength = httpResult.SourceCode.Length;
                }
                else
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            response.GetResponseStream().CopyTo(ms);
                            httpResult.FileByte = ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                    if (responseHeaders?.ContentLength == 0) responseHeaders.ContentLength = httpResult.FileByte.Length;
                }
            }
            return httpResult;
        }


        public async Task<(bool, string, string)> CheckUpdate(string UpdateUrl)
        {
            if (string.IsNullOrEmpty(UpdateUrl)) return (false, "", "");
            return await Task.Run(async () =>
            {
                HttpResult httpResult = null;
                try
                {
                    httpResult = await Http(UpdateUrl);
                }
                catch (TimeoutException ex) { Log($"URL={UpdateUrl},Message-{ex.Message}"); }
                if (httpResult == null || string.IsNullOrEmpty(httpResult.SourceCode) || httpResult.SourceCode.IndexOf('\n') < 0) return (false, "", "");
                string remote = httpResult.SourceCode.Split('\n')[0].Replace("\r", "");
                string updateContent = httpResult.SourceCode.Replace(remote + "\n", "");
                string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                try
                {
                    using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "OldVersion"))
                    {
                        sw.WriteLine(local + "\n");
                        sw.WriteLine(updateContent);
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
                return (true, remote, updateContent);
            });
        }


        public async Task<WebSite> CheckUrlType(string url)
        {
            WebSite webSite = WebSite.None;
            if (string.IsNullOrEmpty(url)) return webSite;
            bool enablecookie = false;
            string label = "";
            (bool result, string title) = await TestAndGetTitle(url, enablecookie, "", label);
            if (result)
            {
                //其他，进行判断
                if (title.ToLower().IndexOf("javbus") >= 0 && title.IndexOf("歐美") < 0)
                {
                    webSite = WebSite.Bus;
                }
                else if (title.ToLower().IndexOf("javbus") >= 0 && title.IndexOf("歐美") >= 0)
                {
                    webSite = WebSite.BusEu;
                }
                else if (title.ToLower().IndexOf("javlibrary") >= 0)
                {
                    webSite = WebSite.Library;
                }
                else if (title.ToLower().IndexOf("fanza") >= 0)
                {
                    webSite = WebSite.DMM;
                }
                else if (title.ToLower().IndexOf("jav321") >= 0)
                {
                    webSite = WebSite.Jav321;
                }
                else if (title.ToLower().IndexOf("javdb") >= 0)
                {
                    webSite = WebSite.DB;

                }
                else if (title.ToLower().IndexOf("avmoo") >= 0)
                {
                    webSite = WebSite.MOO;

                }
                else
                {
                    webSite = WebSite.None;
                }
            }
            return webSite;
        }


        public async Task<(bool, string)> TestAndGetTitle(string Url, bool EnableCookie, string Cookie, string Label)
        {
            bool result = false;
            string title = "";
            if (string.IsNullOrEmpty(Url)) return (result, title);
            HttpResult httpResult = null;
            if (EnableCookie)
            {
                try
                {
                    if (Label == "DB")
                    {
                        httpResult = await Http(Url + "v/P2Rz9", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult != null && httpResult.SourceCode.IndexOf("FC2-659341") >= 0)
                        {
                            result = true;
                            title = "DB";
                        }
                        else result = false;
                    }
                    else if (Label == "DMM")
                    {
                        httpResult = await Http($"{Url}mono/dvd/-/search/=/searchstr=APNS-006/ ", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult != null && httpResult.SourceCode.IndexOf("里美まゆ・川") >= 0)
                        {
                            result = true;
                            title = "DMM";
                        }
                        else result = false;
                    }
                    else if (Label == "MOO")
                    {
                        httpResult = await Http($"{Url}movie/655358482fd14364 ", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult != null && httpResult.SourceCode.IndexOf("SIVR-118") >= 0)
                        {
                            result = true;
                            title = "MOO";
                        }
                        else result = false;
                    }
                }
                catch (TimeoutException ex)
                {
                    Log(ex.Message);
                }
            }
            else
            {
                try { httpResult = await Http(Url, new CrawlerHeader() { Cookies = Cookie }); }
                catch (TimeoutException ex) { Log(ex); }

                if (httpResult != null)
                {
                    result = true;
                    //获得标题
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(httpResult.SourceCode);
                    HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");
                    if (titleNode != null) title = titleNode.InnerText;
                }
            }
            return (result, title);
        }

        public async Task<HttpResult> DownLoadFile(string Url, WebProxy Proxy = null, string setCookie = "")
        {
            HttpResult httpResult = null;
            if (string.IsNullOrEmpty(Url)) return httpResult;
            SetCertificatePolicy();
            try { httpResult = await Http(Url, new CrawlerHeader() { Cookies = setCookie }, HttpMode.Stream); }
            catch (TimeoutException ex) { Log(ex); }
            return httpResult;
        }




        /// <summary>
        /// Sets the cert policy.
        /// </summary>
        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback
                       += RemoteCertificateValidate;
        }

        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
            X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!
            //System.Console.WriteLine("Warning, trust any certificate");
            return true;
        }

        public virtual void Log(string str)
        {
            throw new NotImplementedException();
        }

        public void Log(Exception ex)
        {
            throw new NotImplementedException();
        }
    }













}
