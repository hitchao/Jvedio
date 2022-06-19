using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Mapper
{
    public class ActorMapper : BaseMapper<ActorInfo>
    {

        public static string
            actor_join_sql = " join metadata_to_actor on metadata_to_actor.DataID=metadata.DataID " +
                             "JOIN actor_info on metadata_to_actor.ActorID=actor_info.ActorID ";





    }
}
