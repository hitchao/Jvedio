using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.VisualTools.WindowHelper;
using static SuperUtils.WPF.VisualTools.VisualHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_Edit.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Edit : SuperControls.Style.BaseWindow
    {
        private Main main { get; set; }

        private Window_Details windowDetails { get; set; }

        VieModel_Edit vieModel { get; set; }

        public Window_Edit(long dataID)
        {
            InitializeComponent();

            Init();
            vieModel = new VieModel_Edit(dataID);
            this.DataContext = vieModel;
            ReLoad();
        }

        private void Init()
        {
            main = GetWindowByName("Main") as Main;
            windowDetails = GetWindowByName("Window_Details") as Window_Details;
            if (StyleManager.GlobalFont != null) this.FontFamily = StyleManager.GlobalFont; // 设置字体
        }

        public void ReLoad()
        {
            genreTagPanel.TagList = vieModel.CurrentVideo.GenreList;
            genreTagPanel.Refresh();
            labelTagPanel.TagList = vieModel.CurrentVideo.LabelList;
            labelTagPanel.Refresh();
            subSectionTagPanel.TagList = vieModel.CurrentVideo.SubSectionList;
            subSectionTagPanel.Refresh();
        }

        private void SaveInfo(object sender, RoutedEventArgs e)
        {
            bool success = vieModel.Save();
            if (success)
            {
                vieModel.Reset();
                ReLoad();

                // 更新到主界面和详情界面
                main?.RefreshData(vieModel.CurrentVideo.DataID);
                windowDetails?.Refresh();
                SuperControls.Style.MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
            }
            else
            {
                SuperControls.Style.MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_Fail"));
            }
        }

        private void ChoseMovieBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        private void ChoseMovieBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragdropFiles == null || dragdropFiles.Length == 0) return;

            if (dragdropFiles.Length == 1)
            {
                string path = dragdropFiles[0];
                if (ScanHelper.IsProperMovie(path)) vieModel.CurrentVideo.Path = path;
            }
            else
            {
                vieModel.CurrentVideo.Path = dragdropFiles[0];
                if (vieModel.CurrentVideo.SubSectionList == null)
                    vieModel.CurrentVideo.SubSectionList = new System.Collections.Generic.List<string>();
                foreach (var file in dragdropFiles)
                {
                    if (vieModel.CurrentVideo.SubSectionList.Contains(file)) continue;
                    if (FileHelper.IsFile(file) && ScanHelper.IsProperMovie(file))
                    {
                        vieModel.CurrentVideo.SubSectionList.Add(file);
                    }
                }

                vieModel.CurrentVideo.SubSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, vieModel.CurrentVideo.SubSectionList);
                ReLoad();
            }

            calcSize();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime date = DateTime.Now;
            bool success = DateTime.TryParse((sender as SearchBox).Text, out date);
            if (success)
            {
                this.vieModel.CurrentVideo.ReleaseDate = date.ToString("yyyy-MM-dd ");
            }
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Edit.MoreExpanded = vieModel.MoreExpanded;
            ConfigManager.Edit.Save();
        }

        private void NewGenre(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, LangManager.GetValueByKey("PleaseEnter"));
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                if (vieModel.CurrentVideo.GenreList == null)
                    vieModel.CurrentVideo.GenreList = new System.Collections.Generic.List<string>();
                vieModel.CurrentVideo.GenreList.Add(text);
                vieModel.CurrentVideo.Genre = string.Join(SuperUtils.Values.ConstValues.SeparatorString, vieModel.CurrentVideo.GenreList);
                genreTagPanel.TagList = null;
                genreTagPanel.TagList = vieModel.CurrentVideo.GenreList;
                genreTagPanel.Refresh();
            }
        }

        private void NewLabel(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, LangManager.GetValueByKey("PleaseEnter"));
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                addLabel(text);
            }
        }

        private void addLabel(string label)
        {
            if (vieModel.CurrentVideo.LabelList == null)
                vieModel.CurrentVideo.LabelList = new System.Collections.Generic.List<string>();
            if (vieModel.CurrentVideo.LabelList.Contains(label)) return;
            vieModel.CurrentVideo.LabelList.Add(label);
            vieModel.CurrentVideo.Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, vieModel.CurrentVideo.LabelList);
            labelTagPanel.TagList = null;
            labelTagPanel.TagList = vieModel.CurrentVideo.LabelList;
            labelTagPanel.Refresh();
        }

        private void GenreChanged(object sender, SuperControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
                vieModel.CurrentVideo.Genre = string.Join(SuperUtils.Values.ConstValues.SeparatorString, e.List);
        }

        private void LabelChanged(object sender, SuperControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
                vieModel.CurrentVideo.Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, e.List);
        }

        private void SubSectionChanged(object sender, SuperControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
            {
                vieModel.CurrentVideo.SubSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, e.List);
                calcSize();
            }
        }

        private void NewSubSection(object sender, RoutedEventArgs e)
        {
            List<string> fileNames = SelectVideo(string.Empty, true);
            if (vieModel.CurrentVideo.SubSectionList == null)
                vieModel.CurrentVideo.SubSectionList = new System.Collections.Generic.List<string>();
            vieModel.CurrentVideo.SubSectionList.AddRange(fileNames);
            vieModel.CurrentVideo.SubSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, vieModel.CurrentVideo.SubSectionList);
            subSectionTagPanel.TagList = null;
            subSectionTagPanel.TagList = vieModel.CurrentVideo.SubSectionList;
            calcSize();
            subSectionTagPanel.Refresh();
        }

        private void ChooseFile(object sender, MouseButtonEventArgs e)
        {


            List<string> fileNames = SelectVideo(vieModel.CurrentVideo.Path);
            if (fileNames.Count > 0)
            {
                vieModel.CurrentVideo.Path = fileNames[0];
                calcSize();
            }
        }

        public void calcSize()
        {
            long total = 0;
            if (vieModel.CurrentVideo.SubSectionList != null && vieModel.CurrentVideo.SubSectionList.Count > 0)
            {
                foreach (var item in vieModel.CurrentVideo.SubSectionList)
                {
                    if (File.Exists(item)) total += new FileInfo(item).Length;
                }
            }
            else
            {
                total = File.Exists(vieModel.CurrentVideo.Path) ? new FileInfo(vieModel.CurrentVideo.Path).Length : 0;
            }

            vieModel.CurrentVideo.Size = total;
            vieModel.CurrentVideo.LastScanDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public List<string> SelectVideo(string path, bool multi = false)
        {
            List<string> result = new List<string>();
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFile");
            fileDialog.FileName = path;
            fileDialog.Filter = Window_Settings.SupportVideoFormat;
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            fileDialog.Multiselect = multi;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                fileDialog.FileNames != null)
                result.AddRange(fileDialog.FileNames.Where(arg => File.Exists(arg)));
            return result;
        }

        private void AddToLabel(object sender, RoutedEventArgs e)
        {
            searchLabelPopup.IsOpen = true;
        }

        private void newActor_Cancel(object sender, RoutedEventArgs e)
        {
            searchActorPopup.IsOpen = false;
        }

        private void newActor_Confirm(object sender, RoutedEventArgs e)
        {
            searchActorPopup.IsOpen = false;

            long actorID = vieModel.ActorID;
            if (actorID <= 0) return;
            if (vieModel.CurrentVideo.ActorNameList == null)
                vieModel.CurrentVideo.ActorNameList = new System.Collections.Generic.List<string>();
            if (vieModel.CurrentVideo.ActorInfos == null)
                vieModel.CurrentVideo.ActorInfos = new System.Collections.Generic.List<ActorInfo>();
            ActorInfo actorInfo = MapperManager.actorMapper.SelectOne(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            if (actorInfo != null && !vieModel.ViewActors.Contains(actorInfo))
                vieModel.ViewActors.Add(actorInfo);
        }

        private void CurrentActorPageChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.CurrentActorPage = pagination.CurrentPage;
            VieModel_Main.ActorPageQueue.Enqueue(pagination.CurrentPage);
            vieModel.SelectActor();
        }

        private void ActorPageSizeChange(object sender, EventArgs e)
        {
            Pagination pagination = sender as Pagination;
            vieModel.ActorPageSize = pagination.PageSize;
        }

        private long getDataID(UIElement o)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element == null) return -1;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (grid != null && grid.Tag != null)
            {
                long.TryParse(grid.Tag.ToString(), out long result);
                return result;
            }

            return -1;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            long actorID = getDataID(sender as FrameworkElement);
            ActorInfo actorInfo = vieModel.CurrentActorList.Where(arg => arg.ActorID == actorID).FirstOrDefault();
            if (actorInfo != null && !string.IsNullOrEmpty(actorInfo.ActorName))
            {
                vieModel.ActorName = actorInfo.ActorName;
                vieModel.ActorID = actorInfo.ActorID;
                vieModel.CurrentImage = actorInfo.SmallImage;
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            border.Background = StyleManager.Common.HighLight.Background;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];
        }

        private void NewActor(object sender, MouseButtonEventArgs e)
        {
            searchActorPopup.IsOpen = true;
            vieModel.SelectActor();
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType.Equals(PathType.RelativeToData))
                MessageCard.Info(LangManager.GetValueByKey("ShowActorImageWarning"));
        }

        private void DeleteActor(object sender, MouseButtonEventArgs e)
        {
            long actorID = getDataID(sender as FrameworkElement);

            for (int i = vieModel.ViewActors.Count - 1; i >= 0; i--)
            {
                if (vieModel.ViewActors[i].ActorID == actorID)
                {
                    vieModel.ViewActors.RemoveAt(i);
                    break;
                }
            }
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TextBlock textBlock = border.Child as TextBlock;
            string text = textBlock.Text;
            addLabel(text.Substring(0, text.IndexOf("(")));
            searchLabelPopup.IsOpen = false;
        }
    }
}
