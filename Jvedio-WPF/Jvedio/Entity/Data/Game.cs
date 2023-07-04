using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.Data
{
    [Table(tableName: "metadata_game")]
    public class Game : MetaData
    {
        [TableId(IdType.AUTO)]
        public long GID { get; set; }

#pragma warning disable CS0108 // “Game.DataID”隐藏继承的成员“MetaData.DataID”。如果是有意隐藏，请使用关键字 new。
        public long DataID { get; set; }
#pragma warning restore CS0108 // “Game.DataID”隐藏继承的成员“MetaData.DataID”。如果是有意隐藏，请使用关键字 new。

        public string Branch { get; set; }

        public string OriginalPainting { get; set; }

        public string VoiceActors { get; set; }

        public string Play { get; set; }

        public string Music { get; set; }

        public string Singers { get; set; }

        public string Plot { get; set; }

        public string Outline { get; set; }

        public string ExtraName { get; set; }

        public string Studio { get; set; }

        public string Publisher { get; set; }

        public string WebType { get; set; }

        public string WebUrl { get; set; }

        public string ExtraInfo { get; set; }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }
    }
}
