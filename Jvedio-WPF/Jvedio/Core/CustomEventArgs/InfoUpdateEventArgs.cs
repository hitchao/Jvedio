using Jvedio.Core.Enums;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomEventArgs
{
    public class InfoUpdateEventArgs : EventArgs
    {
        public bool Success = false;
        public Movie Movie;
        public double progress = 0;
        public DownLoadState state;
    }

}
