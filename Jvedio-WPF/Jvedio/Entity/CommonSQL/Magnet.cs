using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{

    [Table(tableName: "common_magnets")]
    public class Magnet
    {
        public Magnet()
        {
            Tags = new List<string>();
        }

        public Magnet(long DataID) : base()
        {
            this.DataID = DataID;
        }

        [TableId(IdType.AUTO)]
        public long MagnetID { get; set; }
        public string MagnetLink { get; set; }
        public string TorrentUrl { get; set; }
        public long DataID { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
        public string Releasedate { get; set; }
        public string Tag { get; set; }

        [TableField(false)]
        public List<string> Tags { get; set; }
        public int DownloadNumber { get; set; }
        public string ExtraInfo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }

    }
}
