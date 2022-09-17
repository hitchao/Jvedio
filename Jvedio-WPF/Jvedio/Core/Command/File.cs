using GalaSoft.MvvmLight.Command;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Jvedio.Core.Command
{
    public static class File
    {
        public static RelayCommand OpenBaseDir { get; set; }
        public static RelayCommand<string> OpenDir { get; set; }
        public static RelayCommand<string> OpenSelectFile { get; set; }

        static File()
        {
            OpenBaseDir = new RelayCommand(() => FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory));
            OpenDir = new RelayCommand<string>(dir => FileHelper.TryOpenPath(dir));
            OpenSelectFile = new RelayCommand<string>(path => FileHelper.TryOpenSelectPath(path));
        }
    }
}
