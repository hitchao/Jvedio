using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.pojo
{

    public class MyListItem : INotifyPropertyChanged
    {
        private long number = 0;
        private string name = "";
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public long Number
        {
            get
            {
                return number;
            }

            set
            {
                number = value;
                OnPropertyChanged();
            }

        }

        public MyListItem(string name, long number)
        {
            this.Name = name;
            this.Number = number;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
