using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.ViewModel;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static SuperUtils.WPF.VisualTools.VisualHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_Edit.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Edit : BaseWindow
    {

        #region "属性"
        private Main main { get; set; }

        private Window_Details windowDetails { get; set; }

        private VieModel_Edit vieModel { get; set; }
        #endregion

        public static Action<long> onRefreshData { get; set; }

        public Window_Edit(long dataID)
        {
            InitializeComponent();

            vieModel = new VieModel_Edit(dataID, this);
            this.DataContext = vieModel;

            Init();
        }

        private void Init()
        {
            main = GetWindowByName("Main", App.Current.Windows) as Main;
            windowDetails = GetWindowByName("Window_Details", App.Current.Windows) as Window_Details;
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BindingEvent();
        }


        private void BindingEvent()
        {
            actorList.onShowSameActor += SelectOneActor;
        }

        private void SelectOneActor(long actorID)
        {
            ActorInfo actorInfo = actorList.CurrentList.FirstOrDefault(arg => arg.ActorID == actorID);
            if (actorInfo != null && !string.IsNullOrEmpty(actorInfo.ActorName)) {
                vieModel.ActorName = actorInfo.ActorName;
                vieModel.ActorID = actorInfo.ActorID;
                vieModel.CurrentImage = actorInfo.SmallImage;
            }
        }

        private async void SaveInfo(object sender, RoutedEventArgs e)
        {
            vieModel.Saving = true;
            bool success = await vieModel.Save();
            if (success) {
                vieModel.Init();
                onRefreshData?.Invoke(vieModel.CurrentVideo.DataID);
                SuperControls.Style.MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
            } else {
                SuperControls.Style.MessageNotify.Error(SuperControls.Style.LangManager.GetValueByKey("Message_Fail"));
            }
            vieModel.Saving = false;
        }

        private void ChoseMovieBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }

        private void ChoseMovieBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragdropFiles == null || dragdropFiles.Length == 0)
                return;

            if (dragdropFiles.Length == 1) {
                string path = dragdropFiles[0];
                if (VideoParser.IsProperMovie(path))
                    vieModel.CurrentVideo.Path = path;
            } else {
                vieModel.CurrentVideo.Path = dragdropFiles[0];
                if (vieModel.CurrentVideo.SubSectionList == null)
                    vieModel.CurrentVideo.SubSectionList = new ObservableCollection<ObservableString>();
                foreach (var file in dragdropFiles) {
                    ObservableString str = new ObservableString(file);
                    if (vieModel.CurrentVideo.SubSectionList.Contains(str))
                        continue;
                    if (FileHelper.IsFile(file) && VideoParser.IsProperMovie(file)) {
                        vieModel.CurrentVideo.SubSectionList.Add(str);
                    }
                }

                vieModel.CurrentVideo.SubSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, vieModel.CurrentVideo.SubSectionList);
            }

            CalcSize();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime date = DateTime.Now;
            bool success = DateTime.TryParse((sender as SearchBox).Text, out date);
            if (success) {
                this.vieModel.CurrentVideo.ReleaseDate = date.ToString("yyyy-MM-dd ");
            }
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Edit.MoreExpanded = vieModel.MoreExpanded;
            ConfigManager.Edit.Save();
        }


        private void GenreChanged(object sender, RoutedEventArgs eventArgs)
        {
            List<string> list = new List<string>();
            if (vieModel.CurrentVideo.GenreList != null)
                list = vieModel.CurrentVideo.GenreList.Select(arg => arg.Value).ToList();
            vieModel.CurrentVideo.Genre = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);

            windowDetails?.RefreshGenre(vieModel.CurrentVideo.DataID, vieModel.CurrentVideo.Genre);
        }

        private void LabelChanged(object sender, RoutedEventArgs eventArgs)
        {
            List<string> list = new List<string>();
            if (vieModel.CurrentVideo.LabelList != null)
                list = vieModel.CurrentVideo.LabelList.Select(arg => arg.Value).ToList();
            vieModel.CurrentVideo.Label = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);
            MapperManager.metaDataMapper.SaveLabel(vieModel.CurrentVideo.toMetaData());
            windowDetails?.RefreshLabel(vieModel.CurrentVideo.DataID, vieModel.CurrentVideo.Label);
            vieModel.GetLabels();
        }

        private void SeriesChanged(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            if (vieModel.CurrentVideo.SeriesList != null)
                list = vieModel.CurrentVideo.SeriesList.Select(arg => arg.Value).ToList();
            vieModel.CurrentVideo.Series = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);

            windowDetails?.RefreshSeries(vieModel.CurrentVideo.DataID, vieModel.CurrentVideo.Series);
        }

        private void SubSectionChanged(object sender, RoutedEventArgs eventArgs)
        {
            List<string> list = new List<string>();
            if (vieModel.CurrentVideo.SubSectionList != null)
                list = vieModel.CurrentVideo.SubSectionList.Select(arg => arg.Value).ToList();
            vieModel.CurrentVideo.SubSection = string.Join(SuperUtils.Values.ConstValues.SeparatorString, list);
            CalcSize();
        }

        private void NewSubSection(object sender, RoutedEventArgs e)
        {
            List<string> fileNames = SelectVideo(string.Empty, true);
            if (vieModel.CurrentVideo.SubSectionList == null)
                vieModel.CurrentVideo.SubSectionList = new ObservableCollection<ObservableString>();

            foreach (var item in fileNames) {
                vieModel.CurrentVideo.SubSectionList.Add(new ObservableString(item));
            }

            // 默认原路径是否存在
            bool existDefault = vieModel.CurrentVideo.SubSectionList.Any(arg => arg.Value.Equals(vieModel.CurrentVideo.Path));
            if (!existDefault)
                vieModel.CurrentVideo.SubSectionList.Insert(0, new ObservableString(vieModel.CurrentVideo.Path));

            vieModel.CurrentVideo.SubSection =
                string.Join(SuperUtils.Values.ConstValues.SeparatorString,
                vieModel.CurrentVideo.SubSectionList.Select(arg => arg.Value));

            CalcSize();
        }



        public void CalcSize()
        {
            long total = 0;
            if (vieModel.CurrentVideo.SubSectionList != null && vieModel.CurrentVideo.SubSectionList.Count > 0) {
                foreach (var item in vieModel.CurrentVideo.SubSectionList.Select(arg => arg.Value)) {
                    if (File.Exists(item))
                        total += new FileInfo(item).Length;
                }
            } else {
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
            if (actorID <= 0)
                return;
            if (vieModel.CurrentVideo.ActorNameList == null)
                vieModel.CurrentVideo.ActorNameList = new System.Collections.Generic.List<string>();
            if (vieModel.CurrentVideo.ActorInfos == null)
                vieModel.CurrentVideo.ActorInfos = new System.Collections.Generic.List<ActorInfo>();
            ActorInfo actorInfo = MapperManager.actorMapper.SelectOne(new SelectWrapper<ActorInfo>().Eq("ActorID", actorID));
            if (actorInfo != null && !vieModel.ViewActors.Contains(actorInfo))
                vieModel.ViewActors.Add(actorInfo);
        }



        private long GetDataID(UIElement o)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element == null)
                return -1;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            if (grid != null && grid.Tag != null) {
                long.TryParse(grid.Tag.ToString(), out long result);
                return result;
            }

            return -1;
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

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TextBlock textBlock = border.Child as TextBlock;
            string text = textBlock.Text;
            string value = text.Substring(0, text.IndexOf("("));
            ObservableString observableString = new ObservableString(value);
            if (vieModel.CurrentVideo.LabelList.Contains(observableString)) {
                searchLabelPopup.IsOpen = false;
                return;
            }
            vieModel.CurrentVideo.LabelList.Add(observableString);
            LabelChanged(null, null);
            searchLabelPopup.IsOpen = false;
        }

        private void NewActor(object sender, RoutedEventArgs e)
        {
            searchActorPopup.IsOpen = true;
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType.Equals(PathType.RelativeToData))
                MessageCard.Info(LangManager.GetValueByKey("ShowActorImageWarning"));
        }

        private void DeleteActor(object sender, RoutedEventArgs e)
        {
            long actorID = GetDataID(sender as FrameworkElement);

            for (int i = vieModel.ViewActors.Count - 1; i >= 0; i--) {
                if (vieModel.ViewActors[i].ActorID == actorID) {
                    vieModel.ViewActors.RemoveAt(i);
                    break;
                }
            }
        }

        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            List<string> fileNames = SelectVideo(vieModel.CurrentVideo.Path);
            if (fileNames.Count > 0) {
                vieModel.CurrentVideo.Path = fileNames[0];
                CalcSize();
            }
        }

        private void onTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.GotFocus(sender);
        }

        private void onTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.LostFocus(sender);
        }
    }
}
