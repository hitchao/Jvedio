using Jvedio.Plot.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

namespace Jvedio.Plot.Bar
{
    /// <summary>
    /// BarView.xaml 的交互逻辑
    /// </summary>
    /// 


    public partial class BarView : UserControl
    {

        public IEnumerable<BarData> Datas;
        public string Title;
        public BarViewModel model;

        public double MaxBarWidth = 100;
        public double MinBarWidth = 40;
        //自定义控件的属性
        private Brush DefaultBackground = Brushes.LightGray;

        public BarView()
        {
            InitializeComponent();
            ItemsControl1.MouseWheel += AdjustBarWidth;
            this.Loaded += Load;
            ApplyButton.Click += Apply;
            SortButton.Click += Sort;
            ResetButton.Click += Reset;
        }


        private void Apply(object sender, EventArgs e)
        {
            if (model == null || Slider.Value <= 0) return;
            ItemsControl1.ItemsSource = null;
            ItemsControl2.ItemsSource = null;
            model.ShowCurrent(Slider.Value);
            ItemsControl1.ItemsSource = model.CurrentDatas;
            ItemsControl2.ItemsSource = model.CurrentDatas;
        }

        private void Load(object sender, EventArgs e)
        {
            model = new BarViewModel(Title,ScrollViewer.ActualHeight, ScrollViewer.ActualWidth);
            if (this.Datas != null) model.Init(this.Datas);
            this.DataContext = model;
            ItemsControl1.Background = DefaultBackground;
            ItemsControl1.ItemsSource = model.CurrentDatas;
            ItemsControl2.ItemsSource = model.CurrentDatas;
        }

        public void Refresh()
        {
            App.Current.Dispatcher.Invoke((Action)delegate { 
                model = new BarViewModel(Title, ScrollViewer.ActualHeight, ScrollViewer.ActualWidth);
                if (this.Datas != null) model.Init(this.Datas);
                this.DataContext = model;
                ItemsControl1.Background = DefaultBackground;
                ItemsControl1.ItemsSource = model.CurrentDatas;
                ItemsControl2.ItemsSource = model.CurrentDatas;
            });
        }


        private void AdjustBarWidth(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (model.BarWidth + 5 <= MaxBarWidth)
                    model.BarWidth += 5;
            }
            else
            {
                if (model.BarWidth - 5 >= MinBarWidth)
                    model.BarWidth -= 5;
            }

            ItemsControl1.ItemsSource = null;
            ItemsControl2.ItemsSource = null;
            ItemsControl1.ItemsSource = model.CurrentDatas;
            ItemsControl2.ItemsSource = model.CurrentDatas;
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            model = new BarViewModel(Title,ScrollViewer.ActualHeight, ScrollViewer.ActualWidth, model.Descending);
            model.Init(Datas,model.Current);
            this.DataContext = model;
            ItemsControl1.ItemsSource = null;
            ItemsControl2.ItemsSource = null;
            ItemsControl1.ItemsSource = model.CurrentDatas;
            ItemsControl2.ItemsSource = model.CurrentDatas;
        }

        private void Sort(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string text = button.Content.ToString();
            if (text == Jvedio.Language.Resources.SmallToBig)
                button.Content = Jvedio.Language.Resources.BigToSmall;
            else
                button.Content = Jvedio.Language.Resources.SmallToBig;
            model = new BarViewModel(Title,ScrollViewer.ActualHeight, ScrollViewer.ActualWidth, !model.Descending);
            model.Init(Datas);
            this.DataContext = model;
            ItemsControl1.ItemsSource = null;
            ItemsControl2.ItemsSource = null;
            ItemsControl1.ItemsSource = model.CurrentDatas;
            ItemsControl2.ItemsSource = model.CurrentDatas;
        }


    }

    public class BarData
    {
        public string Key { get; set; }
        public double Value { get; set; }

        public double ActualValue { get; set; }
        public double BarWidth { get; set; }
    }

    public class ToolTipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0].ToString() + Environment.NewLine + values[1].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class WidthVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double.TryParse(values[0].ToString(), out double barwidth);
            double.TryParse(values[1].ToString(), out double width);
            if (width <= 0 || width > barwidth - 15)
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
