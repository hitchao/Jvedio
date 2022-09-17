
using SuperControls.Style;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Net;
using SuperUtils.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.Core.Global.UrlManager;

namespace Jvedio
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
    public partial class Dialog_Upgrade : SuperControls.Style.BaseDialog, System.ComponentModel.INotifyPropertyChanged
    {

        private static string UpgradeProgram = "Jvedio.Update.exe";


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




        private bool _IsUpgrading = false;
        public bool IsUpgrading
        {
            get => _IsUpgrading;
            set
            {
                _IsUpgrading = value;
                NotifyPropertyChanged();
            }

        }


        private bool _IsChecking = false;

        public bool IsChecking
        {
            get => _IsChecking;
            set
            {
                _IsChecking = value;
                NotifyPropertyChanged();
            }

        }
        private string _LatestVersion = "";

        public string LatestVersion
        {
            get => _LatestVersion;
            set
            {
                _LatestVersion = value;
                CanUpgrade = !string.IsNullOrEmpty(value);
                NotifyPropertyChanged();
            }

        }
        private string _ReleaseDate = "";

        public string ReleaseDate
        {
            get => _ReleaseDate;
            set
            {
                _ReleaseDate = value;
                NotifyPropertyChanged();
            }

        }
        private string _ReleaseNote = "";

        public string ReleaseNote
        {
            get => _ReleaseNote;
            set
            {
                _ReleaseNote = value;
                NotifyPropertyChanged();
            }

        }
        private string _LocalVersion = "";

        public string LocalVersion
        {
            get => _LocalVersion;
            set
            {
                _LocalVersion = value;
                NotifyPropertyChanged();
            }

        }
        private bool _CanUpgrade = false;

        public bool CanUpgrade
        {
            get => _CanUpgrade;
            set
            {
                _CanUpgrade = value;
                NotifyPropertyChanged();
            }

        }
        private double _UpgradeProgress = 0;

        public double UpgradeProgress
        {
            get => _UpgradeProgress;
            set
            {
                _UpgradeProgress = value;
                NotifyPropertyChanged();
            }

        }

        public Dialog_Upgrade(Window owner, bool showbutton, string latestVersion, string releaseDate, string releaseNote) : base(owner, showbutton)
        {
            InitializeComponent();
            this.LatestVersion = latestVersion;

            this.ReleaseDate = releaseDate;
            this.ReleaseNote = releaseNote;
            this.DataContext = this;

        }
        public Dialog_Upgrade(Window owner) : this(owner, false, "", "", "")
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //取消更新操作
            UpgradeHelper.Cancel();
            IsUpgrading = false;
            IsChecking = false;
            base.OnClosing(e);
        }



        private async void BeginUpgrade(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string text = button.Content.ToString();

            // 开始更新
            if (text == Jvedio.Language.Resources.BeginUpgrade)
            {
                button.Content = Jvedio.Language.Resources.StopUpgrade;

                UpgradeHelper.onCompleted += async (s, _) =>
                {
                    IsUpgrading = false;

                    // 调用 Jvedio.Update.exe
                    ////执行命令
                    //string arg = $"xcopy /y/e \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp")}\" \"{AppDomain.CurrentDomain.BaseDirectory}\"&TIMEOUT /T 1&start \"\" \"jvedio.exe\"";
                    //if (!Properties.Settings.Default.Debug) arg += " &exit";

                    //StreamHelper.TryWrite("upgrade.bat", arg);
                    //FileHelper.TryOpenFile("upgrade.bat");
                    if (!File.Exists(UpgradeProgram))
                    {
                        MessageCard.Error($"不存在 {UpgradeProgram} 更新取消");
                    }
                    else
                    {
                        FileHelper.TryOpenFile(UpgradeProgram);
                        await Task.Delay(1000);
                        Application.Current.Shutdown();
                    }

                };
                UpgradeHelper.onDownloading += (s, _) =>
                {
                    MessageCallBackEventArgs ev = _ as MessageCallBackEventArgs;
                    IsUpgrading = true;
                    double.TryParse(ev.Message, out double progress);
                    progress = Math.Round(progress, 2);
                    this.UpgradeProgress = progress;
                };
                UpgradeHelper.onError += (s, ev) =>
                {
                    string msg = (ev as MessageCallBackEventArgs).Message;
                    MessageCard.Error(msg);
                    // 发生错误后立即取消，防止升级错误
                    SetFailUpgradeStatus(button);
                };


                button.Style = (System.Windows.Style)App.Current.Resources["ButtonDanger"];
                bool success = await UpgradeHelper.BeginUpgrade((msg) =>
                  {
                      MessageCard.Info(msg);
                  });
                if (!success)
                {
                    // 更新不成功
                    SetFailUpgradeStatus(button);
                }
            }
            else
            {
                SetFailUpgradeStatus(button);
            }





        }

        private void SetFailUpgradeStatus(Button button)
        {
            button.Content = Jvedio.Language.Resources.BeginUpgrade;
            button.Style = (System.Windows.Style)App.Current.Resources["ButtonStyleFill"];
            UpgradeHelper.Cancel();
            IsUpgrading = false;
            IsChecking = false;
        }

        private void GotoDownloadUrl(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(ReleaseUrl);
        }



        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            UpgradeSourceTextBlock.Text = $"{Jvedio.Language.Resources.UpgradeSource}：{UpgradeSource}";
            LocalVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }


        private async void CheckUpgrade(object sender, RoutedEventArgs e)
        {
            if (IsChecking || IsUpgrading) return;
            IsChecking = true;
            try
            {
                (string LatestVersion, string ReleaseDate, string ReleaseNote) result = await UpgradeHelper.getUpgardeInfo();
                this.LatestVersion = result.LatestVersion;
                this.ReleaseDate = result.ReleaseDate;
                this.ReleaseNote = result.ReleaseNote;
            }
            catch (Exception ex)
            {
                MessageCard.Error(ex.Message);
            }


            IsChecking = false;
        }

    }
}