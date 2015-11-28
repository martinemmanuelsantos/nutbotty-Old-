using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class Log
    {
        Log()
        {
        }

        public static void Message(string message, bool console)
        {
            Debug.WriteLine("[" + DateTime.Now + "] " + message);
            if (console)
            {
                Console.WriteLine("[" + DateTime.Now + "] " + message);
            }
        }
    }
}
