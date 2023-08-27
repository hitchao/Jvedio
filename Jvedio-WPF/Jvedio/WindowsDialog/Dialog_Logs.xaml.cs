using ICSharpCode.AvalonEdit;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Logs : SuperControls.Style.BaseDialog
    {
        public Dialog_Logs(string text) : base(false)
        {
            InitializeComponent();
            textBox.Text = text;
        }

        private void textBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.GotFocus(sender);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.LostFocus(sender);
        }
    }
}