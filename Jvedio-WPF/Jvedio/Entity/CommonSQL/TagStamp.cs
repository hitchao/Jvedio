
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.WPF.VisualTools;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Jvedio.Entity.CommonSQL
{
    [Table(tableName: "common_tagstamp")]
    public class TagStamp : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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

        public override int GetHashCode()
        {
            return TagID.GetHashCode();
        }

    }
}
