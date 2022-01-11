using Jvedio.Utils.FileProcess;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
    public partial class Dialog_Upgrade : Jvedio.Style.BaseDialog, System.ComponentModel.INotifyPropertyChanged
    {
        private string remote = "";
        private string log = "";
        private Upgrade upgrade;

        private bool IsClosed = false;
        private bool isChecking = false;
        private bool isUpgrading = false;


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsUpgrading
        {
            get => isUpgrading;
            set
            {
                if (value != isUpgrading)
                {
                    isUpgrading = value;
                    if (value) IsChecking = true;
                    else IsChecking = false;
                    NotifyPropertyChanged();
                }
            }

        }

        public bool IsChecking
        {
            get => isChecking;
            set
            {
                if (value != isChecking)
                {
                    isChecking = value;
                    NotifyPropertyChanged();
                }
            }

        }

        public Dialog_Upgrade(Window owner, bool showbutton, string remote, string log) : base(owner, showbutton)
        {
            InitializeComponent();
            this.remote = remote;
            this.log = log;
            this.DataContext = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //取消更新操作
            upgrade?.Stop();
            IsUpgrading = false;
            IsChecking = false;
            IsClosed = true;
            base.OnClosing(e);
        }



        private void BeginUpgrade(object sender, RoutedEventArgs e)
        {
            if (IsChecking || IsUpgrading) return;

            Button button = (Button)sender;
            string text = button.Content.ToString();
            if (text == Jvedio.Language.Resources.BeginUpgrade)
            {
                button.Content = Jvedio.Language.Resources.StopUpgrade;
                upgrade = new Upgrade();
                upgrade.UpgradeCompleted += (s, _) =>
                {
                    IsUpgrading = false;
                    //执行命令
                    string arg = $"xcopy /y/e \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp")}\" \"{AppDomain.CurrentDomain.BaseDirectory}\"&TIMEOUT /T 1&start \"\" \"jvedio.exe\" &exit";
                    StreamHelper.TryWrite("upgrade.bat", arg);
                    FileHelper.TryOpenFile("upgrade.bat");
                    Application.Current.Shutdown();
                };

                upgrade.onProgressChanged += (s, _) =>
                {
                    ProgressBUpdateEventArgs ev = _ as ProgressBUpdateEventArgs;
                    IsUpgrading = true;
                    if (ev.maximum != 0)
                    {
                        UpgradeProgressBar.Value = (int)(ev.value / ev.maximum * 100);
                    }
                };
                button.Style = (System.Windows.Style)App.Current.Resources["ButtonDanger"];
                upgrade.Start();
            }
            else
            {
                button.Content = Jvedio.Language.Resources.BeginUpgrade;
                button.Style = (System.Windows.Style)App.Current.Resources["ButtonStyleFill"];
                upgrade?.Stop();
                IsUpgrading = false;
                IsChecking = false;
            }





        }

        private void GotoDownloadUrl(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(ReleaseUrl);
        }



        private async void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            UpgradeSourceTextBlock.Text = $"{Jvedio.Language.Resources.UpgradeSource}：{GlobalVariable.UpgradeSource}";
            LocalVersionTextBlock.Text = $"{Jvedio.Language.Resources.CurrentVersion}：{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            if (remote != "")
            {
                RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                UpdateContentTextBox.Text = GetContentByLanguage(log);
            }
            else
            {
                //TODO
                IsChecking = true;
                (bool success, string remote, string updateContent) = await new MyNet().CheckUpdate(UpdateUrl);
                string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (success && !IsClosed)
                {
                    RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                    UpdateContentTextBox.Text = GetContentByLanguage(updateContent);
                }
                IsChecking = false;
            }
        }

        private async void CheckUpgrade(object sender, RoutedEventArgs e)
        {
            if (IsChecking || IsUpgrading) return;
            IsChecking = true;
            (bool success, string remote, string updateContent) = await new MyNet().CheckUpdate(UpdateUrl);
            string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (success && !IsClosed)
            {
                RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                UpdateContentTextBox.Text = GetContentByLanguage(updateContent);
            }
            IsChecking = false;
        }

        private string GetContentByLanguage(string content)
        {
            int start = -1;
            int end = -1;
            switch (Properties.Settings.Default.Language)
            {

                case "中文":
                    end = content.IndexOf("--English--");
                    if (end == -1) return content;
                    else return content.Substring(0, end).Replace("--中文--", "");

                case "English":
                    start = content.IndexOf("--English--");
                    end = content.IndexOf("--日本語--");
                    if (end == -1 || start == -1) return content;
                    else return content.Substring(start, end - start).Replace("--English--", "");

                case "日本語":
                    start = content.IndexOf("--日本語--");
                    if (start == -1) return content;
                    else return content.Substring(start).Replace("--日本語--", "");

                default:
                    return content;
            }
        }
    }
}