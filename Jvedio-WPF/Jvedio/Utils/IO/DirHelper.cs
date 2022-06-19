using Jvedio.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Jvedio.Utils.IO
{
    public static class DirHelper
    {

        public static void TryCreateDirectory(string dir, Action<Exception> errorCallBack = null)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex);
                Logger.Error(ex);
            }

        }

        public static void TryCopy(string sourcePath, string targetPath)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

        }


        /// <summary>
        /// 扫描文件
        /// 
        /// </summary>
        /// <remarks>速度：2s 左右扫描完 50万个文件（8T）</remarks>
        /// <see cref="https://stackoverflow.com/a/2107294/13454100"/>
        /// <param name="fileSearchPattern"></param>
        /// <param name="rootFolderPath"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFileList(string rootFolderPath, string fileSearchPattern = "*.*",

            Action<Exception> callBack = null, Action<string> scanDir = null, CancellationTokenSource cts = null)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                if (cts != null && cts.IsCancellationRequested) break;
                rootFolderPath = pending.Dequeue();
                try
                {
                    tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    callBack?.Invoke(e);
                    continue;
                }

                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }

                try
                {
                    scanDir?.Invoke(rootFolderPath);
                    tmp = Directory.GetDirectories(rootFolderPath);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    callBack?.Invoke(ex);
                }
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
                if (Directory.Exists(source))
                    Directory.Move(source, target);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

            return false;
        }
        public static bool TryDelete(string dir, bool recursive = true)
        {
            try
            {
                Directory.Delete(dir, recursive);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

            return false;
        }

        public static string[] TryGetDirList(string dir)
        {
            try
            {
                return Directory.GetDirectories(dir);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return new string[0];
        }

        public static long getDirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += getDirSize(di);
            }
            return size;
        }
    }
}
