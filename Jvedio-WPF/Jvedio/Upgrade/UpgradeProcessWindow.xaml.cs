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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jvedio
{
    /// <summary>
    /// UpgradeProcessWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class UpgradeProcessWindow : ChaoControls.Style.BaseWindow
    {
        public bool IsClosed { get; set; }
        public UpgradeProcessWindow()
        {
            InitializeComponent();
            IsClosed = false;
        }

        private void BaseWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }
    }
}
