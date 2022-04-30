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

namespace Jvedio.Core.FFmpeg
{
    public class ScreenShotTask : AbstractTask
    {



        public new static Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {

            {TaskStatus.WaitingToRun,"等待中..."},
            {TaskStatus.Running,"截图中..."},
            {TaskStatus.Canceled,"已取消"},
            {TaskStatus.RanToCompletion,"已完成"},
        };





        public ScreenShotTask(Video video, bool gif = false) : this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
            Gif = gif;
        }

        public long DataID { get; set; }
        public bool Gif { get; set; }

        public ScreenShotTask(MetaData data) : base()
        {
            DataID = data.DataID;
            DataType = data.DataType;


        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ScreenShotTask other)
                return other.DataID.Equals(DataID);
            return false;

        }

        public override int GetHashCode()
        {
            return DataID.GetHashCode();
        }


        public event EventHandler onCompleted;

        public DataType DataType { get; set; }
        public string Title { get; set; }


        public override void doWrok()
        {
            Task.Run(async () =>
            {
                Progress = 0;
                stopwatch.Start();
                Video video = videoMapper.SelectVideoByID(DataID);
                ScreenShot shot = new ScreenShot(video, token);
                shot.onProgress += (s, e) =>
                {
                    ScreenShot screenShot = s as ScreenShot;
                    Progress = (float)Math.Round(((float)screenShot.CurrentTaskCount + 1) / screenShot.TotalTaskCount, 4) * 100;
                };

                shot.onError += (s, e) =>
                {
                    MessageCallBackEventArgs arg = e as MessageCallBackEventArgs;
                    if (!string.IsNullOrEmpty(arg.Message))
                        logger.Error(arg.Message);
                };

                try
                {
                    string outputs = "";
                    if (Gif)
                        outputs = await shot.AsyncGenrateGif();
                    else
                        outputs = await shot.AsyncScreenShot();
                    Success = true;
                    Status = TaskStatus.RanToCompletion;
                    stopwatch.Stop();
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                    logger.Info($"总计耗时：{ElapsedMilliseconds} ms");
                    logger.Info("详细信息");
                    logger.Info(outputs);
                }
                catch (Exception ex)
                {
                    StatusText = ex.Message;
                    logger.Error(ex.Message);
                    finalizeWithCancel();
                }
                onCompleted?.Invoke(this, null);


            });
        }
    }
}
