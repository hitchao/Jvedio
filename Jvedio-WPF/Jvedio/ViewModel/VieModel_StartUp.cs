using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;


namespace Jvedio.ViewModel
{
    public class VieModel_StartUp : ViewModelBase
    {
        public RelayCommand ListDatabaseCommand { get; set; }



        private bool _initCompleted;
        public bool InitCompleted
        {
            get { return _initCompleted; }
            set
            {
                _initCompleted = value;
                RaisePropertyChanged();
            }
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


        public VieModel_StartUp()
        {
            ListDatabaseCommand = new RelayCommand(ListDatabase);
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
                    if (!string.IsNullOrEmpty(name) && !DataBases.Contains(name))
                        DataBases.Add(name);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            if (!DataBases.Contains(Jvedio.Language.Resources.NewLibrary))
                DataBases.Add(Jvedio.Language.Resources.NewLibrary);
        }







    }
}
