
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Core.Global;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    [Table(tableName: "app_databases")]
    public class AppDatabase : Serializable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public AppDatabase()
        {
            Image = new BitmapImage(new Uri("/Resources/Picture/datalist_video.png", UriKind.Relative));
        }

        // 默认 IdType.AUTO
        [TableId(IdType.AUTO)]
        public long DBId { get; set; }

        public string _Name;

        public string Name {
            get { return _Name; }

            set {
                _Name = value;
                RaisePropertyChanged();
            }
        }

        public string _ImagePath = string.Empty;

        public string ImagePath {
            get { return _ImagePath; }

            set {
                _ImagePath = value;
                string actual_path = Path.Combine(PathManager.ProjectImagePath, ImagePath);
                if (File.Exists(actual_path))
                    Image = ImageHelper.BitmapImageFromFile(actual_path);
                RaisePropertyChanged();
            }
        }

        private BitmapSource _image;

        [TableField(exist: false)]
        public BitmapSource Image {
            get { return _image; }

            set {
                _image = value;
                RaisePropertyChanged();
            }
        }

        public long? Count { get; set; }

        public DataType DataType { get; set; }

        public int? ViewCount { get; set; }

        public string _CreateDate;

        public string CreateDate {
            get { return _CreateDate; }

            set {
                _CreateDate = value;
                RaisePropertyChanged();
            }
        }

        public string UpdateDate { get; set; }

        public int Hide { get; set; }

        public string ScanPath { get; set; }

        public string ExtraInfo { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            AppDatabase o = obj as AppDatabase;
            if (o == null)
                return false;
            return o.DataType == this.DataType && o.Name == this.Name;
        }

        public override int GetHashCode()
        {
            return this.DataType.GetHashCode() + this.Name.GetHashCode();
        }

        static Dictionary<DataType, string> typeDict = new Dictionary<DataType, string>()
        {
            { DataType.Video, "metadata_video" },
            { DataType.Comics, "metadata_comic" },
            { DataType.Game, "metadata_game" },
            { DataType.Picture, "metadata_picture" },
        };

        public void deleteByID(long id)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("begin;");
            builder.Append($"delete from {typeDict[DataType]} where DataID in( SELECT DataID FROM metadata where DBId ='{id}');");
            builder.Append($"delete from metadata_to_tagstamp where DataID in( SELECT DataID FROM metadata where DBId ='{id}');");
            builder.Append($"delete from metadata_to_actor where DataID in( SELECT DataID FROM metadata where DBId ='{id}');");
            builder.Append($"delete from metadata_to_label where DataID in( SELECT DataID FROM metadata where DBId ='{id}');");
            builder.Append($"delete from metadata_to_translation where DataID in( SELECT DataID FROM metadata where DBId ='{id}');");
            builder.Append($"delete from metadata where DBId ='{id}';");
            builder.Append("commit;");
            try {
                MapperManager.appDatabaseMapper.ExecuteNonQuery(builder.ToString());
                MapperManager.appDatabaseMapper.DeleteById(id);
            } catch (Exception ex) {
                throw new SqlException(builder.ToString(), ex);
            }
        }
    }
}
