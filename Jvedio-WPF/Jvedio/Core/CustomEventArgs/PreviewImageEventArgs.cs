using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomEventArgs
{
    public class PreviewImageEventArgs : EventArgs
    {
        public string Path { get; set; }
        public byte[] FileByte { get; set; }

        public PreviewImageEventArgs(string path, byte[] fileByte)
        {
            Path = path;
            FileByte = fileByte;
        }
    }
}
