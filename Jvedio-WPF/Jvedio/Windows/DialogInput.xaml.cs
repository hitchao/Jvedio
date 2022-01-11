using System;
using System.Windows;
using System.Windows.Input;

namespace Jvedio
{
    public partial class DialogInput : Window
    {

        public DialogInput(Window window, string title, string defaultContent = "")
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            TitleTextBlock.Text = title;
            ContentTextBox.Text = defaultContent;
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
        }

        public string Text
        {
            get { return ContentTextBox.Text; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ContentTextBox.SelectAll();
            ContentTextBox.Focus();
        }


        private void Confirm(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ContentTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.DialogResult = true;
            else if (e.Key == Key.Escape)
                this.DialogResult = false;
            else if (e.Key == Key.Delete)
                ContentTextBox.Text = "";
        }
    }
}
