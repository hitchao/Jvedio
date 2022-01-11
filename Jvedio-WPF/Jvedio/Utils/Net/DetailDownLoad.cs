using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Jvedio.Utils.Net;

namespace Jvedio
{

    /// <summary>
    /// 详情信息的下载
    /// </summary>
    public class DetailDownLoad
    {
        public event EventHandler MessageCallBack;
        public event EventHandler SmallImageDownLoadCompleted;
        public event EventHandler BigImageDownLoadCompleted;
        public event EventHandler InfoDownloadCompleted;
        public event EventHandler ExtraImageDownLoadCompleted;
        public event EventHandler InfoUpdate;
        public event EventHandler CancelEvent;
        private CancellationTokenSource cts; //线程 Token
        private object lockobject;
        private double Maximum;
        private double Value;
        private DetailMovie DetailMovie;
        public bool IsDownLoading = false;




        public DetailDownLoad(DetailMovie detailMovie)
        {
            Value = 0;
            Maximum = 1;
            DetailMovie = detailMovie;
            cts = new CancellationTokenSource();
            cts.Token.Register(() => { Console.WriteLine("取消当前同步任务"); });
            lockobject = new object();
            IsDownLoading = false;
        }

        public void CancelDownload()
        {
            cts.Cancel();
            IsDownLoading = false;
        }


        public async void DownLoad()
        {
            IsDownLoading = true;
            //下载信息
            if (DetailMovie.IsToDownLoadInfo())
            {
                HttpResult httpResult = await MyNet.DownLoadFromNet(DetailMovie);
                if (httpResult != null && !httpResult.Success)
                {
                    string error = httpResult.Error != "" ? httpResult.Error : httpResult.StatusCode.ToStatusMessage();
                    MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($" {DetailMovie.id} {Jvedio.Language.Resources.DownloadMessageFailFor}：{error}"));

                }

            }
            DetailMovie dm = DataBase.SelectDetailMovieById(DetailMovie.id);
            if (dm == null || string.IsNullOrEmpty(dm.title))
            {
                InfoUpdate?.Invoke(this, new DetailMovieEventArgs() { DetailMovie = dm, value = 1, maximum = 1 });
                return;
            }
            InfoUpdate?.Invoke(this, new DetailMovieEventArgs() { DetailMovie = dm, value = Value, maximum = Maximum });
            InfoDownloadCompleted?.Invoke(this, new MessageCallBackEventArgs(DetailMovie.id));

            if (!File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg")) DownLoadBigPic(dm); //下载大图
            if (!File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg")) DownLoadSmallPic(dm); //下载小图

            List<string> urlList = new List<string>();
            foreach (var item in dm.extraimageurl?.Split(';')) { if (!string.IsNullOrEmpty(item)) urlList.Add(item); }
            Maximum = urlList.Count() == 0 ? 1 : urlList.Count;

            DownLoadExtraPic(dm);//下载预览图
        }

        private async void DownLoadSmallPic(DetailMovie dm)
        {
            HttpStatusCode sc = HttpStatusCode.Forbidden;
            if (dm.smallimageurl != "")
            {
                (bool success, string cookie) = await Task.Run(() => { return MyNet.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id, callback: (statuscode) => { sc = (HttpStatusCode)statuscode; }); });
                if (success) SmallImageDownLoadCompleted?.Invoke(this, new MessageCallBackEventArgs(dm.id));
                else MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($"{Jvedio.Language.Resources.DownloadSPicFailFor} {sc.ToStatusMessage()} {Jvedio.Language.Resources.Message_ViewLog}"));
            }
        }


        private async void DownLoadBigPic(DetailMovie dm)
        {
            HttpStatusCode sc = HttpStatusCode.Forbidden;
            if (dm.bigimageurl != "")
            {
                (bool success, string cookie) = await Task.Run(() =>
                {
                    return MyNet.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id, callback: (statuscode) => { sc = (HttpStatusCode)statuscode; });
                });
                if (success) BigImageDownLoadCompleted?.Invoke(this, new MessageCallBackEventArgs(dm.id));
                else MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($"{Jvedio.Language.Resources.DownloadBPicFailFor} {sc.ToStatusMessage()} {Jvedio.Language.Resources.Message_ViewLog}"));

            }
            InfoUpdate?.Invoke(this, new DetailMovieEventArgs() { DetailMovie = dm, value = Value, maximum = Maximum });
        }




        private async void DownLoadExtraPic(DetailMovie dm)
        {
            List<string> urlList = dm.extraimageurl?.Split(';').Where(arg => arg.IsProperUrl()).ToList();
            bool dlimageSuccess = false; string cookies = "";
            for (int i = 0; i < urlList.Count(); i++)
            {
                HttpStatusCode sc = HttpStatusCode.Forbidden;
                if (cts.IsCancellationRequested) { CancelEvent?.Invoke(this, EventArgs.Empty); break; }
                string filepath = "";
                filepath = BasePicPath + "ExtraPic\\" + dm.id + "\\" + Path.GetFileName(new Uri(urlList[i]).LocalPath);
                if (!File.Exists(filepath))
                {
                    (dlimageSuccess, cookies) = await Task.Run(() => { return MyNet.DownLoadImage(urlList[i], ImageType.ExtraImage, dm.id, Cookie: cookies, callback: (statuscode) => { sc = (HttpStatusCode)statuscode; }); });
                    if (dlimageSuccess)
                    {
                        ExtraImageDownLoadCompleted?.Invoke(this, new MessageCallBackEventArgs(filepath));
                        if (urlList[i].IndexOf("dmm") > 0)
                            Thread.Sleep(Delay.MEDIUM);
                        else
                            Thread.Sleep(Delay.SHORT_3);
                    }
                    else
                    {
                        Logger.LogN($" {Jvedio.Language.Resources.Preview} {i + 1} {Jvedio.Language.Resources.Message_Fail}：{urlList[i]}， {Jvedio.Language.Resources.Reason} ： {sc.ToStatusMessage()}");
                        MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($" {Jvedio.Language.Resources.Preview} {i + 1} {Jvedio.Language.Resources.Message_Fail}，{Jvedio.Language.Resources.Reason} ：{sc.ToStatusMessage()} ，{Jvedio.Language.Resources.Message_ViewLog}"));
                    }

                }
                lock (lockobject) Value += 1;
                InfoUpdate?.Invoke(this, new DetailMovieEventArgs() { DetailMovie = dm, value = Value, maximum = Maximum });
            }
            lock (lockobject) Value = Maximum;
            InfoUpdate?.Invoke(this, new DetailMovieEventArgs() { DetailMovie = dm, value = Value, maximum = Maximum });
            IsDownLoading = false;
        }
    }

    public class DetailMovieEventArgs : EventArgs
    {
        public DetailMovie DetailMovie;
        public double value = 0;
        public double maximum = 1;
    }
}
