using DynamicData.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jvedio.Windows
{
    /// <summary>
    /// Window_Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Progress : SuperControls.Style.BaseWindow, INotifyPropertyChanged
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
