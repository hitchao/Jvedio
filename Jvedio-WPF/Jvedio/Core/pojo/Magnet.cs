using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.pojo
{
    public class Magnet
    {
        //  (id VARCHAR(50) PRIMARY KEY, link TEXT , title TEXT , size TEXT, releasedate VARCHAR(10) DEFAULT '1900-01-01', tag TEXT)";

        public Magnet() : this("") { }


        public Magnet(string id)
        {
            this.id = id;
            link = "";
            title = "";
            size = 0;
            releasedate = "1970-01-01";
            tag = new List<string>();
        }



        public string id { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        public double size { get; set; }
        public string releasedate { get; set; }
        public List<string> tag { get; set; }

    }
}
