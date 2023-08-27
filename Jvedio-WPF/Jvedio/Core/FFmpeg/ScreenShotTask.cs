using Jvedio.Core.Enums;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using SuperUtils.Framework.Tasks;
using SuperUtils.Time;
using System;
using System.Threading.Tasks;
using static Jvedio.MapperManager;

namespace Jvedio.Core.FFmpeg
{
    public class ScreenShotTask : AbstractTask
    {
        #region "事件"

        public static Action<string> onScreenShotError;

        public static Action<bool, long> onScreenShotCompleted;

        #endregion

        #region "属性"

        #endregion
        public long DataID { get; set; }
        public bool Gif { get; set; }

        public DataType DataType { get; set; }

        static ScreenShotTask()
        {
            STATUS_TO_TEXT_DICT[TaskStatus.Running] = $"{LangManager.GetValueByKey("ScreenShotting")}...";
        }

        public ScreenShotTask(Video video, bool gif = false) : this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
            if (string.IsNullOrEmpty(Title))
                Title = System.IO.Path.GetFileNameWithoutExtension(video.Path);
            Gif = gif;
        }


        public ScreenShotTask(MetaData data) : base()
        {
            DataID = data.DataID;
            DataType = data.DataType;
        }

        public override void DoWork()
        {
            Task.Run(async () => {
                Progress = 0;
                TimeWatch.Start();
                Video video = videoMapper.SelectVideoByID(DataID);
                if (video == null || video.DataID <= 0) {
                    Logger.Error($"can not find video by id[{DataID}]");
                    return;
                }

                ScreenShot shot = new ScreenShot(video, Token);
                shot.onProgress += (s, e) => {
                    ScreenShot screenShot = s as ScreenShot;
                    Progress = (float)Math.Round(((float)screenShot.CurrentTaskCount + 1) / screenShot.TotalTaskCount, 4) * 100;
                };

                shot.onError += (s, e) => {
                    MessageCallBackEventArgs arg = e as MessageCallBackEventArgs;
                    if (!string.IsNullOrEmpty(arg.Message))
                        Logger.Error(arg.Message);
                };

                try {
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
                } catch (Exception ex) {
                    StatusText = ex.Message;
                    Logger.Error(ex.Message);
                    FinalizeWithCancel();
                }

                OnCompleted(null);
            });
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is ScreenShotTask other)
                return other.DataID.Equals(DataID);
            return false;
        }

        public override int GetHashCode()
        {
            return DataID.GetHashCode();
        }

        #region "对外方法"
        public static void ScreenShotVideo(Video video, bool gif = false)
        {
            long dataID = video.DataID;
            ScreenShotTask screenShotTask = new ScreenShotTask(video, gif);
            screenShotTask.onError += (e, v) => onScreenShotError((v as MessageCallBackEventArgs).Message);
            screenShotTask.onCompleted += (e, v) => onScreenShotCompleted((e as ScreenShotTask).Success, dataID);
            AddToScreenShot(screenShotTask);
        }



        public static void AddToScreenShot(ScreenShotTask task)
        {
            if (App.ScreenShotManager.Exists(task)) {
                MessageNotify.Warning(LangManager.GetValueByKey("TaskExists"));
                return;
            }

            App.ScreenShotManager.AddTask(task);
        }

        #endregion
    }
}
