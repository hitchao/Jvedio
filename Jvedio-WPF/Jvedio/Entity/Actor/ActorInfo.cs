using DynamicData.Annotations;
using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    [Table(tableName: "actor_info")]
    public class ActorInfo : INotifyPropertyChanged
    {
        [TableId(IdType.AUTO)]
        public long ActorID { get; set; }
        public string ActorName { get; set; }
        public string Country { get; set; }
        public string Nation { get; set; }
        public string BirthPlace { get; set; }
        public string Birthday { get; set; }
        public int Age { get; set; }
        public string BloodType { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public Gender Gender { get; set; }
        public string Hobby { get; set; }
        public char Cup { get; set; }
        public int Chest { get; set; }
        public int Waist { get; set; }
        public int Hipline { get; set; }
        public string WebType { get; set; }
        public string WebUrl { get; set; }
        public float Grade { get; set; }
        public string SmallImagePath { get; set; }
        public string PreviewImagePath { get; set; }
        public string ExtraInfo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }

        [TableField(exist: false)]
        public long ImageID { get; set; }


        /// <summary>
        /// 出演的作品的数量
        /// </summary>
        [TableField(exist: false)]
        public long Count { get; set; }

        private BitmapSource _smallimage;

        [TableField(exist: false)]
        public BitmapSource SmallImage { get { return _smallimage; } set { _smallimage = value; OnPropertyChanged(); } }




        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            ActorInfo actorInfo = obj as ActorInfo;
            if (actorInfo == null) return false;
            return this.ActorID == actorInfo.ActorID;
        }

        public override int GetHashCode()
        {
            return this.ActorID.GetHashCode();
        }

        public override string ToString()
        {
            return ClassUtils.toString(this);
        }

        public static void SetImage(ref ActorInfo actorInfo)
        {
            //加载图片
            string smallImagePath = Video.parseImagePath(actorInfo.SmallImagePath);
            BitmapImage smallimage = ImageProcess.ReadImageFromFile(smallImagePath);
            if (smallimage == null) smallimage = GlobalVariable.DefaultActorImage;
            actorInfo.SmallImage = smallimage;
        }
    }
}
