using DynamicData.Annotations;
using Jvedio.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.pojo.data
{
    public class SqliteInfo : INotifyPropertyChanged
    {


        public SqliteInfo()
        {
            Image = new BitmapImage(new Uri("/Resources/Picture/datalist_video.png", UriKind.Relative));
        }


        public int ID { get; set; }
        public double Size { get; set; }
        public InfoType Type { get; set; }

        public string _CreateDate;
        public string CreateDate { get { return _CreateDate; } set { _CreateDate = value; OnPropertyChanged(); } }        
        

        public string _Name;
        public string Name { get { return _Name; } set { _Name = value; OnPropertyChanged(); } }
        public string _Path;
        public string Path { get { return _Path; } set { _Path = value; OnPropertyChanged(); } }        
        public string _ImagePath="";
        public string ImagePath { get { return _ImagePath; } 
            set { 
                _ImagePath = value;
                if (File.Exists(_ImagePath))
                {
                    Image = ImageProcess.BitmapImageFromFile(_ImagePath);
                }
                OnPropertyChanged(); 
            } 
        }

        private BitmapSource _image ;
        public BitmapSource Image { get { return _image; } set { 
                _image = value; 
                OnPropertyChanged(); 
            } 
        }

        public Int64 _Count;
        public Int64 Count { get { return _Count; } set { _Count = value; OnPropertyChanged(); } }

         public Int64 _ViewCount;
        public Int64 ViewCount { get { return _ViewCount; } set { _ViewCount = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SqliteInfo o = obj as SqliteInfo;
            if(o == null) return false;
            return o.Type == this.Type && o.Name == this.Name && o.Path==this.Path;
        }

        public override int GetHashCode()
        {
            return  this.Type.GetHashCode()+this.Name.GetHashCode() + this.Path.GetHashCode();
        }
    }

}
