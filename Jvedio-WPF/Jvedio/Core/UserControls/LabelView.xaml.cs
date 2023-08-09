using SuperControls.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Jvedio.Core.UserControls
{
    /// <summary>
    /// LabelView.xaml 的交互逻辑
    /// </summary>
    public partial class LabelView : UserControl, INotifyPropertyChanged
    {

        public const string SQL_JOIN = " join metadata_to_label on metadata_to_label.DataID=metadata.DataID ";



        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region "事件"

        private delegate void AsyncLoadItemDelegate<T>(ObservableCollection<T> list, T item);

        private void AsyncLoadItem<T>(ObservableCollection<T> list, T item) => list.Add(item);

        public Action<string, LabelType> onLabelClick;

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

        public async void Init(List<string> list)
        {
            LabelList = new ObservableCollection<string>();

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

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Text is string label && label.IndexOf("(") > 0)
                onLabelClick?.Invoke(label.Substring(0, label.LastIndexOf("(")), LabelType);
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
