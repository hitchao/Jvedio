using SuperControls.Style;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jvedio
{
    /// <summary>
    /// Window_TagStamp.xaml 的交互逻辑
    /// </summary>
    public partial class Window_TagStamp : BaseDialog
    {
        #region "属性"

        public string TagName { get; set; }
        private int idx { get; set; }

        private SolidColorBrush _BackgroundBrush = Brushes.Orange;

        public SolidColorBrush BackgroundBrush {
            get { return _BackgroundBrush; }
            set { _BackgroundBrush = value; }
        }

        private SolidColorBrush _ForegroundBrush = Brushes.White;

        public SolidColorBrush ForegroundBrush {
            get { return _ForegroundBrush; }
            set { _ForegroundBrush = value; }
        }

        #endregion

        public Window_TagStamp()
        {
            InitializeComponent();
        }

        public Window_TagStamp(string name, SolidColorBrush background, SolidColorBrush foreground) : this()
        {
            textBox.Text = name;
            border.Background = background;
            border2.Background = foreground;
            BackgroundBrush = background;
            ForegroundBrush = foreground;
        }

        private async void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(100);
            colorPopup.IsOpen = true;
            idx = 0;
        }

        private void ColorPicker_Canceled(object sender, EventArgs e)
        {
            colorPopup.IsOpen = false;
        }


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TagName = (sender as SearchBox).Text;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void border2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(100);
            colorPopup.IsOpen = true;
            idx = 1;
        }

        private void BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            textBox.Focus();
            textBox.SelectAll();
        }

        private void CancelColorPicker(object sender, RoutedEventArgs e)
        {
            colorPopup.IsOpen = false;
        }

        private void ConfirmColorPicker(object sender, RoutedEventArgs e)
        {
            colorPopup.IsOpen = false;
            if (idx == 0) {
                border.Background = new SolidColorBrush(colorPicker.SelectedColor);
                BackgroundBrush = new SolidColorBrush(colorPicker.SelectedColor);
            } else {
                border2.Background = new SolidColorBrush(colorPicker.SelectedColor);
                ForegroundBrush = new SolidColorBrush(colorPicker.SelectedColor);
            }
        }
    }
}
