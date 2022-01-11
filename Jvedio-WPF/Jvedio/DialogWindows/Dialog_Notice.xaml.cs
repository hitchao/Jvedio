using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using System.Windows.Documents;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Notice : Jvedio.Style.BaseDialog
    {

        
        public Dialog_Notice(Window owner,bool showbutton,string content) : base(owner, showbutton)
        {
            InitializeComponent();
            this.ContentRendered += (s, e) => NoticeTextBlock.Text = content;
        }



    }
}