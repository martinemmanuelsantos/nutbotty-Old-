using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Events
{
    class TwitchChatUnknownEvent : TwitchChatEvent
    {
        string message;

        public TwitchChatUnknownEvent(string ircString)
        {
            this.message = ircString;
        }

        public override string ToString()
        {
            return "Unknown Chat Event: " + message;
        }
    }
}
