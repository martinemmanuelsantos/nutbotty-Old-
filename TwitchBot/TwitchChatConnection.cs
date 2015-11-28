﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchBot.Events;

namespace TwitchBot
{
    class TwitchChatConnection
    {

        #region Member Variables
        private IrcClient ircClient;
        LinkedList<TwitchChatRoom> chatrooms = new LinkedList<TwitchChatRoom>();
        #endregion

        #region Constructors
        public TwitchChatConnection(IrcClient ircClient, bool isWhisperConnection)
        {
            this.ircClient = ircClient;
            this.ircClient.connect();
            if (isWhisperConnection)
            {
                ircClient.sendIrcString("CAP REQ :twitch.tv/commands");
            }
        } 
        #endregion
        
        #region Getters
        public IrcClient IrcClient
        {
            get
            {
                return ircClient;
            }

        }
        #endregion

        #region Methods
        internal void join(TwitchChatRoom channel)
        {
            ircClient.joinChannel(channel.Channel.ChannelName);
            chatrooms.AddLast(channel);
        }
        
        /// <summary>
        /// Continually check for messages from the IRC input stream, and respond according to the event typpe
        /// </summary>
        internal void Run()
        {
            while (true)
            {
                TwitchChatEvent chatEvent = GetNextEvent();
                RespondToEvent(chatEvent);
            }            
        }


        /// <summary>
        /// Determine the type of event given a full IRC string. Possible chat events include "PING", "WHISPER", "PRIVMSG", host events, otherwise unknown
        /// </summary>
        /// <param name="ircString">Complete IRC string</param>
        /// <returns></returns>
        private TwitchChatEvent GetNextEvent()
        {
            // Retrieve the whole IRC message. If you are disconnected, repeatedly request to connect to the server indefinitely
            string ircString = null;
            while (ircString == null)
            {
                try
                {
                    ircString = this.ircClient.readIrcString();
                }
                catch (Exception e)
                {
                    Log.Message("You have been disconnected: " + e, true);
                    this.ircClient.connect();
                }
            }

            return TwitchChatEvent.parseEvent(ircString);
        }

        #region Bot Logic
        /// <summary>
        /// Logic for responding to chat events
        /// </summary>
        /// <param name="chatEvent"></param>
        private void RespondToEvent(TwitchChatEvent chatEvent)
        {

            // Logic for responding to a "PING" event
            if (chatEvent.GetType().Equals(typeof(TwitchChatPing)))
            {
                ircClient.sendIrcString("PONG");
                Log.Message(chatEvent.ToString(), false);
            }

            // Logic for responding to a "PRIVMSG" event
            else if (chatEvent.GetType().Equals(typeof(TwitchChatMessage)))
            {
                // Check all the chat roomms currently connected, then respond to the correct channel
                TwitchChatMessage chatMessage = (TwitchChatMessage)chatEvent;
                foreach (TwitchChatRoom chatroom in chatrooms)
                {
                    if (chatMessage.Channel.Equals(chatroom.Channel.ChannelName))
                    {
                        chatroom.RespondToEvent(chatEvent);
                        break;
                    }
                }
            }

            // Logic for responding to a "WHISPER" event
            else if (chatEvent.GetType().Equals(typeof(TwitchChatWhisper)))
            {
                Log.Message(chatEvent.ToString(), true);
            }

            // Logic for responding to a "HOST" event
            else if (chatEvent.GetType().Equals(typeof(TwitchChatHost)))
            {
                TwitchChatHost hostData = (TwitchChatHost)chatEvent;
                ircClient.sendChatMessage(hostData.Hostee, chatEvent.ToString());
                Log.Message(hostData.ToString(), true);
            }

            // Logic for responding to an unknown event
            else 
            {
                //Log.Message(chatEvent.ToString(), false);
            }
        }
        #endregion

        #endregion
    }
}
