using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{

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
            sex = 1;//女演员
            actressimageurl = "";
            smallimage = GlobalVariable.DefaultActorImage;
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
        public int sex { get; set; }
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

        public ActorInfo toActorInfo()
        {
            ActorInfo info = new ActorInfo()
            {
                ActorName = name,
                Country = "Japan",
                Nation = "",
                BirthPlace = birthplace,
                Birthday = birthday,
                Age = age,
                BloodType = "",
                Height = height,
                Gender = Core.Enums.Gender.Girl,
                Hobby = hobby,
                Cup = '0',
                Chest = chest,
                Waist = waist,
                Hipline = hipline,
                WebType = source.Replace("jav", ""),
                WebUrl = string.IsNullOrEmpty(sourceurl) ? imageurl : sourceurl,
            };
            if (!string.IsNullOrEmpty(cup))
                info.Cup = cup.ToCharArray()[0];



            return info;
        }


    }


}
