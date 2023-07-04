using Jvedio.Core.Enums;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.CommonSQL
{
    [Table(tableName: "common_association")]

    public class Association
    {
        [TableId(IdType.AUTO)]
        public long AID { get; set; }

        public long MainDataID { get; set; }

        public long SubDataID { get; set; }

        public AssociationType AssociationType { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }

        public Association(long mainDataID, long subDataID)
        {
            this.MainDataID = mainDataID;
            this.SubDataID = subDataID;
        }

        public Association()
        {
        }
    }
}
