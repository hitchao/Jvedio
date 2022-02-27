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
                Genre = genre,
                Tag = tag,
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
            dict.Add("actress", actressimageurl.Split(';'));
            dict.Add("smallimage", smallimageurl);
            dict.Add("bigimage", bigimageurl);
            dict.Add("extraimages", extraimageurl.Split(';'));
            string json = JsonConvert.SerializeObject(dict);

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

                WebType = source.Replace("jav", ""),
                WebUrl = sourceurl,

                PreviewImagePaths = Path.Combine(GlobalVariable.BasePicPath, "ExtraPic", id),
                ScreenShotPaths = Path.Combine(GlobalVariable.BasePicPath, "ScreenShot"),
                GifImagePath = Path.Combine(GlobalVariable.BasePicPath, "Gif", $"{id}.jpg"),
                BigImagePath = Path.Combine(GlobalVariable.BasePicPath, "BigPic", $"{id}.jpg"),
                SmallImagePath = Path.Combine(GlobalVariable.BasePicPath, "SmallPic", $"{id}.jpg"),
                ImageUrls = json,
            };
            return result;
        }

    }

}
