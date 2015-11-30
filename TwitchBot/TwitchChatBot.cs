using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchBot.Entities;
using TwitchBot.Events;

namespace TwitchBot
{
    class TwitchChatBot
    {

        #region Attributes
        // Attributes for the Twitch bot
        // BOTNAME = Twitch username of your bot
        // OAUTH_TOKEN = Oauth token of your Twitch bot user
        public const string BOTNAME = "nutbotty";
        const string OAUTH_TOKEN = "oauth:5f2nzeaav3vby8fqvgfowcd3cflvd4";
        const int RECONNECT_TIME = 5;
        const string IRC_SERVER = "irc.twitch.tv";
        const string WHISPER_SERVER = "192.16.64.212";
        const int PORT = 6667;
        #endregion

        #region TwitchChatBot Entry Point
        static void Main(string[] args)
        {
            //List<TwitchChannel> channels = TwitchChatBotDB.GetChannels();
            //foreach (TwitchChannel channel in channels)
            //{
            //    Debug.WriteLine(channel);
            //}
            TwitchChatConnection chatConnection = new TwitchChatConnection(new IrcClient(IRC_SERVER, PORT, RECONNECT_TIME, BOTNAME, OAUTH_TOKEN), false);
            TwitchChatConnection whisperConnection = new TwitchChatConnection(new IrcClient(WHISPER_SERVER, PORT, RECONNECT_TIME, BOTNAME, OAUTH_TOKEN), true);

            List<TwitchChannel> channels = new List<TwitchChannel>();
            channels.Add(new TwitchChannel(BOTNAME));
            channels.AddRange(TwitchChatBotDB.GetChannels());

            foreach (TwitchChannel channel in channels)
            {
                new TwitchChatRoom(chatConnection, whisperConnection, channel);
            }

            new Thread(new ThreadStart(chatConnection.Run)).Start();
            new Thread(new ThreadStart(whisperConnection.Run)).Start();

        } 
        #endregion

    }
}
