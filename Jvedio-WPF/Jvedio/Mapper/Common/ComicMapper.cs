using Jvedio.Entity.Data;
using Jvedio.Mapper.BaseMapper;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;

namespace Jvedio.Mapper
{
    public class ComicMapper : BaseMapper<Comic>
    {
        public static string BASE_SQL =
            $" FROM metadata_comic JOIN metadata on metadata.DataID=metadata_comic.DataID ";


        public static string[] SelectFields = {
            "metadata.DataID",
            "CID",
            "metadata.Grade",
            "Title",
            "Path",
            "PicCount",
            "LastScanDate",
            "ReleaseDate",
            "PicPaths",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",
        };

        public static string[] SelectAllFields = {
            "metadata.DataID",
            "DBId",
            "Title",
            "Size",
            "Path",
            "Hash",
            "Country",
            "ReleaseDate",
            "ReleaseYear",
            "ViewCount",
            "DataType",
            "Rating",
            "RatingCount",
            "FavoriteCount",
            "Genre",
            "Grade",
            $"(select group_concat(LabelName,'{SuperUtils.Values.ConstValues.Separator}') from metadata_to_label where metadata_to_label.DataID=metadata.DataID) as Label",
            "ViewDate",
            "FirstScanDate",
            "LastScanDate",
            "CreateDate",
            "UpdateDate",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",

            "CID",
            "Language",
            "ComicType",
            "Artist",
            "Plot",
            "Outline",
            "PicCount",
            "PicPaths",
            "WebType",
            "WebUrl",
            "ExtraInfo",

        };

        public static List<string> SortDict = new List<string>()
        {
             "Size" ,
             "FirstScanDate" ,
             "LastScanDate" ,
             "Grade" ,
            "Title" ,
             "ViewCount" ,
            "ReleaseDate" ,
            "Rating" ,
        };

        public Comic SelectByID(long dataid)
        {
            SelectWrapper<Comic> wrapper = new SelectWrapper<Comic>();

            wrapper.Select(SelectAllFields).Eq("metadata.DataID", dataid);
            string sql = $"{wrapper.toSelect(false)} FROM metadata_comic " +
                        "JOIN metadata " +
                        "on metadata.DataID=metadata_comic.DataID " + wrapper.toWhere(false);
            List<Dictionary<string, object>> list = Select(sql);
            List<Comic> comics = ToEntity<Comic>(list, typeof(Comic).GetProperties(), false);
            if (comics != null && comics.Count > 0)
            {
                return comics[0];
            }
            return null;
        }
    }
}
