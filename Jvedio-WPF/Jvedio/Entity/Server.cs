using DynamicData.Annotations;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.Utils;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jvedio.Entity
{

    /// <summary>
    /// 服务器源
    /// </summary>
    public class Server : INotifyPropertyChanged
    {
        public Server(string name)
        {
            this.Name = name;
        }


        public Server()
        {

        }


        private bool isEnable = false;
        private string url = "";
        private string cookie = "";
        private int available = 0;//指示测试是否通过
        private string name = "";
        private string lastRefreshDate = "";

        public bool IsEnable { get => isEnable; set { isEnable = value; OnPropertyChanged(); } }


        public string Url
        {
            get => url; set
            {
                url = value.ToString().ToProperUrl();
                OnPropertyChanged();
            }
        }
        public string Cookie { get => cookie; set { cookie = value; OnPropertyChanged(); } }

        public int Available
        {
            get => available; set
            {
                available = value;
                OnPropertyChanged();
            }
        }
        public string Name { get => name; set { name = value; OnPropertyChanged(); } }
        public string LastRefreshDate { get => lastRefreshDate; set { lastRefreshDate = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

}
