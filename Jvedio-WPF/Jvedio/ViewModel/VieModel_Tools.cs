using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.ViewModel
{


    class VieModel_Tools : ViewModelBase
    {
        private ObservableCollection<string> scanPath=new ObservableCollection<string>();

        public ObservableCollection<string> ScanPath
        {
            get { return scanPath; }
            set
            {
                scanPath = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> NFOscanPath = new ObservableCollection<string>();

        public ObservableCollection<string> NFOScanPath
        {
            get { return NFOscanPath; }
            set
            {
                NFOscanPath = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> scanEuPath = new ObservableCollection<string>();

        public ObservableCollection<string> ScanEuPath
        {
            get { return scanEuPath; }
            set
            {
                scanEuPath = value;
                RaisePropertyChanged();
            }
        }




        public VieModel_Tools()
        {

        }




    }
}
