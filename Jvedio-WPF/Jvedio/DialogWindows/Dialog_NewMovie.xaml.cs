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
using Jvedio.Utils;
using Jvedio.Style;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_NewMovie : Jvedio.Style.BaseDialog
    {
        public NewMovieDialogResult Result { get; private set; }

        public Dialog_NewMovie(Window owner) : base(owner)
        {
            InitializeComponent();
            RadioButtonStackPanel.Children.OfType<RadioButton>().ToList()[DefaultNewMovieType].IsChecked = true;
            FC2Checked.IsChecked = AutoAddPrefix;
            PrefixTextBox.Text = Prefix;
        }

        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            var rbs = RadioButtonStackPanel.Children.OfType<RadioButton>().ToList();
            int idx = rbs.FindIndex(arg => arg.IsChecked == true);
            Result = new NewMovieDialogResult(AddMovieTextBox.Text, idx);
            base.Confirm(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowTools windowTools = (WindowTools)FileProcess.GetWindowByName("WindowTools");
            if (windowTools == null) windowTools = new WindowTools();
            windowTools.Show();
            windowTools.Activate();
            windowTools.TabControl.SelectedIndex = 0;
            this.Close();
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            AddMovieTextBox.Focus();

        }

        private void SetFc2Checked(object sender, RoutedEventArgs e)
        {
            AutoAddPrefix = (bool) (sender as CheckBox).IsChecked;
        }

        private void SetNewMovieType(object sender, RoutedEventArgs e)
        {
            var r = RadioButtonStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < r.Count; i++)
            {
                if ((bool)r[i].IsChecked)
                {
                    DefaultNewMovieType = i;
                    break;
                }
            }
        }

        private void PrefixTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Prefix = (sender as TextBox).Text;
        }

        private void PrefixTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Prefix = (sender as TextBox).Text;
        }
    }


    public class NewMovieDialogResult : JvedioDialogResult
    {

        public VedioType VedioType {get;set;}
        public NewMovieDialogResult(string text, int option) : base(text, option)
        {
            VedioType = (VedioType)(option+1);
        }
    }
}