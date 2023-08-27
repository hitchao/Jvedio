using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Jvedio.Core.FFmpeg;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Framework.Tasks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Jvedio.Core.UserControls.Tasks
{
    /// <summary>
    /// TaskList.xaml 的交互逻辑
    /// </summary>
    public partial class TaskList : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "事件"

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);


        public Action onRemoveAll;
        public Action onRemoveCancel;
        public Action onRemoveComplete;

        private Action _onCancelAll;

        public Action onCancelAll {
            get { return _onCancelAll; }

            set {
                _onCancelAll = value;
                RaisePropertyChanged();
            }
        }



        private Action<string> _onCancel;

        public Action<string> onCancel {
            get { return _onCancel; }

            set {
                _onCancel = value;
                RaisePropertyChanged();
            }
        }


        private Action<TaskList, string> _onShowDetail;

        public Action<TaskList, string> onShowDetail {
            get { return _onShowDetail; }

            set {
                _onShowDetail = value;
                RaisePropertyChanged();
            }
        }
        private Action<string> _onRestart;

        public Action<string> onRestart {
            get { return _onRestart; }

            set {
                _onRestart = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "属性"

        private TaskType _TaskType;

        public TaskType TaskType {
            get { return _TaskType; }

            set {
                _TaskType = value;
                RaisePropertyChanged();
            }
        }
        private bool _ShowLog = false;

        public bool ShowLog {
            get { return _ShowLog; }

            set {
                _ShowLog = value;
                RaisePropertyChanged();
            }
        }
        private bool _ShowProgress;

        public bool ShowProgress {
            get { return _ShowProgress; }

            set {
                _ShowProgress = value;
                RaisePropertyChanged();
            }
        }

        private double _AllTaskProgress = 0;

        public double AllTaskProgress {
            get { return _AllTaskProgress; }

            set {
                _AllTaskProgress = value;
                RaisePropertyChanged();
                if (value == 0)
                    ShowProgress = false;
                else
                    ShowProgress = true;
            }
        }

        private ObservableCollection<AbstractTask> _TaskStatusList;

        public ObservableCollection<AbstractTask> TaskStatusList {
            get { return _TaskStatusList; }

            set {
                _TaskStatusList = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        public TaskList(TaskType type)
        {
            InitializeComponent();
            TaskStatusList = new ObservableCollection<AbstractTask>();
            TaskType = type;
        }

#if DEBUG
        public async void InitSample()
        {
            for (int i = 1; i <= 100; i++) {
                SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                wrapper.Eq("DataID", i);
                Video video = MapperManager.videoMapper.SelectOne(wrapper);
                if (video == null)
                    continue;
                ScreenShotTask task = new ScreenShotTask(video, false);
                if (i % 3 == 0) {
                    task.Status = TaskStatus.Running;
                } else if (i % 4 == 0) {
                    task.Status = TaskStatus.Canceled;
                } else if (i % 5 == 0) {
                    task.Status = TaskStatus.RanToCompletion;
                } else if (i % 6 == 0) {
                    task.Status = TaskStatus.WaitingToRun;
                }

                task.Progress = new Random(i).Next(0, 100);
                task.Message = "发生了异常发生了异常发生了异常发生了异常发生了异常发生了异常发生了异常发生了异常发生了异常发生了异常";
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new AsyncLoadItemDelegate<AbstractTask>(AsyncLoadItem), TaskStatusList, task);
            }
        }
#endif

        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void RemoveComplete(object sender, RoutedEventArgs e)
        {
            onRemoveComplete?.Invoke();
        }
        private void RemoveAll(object sender, RoutedEventArgs e)
        {
            onRemoveAll?.Invoke();
        }

        private void RemoveCancel(object sender, RoutedEventArgs e)
        {
            onRemoveCancel?.Invoke();
        }

        private void CancelAll(object sender, RoutedEventArgs e)
        {
            onCancelAll?.Invoke();
        }

        private void CancelOne(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ele && ele.Uid is string id)
                onCancel?.Invoke(id);
        }

        private void ShowDetail(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ele && ele.Uid is string id)
                onShowDetail?.Invoke(this, id);
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ele && ele.Uid is string id)
                onRestart?.Invoke(id);
        }

        private void textBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.GotFocus(sender);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.LostFocus(sender);
        }


        public void SetLogs(string logs)
        {
            logTextBox.Clear();
            logTextBox.Text = logs;
        }


        private void HideLog(object sender, RoutedEventArgs e)
        {
            ShowLog = false;
        }

        private void taskList_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            IHighlightingDefinition highlightingDefinition = HighlightingManager.Instance.GetDefinition("截图日志");
            logTextBox.SyntaxHighlighting = highlightingDefinition;

            TextEditorOptions textEditorOptions = new TextEditorOptions();
            textEditorOptions.HighlightCurrentLine = true;
            logTextBox.Options = textEditorOptions;
        }
    }
}
