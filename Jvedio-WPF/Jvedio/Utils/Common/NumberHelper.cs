using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Common
{
    public static class NumberHelper
    {
        public static int generateRandomMS(int ms, int offset = 10 * 1000)
        {
            int result = ms + new Random().Next(-offset, offset);
            if (result < 0) result = ms;
            return result;
        }
    }
}
