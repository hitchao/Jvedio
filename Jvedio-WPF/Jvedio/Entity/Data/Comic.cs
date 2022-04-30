using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity.Data
{

    [Table(tableName: "metadata_comic")]
    public class Comic : Picture
    {
        [TableId(IdType.AUTO)]
        public long CID { get; set; }
        public long DataID { get; set; }
        public string Language { get; set; }
        public ComicType ComicType { get; set; }
        public string Artist { get; set; }
        public string Plot { get; set; }
        public string Outline { get; set; }
        public int PicCount { get; set; }
        public string PicPaths { get; set; }
        public string WebType { get; set; }
        public string WebUrl { get; set; }
        public string ExtraInfo { get; set; }


        public new MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }


    }
}
