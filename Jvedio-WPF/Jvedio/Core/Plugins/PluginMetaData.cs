using Jvedio.Core.Scan;
using Jvedio.Logs;
using Jvedio.Utils.Common;
using Jvedio.Utils.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.Plugins
{
    public enum PluginType
    {

        Cralwer,
        Theme,
        None
    }


    public class AuthorInfo
    {
        public string Name { get; set; }
        public Dictionary<string, string> Infos { get; set; }

        public AuthorInfo()
        {
            Infos = new Dictionary<string, string>();
        }
    }


    public class ReleaseNotes
    {
        public string Version { get; set; }
        public string Date { get; set; }
        public string Desc { get; set; }
        public string MarkDown { get; set; }
        public Dictionary<string, string> KeyWords { get; set; }
    }

    public class PluginMetaData
    {
        private string _PluginID;
        public string PluginID
        {
            get { return _PluginID; }
            set { _PluginID = value.ToLower(); }
        }
        public string PluginName { get; set; }
        public PluginType PluginType { get; set; }
        public List<AuthorInfo> Authors { get; set; }
        public ReleaseNotes ReleaseNotes { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public string ImagePath { get; set; }



        // 其它的字段
        public string AuthorNames { get; set; }
        public bool Enabled { get; set; }
        public bool HasNewVersion { get; set; }
        public bool Installed { get; set; }
        public string NewVersion { get; set; }
        public string FileHash { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; }


        public PluginType Type { get; set; }



        public static PluginMetaData Parse(string jsonPath)
        {
            string content = FileHelper.TryReadFile(jsonPath);
            if (string.IsNullOrEmpty(content)) return null;
            Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(content);
            if (dict == null || !dict.ContainsKey("PluginMetaData")) return null;
            PluginMetaData metaData = null;
            try
            {
                metaData = Parse(dict["PluginMetaData"] as JObject);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
            }
            SetMarkDown(ref metaData, Path.GetDirectoryName(jsonPath));
            SetImagePath(ref metaData, Path.GetDirectoryName(jsonPath));


            return metaData;
        }

        public static void SetMarkDown(ref PluginMetaData data, string pluginDir)
        {
            if (data == null || string.IsNullOrEmpty(pluginDir)) return;
            string path = Path.Combine(pluginDir, "readme.md");
            string content = FileHelper.TryReadFile(path);
            if (string.IsNullOrEmpty(content)) return;
            data.ReleaseNotes.MarkDown = content;
        }

        public static void SetImagePath(ref PluginMetaData data, string pluginDir)
        {
            if (data == null || string.IsNullOrEmpty(pluginDir)) return;
            string imagePath = Path.Combine(pluginDir, "images");
            List<string> list = FileHelper.TryGetAllFiles(imagePath, "plugin.*").ToList();
            string path = "";
            foreach (var item in list)
            {
                path = FileHelper.FindWithExt(item, ScanTask.PICTURE_EXTENSIONS_LIST);
                if (File.Exists(path)) break;
            }
            if (File.Exists(path)) data.ImagePath = path;
        }

        private static PluginMetaData Parse(JObject dict)
        {
            if (dict == null || !dict.ContainsKey("PluginType") || !dict.ContainsKey("ReleaseNotes") ||
                !dict.ContainsKey("PluginName"))
            {
                return null;
            }
            string pluginType = dict["PluginType"].ToString();
            bool parsed = int.TryParse(pluginType, out int type);
            if (!parsed) return null;
            PluginMetaData metaData = new PluginMetaData();
            metaData.PluginType = (PluginType)type;
            metaData.PluginName = dict["PluginName"].ToString();
            if (dict.ContainsKey("Authors") && dict["Authors"] is JArray jArray)
            {
                metaData.Authors = new List<AuthorInfo>();
                foreach (JObject item in jArray)
                {
                    if (!item.HasValues || !item.ContainsKey("Name")) continue;
                    AuthorInfo authorInfo = new AuthorInfo();

                    foreach (JProperty child in item.Children())
                    {
                        string name = child.Name;
                        string value = child.Value.ToString();
                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) continue;
                        if (name.Equals("Name"))
                        {
                            authorInfo.Name = value;
                        }
                        else
                        {
                            authorInfo.Infos.Add(name, value);
                        }
                    }
                    metaData.Authors.Add(authorInfo);
                }
                metaData.AuthorNames = string.Join("/", metaData.Authors.Select(arg => arg.Name));

            }

            JObject jObject = dict["ReleaseNotes"] as JObject;
            if (jObject == null || !jObject.ContainsKey("Date")) return null;
            ReleaseNotes releaseNotes = new ReleaseNotes();
            if (jObject.ContainsKey("Version")) releaseNotes.Version = jObject["Version"].ToString();
            if (jObject.ContainsKey("Date")) releaseNotes.Date = jObject["Date"].ToString();
            if (jObject.ContainsKey("Desc")) releaseNotes.Desc = jObject["Desc"].ToString();
            if (jObject.ContainsKey("MarkDown")) releaseNotes.MarkDown = jObject["MarkDown"].ToString();
            if (jObject.ContainsKey("KeyWords") && jObject["KeyWords"] is JArray array)
            {
                releaseNotes.KeyWords = new Dictionary<string, string>();
                foreach (JObject item in array)
                {
                    if (!item.HasValues || !item.ContainsKey("Key") || !item.ContainsKey("Value")) continue;
                    string key = item["Key"].ToString();
                    string value = item["Value"].ToString();
                    releaseNotes.KeyWords.Add(key, value);
                }
            }

            metaData.ReleaseNotes = releaseNotes;
            return metaData;
        }
    }
}
