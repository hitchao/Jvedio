namespace Jvedio.Core.CustomEventArgs
{
    public class MessageCallBackEventArgs : System.EventArgs
    {
        public string Message = string.Empty;

        public MessageCallBackEventArgs(string message = "")
        {
            Message = message;
            if (string.IsNullOrEmpty(Message)) Message = string.Empty;
        }
    }
}
