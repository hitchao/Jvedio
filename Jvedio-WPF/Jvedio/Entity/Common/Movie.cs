
using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Entity.Common;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.IO;
using SuperUtils.Values;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Xml;
using static Jvedio.App;

namespace Jvedio.Entity
{
    /// <summary>
    /// Jvedio 影片
    /// </summary>
    [Obsolete]
    public class Movie : IDisposable, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "属性"


        public string id { get; set; }

        public long DBId { get; set; }

        public long MVID { get; set; }

        private string _title;

        public string title {
            get { return _title; }

            set {
                _title = value;
                RaisePropertyChanged();
            }
        }

        public double filesize { get; set; }

        private string _filepath;

        public string filepath {
            get { return _filepath; }

            set {
                _filepath = value;
                RaisePropertyChanged();
            }
        }

        public bool hassubsection { get; set; }

        private string _subsection;

        public string subsection {
            get { return _subsection; }

            set {
                _subsection = value;
                string[] subsections = value.Split(';');
                if (subsections.Length >= 2) {
                    hassubsection = true;
                    subsectionlist = new List<string>();
                    foreach (var item in subsections) {
                        if (!string.IsNullOrEmpty(item))
                            subsectionlist.Add(item);
                    }
                }

                RaisePropertyChanged();
            }
        }

        public List<string> subsectionlist { get; set; }

        public string tagstamps { get; set; }

        public int vediotype { get; set; }

        public string scandate { get; set; }

        private string _releasedate;

        public string releasedate {
            get { return _releasedate; }

            set {
                DateTime dateTime = new DateTime(1970, 01, 01);
                DateTime.TryParse(value.ToString(), out dateTime);
                _releasedate = dateTime.ToString("yyyy-MM-dd");
            }
        }

        public int visits { get; set; }

        public string director { get; set; }

        public string genre { get; set; }


        /// <summary>
        /// 系列
        /// </summary>
        public string tag { get; set; }

        public string actor { get; set; }

        public string actorid { get; set; }

        public string studio { get; set; }

        public float rating { get; set; }

        public string chinesetitle { get; set; }

        public int favorites { get; set; }

        public string label { get; set; }

        public string plot { get; set; }

        public string outline { get; set; }

        public int year { get; set; }

        public int runtime { get; set; }

        public string country { get; set; }

        public int countrycode { get; set; }

        public string otherinfo { get; set; }

        public string sourceurl { get; set; }

        public string source { get; set; }

        public string actressimageurl { get; set; }

        public string smallimageurl { get; set; }

        public string bigimageurl { get; set; }

        public string extraimageurl { get; set; }

        private Uri _GifUri;

