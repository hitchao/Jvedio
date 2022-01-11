using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using Jvedio.Utils;
using Jvedio.Core.pojo;
using Jvedio.Core;

namespace Jvedio.ViewModel
{
    public class VieModel_Settings : ViewModelBase
    {

        public VieModel_Settings()
        {
            DataBase = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);
            DataBases = ((Main)FileProcess.GetWindowByName("Main")).vieModel.DataBases;
            ThemeList = new ObservableCollection<Theme>();
            foreach (Theme theme in ThemeLoader.Themes)
            {
                if (theme.Name == Skin.白色.ToString() || theme.Name == Skin.黑色.ToString() || theme.Name == Skin.蓝色.ToString()) continue;
                ThemeList.Add(theme);
            }



        }


        public void Reset()
        {
            //读取配置文件
            ScanPath = new ObservableCollection<string>();
            foreach (var item in ReadScanPathFromConfig(DataBase))
            {
                ScanPath.Add(item);
            }
            if (ScanPath.Count == 0) ScanPath = null;
            GlobalVariable.InitVariable();
            Servers = new ObservableCollection<Server>();

            Type type = JvedioServers.GetType();
            foreach (var item in type.GetProperties())
            {
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(item.Name);
                Server server = (Server)propertyInfo.GetValue(JvedioServers);
                if (server.Url != "")
                    Servers.Add(server);
            }
        }


        private string _ViewRenameFormat;

        public string ViewRenameFormat
        {
            get { return _ViewRenameFormat; }
            set
            {
                _ViewRenameFormat = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Theme> _ThemeList;

        public ObservableCollection<Theme> ThemeList
        {
            get { return _ThemeList; }
            set
            {
                _ThemeList = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Server> _Servers;

        public ObservableCollection<Server> Servers
        {
            get { return _Servers; }
            set
            {
                _Servers = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _ScanPath;

        public ObservableCollection<string> ScanPath
        {
            get { return _ScanPath; }
            set
            {
                _ScanPath = value;
                RaisePropertyChanged();
            }
        }


        private Skin _Themes = (Skin)Enum.Parse(typeof(Skin), "黑色", true);

        public Skin Themes
        {
            get { return _Themes; }
            set
            {
                _Themes = value;
                RaisePropertyChanged();
            }
        }


        private MyLanguage _Language = (MyLanguage)Enum.Parse(typeof(MyLanguage), Properties.Settings.Default.Language, true);

        public MyLanguage Language
        {
            get { return _Language; }
            set
            {
                _Language = value;
                RaisePropertyChanged();
            }
        }

        private string _DataBase;

        public string DataBase
        {
            get { return _DataBase; }
            set
            {
                _DataBase = value;
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






    }
}
