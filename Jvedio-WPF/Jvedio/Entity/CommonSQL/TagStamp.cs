using DynamicData.Annotations;
using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Utils.Visual;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Jvedio.Entity.CommonSQL
{

    [Table(tableName: "common_tagstamp")]
    public class TagStamp : INotifyPropertyChanged
    {
        [TableId(IdType.AUTO)]
        public long TagID { get; set; }
        public string TagName { get; set; }
        public string _Foreground;
        public string Foreground
        {
            get
            {
                return _Foreground;
            }
            set
            {
                _Foreground = value;
                if (!string.IsNullOrEmpty(value) && value.IndexOf(",") > 0)
                    ForegroundBrush = VisualHelper.RGBAToBrush(value.Split(','));
                OnPropertyChanged();
            }
        }


        public string _Background;
        public string Background
        {
            get
            {
                return _Background;
            }
            set
            {
                _Background = value;
                if (!string.IsNullOrEmpty(value) && value.IndexOf(",") > 0)
                    BackgroundBrush = VisualHelper.RGBAToBrush(value.Split(','));
                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public long Count { get; set; }


        public bool _Selected = true;

        [TableField(exist: false)]
        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public SolidColorBrush ForegroundBrush { get; set; }
        [TableField(exist: false)]
        public SolidColorBrush BackgroundBrush { get; set; }
        public string ExtraInfo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            TagStamp tagStamp = obj as TagStamp;
            return tagStamp != null && tagStamp.TagID == this.TagID;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }



}
