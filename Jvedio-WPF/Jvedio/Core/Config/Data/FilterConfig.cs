using Jvedio.Core.Config.Base;

namespace Jvedio.Core.Config.Data
{
    public class FilterConfig : AbstractConfig
    {
        private FilterConfig() : base("FilterConfig")
        {
            ExpandTag = true;

        }

        private bool _ExpandTag;
        public bool ExpandTag {
            get { return _ExpandTag; }
            set {
                _ExpandTag = value;
                RaisePropertyChanged();
            }
        }
        private bool _ExpandCommon;
        public bool ExpandCommon {
            get { return _ExpandCommon; }
            set {
                _ExpandCommon = value;
                RaisePropertyChanged();
            }
        }
        private bool _ExpandGenre;
        public bool ExpandGenre {
            get { return _ExpandGenre; }
            set {
                _ExpandGenre = value;
                RaisePropertyChanged();
            }
        }
        private bool _ExpandSeries;
        public bool ExpandSeries {
            get { return _ExpandSeries; }
            set {
                _ExpandSeries = value;
                RaisePropertyChanged();
            }
        }
        private bool _ExpandDirector;
        public bool ExpandDirector {
            get { return _ExpandDirector; }
            set {
                _ExpandDirector = value;
                RaisePropertyChanged();
            }
        }
        private bool _ExpandStudio;
        public bool ExpandStudio {
            get { return _ExpandStudio; }
            set {
                _ExpandStudio = value;
                RaisePropertyChanged();
            }
        }

        private static FilterConfig _instance = null;

        public static FilterConfig CreateInstance()
        {
            if (_instance == null)
                _instance = new FilterConfig();

            return _instance;
        }

    }
}
