using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    [Table(tableName: "actor_info")]
    public class ActorInfo
    {
        [TableId(IdType.AUTO)]
        public long ActorID { get; set; }
        public string ActorName { get; set; }
        public int NameFlag { get; set; }
        public string Country { get; set; }
        public string Nation { get; set; }
        public string BirthPlace { get; set; }
        public string Birthday { get; set; }
        public string BloodType { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public Gender Gender { get; set; }
        public string Hobby { get; set; }
        public char Cup { get; set; }
        public int Chest { get; set; }
        public int Waist { get; set; }
        public int Hipline { get; set; }
        public string WebType { get; set; }
        public string WebUrl { get; set; }
        public string ImagePath { get; set; }
        public string ExtraInfo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }

        [TableField(exist: false)]
        public long ImageID { get; set; }


        /// <summary>
        /// 出演的作品的数量
        /// </summary>
        [TableField(exist: false)]
        public long Count { get; set; }
    }
}
