using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Jvedio.GlobalVariable;

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
            ImageViewer.ImageSource= BitmapFrame.Create((BitmapSource)source);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                var anim = new DoubleAnimation(0, (Duration)FadeInterval);
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
            ImageViewer.ImageSource = null;
            base.OnClosed(e);
        }

        //private void Grid_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed )
        //    {
        //        this.DragMove();
        //    }
        //}

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseWindow(null, null);
        }
    }
}
