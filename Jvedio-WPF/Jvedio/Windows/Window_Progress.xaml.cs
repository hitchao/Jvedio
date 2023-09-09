
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Jvedio.Windows
{
    /// <summary>
    /// Window_Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Progress : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "属性"
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
        #endregion

        public Window_Progress()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Window_Progress(string title = "",
            string mainText = "总进度",
            string subText = "次进度",
            float mainProgress = 0,
            float subProgress = 0,
            string logText = "日志") : this()
        {
            if (!string.IsNullOrEmpty(title))
                Title = title;
            MainText = mainText;
            SubText = subText;
            MainProgress = mainProgress;
            SubProgress = subProgress;
            LogText = logText;
        }

        private void Progress_Window_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }
    }
}
