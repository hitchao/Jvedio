using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.CustomEventArgs
{
    public class MessageCallBackEventArgs : System.EventArgs
    {
        public string Message = "";
        public MessageCallBackEventArgs(string message = "")
        {
            Message = message;
        }
    }
}
