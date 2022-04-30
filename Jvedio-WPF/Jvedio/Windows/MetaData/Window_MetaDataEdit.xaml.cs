using ChaoControls.Style;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity;
using Jvedio.Style;
using Jvedio.Utils;
using Jvedio.Utils.Visual;
using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.FileProcess;
namespace Jvedio
{
    /// <summary>
    /// WindowEdit.xaml 的交互逻辑
    /// </summary>
    public partial class Window_MetaDataEdit : ChaoControls.Style.BaseWindow
    {

        Window_MetaDatas window_MetaDatas = GetWindowByName("Window_MetaDatas") as Window_MetaDatas;


        VieModel_MetaDataEdit vieModel;

        public DataType CurrentDataType = DataType.Picture;

        public Window_MetaDataEdit() : this(539)
        {
            this.MaxHeight = SystemParameters.WorkArea.Height - 50;
        }

        public Window_MetaDataEdit(long dataID, DataType dataType = DataType.Picture)
        {
            InitializeComponent();
            CurrentDataType = dataType;
            vieModel = new VieModel_MetaDataEdit(dataID);
            this.DataContext = vieModel;
            ReLoad();

        }


        public void ReLoad()
        {
            //actorTagPanel.TagList = vieModel.CurrentData.ActorNameList;
            //actorTagPanel.Refresh();
            genreTagPanel.TagList = vieModel.CurrentData.GenreList;
            genreTagPanel.Refresh();
            labelTagPanel.TagList = vieModel.CurrentData.LabelList;
            labelTagPanel.Refresh();

        }



        private void UpdateMain(string oldID, string newID)
        {
            //Main main = App.Current.Windows[0] as Main;
            //Movie movie = SelectMovie(newID);
            //addTag(ref movie);
            //movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");
            //movie.bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");

            //for (int i = 0; i < main.vieModel.CurrentMovieList.Count; i++)
            //{
            //    try
            //    {
            //        if (main.vieModel.CurrentMovieList[i]?.id.ToUpper() == oldID.ToUpper())
            //        {
            //            main.vieModel.CurrentMovieList[i] = null;
            //            main.vieModel.CurrentMovieList[i] = movie;
            //            break;
            //        }
            //    }
            //    catch { }
            //}


            //for (int i = 0; i < main.vieModel.MovieList.Count; i++)
            //{
            //    try
            //    {
            //        if (main.vieModel.MovieList[i]?.id.ToUpper() == oldID.ToUpper())
            //        {
            //            main.vieModel.MovieList[i] = null;
            //            main.vieModel.MovieList[i] = movie;
            //            break;
            //        }
            //    }
            //    catch { }
            //}

            //for (int i = 0; i < main.vieModel.FilterMovieList.Count; i++)
            //{
            //    try
            //    {
            //        if (main.vieModel.FilterMovieList[i]?.id.ToUpper() == oldID.ToUpper())
            //        {
            //            main.vieModel.FilterMovieList[i] = null;
            //            main.vieModel.FilterMovieList[i] = movie;
            //            break;
            //        }
            //    }
            //    catch { }
            //}
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = vieModel.Save();
            if (success)
            {
                vieModel.Reset();
                ReLoad();
                // todo 更新到主界面和详情界面
                window_MetaDatas?.RefreshData(vieModel.CurrentData.DataID);
                ChaoControls.Style.MessageCard.Success(Jvedio.Language.Resources.Message_Success);
            }
            else
            {
                ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.Message_Fail);
            }

        }




