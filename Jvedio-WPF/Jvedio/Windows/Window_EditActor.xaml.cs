using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using System.IO;
using System.Linq;
using System.Windows;
using static Jvedio.MapperManager;
using static SuperUtils.Media.ImageHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

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
            main = GetWindowByName("Main", App.Current.Windows) as Main;
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
            if (this.ActorID <= 0)
                return;
            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            wrapper.Eq("ActorID", this.ActorID);
            ActorInfo actorInfo = actorMapper.SelectById(wrapper);
            if (actorInfo == null)
                return;
            ActorInfo.SetImage(ref actorInfo);
            CurrentActorInfo = null;
            CurrentActorInfo = actorInfo;
        }

        private void SaveActor(object sender, RoutedEventArgs e)
        {
            CurrentActorInfo.ActorName = CurrentActorInfo.ActorName.ToProperFileName();
            if (string.IsNullOrEmpty(CurrentActorInfo.ActorName)) {
                MessageNotify.Error(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }

            if (ActorID > 0) {
                int update = actorMapper.UpdateById(CurrentActorInfo);
                if (update > 0) {
                    MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
                    main?.RefreshActor(CurrentActorInfo.ActorID);
                }
            } else {
                // 新增
                // 检查是否存在
                SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
                wrapper.Eq("ActorName", CurrentActorInfo.ActorName.ToProperSql());
                ActorInfo actorInfo = actorMapper.SelectOne(wrapper);
                bool insert = true;
                if (actorInfo != null && !string.IsNullOrEmpty(actorInfo.ActorName)) {
                    insert = (bool)new MsgBox($"{LangManager.GetValueByKey("LibraryAlreadyHas")} {actorInfo.ActorName} {LangManager.GetValueByKey("SameActorToAdd")}").ShowDialog();
                }

                if (insert) {
                    actorMapper.Insert(CurrentActorInfo);
                    if (CurrentActorInfo.ActorID > 0) {
                        this.DialogResult = true;
                    } else
                        MessageNotify.Error(LangManager.GetValueByKey("Error"));
                }
            }
        }

        private void SetActorImage(object sender, RoutedEventArgs e)
        {
            if (CurrentActorInfo == null || string.IsNullOrEmpty(CurrentActorInfo.ActorName)) {
                MsgBox.Show(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }
            string imageFileName = string.Empty;
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFile");
            openFileDialog1.Filter = Window_Settings.SupportPictureFormat;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string filename = openFileDialog1.FileName;
                if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                    imageFileName = filename;
            }

            bool copied = false;
            string targetFileName = CurrentActorInfo.GetImagePath(searchExt: false);
            if (File.Exists(targetFileName)) {
                if (new MsgBox(LangManager.GetValueByKey("ActorImageExistsAndUseID")).ShowDialog() == true) {
                    string dir = System.IO.Path.GetDirectoryName(targetFileName);
                    string ext = System.IO.Path.GetExtension(targetFileName);
                    targetFileName = System.IO.Path.Combine(dir, $"{CurrentActorInfo.ActorID}_{CurrentActorInfo.ActorName}{ext}");
                    FileHelper.TryCopyFile(imageFileName, targetFileName, true);
                    copied = true;
                }
            } else {
                FileHelper.TryCopyFile(imageFileName, targetFileName);
                copied = true;
            }

            if (copied) {
                // 设置图片
                CurrentActorInfo.SmallImage = null;
                CurrentActorInfo.SmallImage = BitmapImageFromFile(targetFileName);

                main?.RefreshActor(CurrentActorInfo.ActorID);
            }
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData) {
                MsgBox.Show(LangManager.GetValueByKey("ActorImageNotSupported"));
                return;
            }




            if (CurrentActorInfo == null || string.IsNullOrEmpty(CurrentActorInfo.ActorName)) {
                MsgBox.Show(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }

            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            string saveDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePicPath, "Actresses"));
            string name = CurrentActorInfo.ActorName.ToProperFileName();
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragdropFiles != null && dragdropFiles.Length > 0) {
                System.Collections.Generic.List<string> list = dragdropFiles.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();
                if (list.Count > 0) {
                    string originPath = list[0];
                    if (FileHelper.IsFile(originPath)) {
                        // 设置演员头像
                        string targetFileName = CurrentActorInfo.GetImagePath(searchExt: false);
                        bool copy = true;
                        if (File.Exists(targetFileName) && new MsgBox(LangManager.GetValueByKey("ExistsToOverwrite")).ShowDialog() != true) {
                            copy = false;
                        }
                        if (copy) {
                            FileHelper.TryCopyFile(originPath, targetFileName, true);
                            CurrentActorInfo.SmallImage = ImageHelper.BitmapImageFromFile(targetFileName);
                        }

                    }
                }
            }

        }
    }
}
