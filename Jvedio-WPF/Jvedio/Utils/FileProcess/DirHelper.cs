using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils
{
    public static class DirHelper
    {

        /// <summary>
        /// 扫描文件
        /// 
        /// </summary>
        /// <remarks>速度：2s 左右扫描完 50万个文件（8T）</remarks>
        /// <see cref="https://stackoverflow.com/a/2107294/13454100"/>
        /// <param name="fileSearchPattern"></param>
        /// <param name="rootFolderPath"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFileList(string rootFolderPath, string fileSearchPattern = "*.*", Action<Exception> callBack = null)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                try
                {
                    tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                }
                catch (UnauthorizedAccessException e)
                {
                    callBack?.Invoke(e);
                    continue;
                }
                catch (IOException e)
                {
                    callBack?.Invoke(e);
                    continue;
                }
                catch (Exception e)
                {
                    callBack?.Invoke(e);
                    continue;
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }
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
