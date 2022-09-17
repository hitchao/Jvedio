using SuperUtils.Framework.ORM.Attributes;

namespace Jvedio.Entity
{
    [Table(tableName: "common_transaltions")]
    public class Translation : Serilizable
    {
        [TableId(IdType.AUTO)]
        public int TransaltionID { get; set; }

        public string SourceLang { get; set; }

        public string TargetLang { get; set; }

        public string SourceText { get; set; }

        public string TargetText { get; set; }

        public string Platform { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }

        [TableField(false)]
        public string VID { get; set; }
    }
}
