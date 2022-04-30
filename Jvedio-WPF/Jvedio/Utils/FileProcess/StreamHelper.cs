using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.FileProcess
{
    public static class StreamHelper
    {
        public static string TryRead(string path)
        {
            if (!File.Exists(path)) return "";
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
                return "";
            }
        }

        public static bool TryWrite(string path, string content, bool overWrite = false)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path, overWrite))
                {
                    sw.Write(content);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
                return false;
            }
        }

    }
}
