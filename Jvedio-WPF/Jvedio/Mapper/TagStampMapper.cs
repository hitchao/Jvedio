using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class TagStampMapper : BaseMapper<TagStamp>
    {
        public static string AllTagSql = "SELECT common_tagstamp.*,count(common_tagstamp.TagID) as Count from metadata_to_tagstamp " +
                "join common_tagstamp " +
                "on metadata_to_tagstamp.TagID=common_tagstamp.TagID " +
                "join metadata " +
                "on metadata.DataID=metadata_to_tagstamp.DataID " +
                $"where metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                "GROUP BY common_tagstamp.TagID;";




        public List<TagStamp> getAllTagStamp()
        {
            return GlobalMapper.tagStampMapper.selectList();

        }
    }
}
