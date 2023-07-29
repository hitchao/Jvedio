using Jvedio.Core.Config.Base;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Main : AbstractConfig
    {
        public const int MAX_IMAGE_WIDTH = 800;
        private Main() : base($"WindowConfig.Main")
        {
            Width = SystemParameters.WorkArea.Width * 0.8;
            Height = SystemParameters.WorkArea.Height * 0.8;
            SideGridWidth = 200;
            FirstRun = true;
            ShowSearchHistory = true;
            SideDefaultExpanded = true;
            SideTagStampExpanded = true;

            DisplayFunBar = true;
            DisplayNavigation = false;
            DisplayPage = true;
            DisplaySearchBox = true;
            DisplayStatusBar = true;
        }

        private static Main _instance = null;

        public static Main CreateInstance()
        {
            if (_instance == null)
                _instance = new Main();

            return _instance;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public long WindowState { get; set; }

        public long CurrentDBId { get; set; }

        public long SearchSelectedIndex { get; set; }

        public long ClassifySelectedIndex { get; set; }

        public double SideGridWidth { get; set; }

        public bool FirstRun { get; set; }

        public bool ShowSearchHistory { get; set; }
        public bool SideDefaultExpanded { get; set; }
        public bool SideTagStampExpanded { get; set; }


        public string LatestNotice { get; set; }

        public bool _DisplayFunBar;
        public bool DisplayFunBar {
            get { return _DisplayFunBar; }
            set {
                _DisplayFunBar = value;
                RaisePropertyChanged();
            }
        }
        public bool _DisplayNavigation;
        public bool DisplayNavigation {
            get { return _DisplayNavigation; }
            set {
                _DisplayNavigation = value;
                RaisePropertyChanged();
            }
        }
        public bool _DisplayPage;
        public bool DisplayPage {
            get { return _DisplayPage; }
            set {
                _DisplayPage = value;
                RaisePropertyChanged();
            }
        }
        public bool _DisplaySearchBox;
        public bool DisplaySearchBox {
            get { return _DisplaySearchBox; }
            set {
                _DisplaySearchBox = value;
                RaisePropertyChanged();
            }
        }
        public bool _DisplayStatusBar;
        public bool DisplayStatusBar {
            get { return _DisplayStatusBar; }
            set {
                _DisplayStatusBar = value;
                RaisePropertyChanged();
            }
        }
        public bool _PaginationCombobox;
        public bool PaginationCombobox {
            get { return _PaginationCombobox; }
            set {
                _PaginationCombobox = value;
                RaisePropertyChanged();
            }
        }
    }
}
