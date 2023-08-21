using Jvedio.Core.Enums;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "metadata_comic")]
    public class Comic : Picture
    {
        [TableId(IdType.AUTO)]
        public long CID { get; set; }


        public string Language { get; set; }

        public ComicType ComicType { get; set; }

        public string Artist { get; set; }

#pragma warning disable CS0108 // “Comic.Plot”隐藏继承的成员“Picture.Plot”。如果是有意隐藏，请使用关键字 new。
        public string Plot { get; set; }

#pragma warning restore CS0108 // “Comic.Plot”隐藏继承的成员“Picture.Plot”。如果是有意隐藏，请使用关键字 new。
#pragma warning disable CS0108 // “Comic.Outline”隐藏继承的成员“Picture.Outline”。如果是有意隐藏，请使用关键字 new。
        public string Outline { get; set; }

#pragma warning restore CS0108 // “Comic.Outline”隐藏继承的成员“Picture.Outline”。如果是有意隐藏，请使用关键字 new。
#pragma warning disable CS0108 // “Comic.PicCount”隐藏继承的成员“Picture.PicCount”。如果是有意隐藏，请使用关键字 new。
        public int PicCount { get; set; }

#pragma warning restore CS0108 // “Comic.PicCount”隐藏继承的成员“Picture.PicCount”。如果是有意隐藏，请使用关键字 new。
#pragma warning disable CS0108 // “Comic.PicPaths”隐藏继承的成员“Picture.PicPaths”。如果是有意隐藏，请使用关键字 new。
        public string PicPaths { get; set; }

#pragma warning restore CS0108 // “Comic.PicPaths”隐藏继承的成员“Picture.PicPaths”。如果是有意隐藏，请使用关键字 new。
        public string WebType { get; set; }

        public string WebUrl { get; set; }

#pragma warning disable CS0108 // “Comic.ExtraInfo”隐藏继承的成员“Picture.ExtraInfo”。如果是有意隐藏，请使用关键字 new。
        public string ExtraInfo { get; set; }
#pragma warning restore CS0108 // “Comic.ExtraInfo”隐藏继承的成员“Picture.ExtraInfo”。如果是有意隐藏，请使用关键字 new。

        public new MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }
    }
}
