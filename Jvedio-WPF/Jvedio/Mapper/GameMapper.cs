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
    public class GameMapper : BaseMapper<Game>
    {



        public static string BASE_SQL =
            $" FROM metadata_game JOIN metadata on metadata.DataID=metadata_game.DataID ";


        public static string[] SelectFields = {
            "metadata.DataID",
            "GID",
            "metadata.Grade",
            "Title",
            "Path",

            "LastScanDate",
            "BigImagePath",
            "ReleaseDate",

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

            "GID",
            "Branch",
            "OriginalPainting",
            "VoiceActors",
            "Play",
            "Music",
            "Singers",
            "Plot",
            "Outline",
            "ExtraName",
            "Studio",
            "Publisher",
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

        public Game SelectByID(long dataid)
        {
            SelectWrapper<Game> wrapper = new SelectWrapper<Game>();

            wrapper.Select(SelectAllFields).Eq("metadata.DataID", dataid);
            string sql = $"{wrapper.toSelect(false)} FROM metadata_game " +
                        "JOIN metadata " +
                        "on metadata.DataID=metadata_game.DataID " + wrapper.toWhere(false);
            List<Dictionary<string, object>> list = select(sql);
            List<Game> games = toEntity<Game>(list, typeof(Game).GetProperties(), false);
            if (games != null && games.Count > 0)
            {
                return games[0];
            }
            return null;
        }
    }
}
