using DynamicData.Annotations;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Utils;
using JvedioLib;
using Newtonsoft.Json;
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
    public class Movie : IDisposable
    {
        public Movie(string id)
        {
            this.id = id;
            title = "";
            filesize = 0;
            filepath = "";
            hassubsection = false;
            subsection = "";
            subsectionlist = new List<string>();
            tagstamps = "";
            vediotype = 1;
            scandate = "";
            visits = 0;
            releasedate = "";
            director = "";
            tag = "";
            runtime = 0;
            genre = "";
            actor = "";
            actorid = "";
            studio = "";
            rating = 0;
            chinesetitle = "";
            favorites = 0;
            label = "";
            plot = "";
            outline = "";
            year = 1970;
            runtime = 0;
            country = "";
            countrycode = 0;
            otherinfo = "";
            sourceurl = "";
            source = "";
            actressimageurl = "";
            smallimageurl = "";
            bigimageurl = "";
            extraimageurl = "";
            smallimage = GlobalVariable.DefaultSmallImage;
            bigimage = GlobalVariable.DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
        }
        public Movie() : this("") { }

        public virtual void Dispose()
        {
            subsectionlist.Clear();
            smallimage = null;
            bigimage = null;
        }


        public bool IsToDownLoadInfo()
        {
            return this != null && (this.title == "" || this.sourceurl == "" || this.smallimageurl == "" || this.bigimageurl == "");
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
        public string title { get { return _title; } set { _title = value; OnPropertyChanged(); } }
        public double filesize { get; set; }

        private string _filepath;
        public string filepath
        {
            get { return _filepath; }

            set
            {
                _filepath = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }

        }

        private BitmapSource _smallimage;
        public BitmapSource smallimage { get { return _smallimage; } set { _smallimage = value; OnPropertyChanged(); } }

        private BitmapSource _bigimage;
        public BitmapSource bigimage { get { return _bigimage; } set { _bigimage = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public static Movie GetInfoFromNfo(string path, long minFileSize = 0)
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
                Logger.LogE(ex);
                Console.WriteLine(ex.Message);
                return null;
            }
            if (rootNode == null || rootNode.ChildNodes == null || rootNode.ChildNodes.Count == 0) return null;
            Movie movie = new Movie();
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                try
                {
                    switch (node.Name)
                    {
                        case "id": movie.id = node.InnerText.ToUpper(); break;
                        case "num": movie.id = node.InnerText.ToUpper(); break;
                        case "title": movie.title = node.InnerText; break;
                        case "release": movie.releasedate = node.InnerText; break;
                        case "releasedate": movie.releasedate = node.InnerText; break;
                        case "director": movie.director = node.InnerText; break;
                        case "studio": movie.studio = node.InnerText; break;
                        case "rating": movie.rating = node.InnerText == "" ? 0 : float.Parse(node.InnerText); break;
                        case "plot": movie.plot = node.InnerText; break;
                        case "outline": movie.outline = node.InnerText; break;
                        case "year": movie.year = node.InnerText == "" ? 1970 : int.Parse(node.InnerText); break;
                        case "runtime": movie.runtime = node.InnerText == "" ? 0 : int.Parse(node.InnerText); break;
                        case "country": movie.country = node.InnerText; break;
                        case "source": movie.sourceurl = node.InnerText; break;
                        default: break;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            // 对于 NFO ，只要没有 VID，就不导入
            if (string.IsNullOrEmpty(movie.id)) return null;


            if (!string.IsNullOrEmpty(movie.id))
                movie.vediotype = Identify.GetVideoType(movie.id);
            //扫描视频获得文件大小
            if (File.Exists(path))
            {
                string fatherpath = new FileInfo(path).DirectoryName;
                List<string> files = DirHelper.GetFileList(fatherpath).ToList();


                if (files != null && files.Count > 0)
                {
                    var list = files.Where(arg => ScanTask.VIDEO_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();

                    if (list != null || list.Count != 0)
                    {
                        if (list.Count == 1)
                        {
                            movie.filepath = list[0];// 默认取第一个
                        }
                        else
                            movie.subsection = String.Join(GlobalVariable.Separator.ToString(), list); //分段视频
                    }
                    else
                    {
                        // 如果没扫到视频就不导入
                        return null;
                    }
                }



            }

            //tag
            XmlNodeList tagNodes = doc.SelectNodes("/movie/tag");
            List<string> tags = new List<string>();
            if (tagNodes != null)
            {
                foreach (XmlNode item in tagNodes)
                {
                    if (item.InnerText != "") { tags.Add(item.InnerText.Replace(" ", "")); }
                }
                if (movie.id.IndexOf("FC2") >= 0)
                    movie.genre = string.Join(" ", tags);
                else
                    movie.tag = string.Join(" ", tags);
            }

            //genre
            XmlNodeList genreNodes = doc.SelectNodes("/movie/genre");
            List<string> genres = new List<string>();
            if (genreNodes != null)
            {
                foreach (XmlNode item in genreNodes)
                {
                    if (item.InnerText != "") { genres.Add(item.InnerText); }

                }
                movie.genre = string.Join(" ", genres);
            }

            //actor
            XmlNodeList actorNodes = doc.SelectNodes("/movie/actor/name");
            List<string> actors = new List<string>();
            if (actorNodes != null)
            {
                foreach (XmlNode item in actorNodes)
                {
                    if (item.InnerText != "") { actors.Add(item.InnerText); }
                }
                movie.actor = string.Join(" ", actors);
            }

            //fanart
            XmlNodeList fanartNodes = doc.SelectNodes("/movie/fanart/thumb");
            List<string> extraimageurls = new List<string>();
            if (fanartNodes != null)
            {
                foreach (XmlNode item in fanartNodes)
                {
                    if (item.InnerText != "") { extraimageurls.Add(item.InnerText); }
                }
                movie.extraimageurl = string.Join(" ", extraimageurls);
            }

            // 检查一下视频是否为空
            if (movie.isNullMovie())
                return null;
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
                Hash = "",
                Country = country,
                ReleaseDate = releasedate,
                ReleaseYear = year,
                ViewCount = visits,
                DataType = DataType.Video,
                Rating = rating,
                RatingCount = 0,
                FavoriteCount = 0,
                Genre = genre.Replace(' ', GlobalVariable.Separator),

                Label = label.Replace(' ', GlobalVariable.Separator),
                Grade = favorites,
                ViewDate = "",
                FirstScanDate = scandate,
                LastScanDate = otherinfo,
            };
            return result;
        }
        public Video toVideo()
        {
            MetaData data = toMetaData();
            var serializedParent = JsonConvert.SerializeObject(data);
            Video video = JsonConvert.DeserializeObject<Video>(serializedParent);
            video.VID = id;
            video.VideoType = (VideoType)vediotype;
            video.Series = tag.Replace(' ', GlobalVariable.Separator);
            video.Director = director;
            video.Studio = studio;
            video.Publisher = studio;
            video.Plot = plot;
            video.Outline = outline;
            video.Duration = runtime;
            video.SubSection = subsection.Replace(';', GlobalVariable.Separator);
            video.WebType = source.Replace("jav", "").Replace("fc2adult", "fc2");
            video.WebUrl = sourceurl;
            //video.ImageUrls = json;   // 让 ImageUrls 为空，这样子导入旧的数据库后就会自动同步新信息
            return video;
        }




    }

}
