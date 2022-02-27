using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.FileProcess
{
    public static class DirHelper
    {
        public static bool TryMoveDir(string source, string target)
        {
            try
            {
                Directory.Move(source, target);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

            return false;
        }
    }
}
