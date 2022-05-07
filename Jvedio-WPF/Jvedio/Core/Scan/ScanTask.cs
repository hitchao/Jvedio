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
using Jvedio.Core.SimpleORM;
using Jvedio.Mapper;
using static Jvedio.GlobalMapper;
using Jvedio.Core.Net;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.CustomTask;

namespace Jvedio.Core.Scan
{
    public class ScanTask : AbstractTask
    {

        public static string VIDEO_EXTENSIONS = "3g2,3gp,3gp2,3gpp,amr,amv,asf,avi,bdmv,bik,d2v,divx,drc,dsa,dsm,dss,dsv,evo,f4v,flc,fli,flic,flv,hdmov,ifo,ivf,m1v,m2p,m2t,m2ts,m2v,m4b,m4p,m4v,mkv,mp2v,mp4,mp4v,mpe,mpeg,mpg,mpls,mpv2,mpv4,mov,mts,ogm,ogv,pss,pva,qt,ram,ratdvd,rm,rmm,rmvb,roq,rpm,smil,smk,swf,tp,tpr,ts,vob,vp6,webm,wm,wmp,wmv";
        public static string PICTURE_EXTENSIONS = "bmp,gif,ico,jpe,jpeg,jpg,png";
        public static List<string> VIDEO_EXTENSIONS_LIST = VIDEO_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();
        public static List<string> PICTURE_EXTENSIONS_LIST = PICTURE_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();
        public event EventHandler onScanning;




        #region "property"


        public ScanResult ScanResult { get; set; }


        public static new Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {

            {TaskStatus.Running,"扫描中..."},
            {TaskStatus.Canceled,"已取消"},
            {TaskStatus.RanToCompletion,"已完成"},
        };




        #endregion


        public List<string> ScanPaths { get; set; }
        public List<string> FilePaths { get; set; }

        public List<string> FileExt { get; set; }


        public ScanTask(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null) : base()
        {
            if (scanPaths != null && scanPaths.Count > 0)
                ScanPaths = scanPaths.Where(arg => Directory.Exists(arg)).ToList();
            if (filePaths != null && filePaths.Count > 0)
                FilePaths = filePaths.Where(arg => File.Exists(arg)).ToList();
            if (fileExt != null)
            {
                FileExt = new List<string>();
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
            ScanResult = new ScanResult();
        }




        public override void doWrok()
        {

            Task.Run(() =>
           {
               stopwatch.Start();
               logger.Info("开始扫描任务");
               foreach (string path in ScanPaths)
               {
                   IEnumerable<string> paths = DirHelper.GetFileList(path, "*.*", (ex) =>
                   {
                       // 发生异常
                       logger.Error(ex.Message);
                   }, (dir) =>
                   {
                       Message = dir;
                       onScanning?.Invoke(this, new MessageCallBackEventArgs(dir));
                   }, tokenCTS);
                   FilePaths.AddRange(paths);
               }

               try { CheckStatus(); }
               catch (TaskCanceledException ex)
               {
                   logger.Error(ex.Message);
                   Status = TaskStatus.Canceled;
                   return;
               }

               ScanHelper scanHelper = new ScanHelper();

               try
               {
                   (List<Video> import, List<string> notImport, List<string> failNFO) parseResult
                    = scanHelper.parseMovie(FilePaths, FileExt, token, Properties.Settings.Default.ScanNfo);
                   try { CheckStatus(); }
                   catch (TaskCanceledException ex)
                   {
                       logger.Error(ex.Message);
                       Status = TaskStatus.Canceled;
                       return;
                   }

                   handleImport(parseResult.import);
                   handleNotImport(parseResult.notImport);
                   handleFailNFO(parseResult.failNFO);
               }
               catch (Exception ex)
               {
                   Logger.LogF(ex);
                   logger.Error(ex.Message);
               }





               stopwatch.Stop();
               ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
               ScanResult.ElapsedMilliseconds = ElapsedMilliseconds;
               Status = TaskStatus.RanToCompletion;
               OnCompleted(null);
           });
        }



        private void handleImport(List<Video> import)
        {
            logger.Info("开始处理导入");
            // 分为 2 部分，有识别码和无识别码
            List<Video> noVidList = import.Where(arg => string.IsNullOrEmpty(arg.VID)).ToList();
            List<Video> vidList = import.Where(arg => !string.IsNullOrEmpty(arg.VID)).ToList();


            // 1. 处理有识别码的
            string sql = VideoMapper.BASE_SQL;
            sql = "select metadata.DataID,VID,Hash,Size,Path,MVID " + sql + $" and metadata.DBId={GlobalConfig.Main.CurrentDBId}";
            List<Dictionary<string, object>> list = videoMapper.select(sql);
            List<Video> existVideos = videoMapper.toEntity<Video>(list, typeof(Video).GetProperties(), false);

            // 1.1 不需要导入
            // 存在同路径、相同大小的影片
            foreach (var item in vidList.Where(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.Path.Equals(t.Path)).Any()))
            {
                ScanResult.NotImport.Add(item.Path, "同路径、相同大小的影片");
            }
            vidList.RemoveAll(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.Path.Equals(t.Path)).Any());

