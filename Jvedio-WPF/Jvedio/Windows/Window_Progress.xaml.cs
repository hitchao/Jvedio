using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jvedio.Windows
{
    /// <summary>
    /// Window_Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Progress : ChaoControls.Style.BaseWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private string _MainText = "";
        public string MainText
        {
            get => _MainText;
            set
            {
                _MainText = value;
                OnPropertyChanged();
            }
        }

        private string _SubText = "";
        public string SubText
        {
            get => _SubText;
            set
            {
                _SubText = value;
                OnPropertyChanged();
            }
        }

        private float _MainProgress = 50;
        public float MainProgress
        {
            get => _MainProgress;
            set
            {
                _MainProgress = value;
                OnPropertyChanged();
            }
        }
        private float _SubProgress = 50;
        public float SubProgress
        {
            get => _SubProgress;
            set
            {
                _MainProgress = value;
                OnPropertyChanged();
            }
        }

        private string _LogText = "";
        public string LogText
        {
            get => _LogText;
            set
            {
                _LogText = value;
                OnPropertyChanged();
            }
        }

        private bool _HideSub = false;
        public bool HideSub
        {
            get => _HideSub;
            set
            {
                _HideSub = value;
                OnPropertyChanged();
            }
        }


        public Window_Progress()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Window_Progress(string title = "", string mainText = "总进度", string subText = "次进度", float mainProgress = 0, float subProgress = 0, string logText = "日志") : this()
        {
            if (!string.IsNullOrEmpty(title)) Title = title;
            MainText = mainText;
            SubText = subText;
            MainProgress = mainProgress;
            SubProgress = subProgress;
            LogText = logText;
        }

    }
}
