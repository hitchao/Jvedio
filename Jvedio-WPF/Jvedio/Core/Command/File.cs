using GalaSoft.MvvmLight.Command;
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
            OpenBaseDir = new RelayCommand(OpenBaseDirectory);
            OpenDir = new RelayCommand<string>(t => OpenDirectory(t));
            OpenSelectFile = new RelayCommand<string>(t => OpenSelect(t));
        }

        static void OpenBaseDirectory()
        {
            FileHelper.TryOpenPath(AppDomain.CurrentDomain.BaseDirectory);
        }

        static void OpenDirectory(string dir)
        {
            FileHelper.TryOpenPath(dir);
        }

        static void OpenSelect(string path)
        {
            FileHelper.TryOpenSelectPath(path);
        }
    }
}
