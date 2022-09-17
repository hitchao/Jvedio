using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.Global;
using Jvedio.Core.Plugins.Crawler;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Jvedio.Core.Plugins
{
    /// <summary>
    /// 插件基本信息
    /// </summary>
    public class FetchState
    {
        public string PluginID { get; set; }

        public bool Downloading { get; set; }

        public PluginMetaData MetaData { get; set; }
    }

    public class PluginManager
    {
        public static List<PluginMetaData> PluginList = new List<PluginMetaData>();

        public static List<FetchState> FetchingList { get; set; }

        public static List<string> DownloadingList { get; set; }

        private static void MergeAllPlugin()
        {
            PluginList.AddRange(ThemeManager.PluginMetaDatas);
            PluginList.AddRange(CrawlerManager.PluginMetaDatas);
        }

        public static void Init()
        {
            MergeAllPlugin();
            SetPluginEnabled();
            FetchingList = new List<FetchState>();
            DownloadingList = new List<string>();
        }

        private static void SetPluginEnabled()
        {
            if (PluginList?.Count > 0
                && !string.IsNullOrEmpty(ConfigManager.Settings.PluginEnabledJson))
            {
                string json = ConfigManager.Settings.PluginEnabledJson;
                if (string.IsNullOrEmpty(json)) return;
                Dictionary<string, bool> dict = JsonUtils.TryDeserializeObject<Dictionary<string, bool>>(json);
                if (dict == null || dict.Count <= 0) return;
                foreach (PluginMetaData plugin in PluginList)
                {
                    string pluginID = plugin.PluginID;
                    if (string.IsNullOrEmpty(pluginID)) continue;
                    if (dict.ContainsKey(pluginID))
                        plugin.Enabled = dict[pluginID];
                }
            }
        }

        public static async Task<PluginMetaData> FetchPlugin(PluginMetaData data)
        {
            if (data == null || string.IsNullOrEmpty(data.PluginID)) return null;

            if (FetchingList.Any(arg => arg.PluginID.Equals(data.PluginID)))
            {
                FetchState fetchState = FetchingList.Where(arg => arg.PluginID.Equals(data.PluginID)).FirstOrDefault();
                if (fetchState != null && fetchState.Downloading == false && fetchState.MetaData != null)
                {
                    return fetchState.MetaData;
                }
            }

            return await Task.Run(async () =>
           {
               FetchState fetchState = new FetchState();
               fetchState.Downloading = true;
               fetchState.PluginID = data.PluginID;
               RequestHeader header = CrawlerHeader.GitHub;
               string base_url = $"https://hitchao.github.io/Jvedio-Plugin/plugins/{data.PluginType.ToString().ToLower()}s/{data.GetRawPluginID()}";
               string url_main_json = $"{base_url}/main.json";
               string url_readme = $"{base_url}/readme.md";
               HttpResult httpResult = await HttpClient.Get(url_main_json, header);
               if (httpResult.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
               {
                   // 解析 main.json
                   string json = httpResult.SourceCode;
                   PluginMetaData pluginMeta = PluginMetaData.ParseStr(json);
                   if (string.IsNullOrEmpty(pluginMeta.ReleaseNotes.MarkDown))
                   {
                       // 下载 markdown
                       HttpResult http = await HttpClient.Get(url_readme, header);
                       if (httpResult.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
                       {
                           pluginMeta.ReleaseNotes.MarkDown = http.SourceCode;
                       }
                   }

                   pluginMeta.PluginID = data.PluginID;
                   pluginMeta.SetRemoteUrl();
                   fetchState.Downloading = false;
                   fetchState.MetaData = pluginMeta;
                   FetchingList.Add(fetchState);
                   return pluginMeta;
               }

               return null;
           });
        }

        public static void DownloadPlugin(PluginMetaData data)
        {
            Task.Run(async () =>
            {
                RequestHeader header = CrawlerHeader.GitHub;
                string base_url = $"https://hitchao.github.io/Jvedio-Plugin/plugins/{data.PluginType.ToString().ToLower()}s/{data.GetRawPluginID()}";
                string url_main_json = $"{base_url}/main.json";
                string url_readme = $"{base_url}/readme.md";
                string url_plugin_image = $"{base_url}/images/plugin.png";
                string base_path = System.IO.Path.Combine(PathManager.BasePluginsPath, "temp", data.PluginType.ToString().ToLower() + "s", data.GetRawPluginID());
                DirHelper.TryCreateDirectory(base_path, (err) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MessageCard.Error(err.Message);
                    });
                });
                HttpResult httpResult = await HttpClient.Get(url_main_json, header);
                if (httpResult.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(httpResult.SourceCode))
                {
                    // 解析 main.json

                    // 下载 main.json, readme.md, plugin.png
                    HttpResult result = await HttpClient.Get(url_main_json, header, CommonNet.Enums.HttpMode.Stream);
                    if (result.StatusCode == HttpStatusCode.OK && result.FileByte != null)
                    {
                        string savePath = System.IO.Path.Combine(base_path, "main.json");

                        FileHelper.ByteArrayToFile(result.FileByte, savePath);
                    }

                    result = await HttpClient.Get(url_readme, header, CommonNet.Enums.HttpMode.String);
                    if (result.StatusCode == HttpStatusCode.OK && result.SourceCode != null)
                    {
                        string savePath = System.IO.Path.Combine(base_path, "readme.md");
                        FileHelper.TryWriteToFile(savePath, result.SourceCode);
                    }

                    result = await HttpClient.Get(url_plugin_image, header, CommonNet.Enums.HttpMode.Stream);
                    if (result.StatusCode == HttpStatusCode.OK && result.FileByte != null)
                    {
                        string savePath = System.IO.Path.Combine(base_path, "images");
                        DirHelper.TryCreateDirectory(savePath);
                        FileHelper.ByteArrayToFile(result.FileByte, System.IO.Path.Combine(savePath, "plugin.png"));
                    }

                    // 根据 main.json，下载文件
                    string json = httpResult.SourceCode;
                    List<string> list = PluginMetaData.GetFileListByJson(json);
                    if (list?.Count > 0)
                    {
                        foreach (string item in list)
                        {
                            string url = System.IO.Path.Combine(base_url, item);
                            string savePath = System.IO.Path.Combine(base_path, item);
                            result = await HttpClient.Get(url, header, CommonNet.Enums.HttpMode.Stream);
                            if (result.StatusCode == HttpStatusCode.OK && result.FileByte != null)
                            {
                                string path = System.IO.Path.GetDirectoryName(savePath);
                                DirHelper.TryCreateDirectory(path);
                                FileHelper.ByteArrayToFile(result.FileByte, savePath);
                            }
                        }
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MessageCard.Info($"插件【{data.PluginName}】下载完成！重启后生效");
                    });
                }
            });
        }
    }
}
