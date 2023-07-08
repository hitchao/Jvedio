
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

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string _MainText = string.Empty;

        public string MainText {
            get => _MainText;
            set {
                _MainText = value;
                RaisePropertyChanged();
            }
        }

        private string _SubText = string.Empty;

        public string SubText {
            get => _SubText;
            set {
                _SubText = value;
                RaisePropertyChanged();
            }
        }

        private float _MainProgress = 50;

        public float MainProgress {
            get => _MainProgress;
            set {
                _MainProgress = value;
                RaisePropertyChanged();
            }
        }

        private float _SubProgress = 50;

        public float SubProgress {
            get => _SubProgress;
            set {
                _MainProgress = value;
                RaisePropertyChanged();
            }
        }

        private string _LogText = string.Empty;

        public string LogText {
            get => _LogText;
            set {
                _LogText = value;
                RaisePropertyChanged();
            }
        }

        private bool _HideSub = false;

        public bool HideSub {
            get => _HideSub;
            set {
                _HideSub = value;
                RaisePropertyChanged();
            }
        }

        public Window_Progress()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Window_Progress(string title = "", string mainText = "总进度", string subText = "次进度", float mainProgress = 0, float subProgress = 0, string logText = "日志") : this()
        {
            if (!string.IsNullOrEmpty(title))
                Title = title;
            MainText = mainText;
            SubText = subText;
            MainProgress = mainProgress;
            SubProgress = subProgress;
            LogText = logText;
        }
    }
}
