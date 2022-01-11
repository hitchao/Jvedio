using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;

namespace Jvedio
{



    public class Magnet
    {
        //  (id VARCHAR(50) PRIMARY KEY, link TEXT , title TEXT , size TEXT, releasedate VARCHAR(10) DEFAULT '1900-01-01', tag TEXT)";

        public Magnet() : this("") { }


        public Magnet(string id)
        {
            this.id = id;
            link = "";
            title = "";
            size = 0;
            releasedate = "1970-01-01";
            tag = new List<string>();
        }



        public string id { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        public double size { get; set; }
        public string releasedate { get; set; }
        public List<string> tag { get; set; }

    }

    /// <summary>
    /// Jvedio 影片
    /// </summary>
    public class Movie : IDisposable
    {
        public Movie(string id)
        {
            this.id = id;
            title = "";
            filesize = 0;
            filepath = "";
            hassubsection = false;
            subsection = "";
            subsectionlist = new List<string>();
            tagstamps = "";
            vediotype = 1;
            scandate = "";
            visits = 0;
            releasedate = "";
            director = "";
            tag = "";
            runtime = 0;
            genre = "";
            actor = "";
            actorid = "";
            studio = "";
            rating = 0;
            chinesetitle = "";
            favorites = 0;
            label = "";
            plot = "";
            outline = "";
            year = 1970;
            runtime = 0;
            country = "";
            countrycode = 0;
            otherinfo = "";
            sourceurl = "";
            source = "";
            actressimageurl = "";
            smallimageurl = "";
            bigimageurl = "";
            extraimageurl = "";
            smallimage = DefaultSmallImage;
            bigimage = DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
        }
        public Movie() : this("") { }

        public virtual void Dispose()
        {
            subsectionlist.Clear();
            smallimage = null;
            bigimage = null;
        }


        public bool IsToDownLoadInfo()
        {
            return this != null && (this.title == "" || this.sourceurl == "" || this.smallimageurl == "" || this.bigimageurl == "");
        }


        public string id { get; set; }
        private string _title;
        public string title { get { return _title; } set { _title = value; OnPropertyChanged(); } }
        public double filesize { get; set; }

        private string _filepath;
        public string filepath
        {
            get { return _filepath; }

            set
            {
                _filepath = value;
                OnPropertyChanged();
            }
        }
        public bool hassubsection { get; set; }

        private string _subsection;
        public string subsection
        {
            get { return _subsection; }
            set
            {
                _subsection = value;
                string[] subsections = value.Split(';');
                if (subsections.Length >= 2)
                {
                    hassubsection = true;
                    subsectionlist = new List<string>();
                    foreach (var item in subsections)
                    {
                        if (!string.IsNullOrEmpty(item)) subsectionlist.Add(item);
                    }
                }
                OnPropertyChanged();
            }
        }

        public List<string> subsectionlist { get; set; }

        public string tagstamps { get; set; }

        public int vediotype { get; set; }
        public string scandate { get; set; }


        private string _releasedate;
        public string releasedate
        {
            get { return _releasedate; }
            set
            {
                DateTime dateTime = new DateTime(1970, 01, 01);
                DateTime.TryParse(value.ToString(), out dateTime);
                _releasedate = dateTime.ToString("yyyy-MM-dd");
            }
        }
        public int visits { get; set; }
        public string director { get; set; }
        public string genre { get; set; }
        public string tag { get; set; }


        public string actor { get; set; }
        public string actorid { get; set; }
        public string studio { get; set; }
        public float rating { get; set; }
        public string chinesetitle { get; set; }
        public int favorites { get; set; }
        public string label { get; set; }
        public string plot { get; set; }
        public string outline { get; set; }
        public int year { get; set; }
        public int runtime { get; set; }
        public string country { get; set; }
        public int countrycode { get; set; }
        public string otherinfo { get; set; }
        public string sourceurl { get; set; }
        public string source { get; set; }

        public string actressimageurl { get; set; }
        public string smallimageurl { get; set; }
        public string bigimageurl { get; set; }
        public string extraimageurl { get; set; }


        private Uri _GifUri;

        public Uri GifUri
        {
            get
            {
                return _GifUri;
            }

            set
            {
                _GifUri = value;
                OnPropertyChanged();
            }

        }

        private BitmapSource _smallimage;
        public BitmapSource smallimage { get { return _smallimage; } set { _smallimage = value; OnPropertyChanged(); } }

