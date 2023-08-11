using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper.BaseMapper;
using System.Collections.Generic;

namespace Jvedio.Mapper
{
    public class TagStampMapper : BaseMapper<TagStamp>
    {
        public static string GetTagSql()
        {
            return "SELECT common_tagstamp.*,count(common_tagstamp.TagID) as Count from metadata_to_tagstamp " +
                "join common_tagstamp " +
                "on metadata_to_tagstamp.TagID=common_tagstamp.TagID " +
                "join metadata " +
                "on metadata.DataID=metadata_to_tagstamp.DataID " +
                $"where metadata.DBId={ConfigManager.Main.CurrentDBId} and metadata.DataType={0} " +
                "GROUP BY common_tagstamp.TagID;";
        }

        public List<TagStamp> GetAllTagStamp()
        {
            return MapperManager.tagStampMapper.SelectList();
        }
    }
}
