using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    public static class ClipBoard
    {
        public static bool TrySetDataObject(object o, string token, bool showsuccess = true)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(o, false, 5, 200);
                if (showsuccess)
                    HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.HasCopy, token);

                return true;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, token);
                return false;
            }
        }

        //TODO
        //复制文件显示成功，却无法粘贴
        public static bool TrySetFileDropList(StringCollection filePaths, string token, bool showsuccess = true)
        {
            try
            {
                System.Windows.Clipboard.Clear();
                System.Windows.Clipboard.SetFileDropList(filePaths);
                if (showsuccess)
                    HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.HasCopy, token);
                return true;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, token);
                return false;
            }
        }

    }
}