        public Uri GifUri {
            get {
                return _GifUri;
            }

            set {
                _GifUri = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _smallimage;

        public BitmapSource smallimage {
            get { return _smallimage; }

            set {
                _smallimage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _bigimage;

        public BitmapSource bigimage {
            get { return _bigimage; }

            set {
                _bigimage = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public Movie(string id)
        {
            this.id = id;
            title = string.Empty;
            filesize = 0;
            filepath = string.Empty;
            hassubsection = false;
            subsection = string.Empty;
            subsectionlist = new List<string>();
            tagstamps = string.Empty;
            vediotype = 1;
            scandate = string.Empty;
            visits = 0;
            releasedate = string.Empty;
            director = string.Empty;
            tag = string.Empty;
            runtime = 0;
            genre = string.Empty;
            actor = string.Empty;
            actorid = string.Empty;
            studio = string.Empty;
            rating = 0;
            chinesetitle = string.Empty;
            favorites = 0;
            label = string.Empty;
            plot = string.Empty;
            outline = string.Empty;
            year = 1970;
            runtime = 0;
            country = string.Empty;
            countrycode = 0;
            otherinfo = string.Empty;
            sourceurl = string.Empty;
            source = string.Empty;
            actressimageurl = string.Empty;
            smallimageurl = string.Empty;
            bigimageurl = string.Empty;
            extraimageurl = string.Empty;
            smallimage = MetaData.DefaultSmallImage;
            bigimage = MetaData.DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
        }

        public Movie() : this(string.Empty)
        {
        }

        public virtual void Dispose()
        {
            subsectionlist.Clear();
            smallimage = null;
            bigimage = null;
        }

        public bool IsToDownLoadInfo()
        {
            return this != null && (this.title == string.Empty || this.sourceurl == string.Empty || this.smallimageurl == string.Empty || this.bigimageurl == string.Empty);
        }

        public bool isNullMovie()
        {
            return
                string.IsNullOrEmpty(title) &&
                string.IsNullOrEmpty(id) &&
                string.IsNullOrEmpty(filepath) &&
                filesize == 0;
        }

        public static Movie GetInfoFromNfo(string path)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = null;
            try {
                doc.Load(path);
                rootNode = doc.SelectSingleNode("movie");
            } catch (Exception ex) {
                throw ex;
            }

            if (rootNode == null || rootNode.ChildNodes == null || rootNode.ChildNodes.Count == 0)
                return null;
            Movie movie = new Movie();
            foreach (XmlNode node in rootNode.ChildNodes) {
                if (node == null || string.IsNullOrEmpty(node.Name))
                    continue;
                NfoParse.Parse(ref movie, node.Name, node.InnerText);
            }

            // 对于 NFO ，只要没有 VID，就不导入
            if (string.IsNullOrEmpty(movie.id))
                return null;

            if (!string.IsNullOrEmpty(movie.id))
                movie.vediotype = JvedioLib.Security.Identify.GetVideoType(movie.id);

            // 扫描视频获得文件大小
            SetFileSize(path, ref movie);

            // 系列 tag
            string tagString = GetTagList(doc, "tag");
            if (movie.id.IndexOf("FC2") >= 0)
                movie.genre = tagString;
            else
                movie.tag = tagString;

            // 类别 genre
            movie.genre = GetTagList(doc, "genre");

            // 演员 actor
            movie.actor = FindAndJoinData(doc, new List<string>() { "actor/name" });

            // 演员头像地址
            movie.actressimageurl = FindAndJoinData(doc, new List<string>() { "actor/thumb" }, RenameConfig.DEFAULT_NULL_STRING);

            // fanart
            movie.extraimageurl = FindAndJoinData(doc, new List<string>() { "fanart/thumb" }, RenameConfig.DEFAULT_NULL_STRING);

            // 检查一下视频是否为空
            if (movie.isNullMovie())
                return null;
            return movie;
        }

        public static void SetFileSize(string path, ref Movie movie)
        {
            if (File.Exists(path)) {
                string fatherPath = new FileInfo(path).DirectoryName;
                List<string> files = DirHelper.GetFileList(fatherPath)?.ToList();
                if (files != null && files.Count > 0) {
                    var list = files.Where(arg => ScanTask.VIDEO_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();
                    if (list != null || list.Count != 0) {
                        if (list.Count == 1)
                            movie.filepath = list[0]; // 默认取第一个
                        else
                            movie.subsection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list); // 分段视频
                    } else {
                        // 如果没扫到视频仍然导入
                        Logger.Warn($"{LangManager.GetValueByKey("DataNotFoundWithNFO")} {path}");
                        movie.filepath = path;      // nfo 路径视为该资源路径
                    }
                }
            }
        }

        public static string GetTagList(XmlDocument doc, string tagName)
        {
            if (!NfoParse.CurrentNFOParse.ContainsKey(tagName))
                return "";
            NfoParse nfoParse = NfoParse.CurrentNFOParse[tagName];
            List<string> list = nfoParse.ParseValues.Select(arg => arg.Value).ToList();
            if (list == null || list.Count == 0)
                return "";

            return FindAndJoinData(doc, list);
        }

        public static string FindAndJoinData(XmlDocument doc, List<string> list, string addNull = "")
        {
            List<string> result = new List<string>();
            string value = "";
            foreach (string name in list) {
                XmlNodeList nodes = TrySelectNode(doc, $"/movie/{name}");
                if (nodes != null && nodes.Count > 0) {
                    foreach (XmlNode node in nodes) {
                        value = node.InnerText.Trim();

                        if (!string.IsNullOrEmpty(addNull) && string.IsNullOrEmpty(value))
                            value = addNull;
                        if (string.IsNullOrEmpty(value))
                            continue;
                        result.Add(value);
                    }
                }
            }
            return string.Join(ConstValues.SeparatorString, result);
        }

        public MetaData toMetaData()
        {
            MetaData result = new MetaData() {
                DBId = DBId,
                Title = title,
                Size = (long)filesize,
                Path = filepath,
                Hash = string.Empty,
                Country = country,
                ReleaseDate = releasedate,
                ReleaseYear = year,
                ViewCount = visits,
                DataType = DataType.Video,
                Rating = rating,
                RatingCount = 0,
                FavoriteCount = 0,
                Genre = string.Join(SuperUtils.Values.ConstValues.SeparatorString, genre.Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries)),
                Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, label.Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries)),
                Grade = favorites,
                ViewDate = string.Empty,
                FirstScanDate = scandate,
                LastScanDate = otherinfo,
            };
            return result;
        }

        private static XmlNodeList TrySelectNode(XmlDocument doc, string node)
        {
            try {
                return doc.SelectNodes(node);
            } catch (Exception ex) {
                Logger.Error(ex);
                return null;
            }
        }

        public Video toVideo()
        {
            MetaData data = toMetaData();
            var serializedParent = JsonConvert.SerializeObject(data);
            Video video = JsonUtils.TryDeserializeObject<Video>(serializedParent);
            if (video == null)
                video = new Video();
            video.VID = id;
            video.VideoType = (VideoType)vediotype;
            video.Series = tag;
            video.Director = director;
            video.Studio = studio;
            video.Publisher = studio;
            video.Plot = plot;
            video.Outline = outline;
            video.Duration = runtime;
            video.SubSection = subsection.Replace(';', SuperUtils.Values.ConstValues.Separator);
            video.WebType = source.Replace("jav", string.Empty).Replace("fc2adult", "fc2");
            video.WebUrl = sourceurl;

            // video.ImageUrls = json;   // 让 ImageUrls 为空，这样子导入旧的数据库后就会自动同步新信息
            video.ActorNames = actor;   // 演员
            video.ActorThumbs = string.IsNullOrEmpty(actressimageurl) ? new List<string>()
                : actressimageurl.Split(SuperUtils.Values.ConstValues.Separator).ToList();
            return video;
        }
    }
}
