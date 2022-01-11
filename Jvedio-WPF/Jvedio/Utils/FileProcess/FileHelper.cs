using Jvedio.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;

using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Jvedio
{
    public static class FileHelper
    {

        public static string[] TryGetAllFiles(string path,string pattern)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    return Directory.GetFiles(path, pattern,SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                }
            }
            return null;
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

        public static string SelectPath(Window window)
        {
            var dialog = new CommonOpenFileDialog();
            string result = "";
            dialog.Title = Jvedio.Language.Resources.ChooseDir;
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dialog.ShowHiddenItems = true;
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
                if (File.Exists(path))
                {
                    Process.Start("explorer.exe", "/select, \"" + path + "\"");
                    return true;
                }

                else
                {
                    if (token != "") HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{path}", token);
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
