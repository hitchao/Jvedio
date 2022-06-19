using Jvedio.Logs;
using System;
using System.Collections.Specialized;

namespace Jvedio.Utils.IO
{
    public static class ClipBoard
    {
        public static bool TrySetDataObject(object o)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(o, false, 5, 200);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        //TODO
        //复制文件显示成功，却无法粘贴
        public static bool TrySetFileDropList(StringCollection filePaths, Action<string> callBack = null)
        {
            try
            {
                System.Windows.Clipboard.Clear();
                System.Windows.Clipboard.SetFileDropList(filePaths);
                return true;
            }
            catch (Exception ex)
            {
                callBack.Invoke(ex.Message);
                return false;
            }
        }

    }
}