        private void ChoseMovieBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }



        private void ChoseMovieBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragdropFiles == null || dragdropFiles.Length == 0) return;

            if (dragdropFiles.Length == 1)
            {
                string path = dragdropFiles[0];
                if (ScanHelper.IsProperMovie(path)) vieModel.CurrentData.Path = path;
            }
            else
            {
                //vieModel.CurrentData.Path = dragdropFiles[0];
                //if (vieModel.CurrentData.SubSectionList == null)
                //    vieModel.CurrentData.SubSectionList = new System.Collections.Generic.List<string>();
                //foreach (var file in dragdropFiles)
                //{
                //    if (vieModel.CurrentData.SubSectionList.Contains(file)) continue;
                //    if (FileHelper.IsFile(file) && ScanHelper.IsProperMovie(file))
                //    {
                //        vieModel.CurrentData.SubSectionList.Add(file);
                //    }
                //}
                //vieModel.CurrentData.SubSection = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentData.SubSectionList);
                //ReLoad();
            }
            calcSize();
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragdropFiles == null || dragdropFiles.Length == 0) return;

            if (dragdropFiles.Length == 1)
            {
                string path = dragdropFiles[0];
                if (ScanHelper.IsProperMovie(path)) vieModel.CurrentData.Path = path;
            }
        }



        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime date = DateTime.Now; ;
            bool success = DateTime.TryParse((sender as SearchBox).Text, out date);
            if (success)
            {
                this.vieModel.CurrentData.ReleaseDate = date.ToString("yyyy-MM-dd ");
            }
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalConfig.Edit.MoreExpanded = vieModel.MoreExpanded;
            GlobalConfig.Edit.Save();
        }


        private void NewGenre(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, "请输入");
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                if (vieModel.CurrentData.GenreList == null)
                    vieModel.CurrentData.GenreList = new System.Collections.Generic.List<string>();
                vieModel.CurrentData.GenreList.Add(text);
                vieModel.CurrentData.Genre = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentData.GenreList);
                genreTagPanel.TagList = null;
                genreTagPanel.TagList = vieModel.CurrentData.GenreList;
                genreTagPanel.Refresh();
            }
        }

        private void NewLabel(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, "请输入");
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                addLabel(text);
            }
        }


        private void addLabel(string label)
        {
            if (vieModel.CurrentData.LabelList == null)
                vieModel.CurrentData.LabelList = new System.Collections.Generic.List<string>();
            if (vieModel.CurrentData.LabelList.Contains(label)) return;
            vieModel.CurrentData.LabelList.Add(label);
            vieModel.CurrentData.Label = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentData.LabelList);
            labelTagPanel.TagList = null;
            labelTagPanel.TagList = vieModel.CurrentData.LabelList;
            labelTagPanel.Refresh();
        }

        private void GenreChanged(object sender, ChaoControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
                vieModel.CurrentData.Genre = String.Join(GlobalVariable.Separator.ToString(), e.List);


        }

        private void LabelChanged(object sender, ChaoControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
                vieModel.CurrentData.Label = String.Join(GlobalVariable.Separator.ToString(), e.List);

        }

        private void ActorChanged(object sender, ChaoControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
            {
                foreach (var item in e.List)
                {
                    Console.WriteLine(item);
                }

                //vieModel.CurrentData.Label = String.Join(GlobalVariable.Separator.ToString(), e.List);
            }

        }




        private void ChooseFile(object sender, MouseButtonEventArgs e)
        {
            string path = vieModel.CurrentData.Path;
            string result = "";
            if (CurrentDataType == DataType.Game)
            {
                result = SelectData(path, "*.exe|*.exe");
            }
            else
            {
                result = FileHelper.SelectPath(this, path);
            }
            if (!string.IsNullOrEmpty(result) && !result.Equals(path))
            {
                vieModel.CurrentData.Path = result;

                // 重新扫描
                if (CurrentDataType == DataType.Picture)
                {
                    IEnumerable<string> enumerable = DirHelper.GetFileList(result);
                    List<string> videoPaths = enumerable.Where(arg => ScanTask.VIDEO_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();
                    List<string> imgPaths = enumerable.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();
                    vieModel.CurrentPicture.PicCount = imgPaths.Count;
                    vieModel.CurrentPicture.PicPaths = string.Join(GlobalVariable.Separator.ToString(), imgPaths.Select(arg => Path.GetFileName(arg)));
                    vieModel.CurrentPicture.VideoPaths = String.Join(GlobalVariable.Separator.ToString(), videoPaths);
                }
                else if (CurrentDataType == DataType.Comics)
                {
                    IEnumerable<string> enumerable = DirHelper.GetFileList(result);
                    List<string> imgPaths = enumerable.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(Path.GetExtension(arg).ToLower())).ToList();
                    vieModel.CurrentComic.PicCount = imgPaths.Count;
                    vieModel.CurrentComic.PicPaths = string.Join(GlobalVariable.Separator.ToString(), imgPaths.Select(arg => Path.GetFileName(arg)));
                }



                calcSize();
            }

        }

        public string SelectData(string path, string filter)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = Jvedio.Language.Resources.ChooseFile;
            fileDialog.FileName = Path.GetFileName(path);
            fileDialog.InitialDirectory = Path.GetDirectoryName(path);
            fileDialog.Filter = filter;
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = fileDialog.FileName;
                if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                    return filename;
            }
            return path;
        }

        public void calcSize()
        {
            string path = vieModel.CurrentData.Path;
            if (FileHelper.IsFile(path)) path = Path.GetDirectoryName(path);
            long total = Directory.Exists(path) ? DirHelper.getDirSize(new DirectoryInfo(path)) : 0;

            vieModel.CurrentData.Size = total;
            vieModel.CurrentData.LastScanDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }




        private void AddToLabel(object sender, RoutedEventArgs e)
        {
            searchLabelPopup.IsOpen = true;
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



        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            border.Background = GlobalStyle.Common.HighLight.Background;

        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Grid grid = element.FindParentOfType<Grid>("rootGrid");
            Border border = grid.Children[0] as Border;
            border.Background = (SolidColorBrush)Application.Current.Resources["ListBoxItem.Background"];


        }


        private void newLabel_Cancel(object sender, RoutedEventArgs e)
        {
            searchLabelPopup.IsOpen = false;
        }

        private void newLabel_Confirm(object sender, RoutedEventArgs e)
        {

        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TextBlock textBlock = border.Child as TextBlock;
            string text = textBlock.Text;
            addLabel(text.Substring(0, text.IndexOf("(")));
            searchLabelPopup.IsOpen = false;
        }

        private void ChooseImage(object sender, MouseButtonEventArgs e)
        {
            string path = Path.GetDirectoryName(vieModel.CurrentData.Path);
            string imgPath = SelectData(path, GlobalVariable.SupportPictureFormat);
            if (File.Exists(imgPath))
            {
                vieModel.CurrentImage = ImageProcess.ReadImageFromFile(imgPath);
                //vieModel.CurrentGame.BigImagePath = imgPath;
            }
        }
    }



}
