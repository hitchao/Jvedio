
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    /// <summary>
    /// 主界面演员
    /// </summary>
    [Obsolete]
    public class Actress : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #region "属性"


        public static DateTime DEFAULT_DATE_TIME = new DateTime(1970, 01, 01);
        public int num { get; set; }// 仅仅用于计数

        public string id { get; set; }

        public string name { get; set; }

        public int sex { get; set; }

        public string actressimageurl { get; set; }

        private BitmapSource _smallimage;

        public BitmapSource smallimage {
            get { return _smallimage; }

            set {
                _smallimage = value;
                RaisePropertyChanged();
            }
        }

        public BitmapSource bigimage { get; set; }

        private string _birthday;

        public string birthday {
            get { return _birthday; }

            set {
                // 验证数据
                DateTime dateTime = DEFAULT_DATE_TIME;
                if (DateTime.TryParse(value, out dateTime))
                    _birthday = dateTime.ToString("yyyy-MM-dd");
                else
                    _birthday = string.Empty;
                RaisePropertyChanged();
            }
        }

        private int _age;

        public int age {
            get { return _age; }

            set {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 200)
                    a = 0;
                _age = a;
                RaisePropertyChanged();
            }
        }

        private int _height;

        public int height {
            get { return _height; }

            set {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 300)
                    a = 0;
                _height = a;
                RaisePropertyChanged();
            }
        }

        private string _cup;

        public string cup {
            get { return _cup; }

            set {
                if (string.IsNullOrEmpty(value))
                    _cup = string.Empty;
                else
                    _cup = value[0].ToString().ToUpper();
                RaisePropertyChanged();
            }
        }

        private int _hipline;

        public int hipline {
            get { return _hipline; }

            set {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 500)
                    a = 0;
                _hipline = a;
                RaisePropertyChanged();
            }
        }

        private int _waist;

        public int waist {
            get { return _waist; }

            set {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 500)
                    a = 0;
                _waist = a;
                RaisePropertyChanged();
            }
        }

        private int _chest;

        public int chest {
            get { return _chest; }

            set {
                int.TryParse(value.ToString(), out int a);
                if (a < 0 || a > 500)
                    a = 0;
                _chest = a;
            }
        }

        public string birthplace { get; set; }

        public string hobby { get; set; }

        public string sourceurl { get; set; }

        public string source { get; set; }

        public string imageurl { get; set; }

        public int like { get; set; }

        #endregion

        public Actress() : this(string.Empty)
        {
        }

        public Actress(string name = "")
        {
            id = string.Empty;
            this.name = name;
            sex = 1; // 女演员
            actressimageurl = string.Empty;
            smallimage = MetaData.DefaultActorImage;
            bigimage = null;
            birthday = string.Empty;
            age = 0;
            height = 0;
            cup = string.Empty;
            hipline = 0;
            waist = 0;
            chest = 0;
            birthplace = string.Empty;
            hobby = string.Empty;
            sourceurl = string.Empty;
            source = string.Empty;
            imageurl = string.Empty;
            like = 0;
        }

        public void Dispose()
        {
            smallimage = null;
            bigimage = null;
        }
        public ActorInfo toActorInfo()
        {
            ActorInfo info = new ActorInfo() {
                ActorName = name,
                Country = "Japan",
                Nation = string.Empty,
                BirthPlace = birthplace,
                Birthday = birthday,
                Age = age,
                BloodType = string.Empty,
                Height = height,
                Gender = Core.Enums.Gender.Girl,
                Hobby = hobby,
                Cup = '0',
                Chest = chest,
                Waist = waist,
                Hipline = hipline,
                WebType = source.Replace("jav", string.Empty),
                WebUrl = string.IsNullOrEmpty(sourceurl) ? imageurl : sourceurl,
            };
            if (!string.IsNullOrEmpty(cup))
                info.Cup = cup.ToCharArray()[0];

            return info;
        }
    }
}
