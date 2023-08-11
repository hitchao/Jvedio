using Jvedio.Mapper;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.WPF.VisualTools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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


        private static List<long> SystemTags =
            new List<long>() { TAG_ID_HD, TAG_ID_TRANSLATED, TAG_ID_NEW_ADD };

        public const long TAG_ID_NEW_ADD = 10000;
        public const long TAG_ID_HD = 1;
        public const long TAG_ID_TRANSLATED = 2;



        /// <summary>
        /// 标签戳，全局缓存，避免每次都查询
        /// </summary>
        public static List<TagStamp> TagStamps { get; set; }

        public static void Init()
        {
            TagStamps = MapperManager.tagStampMapper.GetAllTagStamp();
        }


        [TableId(IdType.AUTO)]
        public long TagID { get; set; }

        public string TagName { get; set; }

        public string _Foreground;

        public string Foreground {
            get {
                return _Foreground;
            }

            set {
                _Foreground = value;
                if (!string.IsNullOrEmpty(value) && value.IndexOf(",") > 0)
                    ForegroundBrush = VisualHelper.RGBAToBrush(value.Split(','));
                RaisePropertyChanged();
            }
        }

        public string _Background;

        public string Background {
            get {
                return _Background;
            }

            set {
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
        public bool Selected {
            get {
                return _Selected;
            }

            set {
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
            if (obj == null)
                return false;
            TagStamp tagStamp = obj as TagStamp;
            return tagStamp != null && tagStamp.TagID == this.TagID;
        }

        public override int GetHashCode()
        {
            return TagID.GetHashCode();
        }


        public bool IsSystemTag()
        {
            return SystemTags.Contains(TagID);
        }

        public static ObservableCollection<TagStamp> InitTagStamp(List<TagStamp> beforeTagStamps = null)
        {
            ObservableCollection<TagStamp> result = new ObservableCollection<TagStamp>();
            List<Dictionary<string, object>> list = MapperManager.tagStampMapper.Select(TagStampMapper.GetTagSql());
            List<TagStamp> tagStamps = new List<TagStamp>();
            if (list != null && list.Count > 0) {
                tagStamps = MapperManager.tagStampMapper.ToEntity<TagStamp>(list, typeof(TagStamp).GetProperties(), false);
                if (beforeTagStamps != null && beforeTagStamps.Count > 0) {
                    foreach (var item in tagStamps) {
                        TagStamp tagStamp = beforeTagStamps.FirstOrDefault(arg => arg.TagID == item.TagID);
                        if (tagStamp != null)
                            item.Selected = tagStamp.Selected;
                    }
                }
            }

            result = new ObservableCollection<TagStamp>();

            // 先增加默认的：高清、中文
            foreach (TagStamp item in TagStamp.TagStamps) {
                TagStamp tagStamp = tagStamps.Where(arg => arg.TagID == item.TagID).FirstOrDefault();
                if (tagStamp != null)
                    result.Add(tagStamp);
                else {
                    // 无该标记
                    item.Count = 0;
                    result.Add(item);
                }
            }
            return result;
        }

    }
}
