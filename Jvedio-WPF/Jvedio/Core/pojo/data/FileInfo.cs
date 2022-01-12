using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.pojo.data
{
    public class FileInfo
    {

        public string _CreateDate;
        public string CreateDate { get { return _CreateDate; } set { _CreateDate = value; OnPropertyChanged(); } }

        public string _Name;
        public string Name { get { return _Name; } set { _Name = value; OnPropertyChanged(); } }
        public string _Path;
        public string Path { get { return _Path; } set { _Path = value; OnPropertyChanged(); } }

        private BitmapSource _image = GlobalVariable.DefaultSmallImage;
        public BitmapSource Image { get { return _image; } set { _image = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj == null ? false : (obj as FileInfo)?.Path == Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }


    public class SqliteInfo : FileInfo
    {
        public Int64 _Count;
        public Int64 Count { get { return _Count; } set { _Count = value; OnPropertyChanged(); } }

        public override bool Equals(object obj)
        {
            return  obj==null?false:(obj as SqliteInfo)?.Path == Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
