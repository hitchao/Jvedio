using DynamicData.Annotations;
using Jvedio.Utils.Encrypt;
using Jvedio.Utils.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// 校验MD5并复制文件
    /// </summary>
    public class Upgrade
    {
        public event EventHandler UpgradeCompleted;
        public event EventHandler onProgressChanged;
        private ProgressBUpdateEventArgs DownLoadProgress;
        public bool StopUpgrade = false;

        public List<string> DownLoadList;
        public static string list_url = "https://hitchao.github.io/jvedioupdate/list";
        public static string file_url = "https://hitchao.github.io/jvedioupdate/File/";


        public void Start()
        {
            StopUpgrade = false;
            DownLoadFromGithub();
        }


        public void Stop()
        {
            StopUpgrade = true;
        }


        private async Task<bool> GetDownLoadList()
        {
            HttpResult httpResult = null;
            try { httpResult = await new MyNet().Http(list_url); }
            catch (TimeoutException ex)
            {
                Logger.LogN(list_url + " => " + ex.Message);
                return false;
            }
            if (httpResult == null || string.IsNullOrEmpty(httpResult.SourceCode)) return false;
            Dictionary<string, string> filemd5 = new Dictionary<string, string>();
            foreach (var item in httpResult.SourceCode.Split('\n'))
            {
                if (!string.IsNullOrEmpty(item) && item.IndexOf(' ') > 0)
                {
                    string[] info = item.Split(' ');
                    if (!filemd5.ContainsKey(info[0])) filemd5.Add(info[0], info[1]);
                }
            }
            List<string> filenamelist = filemd5.Keys.ToList();
            DownLoadList = new List<string>();
            filenamelist.ForEach(arg =>
            {
                string localfilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arg);
                if (File.Exists(localfilepath))
                {
                    //存在 => 校验
                    if (Encrypt.GetFileMD5(localfilepath) != filemd5[arg])
                    {
                        DownLoadList.Add(arg);//md5 不一致 ，下载
                    }
                }
                else
                {
                    DownLoadList.Add(arg); //不存在 =>下载
                }
            });
            return true;
        }


        private void WriteFile(byte[] filebyte, string savepath)
        {
            FileInfo fileInfo = new FileInfo(savepath);
            if (!Directory.Exists(fileInfo.Directory.FullName)) Directory.CreateDirectory(fileInfo.Directory.FullName);//创建文件夹
            try
            {
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(filebyte, 0, filebyte.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }


        private async void DownLoadFromGithub()
        {
            string temppath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            //新建临时文件夹
            if (!Directory.Exists(temppath)) Directory.CreateDirectory(temppath);
            await GetDownLoadList();
            DownLoadProgress = new ProgressBUpdateEventArgs();
            DownLoadProgress.maximum = DownLoadList.Count;
            foreach (var item in DownLoadList)
            {
                if (StopUpgrade) return;
                Console.WriteLine(item);
                string filepath = Path.Combine(temppath, item);
                if (!File.Exists(filepath))
                {
                    HttpResult streamResult = await new MyNet().DownLoadFile(file_url + item);
                    //写入本地
                    if (streamResult != null) WriteFile(streamResult.FileByte, filepath);
                }
                DownLoadProgress.value += 1;
                if (!StopUpgrade) onProgressChanged?.Invoke(this, DownLoadProgress);
            }

            //复制文件并覆盖 执行 cmd 命令
            UpgradeCompleted?.Invoke(this, null);
        }


    }

}