            // 存在不同路径、相同大小、相同 VID、且原路径也存在的影片
            foreach (var item in vidList.Where(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.VID.Equals(t.VID) && !arg.Path.Equals(t.Path) && File.Exists(t.Path)).Any()))
            {
                ScanResult.NotImport.Add(item.Path, "不同路径、相同大小、相同 VID、且原路径也存在的影片");

            }
            vidList.RemoveAll(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.VID.Equals(t.VID) && !arg.Path.Equals(t.Path) && File.Exists(t.Path)).Any());
            // 存在不同路径，相同 VID，不同大小，且原路径存在（可能是剪辑的视频）
            foreach (var item in vidList.Where(arg => existVideos.Where(t => arg.VID.Equals(t.VID) && !arg.Path.Equals(t.Path) && !arg.Size.Equals(t.Size) && File.Exists(t.Path)).Any()))
            {
                ScanResult.NotImport.Add(item.Path, "不同路径、相同大小、相同 VID、且原路径也存在的影片");

            }
            vidList.RemoveAll(arg => existVideos.Where(t => arg.VID.Equals(t.VID) && !arg.Path.Equals(t.Path) && !arg.Size.Equals(t.Size) && File.Exists(t.Path)).Any());

            // 1.2 需要 update 路径
            // VID 相同，原路径不同
            List<Video> toUpdate = new List<Video>();
            foreach (Video video in vidList)
            {
                Video existVideo = existVideos.Where(t => video.VID.Equals(t.VID) && !video.Path.Equals(t.Path)).FirstOrDefault();
                if (existVideo != null)
                {

                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID;//下面使用 videoMapper 更新的时候会使用到
                    video.LastScanDate = DateHelper.Now();
                    toUpdate.Add(video);
                    ScanResult.Update.Add(video.Path);
                }

            }
            vidList.RemoveAll(arg => existVideos.Where(t => arg.VID.Equals(t.VID)).Any());
            // 1.3 需要 insert
            List<Video> toInsert = vidList;

            // 2. 处理无识别码的
            // 存在相同 HASH ，不同路径的影片
            foreach (var item in noVidList.Where(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && arg.Path.Equals(t.Path)).Any()))
            {
                ScanResult.NotImport.Add(item.Path, "相同 HASH ，不同路径的影片");

            }

            noVidList.RemoveAll(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && arg.Path.Equals(t.Path)).Any());

            // hash 相同，原路径不同则需要更新
            foreach (Video video in noVidList)
            {
                Video existVideo = existVideos.Where(t => video.Hash.Equals(t.Hash) && !video.Path.Equals(t.Path)).FirstOrDefault();
                if (existVideo != null)
                {
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID;//下面使用 videoMapper 更新的时候会使用到
                    video.LastScanDate = DateHelper.Now();
                    toUpdate.Add(video);
                    ScanResult.Update.Add(video.Path);
                }

            }
            // 剩余的导入
            noVidList.RemoveAll(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && !arg.Path.Equals(t.Path)).Any());
            toInsert.AddRange(noVidList);

            // 1.更新
            videoMapper.updateBatch(toUpdate, "SubSection");// 分段视频
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            metaDataMapper.updateBatch(toUpdateData, "Path", "LastScanDate");


            // 2.导入
            foreach (Video video in toInsert)
            {
                video.DBId = GlobalConfig.Main.CurrentDBId;
                video.FirstScanDate = DateHelper.Now();
                video.LastScanDate = DateHelper.Now();
                ScanResult.Import.Add(video.Path);
            }



            List<MetaData> toInsertData = toInsert.Select(arg => arg.toMetaData()).ToList();
            if (toInsertData.Count <= 0) return;
            long.TryParse(metaDataMapper.insertAndGetID(toInsertData[0]).ToString(), out long before);
            toInsertData.RemoveAt(0);

            try
            {

                metaDataMapper.executeNonQuery("BEGIN TRANSACTION;");//开启事务，这样子其他线程就不能更新
                metaDataMapper.insertBatch(toInsertData);

            }
            catch (Exception ex)
            {
                Logger.LogD(ex);
                OnError(new MessageCallBackEventArgs(ex.Message));
            }
            finally
            {
                metaDataMapper.executeNonQuery("END TRANSACTION;");
            }

            // 处理 DataID
            foreach (Video video in toInsert)
            {
                video.DataID = before;
                before++;
            }

            try
            {
                videoMapper.executeNonQuery("BEGIN TRANSACTION;");//开启事务，这样子其他线程就不能更新
                GlobalVariable.DataBaseBusy = true;
                videoMapper.insertBatch(toInsert);

                //Console.WriteLine("暂停一段时间");

                //Task.Delay(5000).Wait();

                //Console.WriteLine("暂停结束");

            }
            catch (Exception ex)
            {
                Logger.LogD(ex);
                OnError(new MessageCallBackEventArgs(ex.Message));
            }
            finally
            {
                videoMapper.executeNonQuery("END TRANSACTION;");
                GlobalVariable.DataBaseBusy = false;
            }
        }

        private void handleNotImport(List<string> notImport)
        {
            foreach (string path in notImport)
            {
                ScanResult.NotImport.Add(path, "文件过小或忽略的文件拓展名");
            }
        }
        private void handleFailNFO(List<string> failNFO)
        {
            foreach (string path in failNFO)
            {
                ScanResult.FailNFO.AddRange(failNFO);
            }
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

    }
}
