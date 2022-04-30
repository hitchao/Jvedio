using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            $"(select group_concat(LabelName,'{GlobalVariable.Separator}') from metadata_to_label where metadata_to_label.DataID=metadata.DataID) as Label",
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

        public static Dictionary<int, string> SortDict = new Dictionary<int, string>()
        {
            { 0, "Size" },
            { 1, "FirstScanDate" },
            { 2, "LastScanDate" },
            { 3, "Grade" },
            { 4, "Title" },
            { 5, "ViewCount" },
            { 6, "ReleaseDate" },
            { 7, "Rating" },
        };

        public Comic SelectByID(long dataid)
        {
            SelectWrapper<Comic> wrapper = new SelectWrapper<Comic>();

            wrapper.Select(SelectAllFields).Eq("metadata.DataID", dataid);
            string sql = $"{wrapper.toSelect(false)} FROM metadata_comic " +
                        "JOIN metadata " +
                        "on metadata.DataID=metadata_comic.DataID " + wrapper.toWhere(false);
            List<Dictionary<string, object>> list = select(sql);
            List<Comic> comics = toEntity<Comic>(list, typeof(Comic).GetProperties(), false);
            if (comics != null && comics.Count > 0)
            {
                return comics[0];
            }
            return null;
        }
    }
}
