using DynamicData.Annotations;
using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{

    [Table(tableName: "metadata")]
    public class MetaData : INotifyPropertyChanged
    {

        [TableId(IdType.AUTO)]
        public long DataID { get; set; }
        public long DBId { get; set; }
        public string Title { get; set; }
        private long _Size;
        public long Size
        {
            get { return _Size; }
            set
            {
                _Size = value;
                OnPropertyChanged();
            }
        }
        private string _Path;
        public string Path
        {
            get { return _Path; }
            set
            {
                _Path = value;
                OnPropertyChanged();
            }
        }
        public string Hash { get; set; }
        public string Country { get; set; }
        public string ReleaseDate { get; set; }
        public int ReleaseYear { get; set; }
        public int ViewCount { get; set; }
        public DataType DataType { get; set; }
        public float Rating { get; set; }
        public int RatingCount { get; set; }
        public int FavoriteCount { get; set; }
        private string _Genre;
        public string Genre
        {
            get { return _Genre; }
            set
            {
                _Genre = value;
                GenreList = new List<string>();
                if (!string.IsNullOrEmpty(value))
                {
                    GenreList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> GenreList { get; set; }

        public float Grade { get; set; }

        private string _Label;
        [TableField(exist: false)]
        public string Label
        {
            get { return _Label; }
            set
            {
                _Label = value;
                LabelList = new List<string>();
                if (!string.IsNullOrEmpty(value))
                {
                    LabelList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> LabelList { get; set; }

        public string ViewDate { get; set; }
        public string FirstScanDate { get; set; }


        private string _LastScanDate;
        public string LastScanDate
        {
            get { return _LastScanDate; }
            set
            {
                _LastScanDate = value;
                OnPropertyChanged();
            }
        }
        public string CreateDate { get; set; }
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
            MetaData metaData = obj as MetaData;
            return metaData != null && metaData.DataID == this.DataID;
        }

        public override int GetHashCode()
        {
            return this.DataID.GetHashCode();
        }
    }
}
