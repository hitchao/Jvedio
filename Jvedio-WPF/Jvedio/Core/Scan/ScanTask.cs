using DynamicData.Annotations;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Utils.FileProcess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class ScanTask : ITask, INotifyPropertyChanged
    {

        public static string VIDEO_EXTENSIONS = "3g2,3gp,3gp2,3gpp,amr,amv,asf,avi,bdmv,bik,d2v,divx,drc,dsa,dsm,dss,dsv,evo,f4v,flc,fli,flic,flv,hdmov,ifo,ivf,m1v,m2p,m2t,m2ts,m2v,m4b,m4p,m4v,mkv,mp2v,mp4,mp4v,mpe,mpeg,mpg,mpls,mpv2,mpv4,mov,mts,ogm,ogv,pss,pva,qt,ram,ratdvd,rm,rmm,rmvb,roq,rpm,smil,smk,swf,tp,tpr,ts,vob,vp6,webm,wm,wmp,wmv,nfo";
        public static List<string> VIDEO_EXTENSIONS_LIST = VIDEO_EXTENSIONS.Split(',').ToList();


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

        public string[] FileExt { get; set; }

        public System.IO.SearchOption SearchOption { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ScanTask(List<string> scanPaths, List<string> filePaths)
        {
            if (scanPaths != null && scanPaths.Count > 0)
                ScanPaths = scanPaths.Where(arg => Directory.Exists(arg)).ToList();
            if (filePaths != null && filePaths.Count > 0)
                FilePaths = filePaths.Where(arg => File.Exists(arg)).ToList();

            if (ScanPaths == null) ScanPaths = new List<string>();
            if (FilePaths == null) FilePaths = new List<string>();

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

               Task.Delay(2000).Wait();

               try { CheckStatus(); }
               catch (TaskCanceledException ex)
               {
                   Console.WriteLine(ex.Message);
                   return;
               }
               // 处理
               foreach (var item in FilePaths)
               {

               }


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
                Status = TaskStatus.Canceled;
        }
    }
}
