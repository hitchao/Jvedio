using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        #region "事件"
        public static Action<long> onActorInfoChanged;

        #endregion


        #region "属性"
        public long ActorID { get; set; }

        public ActorInfo CurrentActorInfo { get; set; }


        #endregion

        private Window_EditActor()
        {
            InitializeComponent();
        }


        public Window_EditActor(long actorID, bool newActor = false) : this()
        {
            this.DataContext = this;

            if (newActor) {
                CurrentActorInfo = new ActorInfo();
            } else {
                if (actorID <= 0)
                    return;
                this.ActorID = actorID;
                Init();
            }

        }

        public void Init()
        {
            if (this.ActorID <= 0)
                return;
            CurrentActorInfo = ActorInfo.GetById(ActorID);
            SetGender();
        }

        private void SetGender()
        {
            if (CurrentActorInfo == null)
                return;
            if (CurrentActorInfo.Gender == Gender.Boy)
                boy.IsChecked = true;
            else
                girl.IsChecked = true;
        }

        private void SaveActor(object sender, RoutedEventArgs e)
        {
            string actorName = CurrentActorInfo.ActorName.ToProperFileName();
            if (string.IsNullOrEmpty(actorName)) {
                MessageNotify.Error(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }

            CurrentActorInfo.ActorName = actorName;

            if (ActorID > 0) {
                int update = actorMapper.UpdateById(CurrentActorInfo);
                if (update > 0) {
                    ActorInfo actorInfo = ActorInfo.GetById(ActorID);
                    CurrentActorInfo.SmallImage = null;
                    CurrentActorInfo.SmallImage = actorInfo.SmallImage;

                    MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
                    onActorInfoChanged(ActorID);
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
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFile");
            fileDialog.Filter = Window_Settings.SupportPictureFormat;
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string filename = fileDialog.FileName;
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                imageFileName = filename;


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
                SetCurrentImage(targetFileName);
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
                            SetCurrentImage(targetFileName);
                        }

                    }
                }
            }
        }

        private void SetCurrentImage(string targetFileName)
        {
            CurrentActorInfo.SmallImage = null;
            CurrentActorInfo.SmallImage = BitmapImageFromFile(targetFileName);
            onActorInfoChanged?.Invoke(CurrentActorInfo.ActorID);
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }


        private void SetGender(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button &&
                button.Parent is StackPanel panel &&
                panel.Children.OfType<RadioButton>().ToList() is List<RadioButton> buttonList) {
                int idx = buttonList.IndexOf(button);
                if (idx == 0) {
                    CurrentActorInfo.Gender = Gender.Boy;
                } else {
                    CurrentActorInfo.Gender = Gender.Girl;
                }
            }
        }

        private void onTextBoxFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.GotFocus(sender);
        }

        private void onTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.LostFocus(sender);
        }
    }
}