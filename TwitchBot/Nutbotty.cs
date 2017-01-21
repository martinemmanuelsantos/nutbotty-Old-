﻿using System;
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
    class Nutbotty
    {

        #region Attributes
        // Attributes for the Twitch bot
        // BOTNAME = Twitch username of your bot
        // OAUTH_TOKEN = Oauth token of your Twitch bot user
        public const string BOTNAME = "nutbotty";
        const string OAUTH_TOKEN = "oauth:5f2nzeaav3vby8fqvgfowcd3cflvd4";
        const int RECONNECT_TIME = 5;
        const string IRC_SERVER = "irc.chat.twitch.tv";
        const string WHISPER_SERVER = "irc.chat.twitch.tv";
        const int PORT = 6667;
        public static List<Channel> channels;
        #endregion

        #region TwitchChatBot Entry Point
        static void Main(string[] args)
        {
            // Set up the chat server and whisper server, using the authentication fields
            // TwitchChatConnection oldServerConnection = new TwitchChatConnection(new IrcClient(IRC_SERVER, PORT, RECONNECT_TIME, BOTNAME, OAUTH_TOKEN), false);
            TwitchChatConnection ircServerConnection = new TwitchChatConnection(new IrcClient(IRC_SERVER, PORT, RECONNECT_TIME, BOTNAME, OAUTH_TOKEN), false);
            TwitchChatConnection whisperConnection = new TwitchChatConnection(new IrcClient(WHISPER_SERVER, PORT, RECONNECT_TIME, BOTNAME, OAUTH_TOKEN), true);

            // Create a list of channels from all the rows in the CHANNELS table, as well as the bot itself,
            // then create a connection to the chat server and whisper connection
            channels = new List<Channel>();
            channels.Add(new Channel(BOTNAME));
            channels.AddRange(Database.GetChannelList());
            foreach (Channel channel in channels)
            {
                // new TwitchChatRoom(oldServerConnection, whisperConnection, channel);
                new TwitchChatRoom(ircServerConnection, ircServerConnection, channel);
            }

            // Start the pulling in data from the chat server and whisper server streams
            // (if you want to added a second, third, fourth etc. bot, double up on these threads)
            // new Thread(new ThreadStart(oldServerConnection.Run)).Start();
            new Thread(new ThreadStart(ircServerConnection.Run)).Start();
            new Thread(new ThreadStart(whisperConnection.Run)).Start();

        } 
        #endregion

    }
}
