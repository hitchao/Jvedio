
using SuperControls.Style;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
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
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace Jvedio
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
    public partial class Dialog_Sponsor : SuperControls.Style.BaseDialog, System.ComponentModel.INotifyPropertyChanged
    {

        public static string Alipay = "https://hitchao.github.io/jvedioupdate/alipay.txt";
        public static string Wechat = "https://hitchao.github.io/jvedioupdate/wechat.txt";

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private async void BaseDialog_ContentRendered(object sender, EventArgs e)
        {

            string v1 = await getImage(Wechat);
            string v2 = await getImage(Alipay);
            if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
            {
                Loading = false;
                return;
            }

            BitmapImage wechatImage = new BitmapImage();
            wechatImage.BeginInit();
            wechatImage.StreamSource = new MemoryStream(System.Convert.FromBase64String(v1));
            wechatImage.EndInit();
            BitmapImage alipayImage = new BitmapImage();
            alipayImage.BeginInit();
            alipayImage.StreamSource = new MemoryStream(System.Convert.FromBase64String(v2));
            alipayImage.EndInit();



            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
            {
                wechat.Source = wechatImage;
                alipay.Source = alipayImage;
                Loading = false;
            });



        }

        private async Task<string> getImage(string url)
        {
            return await Task.Run(async () =>
            {
                HttpResult httpResult = await HttpClient.Get(url, CrawlerHeader.GitHub);
                if (httpResult.SourceCode != null)
                {
                    string content = httpResult.SourceCode;

                    string value = JvedioLib.Security.Encrypt.AesDecrypt(content, 2);
                    if (value.IndexOf(",") > 0)
                        value = value.Split(',')[1];
                    return value;
                }
                return null;
            });
        }



        public Dialog_Sponsor(Window window) : base(window, false)
        {
            InitializeComponent();
        }



        private bool _Loading = false;
        public bool Loading
        {
            get => _Loading;
            set
            {
                _Loading = value;
                NotifyPropertyChanged();
            }

        }



    }
}