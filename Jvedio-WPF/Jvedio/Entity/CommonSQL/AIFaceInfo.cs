using Jvedio.Core.Enums;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;

namespace Jvedio.Entity.CommonSQL
{
    [Table(tableName: "common_ai_face")]

    public class AIFaceInfo
    {
        [TableId(IdType.AUTO)]
        public long AIID { get; set; }

        public int Age { get; set; }

        public float Beauty { get; set; }

        public string Expression { get; set; }

        public string FaceShape { get; set; }

        public Gender Gender { get; set; }

        public bool Glasses { get; set; }

        public string Race { get; set; }

        public string Emotion { get; set; }

        public bool Mask { get; set; }

        public string Platform { get; set; }

        public string ExtraInfo { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }
    }
}
