using DynamicData.Annotations;
using Jvedio.Utils.Encrypt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Jvedio.Utils.Net;

namespace Jvedio
{
    // 明明很简单的东西，现在居然有点看不懂了

    /// <summary>
    /// 下载类，分为FC2和非FC2，前者同时下载2个，后者同时下载3个
    /// </summary>
    public class DownLoader
    {
        public static int SemaphoreNum = 3;
        public static int SemaphoreFC2Num = 1;
        public DownLoadState State = DownLoadState.DownLoading;
        public event EventHandler InfoUpdate;
        public event EventHandler MessageCallBack;
        private Semaphore Semaphore;
        private Semaphore SemaphoreFC2;
        public DownLoadProgress downLoadProgress;

        public bool enforce = false;//是否强制下载信息
        private bool Cancel { get; set; }
        public List<Movie> Movies { get; set; }

        public List<Movie> MoviesFC2 { get; set; }



        /// <summary>
        /// 初始化 DownLoader
        /// </summary>
        /// <param name="movies">非 FC2 影片</param>
        /// <param name="moviesFC2">FC2 影片</param>
        public DownLoader(List<Movie> movies, List<Movie> moviesFC2)
        {
            Movies = movies;
            MoviesFC2 = moviesFC2;
            Semaphore = new Semaphore(SemaphoreNum, SemaphoreNum);
            SemaphoreFC2 = new Semaphore(SemaphoreFC2Num, SemaphoreFC2Num);
            downLoadProgress = new DownLoadProgress() { lockobject = new object(), value = 0, maximum = Movies.Count + MoviesFC2.Count };//所有影片的进度
        }


        public DownLoader(List<Movie> _movies, List<Movie> _moviesFC2, bool force) : this(_movies, _moviesFC2)
        {
            enforce = force;
        }




        /// <summary>
        /// 取消下载
        /// </summary>
        public void CancelDownload()
        {
            Cancel = true;
            State = DownLoadState.Fail;
        }


        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartThread()
        {
            if (Movies.Count == 0 & MoviesFC2.Count == 0) { this.State = DownLoadState.Completed; return; }
            for (int i = 0; i < Movies.Count; i++)
            {
                Thread threadObject = new Thread(DownLoad);
                threadObject.Start(Movies[i]);
            }

            for (int i = 0; i < MoviesFC2.Count; i++)
            {
                Thread threadObject = new Thread(DownLoad);
                threadObject.Start(MoviesFC2[i]);
            }

            Console.WriteLine($"启动了{Movies.Count + MoviesFC2.Count}个线程");

        }




