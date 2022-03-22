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
using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Jvedio.Core.SimpleMarkDown;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Notice : Jvedio.Style.BaseDialog
    {

        public string Message { get; set; }

        public Dialog_Notice(Window owner, bool showbutton, string message) : base(owner, showbutton)
        {
            InitializeComponent();
            Message = message;
            this.ContentRendered += RenderContent;
        }

        public void RenderContent(object sender, EventArgs e)
        {
            richTextBox.Document = MarkDown.parse(Message);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string configName = "Notice";
            //获取本地的公告
            string notices = "";
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            wrapper.Eq("ConfigName", configName);
            AppConfig appConfig = GlobalMapper.appConfigMapper.selectOne(wrapper);
            if (appConfig != null && !string.IsNullOrEmpty(appConfig.ConfigValue))
                notices = appConfig.ConfigValue.Replace(GlobalVariable.Separator, '\n');
            this.Message = notices;
            RenderContent(sender, e);
        }
    }
}