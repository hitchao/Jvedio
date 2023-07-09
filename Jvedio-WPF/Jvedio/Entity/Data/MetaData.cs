
using Jvedio.Core.Enums;
using Jvedio.Entity.CommonSQL;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    [Table(tableName: "metadata")]
    public class MetaData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        [TableId(IdType.AUTO)]
        public long DataID { get; set; }

        public long DBId { get; set; }

        public string Title { get; set; }

        private long _Size;

        public long Size {
            get { return _Size; }

            set {
                _Size = value;
                RaisePropertyChanged();
            }
        }

        private string _Path;

        public string Path {
            get { return _Path; }

            set {
                _Path = value;

                RaisePropertyChanged();
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

        public string Genre {
            get { return _Genre; }

            set {
                _Genre = value;
                GenreList = new ObservableCollection<ObservableString>();
                if (!string.IsNullOrEmpty(value))
                    foreach (var item in value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries))
                        GenreList.Add(new ObservableString(item));

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ObservableString> _GenreList;
        [TableField(exist: false)]
        public ObservableCollection<ObservableString> GenreList {
            get { return _GenreList; }
            set {
                _GenreList = value;
                RaisePropertyChanged();
            }
        }

        private float _Grade;
        public float Grade {
            get { return _Grade; }
            set {
                _Grade = value;
                RaisePropertyChanged();
            }
        }

        private string _Label;

        [TableField(exist: false)]
        public string Label {
            get { return _Label; }

            set {
                _Label = value;
                LabelList = new ObservableCollection<ObservableString>();
                if (!string.IsNullOrEmpty(value)) {
                    foreach (var item in value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries))
                        LabelList.Add(new ObservableString(item));

                }
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<ObservableString> _LabelList;
        [TableField(exist: false)]
        public ObservableCollection<ObservableString> LabelList {
            get { return _LabelList; }
            set {
                _LabelList = value;
                RaisePropertyChanged();
            }
        }

        public string ViewDate { get; set; }

        public string _FirstScanDate;

        public string FirstScanDate {
            get { return _FirstScanDate; }

            set {
                _FirstScanDate = value;
                RaisePropertyChanged();
            }
        }

        private string _LastScanDate;

        public string LastScanDate {
            get { return _LastScanDate; }

            set {
                _LastScanDate = value;
                RaisePropertyChanged();
            }
        }

        public string CreateDate { get; set; }

        public string UpdateDate { get; set; }
        public int PathExist { get; set; }

        private BitmapSource _ViewImage;

        [TableField(exist: false)]
        public BitmapSource ViewImage {
            get { return _ViewImage; }

            set {
                _ViewImage = value;
                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public ObservableCollection<TagStamp> TagStamp { get; set; }

        [TableField(exist: false)]
        public string TagIDs { get; set; }

        [TableField(exist: false)]
        public bool HasVideo { get; set; }

        [TableField(exist: false)]
        public List<string> AttachedVideos { get; set; }

        [TableField(exist: false)]
        public long Count { get; set; }

        public static BitmapImage DefaultSmallImage { get; set; }

        public static BitmapImage DefaultBigImage { get; set; }

        public static BitmapImage DefaultActorImage { get; set; }

        static MetaData()
        {
            DefaultSmallImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_S.png", UriKind.Relative));
            DefaultBigImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            DefaultActorImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            MetaData metaData = obj as MetaData;
            return metaData != null && metaData.DataID == this.DataID;
        }

        public override int GetHashCode()
        {
            return this.DataID.GetHashCode();
        }
    }
}
