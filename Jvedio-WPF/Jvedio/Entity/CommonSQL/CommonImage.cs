using Jvedio.Core.Enums;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity
{
    [Table(tableName: "common_images")]
    public class CommonImage : Serializable
    {
        [TableId(IdType.AUTO)]
        public long ImageID { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public PathType PathType { get; set; }

        public string Ext { get; set; }

        public long Size { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public string Url { get; set; }

        public string ExtraInfo { get; set; }

        public string Source { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }
    }
}
