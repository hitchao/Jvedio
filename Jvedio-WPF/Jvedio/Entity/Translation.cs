using Jvedio.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    public class Translation
    {
        /**
    TransaltionID INTEGER PRIMARY KEY autoincrement,

    SourceLang VARCHAR(100),
    TargetLang VARCHAR(100),
    SourceText TEXT,
    TargetText TEXT,
    Platform VARCHAR(100),
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
         * 
         **/


        [TableId(Core.Enum.IdType.AUTO)]
        public int TransaltionID { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
        public string SourceText { get; set; }
        public string TargetText { get; set; }
        public string Platform { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }

        public override string ToString()
        {
            return "TransaltionID = " + TransaltionID + ", SourceLang = " + SourceLang + ", TargetLang = " + TargetLang +
                ", SourceText = " + SourceText + ", TargetText = " + TargetText +
                ", Platform = " + Platform + ", CreateDate = " + CreateDate + ", UpdateDate = " + UpdateDate;
        }
    }
}
