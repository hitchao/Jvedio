
using Jvedio.Core.Enums;
using Jvedio.Entity.CommonSQL;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        #region "属性"



        private long _DataID;

        [TableId(IdType.AUTO)]
        public long DataID {
            get { return _DataID; }

            set {
                _DataID = value;
                RaisePropertyChanged();
            }
        }

        private long _DBId;

        public long DBId {
            get { return _DBId; }

            set {
                _DBId = value;
                RaisePropertyChanged();
            }
        }

        private string _Title;
        public string Title {
            get { return _Title; }

            set {
                _Title = value;
                RaisePropertyChanged();
            }
        }

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

        private string _Hash;

        public string Hash {
            get { return _Hash; }

            set {
                _Hash = value;
                RaisePropertyChanged();
            }
        }


        private string _Country;

        public string Country {
            get { return _Country; }

            set {
                _Country = value;
                RaisePropertyChanged();
            }
        }

        private string _ReleaseDate;

        public string ReleaseDate {
            get { return _ReleaseDate; }

            set {
                _ReleaseDate = value;
                RaisePropertyChanged();
            }
        }

        private int _ReleaseYear;
        public int ReleaseYear {
            get { return _ReleaseYear; }

            set {
                _ReleaseYear = value;
                RaisePropertyChanged();
            }
        }

        private int _ViewCount;
        public int ViewCount {
            get { return _ViewCount; }

            set {
                _ViewCount = value;
                RaisePropertyChanged();
            }
        }

        private DataType _DataType;

        public DataType DataType {
            get { return _DataType; }

            set {
                _DataType = value;
                RaisePropertyChanged();
            }
        }

        private float _Rating;

        public float Rating {
            get { return _Rating; }

            set {
                _Rating = value;
                RaisePropertyChanged();
            }
        }

        private int _RatingCount;

        public int RatingCount {
            get { return _RatingCount; }

            set {
                _RatingCount = value;
                RaisePropertyChanged();
            }
        }

        private int _FavoriteCount;

        public int FavoriteCount {
            get { return _FavoriteCount; }

            set {
                _FavoriteCount = value;
                RaisePropertyChanged();
            }
        }


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

        private string _ViewDate;


        public string ViewDate {
            get { return _ViewDate; }

            set {
                _ViewDate = value;
                RaisePropertyChanged();
            }
        }


        private string _FirstScanDate;

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

        private string _CreateDate;

        public string CreateDate {
            get { return _CreateDate; }

            set {
                _CreateDate = value;
                RaisePropertyChanged();
            }
        }

        private string _UpdateDate;

        public string UpdateDate {
            get { return _UpdateDate; }

            set {
                _UpdateDate = value;
                RaisePropertyChanged();
            }
        }

        private int _PathExist;

        public int PathExist {
            get { return _PathExist; }

            set {
                _PathExist = value;
                RaisePropertyChanged();
            }
        }


        private BitmapSource _ViewImage;

        [TableField(exist: false)]
        public BitmapSource ViewImage {
            get { return _ViewImage; }

            set {
                _ViewImage = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<TagStamp> _TagStamp;

        [TableField(exist: false)]
        public ObservableCollection<TagStamp> TagStamp {
            get { return _TagStamp; }
            set {
                _TagStamp = value;
                RaisePropertyChanged();

            }
        }


        private string _TagIDs;

        [TableField(exist: false)]
        public string TagIDs {
            get { return _TagIDs; }
            set {

                _TagIDs = value;
                RaisePropertyChanged();
            }
        }


        private bool _HasVideo;
        [TableField(exist: false)]
        public bool HasVideo {
            get { return _HasVideo; }

            set {
                _HasVideo = value;
                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> AttachedVideos { get; set; }

        [TableField(exist: false)]
        public long Count { get; set; }

        public static BitmapImage DefaultSmallImage { get; set; }

        public static BitmapImage DefaultBigImage { get; set; }

        public static BitmapImage DefaultActorImage { get; set; }
        #endregion

        static MetaData()
        {
            DefaultSmallImage =
                new BitmapImage(new Uri("pack://application:,,,/Resources/Picture/NoPrinting_S.png", UriKind.RelativeOrAbsolute));
            DefaultBigImage =
                new BitmapImage(new Uri("pack://application:,,,/Resources/Picture/NoPrinting_B.png", UriKind.RelativeOrAbsolute));
            DefaultActorImage =
                new BitmapImage(new Uri("pack://application:,,,/Resources/Picture/NoPrinting_A.png", UriKind.RelativeOrAbsolute));
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
