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
    public partial class WindowEdit : BaseWindow
    {



        VieModel_Edit vieModel;

        private string ID;


        public WindowEdit(string id = "")
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            ID = id;
            vieModel = new VieModel_Edit();

            if (ID == "")
                vieModel.Reset();
            else
                vieModel.Query(ID);
            this.DataContext = vieModel;

            this.Height = SystemParameters.PrimaryScreenHeight * 0.6;
            this.Width = SystemParameters.PrimaryScreenHeight * 0.6 * 800 / 450;
        }

        public void ChoseMovie(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseFile;
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = $"{Jvedio.Language.Resources.NormalVedio}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{Jvedio.Language.Resources.OtherVedio}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{Jvedio.Language.Resources.AllFile} (*.*)|*.*";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (vieModel.DetailMovie == null) vieModel.DetailMovie = new DetailMovie();
                SaveInfo(OpenFileDialog1.FileName);
            }

        }







        private void UpdateDetail()
        {
            if (GetWindowByName("WindowDetails") != null)
            {
                WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                windowDetails.vieModel.Query(vieModel.DetailMovie.id);
            }
        }


        public Movie SelectMovie(string ID)
        {
            string table = ((Main)GetWindowByName("Main")).GetCurrentList();
            if (string.IsNullOrEmpty(table))
            {
                return DataBase.SelectMovieByID(ID); ;
            }
            else
            {
                using (MySqlite mySqlite = new MySqlite("mylist"))
                {
                    return mySqlite.SelectMovieBySql($"select * from {table} where id='{ID}'");
                }
            }
        }

        private void UpdateMain(string oldID, string newID)
        {
            Main main = App.Current.Windows[0] as Main;
            Movie movie = SelectMovie(newID);
            addTag(ref movie);
            movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");
            movie.bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");

            for (int i = 0; i < main.vieModel.CurrentMovieList.Count; i++)
            {
                try
                {
                    if (main.vieModel.CurrentMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        main.vieModel.CurrentMovieList[i] = null;
                        main.vieModel.CurrentMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }


            for (int i = 0; i < main.vieModel.MovieList.Count; i++)
            {
                try
                {
                    if (main.vieModel.MovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        main.vieModel.MovieList[i] = null;
                        main.vieModel.MovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }

            for (int i = 0; i < main.vieModel.FilterMovieList.Count; i++)
            {
                try
                {
                    if (main.vieModel.FilterMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        main.vieModel.FilterMovieList[i] = null;
                        main.vieModel.FilterMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (vieModel.DetailMovie.id == "") { new Msgbox(this, Jvedio.Language.Resources.NullID).ShowDialog(); return; }
            if (vieModel.DetailMovie.vediotype <= 0) { new Msgbox(this, Jvedio.Language.Resources.Message_ChooseVedioType).ShowDialog(); return; }

            string oldID = vieModel.DetailMovie.id;
            string newID = idTextBox.Text;
            bool success = vieModel.SaveModel(idTextBox.Text);
            if (success)
            {
                UpdateMain(oldID, newID);//更新主窗口
                UpdateDetail();//更新详情窗口
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "EditGrowl");
            }
            else
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SaveFailForExistID, "EditGrowl");
            }

        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }




        private void ChoseMovieBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void ChoseMovieBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (IsFile(dragdropFile))
                {
                    if (Scan.IsProperMovie(dragdropFile))
                    {
                        SaveInfo(dragdropFile);
                        break;
                    }
                }
            }
        }

        private void SaveInfo(string filepath)
        {
            if (vieModel.DetailMovie == null) return;
            if (!string.IsNullOrEmpty(vieModel.DetailMovie.id))
            {
                //视频类型、文件大小、创建时间
                vieModel.DetailMovie.filepath = filepath;

                FileInfo fileInfo = new FileInfo(filepath);

                string id = Identify.GetFanhao(fileInfo.Name);
                int vt = (int)Identify.GetVideoType(id);
                if (vt > 0) vieModel.DetailMovie.vediotype = vt;
                if (File.Exists(filepath))
                {
                    string createDate = "";
                    try { createDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                    catch { }
                    if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    vieModel.DetailMovie.otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    vieModel.DetailMovie.scandate = createDate;
                }

                vieModel.SaveModel();

                string table = ((Main)GetWindowByName("Main")).GetCurrentList();
                if (string.IsNullOrEmpty(table))
                    vieModel.Query(vieModel.id);
                else
                    vieModel.Query(vieModel.id, table);
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_EditUpdateSuccess, "EditGrowl");
            }
            else
            {
                vieModel.Refresh(filepath);
            }
        }

        private void Jvedio_BaseWindow_ContentRendered(object sender, EventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime date = DateTime.Now; ;
            bool success = DateTime.TryParse((sender as TextBox).Text, out date);
            if (success)
            {
                this.vieModel.DetailMovie.releasedate = date.ToString("yyyy-MM-dd ");
            }
        }
    }



}