        private BitmapSource _bigimage;
        public BitmapSource bigimage { get { return _bigimage; } set { _bigimage = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 将source url 中的 链接替换掉
        /// </summary>
        /// <returns></returns>
        public string GetSourceUrl()
        {
            string result = "";
            if (this.sourceurl.IsProperUrl())
            {
                Uri uri1 = new Uri(sourceurl);
                // 需要替换网址的有 ： bus db library
                string Source = source.ToUpper();

                if (Source == "JAVBUS")
                {
                    if (JvedioServers.Bus.Url.IsProperUrl())
                    {
                        Uri uri = new Uri(JvedioServers.Bus.Url);
                        result = uri1.OriginalString.Replace(uri1.Host, uri.Host);
                    }
                }
                else if (Source == "JAVDB")
                {
                    if (JvedioServers.DB.Url.IsProperUrl())
                    {
                        Uri uri = new Uri(JvedioServers.DB.Url);
                        result = uri1.OriginalString.Replace(uri1.Host, uri.Host);
                    }
                }
                else if (Source == "javlibrary".ToUpper())
                {
                    if (JvedioServers.Library.Url.IsProperUrl())
                    {
                        Uri uri = new Uri(JvedioServers.Library.Url);
                        result = uri1.OriginalString.Replace(uri1.Host, uri.Host);
                    }
                }

            }

            return result;
        }


    }




    /// <summary>
    /// 详情页面的 Jvedio 影片，多了预览图、类别、演员、标签
    /// </summary>
    public class DetailMovie : Movie
    {
        public DetailMovie() : base()
        {
            genrelist = new List<string>();
            actorlist = new List<Actress>();
            labellist = new List<string>();
            extraimagelist = new ObservableCollection<BitmapSource>();
            extraimagePath = new ObservableCollection<string>();
        }

        public override void Dispose()
        {
            genrelist.Clear();
            actorlist.Clear();
            labellist.Clear();
            extraimagelist.Clear();
            extraimagePath.Clear();
            base.Dispose();
        }


        public List<string> genrelist { get; set; }
        public List<Actress> actorlist { get; set; }

        private List<string> _labellist;
        public List<string> labellist
        {
            get
            {
                return _labellist;
            }
            set
            {
                _labellist = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BitmapSource> extraimagelist { get; set; }
        public ObservableCollection<string> extraimagePath { get; set; }




    }



    /// <summary>
    /// 视频信息
    /// </summary>
    public class VideoInfo
    {
        public string Format { get; set; }//视频格式
        public string BitRate { get; set; }//总码率
        public string Duration { get; set; }
        public string FileSize { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Resolution { get; set; }
        public string DisplayAspectRatio { get; set; }//宽高比
        public string FrameRate { get; set; }//帧率
        public string BitDepth { get; set; }//位深度
        public string PixelAspectRatio { get; set; }//像素宽高比
        public string Encoded_Library { get; set; }//编码库
        public string FrameCount { get; set; }//总帧数
        //音频信息
        public string AudioFormat { get; set; }
        public string AudioBitRate { get; set; }//码率
        public string AudioSamplingRate { get; set; }//采样率
        public string Channel { get; set; }//声道数

        public string Extension { get; set; }

        public string FileName { get; set; }

        public VideoInfo()
        {
            Format = "";
            BitRate = "";
            Duration = "";
            FileSize = "";
            Width = "";
            Height = "";
            Resolution = "";
            DisplayAspectRatio = "";
            FrameRate = "";
            BitDepth = "";
            PixelAspectRatio = "";
            Encoded_Library = "";
            FrameCount = "";
            AudioFormat = "";
            AudioBitRate = "";
            AudioSamplingRate = "";
            Channel = "";
            Extension = "";
            FileName = "";
        }

    }




    /// <summary>
    /// 【按类别】中的分类
    /// </summary>
    public class Genre
    {
        public List<string> theme { get; set; }
        public List<string> role { get; set; }
        public List<string> clothing { get; set; }
        public List<string> body { get; set; }
        public List<string> behavior { get; set; }
        public List<string> playmethod { get; set; }
        public List<string> other { get; set; }
        public List<string> scene { get; set; }

        public Genre()
        {
            theme = new List<string>();
            role = new List<string>();
            clothing = new List<string>();
            body = new List<string>();
            behavior = new List<string>();
            playmethod = new List<string>();
            other = new List<string>();
            scene = new List<string>();
        }

    }
    /// <summary>
    /// 主界面演员
    /// </summary>
    public class Actress : INotifyPropertyChanged, IDisposable
    {

        public Actress() : this("") { }

        public Actress(string name = "")
        {
            id = "";
            this.name = name;
            actressimageurl = "";
            smallimage = DefaultActorImage;
            bigimage = null;
            birthday = "";
            age = 0;
            height = 0;
            cup = "";
            hipline = 0;
            waist = 0;
            chest = 0;
            birthplace = "";
            hobby = "";
            sourceurl = "";
            source = "";
            imageurl = "";
            like = 0;

        }
        public void Dispose()
        {
            smallimage = null;
            bigimage = null;
        }

        public int num { get; set; }//仅仅用于计数
        public string id { get; set; }
        public string name { get; set; }
        public string actressimageurl { get; set; }
        private BitmapSource _smallimage;
        public BitmapSource smallimage { get { return _smallimage; } set { _smallimage = value; OnPropertyChanged(); } }
        public BitmapSource bigimage { get; set; }


        private string _birthday;
        public string birthday
        {
            get { return _birthday; }
            set
            {
                //验证数据
                DateTime dateTime = new DateTime(1900, 01, 01);
                if (DateTime.TryParse(value, out dateTime)) _birthday = dateTime.ToString("yyyy-MM-dd");
                else _birthday = "";
                OnPropertyChanged();
            }
        }

        private int _age;
        public int age
        {
            get { return _age; }
            set
            {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 200) a = 0;
                _age = a;
                OnPropertyChanged();
            }
        }

        private int _height;
        public int height
        {
            get { return _height; }
            set
            {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 300) a = 0;
                _height = a;
                OnPropertyChanged();
            }
        }

        private string _cup;
        public string cup
        {
            get { return _cup; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _cup = "";
                else
                    _cup = value[0].ToString().ToUpper();
                OnPropertyChanged();
            }
        }


        private int _hipline;
        public int hipline
        {
            get { return _hipline; }
            set
            {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 500) a = 0;
                _hipline = a;
                OnPropertyChanged();
            }
        }


        private int _waist;
        public int waist
        {
            get { return _waist; }
            set
            {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 500) a = 0;
                _waist = a;
                OnPropertyChanged();
            }
        }


        private int _chest;
        public int chest
        {
            get { return _chest; }
            set
            {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 500) a = 0;
                _chest = a;
            }
        }

