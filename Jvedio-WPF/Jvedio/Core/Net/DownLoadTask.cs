using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Framework.Tasks;
using SuperUtils.IO;
using SuperUtils.NetWork.Entity;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Net
{
    // todo 检视
    public class DownLoadTask : AbstractTask
    {
        private const String WRONG_STATUS_CODE = "-1";

        #region "事件"
        public static Action<DownLoadTask> onDownloadSuccess;
        public static Action<long, string, byte[]> onDownloadPreview;

        #endregion

        #region "属性"

        public bool DownloadPreview { get; set; } // 是否下载预览图
        public long DataID { get; set; }

        public DataType DataType { get; set; }

        public bool OverrideInfo { get; set; }// 强制下载覆盖信息

        #endregion


        private static class Delay
        {
            public static int INFO = 3000;
            public static int EXTRA_IMAGE = 500;
            public static int BIG_IMAGE = 50;
            public static int SMALL_IMAGE = 50;
        }

        public DownLoadTask(Video video, bool downloadPreview = false, bool overrideInfo = false) :
            this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
            DownloadPreview = downloadPreview;
            OverrideInfo = overrideInfo;
        }

        static DownLoadTask()
        {
            STATUS_TO_TEXT_DICT[TaskStatus.Running] = $"{LangManager.GetValueByKey("Downloading")}...";
        }

        public DownLoadTask(MetaData data) : base()
        {
            DataID = data.DataID;
            DataType = data.DataType;
        }

        static Dictionary<int, string> STATUS_DICT { get; set; } = new Dictionary<int, string>()
        {
            {200,"成功获取资源" },
            {500,"远程服务器错误" },
            {403,"远程服务器拒绝了您的访问（您的 IP 可能被限制了）" },
            {404,"远程服务器无该资源" },
        };

        public string StatusCodeToMessage(int status)
        {
            if (STATUS_DICT.ContainsKey(status)) {
                return STATUS_DICT[status];
            } else {
                return status.ToString();
            }
        }

        public string StatusCodeToMessage(string status)
        {
            if (int.TryParse(status, out int _temp))
                return StatusCodeToMessage(_temp);
            return "";
        }

        public async Task<Dictionary<string, object>> GetDataInfo(Video video, VideoDownLoader downLoader, RequestHeader header, Action<RequestHeader> headerCallBack)
        {
            Dictionary<string, object> dict = null;
            if (video == null || video.DataID <= 0) {
                Message = $"不存在 DataID={DataID} 的资源";
                Logger.Error(Message);
                FinalizeWithCancel();
                throw new Exception(Message);
            }

            // 判断是否需要下载，自动跳过已下载的信息
            if (OverrideInfo || video.ToDownload()) {
                if (!string.IsNullOrEmpty(video.VID)) {
                    // 有 VID 的
                    try {
                        dict = await downLoader.GetInfo((h) => { headerCallBack?.Invoke(h); });
                    } catch (CrawlerNotFoundException ex) {
                        // todo 显示到界面上
                        Message = ex.Message;
                        Logger.Error(Message);
                        FinalizeWithCancel();
                        throw ex;
                    } catch (DllLoadFailedException ex) {
                        Message = ex.Message;
                        Logger.Error(Message);
                        FinalizeWithCancel();
                        throw ex;
                    }
                } else {
                    // 无 VID 的
                    throw new Exception("同步信息需要 VID");
                }

                // 等待了很久都没成功
                Logger.Info($"暂停 {DateHelper.ToReadableTime(Delay.INFO)}");
                await Task.Delay(Delay.INFO);
                return dict;
            } else {
                Message = "该资源信息已同步，跳过信息下载";
                Logger.Info(Message);
                Logger.Info(LangManager.GetValueByKey("SkipDownLoadInfoAndDownloadImage"));
                return null;
            }
        }

        public async Task<bool> DownloadPoster(Video video, Dictionary<string, object> dict, VideoDownLoader downLoader, RequestHeader header)
        {
            if (!ConfigManager.DownloadConfig.DownloadPoster)
                return true;
            object o = GetInfoFromExist("BigImageUrl", video, dict);
            string imageUrl = o != null ? o.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(imageUrl)) {
                // todo 原来的 domain 可能没法用，得替换 domain
                Logger.Info($"url: {imageUrl}");
                string saveFileName = video.GetBigImage(Path.GetExtension(imageUrl), false);
                if (!File.Exists(saveFileName)) {
                    byte[] fileByte = await downLoader.DownloadImage(imageUrl, header, (error) => {
                        if (!string.IsNullOrEmpty(error))
                            Logger.Error($"{imageUrl} => {error}");
                    });
                    if (fileByte != null && fileByte.Length > 0) {
                        FileHelper.ByteArrayToFile(fileByte, saveFileName);
                        StatusText = "3.1 同步海报图成功";
                        return true;
                    } else
                        Logger.Error($"同步失败，文件大小为空");
                    await Task.Delay(Delay.BIG_IMAGE);
                } else {
                    Logger.Info($"{LangManager.GetValueByKey("SkipDownloadImage")} {saveFileName}");
                }
            } else {
                Logger.Error("图片地址为空");
            }
            return false;
        }

        public async Task<bool> DownloadThumbnail(Video video, Dictionary<string, object> dict, VideoDownLoader downLoader, RequestHeader header)
        {
            if (!ConfigManager.DownloadConfig.DownloadThumbNail)
                return true;
            object o = GetInfoFromExist("SmallImageUrl", video, dict);
            string imageUrl = o != null ? o.ToString() : string.Empty;

            // 2. 小图
            if (!string.IsNullOrEmpty(imageUrl)) {
                Logger.Info($"url: {imageUrl}");
                string saveFileName = video.GetSmallImage(Path.GetExtension(imageUrl), false);
                if (!File.Exists(saveFileName)) {
                    byte[] fileByte = await downLoader.DownloadImage(imageUrl, header, (error) => {
                        if (!string.IsNullOrEmpty(error))
                            Logger.Error($"{imageUrl} => {error}");
                    });
                    if (fileByte != null && fileByte.Length > 0) {
                        FileHelper.ByteArrayToFile(fileByte, saveFileName);
                        StatusText = "4.1 同步缩略图成功";
                        return true;
                    } else
                        Logger.Error($"sync thumbnail failed, file byte is empty");
                    await Task.Delay(Delay.SMALL_IMAGE);
                } else {
                    Logger.Info($"{LangManager.GetValueByKey("SkipDownloadImage")} {saveFileName}");
                }
            } else {
                Logger.Error("sync thumbnail image url is empty");
            }
            return false;
        }

        public void SaveActorNames(object names, Video video)
        {
            if (names is List<string> actorNames && actorNames.Count > 0) {
                int actorCount = actorNames.Count;
                for (int i = 0; i < actorCount; i++) {
                    string actorName = actorNames[i];
                    ActorInfo actorInfo =
                        actorMapper.SelectOne(new SelectWrapper<ActorInfo>().Eq("ActorName", actorName));
                    if (actorInfo == null || actorInfo.ActorID <= 0) {
                        actorInfo = new ActorInfo();
                        actorInfo.ActorName = actorName;
                        actorMapper.Insert(actorInfo);
                    }
                    // 保存信息
                    string sql = $"insert or ignore into metadata_to_actor (ActorID,DataID) values ({actorInfo.ActorID},{video.DataID})";
                    metaDataMapper.ExecuteNonQuery(sql);
                    StatusText = $"{i + 1}/{actorCount} 成功保存演员信息：{actorName}";
                }
            }
        }

        public async Task<bool> DownloadActors(Video video, Dictionary<string, object> dict, VideoDownLoader downLoader, RequestHeader header)
        {
            if (!ConfigManager.DownloadConfig.DownloadActor)
                return true;
            object names = GetInfoFromExist("ActorNames", video, dict);
            object urls = GetInfoFromExist("ActressImageUrl", video, dict);

            if (names == null)
                return false;

            if (urls == null) {
                SaveActorNames(names, video);
                return true;
            }

            // 必须要有演员头像才能导入
            if (names != null &&
                urls != null &&
                names is List<string> actorNames &&
                urls is List<string> ActressImageUrl) {
                if (actorNames != null &&
                    ActressImageUrl != null &&
                    actorNames.Count == ActressImageUrl.Count) {
                    int actorCount = actorNames.Count;
                    for (int i = 0; i < actorCount; i++) {
                        string actorName = actorNames[i];
                        string url = ActressImageUrl[i];
                        Logger.Info($"{actorName}: {url}");
                        ActorInfo actorInfo = actorMapper.SelectOne(new SelectWrapper<ActorInfo>().Eq("ActorName", actorName));
                        if (actorInfo == null || actorInfo.ActorID <= 0) {
                            actorInfo = new ActorInfo();
                            actorInfo.ActorName = actorName;
                            actorInfo.ImageUrl = url;
                            actorMapper.Insert(actorInfo);
                        }

                        // 保存信息
                        string sql = $"insert or ignore into metadata_to_actor (ActorID,DataID) values ({actorInfo.ActorID},{video.DataID})";
                        metaDataMapper.ExecuteNonQuery(sql);
                        StatusText = $"{i + 1}/{actorCount} 成功保存演员信息: {actorName}";
                        // 下载图片
                        string saveFileName = actorInfo.GetImagePath(video.Path, Path.GetExtension(url), false);
                        if (!File.Exists(saveFileName)) {
                            byte[] fileByte = await downLoader.DownloadImage(url, header, (error) => {
                                if (!string.IsNullOrEmpty(error))
                                    Logger.Error($"{url} => {error}");
                            });
                            if (fileByte != null && fileByte.Length > 0) {
                                FileHelper.ByteArrayToFile(fileByte, saveFileName);
                                StatusText = $"{i + 1}/{actorCount} 成功同步演员头像: {actorName}";
                            } else
                                Logger.Error($"{i + 1}/{actorCount} sync actor（{actorName}）image failed, file byte is empty");
                        } else {
                            Logger.Info($"{LangManager.GetValueByKey("SkipDownloadImage")} {saveFileName}");
                        }
                    }
                    return true;
                } else {
                    Logger.Error("empty: ActressImageUrl,  actorNames");
                }
            } else {
                Logger.Error("empty: names,  urls");
            }
            return false;
        }

        public async Task<bool> DownloadPreviews(Video video, Dictionary<string, object> dict, VideoDownLoader downLoader, RequestHeader header)
        {
            if (!ConfigManager.DownloadConfig.DownloadPreviewImage)
                return true;
            object urls = GetInfoFromExist("ExtraImageUrl", video, dict);
            if (DownloadPreview &&
                urls != null &&
                urls is List<string> imageUrls) {
                if (imageUrls != null && imageUrls.Count > 0) {
                    int imageCount = imageUrls.Count;
                    for (int i = 0; i < imageCount; i++) {
                        if (Canceled) {
                            FinalizeWithCancel();
                            return false;
                        }
                        string url = imageUrls[i];

                        // 下载图片
                        string saveDir = video.GetExtraImage();
                        DirHelper.TryCreateDirectory(saveDir);

                        string saveFileName = Path.Combine(saveDir, Path.GetFileName(url));
                        if (!File.Exists(saveFileName)) {
                            StatusText = $"{LangManager.GetValueByKey("Preview")} {i + 1}/{imageCount}";
                            byte[] fileByte = await downLoader.DownloadImage(url, header, (error) => {
                                if (!string.IsNullOrEmpty(error))
                                    Logger.Error($"{url} => {error}");
                            });
                            if (fileByte != null && fileByte.Length > 0) {
                                FileHelper.ByteArrayToFile(fileByte, saveFileName);
                                onDownloadPreview?.Invoke(DataID, saveFileName, fileByte);
                                StatusText = $"{i + 1}/{imageCount} 同步预览图成功";
                            } else {
                                Logger.Error($"{i + 1}/{imageCount} 下载图片失败，文件为空");
                            }

                            await Task.Delay(Delay.EXTRA_IMAGE);
                        } else {
                            Logger.Info($"{LangManager.GetValueByKey("SkipDownloadImage")} {saveFileName}");
                        }
                    }
                    return true;
                } else {
                    Logger.Warning("远程服务器无预览图");
                }
            } else if (!DownloadPreview) {
                StatusText = LangManager.GetValueByKey("NotSetPreviewDownload");
                Logger.Warning(LangManager.GetValueByKey("NotSetPreviewDownload"));
            } else {
                Logger.Warning("解析远程预览图失败");
            }
            return false;
        }

        public async Task<bool> CheckDataInfo(Video video, Dictionary<string, object> dict, VideoDownLoader downLoader, RequestHeader header)
        {
            // 只有同步了信息才需要校验信息
            if (!(video.ToDownload() || OverrideInfo)) {
                return true;
            }
            bool success = false;
            if (dict != null && dict.ContainsKey("Error")) {
                string statusCode = dict.Get("StatusCode", WRONG_STATUS_CODE).ToString();
                Logger.Info($"响应码: {statusCode} ({StatusCodeToMessage(statusCode)})");
                string error = dict["Error"].ToString();
                if (!string.IsNullOrEmpty(error) && !error.Equals(HttpResult.DEFAULT_ERROR_MSG)) {
                    Message = error;
                    Logger.Error(error);
                }
                success = dict.ContainsKey("Title") && !string.IsNullOrEmpty(dict["Title"].ToString());
            }
            if (!success) {
                if (string.IsNullOrEmpty(Message)) {
                    Message = dict.Get("StatusCode", "-1").ToString();
                    Logger.Error(Message);
                }
                if (int.TryParse(Message, out int status)) {
                    Message = StatusCodeToMessage(status);
                    Logger.Warning(Message);
                }

                await Task.Delay(Delay.INFO);
                // 发生了错误，停止下载
                FinalizeWithCancel();
                // 但是已经请求了网址，所以视为完成，并加入到长时间等待队列
                Status = TaskStatus.Canceled;
                return false;
            } else {
                StatusText = "2.1 同步信息成功";
            }

            bool downloadInfo = video.ParseDictInfo(dict); // 是否从网络上刮削了信息
            if (downloadInfo) {
                StatusText = LangManager.GetValueByKey("SaveToLibrary");
                // 并发锁
                videoMapper.UpdateById(video);
                metaDataMapper.UpdateById(video.toMetaData());

                // 保存 dataCode
                if (dict.ContainsKey("DataCode") && dict.ContainsKey("WebType")) {
                    UrlCode urlCode = new UrlCode();
                    urlCode.LocalValue = video.VID;
                    urlCode.RemoteValue = dict["DataCode"].ToString();
                    urlCode.ValueType = "video";
                    urlCode.WebType = dict["WebType"].ToString();
                    urlCodeMapper.Insert(urlCode, InsertMode.Replace);
                    StatusText = "2.2 成功保存 DataCode";
                }

                // 保存 nfo
                video.SaveNfo();
                if (ConfigManager.Settings.SaveInfoToNFO)
                    StatusText = "2.3 成功保存 NFO";
                onDownloadSuccess?.Invoke(this);
                return true;
            } else {
                return false;
            }
        }

        public override void DoWork()
        {
            Task.Run(async () => {
                Progress = 0;
                TimeWatch.Start();
                if (DataType == DataType.Video) {
                    Video video = videoMapper.SelectVideoByID(DataID);
                    RequestHeader header = null;
                    Dictionary<string, object> dict = null;
                    VideoDownLoader downLoader = new VideoDownLoader(video, Token, Logger);
                    StatusText = $"1. 开始同步信息: {video.VID}";
                    try {
                        //dict = new Dictionary<string, object>();
                        dict = await GetDataInfo(video, downLoader, header, (h) => { header = h; });
                    } catch (Exception ex) {
                        Logger.Error(ex.Message);
                        FinalizeWithCancel();
                        return;
                    }
                    if (dict != null && dict.ContainsKey("PluginID") && dict["PluginID"] is string PluginID)
                        Logger?.Info($"使用刮削器：{PluginID}");

                    bool success = true;
                    Progress = 10f;
                    StatusText = "2. 校验信息";
                    success = await CheckDataInfo(video, dict, downLoader, header);
                    //success = true;
                    if (!success) {
                        dict = null;
                        StatusText = "2.1. 校验信息不通过";
                        FinalizeWithCancel();
                        return;
                    }

                    if (Canceled) {
                        FinalizeWithCancel();
                        return;
                    }

                    // 可能刮削了信息，但是没刮削图片
                    if (header == null) {
                        header = new RequestHeader();
                        header.WebProxy = ConfigManager.ProxyConfig.GetWebProxy();
                        header.TimeOut = ConfigManager.ProxyConfig.HttpTimeout * 1000; // 转为 ms
                    }
                    Message = "";
                    StatusText = "3. 开始同步海报图";
                    success = await DownloadPoster(video, dict, downLoader, header);
                    Progress = 66f;
                    if (Canceled) {
                        FinalizeWithCancel();
                        return;
                    }
                    StatusText = "4. 开始同步缩略图";
                    success = await DownloadThumbnail(video, dict, downLoader, header);
                    Progress = 77f;
                    if (Canceled) {
                        FinalizeWithCancel();
                        return;
                    }

                    onDownloadSuccess?.Invoke(this);
                    StatusText = "5. 开始同步演员头像";
                    success = await DownloadActors(video, dict, downLoader, header);

                    Progress = 88f;
                    if (Canceled) {
                        FinalizeWithCancel();
                        return;
                    }

                    StatusText = "6. 开始同步预览图";
                    success = await DownloadPreviews(video, dict, downLoader, header);
                    Status = TaskStatus.RanToCompletion;
                }
                StatusText = "7. 同步所有内容完成";
                Running = false;
                Progress = 100.00f;
                TimeWatch.Stop();
                ElapsedMilliseconds = TimeWatch.ElapsedMilliseconds;
                Logger.Info($"{LangManager.GetValueByKey("TotalCost")} {DateHelper.ToReadableTime(ElapsedMilliseconds)}");

                // 任务完成后暂停 3s 随机
                await Task.Delay(new Random().Next(0, 3000));

                OnCompleted(null);
            });
        }

        private object GetInfoFromExist(string type, Video video, Dictionary<string, object> dict)
        {
            if (dict != null && dict.Count > 0) {
                if (dict.ContainsKey(type)) {
                    if (dict[type].GetType() == typeof(Newtonsoft.Json.Linq.JArray)) {
                        Newtonsoft.Json.Linq.JArray jArray = Newtonsoft.Json.Linq.JArray.Parse(dict[type].ToString());
                        return jArray.Select(x => x.ToString()).ToList();
                    }

                    return dict[type];
                }

                return null;
            } else if (video != null) {
                string imageUrls = video.ImageUrls;
                if (!string.IsNullOrEmpty(imageUrls)) {
                    Dictionary<string, object> dic = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(imageUrls);
                    if (dic == null)
                        return null;
                    return GetInfoFromExist(type, null, dic); // 递归调用
                }
            }

            return null;
        }

        #region "对外静态方法"
        public static void DownloadVideo(Video video)
        {
            DownLoadTask downloadTask = new DownLoadTask(video, ConfigManager.DownloadConfig.DownloadPreviewImage, ConfigManager.DownloadConfig.OverrideInfo);

            if (App.DownloadManager.Exists(downloadTask)) {
                MessageNotify.Warning(LangManager.GetValueByKey("TaskExists"));
                return;
            }
            downloadTask.onError += (s, ev) => MessageCard.Error((ev as MessageCallBackEventArgs).Message);
            App.DownloadManager.AddTask(downloadTask);
        }

        #endregion



        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is DownLoadTask other)
                return other.DataID.Equals(DataID);
            return false;
        }

        public override int GetHashCode()
        {
            return DataID.GetHashCode();
        }
    }
}
