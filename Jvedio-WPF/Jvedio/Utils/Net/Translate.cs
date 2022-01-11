
using Jvedio.Utils.Encrypt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jvedio
{
    //https://ai.youdao.com/DOCSIRMA/html/%E8%87%AA%E7%84%B6%E8%AF%AD%E8%A8%80%E7%BF%BB%E8%AF%91/API%E6%96%87%E6%A1%A3/%E6%96%87%E6%9C%AC%E7%BF%BB%E8%AF%91%E6%9C%8D%E5%8A%A1/%E6%96%87%E6%9C%AC%E7%BF%BB%E8%AF%91%E6%9C%8D%E5%8A%A1-API%E6%96%87%E6%A1%A3.html
    public static class Translate
    {
        public static string Youdao_appKey;
        public static string Youdao_appSecret;



        public static void InitYoudao()
        {
            Youdao_appKey = Properties.Settings.Default.TL_YOUDAO_APIKEY.Replace(" ", "");
            Youdao_appSecret = Properties.Settings.Default.TL_YOUDAO_SECRETKEY.Replace(" ", "");
        }


        //TODO 国际化
        public static Task<string> Youdao(string q)
        {
            string from = "auto";
            string to = "zh-CHS";
            string language = Jvedio.Properties.Settings.Default.Language;
            switch (language)
            {

                case "中文":
                    to = "zh-CHS";
                    break;
                case "English":
                    to = "en";
                    break;
                case "日本語":
                    to = "ja";
                    break;
                default:
                    break;
            }
            return Task.Run(() =>
            {
                InitYoudao();
                Dictionary<String, String> dic = new Dictionary<String, String>();
                string url = "https://openapi.youdao.com/api";

                string salt = DateTime.Now.Millisecond.ToString();
                dic.Add("from", from);
                dic.Add("to", to);
                dic.Add("signType", "v3");
                TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                long millis = (long)ts.TotalMilliseconds;
                string curtime = Convert.ToString(millis / 1000);
                dic.Add("curtime", curtime);
                string signStr = Youdao_appKey + Truncate(q) + salt + curtime + Youdao_appSecret; ;
                string sign = ComputeHash(signStr, new SHA256CryptoServiceProvider());
                dic.Add("q", System.Web.HttpUtility.UrlEncode(q));
                dic.Add("appKey", Youdao_appKey);
                dic.Add("salt", salt);
                dic.Add("sign", sign);
                try
                {
                    return GetYoudaoResult(Post(url, dic));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }

            });

        }

        public static string GetYoudaoResult(string content)
        {
            if (content.IndexOf("translation") < 0) return "";
            string pattern = @"""translation"":\["".+""\]";
            string result = Regex.Match(content, pattern).Value;
            return result.Replace("\"translation\":[\"", "").Replace("\"]", "");
        }


        public static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            if (string.IsNullOrEmpty(input) || algorithm == null) return "";
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        public static string Post(string url, Dictionary<String, String> dic)
        {
            string result = "";
            if (string.IsNullOrEmpty(url)) return result;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                StringBuilder builder = new StringBuilder();
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    builder.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
                byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp.ContentType.ToLower().Equals("audio/mp3"))
                {
                    SaveBinaryFile(resp, "合成的音频存储路径");
                }
                else
                {
                    Stream stream = resp.GetResponseStream();
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogN(ex.Message);
            }
            return "";
        }

        public static string Truncate(string q)
        {
            if (string.IsNullOrEmpty(q)) return null;
            int len = q.Length;
            return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
        }

        private static bool SaveBinaryFile(WebResponse response, string FileName)
        {
            string FilePath = FileName + DateTime.Now.Millisecond.ToString() + ".mp3";
            bool Value = true;
            byte[] buffer = new byte[1024];

            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                Stream outStream = System.IO.File.Create(FilePath);
                Stream inStream = response.GetResponseStream();

                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            }
            catch
            {
                Value = false;
            }
            return Value;
        }



        public static string GetResult(string content)
        {
            if (content.IndexOf("dst") < 0) return "";
            var lst = new List<string>();
            dynamic json = JsonConvert.DeserializeObject(content);
            foreach (var item in json.trans_result)
            {
                lst.Add(item.dst.ToString());
            }
            return string.Join(";", lst);
        }

        private static string GetRandomString(int length)
        {
            string availableChars = "0123456789";
            var id = new char[length];
            Random random = new Random();
            for (var i = 0; i < length; i++)
            {
                id[i] = availableChars[random.Next(0, availableChars.Length)];
            }

            return new string(id);
        }

        public static string GenerateSign(string query, string appid, string pwd, string salt)
        {
            //http://api.fanyi.baidu.com/doc/21
            //appid+q+salt+密钥 
            string r = appid + query + salt + pwd;
            return Encrypt.CalculateMD5Hash(r);
        }



    }

}
