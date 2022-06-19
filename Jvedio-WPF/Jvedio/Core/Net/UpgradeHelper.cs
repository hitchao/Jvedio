using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Exceptions;
using Jvedio.Logs;
using Jvedio.Utils.Common;
using Jvedio.Utils.IO;
using JvedioLib.Security;
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





        //public static string list_url = "https://hitchao.github.io/jvedioupdate/list";// 4.6 前的版本
        public const string LIST_URL = "https://hitchao.github.io/jvedioupdate/list.json";
        public static string file_url = "https://hitchao.github.io/jvedioupdate/File/";

        public static event EventHandler onCompleted;
        public static event EventHandler onDownloading;
        public static event EventHandler onError;

        public static bool Canceld = true;

        private static int TotalLanguages = 3;
        static List<string> languageDict = new List<string>() { "zh-CN", "en-US", "ja-JP", };

        public async static Task<(string LatestVersion, string ReleaseDate, string ReleaseNote)> getUpgardeInfo()
        {

            string LatestVersion = "";
            string ReleaseDate = "";
            string ReleaseNote = "";
            try
            {
                HttpResult result = await HttpClient.Get(GlobalVariable.UpdateUrl, CrawlerHeader.GitHub);
                if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.SourceCode))
                {
                    string sourceCode = result.SourceCode;
                    Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(sourceCode);
                    if (dict == null || !dict.ContainsKey("LatestVersion"))
                    {
                        throw new Exception("解析错误！");
                    }
                    else
                    {
                        if (dict.ContainsKey("LatestVersion") && dict["LatestVersion"] != null)
                            LatestVersion = dict["LatestVersion"].ToString();

                        if (dict.ContainsKey("ReleaseDate") && dict["ReleaseDate"] != null)
                            ReleaseDate = dict["ReleaseDate"].ToString();


                        if (dict.ContainsKey("ReleaseNote") && dict["ReleaseNote"] != null)
                        {
                            Dictionary<string, string> d = JsonUtils.TryDeserializeObject<Dictionary<string, string>>(dict["ReleaseNote"].ToString());
                            if (d != null && d.Count == TotalLanguages && GlobalConfig.Settings.SelectedLanguage < languageDict.Count)
                            {
                                string lang = languageDict[(int)GlobalConfig.Settings.SelectedLanguage];
                                ReleaseNote = d.ContainsKey(lang) ? d[lang] : "";
                            }

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
            List<string> toDownload = null; // null 表示由于网址同步不成功
            try
            {
                HttpResult httpResult = await HttpClient.Get(LIST_URL, CrawlerHeader.GitHub);
                if (httpResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
                {
                    Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(httpResult.SourceCode);
                    if (dict == null || !dict.ContainsKey("FileName") || !dict.ContainsKey("FileHash"))
                        throw new Exception("更新文件的索引解析失败！");
                    List<string> FileName = ((JArray)dict["FileName"]).ToObject<List<string>>();
                    List<string> FileHash = ((JArray)dict["FileHash"]).ToObject<List<string>>();

                    toDownload = new List<string>();
                    for (int i = 0; i < FileName.Count; i++)
                    {
                        string fileName = FileName[i];
                        string fileHash = FileHash[i];
                        if (string.IsNullOrEmpty(fileHash) || string.IsNullOrEmpty(fileName)) continue;
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
                Logger.Error(ex.Message);

            }
            return toDownload;
        }


        private static void WriteFile(byte[] filebyte, string savepath)
        {
            FileInfo fileInfo = new FileInfo(savepath);
            DirHelper.TryCreateDirectory(fileInfo.Directory.FullName, (ex) =>
            {
                throw ex;
            });
            try
            {
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(filebyte, 0, filebyte.Length);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<bool> BeginUpgrade(Action<string> callback = null)
        {
            Canceld = false;
            List<string> list = await GetFileList();
            if (list == null)
            {
                callback.Invoke("从远程地址拉取更新列表失败");
            }
            else
            {
                if (list.Count == 0)
                {
                    callback.Invoke("所有文件都是最新的，无需更新！");
                }
                else
                {
                    return await DownLoadFiles(list);
                }
            }
            return false;
        }

        private static async Task<bool> DownLoadFiles(List<string> list)
        {

            //新建临时文件夹
            string temppath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");

            DirHelper.TryCreateDirectory(temppath, (ex) =>
            {
                onError?.Invoke(null, new MessageCallBackEventArgs(ex.Message));
                Canceld = true;
            });



            bool success = true;
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
                        HttpResult streamResult = await HttpHelper.AsyncDownLoadFile(file_url + item, CrawlerHeader.GitHub);
                        //写入本地
                        if (streamResult.FileByte != null) WriteFile(streamResult.FileByte, filepath);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(null, new MessageCallBackEventArgs(ex.Message));
                        success = false;
                        Logger.Error(ex);
                        break;
                    }

                }
                count++;
                double progress = Math.Round(count / total * 100, 4); ;
                if (!Canceld) onDownloading?.Invoke(null, new MessageCallBackEventArgs(progress.ToString()));
            }

            //复制文件并覆盖 执行 cmd 命令
            if (success && !Canceld) onCompleted?.Invoke(null, null);
            return true;
        }


        public static void Cancel()
        {
            Canceld = true;
            Logger.Error("已取消下载任务");
        }

    }
}
