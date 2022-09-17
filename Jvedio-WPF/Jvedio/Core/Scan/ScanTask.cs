using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.CustomTask;
using Jvedio.Core.DataBase;
using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using Jvedio.Entity;
using Jvedio.Mapper;
using JvedioLib.Security;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    public class ScanTask : AbstractTask
    {
        public static string VIDEO_EXTENSIONS { get; set; }

        public static string PICTURE_EXTENSIONS { get; set; }

        public static List<string> VIDEO_EXTENSIONS_LIST { get; set; }

        public static List<string> PICTURE_EXTENSIONS_LIST { get; set; }

        public event EventHandler onScanning;

        public static Dictionary<NotImportReason, string> ReasonToString = new Dictionary<NotImportReason, string>()
        {
            { NotImportReason.NotInExtension, "扩展名不支持" },
            { NotImportReason.RepetitiveVideo, "重复的视频" },
            { NotImportReason.RepetitiveVID, "重复的识别码" },
            { NotImportReason.SizeTooSmall, "文件过小" },
            { NotImportReason.SizeTooLarge, "文件过大" },
        };

        static ScanTask()
        {
            VIDEO_EXTENSIONS = "3g2,3gp,3gp2,3gpp,amr,amv,asf,avi,bdmv,bik,d2v,divx,drc,dsa,dsm,dss,dsv,evo,f4v,flc,fli,flic,flv,hdmov,ifo,ivf,m1v,m2p,m2t,m2ts,m2v,m4b,m4p,m4v,mkv,mp2v,mp4,mp4v,mpe,mpeg,mpg,mpls,mpv2,mpv4,mov,mts,ogm,ogv,pss,pva,qt,ram,ratdvd,rm,rmm,rmvb,roq,rpm,smil,smk,swf,tp,tpr,ts,vob,vp6,webm,wm,wmp,wmv";
            PICTURE_EXTENSIONS = "png,jpg,jpeg,bmp,jpe,ico,gif";
            VIDEO_EXTENSIONS_LIST = VIDEO_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();
            PICTURE_EXTENSIONS_LIST = PICTURE_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();
        }

        #region "property"

        public ScanResult ScanResult { get; set; }

        public static new Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {
            { TaskStatus.Running, "扫描中..." },
            { TaskStatus.Canceled, "已取消" },
            { TaskStatus.RanToCompletion, "已完成" },
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

        private List<Video> existVideos { get; set; }

        private List<ActorInfo> existActors { get; set; }

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
                   Running = false;
                   return;
               }

               ScanHelper scanHelper = new ScanHelper();

               try
               {
                   (List<Video> import, Dictionary<string, NotImportReason> notImport, List<string> failNFO) parseResult
                    = scanHelper.parseMovie(FilePaths, FileExt, token, Properties.Settings.Default.ScanNfo, callBack: (msg) =>
                    {
                        logger.Error(msg);
                    });

                   ScanResult.TotalCount = parseResult.import.Count + parseResult.notImport.Count + parseResult.failNFO.Count;
                   try { CheckStatus(); }
                   catch (TaskCanceledException ex)
                   {
                       logger.Error(ex.Message);
                       FinalizeWithCancel();
                       OnCompleted(null);
                       return;
                   }

                   // 将 NFO 提取出来
                   List<Video> importNFO = parseResult.import.Where(arg => arg.Path.ToLower().EndsWith(".nfo")).ToList();
                   parseResult.import.RemoveAll(arg => arg.Path.ToLower().EndsWith(".nfo"));
                   HandleImport(parseResult.import);
                   HandleImportNFO(importNFO);          // 导入视频后再导入 NFO
                   HandleNotImport(parseResult.notImport);
                   HandleFailNFO(parseResult.failNFO);
               }
               catch (Exception ex)
               {
                   Logger.LogF(ex);
                   logger.Error(ex.Message);
               }

               Running = false;
               stopwatch.Stop();
               ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
               ScanResult.ElapsedMilliseconds = ElapsedMilliseconds;
               Status = TaskStatus.RanToCompletion;
               OnCompleted(null);
           });
        }

        private List<Video> GetExistVideos()
        {
            string sql = VideoMapper.BASE_SQL;
            sql = "select metadata.DataID,VID,Hash,Size,Path,MVID " + sql + $" and metadata.DBId={ConfigManager.Main.CurrentDBId}";
            List<Dictionary<string, object>> list = videoMapper.Select(sql);
            return videoMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);
        }

        private void HandleImport(List<Video> import)
        {
            logger.Info("开始处理导入");
            // 分为 2 部分，有识别码和无识别码
            List<Video> noVidList = import.Where(arg => string.IsNullOrEmpty(arg.VID)).ToList();
            List<Video> vidList = import.Where(arg => !string.IsNullOrEmpty(arg.VID)).ToList();

            existVideos = GetExistVideos();
            // 1. 处理有识别码的
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
                    ScanResult.Update.Add(video.Path, string.Empty);
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
                    ScanResult.Update.Add(video.Path, "HASH 相同，原路径不同");
                }
            }
            // 剩余的导入
            noVidList.RemoveAll(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && !arg.Path.Equals(t.Path)).Any());
            toInsert.AddRange(noVidList);

            // 1.更新
            videoMapper.UpdateBatch(toUpdate, "SubSection");// 分段视频
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            metaDataMapper.UpdateBatch(toUpdateData, "Path", "LastScanDate");
            AddTags(toUpdate);
            // 2.导入
            InsertData(toInsert);
        }

        private void HandleNotImport(Dictionary<string, NotImportReason> notImport)
        {
            foreach (var key in notImport.Keys)
            {
                if (ScanResult.NotImport.ContainsKey(key)) continue;
                NotImportReason reason = notImport[key];
                if (reason == NotImportReason.RepetitiveVID)
                {
                    string vid = Identify.GetVID(Path.GetFileNameWithoutExtension(key));
                    ScanResult.NotImport.Add(key, $"{ReasonToString[reason]} => {vid}");
                }
                else
                {
                    ScanResult.NotImport.Add(key, ReasonToString[reason]);
                }
            }
        }

        private void HandleFailNFO(List<string> failNFO)
        {
            foreach (string path in failNFO)
                ScanResult.FailNFO.AddRange(failNFO);
        }

        private static string[] NFOUpdateMetaProps = new string[]
        {
            "Title",
            "ReleaseYear",
            "ReleaseDate",
            "Country",
            "Genre",
            "Rating",
            "LastScanDate",
        };

        private static string[] NFOUpdateVideoProps = new string[]
        {
            "Plot",
            "Director",
            "Duration",
            "Studio",
            "Series",
            "Outline",
        };

        private void HandleImportNFO(List<Video> import)
        {
            if (import?.Count <= 0) return;

            existVideos = GetExistVideos();
            existActors = actorMapper.SelectList();

            // 复制图片
            if (ConfigManager.ScanConfig.CopyNFOPicture)
            {
                Dictionary<string, object> PicPaths = ConfigManager.Settings.PicPaths;
                if (PicPaths != null && PicPaths.ContainsKey(PathType.RelativeToData.ToString()))
                {
                    Dictionary<string, string> dict = null;
                    try
                    {
                        dict = (Dictionary<string, string>)PicPaths[PathType.RelativeToData.ToString()];
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }

                    if (dict != null && dict.ContainsKey("BigImagePath") && dict.ContainsKey("SmallImagePath")
                        && !string.IsNullOrEmpty(dict["BigImagePath"]) && !string.IsNullOrEmpty(dict["SmallImagePath"]))
                    {
                        string BigImagePath = dict["BigImagePath"];
                        string SmallImagePath = dict["SmallImagePath"];

                        string name1 = Path.GetFileNameWithoutExtension(BigImagePath).ToLower();
                        string name2 = Path.GetFileNameWithoutExtension(SmallImagePath).ToLower();

                        foreach (var item in import)
                        {
                            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2) || string.IsNullOrEmpty(item.Path)) continue;
                            string dir = Path.GetDirectoryName(item.Path);
                            if (!Directory.Exists(dir)) continue;
                            List<string> list = FileHelper.TryGetAllFiles(dir, "*.*").ToList();
                            if (list?.Count <= 0) continue;
                            list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();
                            string bigPath = list.Where(arg => Path.GetFileNameWithoutExtension(arg).ToLower().IndexOf(name1) >= 0).FirstOrDefault();
                            string smallPath = list.Where(arg => Path.GetFileNameWithoutExtension(arg).ToLower().IndexOf(name2) >= 0).FirstOrDefault();
                            if (File.Exists(bigPath))
                            {
                                string bigImagePath = item.getBigImage();
                                if (!File.Exists(bigImagePath))
                                    bigImagePath = Path.Combine(Path.GetDirectoryName(bigImagePath), Path.GetFileNameWithoutExtension(bigImagePath) + Path.GetExtension(bigPath));
                                FileHelper.TryCopyFile(bigPath, bigImagePath, true);
                            }

                            if (File.Exists(smallPath))
                            {
                                string smallImagePath = item.getSmallImage();
                                if (!File.Exists(smallImagePath))
                                    smallImagePath = Path.Combine(Path.GetDirectoryName(smallImagePath), Path.GetFileNameWithoutExtension(smallImagePath) + Path.GetExtension(smallPath));
                                FileHelper.TryCopyFile(smallPath, smallImagePath, true);
                            }
                        }
                    }
                }
            }

            // 1. 需要更新的
            List<Video> toUpdate = new List<Video>();
            foreach (Video video in import)
            {
                Video existVideo = existVideos.Where(t => video.VID.Equals(t.VID)).FirstOrDefault();
                if (existVideo != null)
                {
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID;//下面使用 videoMapper 更新的时候会使用到
                    ScanResult.Update.Add(video.Path, "NFO 信息更新");
                    video.Path = null;
                    video.LastScanDate = DateHelper.Now();
                    toUpdate.Add(video);
                    HandleActor(video);
                }
            }
            import.RemoveAll(arg => existVideos.Where(t => arg.VID.Equals(t.VID)).Any());

            videoMapper.UpdateBatch(toUpdate, NFOUpdateVideoProps);
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            metaDataMapper.UpdateBatch(toUpdateData, NFOUpdateMetaProps);
            // 2. 剩下的都是需要导入的
            InsertData(import);
        }

        private void HandleActor(Video video)
        {
            // 更新演员
            if (!string.IsNullOrEmpty(video.ActorNames))
            {
                List<string> list = video.ActorNames.Split(SuperUtils.Values.ConstValues.Separator).ToList();
                List<string> urls = video.ActorThumbs;
                for (int i = 0; i < list.Count; i++)
                {
                    string name = list[i];
                    string url = i < urls.Count ? urls[i] : string.Empty;
                    ActorInfo actorInfo = existActors?.Where(arg => arg.ActorName.Equals(name)).FirstOrDefault();
                    if (actorInfo == null || actorInfo.ActorID <= 0)
                    {
                        actorInfo = new ActorInfo();
                        actorInfo.ActorName = name;
                        actorInfo.ImageUrl = url;
                        actorMapper.Insert(actorInfo);
                        existActors.Add(actorInfo);
                    }
                    else
                    {
                        actorInfo.ImageUrl = url;
                        actorMapper.UpdateFieldById("ImageUrl", url, actorInfo.ActorID);
                    }
                    // 保存信息
                    string sql = $"insert or ignore into metadata_to_actor (ActorID,DataID) values ({actorInfo.ActorID},{video.DataID})";
                    metaDataMapper.ExecuteNonQuery(sql);
                }
            }
        }

        private void InsertData(ICollection<Video> toInsert)
        {
            foreach (Video video in toInsert)
            {
                video.DBId = ConfigManager.Main.CurrentDBId;
                video.FirstScanDate = DateHelper.Now();
                video.LastScanDate = DateHelper.Now();
                ScanResult.Import.Add(video.Path);
            }

            List<MetaData> toInsertData = toInsert.Select(arg => arg.toMetaData()).ToList();
            if (toInsertData.Count <= 0) return;
            long.TryParse(metaDataMapper.InsertAndGetID(toInsertData[0]).ToString(), out long before);
            toInsertData.RemoveAt(0);

            try
            {
                metaDataMapper.ExecuteNonQuery("BEGIN TRANSACTION;");//开启事务，这样子其他线程就不能更新
                metaDataMapper.InsertBatch(toInsertData);
                SqlManager.DataBaseBusy = true;
            }
            catch (Exception ex)
            {
                Logger.LogD(ex);
                OnError(new MessageCallBackEventArgs(ex.Message));
            }
            finally
            {
                metaDataMapper.ExecuteNonQuery("END TRANSACTION;");
                SqlManager.DataBaseBusy = false;
            }

            // 处理 DataID
            foreach (Video video in toInsert)
            {
                video.DataID = before;
                before++;
            }

            try
            {
                videoMapper.ExecuteNonQuery("BEGIN TRANSACTION;");//开启事务，这样子其他线程就不能更新
                SqlManager.DataBaseBusy = true;
                videoMapper.InsertBatch(toInsert);
            }
            catch (Exception ex)
            {
                Logger.LogD(ex);
                OnError(new MessageCallBackEventArgs(ex.Message));
            }
            finally
            {
                videoMapper.ExecuteNonQuery("END TRANSACTION;");
                SqlManager.DataBaseBusy = false;
            }

            AddTags(toInsert);

            // 处理演员
            existActors = actorMapper.SelectList();
            foreach (var video in toInsert)
                HandleActor(video);
        }

        private void AddTags(ICollection<Video> videos)
        {
            // 处理标记
            List<string> list = new List<string>();
            foreach (Video video in videos)
            {
                // 高清
                if (video.IsHDV())
                    list.Add($"({video.DataID},1)");
                // 中文
                if (video.IsCHS())
                    list.Add($"({video.DataID},2)");
            }
            if (list.Count > 0)
            {
                string sql = $"insert or ignore into metadata_to_tagstamp (DataID,TagID) values {string.Join(",", list)}";
                videoMapper.ExecuteNonQuery(sql);
            }
        }

        public void CheckStatus()
        {
            if (Status == TaskStatus.Canceled)
            {
                stopwatch.Stop();
                Running = false;
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                throw new TaskCanceledException();
            }
        }
    }
}
