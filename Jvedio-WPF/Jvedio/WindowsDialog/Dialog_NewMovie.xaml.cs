using Jvedio.Core.Enums;
using SuperControls.Style;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_NewMovie : SuperControls.Style.BaseDialog
    {
        public NewVideoDialogResult Result { get; private set; }


        public bool AutoAddPrefix { get; set; }

        public string Prefix { get; set; }

        public VideoType VideoType { get; set; }

        public Dialog_NewMovie(Window owner) : base(owner)
        {
            InitializeComponent();
            VideoType = VideoType.Censored;
            autoPrefix.IsChecked = AutoAddPrefix;
            PrefixTextBox.Text = Prefix;
            if (!ConfigManager.Settings.TeenMode)
            {
                videoTypeWrapPanel.Visibility = Visibility.Visible;
            }
            autoPrefix.IsChecked = ConfigManager.Settings.AutoAddPrefix;
            PrefixTextBox.Text = ConfigManager.Settings.Prefix;

            AutoAddPrefix = ConfigManager.Settings.AutoAddPrefix;
            Prefix = ConfigManager.Settings.Prefix;


        }

        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            Result = new NewVideoDialogResult(AddMovieTextBox.Text, AutoAddPrefix ? Prefix : "", VideoType);
            base.Confirm(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //WindowTools windowTools = (WindowTools)FileProcess.GetWindowByName("WindowTools");
            //if (windowTools == null) windowTools = new WindowTools();
            //windowTools.Show();
            //windowTools.Activate();
            //windowTools.TabControl.SelectedIndex = 0;
            //this.Close();
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            AddMovieTextBox.Focus();

        }

        private void SetChecked(object sender, RoutedEventArgs e)
        {
            AutoAddPrefix = (bool)(sender as CheckBox).IsChecked;
        }



        private void PrefixTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Prefix = (sender as SearchBox).Text;
        }

        private void PrefixTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Prefix = (sender as SearchBox).Text;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoType = (VideoType)((sender as ComboBox).SelectedIndex);
        }

        private void BaseDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Settings.AutoAddPrefix = AutoAddPrefix;
            ConfigManager.Settings.Prefix = Prefix;
            ConfigManager.Settings.Save();
        }
    }



    public class NewVideoDialogResult
    {
        public string Text { get; set; }

        public string Prefix { get; set; }

        public VideoType VideoType { get; set; }


        public NewVideoDialogResult(string text, string prefix, VideoType videoType)
        {
            Text = text;
            Prefix = prefix;
            VideoType = videoType;

        }
    }


}