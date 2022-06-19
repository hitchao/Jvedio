using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity.CommonSQL
{

    [Table(tableName: "app_configs")]
    public class AppConfig
    {

        [TableId(IdType.AUTO)]
        public long ConfigId { get; set; }
        public string ConfigName { get; set; }
        public string ConfigValue { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }
    }
}
