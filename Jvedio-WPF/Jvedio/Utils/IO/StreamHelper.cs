using Jvedio.Core.Logs;
using System;
using System.IO;
using System.Text;

namespace Jvedio.Utils.IO
{
    public static class StreamHelper
    {

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
