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

        public static bool TryWrite(string path, string content, bool overWrite = false, Encoding encoding = null)
        {
            // 必须是无 BOM 编码，bat 文件才能运行
            if (encoding == null) encoding = new UTF8Encoding(false);
            try
            {
                using (StreamWriter sw = new StreamWriter(path, overWrite, encoding))
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
