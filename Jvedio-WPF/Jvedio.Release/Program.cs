using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Release
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CopyAndRemoveFile();
        }

        public static void CopyAndRemoveFile()
        {
            string origin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Jvedio/bin/Debug");
            origin = Path.GetFullPath(origin);
            string target = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Release/public/File");
            target = Path.GetFullPath(origin);

            try
            {
                Directory.Delete(target, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            TryCopy(origin, target);

            // 删除文件
            DeleteFiles();
            // 生成 json
            GenerateMD5Json();
            Console.WriteLine("完成！");
            Console.ReadKey();
        }


        private static void DeleteFiles()
        {

            string basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Release/public/File");
            basePath = Path.GetFullPath(basePath);
            if (Directory.Exists(basePath))
            {
                DirectoryInfo folder = new DirectoryInfo(basePath);
                FileInfo[] fileList = folder.GetFiles();
                foreach (FileInfo file in fileList)
                {
                    if (file.Extension == ".xml" | file.Extension == ".ini" | file.Extension == ".application" | file.Extension == ".xml" | file.Extension == ".txt")
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                    }
                }
                List<string> deleteFilePaths = new List<string> { "AI.sqlite", "Info.sqlite", "Translate.sqlite", "OldVersion", "RecentWatch", "WindowConfig", "ServersConfig", "mylist.sqlite" };
                if (File.Exists("filestodelete.txt"))
                {
                    using (StreamReader sr = new StreamReader("filestodelete.txt"))
                    {
                        string content = sr.ReadToEnd();
                        var filenames = content.Split(Environment.NewLine.ToCharArray());

                        if (filenames.Length > 0)
                        {
                            foreach (var item in filenames)
                            {
                                deleteFilePaths.Add(item);
                            }
                        }
                        else
                        {
                            deleteFilePaths.Add(content);
                        }

                    }
                }




                foreach (var item in deleteFilePaths)
                {
                    if (File.Exists(basePath + item)) { File.Delete(basePath + item); }
                }
                string[] deleteDirPaths = new string[] { "app.publish", "BackUp", "DataBase", "log", "Pic", "data" };

                foreach (var item in deleteDirPaths)
                {
                    string p = Path.Combine(basePath, item);
                    if (Directory.Exists(p))
                    {
                        try
                        {
                            Directory.Delete(p, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                }

                string pluginPath = Path.Combine(basePath, "plugins", "crawlers");
                string[] plugins = Directory.GetFiles(pluginPath, "*.*");


                string[] retainPlugins = new string[] { "CommonNet.dll", "HtmlAgilityPack.dll" };
                foreach (var item in plugins)
                {
                    string fileName = Path.GetFileName(item);
                    if (!retainPlugins.Contains(fileName))
                    {
                        File.Delete(item);
                    }
                }
            }



        }


        private static void GenerateMD5Json()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Release/public/File/");
            path = Path.GetFullPath(path);
            if (Directory.Exists(path))
            {
                try
                {
                    Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

                    List<string> list1 = new List<string>();
                    List<string> list2 = new List<string>();

                    List<string> fileswithMD5 = new List<string>();

                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    if (files != null)
                    {
                        foreach (var item in files)
                        {
                            if (File.Exists(item))
                            {
                                FileInfo fileInfo = new FileInfo(item);

                                list1.Add(fileInfo.FullName.Replace(path, ""));
                                list2.Add(GetMD5(item));

                                fileswithMD5.Add(fileInfo.FullName.Replace(path, "") + " " + GetMD5(item));
                            }
                        }
                    }

                    dict.Add("FileName", list1);
                    dict.Add("FileHash", list2);

                    string value = JsonConvert.SerializeObject(dict);

                    string jsonPath = Path.GetFullPath(Path.Combine(path, "../list.json"));
                    using (var listfile = new StreamWriter(jsonPath, false))
                    {
                        listfile.Write(value);
                    }

                    // 老版本
                    string total = string.Join("\n", fileswithMD5);
                    string hashPath = Path.GetFullPath(Path.Combine(path, "../list"));
                    using (var listfile = new StreamWriter(hashPath, false))
                    {
                        listfile.Write(total);
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
        public static string GetMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
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
                Console.WriteLine(ex.Message);
            }

        }
    }
}
