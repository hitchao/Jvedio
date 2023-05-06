using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.Tasks;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Jvedio.MapperManager;

namespace Jvedio.Core.FFmpeg
{
    public class ScreenShotTask : AbstractTask
    {

        static ScreenShotTask()
        {
            STATUS_TO_TEXT_DICT[TaskStatus.Running] = $"{LangManager.GetValueByKey("ScreenShotting")}...";
        }

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

        public DataType DataType { get; set; }

        public string Title { get; set; }

        public override void DoWork()
        {
            Task.Run(async () =>
            {
                Progress = 0;
                TimeWatch.Start();
                Video video = videoMapper.SelectVideoByID(DataID);
                if (video == null || video.DataID <= 0)
                {
                    Logger.Error($"未找到 DataID={DataID} 的资源");
                    return;
                }

                ScreenShot shot = new ScreenShot(video, Token);
                shot.onProgress += (s, e) =>
                {
                    ScreenShot screenShot = s as ScreenShot;
                    Progress = (float)Math.Round(((float)screenShot.CurrentTaskCount + 1) / screenShot.TotalTaskCount, 4) * 100;
                };

                shot.onError += (s, e) =>
                {
                    MessageCallBackEventArgs arg = e as MessageCallBackEventArgs;
                    if (!string.IsNullOrEmpty(arg.Message))
                        Logger.Error(arg.Message);
                };

                try
                {
                    string outputs = string.Empty;
                    if (Gif)
                        outputs = await shot.AsyncGenerateGif();
                    else
                        outputs = await shot.AsyncScreenShot();
                    Success = true;
                    Status = TaskStatus.RanToCompletion;
                    TimeWatch.Stop();
                    ElapsedMilliseconds = TimeWatch.ElapsedMilliseconds;
                    Logger.Info($"{LangManager.GetValueByKey("TotalCost")} {DateHelper.ToReadableTime(ElapsedMilliseconds)}");
                    Logger.Info(LangManager.GetValueByKey("Detail"));
                    Logger.Info(outputs);
                }
                catch (Exception ex)
                {
                    StatusText = ex.Message;
                    Logger.Error(ex.Message);
                    FinalizeWithCancel();
                }

                OnCompleted(null);
            });
        }
    }
}
