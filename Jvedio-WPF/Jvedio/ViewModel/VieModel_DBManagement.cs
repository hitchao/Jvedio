using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System.IO;
using Jvedio.Plot.Bar;

namespace Jvedio.ViewModel
{
    public class VieModel_DBManagement : ViewModelBase
    {
        protected List<Movie> Movies;

        public VieModel_DBManagement()
        {
            ListDatabaseCommand = new RelayCommand(ListDatabase);
            StatisticCommand = new RelayCommand(Statistic);


        }

        #region "RelayCommand"
        public RelayCommand ListDatabaseCommand { get; set; }
        public RelayCommand StatisticCommand { get; set; }

        #endregion




        private int _ProgressBarValue = 0;

        public int ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set
            {
                _ProgressBarValue = value;
                RaisePropertyChanged();
            }
        }



        public void ListDatabase()
        {
            DataBases = new ObservableCollection<string>();
            try
            {
                var files = Directory.GetFiles("DataBase", "*.sqlite", SearchOption.TopDirectoryOnly).ToList();

                foreach (var item in files)
                {
                    string name = Path.GetFileNameWithoutExtension(item);
                    if (!string.IsNullOrEmpty(name))
                        DataBases.Add(name);
                }
            }
            catch { }

        }






        public  void Statistic()
        {
            Movies = new List<Movie>();
            string name = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower();
            name = "DataBase\\" + name;
            MySqlite db = new MySqlite(name);
            Movies =  db.SelectMoviesBySql("SELECT * FROM movie");
            db.CloseDB();
        }

        public List<BarData> LoadActor()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                arg.actor.Split(new char[] { ' ', '/' }).ToList().ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!dic.ContainsKey(item))
                            dic.Add(item, 1);
                        else
                            dic[item] += 1;
                    }

                });
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();
        }

        public List<BarData> LoadTag()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                arg.tag.Split(' ').ToList().ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!dic.ContainsKey(item))
                            dic.Add(item, 1);
                        else
                            dic[item] += 1;
                    }
                });
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();
        }

        public List<BarData> LoadGenre()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                arg.genre.Split(' ').ToList().ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!dic.ContainsKey(item))
                            dic.Add(item, 1);
                        else
                            dic[item] += 1;
                    }
                });
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();

        }

        public List<BarData> LoadID()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                string id = "";
                if (arg.vediotype == 3)
                    id = Identify.GetEuFanhao(arg.id).Split('.')[0];
                else
                    id = Identify.GetFanhao(arg.id).Split('-')[0];
                if (!dic.ContainsKey(id))
                    dic.Add(id, 1);
                else
                    dic[id] += 1;
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();
        }







        private ObservableCollection<string> _DataBases;



        public ObservableCollection<string> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }


        }

        private string _CurrentDataBase;

        public string CurrentDataBase
        {
            get { return _CurrentDataBase; }
            set
            {
                _CurrentDataBase = value;
                RaisePropertyChanged();
            }


        }





    }
}
