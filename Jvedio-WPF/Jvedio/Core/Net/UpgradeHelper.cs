using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Utils.Encrypt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public static class UpgradeHelper
    {

        public static RequestHeader Header;



        static UpgradeHelper()
        {
            Header = new RequestHeader();
            Header.Method = System.Net.Http.HttpMethod.Get;
            Header.WebProxy = GlobalConfig.ProxyConfig.GetWebProxy();
            Header.Headers = new Dictionary<string, string>()
            {
                {"User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36" },
            };
        }

        //public static string list_url = "https://hitchao.github.io/jvedioupdate/list";// 4.6 前的版本
        public const string LIST_URL = "https://hitchao.github.io/jvedioupdate/list.json";
        public static string file_url = "https://hitchao.github.io/jvedioupdate/File/";

        public static event EventHandler onCompleted;
        public static event EventHandler onDownloading;
        public static event EventHandler onError;

        public static bool Canceld = true;

        static Dictionary<int, string> languageDict = new Dictionary<int, string>()
        {
            {0,"zh-CN" },
            {1,"en-US" },
            {2,"ja-JP" },
        };

        public async static Task<(string LatestVersion, string ReleaseDate, string ReleaseNote)> getUpgardeInfo()
        {

            string LatestVersion = "";
            string ReleaseDate = "";
            string ReleaseNote = "";
            try
            {
                HttpResult result = await HttpClient.Get(GlobalVariable.UpdateUrl, Header);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string sourceCode = result.SourceCode;
                    Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(sourceCode);
                    if (dict == null || !dict.ContainsKey("LatestVersion"))
                    {
                        throw new Exception("解析错误！");
                    }
                    else
                    {
                        LatestVersion = dict["LatestVersion"].ToString();

                        if (dict.ContainsKey("ReleaseDate"))
                            ReleaseDate = dict["ReleaseDate"].ToString();


                        if (dict.ContainsKey("ReleaseNote"))
                        {
                            Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict["ReleaseNote"].ToString());
                            string lang = languageDict[(int)GlobalConfig.Settings.SelectedLanguage];
                            ReleaseNote = d.ContainsKey(lang) ? d[lang] : "";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (LatestVersion, ReleaseDate, ReleaseNote);
        }


        private static async Task<List<string>> GetFileList()
        {
            List<string> toDownload = new List<string>();
            try
            {
                HttpResult httpResult = await HttpClient.Get(LIST_URL, Header);
                if (httpResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
                {
                    Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResult.SourceCode);
                    if (!dict.ContainsKey("FileName") || !dict.ContainsKey("FileHash"))
                        throw new Exception("更新文件的索引解析失败！");
                    List<string> FileName = ((JArray)dict["FileName"]).ToObject<List<string>>();
                    List<string> FileHash = ((JArray)dict["FileHash"]).ToObject<List<string>>();
                    for (int i = 0; i < FileName.Count; i++)
                    {
                        string fileName = FileName[i];
                        string fileHash = FileHash[i];
                        string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                        if (File.Exists(localPath))
                        {
                            //校验
                            if (!Encrypt.GetFileMD5(localPath).Equals(fileHash))
                            {
                                toDownload.Add(fileName);//md5 不一致 ，下载
                            }
                        }
                        else
                        {
                            toDownload.Add(fileName); //不存在 =>下载
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(null, new MessageCallBackEventArgs(ex.Message));
                Logger.LogN(ex.Message);

            }
            return toDownload;
        }


        private static void WriteFile(byte[] filebyte, string savepath)
        {
            FileInfo fileInfo = new FileInfo(savepath);
            if (!Directory.Exists(fileInfo.Directory.FullName)) Directory.CreateDirectory(fileInfo.Directory.FullName);//创建文件夹
            try
            {
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(filebyte, 0, filebyte.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }


        public static async Task<bool> BeginUpgrade()
        {
            Canceld = false;
            List<string> list = await GetFileList();
            if (list != null && list.Count > 0)
                await DownLoadFiles(list);
            return true;
        }

        private static async Task<bool> DownLoadFiles(List<string> list)
        {

            //新建临时文件夹
            string temppath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            if (!Directory.Exists(temppath)) Directory.CreateDirectory(temppath);

            double count = 0;
            double total = list.Count;
            foreach (var item in list)
            {
                if (Canceld) break;
                string filepath = Path.Combine(temppath, item);
                if (!File.Exists(filepath))
                {
                    try
                    {
                        HttpResult streamResult = await HttpHelper.AsyncDownLoadFile(file_url + item, Header);
                        //写入本地
                        if (streamResult.FileByte != null) WriteFile(streamResult.FileByte, filepath);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(null, new MessageCallBackEventArgs(ex.Message));
                    }

                }
                count++;
                double progress = Math.Round(count / total * 100, 4); ;
                if (!Canceld) onDownloading?.Invoke(null, new MessageCallBackEventArgs(progress.ToString()));
            }

            //复制文件并覆盖 执行 cmd 命令
            if (!Canceld) onCompleted?.Invoke(null, null);
            return true;
        }


        public static void Cancel()
        {
            Canceld = true;
            Logger.Error("已取消下载任务");
        }

    }
}