        public string birthplace { get; set; }
        public string hobby { get; set; }

        public string sourceurl { get; set; }
        public string source { get; set; }
        public string imageurl { get; set; }

        public int like { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }


    public class WindowProperty
    {
        public WindowProperty()
        {
            Location = new Point(0, 0);
            Size = new Size(0, 0);
            WinState = JvedioWindowState.Normal;
        }
        public Point Location { get; set; }
        public Size Size { get; set; }

        public JvedioWindowState WinState { get; set; }
    }


    public class MyListItem : INotifyPropertyChanged
    {
        private long number = 0;
        private string name = "";
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public long Number
        {
            get
            {
                return number;
            }

            set
            {
                number = value;
                OnPropertyChanged();
            }

        }

        public MyListItem(string name, long number)
        {
            this.Name = name;
            this.Number = number;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


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



    public class Servers
    {

        public Server Bus { get; set; }
        public Server BusEurope { get; set; }
        public Server Library { get; set; }
        public Server FC2 { get; set; }
        public Server Jav321 { get; set; }
        public Server DMM { get; set; }
        public Server DB { get; set; }
        public Server MOO { get; set; }


        public Servers()
        {
            Bus = new Server("Bus");
            BusEurope = new Server("BusEurope");
            Library = new Server("Library");
            FC2 = new Server("FC2");
            Jav321 = new Server("Jav321");
            DMM = new Server("DMM");
            MOO = new Server("MOO");
        }

        public void Save()
        {
            ServerConfig.Instance.SaveServer(Bus);
            ServerConfig.Instance.SaveServer(BusEurope);
            ServerConfig.Instance.SaveServer(Library);
            ServerConfig.Instance.SaveServer(FC2);
            ServerConfig.Instance.SaveServer(Jav321);
            ServerConfig.Instance.SaveServer(DMM);
            ServerConfig.Instance.SaveServer(DB);
            ServerConfig.Instance.SaveServer(MOO);
        }

        /// <summary>
        /// 检查是否启用服务器源且地址不为空
        /// </summary>
        /// <returns></returns>
        public bool IsProper()
        {
            return Jav321.IsEnable && !string.IsNullOrEmpty(Jav321.Url)
                                || Bus.IsEnable && !string.IsNullOrEmpty(Bus.Url)
                                || BusEurope.IsEnable && !string.IsNullOrEmpty(BusEurope.Url)
                                || Library.IsEnable && !string.IsNullOrEmpty(Library.Url)
                                || DB.IsEnable && !string.IsNullOrEmpty(DB.Url)
                                || FC2.IsEnable && !string.IsNullOrEmpty(FC2.Url)
                                || DMM.IsEnable && !string.IsNullOrEmpty(DMM.Url)
                                || MOO.IsEnable && !string.IsNullOrEmpty(MOO.Url);
        }

    }










}
