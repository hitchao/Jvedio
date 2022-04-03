using Jvedio.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    /// <summary>
    /// 详情页面的 Jvedio 影片，多了预览图、类别、演员、标签
    /// </summary>
    public class DetailMovie : Movie
    {
        public DetailMovie() : base()
        {
            genrelist = new List<string>();
            actorlist = new List<Actress>();
            labellist = new List<string>();
            extraimagelist = new ObservableCollection<BitmapSource>();
            extraimagePath = new ObservableCollection<string>();
        }

        public override void Dispose()
        {
            genrelist.Clear();
            actorlist.Clear();
            labellist.Clear();
            extraimagelist.Clear();
            extraimagePath.Clear();
            base.Dispose();
        }


        public List<string> genrelist { get; set; }
        public List<Actress> actorlist { get; set; }

        private List<string> _labellist;
        public List<string> labellist
        {
            get
            {
                return _labellist;
            }
            set
            {
                _labellist = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BitmapSource> extraimagelist { get; set; }
        public ObservableCollection<string> extraimagePath { get; set; }



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
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(actressimageurl)) dict.Add("actress", actressimageurl.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            if (!string.IsNullOrEmpty(smallimageurl)) dict.Add("smallimage", smallimageurl);
            if (!string.IsNullOrEmpty(bigimageurl)) dict.Add("bigimage", bigimageurl);
            if (!string.IsNullOrEmpty(extraimageurl)) dict.Add("extraimages", extraimageurl.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            string json = "";
            if (dict.Count > 0) json = JsonConvert.SerializeObject(dict);

            Video video = (Video)toMetaData();
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
            video.WebType = source.Replace("jav", "");
            video.WebUrl = sourceurl;
            video.PreviewImagePath = "*PicPath*/ExtraPic/" + id;
            video.ScreenShotPath = "*PicPath*/ScreenShot/" + id;
            video.GifImagePath = "*PicPath*/Gif/" + $"{id}.gif";
            video.BigImagePath = "*PicPath*/BigPic/" + $"{id}.jpg";
            video.SmallImagePath = "*PicPath*/SmallPic/" + $"{id}.jpg";
            video.ImageUrls = json;

            return video;
        }

    }

}
