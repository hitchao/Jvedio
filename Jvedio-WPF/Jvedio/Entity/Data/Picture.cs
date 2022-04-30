﻿using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity.Data
{

    [Table(tableName: "metadata_picture")]
    public class Picture : MetaData
    {
        [TableId(IdType.AUTO)]
        public long PID { get; set; }
        public long DataID { get; set; }
        public string Director { get; set; }
        public string Studio { get; set; }
        public string Publisher { get; set; }
        public string Plot { get; set; }
        public string Outline { get; set; }
        public int PicCount { get; set; }
        public string PicPaths { get; set; }
        public string VideoPaths { get; set; }
        public string ExtraInfo { get; set; }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }

        public Comic toSimpleComic()
        {
            Comic comic = new Comic();
            comic.DataID = this.DataID;
            comic.LastScanDate = this.LastScanDate;
            comic.FirstScanDate = this.FirstScanDate;
            comic.PicCount = this.PicCount;
            comic.PicPaths = this.PicPaths;
            comic.DBId = this.DBId;
            comic.DataType = DataType.Comics;
            comic.Title = this.Title;
            comic.Path = this.Path;
            comic.Size = this.Size;
            comic.Hash = this.Hash;
            return comic;
        }

    }
}
