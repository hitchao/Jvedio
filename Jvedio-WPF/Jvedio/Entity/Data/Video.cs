using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    public class Video : MetaData
    {
        public string VID { get; set; }
        public VideoType VideoType { get; set; }
        public string Director { get; set; }
        public string Country { get; set; }
        public string Studio { get; set; }
        public string Publisher { get; set; }
        public string Plot { get; set; }
        public string Outline { get; set; }
        public int Duration { get; set; }
        public string PreviewImages { get; set; }
        public string ExtraInfo { get; set; }



    }
}
