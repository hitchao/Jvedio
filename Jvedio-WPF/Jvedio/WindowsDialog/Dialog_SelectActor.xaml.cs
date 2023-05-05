using Jvedio.Core.Crawler;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_SelectActor : SuperControls.Style.BaseDialog
    {
#pragma warning disable CS0612 // “ActorSearch”已过时
        public List<ActorSearch> ActorSearches;
#pragma warning restore CS0612 // “ActorSearch”已过时

        public List<int> SelectedActor;
        public int VideoType = 1;
        public int StartPage = 1;
        public int EndPage = 500;

#pragma warning disable CS0612 // “ActorSearch”已过时
        public Dialog_SelectActor(bool showbutton, List<ActorSearch> actorSearches) : base(showbutton)
#pragma warning restore CS0612 // “ActorSearch”已过时
        {
            InitializeComponent();
            ActorSearches = actorSearches;
            SelectedActor = new List<int>();
            ActorItemsControl.ItemsSource = this.ActorSearches;
        }

        private void ActorBorderMouseEnter(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            int.TryParse(image.Tag.ToString(), out int id);
            if (SelectedActor.Contains(id)) return;
            StackPanel stackPanel = image.Parent as StackPanel;
            Border border = ((Grid)stackPanel.Parent).Children[0] as Border;

            border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
            border.Background = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
        }

        private void ActorBorderMouseLeave(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            int.TryParse(image.Tag.ToString(), out int id);
            StackPanel stackPanel = image.Parent as StackPanel;
            Border border = ((Grid)stackPanel.Parent).Children[0] as Border;
            if (!SelectedActor.Contains(id))
            {
                border.BorderBrush = Brushes.Transparent;
                border.Background = (SolidColorBrush)Application.Current.Resources["Window.Side.Background"];
            }
        }

        private void SelectActor(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            int.TryParse(image.Tag.ToString(), out int id);
            if (!SelectedActor.Contains(id) && SelectedActor.Count < 3)
                SelectedActor.Add(id);
            else
                SelectedActor.Remove(id);
            ActorBorderMouseEnter(sender, null);
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
    }
}