using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{
    /// <summary>
    /// 详情页面的 Jvedio 影片，多了预览图、类别、演员、标签
    /// </summary>
    ///
    [Obsolete]
    public class DetailMovie : Movie
    {

        #region "属性"

        public List<string> genrelist { get; set; }

        public List<Actress> actorlist { get; set; }

        private List<string> _labellist;

        public List<string> labellist {
            get {
                return _labellist;
            }

            set {
                _labellist = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<BitmapSource> extraimagelist { get; set; }

        public ObservableCollection<string> extraimagePath { get; set; }


        #endregion

        public DetailMovie() : base()
        {
            genrelist = new List<string>();
            actorlist = new List<Actress>();
            labellist = new List<string>();
            extraimagelist = new ObservableCollection<BitmapSource>();
            extraimagePath = new ObservableCollection<string>();
        }

        public override void Dispose()
        {
            genrelist.Clear();
            actorlist.Clear();
            labellist.Clear();
            extraimagelist.Clear();
            extraimagePath.Clear();
            base.Dispose();
        }

    }
}
