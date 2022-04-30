
using System;
using System.Windows;
using System.Windows.Documents;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_About : Jvedio.Style.BaseDialog
    {

        public Dialog_About(Window owner,bool showbutton) : base(owner, showbutton)
        {
            InitializeComponent();

        }

        private void OpenUrl(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            FileHelper.TryOpenUrl(hyperlink.NavigateUri.ToString());
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            VersionTextBlock.Text = Jvedio.Language.Resources.Version + $" : {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        }
    }
}