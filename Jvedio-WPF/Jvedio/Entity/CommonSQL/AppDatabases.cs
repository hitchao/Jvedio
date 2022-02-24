using DynamicData.Annotations;
using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
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

    // todo 修改 AppDatabases 对应的增删改查
    [Table(tableName: "app_databases")]
    public class AppDatabase : Serilizable, INotifyPropertyChanged
    {

        public AppDatabase()
        {
            Image = new BitmapImage(new Uri("/Resources/Picture/datalist_video.png", UriKind.Relative));
        }


        // 默认 IdType.AUTO
        [TableId(IdType.AUTO)]
        public long DBId { get; set; }
        public string _Name;
        public string Name { get { return _Name; } set { _Name = value; OnPropertyChanged(); } }
        public string _Path;
        public string Path { get { return _Path; } set { _Path = value; OnPropertyChanged(); } }
        public string _ImagePath = "";
        public string ImagePath
        {
            get { return _ImagePath; }
            set
            {
                _ImagePath = value;
                if (File.Exists(_ImagePath)) Image = ImageProcess.BitmapImageFromFile(_ImagePath);
                OnPropertyChanged();
            }
        }

        private BitmapSource _image;

        [TableField(exist: false)]
        public BitmapSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }


        public long? Size { get; set; }
        public long? Count { get; set; }
        public DataType DataType { get; set; }

        public int? ViewCount { get; set; }
        public string _CreateDate;
        public string CreateDate { get { return _CreateDate; } set { _CreateDate = value; OnPropertyChanged(); } }
        public string UpdateDate { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AppDatabase o = obj as AppDatabase;
            if (o == null) return false;
            return o.DataType == this.DataType && o.Name == this.Name && o.Path == this.Path;
        }

        public override int GetHashCode()
        {
            return this.DataType.GetHashCode() + this.Name.GetHashCode() + this.Path.GetHashCode();
        }
    }
}
