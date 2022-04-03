using DynamicData.Annotations;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class ScanTask : ITask, INotifyPropertyChanged
    {

        public static string VIDEO_EXTENSIONS = "3g2,3gp,3gp2,3gpp,amr,amv,asf,avi,bdmv,bik,d2v,divx,drc,dsa,dsm,dss,dsv,evo,f4v,flc,fli,flic,flv,hdmov,ifo,ivf,m1v,m2p,m2t,m2ts,m2v,m4b,m4p,m4v,mkv,mp2v,mp4,mp4v,mpe,mpeg,mpg,mpls,mpv2,mpv4,mov,mts,ogm,ogv,pss,pva,qt,ram,ratdvd,rm,rmm,rmvb,roq,rpm,smil,smk,swf,tp,tpr,ts,vob,vp6,webm,wm,wmp,wmv";
        public static List<string> VIDEO_EXTENSIONS_LIST = VIDEO_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();


        public TaskStatus _Status;
        public TaskStatus Status
        {

            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                if (STATUS_TO_TEXT_DICT.ContainsKey(value))
                    StatusText = STATUS_TO_TEXT_DICT[value];
                Running = value == TaskStatus.Running;
                OnPropertyChanged();
            }
        }


        public static Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {

            {TaskStatus.Running,"扫描中..."},
            {TaskStatus.Canceled,"已取消"},
            {TaskStatus.RanToCompletion,"已完成"},
        };

        public string _StatusText;
        public string StatusText
        {

            get
            {
                return _StatusText;
            }
            set
            {
                _StatusText = value;
                OnPropertyChanged();
            }
        }

        public bool _Running;
        public bool Running
        {

            get
            {
                return _Running;
            }
            set
            {
                _Running = value;
                OnPropertyChanged();
            }
        }


        public string _CreateTime;
        public string CreateTime
        {

            get
            {
                return _CreateTime;
            }
            set
            {
                _CreateTime = value;
                OnPropertyChanged();
            }
        }

        public Stopwatch stopwatch { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public List<string> ScanPaths { get; set; }
        public List<string> FilePaths { get; set; }

        public List<string> FileExt { get; set; }

        private CancellationTokenSource tokenCTS;
        private CancellationToken token;

        public System.IO.SearchOption SearchOption { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ScanTask(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            if (scanPaths != null && scanPaths.Count > 0)
                ScanPaths = scanPaths.Where(arg => Directory.Exists(arg)).ToList();
            if (filePaths != null && filePaths.Count > 0)
                FilePaths = filePaths.Where(arg => File.Exists(arg)).ToList();
            if (fileExt != null)
            {
                foreach (var item in fileExt)
                {
                    string ext = item.Trim();
                    if (string.IsNullOrEmpty(ext)) continue;
                    if (!item.StartsWith("."))
                        FileExt.Add("." + ext);
                    else
                        FileExt.Add(ext);
                }
            }


            if (ScanPaths == null) ScanPaths = new List<string>();
            if (FilePaths == null) FilePaths = new List<string>();
            if (FileExt == null) FileExt = VIDEO_EXTENSIONS_LIST;// 默认导入视频

            tokenCTS = new CancellationTokenSource();
            tokenCTS.Token.Register(() =>
            {
                Console.WriteLine("取消任务");
            });
            token = tokenCTS.Token;

            stopwatch = new Stopwatch();

        }



        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            Status = TaskStatus.Running;
            CreateTime = DateHelper.Now();
            try
            {
                doWrok();
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void doWrok()
        {

            Task.Run(() =>
           {
               stopwatch.Start();
               foreach (string path in ScanPaths)
               {
                   IEnumerable<string> paths = DirHelper.GetFileList(path, "*.*", (ex) =>
                   {

                   });
                   FilePaths.AddRange(paths);
               }

               try { CheckStatus(); }
               catch (TaskCanceledException ex)
               {
                   Console.WriteLine(ex.Message);
                   return;
               }
               ScanHelper scanHelper = new ScanHelper();
               (List<Video> import, List<string> notImport, List<string> failNFO) parseResult
               = scanHelper.parseMovie(FilePaths, FileExt, token, Properties.Settings.Default.ScanNfo);

               List<MetaData> metaDatas = parseResult.import.Select(arg => (MetaData)arg).ToList();
               List<Video> videos = parseResult.import;

               // 检查是否有重复



               GlobalMapper.metaDataMapper.executeNonQuery("BEGIN EXCLUSIVE TRANSACTION;");//设置排它锁
               GlobalMapper.metaDataMapper.insertBatch(metaDatas);
               GlobalMapper.videoMapper.insertBatch(videos);

               GlobalMapper.metaDataMapper.executeNonQuery("END TRANSACTION;");

               stopwatch.Stop();
               ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
               Status = TaskStatus.RanToCompletion;
           });
        }





        public void CheckStatus()
        {
            if (Status == TaskStatus.Canceled)
            {
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                throw new TaskCanceledException();
            }

        }

        public void Pause()
        {
            Status = TaskStatus.WaitingToRun;
        }

        public void Cancel()
        {
            if (Status == TaskStatus.Running)
            {
                Status = TaskStatus.Canceled;
                tokenCTS.Cancel();
            }
        }
    }
}
