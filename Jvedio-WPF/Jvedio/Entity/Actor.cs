using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    public class Actor : Actress
    {
        public Actor(String name = "") : base(name)
        {
            sex = 2;// 男演员
        }
    }
}
