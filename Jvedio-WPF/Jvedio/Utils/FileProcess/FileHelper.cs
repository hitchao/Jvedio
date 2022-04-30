
using ChaoControls.Style;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio
{
    public static class FileHelper
    {



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

        public static void TryMoveFile(string originPath, string targetPath, int delay)
        {
            Task.Run(() =>
            {
                Thread.Sleep(delay * 1000);
                TryMoveFile(originPath, targetPath);
            });
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

        public static string TryLoadFile(string path)
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

        public static bool TryDeleteFile(string path)
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
                    Logger.LogF(ex);
                }
            }
            return false;
        }

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
            {
                result = dialog.FileName;
            }
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

        public static bool TryOpenUrl(string url, string token = "")
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
                    if (token != "") HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.ErrorUrl, token);
                    return false;
                }

            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogE(ex);
                return false;
            }
        }

        public static bool TryOpenPath(string path, string token = "")
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
                    if (token != "") HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, token);
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenSelectPath(string path, string token = "")
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
                        //if (token != "") HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{path}", token);
                        MessageCard.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{path}");
                        return false;
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
                        return false;
                    }
                }


            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenFile(string filename, string token = "")
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
                    if (token != "") HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{filename}", token);
                    return false;
                }


            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenFile(string processPath, string filename, string token)
        {
            try
            {
                if (File.Exists(filename))
                {
                    string cmdCommand = $"\"{processPath}\" \"{filename}\" && pause";
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                        process.Start();
                        process.StandardInput.WriteLine(cmdCommand);
                        process.WaitForExit(1000);
                    }
                    return true;
                }

                else
                {
                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{filename}", token);
                    return false;
                }

            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }
    }
}
