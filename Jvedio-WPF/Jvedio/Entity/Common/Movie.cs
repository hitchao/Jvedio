
using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using Jvedio.Core.Scan;
using JvedioLib.Security;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Jvedio.Entity
{
    /// <summary>
    /// Jvedio 影片
    /// </summary>
    public class Movie : IDisposable, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private const int DEFAULT_RELEASE_YEAR = 1970;

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

        public string id { get; set; }

        public long DBId { get; set; }

        public long MVID { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }

            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }

        public double filesize { get; set; }

        private string _filepath;

        public string filepath
        {
            get { return _filepath; }

            set
            {
                _filepath = value;
                RaisePropertyChanged();
            }
        }

        public bool hassubsection { get; set; }

        private string _subsection;

        public string subsection
        {
            get { return _subsection; }

            set
            {
                _subsection = value;
                string[] subsections = value.Split(';');
                if (subsections.Length >= 2)
                {
                    hassubsection = true;
                    subsectionlist = new List<string>();
                    foreach (var item in subsections)
                    {
                        if (!string.IsNullOrEmpty(item)) subsectionlist.Add(item);
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

        public string releasedate
        {
            get { return _releasedate; }

            set
            {
                DateTime dateTime = new DateTime(1970, 01, 01);
                DateTime.TryParse(value.ToString(), out dateTime);
                _releasedate = dateTime.ToString("yyyy-MM-dd");
            }
        }

        public int visits { get; set; }

        public string director { get; set; }

        public string genre { get; set; }

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

        public Uri GifUri
        {
            get
            {
                return _GifUri;
            }

            set
            {
                _GifUri = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _smallimage;

        public BitmapSource smallimage
        {
            get { return _smallimage; }

            set
            {
                _smallimage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _bigimage;

        public BitmapSource bigimage
        {
            get { return _bigimage; }

            set
            {
                _bigimage = value;
                RaisePropertyChanged();
            }
        }


        public static Movie GetInfoFromNfo(string path)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = null;
            try
            {
                doc.Load(path);
                rootNode = doc.SelectSingleNode("movie");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (rootNode == null || rootNode.ChildNodes == null || rootNode.ChildNodes.Count == 0) return null;
            Movie movie = new Movie();
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                try
                {
                    switch (node.Name)
                    {
                        case "id": movie.id = string.IsNullOrEmpty(node.InnerText) ? string.Empty : node.InnerText.ToUpper(); break;
                        case "num": movie.id = string.IsNullOrEmpty(node.InnerText) ? string.Empty : node.InnerText.ToUpper(); break;
                        case "title": movie.title = node.InnerText; break;
                        case "release": movie.releasedate = node.InnerText; break;
                        case "releasedate": movie.releasedate = node.InnerText; break;
                        case "director": movie.director = node.InnerText; break;
                        case "studio": movie.studio = node.InnerText; break;
                        case "rating": movie.rating = string.IsNullOrEmpty(node.InnerText) ? 0.0f : float.Parse(node.InnerText); break;
                        case "plot": movie.plot = node.InnerText; break;
                        case "outline": movie.outline = node.InnerText; break;
                        case "year": movie.year = string.IsNullOrEmpty(node.InnerText) ? DEFAULT_RELEASE_YEAR : int.Parse(node.InnerText); break;
                        case "runtime": movie.runtime = string.IsNullOrEmpty(node.InnerText) ? 0 : int.Parse(node.InnerText); break;
                        case "country": movie.country = node.InnerText; break;
                        case "source": movie.sourceurl = node.InnerText; break;
                        case "set": movie.tag = node.InnerText; break;
                        default: break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"{LangManager.GetValueByKey("ParseNfoInfoFailFromFile")} => {path}");
                    Logger.Error(ex);
                    continue;
                }
            }

            // 对于 NFO ，只要没有 VID，就不导入
            if (string.IsNullOrEmpty(movie.id)) return null;

            if (!string.IsNullOrEmpty(movie.id))
                movie.vediotype = Identify.GetVideoType(movie.id);

            // 扫描视频获得文件大小
            if (File.Exists(path))
            {
                string fatherpath = new FileInfo(path).DirectoryName;
                List<string> files = DirHelper.GetFileList(fatherpath)?.ToList();
                if (files != null && files.Count > 0)
                {
                    var list = files.Where(arg => ScanTask.VIDEO_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();
                    if (list != null || list.Count != 0)
                    {
                        if (list.Count == 1)
                            movie.filepath = list[0]; // 默认取第一个
                        else
                            movie.subsection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list); // 分段视频
                    }
                    else
                    {
                        // 如果没扫到视频仍然导入
                        Logger.Warning($"{LangManager.GetValueByKey("DataNotFoundWithNFO")} {path}");
                        movie.filepath = path;      // nfo 路径视为该资源路径
                    }
                }
            }

            string sep = SuperUtils.Values.ConstValues.SeparatorString;

            // tag
            XmlNodeList tagNodes = TrySelectNode(doc, "/movie/tag");
            List<string> tags = new List<string>();
            if (tagNodes != null && tagNodes.Count > 0)
            {
                foreach (XmlNode item in tagNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerText))
                        tags.Add(item.InnerText.Replace(" ", string.Empty));
                }

                if (movie.id.IndexOf("FC2") >= 0)
                    movie.genre = string.Join(sep, tags);
                else
                    movie.tag = string.Join(sep, tags);
            }

            // genre
            XmlNodeList genreNodes = TrySelectNode(doc, "/movie/genre");
            List<string> genres = new List<string>();
            if (genreNodes != null && genreNodes.Count > 0)
            {
                foreach (XmlNode item in genreNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerText))
                        genres.Add(item.InnerText);
                }

                movie.genre = string.Join(sep, genres);
            }

            // actor
            XmlNodeList actorNodes = TrySelectNode(doc, "/movie/actor/name");
            List<string> actors = new List<string>();
            if (actorNodes != null && actorNodes.Count > 0)
            {
                foreach (XmlNode item in actorNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerText))
                        actors.Add(item.InnerText);
                }

                movie.actor = string.Join(sep, actors);
            }

            // 演员头像地址
            XmlNodeList thumbNodes = TrySelectNode(doc, "/movie/actor/thumb");
            List<string> thumbs = new List<string>();
            if (thumbNodes?.Count > 0)
            {
                foreach (XmlNode item in thumbNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerText))
                        thumbs.Add(item.InnerText);
                    else
                        thumbs.Add(RenameConfig.DEFAULT_NULL_STRING);
                }

                movie.actressimageurl = string.Join(sep, thumbs);
            }

            // fanart
            XmlNodeList fanartNodes = TrySelectNode(doc, "/movie/fanart/thumb");
            List<string> extraimageurls = new List<string>();
            if (fanartNodes != null && fanartNodes.Count > 0)
            {
                foreach (XmlNode item in fanartNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerText))
                        extraimageurls.Add(item.InnerText);
                }

                movie.extraimageurl = string.Join(sep, extraimageurls);
            }

            // 检查一下视频是否为空
            if (movie.isNullMovie()) return null;
            return movie;
        }

        public MetaData toMetaData()
        {
            MetaData result = new MetaData()
            {
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
            try
            {
                return doc.SelectNodes(node);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public Video toVideo()
        {
            MetaData data = toMetaData();
            var serializedParent = JsonConvert.SerializeObject(data);
            Video video = JsonUtils.TryDeserializeObject<Video>(serializedParent);
            if (video == null) video = new Video();
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
