using SuperUtils.Time;
using static SuperControls.Style.MessageCard;

namespace Jvedio.Entity
{
    public class Message
    {

        public string Date { get; set; }

        public MessageCardType Type { get; set; }

        public string Text { get; set; }

        public Message(MessageCardType type, string text)
        {
            Type = type;
            Text = text;
            Date = DateHelper.Now();
        }

    }
}
