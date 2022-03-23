using ChaoControls.Style;
using Jvedio.Entity;
using Jvedio.Style;
using Jvedio.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static Jvedio.FileProcess;
namespace Jvedio
{
    /// <summary>
    /// WindowEdit.xaml 的交互逻辑
    /// </summary>
    public partial class WindowEdit : ChaoControls.Style.BaseWindow
    {



        VieModel_Edit vieModel;

        public WindowEdit() : this(539)
        {

        }

        public WindowEdit(long dataID)
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            vieModel = new VieModel_Edit(dataID);
            this.DataContext = vieModel;
            ReLoad();

        }


        public void ReLoad()
        {
            actorTagPanel.TagList = vieModel.CurrentVideo.ActorNameList;
            actorTagPanel.Refresh();
            genreTagPanel.TagList = vieModel.CurrentVideo.GenreList;
            genreTagPanel.Refresh();
            labelTagPanel.TagList = vieModel.CurrentVideo.LabelList;
            labelTagPanel.Refresh();
            subSectionTagPanel.TagList = vieModel.CurrentVideo.SubSectionList;
            subSectionTagPanel.Refresh();
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
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "EditGrowl");
            }
            else
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_Fail, "EditGrowl");
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
                if (Scan.IsProperMovie(path)) vieModel.CurrentVideo.Path = path;
            }
            else
            {
                vieModel.CurrentVideo.Path = dragdropFiles[0];
                if (vieModel.CurrentVideo.SubSectionList == null)
                    vieModel.CurrentVideo.SubSectionList = new System.Collections.Generic.List<string>();
                foreach (var file in dragdropFiles)
                {
                    if (vieModel.CurrentVideo.SubSectionList.Contains(file)) continue;
                    if (IsFile(file) && Scan.IsProperMovie(file))
                    {
                        vieModel.CurrentVideo.SubSectionList.Add(file);
                    }
                }
                vieModel.CurrentVideo.SubSection = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentVideo.SubSectionList);
                ReLoad();
            }
            calcSize();
        }



        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime date = DateTime.Now; ;
            bool success = DateTime.TryParse((sender as SearchBox).Text, out date);
            if (success)
            {
                this.vieModel.CurrentVideo.ReleaseDate = date.ToString("yyyy-MM-dd ");
            }
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalConfig.Edit.MoreExpanded = vieModel.MoreExpanded;
            GlobalConfig.Edit.Save();
        }

        private void NewActor(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, "请输入");
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (!string.IsNullOrEmpty(text)) return;
                vieModel.CurrentVideo.ActorNameList.Add(text);



            }
        }

        private void NewGenre(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, "请输入");
            if (dialogInput.ShowDialog() == true)
            {
                string text = dialogInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                if (vieModel.CurrentVideo.GenreList == null)
                    vieModel.CurrentVideo.GenreList = new System.Collections.Generic.List<string>();
                vieModel.CurrentVideo.GenreList.Add(text);
                vieModel.CurrentVideo.Genre = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentVideo.GenreList);
                genreTagPanel.TagList = null;
                genreTagPanel.TagList = vieModel.CurrentVideo.GenreList;
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
                if (vieModel.CurrentVideo.LabelList == null)
                    vieModel.CurrentVideo.LabelList = new System.Collections.Generic.List<string>();
                vieModel.CurrentVideo.LabelList.Add(text);
                vieModel.CurrentVideo.Label = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentVideo.LabelList);
                labelTagPanel.TagList = null;
                labelTagPanel.TagList = vieModel.CurrentVideo.LabelList;
                labelTagPanel.Refresh();
            }
        }

        private void GenreChanged(object sender, ChaoControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
                vieModel.CurrentVideo.Genre = String.Join(GlobalVariable.Separator.ToString(), e.List);


        }

        private void LabelChanged(object sender, ChaoControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
                vieModel.CurrentVideo.Label = String.Join(GlobalVariable.Separator.ToString(), e.List);

        }

        private void SubSectionChanged(object sender, ChaoControls.Style.ListChangedEventArgs e)
        {
            if (e != null && e.List != null)
            {
                vieModel.CurrentVideo.SubSection = String.Join(GlobalVariable.Separator.ToString(), e.List);
                calcSize();
            }

        }

        private void NewSubSection(object sender, RoutedEventArgs e)
        {
            string text = SelectVideo("");
            if (vieModel.CurrentVideo.SubSectionList == null)
                vieModel.CurrentVideo.SubSectionList = new System.Collections.Generic.List<string>();
            vieModel.CurrentVideo.SubSectionList.Add(text);
            vieModel.CurrentVideo.SubSection = String.Join(GlobalVariable.Separator.ToString(), vieModel.CurrentVideo.SubSectionList);
            subSectionTagPanel.TagList = null;
            subSectionTagPanel.TagList = vieModel.CurrentVideo.SubSectionList;
            calcSize();
            subSectionTagPanel.Refresh();

        }

        private void ChooseFile(object sender, MouseButtonEventArgs e)
        {
            vieModel.CurrentVideo.Path = SelectVideo(vieModel.CurrentVideo.Path);
            calcSize();
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


        public string SelectVideo(string path)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseFile;
            OpenFileDialog1.FileName = path;
            OpenFileDialog1.Filter = GlobalVariable.SupportVideoFormat;
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = OpenFileDialog1.FileName;
                if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                    return filename;
            }
            return path;
        }


    }



}
