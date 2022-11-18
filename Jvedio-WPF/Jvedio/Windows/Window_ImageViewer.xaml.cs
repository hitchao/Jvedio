using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Jvedio
{
    /// <summary>
    /// Window_ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ImageViewer : Window
    {
        public Window_ImageViewer(Window owner, ImageSource source)
        {
            InitializeComponent();
            this.Owner = owner;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.8;
            this.Width = SystemParameters.PrimaryScreenHeight * 0.8 * 1230 / 720;
            ImageViewer.Source = BitmapFrame.Create((BitmapSource)source);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                var anim = new DoubleAnimation(0, (Duration)Main.FadeInterval);
                anim.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ImageViewer.Source = null;
            base.OnClosed(e);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseWindow(null, null);
        }
    }
}
