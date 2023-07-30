using Jvedio.Core.Crawler;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_LoadPage : SuperControls.Style.BaseDialog
    {
        public List<ActorSearch> ActorSearches { get; set; }
        public string url { get; set; } = string.Empty;
        public int VideoType { get; set; } = 1;
        public int StartPage { get; set; } = 1;
        public int EndPage { get; set; } = 500;

        private string WebSiteList { get; set; }

        public Dialog_LoadPage(bool showbutton) : base(showbutton)
        {
            InitializeComponent();
            cb.ItemsSource = WebSiteList.Split(';');
            if (cb.Items.Count > 0)
                cb.SelectedIndex = 0;

            tb.Focus();
            tb.SelectAll();
        }

        private void SaveVedioType(object sender, RoutedEventArgs e)
        {
            var rbs = VedioTypeStackPanel.Children.OfType<RadioButton>().ToList();
            RadioButton rb = sender as RadioButton;
            VideoType = rbs.IndexOf(rb) + 1;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StartPage = (int)e.NewValue;
        }

        private void SliderEnd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EndPage = (int)e.NewValue;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            url = ((TextBox)sender).Text;
        }

        private void SaveWebSite(object sender, RoutedEventArgs e)
        {
            List<string> l1 = StringToList(WebSiteList);
            WebSiteList = string.Join(";", l1.Union(StringToList(tb.Text)));
            cb.ItemsSource = WebSiteList.Split(';');
        }

        private void DeleteWebSite(object sender, RoutedEventArgs e)
        {
            List<string> l1 = StringToList(WebSiteList);
            WebSiteList = string.Join(";", l1.Except(StringToList(url)));
            cb.ItemsSource = WebSiteList.Split(';');
        }

        private List<string> StringToList(string str)
        {
            if (str.Length == 0)
                return new List<string>();
            if (str.IndexOf(";") < 0 && str.Length > 0)
                return new List<string>() { str };

            List<string> result = new List<string>();
            foreach (var item in str.Split(';')) {
                if (item.Length > 0) {
                    result.Add(item.Replace(" ", string.Empty));
                }
            }

            return result;
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            tb.Text = e.AddedItems[0].ToString();
        }
    }
}