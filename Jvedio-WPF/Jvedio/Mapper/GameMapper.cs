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
            "ReleaseDate",

            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",
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

    }
}
