
using System;
using System.Windows;
using System.Windows.Documents;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Logs : Jvedio.Style.BaseDialog
    {


        public Dialog_Logs(Window owner, string text) : base(owner, false)
        {
            InitializeComponent();
            textBox.Text = text;
        }


    }
}