        private async void DownLoad(object o)
        {

            //下载信息
            Movie movie = o as Movie;
            if (movie.id.ToUpper().StartsWith("FC2"))
                SemaphoreFC2.WaitOne();
            else
                Semaphore.WaitOne();//阻塞
            if (Cancel || string.IsNullOrEmpty(movie.id))
            {
                if (movie.id.ToUpper().StartsWith("FC2"))
                    SemaphoreFC2.Release();
                else
                    Semaphore.Release();
                return;
            }

            //下载信息
            State = DownLoadState.DownLoading;
            if (movie.IsToDownLoadInfo() || enforce)
            {
                //满足一定条件才下载信息
                HttpResult httpResult = await MyNet.DownLoadFromNet(movie);
                if (httpResult != null)
                {
                    if (httpResult.Success)
                    {
                        InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = movie, progress = downLoadProgress.value, Success = httpResult.Success });//委托到主界面显示
                    }
                    else
                    {
                        string error = httpResult.Error != "" ? httpResult.Error : httpResult.StatusCode.ToStatusMessage();
                        MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($" {movie.id} {Jvedio.Language.Resources.DownloadMessageFailFor}：{error}"));
                    }
                }
            }
            DetailMovie dm = DataBase.SelectDetailMovieById(movie.id);

            if (dm == null)
            {
                if (movie.id.ToUpper().StartsWith("FC2"))
                    SemaphoreFC2.Release();
                else
                    Semaphore.Release();
                return;
            }

            if (!File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") || enforce)
            {
                await MyNet.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);//下载大图
            }



            //fc2 没有缩略图
            if (dm.id.IndexOf("FC2") >= 0)
            {
                //复制海报图作为缩略图
                if (File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") && !File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg"))
                {
                    FileHelper.TryCopyFile(BasePicPath + $"BigPic\\{dm.id}.jpg", BasePicPath + $"SmallPic\\{dm.id}.jpg");
                }

            }
            else
            {
                if (!File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg") || enforce)
                {
                    await MyNet.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id); //下载小图
                }
            }
            dm.smallimage = ImageProcess.GetBitmapImage(dm.id, "SmallPic");
            InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State });//委托到主界面显示
            dm.bigimage = ImageProcess.GetBitmapImage(dm.id, "BigPic");
            lock (downLoadProgress.lockobject) downLoadProgress.value += 1;//完全下载完一个影片
            InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State, Success = true });//委托到主界面显示
            Task.Delay(Delay.MEDIUM).Wait();//每个线程之间暂停
            //取消阻塞
            if (movie.id.ToUpper().IndexOf("FC2") >= 0)
                SemaphoreFC2.Release();
            else
                Semaphore.Release();
        }
    }


    public class DownLoadProgress
    {
        public double maximum = 0;
        public double value = 0;
        public object lockobject;

    }

    public enum DownLoadState
    {
        DownLoading,
        Completed,
        Pause,
        Fail
    }

    public class InfoUpdateEventArgs : EventArgs
    {
        public bool Success = false;
        public Movie Movie;
        public double progress = 0;
        public DownLoadState state;
    }

    public class MessageCallBackEventArgs : EventArgs
    {
        public string Message = "";
        public MessageCallBackEventArgs(string message = "")
        {
            Message = message;
        }
    }



    public class DownLoadActress
    {
        public event EventHandler MessageCallBack;
        public event EventHandler InfoUpdate;
        public DownLoadProgress downLoadProgress;
        private Semaphore Semaphore;
        private ProgressBarUpdate ProgressBarUpdate;
        private bool Cancel { get; set; }
        public DownLoadState State;

        public List<Actress> ActorList { get; set; }

        public DownLoadActress(List<Actress> actresses)
        {
            ActorList = actresses;
            Cancel = false;
            Semaphore = new Semaphore(3, 3);
            ProgressBarUpdate = new ProgressBarUpdate() { value = 0, maximum = 1 };
        }

        public void CancelDownload()
        {
            Cancel = true;
            State = DownLoadState.Fail;
        }

        public void BeginDownLoad()
        {
            if (ActorList.Count == 0) { this.State = DownLoadState.Completed; return; }


            //先根据 BusActress.sqlite 获得 id
            List<Actress> actresslist = new List<Actress>();
            foreach (Actress item in ActorList)
            {
                if (item != null && (item.smallimage == null || string.IsNullOrEmpty(item.birthday)))
                {
                    Actress actress = item;
                    MySqlite db = new MySqlite("BusActress");
                    if (string.IsNullOrEmpty(item.id))
                        actress.id = db.GetInfoBySql($"select id from censored where name='{item.name}'");
                    db.CloseDB();
                    if (string.IsNullOrEmpty(item.imageurl))
                    {
                        //TODO 没法解决演员头像骑兵和步兵的问题
                        //默认骑兵吧，反正拍过步兵的肯定也拍过骑兵
                        //拼接网址
                        actress.imageurl = $"{JvedioServers.Bus.Url}pics/actress/{item.id}_a.jpg";
                    }

                    actresslist.Add(actress);
                }
            }

            ProgressBarUpdate.maximum = actresslist.Count;
            for (int i = 0; i < actresslist.Count; i++)
            {
                Console.WriteLine("开始进程 " + i);
                Thread threadObject = new Thread(DownLoad);
                threadObject.Start(actresslist[i]);
            }
        }

        private async void DownLoad(object o)
        {

            Semaphore.WaitOne();
            Actress actress = o as Actress;
            if (Cancel | actress.id == "")
            {
                Semaphore.Release();
                return;
            }
            try
            {
                this.State = DownLoadState.DownLoading;

                //下载头像
                if (!string.IsNullOrEmpty(actress.imageurl))
                {
                    string url = actress.imageurl;
                    HttpResult streamResult = await new MyNet().DownLoadFile(url);
                    if (streamResult != null)
                    {
                        ImageProcess.SaveImage(actress.name, streamResult.FileByte, ImageType.ActorImage, url);
                        actress.smallimage = ImageProcess.GetBitmapImage(actress.name, "Actresses");
                    }

                }
                //下载信息
                bool success = false;
                success = await Task.Run(() =>
                {
                    Task.Delay(300).Wait();
                    return MyNet.DownLoadActress(actress.id, actress.name, callback: (message) => { MessageCallBack?.Invoke(this, new MessageCallBackEventArgs(message)); });
                });

                if (success) actress = DataBase.SelectInfoByActress(actress);
                ProgressBarUpdate.value += 1;
                InfoUpdate?.Invoke(this, new ActressUpdateEventArgs() { Actress = actress, progressBarUpdate = ProgressBarUpdate, state = State });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }

    public class ActressUpdateEventArgs : EventArgs
    {
        public Actress Actress;
        public ProgressBarUpdate progressBarUpdate;
        public DownLoadState state;
    }

    public class ProgressBUpdateEventArgs : EventArgs
    {
        public double value = 0;
        public double maximum = 0;
    }

    public class ProgressBarUpdate
    {
        public double value;
        public double maximum;
    }


    public class DownLoadInfo : INotifyPropertyChanged
    {
        public string id { get; set; }
        public double speed { get; set; }


        private double _progressbarvalue = 0;
        public double progressbarvalue
        {


            get { return _progressbarvalue; }

            set
            {
                _progressbarvalue = value;
                OnPropertyChanged();
            }

        }

        private double _progress;



        public double progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                progressbarvalue = (int)(value / maximum * 100);

                OnPropertyChanged();
            }
        }
        public double maximum { get; set; }

        private DownLoadState _state;
        public DownLoadState state
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
