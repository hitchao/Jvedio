using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.FileProcess;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;


namespace Jvedio.ViewModel
{
    class VieModel_Edit : ViewModelBase
    {

        public string id;//用于判断是否更改了 id

        public DetailMovie detailmovie;

        public DetailMovie DetailMovie
        {
            get { return detailmovie; }
            set
            {
                detailmovie = value;
                RaisePropertyChanged();
                id = DetailMovie.id;
            }
        }



        public ObservableCollection<string> movieIDList;

        public ObservableCollection<string> MovieIDList
        {
            get { return movieIDList; }
            set
            {
                movieIDList = value;
                RaisePropertyChanged();
            }
        }


        public void Refresh(string filepath)
        {
            DetailMovie models = new DetailMovie { filepath = filepath };
            FileInfo fileInfo = new FileInfo(filepath);

            //获取创建日期
            string createDate = "";
            try { createDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
            catch { }
            if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            models.id = Identify.GetFanhao(fileInfo.Name);
            models.vediotype =(int) Identify.GetVideoType(models.id);
            models.otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            models.scandate= createDate ;
            models.filesize = fileInfo.Length;
            if (models != null) { DetailMovie = models; }
        }




        public RelayCommand<string> QueryCommand { get; set; }

        public void Query(string movieid,string tablename="")
        {
            DetailMovie models = null;
            if (string.IsNullOrEmpty(tablename))
            {
                models = DataBase.SelectDetailMovieById(movieid);
            }
            else
            {
                using(MySqlite mySqlite=new MySqlite("mylist"))
                {
                    models = mySqlite.SelectDetailMovieBySql($"select * from {tablename} where id='{movieid}'");
                }
            }
            
            
            
            DetailMovie = new DetailMovie();
            if (models != null) { DetailMovie = models; }
        }


        public  void Reset()
        {
            Main main = App.Current.Windows[0] as Main;
            var models = main.vieModel.CurrentMovieList.Select(arg => arg.id).ToList();
            MovieIDList = new ObservableCollection<string>();
            models?.ForEach(arg => { MovieIDList.Add(arg.ToUpper()); });
        }


        public bool SaveModel(string ID="")
        {
            string table = ((Main)GetWindowByName("Main")).GetCurrentList();


            if (ID == "" && DetailMovie != null && DetailMovie.id.ToUpper() == id.ToUpper())
            {
                InsertMovie(table);
                return true;
            }

            
            id = ID;
            if (DetailMovie != null)
            {
                
                if (DetailMovie.id.ToUpper() != id.ToUpper())
                {
                    //修改了原来的识别码
                    if (string.IsNullOrEmpty(table))
                    {
                        if (DataBase.SelectMovieByID(ID) != null) return false;
                        DataBase.DeleteByField("movie", "id", DetailMovie.id);
                        DetailMovie.id = id;
                        DataBase.InsertFullMovie(DetailMovie);
                    }
                    else
                    {
                        //修改了清单中的识别码
                        using(MySqlite mySqlite=new MySqlite("mylist"))
                        {
                            if (mySqlite.SelectMovieBySql($"select * from {table} where id='{ID}'") != null) return false;

                            mySqlite.DeleteByField(table, "id", DetailMovie.id);
                            DetailMovie.id = id;
                            mySqlite.InsertFullMovie(DetailMovie, table);
                        }

                    }

                }
                else {
                    InsertMovie(table);
                }

                return true;
            }
            return false;
           
        }

        private void InsertMovie(string table)
        {
            if (string.IsNullOrEmpty(table))
                DataBase.InsertFullMovie(DetailMovie);
            else
            {
                using (MySqlite mySqlite = new MySqlite("mylist"))
                {
                    mySqlite.InsertFullMovie(DetailMovie, table);
                }
            }
        }










    }
}
