using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using System;
using System.Collections.Generic;

namespace Jvedio.Entity
{
    [Table(tableName: "common_magnets")]
    public class Magnet
    {
        public Magnet()
        {
            Tags = new List<string>();
        }

        public Magnet(long dataID) : base()
        {
            this.DataID = dataID;
        }

        [TableId(IdType.AUTO)]
        public long MagnetID { get; set; }

        public string MagnetLink { get; set; }

        public string TorrentUrl { get; set; }

        public long DataID { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        public string Releasedate { get; set; }

        private string _Tag;

        public string Tag {
            get => _Tag;

            set {
                _Tag = value;
                Tags = new List<string>();
                if (!string.IsNullOrEmpty(value)) {
                    Tags.AddRange(value.Split(new char[] { ' ', SuperUtils.Values.ConstValues.Separator },
                        StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }

        [TableField(false)]
        public List<string> Tags { get; set; }

        public string VID { get; set; }

        public int DownloadNumber { get; set; }

        public string ExtraInfo { get; set; }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }
    }
}
