﻿using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    [Table(tableName: "common_magnets")]
    public class CommonMagnet
    {
        [TableId(IdType.AUTO)]
        public long MagnetID { get; set; }
        public string Magnet { get; set; }
        public string TorrentUrl { get; set; }
        public string VID { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
        public string Releasedate { get; set; }
        public string Tag { get; set; }
        public int DownloadNumber { get; set; }
        public string ExtraInfo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }
    }
}
