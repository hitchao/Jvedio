namespace Jvedio.Core.CustomEventArgs
{
    public class MessageCallBackEventArgs : System.EventArgs
    {
        public string Message = "";

        public MessageCallBackEventArgs(string message = "")
        {

            Message = message;
            if (string.IsNullOrEmpty(Message)) Message = "";
        }
    }
}
