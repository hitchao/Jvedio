using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SuperControls.Style.MessageCard;

namespace Jvedio.Entity
{
    public class Message
    {

        public Message(MessageCardType type, string text)
        {
            Type = type;
            Text = text;
        }


        public MessageCardType Type { get; set; }
        public string Text { get; set; }

    }
}
