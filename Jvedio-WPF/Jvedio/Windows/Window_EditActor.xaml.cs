using ChaoControls.Style;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Utils;
using Jvedio.Utils.IO;
using System;
using System.IO;
using System.Windows;
using static Jvedio.GlobalMapper;
using static Jvedio.Utils.Visual.VisualHelper;
using static Jvedio.Utils.Media.ImageHelper;
namespace Jvedio
{
    /// <summary>
    /// Window_EditActor.xaml 的交互逻辑
    /// </summary>
    public partial class Window_EditActor : BaseWindow
    {


        private Main main { get; set; }
        private Window_EditActor()
        {
            InitializeComponent();
            main = GetWindowByName("Main") as Main;
        }

        public long ActorID { get; set; }
        public ActorInfo CurrentActorInfo { get; set; }

        public Window_EditActor(long actorID) : this()
        {
            this.ActorID = actorID;
            this.DataContext = this;
            CurrentActorInfo = new ActorInfo();
            LoadActor();
        }



        public void LoadActor()
        {
            if (this.ActorID <= 0) return;
            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            wrapper.Eq("ActorID", this.ActorID);
            ActorInfo actorInfo = actorMapper.selectById(wrapper);
            if (actorInfo == null) return;
            ActorInfo.SetImage(ref actorInfo);
            CurrentActorInfo = null;
            CurrentActorInfo = actorInfo;
        }



        private void SaveActor(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentActorInfo.ActorName))
            {
                MessageCard.Error("演员名称不可为空！");
                return;
            }
            if (ActorID > 0)
            {
                int update = actorMapper.updateById(CurrentActorInfo);
                if (update > 0)
                {
                    MessageCard.Success(Jvedio.Language.Resources.Message_Success);
                    main?.RefreshActor(CurrentActorInfo.ActorID);
                }

            }
            else
            {
                // 新增
                // 检查是否存在
                SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
                wrapper.Eq("ActorName", CurrentActorInfo.ActorName.ToProperSql());
                ActorInfo actorInfo = actorMapper.selectOne(wrapper);
                bool insert = true;
                if (actorInfo != null && !string.IsNullOrEmpty(actorInfo.ActorName))
                {
                    insert = (bool)new Msgbox(this, $"数据库中已有和 {actorInfo.ActorName} 同名的演员，是否继续添加？").ShowDialog();
                }
                if (insert)
                {
                    actorMapper.insert(CurrentActorInfo);
                    if (CurrentActorInfo.ActorID > 0)
                    {
                        this.DialogResult = true;
                    }
                    else
                        MessageCard.Success("添加失败！");
                }
            }

        }

        private void SetActorImage(object sender, RoutedEventArgs e)
        {
            string imageFileName = "";
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseFile;
            OpenFileDialog1.Filter = GlobalVariable.SupportPictureFormat;
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = OpenFileDialog1.FileName;
                if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                    imageFileName = filename;
            }
            bool copyed = false;
            string targetFileName = CurrentActorInfo.getImagePath(searchExt: false);
            if (File.Exists(targetFileName))
            {
                if (new Msgbox(this, "该图片已存在，是否使用演员识别码命名？").ShowDialog() == true)
                {
                    string dir = System.IO.Path.GetDirectoryName(targetFileName);
                    string ext = System.IO.Path.GetExtension(targetFileName);
                    targetFileName = System.IO.Path.Combine(dir, $"{CurrentActorInfo.ActorID}_{CurrentActorInfo.ActorName}{ext}");
                    FileHelper.TryCopyFile(imageFileName, targetFileName, true);
                    copyed = true;
                }
            }
            else
            {
                FileHelper.TryCopyFile(imageFileName, targetFileName);
                copyed = true;
            }
            if (copyed)
            {
                // 设置图片
                CurrentActorInfo.SmallImage = null;
                CurrentActorInfo.SmallImage = BitmapImageFromFile(targetFileName);

                main?.RefreshActor(CurrentActorInfo.ActorID);
            }

        }
    }
}
