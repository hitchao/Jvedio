using System;

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
            if (string.IsNullOrEmpty(Path))
                Path = string.Empty;
        }
    }
}
