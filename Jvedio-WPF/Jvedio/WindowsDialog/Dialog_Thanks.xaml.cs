using SuperControls.Style;
using SuperUtils.IO;
using System.Windows;
using System.Windows.Documents;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Thanks : SuperControls.Style.BaseDialog
    {
        public Dialog_Thanks() : base(false)
        {
            InitializeComponent();
        }

        private void OpenUrl(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            FileHelper.TryOpenUrl(hyperlink.NavigateUri.ToString(), (err) =>
            {
                MessageCard.Error(err);
            });
        }
    }
}