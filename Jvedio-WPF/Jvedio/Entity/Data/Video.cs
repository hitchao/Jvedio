using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{

    [Table(tableName: "metadata_video")]
    public class Video
    {
        [TableId(IdType.AUTO)]
        public long MVID { get; set; }
        public long DataID { get; set; }
        public string VID { get; set; }
        public VideoType VideoType { get; set; }
        public string Director { get; set; }
        public string Studio { get; set; }
        public string Publisher { get; set; }
        public string Plot { get; set; }
        public string Outline { get; set; }
        public int Duration { get; set; }
        public string SubSection { get; set; }
        public string PreviewImagePaths { get; set; }
        public string ScreenShotPaths { get; set; }
        public string GifImagePath { get; set; }
        public string BigImagePath { get; set; }
        public string SmallImagePath { get; set; }
        public string ImageUrls { get; set; }

        public string WebType { get; set; }
        public string WebUrl { get; set; }
        public string ExtraInfo { get; set; }
    }
}
