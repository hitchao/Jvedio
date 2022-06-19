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
            int v = ms;
            try
            {
                v = new Random().Next(-offset, offset);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }
            int result = ms + v;
            if (result < 0) result = ms;
            return result;
        }
    }
}
