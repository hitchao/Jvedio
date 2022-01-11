using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Jvedio
{

    public partial class Msgbox : Window
    {
        public string Text;
        public event EventHandler CancelTask;

        public Msgbox(Window window, string text)
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            Text = text;

            TextBlock.Text = text;
            if (window == null) return;
            this.Owner = window;

            if (window.Height == System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height || window.Width == System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width)
            {
                this.Left = window.Left;
                this.Top = window.Top;
                this.Height = window.Height;
                this.Width = window.Width;
            }
            else if (window.WindowState == WindowState.Maximized)
            {
                this.Left = 0;
                this.Top = 0;
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.Width = SystemParameters.PrimaryScreenWidth;
            }
            else
            {
                this.Left = window.Left + 15;
                this.Top = window.Top + 15;
                this.Height = window.Height - 30;
                this.Width = window.Width - 30;
            }
            if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
            window.Activate();
            window.Focus();
        }



        private void Confirm(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            CancelTask?.Invoke(this, e);
            this.DialogResult = false;
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.DialogResult = true;
            else if (e.Key == Key.Escape)
                this.DialogResult = false;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            TextBlock.Focus();
        }
    }

    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double height = 200;
            double.TryParse(value.ToString(), out height);

            if (height > 500) height = 500;
            return height + 80;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
