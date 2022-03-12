using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity.CommonSQL
{

    [Table(tableName: "common_custom_list")]
    public class CustomList
    {

        [TableId(IdType.AUTO)]
        public long ListID { get; set; }
        public string ListName { get; set; }
        public long Count { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }
    }
}
