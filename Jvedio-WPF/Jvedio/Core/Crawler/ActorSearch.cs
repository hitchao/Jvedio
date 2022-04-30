using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public class ActorSearch
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public string Link { get; set; }
        public string Img { get; set; }
        public string Tag { get; set; }

        public ActorSearch(string name)
        {
            Name = name;
            ID = 0;
            Link = "";
            Img = "";
            Tag = "";
        }

        public ActorSearch() : this("") { }


    }
}
