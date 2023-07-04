using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity
{
    [Table(tableName: "common_url_code")]
    public class UrlCode
    {
        [TableId(IdType.AUTO)]
        public long CodeId { get; set; }

        public string LocalValue { get; set; }

        public string RemoteValue { get; set; }

        public string WebType { get; set; }

        public string ValueType { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }
    }
}
