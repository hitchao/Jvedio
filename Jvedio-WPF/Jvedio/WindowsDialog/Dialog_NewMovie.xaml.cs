using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_NewMovie : SuperControls.Style.BaseDialog
    {


        public bool AutoAddPrefix { get; set; } = ConfigManager.Settings.AutoAddPrefix;

        public string Prefix { get; set; } = ConfigManager.Settings.Prefix;

        public VideoType VideoType { get; set; } = VideoType.Normal;

        public NewVideoDialogResult Result { get; private set; }


        public Dialog_NewMovie() : base(true)
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            List<RadioButton> radioButtons = videoTypeStackPanel.Children.OfType<RadioButton>().ToList();

            for (int i = 0; i < radioButtons.Count; i++) {
                if ((bool)radioButtons[i].IsChecked) {
                    VideoType = (VideoType)i;
                    break;
                }
            }

            Result = new NewVideoDialogResult(AddMovieTextBox.Text, AutoAddPrefix ? Prefix : string.Empty, VideoType);
            base.Confirm(sender, e);
        }



        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            AddMovieTextBox.Focus();
        }


        private void BaseDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Settings.AutoAddPrefix = AutoAddPrefix;
            ConfigManager.Settings.Prefix = Prefix;
            ConfigManager.Settings.Save();
        }


        private void AddMovieTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((sender as TextBox).Parent as Border).BorderBrush = Brushes.Transparent;

            if (string.IsNullOrEmpty(AddMovieTextBox.Text))
                placeHolderTextBlock.Visibility = Visibility.Visible;
        }

        private void placeHolderTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddMovieTextBox.Focus();
        }

        private void AddMovieTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AddMovieTextBox.Text))
                placeHolderTextBlock.Visibility = Visibility.Visible;
            else
                placeHolderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void AddMovieTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((sender as TextBox).Parent as Border).BorderBrush = (SolidColorBrush)Application.Current.Resources["Button.Selected.BorderBrush"];
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