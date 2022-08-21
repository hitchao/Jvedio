
using SuperControls.Style;
using Jvedio.CommonNet;
using Jvedio.Logs;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Utils.IO
{
    public static class FileHelper
    {



        public static string FindWithExt(string path, List<string> exts)
        {
            foreach (var item in exts)
            {
                path = ChangeExt(path, item);
                if (File.Exists(path)) break;
            }
            return path;
        }


        private static string ChangeExt(string path, string ext)
        {
            if (string.IsNullOrEmpty(path)) return "";
            string dir = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            return System.IO.Path.Combine(dir, name + ext);
        }


        public static string TryGetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            try
            {
                return Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return "";

        }

        public static long TryGetFileLength(string path)
        {
            try
            {
                return new FileInfo(path).Length;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return 0;
            }
        }

        public static string TryGetFullName(string path)
        {
            try
            {
                return new FileInfo(path).Directory.FullName;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static bool ByteArrayToFile(byte[] byteArray, string fileName, Action<string> errorCallBack)
        {
            if (byteArray == null) return false;
            try
            {
                // todo 这里仍然会抛出异常
                // System.NullReferenceException:“未将对象引用设置到对象的实例。”
                // byteArray 是 null。
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                errorCallBack.Invoke(ex.Message);
            }
            return false;
        }


        /// <summary>
        /// 判断拖入的是文件夹还是文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFile(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return false;
                else
                    return true;
            }
            catch
            {
                return true;
            }

        }


        public static void TryWriteToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }


        public static bool TryMoveFile(string originPath, string targetPath)
        {

            try
            {
                File.Move(originPath, targetPath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return false;
        }

        public static string[] TryScanDIr(string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                if (Directory.Exists(path) && !string.IsNullOrEmpty(searchPattern))
                {
                    return Directory.GetFiles(path, searchPattern, searchOption);
                }

            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return null;
        }

        public static string[] TryGetAllFiles(string path, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    return Directory.GetFiles(path, pattern, option);
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                }
            }
            return new string[0];
        }

        public static string TryReadFile(string path)
        {
            if (File.Exists(path))
            {
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
                }
            }
            return "";
        }

        public static bool TryCreateDir(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                }
            }
            return false;
        }




        public static bool TryDeleteDir(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                }
            }
            return false;
        }

        public static void TryMoveToRecycleBin(string path, int delay)
        {
            Task.Run(() =>
            {
                Thread.Sleep(delay * 1000);
                TryMoveToRecycleBin(path);
            });
        }

        public static bool TryMoveToRecycleBin(string path)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return false;
        }

        public static bool TryDeleteFile(string path, Action<string> errorCallBack = null)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return true;
                }
                catch (Exception ex)
                {
                    errorCallBack?.Invoke(ex.Message);
                    Logger.LogF(ex);
                }
            }
            return false;
        }


        // todo 导致窗口置底了
        public static string SelectPath(Window window, string InitialDirectory = null)
        {

            var dialog = new CommonOpenFileDialog();
            string result = "";
            dialog.Title = Jvedio.Language.Resources.ChooseDir;
            dialog.IsFolderPicker = true;
            if (!string.IsNullOrEmpty(InitialDirectory))
                dialog.InitialDirectory = InitialDirectory;
            dialog.ShowHiddenItems = true;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                result = dialog.FileName;

            //这个窗口会将之前的窗口置底，所以要置顶回来
            window.Activate();
            window.Topmost = true;
            window.Topmost = false;
            window.Focus();
            return result;
        }




        public static bool TryCopyFile(string src, string target, bool overwrite = false)
        {
            try
            {
                File.Copy(src, target, overwrite);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenUrl(string url, Action<string> callBack = null)
        {
            try
            {
                if (url.IsProperUrl())
                {
                    Process.Start(url);
                    return true;
                }
                else
                {
                    callBack?.Invoke(Jvedio.Language.Resources.ErrorUrl);
                    Logger.Error(Jvedio.Language.Resources.ErrorUrl);
                }

            }
            catch (Exception ex)
            {
                callBack?.Invoke(ex.Message);
                Logger.Error(ex);
            }

            return false;
        }

        public static bool TryOpenPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                    return true;
                }
                else
                {
                    MessageCard.Error($"{Jvedio.Language.Resources.NotExists}：{path}");

                }
            }
            catch (Exception ex)
            {
                MessageCard.Error(ex.Message);
                Logger.Error(ex);
            }

            return false;
        }

        public static bool TryOpenSelectPath(string path)
        {
            try
            {
                if (IsFile(path))
                {
                    if (File.Exists(path))
                    {
                        Process.Start("explorer.exe", "/select, \"" + path + "\"");
                        return true;
                    }
                    else
                    {
                        MessageCard.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{path}");
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        Process.Start("explorer.exe", " \"" + path + "\"");
                        return true;
                    }
                    else
                    {
                        MessageCard.Error($"{Jvedio.Language.Resources.NotExists}：{path}");

                    }
                }


            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return false;
        }

        public static bool TryOpenFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Process.Start("\"" + filename + "\"");
                    return true;
                }

                else
                {
                    MessageCard.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{filename}");
                }


            }
            catch (Exception ex)
            {
                MessageCard.Error(ex.Message);
                Logger.LogF(ex);

            }
            return false;
        }



        public static bool TryOpenFile(string processPath, string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = processPath,
                            Arguments = filename,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            // 重定向输出会导致打开视频很慢
                            //RedirectStandardOutput = true,
                            //StandardErrorEncoding = Encoding.UTF8,
                            //StandardOutputEncoding = Encoding.UTF8,
                            //RedirectStandardError = true
                        },
                        EnableRaisingEvents = true

                    };
                    process.Start();
                    process.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return false;
        }
    }
}
