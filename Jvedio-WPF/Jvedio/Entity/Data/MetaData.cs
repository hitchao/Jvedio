﻿using DynamicData.Annotations;
using SuperUtils.Framework.ORM.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Mapper.BaseMapper;
using Jvedio.Entity.CommonSQL;
using SuperUtils.Common;
using SuperUtils.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;

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
                    GenreList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                    LabelList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> LabelList { get; set; }

        public string ViewDate { get; set; }
        public string _FirstScanDate;
        public string FirstScanDate
        {
            get { return _FirstScanDate; }
            set
            {
                _FirstScanDate = value;
                OnPropertyChanged();
            }
        }


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

        private BitmapSource _ViewImage;
        [TableField(exist: false)]
        public BitmapSource ViewImage { get { return _ViewImage; } set { _ViewImage = value; OnPropertyChanged(); } }

        [TableField(exist: false)]
        public string TagIDs { get; set; }
        [TableField(exist: false)]
        public bool HasVideo { get; set; }
        [TableField(exist: false)]
        public List<string> AttachedVideos { get; set; }
        [TableField(exist: false)]
        public ObservableCollection<TagStamp> TagStamp { get; set; }
        [TableField(exist: false)]
        public long Count { get; set; }


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

        public static SelectWrapper<MetaData> InitWrapper(DataType dataType)
        {
            SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("metadata.DBId", ConfigManager.Main.CurrentDBId)
                .Eq("metadata.DataType", (int)dataType);
            return wrapper;
        }

        public static void SetImage(ref MetaData data, string imgPath)
        {
            if (data == null) return;
            BitmapImage image = ImageHelper.ReadImageFromFile(imgPath);
            if (image == null) image = GlobalVariable.DefaultBigImage;
            data.ViewImage = image;
        }
        public static void SetImage(ref Video video, string imgPath)
        {
            if (video == null) return;
            BitmapImage image = ImageHelper.ReadImageFromFile(imgPath);
            if (image == null) image = GlobalVariable.DefaultBigImage;
            video.ViewImage = image;
        }
        public static void handleEmpty(ref MetaData data)
        {
            if (data == null) return;
            if (Properties.Settings.Default.ShowFileNameIfTitleEmpty
              && !string.IsNullOrEmpty(data.Path) && string.IsNullOrEmpty(data.Title))
                data.Title = System.IO.Path.GetFileName(data.Path);
            if (Properties.Settings.Default.ShowCreateDateIfReleaseDateEmpty
                && !string.IsNullOrEmpty(data.LastScanDate) && string.IsNullOrEmpty(data.ReleaseDate))
                data.ReleaseDate = DateHelper.ToLocalDate(data.LastScanDate);
        }

        public static void setTagStamps(ref MetaData data)
        {
            if (data == null || string.IsNullOrEmpty(data.TagIDs)) return;
            List<long> list = data.TagIDs.Split(',').Select(arg => long.Parse(arg)).ToList();

            if (list != null && list.Count > 0)
            {
                data.TagStamp = new ObservableCollection<TagStamp>();
                foreach (var item in GlobalVariable.TagStamps.Where(arg => list.Contains(arg.TagID)).ToList())
                    data.TagStamp.Add(item);
            }

        }
    }
}
