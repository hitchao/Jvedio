using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
