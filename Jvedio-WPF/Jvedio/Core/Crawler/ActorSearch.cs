using System;

namespace Jvedio.Core.Crawler
{

    [Obsolete]
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
