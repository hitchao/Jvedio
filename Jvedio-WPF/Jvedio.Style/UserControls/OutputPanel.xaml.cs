using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Jvedio.Style.UserControls
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class OutputPanel : UserControl
    {


        public OutputPanel()
        {
            InitializeComponent();

            ClearButton.Click += ClearButton_Clear;

        }

        public void ClearButton_Clear(object o, RoutedEventArgs e)
        {
            Clear();
        }

        public void Clear()
        {
            StatusTextBox.Clear();
        }


        public void ScrollToEnd()
        {
            StatusTextBox.ScrollToEnd();
        }

        public void AppendText(string str, bool newline)
        {
            if (newline) str += Environment.NewLine;
            StatusTextBox.AppendText(str);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
