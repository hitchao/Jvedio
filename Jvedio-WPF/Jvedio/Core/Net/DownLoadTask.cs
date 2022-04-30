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
using Jvedio.Core.Scan;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.CommonNet.Crawler;
using Newtonsoft.Json;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.CustomTask;
using Jvedio.CommonNet.Entity;

namespace Jvedio.Core.Net
{
    public class DownLoadTask : AbstractTask
    {

        /// <summary>
        /// 是否下载预览图
        /// </summary>
        public bool DownloadPreview { get; set; }
        public event EventHandler onDownloadSuccess;
        public event EventHandler onDownloadPreview;


        private static class Delay
        {
            public static int INFO = 1000;
            public static int EXTRA_IMAGE = 500;
            public static int BIG_IMAGE = 50;
            public static int SMALL_IMAGE = 50;
        }

        public DownLoadTask(Video video, bool downloadPreview = false, bool overrideInfo = false) : this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
            DownloadPreview = downloadPreview;
            OverrideInfo = overrideInfo;
        }

        public new static Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {

            {TaskStatus.WaitingToRun,"等待中..."},
            {TaskStatus.Running,"下载中..."},
            {TaskStatus.Canceled,"已取消"},
            {TaskStatus.RanToCompletion,"已完成"},
        };

        public DownLoadTask(MetaData data) : base()
        {
            DataID = data.DataID;
            DataType = data.DataType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is DownLoadTask other)
                return other.DataID.Equals(DataID);
            return false;

        }

        public override int GetHashCode()
        {
            return DataID.GetHashCode();
        }


        public long DataID { get; set; }
        public DataType DataType { get; set; }
        public string Title { get; set; }
        public bool OverrideInfo { get; set; }//强制下载覆盖信息


        public override void doWrok()
        {
            Task.Run(async () =>
            {
                Progress = 0;
                stopwatch.Start();
                Dictionary<string, object> dict = null;
                if (DataType == DataType.Video)
                {
                    Video video = videoMapper.SelectVideoByID(DataID);
                    VideoDownLoader downLoader = new VideoDownLoader(video, token);
                    RequestHeader header = null;
                    // 判断是否需要下载，自动跳过已下载的信息
                    if (video.toDownload() || OverrideInfo)
                    {
                        StatusText = "下载信息";
                        if (!string.IsNullOrEmpty(video.VID))
                        {
                            // 有 VID 的
                            try
                            {
                                dict = await downLoader.GetInfo((h) => { header = h; });
                            }
                            catch (CrawlerNotFoundException ex)
                            {
                                // todo 显示到界面上
                                Message = ex.Message;
                            }

                        }
                        else
                        {
                            // 无 VID 的




                        }
                        // 等待了很久都没成功

                        await Task.Delay(Delay.INFO);
                    }
                    else
                    {
                        logger.Info("跳过信息刮削，准备下载图片");
                    }


                    bool success = true;// 是否刮削到信息（包括db的部分信息）
                    Progress = 33f;
                    if ((dict != null && dict.ContainsKey("Error")))
                    {
                        string error = dict["Error"].ToString();
                        if (!string.IsNullOrEmpty(error))
                        {
                            Message = error;
                            logger.Error(error);
                        }
                        success = dict.ContainsKey("Title") && !string.IsNullOrEmpty(dict["Title"].ToString());
                    }
                    if (!success)
                    {
                        dict = null;
                        // 发生了错误，停止下载
                        finalizeWithCancel();
                        return;
                    }

                    bool downloadInfo = video.parseDictInfo(dict);// 是否从网络上刮削了信息
                    if (downloadInfo)
                    {
                        logger.Info($"保存入库");
                        // 并发锁
                        videoMapper.updateById(video);
                        metaDataMapper.updateById(video.toMetaData());
                        // 保存 dataCode
                        if (dict.ContainsKey("DataCode") && dict.ContainsKey("WebType"))
                        {
                            UrlCode urlCode = new UrlCode();
                            urlCode.LocalValue = video.VID;
                            urlCode.RemoteValue = dict["DataCode"].ToString();
                            urlCode.ValueType = "video";
                            urlCode.WebType = dict["WebType"].ToString();
                            urlCodeMapper.insert(urlCode, InsertMode.Replace);
                        }
                        // 保存 nfo
                        video.SaveNfo();


                        onDownloadSuccess?.Invoke(this, null);
                    }
                    else
                    {
                        dict = null;
                    }

                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }




                    // 可能刮削了信息，但是没刮削图片
                    if (header == null)
                    {
                        header = new RequestHeader();
                        header.WebProxy = GlobalConfig.ProxyConfig.GetWebProxy();
                        header.TimeOut = GlobalConfig.ProxyConfig.HttpTimeout * 1000;// 转为 ms
                    }



                    object o = getInfoFromExist("BigImageUrl", video, dict);
                    string imageUrl = o != null ? o.ToString() : "";
                    StatusText = "下载大图";
                    // 1. 大图
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // todo 原来的 domain 可能没法用，得替换 domain
                        string saveFileName = video.getBigImage(Path.GetExtension(imageUrl));
                        if (!File.Exists(saveFileName) || !string.IsNullOrEmpty(saveFileName))
                        {
                            byte[] fileByte = await downLoader.DownloadImage(imageUrl, header, (error) =>
                             {
                                 if (!string.IsNullOrEmpty(error))
                                     logger.Error($"{imageUrl} => {error}");
                             });
                            if (fileByte != null && fileByte.Length > 0)
                                FileProcess.ByteArrayToFile(fileByte, saveFileName, (error) => logger.Error(error));
                            await Task.Delay(Delay.BIG_IMAGE);
                        }
                        else
                        {
                            logger.Info($"跳过已下载的图片：{saveFileName}");
                        }
                    }

                    Progress = 66f;


                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }


                    StatusText = "下载小图";
                    o = getInfoFromExist("SmallImageUrl", video, dict);
                    imageUrl = o != null ? o.ToString() : "";
                    // 2. 小图
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        string saveFileName = video.getSmallImage(Path.GetExtension(imageUrl));
                        if (!File.Exists(saveFileName))
                        {
                            byte[] fileByte = await downLoader.DownloadImage(imageUrl, header, (error) =>
                            {
                                if (!string.IsNullOrEmpty(error))
                                    logger.Error($"{imageUrl} => {error}");
                            });
                            if (fileByte != null && fileByte.Length > 0)
                                FileProcess.ByteArrayToFile(fileByte, saveFileName, (error) => logger.Error(error));
                            await Task.Delay(Delay.SMALL_IMAGE);
                        }
                        else
                        {
                            logger.Info($"跳过已下载的图片：{saveFileName}");
                        }
                    }

                    Progress = 77f;
                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }

                    onDownloadSuccess?.Invoke(this, null);
                    StatusText = "下载演员信息和头像";

                    object names = getInfoFromExist("ActorNames", video, dict);
                    object urls = getInfoFromExist("ActressImageUrl", video, dict);
                    // 3. 演员信息和头像
                    if (names != null && urls != null && names is List<string> ActorNames && urls is List<string> ActressImageUrl)
                    {
                        if (ActorNames != null && ActressImageUrl != null && ActorNames.Count == ActressImageUrl.Count)
                        {
                            for (int i = 0; i < ActorNames.Count; i++)
                            {
                                string actorName = ActorNames[i];
                                string url = ActressImageUrl[i];
                                ActorInfo actorInfo = actorMapper.selectOne(new SelectWrapper<ActorInfo>().Eq("ActorName", actorName));
                                if (actorInfo == null || actorInfo.ActorID <= 0)
                                {
                                    actorInfo = new ActorInfo();
                                    actorInfo.ActorName = actorName;
                                    actorMapper.insert(actorInfo);
                                }
                                // 保存信息
                                string sql = $"insert or ignore into metadata_to_actor (ActorID,DataID) values ({actorInfo.ActorID},{video.DataID})";
                                metaDataMapper.executeNonQuery(sql);
                                // 下载图片
                                string saveFileName = actorInfo.getImagePath(video.Path, Path.GetExtension(url));
                                if (!File.Exists(saveFileName))
                                {
                                    byte[] fileByte = await downLoader.DownloadImage(url, header, (error) =>
                                    {
                                        if (!string.IsNullOrEmpty(error))
                                            logger.Error($"{url} => {error}");

                                    });
                                    if (fileByte != null && fileByte.Length > 0)
                                        FileProcess.ByteArrayToFile(fileByte, saveFileName, (error) => logger.Error(error));

                                }
                                else
                                {
                                    logger.Info($"跳过已下载的图片：{saveFileName}");
                                }
                            }
                        }


                    }
                    Progress = 88f;
                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }



                    // 4. 下载预览图

                    urls = getInfoFromExist("ExtraImageUrl", video, dict);
                    if (DownloadPreview && urls != null && urls is List<string> imageUrls)
                    {
                        StatusText = "下载预览图";
                        if (imageUrls != null && imageUrls.Count > 0)
                        {
                            for (int i = 0; i < imageUrls.Count; i++)
                            {
                                if (Canceld)
                                {
                                    finalizeWithCancel();
                                    return;
                                }

                                string url = imageUrls[i];

                                // 下载图片
                                string saveFiledir = video.getExtraImage();
                                if (!Directory.Exists(saveFiledir)) Directory.CreateDirectory(saveFiledir);
                                string saveFileName = Path.Combine(saveFiledir, Path.GetFileName(url));
                                if (!File.Exists(saveFileName))
                                {
                                    StatusText = $"下载预览图 {(i + 1)}/{imageUrls.Count}";
                                    byte[] fileByte = await downLoader.DownloadImage(url, header, (error) =>
                                    {
                                        if (!string.IsNullOrEmpty(error))
                                            logger.Error($"{url} => {error}");

                                    });
                                    if (fileByte != null && fileByte.Length > 0)
                                    {
                                        FileProcess.ByteArrayToFile(fileByte, saveFileName, (error) => logger.Error(error));
                                        PreviewImageEventArgs arg = new PreviewImageEventArgs(saveFileName, fileByte);
                                        onDownloadPreview?.Invoke(this, arg);
                                    }

                                    await Task.Delay(Delay.EXTRA_IMAGE);
                                }
                                else
                                {
                                    logger.Info($"跳过已下载的图片：{saveFileName}");
                                }
                            }
                        }
                    }
                    else
                    if (!DownloadPreview)
                        logger.Info($"未开启预览图下载");
                    Success = true;
                    Status = TaskStatus.RanToCompletion;
                }
                Console.WriteLine("下载完成！");
                Progress = 100.00f;
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                logger.Info($"总计耗时：{ElapsedMilliseconds} ms");
            });
        }






        private object getInfoFromExist(string type, Video video, Dictionary<string, object> dict)
        {
            if (dict != null && dict.Count > 0)
            {
                if (dict.ContainsKey(type))
                {
                    if (dict[type].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    {
                        Newtonsoft.Json.Linq.JArray jArray = Newtonsoft.Json.Linq.JArray.Parse(dict[type].ToString());
                        return jArray.Select(x => x.ToString()).ToList();
                    }
                    return dict[type];
                }
                return null;
            }
            else if (video != null)
            {
                string imageUrls = video.ImageUrls;
                if (!string.IsNullOrEmpty(imageUrls))
                {
                    Dictionary<string, object> dic = null;
                    try { dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(imageUrls); }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    if (dic == null) return null;
                    return getInfoFromExist(type, null, dic);// 递归调用
                }
            }
            return null;
        }
    }
}
