using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Events;

namespace TwitchBot
{
    class TwitchChatRoom
    {

        #region Member Variables
        TwitchChatConnection chatConnection;
        TwitchChatConnection whisperConnection;
        TwitchChannel channel; 
        #endregion

        #region Constructors
        public TwitchChatRoom(TwitchChatConnection chatConnection, TwitchChatConnection whisperConnection, TwitchChannel channel)
        {
            this.chatConnection = chatConnection;
            this.whisperConnection = whisperConnection;
            this.channel = channel;

            chatConnection.join(this);
        } 
        #endregion

        #region Getters
        internal TwitchChatConnection ChatConnection
        {
            get
            {
                return chatConnection;
            }
        }

        internal TwitchChatConnection WhisperConnection
        {
            get
            {
                return whisperConnection;
            }
        }

        internal TwitchChannel Channel
        {
            get
            {
                return channel;
            }
        } 
        #endregion

        internal void RespondToEvent(TwitchChatEvent chatEvent)
        {
            #region Bot Logic
            // Logic for responding to a "PRIVMSG" event
            if (chatEvent.GetType().Equals(typeof(TwitchChatMessage)))
            {
                // Create a reference for the chat data
                TwitchChatMessage chatData = (TwitchChatMessage)(chatEvent);

                if (chatData.ChatMessage.Equals("!discord")) {
                    if (chatData.UserIsSubscriber && chatData.Channel.Equals("oatsngoats"))
                        SendWhisper(chatData.User, "Thanks for subscribing to " + chatData.Channel + ". Please feel free to join the subscriber Discord channel: https://discord.gg/0f4ukPJq31KmhHcZ");
                    else
                        SendWhisper(chatData.User, "You are not subscribed to " + chatData.Channel + ".");
                }

                if (chatData.ChatMessage.Equals("!commands")) { SendChatMessage("Click here for a list of my commands: http://bombch.us/BRtQ"); }
                if (chatData.ChatMessage.Equals("!playlist")) { SendChatMessage("Click here for the BEST playlist ever: http://bombch.us/Bm5w"); }
                if (chatData.ChatMessage.Equals("!hitbox")) { SendChatMessage("Watch me on Hitbox for minimal delay: www.hitbox.tv/nutella4eva"); }
                if (chatData.ChatMessage.Equals("!youtube")) { SendChatMessage("Watch me on YouTube Gaming for quality options: gaming.youtube.com/hamtotem/live"); }
                if (chatData.ChatMessage.Equals("!joicaster")) { SendChatMessage("Try out all my streams www.twitch.tv/nutella4eva www.hitbox.tv/nutella4eva gaming.youtube.com/hamtotem/live"); }
                if (chatData.ChatMessage.Equals("!emotes")) { SendChatMessage("Click here to see list of FrankerFaceZ emotes for " + chatData.Channel + ": www.frankerfacez.com/" + chatData.Channel); }
                if (chatData.ChatMessage.Equals("!ffz")) { SendChatMessage("Install the FrankerFaceZ plugin to use custom emotes: www.frankerfacez.com"); }
                if (chatData.ChatMessage.Equals("!foosdaraid")) { SendChatMessage("Foosda Raid ( ͡° ͜ʖ ͡°)"); }

                if (chatData.ChatMessage.Contains("ResidentSleeper")) { SendChatMessage("ResidentSleeper ResidentSleeper ResidentSleeper ResidentSleeper ResidentSleeper"); }


                // Show the uptime for the stream. Information is pulled from DecAPI API by Alex Thomassen
                if (chatData.ChatMessage.Equals("!uptime"))
                {
                    string uptime = GetUptime(chatData.Channel);
                    SendChatMessage("@" + chatData.User + ": " + uptime);
                    SendWhisper(chatData.User, uptime);
                    Log.Message(chatData.User + " checked the uptime for #" + chatData.Channel + ": " + uptime, true);
                }
            }

            // Logic for responding to an unknown event
            else
            {
                //Log.Message(chatEvent.ToString(), false);
            } 
            #endregion
        }

        #region Helper Methods
        /// <summary>
        /// Send a chat message to the chat room
        /// </summary>
        /// <param name="message">Message to send to the chat room</param>
        internal void SendChatMessage(string message)
        {
            this.chatConnection.IrcClient.sendChatMessage(this.channel.ChannelName, message);
        }

        /// <summary>
        /// Send a whisper to a designated user
        /// </summary>
        /// <param name="user">Username of recpient</param>
        /// <param name="message">Message to whisper</param>
        internal void SendWhisper(string user, string message)
        {
            this.whisperConnection.IrcClient.sendWhisper(user, message);
        }


    /// <summary>
    /// Show the uptime for the stream. Information is pulled from DecAPI.me API by Alex Thomassen
    /// </summary>
    /// <param name="channel">The channel to check</param>
    /// <param name="irc">IRC client</param>
    /// <param name="user">User that is requesting the uptime</param>
    internal string GetUptime(string channel)
        {
            string urlData = String.Empty;
            WebClient wc = new WebClient();
            urlData = wc.DownloadString("https://decapi.me/twitch/uptime.php?channel=" + channel);

            if (urlData.Equals("Channel is not live.")) { return (channel + " is currently offline."); }
            else { return channel + " has been live for " + urlData; }
        }

        internal void CutDick()
        {

            //if (chatObj.ChatMessage.Equals("!dicks"))
            //{
            //    ircClient.sendChatMessage(chatObj.Channel, "8===D");
            //    ircClient.sendChatMessage(chatObj.Channel, "8==/=D");
            //    ircClient.sendChatMessage(chatObj.Channel, "8==/~");
            //    ircClient.sendChatMessage(chatObj.Channel, chatObj.User + "'s dick has been cut off. " + chatObj.User + " bled out on the floor. " + chatObj.User + " is now dead.");
            //}
        }
        #endregion

    }
}
