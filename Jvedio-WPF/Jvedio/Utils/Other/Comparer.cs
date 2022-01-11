using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class  Comparer
    {

        public class RatingComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return Comparer<double>.Default.Compare(double.Parse(x.Split('-').First()), double.Parse(y.Split('-').First()));

            }
        }

    }
}
