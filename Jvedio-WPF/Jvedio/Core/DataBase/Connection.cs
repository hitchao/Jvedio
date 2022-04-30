using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.DataBase
{
    public class Connection
    {
        public static object WriteLock { get; set; }

        static Connection()
        {
            WriteLock = new object();
        }

    }
}
