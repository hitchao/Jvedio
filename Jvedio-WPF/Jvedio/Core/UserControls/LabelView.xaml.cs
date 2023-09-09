using SuperControls.Style;
using SuperControls.Style.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// LabelView.xaml 的交互逻辑
    /// </summary>
    public partial class LabelView : UserControl, INotifyPropertyChanged, ITabItemControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "事件"

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        public Action<string, LabelType> onLabelClick;
        public Action<LabelType> onRefresh;

        private Action<string, LabelType> _onAddLabel;

        public Action<string, LabelType> onAddLabel {
            get { return _onAddLabel; }

            set {
                _onAddLabel = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region "属性"


        private List<string> Cache { get; set; } = new List<string>();

        public ObservableCollection<string> _LabelList;
        public ObservableCollection<string> LabelList {
            get { return _LabelList; }

            set {
                _LabelList = value;
                RaisePropertyChanged();
            }
        }
        public LabelType _LabelType;
        public LabelType LabelType {
            get { return _LabelType; }

            set {
                _LabelType = value;
                RaisePropertyChanged();
            }
        }
        public bool _Nothing;
        public bool Nothing {
            get { return _Nothing; }

            set {
                _Nothing = value;
                RaisePropertyChanged();
            }
        }




        #endregion
        public LabelView(LabelType type)
        {
            InitializeComponent();
            LabelType = type;
        }

        private void labelList_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            Init(Cache);

        }


        public void Refresh(int page = -1)
        {
            onRefresh?.Invoke(LabelType);
            Init(Cache);
        }

        public async void Init(List<string> list)
        {
            LabelList = new ObservableCollection<string>();
            Nothing = true;
            LabelList.CollectionChanged += (s, ev) => {
                if (LabelList.Count == 0)
                    Nothing = true;
                else
                    Nothing = false;
            };

            if (list != null) {
                for (int i = 0; i < list.Count; i++) {
                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new AsyncLoadItemDelegate<string>(AsyncLoadItem), LabelList, list[i]);
                }
            }
        }

        public void SetLabel(List<string> list)
        {
            Cache = new List<string>();
            if (list != null)
                Cache.AddRange(list);
        }



        private void doSearch(object sender, TextChangedEventArgs e)
        {
            if (sender is SearchBox searchBox && searchBox.IsLoaded && Cache != null && Cache.Count > 0) {
                string search = searchBox.Text;
                if (string.IsNullOrEmpty(search)) {
                    Init(Cache);
                } else {
                    List<string> list = Cache.Where(arg => arg.IndexOf(search) >= 0).ToList();
                    Init(list);
                }


            }
        }
        public void SetSearchFocus()
        {
            searchBox.SetFocus();
        }

        public void NextPage()
        {

        }

        public void PreviousPage()
        {

        }

        public void GoToTop()
        {

        }

        public void GoToBottom()
        {

        }

        public void FirstPage()
        {

        }

        public void LastPage()
        {

        }

        private void OnAddLabel(object sender, RoutedEventArgs e)
        {
            DialogInput input = new DialogInput(SuperControls.Style.LangManager.GetValueByKey("PleaseEnter"));
            if (input.ShowDialog(App.Current.MainWindow) == false)
                return;
            string value = input.Text;
            onAddLabel?.Invoke(value, LabelType);
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Text is string label && label.IndexOf("(") > 0)
                onLabelClick?.Invoke(label.Substring(0, label.LastIndexOf("(")), LabelType);
        }
    }

    public enum LabelType
    {
        None,
        LabelName,
        Genre,
        Studio,
        Series,
        Director
    }
}
