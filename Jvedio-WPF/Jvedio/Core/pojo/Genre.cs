using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.pojo
{
    /// <summary>
    /// 【按类别】中的分类
    /// </summary>
    public class Genre
    {
        public List<string> theme { get; set; }
        public List<string> role { get; set; }
        public List<string> clothing { get; set; }
        public List<string> body { get; set; }
        public List<string> behavior { get; set; }
        public List<string> playmethod { get; set; }
        public List<string> other { get; set; }
        public List<string> scene { get; set; }

        public Genre()
        {
            theme = new List<string>();
            role = new List<string>();
            clothing = new List<string>();
            body = new List<string>();
            behavior = new List<string>();
            playmethod = new List<string>();
            other = new List<string>();
            scene = new List<string>();
        }

    }
}
