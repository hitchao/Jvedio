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
                Tag = tag.Replace(' ', GlobalVariable.Separator),
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

            Video result = new Video()
            {
                VID = id,
                VideoType = (VideoType)vediotype,
                Director = director,
                Studio = studio,
                Publisher = studio,
                Plot = plot,
                Outline = outline,
                Duration = runtime,
                SubSection = subsection,

                WebType = source.Replace("jav", ""),
                WebUrl = sourceurl,

                PreviewImagePaths = "*PicPath*/ExtraPic/" + id,
                ScreenShotPaths = "*PicPath*/ScreenShot/" + id,
                GifImagePath = "*PicPath*/Gif/" + $"{id}.jpg",
                BigImagePath = "*PicPath*/BigPic/" + $"{id}.jpg",
                SmallImagePath = "*PicPath*/SmallPic/" + $"{id}.jpg",
                ImageUrls = json,
            };
            return result;
        }

    }

}
