using Jvedio.Core.DataBase;
using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Mapper;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.Tasks;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan
{
    public class ScanTask : AbstractTask
    {

        private const string DEFAULT_VIDEO_EXT = "3g2,3gp,3gp2,3gpp,amr,amv,asf,avi,bdmv,bik,d2v,divx,drc,dsa,dsm,dss,dsv,evo,f4v,flc,fli,flic,flv,hdmov,ifo,ivf,m1v,m2p,m2t,m2ts,m2v,m4b,m4p,m4v,mkv,mp2v,mp4,mp4v,mpe,mpeg,mpg,mpls,mpv2,mpv4,mov,mts,ogm,ogv,pss,pva,qt,ram,ratdvd,rm,rmm,rmvb,roq,rpm,smil,smk,swf,tp,tpr,ts,vob,vp6,webm,wm,wmp,wmv";
        private const string DEFAULT_IMAGE_EXT = "png,jpg,jpeg,bmp,jpe,ico,gif";


        #region "事件"

        public event EventHandler onScanning;



        #endregion

        #region "静态属性"


        public static Dictionary<NotImportReason, string> ReasonToString { get; set; } = new Dictionary<NotImportReason, string>()
        {
            { NotImportReason.NotInExtension, LangManager.GetValueByKey("NotSupportedExt") },
            { NotImportReason.RepetitiveVideo, LangManager.GetValueByKey("RepeatedVideo") },
            { NotImportReason.RepetitiveVID, LangManager.GetValueByKey("RepeatedVID") },
            { NotImportReason.SizeTooSmall, LangManager.GetValueByKey("FileSizeTooSmall") },
            { NotImportReason.SizeTooLarge, LangManager.GetValueByKey("FileSizeTooBig") },
        };


        private static string[] NFOUpdateMetaProps { get; set; } = new string[]
        {
            "Title",
            "ReleaseYear",
            "ReleaseDate",
            "Country",
            "Genre",
            "Rating",
            "LastScanDate",
            "PathExist",
        };

        private static string[] NFOUpdateVideoProps { get; set; } = new string[]
        {
            "Plot",
            "Director",
            "Duration",
            "Studio",
            "Series",
            "Outline",
        };

        #endregion

        #region "属性"

        public static string VIDEO_EXTENSIONS { get; set; }

        public static string PICTURE_EXTENSIONS { get; set; }

        public static List<string> VIDEO_EXTENSIONS_LIST { get; set; }

        public static List<string> PICTURE_EXTENSIONS_LIST { get; set; }

        public ScanResult ScanResult { get; set; }

        /// <summary>
        /// 需要扫描的目录
        /// </summary>
        public List<string> ScanPaths { get; set; }

        public List<string> FilePaths { get; set; }

        public List<string> FileExt { get; set; }

        private List<Video> existVideos { get; set; }

        private List<ActorInfo> existActors { get; set; }
        #endregion



        static ScanTask()
        {
            VIDEO_EXTENSIONS = DEFAULT_VIDEO_EXT;
            PICTURE_EXTENSIONS = DEFAULT_IMAGE_EXT;
            VIDEO_EXTENSIONS_LIST = VIDEO_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();
            PICTURE_EXTENSIONS_LIST = PICTURE_EXTENSIONS.Split(',').Select(arg => "." + arg).ToList();
            STATUS_TO_TEXT_DICT[TaskStatus.Running] = $"{LangManager.GetValueByKey("Scanning")}...";
        }


        public ScanTask(List<string> scanPaths, List<string> filePaths,
            IEnumerable<string> fileExt = null) : base()
        {
            if (scanPaths != null && scanPaths.Count > 0)
                ScanPaths = scanPaths.Where(arg => Directory.Exists(arg)).ToList();
            if (filePaths != null && filePaths.Count > 0)
                FilePaths = filePaths.Where(arg => File.Exists(arg)).ToList();
            if (fileExt != null) {
                FileExt = new List<string>();
                foreach (var item in fileExt) {
                    string ext = item.Trim();
                    if (string.IsNullOrEmpty(ext))
                        continue;
                    if (!item.StartsWith("."))
                        FileExt.Add("." + ext);
                    else
                        FileExt.Add(ext);
                }
            }

            if (ScanPaths == null)
                ScanPaths = new List<string>();
            if (FilePaths == null)
                FilePaths = new List<string>();
            if (FileExt == null)
                FileExt = VIDEO_EXTENSIONS_LIST; // 默认导入视频
            ScanResult = new ScanResult();
            Logs.Add($"scan task init, paths count: {FilePaths.Count}, dir count: {ScanPaths.Count}");
        }

        private void ScanDir()
        {
            int total = ScanPaths.Count;
            int count = 0;
            foreach (string path in ScanPaths) {
                IEnumerable<string> paths = DirHelper.GetFileList(path, "*.*", (ex) => {
                    // 发生异常
                    Logger.Error(ex.Message);
                }, (dir) => {
                    Message = dir;
                    onScanning?.Invoke(this, new MessageCallBackEventArgs(dir));
                }, TokenCTS);
                FilePaths.AddRange(paths);
                count++;
                Progress = (float)Math.Round(((float)count) / total, 4) * 100;
            }
            Progress = 30;
        }

        /// <summary>
        /// 在 UI 中显示要导入的文件
        /// </summary>
        private void NotifyScanPath()
        {
            if (ScanPaths.Count == 0 && FilePaths.Count != 0) {
                string path = FilePaths[FilePaths.Count - 1];
                Message = path;
                onScanning?.Invoke(this, new MessageCallBackEventArgs(path));
            }
        }

        public override void DoWork()
        {
            Logger.Info(LangManager.GetValueByKey("BeginScan"));
            Task.Run(() => {
                StartWatch();
                NotifyScanPath();
                ScanDir();

                try {
                    CheckStatus();
                } catch (TaskCanceledException) {
                    FinalizeWithCancel();
                    return;
                }

                VideoParser videoParser = new VideoParser((msg) => {
                    if (Logs != null)
                        Logs.Add(msg);
                });

                try {
                    (List<Video> import, Dictionary<string, NotImportReason> notImport, List<string> failNFO)
                    parseResult = videoParser.ParseMovie(FilePaths, FileExt, Token, ConfigManager.ScanConfig.ScanNfo, callBack: (msg) => {
                        Logger.Error(msg);
                    });

                    Progress = 60;

                    ScanResult.TotalCount = parseResult.import.Count + parseResult.notImport.Count + parseResult.failNFO.Count;
                    try {
                        CheckStatus();
                    } catch (TaskCanceledException ex) {
                        Logger.Error(ex.Message);
                        base.FinalizeWithCancel();
                        base.OnCompleted(null);
                        return;
                    }

                    // 将 NFO 提取出来
                    List<Video> importNFO = parseResult.import.Where(arg => arg.Path.ToLower().EndsWith(".nfo")).ToList();
                    parseResult.import.RemoveAll(arg => arg.Path.ToLower().EndsWith(".nfo"));
                    HandleImport(parseResult.import);

                    Progress = 70;

                    HandleImportNFO(importNFO);          // 导入视频后再导入 NFO

                    Progress = 80;

                    HandleNotImport(parseResult.notImport);

                    Progress = 90;

                    HandleFailNFO(parseResult.failNFO);
                    Progress = 100;
                    Success = true;
                } catch (Exception ex) {
                    Logger.Error(ex.Message);
                    FinalizeWithCancel();
                    return;
                }

                Running = false;
                StopWatch();
                ScanResult.ElapsedMilliseconds = ElapsedMilliseconds;
                Status = TaskStatus.RanToCompletion;
                base.OnCompleted(null);
            });
        }

        private List<Video> GetExistVideos()
        {
            string sql = VideoMapper.SQL_BASE;
            sql = "select metadata.DataID,VID,Hash,Size,Path,MVID " + sql + $" and metadata.DBId={ConfigManager.Main.CurrentDBId}";
            List<Dictionary<string, object>> list = videoMapper.Select(sql);
            return videoMapper.ToEntity<Video>(list, typeof(Video).GetProperties(), false);
        }

        private void HandleImport(List<Video> import)
        {
            if (import == null || import.Count == 0) {
                return;
            }
            // 分为 2 部分，有识别码和无识别码
            List<Video> noVidList = import.Where(arg => string.IsNullOrEmpty(arg.VID)).ToList();
            List<Video> vidList = import.Where(arg => !string.IsNullOrEmpty(arg.VID)).ToList();

            existVideos = GetExistVideos();

            // 1. 处理有识别码的
            // 1.1 不需要导入
            // 存在同路径、相同大小的影片
            foreach (var item in vidList.Where(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.Path.Equals(t.Path)).Any())) {
                ScanResult.NotImport.Add(item.Path, new ScanDetailInfo(LangManager.GetValueByKey("SamePathFileSize")));
            }

            vidList.RemoveAll(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.Path.Equals(t.Path)).Any());

            // 存在不同路径、相同大小、相同 VID、且原路径也存在的影片
            foreach (var item in vidList.Where(arg => existVideos
                .Where(t => arg.Size.Equals(t.Size) && arg.VID.Equals(t.VID) && !arg.Path.Equals(t.Path) && File.Exists(t.Path))
                .Any())) {
                ScanDetailInfo scanDetailInfo = new ScanDetailInfo(LangManager.GetValueByKey("NotSamePathSameFileSize"));
                StringBuilder builder = new StringBuilder($"和 {item.Path} 存在重复：");
                builder.AppendLine();
                builder.AppendLine();
                List<Video> _videos = existVideos
                .Where(t => item.Size.Equals(t.Size) && item.VID.Equals(t.VID) && !item.Path.Equals(t.Path) && File.Exists(t.Path)).ToList();
                foreach (var _video in _videos)
                    builder.AppendLine(_video.Path);
                scanDetailInfo.Detail = builder.ToString();
                ScanResult.NotImport.Add(item.Path, scanDetailInfo);
            }

            vidList.RemoveAll(arg => existVideos.Where(t => arg.Size.Equals(t.Size) && arg.VID.Equals(t.VID) && !arg.Path.Equals(t.Path) && File.Exists(t.Path)).Any());

            // 存在不同路径，相同 VID，不同大小，且原路径存在（可能是剪辑的视频），且分段视频不相等
            foreach (var item in vidList
                .Where(arg => existVideos.Where(t => arg.VID.Equals(t.VID) &&
                !arg.Path.Equals(t.Path) &&
                !arg.Size.Equals(t.Size) &&
                File.Exists(t.Path) &&
                t.SubSection.Equals(arg.SubSection))
                .Any())) {
                ScanDetailInfo scanDetailInfo = new ScanDetailInfo(LangManager.GetValueByKey("SamePathNotSameFileSize"));
                StringBuilder builder = new StringBuilder($"和 {item.Path} 存在重复：");
                builder.AppendLine();
                builder.AppendLine();
                List<Video> _videos = existVideos
                    .Where(t => item.VID.Equals(t.VID) && !item.Path.Equals(t.Path) && !item.Size.Equals(t.Size) && File.Exists(t.Path)).ToList();
                foreach (var _video in _videos)
                    builder.AppendLine(_video.Path);
                scanDetailInfo.Detail = builder.ToString();
                ScanResult.NotImport.Add(item.Path, scanDetailInfo);
            }

            vidList.RemoveAll(arg => existVideos.Where(t => arg.VID.Equals(t.VID)
            && !arg.Path.Equals(t.Path) &&
            !arg.Size.Equals(t.Size) &&
            File.Exists(t.Path) &&
            t.SubSection.Equals(arg.SubSection)).Any());

            // 1.2 需要 update 路径
            // VID 相同，原路径（或分段视频路径）不同
            List<Video> toUpdate = new List<Video>();
            foreach (Video video in vidList) {
                Video existVideo = existVideos.Where(t => video.VID.Equals(t.VID) && (!video.Path.Equals(t.Path) || !video.SubSection.Equals(t.SubSection))).FirstOrDefault();
                if (existVideo != null) {
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID; // 下面使用 videoMapper 更新的时候会使用到
                    video.LastScanDate = DateHelper.Now();
                    video.PathExist = 1;
                    toUpdate.Add(video);
                    ScanResult.Update.Add(video.Path, string.Empty);
                }
            }

            vidList.RemoveAll(arg => existVideos.Where(t => arg.VID.Equals(t.VID)).Any());

            // 1.3 需要 insert
            List<Video> toInsert = vidList;

            // 2. 处理无识别码的
            // 存在相同 HASH ，相同路径的影片
            foreach (var item in noVidList.Where(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && arg.Path.Equals(t.Path)).Any())) {
                ScanDetailInfo scanDetailInfo = new ScanDetailInfo(LangManager.GetValueByKey("SameHashSamePath"));
                StringBuilder builder = new StringBuilder($"和 {item.Path} 存在重复：");
                builder.AppendLine();
                builder.AppendLine();
                List<Video> _videos = existVideos
                    .Where(t => item.Hash.Equals(t.Hash) && item.Path.Equals(t.Path)).ToList();
                foreach (var _video in _videos)
                    builder.AppendLine(_video.Path);
                scanDetailInfo.Detail = builder.ToString();
                ScanResult.NotImport.Add(item.Path, scanDetailInfo);
            }

            noVidList.RemoveAll(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && arg.Path.Equals(t.Path)).Any());

            // hash 相同，原路径不同则需要更新
            foreach (Video video in noVidList) {
                Video existVideo = existVideos.Where(t => video.Hash.Equals(t.Hash) && !video.Path.Equals(t.Path)).FirstOrDefault();
                if (existVideo != null) {
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID; // 下面使用 videoMapper 更新的时候会使用到
                    video.LastScanDate = DateHelper.Now();
                    video.PathExist = 1;
                    toUpdate.Add(video);
                    ScanResult.Update.Add(video.Path, LangManager.GetValueByKey("SameHashNotSamePath"));
                }
            }

            // 剩余的导入
            noVidList.RemoveAll(arg => existVideos.Where(t => arg.Hash.Equals(t.Hash) && !arg.Path.Equals(t.Path)).Any());
            toInsert.AddRange(noVidList);

            // 1.更新
            if (toUpdate?.Count > 0)
                videoMapper.UpdateBatch(toUpdate, "SubSection"); // 分段视频
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            if (toUpdateData?.Count > 0)
                metaDataMapper.UpdateBatch(toUpdateData, "Path", "LastScanDate", "PathExist");
            AddTags(toUpdate);

            // 2.导入
            InsertData(toInsert);
        }

        private void HandleNotImport(Dictionary<string, NotImportReason> notImport)
        {
            foreach (var key in notImport.Keys) {
                if (ScanResult.NotImport.ContainsKey(key))
                    continue;
                NotImportReason reason = notImport[key];
                if (reason == NotImportReason.RepetitiveVID) {
                    string vid = JvedioLib.Security.Identify.GetVID(Path.GetFileNameWithoutExtension(key));
                    ScanResult.NotImport.Add(key, new ScanDetailInfo($"{ReasonToString[reason]} => {vid}"));
                } else {
                    ScanResult.NotImport.Add(key, new ScanDetailInfo(ReasonToString[reason]));
                }
            }
        }

        private void HandleFailNFO(List<string> failNFO)
        {
            foreach (string path in failNFO)
                ScanResult.FailNFO.AddRange(failNFO);
        }

        private void CopyNfoImage(Dictionary<string, string> dict, List<Video> import, ImageType imageType)
        {
            if (dict == null)
                return;

            switch (imageType) {
                case ImageType.Big:
                    if (dict.ContainsKey("BigImagePath") && !string.IsNullOrEmpty(dict["BigImagePath"])) {
                        string path = dict["BigImagePath"];
                        string dirname = path.ToLower();
                        CopyImage(import, dirname, imageType);
                    }
                    break;
                case ImageType.Small:
                    if (dict.ContainsKey("SmallImagePath") && !string.IsNullOrEmpty(dict["SmallImagePath"])) {
                        string path = dict["SmallImagePath"];
                        string dirname = path.ToLower();
                        CopyImage(import, dirname, imageType);
                    }
                    break;
                case ImageType.Actor:
                case ImageType.Preview:
                case ImageType.ScreenShot:
                    CopyImages(import, imageType);
                    break;
                default:

                    break;
            }
        }


        private string GetImagePathByType(Video video, ImageType type)
        {
            switch (type) {
                case ImageType.Big:
                    return video.GetBigImage();
                case ImageType.Small:
                    return video.GetSmallImage();
                case ImageType.ScreenShot:
                    return video.GetScreenShot();
                case ImageType.Preview:
                    return video.GetExtraImage();
                case ImageType.Actor:
                    return video.GetBigImage();
            }
            return "";
        }

        private void CopyImage(List<Video> import, string dirName, ImageType imageType)
        {
            if (string.IsNullOrEmpty(dirName))
                return;

            foreach (Video item in import) {
                if (string.IsNullOrEmpty(item.Path))
                    continue;
                string dir = Path.GetDirectoryName(item.Path);
                if (!Directory.Exists(dir))
                    continue;
                string[] arr = FileHelper.TryGetAllFiles(dir, "*.*");
                if (arr == null || arr.Length == 0)
                    continue;
                List<string> list = arr.ToList();
                list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST
                                    .Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();
                string filename = Path.GetFileName(dirName);
                string originPath = list.Where(arg =>
                    Path.GetFileNameWithoutExtension(arg).ToLower().IndexOf(filename) >= 0).FirstOrDefault();
                if (File.Exists(originPath)) {
                    string targetImagePath = GetImagePathByType(item, imageType);
                    if (!File.Exists(targetImagePath)) {
                        targetImagePath = Path.Combine(Path.GetDirectoryName(targetImagePath),
                            Path.GetFileNameWithoutExtension(targetImagePath) + Path.GetExtension(originPath));
                        FileHelper.TryCopyFile(originPath, targetImagePath, true);
                    } else if (ConfigManager.ScanConfig.CopyNFOOverwriteImage) {
                        FileHelper.TryCopyFile(originPath, targetImagePath, true);
                    }
                }
            }
        }

        private void CopyImages(List<Video> import, ImageType type)
        {
            foreach (Video item in import) {
                if (string.IsNullOrEmpty(item.Path))
                    continue;
                string dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(item.Path), ConfigManager.ScanConfig.CopyNFOPreviewPath));
                if (type == ImageType.ScreenShot)
                    dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(item.Path), ConfigManager.ScanConfig.CopyNFOScreenShotPath));
                else if (type == ImageType.Actor)
                    dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(item.Path), ConfigManager.ScanConfig.CopyNFOActorPath));
                if (!Directory.Exists(dir))
                    continue;
                string[] arr = FileHelper.TryGetAllFiles(dir, "*.*");
                if (arr == null || arr.Length == 0)
                    continue;
                List<string> list = arr.ToList();
                list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();
                // 预览图目录
                string targetPath = item.GetExtraImage();
                if (type == ImageType.ScreenShot)
                    targetPath = item.GetScreenShot();
                else if (type == ImageType.Actor)
                    targetPath = item.GetActorPath();
                DirHelper.TryCreateDirectory(targetPath);
                foreach (var path in list) {
                    string targetFilePath = Path.Combine(targetPath, Path.GetFileName(path));
                    FileHelper.TryCopyFile(path, targetFilePath, ConfigManager.ScanConfig.CopyNFOOverwriteImage);
                }

            }
        }

        private void HandleImportNFO(List<Video> import)
        {
            if (import == null || import.Count <= 0)
                return;

            existVideos = GetExistVideos();
            existActors = actorMapper.SelectList();

            // 解析图片路径

            Dictionary<string, object> picPaths = ConfigManager.Settings.PicPaths;
            if (picPaths != null && picPaths.ContainsKey(PathType.RelativeToData.ToString())) {
                Dictionary<string, string> dict = null;
                try {
                    dict = (Dictionary<string, string>)picPaths[PathType.RelativeToData.ToString()];
                } catch (Exception ex) {
                    Logger.Error(ex.Message);
                }

                // 复制图片
                if (ConfigManager.ScanConfig.CopyNFOPicture) {
                    CopyNfoImage(dict, import, ImageType.Big);
                    CopyNfoImage(dict, import, ImageType.Small);
                }

                if (ConfigManager.ScanConfig.CopyNFOActorPicture)
                    CopyNfoImage(dict, import, ImageType.Actor);

                if (ConfigManager.ScanConfig.CopyNFOPreview)
                    CopyNfoImage(dict, import, ImageType.Preview);

                if (ConfigManager.ScanConfig.CopyNFOScreenShot)
                    CopyNfoImage(dict, import, ImageType.ScreenShot);
            }

            // 1. 需要更新的
            List<Video> toUpdate = new List<Video>();
            foreach (Video video in import) {
                Video existVideo = existVideos.Where(t => video.VID.Equals(t.VID)).FirstOrDefault();
                if (existVideo != null) {
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID; // 下面使用 videoMapper 更新的时候会使用到
                    ScanResult.Update.Add(video.Path, LangManager.GetValueByKey("UpdateNFO"));
                    video.Path = null;
                    video.PathExist = 1;
                    video.LastScanDate = DateHelper.Now();
                    toUpdate.Add(video);
                    HandleActor(video);
                }
            }

            import.RemoveAll(arg => existVideos.Where(t => arg.VID.Equals(t.VID)).Any());

            if (toUpdate?.Count > 0)
                videoMapper.UpdateBatch(toUpdate, NFOUpdateVideoProps);
            List<MetaData> toUpdateData = toUpdate.Select(arg => arg.toMetaData()).ToList();
            if (toUpdateData?.Count > 0)
                metaDataMapper.UpdateBatch(toUpdateData, NFOUpdateMetaProps);

            // 2. 剩下的都是需要导入的
            InsertData(import);
        }

        private void HandleActor(Video video)
        {
            // 更新演员
            if (!string.IsNullOrEmpty(video.ActorNames)) {
                List<string> list = video.ActorNames.Split(SuperUtils.Values.ConstValues.Separator).ToList();
                List<string> urls = video.ActorThumbs;
                for (int i = 0; i < list.Count; i++) {
                    string name = list[i];
                    string url = i < urls.Count ? urls[i] : string.Empty;
                    ActorInfo actorInfo = existActors?.Where(arg => arg.ActorName.Equals(name)).FirstOrDefault();
                    if (actorInfo == null || actorInfo.ActorID <= 0) {
                        actorInfo = new ActorInfo();
                        actorInfo.ActorName = name;
                        actorInfo.ImageUrl = url;
                        actorMapper.Insert(actorInfo);
                        existActors.Add(actorInfo);
                    } else {
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
            foreach (Video video in toInsert) {
                video.DBId = ConfigManager.Main.CurrentDBId;
                video.FirstScanDate = DateHelper.Now();
                video.LastScanDate = DateHelper.Now();
                video.PathExist = 1;
                ScanResult.Import.Add(video.Path);
            }

            List<MetaData> toInsertData = toInsert.Select(arg => arg.toMetaData()).ToList();
            if (toInsertData.Count <= 0)
                return;
            long.TryParse(metaDataMapper.InsertAndGetID(toInsertData[0]).ToString(), out long before);
            toInsertData.RemoveAt(0);

            try {
                metaDataMapper.ExecuteNonQuery("BEGIN TRANSACTION;"); // 开启事务，这样子其他线程就不能更新
                metaDataMapper.InsertBatch(toInsertData);
                SqlManager.DataBaseBusy = true;
            } catch (Exception ex) {
                Logger.Error(ex.Message);
                OnError(new MessageCallBackEventArgs(ex.Message));
            } finally {
                metaDataMapper.ExecuteNonQuery("END TRANSACTION;");
                SqlManager.DataBaseBusy = false;
            }

            // 处理 DataID
            foreach (Video video in toInsert) {
                video.DataID = before;
                before++;
            }

            try {
                videoMapper.ExecuteNonQuery("BEGIN TRANSACTION;"); // 开启事务，这样子其他线程就不能更新
                SqlManager.DataBaseBusy = true;
                videoMapper.InsertBatch(toInsert);
            } catch (Exception ex) {
                Logger.Error(ex.Message);
                OnError(new MessageCallBackEventArgs(ex.Message));
            } finally {
                videoMapper.ExecuteNonQuery("END TRANSACTION;");
                SqlManager.DataBaseBusy = false;
            }

            AddTags(toInsert);

            // 处理演员
            existActors = actorMapper.SelectList();
            foreach (var video in toInsert)
                HandleActor(video);

            ScanResult.InsertVideos.AddRange(toInsert);
        }

        private void AddTags(ICollection<Video> videos)
        {
            // 处理标记
            List<string> list = new List<string>();
            foreach (Video video in videos) {
                // 高清
                if (video.IsHDV())
                    list.Add($"({video.DataID},1)");

                // 中文
                if (video.IsCHS())
                    list.Add($"({video.DataID},2)");

                // 新加入
                // todo 由于设计之初未考虑到多的空余位置添加标记，因此新标记以 ID 10000开头
                list.Add($"({video.DataID},10000)");
            }

            if (list.Count > 0) {
                string sql = $"insert or ignore into metadata_to_tagstamp (DataID,TagID) values {string.Join(",", list)}";
                videoMapper.ExecuteNonQuery(sql);
            }
        }

        public void CheckStatus()
        {
            if (Status == TaskStatus.Canceled) {
                StopWatch();
                Running = false;
                throw new TaskCanceledException();
            }
        }
    }
}
