using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class VideoMapper : BaseMapper<Video>
    {


        public static string[] SelectFields = {
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

            "VID",
            "MVID",
            "VideoType",
            "Series",
            "Director",
            "Studio",
            "Publisher",
            "Plot",
            "Outline",
            "Duration",
            "SubSection",
            "ImageUrls",
            "WebType",
            "WebUrl",
        };

        public Video SelectVideoByID(long dataid)
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();

            wrapper.Select(SelectFields).Eq("metadata.DataID", dataid);
            string sql = $"{wrapper.toSelect(false)} FROM metadata_video " +
                        "JOIN metadata " +
                        "on metadata.DataID=metadata_video.DataID " + wrapper.toWhere(false);
            List<Dictionary<string, object>> list = select(sql);
            List<Video> videos = toEntity<Video>(list, typeof(Video).GetProperties(), false);
            if (videos != null && videos.Count > 0)
            {
                Video video = videos[0];
                string actor_sql = "select actor_info.* from actor_info " +
                    "JOIN metadata_to_actor on metadata_to_actor.ActorID=actor_info.ActorID " +
                    "join metadata_video on metadata_video.DataID=metadata_to_actor.DataID " +
                    $"where metadata_video.DataID={dataid};";
                List<Dictionary<string, object>> actor_list = select(actor_sql);
                List<ActorInfo> actorInfos = toEntity<ActorInfo>(actor_list, typeof(ActorInfo).GetProperties());
                video.ActorInfos = actorInfos;
                return video;
            }
            return null;
        }


        public void SaveActor(Video video, List<ActorInfo> newActorInfos)
        {
            List<ActorInfo> oldActorInfos = video.ActorInfos;
            if (oldActorInfos == null) oldActorInfos = new List<ActorInfo>();
            if (newActorInfos == null) newActorInfos = new List<ActorInfo>();
            if (newActorInfos.SequenceEqual(oldActorInfos)) return;
            // 删除，新增
            oldActorInfos = oldActorInfos.OrderBy(arg => arg.ActorID).ToList();
            newActorInfos = newActorInfos.OrderBy(arg => arg.ActorID).ToList();
            List<ActorInfo> to_delete = oldActorInfos.Except(newActorInfos).ToList();
            List<ActorInfo> to_create = newActorInfos.Except(oldActorInfos).ToList();

            //删除
            if (to_delete.Count > 0)
            {
                // ('1','2','3')
                string sql = $"delete from metadata_to_actor " +
                    $"where DataID={video.DataID} " +
                    $"and ActorID in ('{string.Join("','", to_delete.Select(arg => arg.ActorID))}')";
                executeNonQuery(sql);
            }

            // 新增
            if (to_create.Count > 0)
            {
                List<string> create = new List<string>();
                to_create.ForEach(arg =>
                {
                    create.Add($"({video.DataID},{arg.ActorID})");
                });

                string sql = $"insert or ignore into metadata_to_actor(DataID,ActorID) " +
                    $"values {string.Join(",", create)}";
                executeNonQuery(sql);
            }
        }

        public void deleteVideoByIds(List<string> idList)
        {
            if (idList == null || idList.Count == 0) return;
            StringBuilder builder = new StringBuilder();
            string ids = string.Join(",", idList);
            builder.Append("begin;");
            builder.Append($"delete from metadata where DataID in ({ids});");
            builder.Append($"delete from metadata_video where DataID in ({ids});");
            builder.Append($"delete from metadata_to_translation where DataID in ({ids});");
            builder.Append($"delete from metadata_to_tagstamp where DataID in ({ids});");
            builder.Append($"delete from metadata_to_actor where DataID in ({ids});");
            builder.Append($"delete from metadata_to_label where DataID in ({ids});");
            builder.Append("commit;");
            GlobalMapper.videoMapper.executeNonQuery(builder.ToString());
        }


        public static string BASE_SQL =
            $" FROM metadata_video JOIN metadata on metadata.DataID=metadata_video.DataID ";

        public static string ACTOR_JOIN_SQL =
            " join metadata_to_actor on metadata_to_actor.DataID=metadata.DataID " +
               "JOIN actor_info on metadata_to_actor.ActorID=actor_info.ActorID ";

        public static string LABEL_JOIN_SQL =
            " join metadata_to_label on metadata_to_label.DataID=metadata.DataID ";

        public static string TAGSTAMP_JOIN_SQL = " join metadata_to_tagstamp on metadata_to_tagstamp.DataID=metadata.DataID ";
    }
}
