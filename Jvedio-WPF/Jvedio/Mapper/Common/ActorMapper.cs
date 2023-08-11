using Jvedio.Entity;
using Jvedio.Mapper.BaseMapper;

namespace Jvedio.Mapper
{
    public class ActorMapper : BaseMapper<ActorInfo>
    {
        public const string SQL_JOIN_ACTOR =
            " JOIN metadata_to_actor on metadata_to_actor.DataID=metadata.DataID" +
            " JOIN actor_info on metadata_to_actor.ActorID=actor_info.ActorID ";
    }
}
