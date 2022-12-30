
using SuperUtils.IO;
using SuperUtils.WPF.VieModel;
using System;

namespace Jvedio.Core.Command
{
    public static class File
    {
        public static RelayCommand<string> OpenBaseDir { get; set; }

        public static RelayCommand<string> OpenDir { get; set; }

        public static RelayCommand<string> OpenSelectFile { get; set; }

        static File()
        {
            OpenBaseDir = new RelayCommand<string>(dir => FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory));
            OpenDir = new RelayCommand<string>(dir => FileHelper.TryOpenPath(dir));
            OpenSelectFile = new RelayCommand<string>(path => FileHelper.TryOpenSelectPath(path));
        }
    }
}
