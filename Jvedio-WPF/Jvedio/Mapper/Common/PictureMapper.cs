using Jvedio.Entity.Data;
using Jvedio.Mapper.BaseMapper;
using SuperUtils.Framework.ORM.Wrapper;
using System.Collections.Generic;

namespace Jvedio.Mapper
{
    public class PictureMapper : BaseMapper<Picture>
    {
        public static string BASE_SQL =
            $" FROM metadata_picture JOIN metadata on metadata.DataID=metadata_picture.DataID ";

        public static string[] SelectFields =
        {
            "metadata.DataID",
            "PID",
            "metadata.Grade",
            "Title",
            "Path",
            "PicCount",
            "LastScanDate",
            "ReleaseDate",
            "PicPaths",
            "VideoPaths",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",
        };

        public static string[] SelectAllFields =
        {
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

            "PID",

            "PicCount",
            "Director",
            "Studio",
            "Publisher",
            "Plot",
            "Outline",
            "PicPaths",
            "VideoPaths",
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

        public Picture SelectByID(long dataId)
        {
            SelectWrapper<Picture> wrapper = new SelectWrapper<Picture>();

            wrapper.Select(SelectAllFields).Eq("metadata.DataID", dataId);
            string sql = $"{wrapper.ToSelect(false)} FROM metadata_picture " +
                        "JOIN metadata " +
                        "on metadata.DataID=metadata_picture.DataID " + wrapper.ToWhere(false);
            List<Dictionary<string, object>> list = Select(sql);
            List<Picture> pictures = ToEntity<Picture>(list, typeof(Picture).GetProperties(), false);
            if (pictures != null && pictures.Count > 0) {
                return pictures[0];
            }

            return null;
        }
    }
}
