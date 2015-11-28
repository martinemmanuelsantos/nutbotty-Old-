using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Events
{
    class TwitchChatPing : TwitchChatEvent
    {
        public TwitchChatPing()
        {

        }

        public override string ToString()
        {
            return "Sent PONG";
        }
    }
}
