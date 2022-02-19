using DynamicData.Annotations;
using Jvedio.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.pojo
{
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
            smallimage = GlobalVariable.DefaultSmallImage;
            bigimage = GlobalVariable.DefaultBigImage;
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
                    if (GlobalVariable.JvedioServers.Bus.Url.IsProperUrl())
                    {
                        Uri uri = new Uri(GlobalVariable.JvedioServers.Bus.Url);
                        result = uri1.OriginalString.Replace(uri1.Host, uri.Host);
                    }
                }
                else if (Source == "JAVDB")
                {
                    if (GlobalVariable.JvedioServers.DB.Url.IsProperUrl())
                    {
                        Uri uri = new Uri(GlobalVariable.JvedioServers.DB.Url);
                        result = uri1.OriginalString.Replace(uri1.Host, uri.Host);
                    }
                }
                else if (Source == "javlibrary".ToUpper())
                {
                    if (GlobalVariable.JvedioServers.Library.Url.IsProperUrl())
                    {
                        Uri uri = new Uri(GlobalVariable.JvedioServers.Library.Url);
                        result = uri1.OriginalString.Replace(uri1.Host, uri.Host);
                    }
                }

            }

            return result;
        }


    }

}
