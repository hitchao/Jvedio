using SuperUtils.WPF.VieModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity.Common
{
    public class TabItemEx : ViewModelBase
    {

        public TabItemEx(string name, bool selected = false, bool pinned = false)
        {
            Name = name;
            Selected = selected;
            Pinned = pinned;
            UUID = System.Guid.NewGuid().ToString();
        }


        private string _UUID;
        public string UUID {
            get { return _UUID; }
            private set { _UUID = value; RaisePropertyChanged(); }
        }

        private string _Name;
        public string Name {
            get { return _Name; }
            set { _Name = value; RaisePropertyChanged(); }
        }

        private bool _Selected;
        public bool Selected {
            get { return _Selected; }
            set { _Selected = value; RaisePropertyChanged(); }
        }

        private bool _Pinned;
        public bool Pinned {
            get { return _Pinned; }
            set { _Pinned = value; RaisePropertyChanged(); }
        }
    }
}
