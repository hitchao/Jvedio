using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity.CommonSQL
{

    [Table(tableName: "common_search_history")]
    public class SearchHistory
    {

        [TableId(IdType.AUTO)]
        public long id { get; set; }
        public string SearchValue { get; set; }
        public SearchField SearchField { get; set; }
        public string ExtraInfo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }
    }
}
