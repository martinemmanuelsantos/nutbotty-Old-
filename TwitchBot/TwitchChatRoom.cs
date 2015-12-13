using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchBot.Entities;
using TwitchBot.Events;
using TwitchBot.Parsers;

namespace TwitchBot
{
    class TwitchChatRoom
    {

        #region Member Variables
        TwitchChatConnection chatConnection;
        TwitchChatConnection whisperConnection;
        Channel channel;
        Random RNG = new Random();
        #endregion

        #region Constructors
        public TwitchChatRoom(TwitchChatConnection chatConnection, TwitchChatConnection whisperConnection, Channel channel)
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

        internal Channel Channel
        {
            get
            {
                return channel;
            }
        } 
        #endregion

        internal void RespondToEvent(Event chatEvent)
        {
            #region Bot Logic
            // Logic for responding to a "PRIVMSG" event
            if (chatEvent.GetType().Equals(typeof(ChatEvent)))
            {
                // Create a reference for the chat data
                ChatEvent chatData = (ChatEvent)(chatEvent);
                string message = chatData.ChatMessage;
                string user = chatData.User;
                string channelName = chatData.Channel;

                #region JOIN and PART commands
                if (message.Equals("!join") && channelName.Equals(Nutbotty.BOTNAME))
                {
                    Log.Message(user + " requested " + Nutbotty.BOTNAME + " to join their channel.", true);
                    if (!Channel.ChannelExistsInDB(user))
                    {
                        Channel channel = new Channel(user);
                        Channel.AddChannelToDB(channel);
                        new TwitchChatRoom(chatConnection, whisperConnection, channel);
                        SendChatMessage(Nutbotty.BOTNAME + " is now available for " + user + ". Type !commands for a list of commands you can use.");
                    }
                    else
                    {
                        SendChatMessage(Nutbotty.BOTNAME + " is already available for " + user + ".");
                    }
                }

                if (message.Equals("!part") && channelName.Equals(Nutbotty.BOTNAME))
                {
                    Log.Message(user + " requested " + Nutbotty.BOTNAME + " to part their channel.", true);
                    if (Channel.ChannelExistsInDB(user))
                    {
                        Channel.DeleteChannelFromDB(user);
                        chatConnection.part(user);
                        SendChatMessage("@" + user + ", thank you for using " + Nutbotty.BOTNAME + ".Type !join if you ever want to use " + Nutbotty.BOTNAME + " again.");
                    }
                    else
                    {
                        SendChatMessage(Nutbotty.BOTNAME + " is not in #" + user + ".");
                    }
                } 
                #endregion

                #region Generic Commands
                // TO-DO: Support the following variables for commands: $channel $user $uptime 
                if (message.Equals("!discord") && channelName.Equals("oatsngoats"))
                {
                    if (chatData.UserIsSubscriber)
                        SendWhisper(user, "Thanks for subscribing to " + chatData.Channel + ". Please feel free to join the subscriber Discord channel: https://discord.gg/0f4ukPJq31KmhHcZ");
                    else
                        SendWhisper(user, "You are not subscribed to " + chatData.Channel + ".");
                }
                
                if (message.Contains(Nutbotty.BOTNAME) && message.Contains("how many points") && channelName.Equals("oatsngoats"))
                {
                    SendChatMessageNoAction("!points");
                }

                if (message.Equals("!commands")) { SendChatMessage("Click here for a list of my commands: http://bombch.us/BRtQ"); }
                if (message.Equals("!playlist")) { SendChatMessage("Click here for the BEST playlist ever: http://bombch.us/Bm5w"); }
                if (message.Equals("!hitbox")) { SendChatMessage("Watch me on Hitbox for minimal delay: www.hitbox.tv/nutella4eva"); }
                if (message.Equals("!youtube")) { SendChatMessage("Watch me on YouTube Gaming for quality options: gaming.youtube.com/hamtotem/live"); }
                if (message.Equals("!joicaster")) { SendChatMessage("Try out all my streams www.twitch.tv/nutella4eva www.hitbox.tv/nutella4eva gaming.youtube.com/hamtotem/live"); }
                if (message.Equals("!emotes")) { SendChatMessage("Click here to see list of FrankerFaceZ emotes for " + chatData.Channel + ": www.frankerfacez.com/" + chatData.Channel); }
                if (message.Equals("!ffz")) { SendChatMessage("Install the FrankerFaceZ plugin to use custom emotes: www.frankerfacez.com"); }
                if (message.Equals("!foosdaraid")) { SendChatMessage("Foosda Raid ( ͡° ͜ʖ ͡°)"); }
                if (message.Equals("!sohawt")) { SendChatMessage("http://i.imgur.com/wp5vWsl.png"); }

                // Show the uptime for the stream. Information is pulled from DecAPI API by Alex Thomassen
                if (message.Equals("!uptime"))
                {
                    string uptime = GetUptime(chatData.Channel);
                    SendChatMessage("@" + user + ": " + uptime);
                    Log.Message(user + " checked the uptime for #" + chatData.Channel + ": " + uptime, true);
                }
                #endregion

                #region QUOTE Commands
                // Pull a random quote from the QUOTES table
                if (message.StartsWith("!quote"))
                {
                    //int randomId = RNG.Next(0, Nutbotty.quotes.Count);
                    //SendChatMessage("[" + randomId + "] " + Nutbotty.quotes[randomId].QuoteText);
                    //Console.WriteLine(randomId + " " + Nutbotty.quotes.Count);

                    // Assume the command has no arguments, then split on space characters
                    string[] args = message.Split(' ');

                    // If there is at least one argument, continue, otherwise end if
                    int ID = -1;
                    if (args.Length <= 1)
                    {
                        ID = RNG.Next(0, Quote.QuoteCountInDB());
                        Log.Message(user + " requested a random quote from the database.", true);
                    } else
                    {
                        try {
                            ID = Convert.ToInt32(args[1]);
                            Log.Message(user + " requested for quote #" + ID + " from the database.", true);
                        } catch (Exception e)
                        {
                            Log.Message(e.Message, true);
                        }
                    }

                    // Check if the quote ID is positive and <= number of rows in the database table
                    if (ID >= 0 && ID < Quote.QuoteCountInDB())
                    {
                        SendChatMessage("[" + ID + "] " + Quote.GetQuoteFromDBAtRow(ID).QuoteText);
                    } else
                    {
                        SendWhisper(user, "There are only " + Quote.QuoteCountInDB() + " quotes in the database.");
                    }
                }

                // Add a quote to the QUOTE table
                if (message.StartsWith("!addquote"))
                {
                    // Parse the quote text data
                    string quoteText = message.Substring("!addquote ".Length);
                    Quote quote = new Quote(quoteText, channelName, user, DateTime.Now);
                    
                    // If the user is a moderator, add the quote to the database, else do nothing
                    if (chatData.UserIsModerator) {
                        // Assume the command has no arguments, then split on space characters
                        bool hasArgs = false;
                        string[] args = message.Split(' ');

                        // If there is at least one argument, continue, otherwise end if
                        if (args.Length > 1) { hasArgs = true; }
                        else { Log.Message("<" + channelName + "> " + user + " attempted to add quote, but there was not enough arguments.", true); }
                        // Add quote to database if there were arguments and quote doesn't already exist in the database
                        if (hasArgs)
                        {
                            if (Quote.QuoteExistsInDB(quoteText))
                            {
                                SendChatMessage(user + ", that quote is already in the database.");
                                Log.Message("<" + channelName + "> " + user + " attempted to add quote, but it already exists --> " + quoteText, true);
                            } else
                            {
                                Quote.AddQuoteToDB(quote);
                                SendChatMessage(user + " added quote [" + (Quote.QuoteCountInDB()-1) + "]: " + quoteText);
                                Log.Message("<" + channelName + "> " + user + " added quote: " + quoteText, true);
                            }
                        }
                    } else
                    {
                        SendWhisper(user, "!addquote it only available to moderators");
                        Log.Message(user + " attempted to add a quote but is not a moderator --> " + quoteText, true);
                    }
                }

                // Delete a quote to the QUOTE table by searching the QuoteText column
                if (message.StartsWith("!delquote"))
                {
                    // Parse the quote text data
                    string quoteText = message.Substring("!delquote ".Length);

                    // If the user is a moderator, add the quote to the database, else do nothing
                    if (chatData.UserIsModerator)
                    {
                        // Assume the command has no arguments
                        bool hasArgs = false;
                        // Split the command on space characters
                        string[] args = message.Split(' ');
                        // If there is at least one argument, continue, otherwise end if
                        if (args.Length > 1) { hasArgs = true; }
                        else { Log.Message("<" + channelName + "> " + user + " attempted to delete a quote, but there was not enough arguments.", true); }
                        // Add quote to database if there were arguments and the quote exists
                        if (hasArgs)
                        {
                            if (Quote.QuoteExistsInDB(quoteText))
                            {
                                Quote.DeleteQuoteFromDB(quoteText);
                                SendChatMessage(user + " deleted quote: " + quoteText);
                                Log.Message("<" + channelName + "> " + user + " deleted quote: " + quoteText, true);
                            } else
                            {
                                SendChatMessage(user + ", that quote was not found in the database.");
                                Log.Message("<" + channelName + "> " + user + " attempted to deleted quote, but it does not exist --> " + quoteText, true);
                            }
                        }
                    }
                    else
                    {
                        SendWhisper(user, "!addquote it only available to moderators");
                        Log.Message(user + " attempted to add a quote but is not a moderator --> " + quoteText, true);
                    }
                }
                #endregion

                #region CERESBOT Guesser
                if (message.Contains(@"Round Started. Type !guess") && user.Equals("ceresbot"))
                {
                    int seed = RNG.Next(100);

                    // 30% chance of 45 seconds | 65% chance of 46 seconds | 5% chance of 47 seconds
                    int seconds;
                    if (seed < 30) { seconds = 45; }
                    else if (seed < 95) { seconds = 46; }
                    else { seconds = 47; }

                    // if 45-46 seconds, milliseconds between 0-99, else between 0-25
                    int milliseconds;
                    if (seconds < 47) { milliseconds = RNG.Next(100); }
                    else { milliseconds = RNG.Next(25); }

                    // Make the guess
                    SendChatMessageNoAction("!guess " + seconds + "\"" + milliseconds.ToString("00"));
                }
                #endregion

                #region STRAWPOLL Parser
                if (message.Contains("strawpoll.me/"))
                {
                    if (StrawpollParser.GetStrawpollInfo(message) == null)
                    {
                        SendChatMessage(user + ", that is not a valid Strawpoll");
                    }
                    else
                    {
                        SendChatMessage(user + " pasted a Strawpoll ➤ " + StrawpollParser.GetStrawpollInfo(message));
                    }
                }
                #endregion

                #region YOUTUBE Parser
                if (message.Contains("youtube.com/") || message.Contains("youtu.be/"))
                {
                    if (YouTubeParser.GetYouTubeVideoID(message) != null)
                    {
                        SendChatMessage(user + " pasted a YouTube video ➤ " + YouTubeParser.GetYouTubeInfo(message, YouTubeParser.IS_VIDEO));
                    }
                    if (YouTubeParser.GetYouTubePlaylistID(message) != null)
                    {
                        SendChatMessage(user + " pasted a YouTube playlist ➤ " + YouTubeParser.GetYouTubeInfo(message, YouTubeParser.IS_PLAYLIST));
                    }
                }
                #endregion

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
            this.chatConnection.IrcClient.SendChatMessage(this.channel.ChannelName, message);
            Log.Message("<" + this.channel.ChannelName + "> " + message, true);
        }
        /// <summary>
        /// Send a chat message to the chat room
        /// </summary>
        /// <param name="message">Message to send to the chat room</param>
        internal void SendChatMessageNoAction(string message)
        {
            this.chatConnection.IrcClient.SendChatMessageNoAction(this.channel.ChannelName, message);
            Log.Message("<" + this.channel.ChannelName + "> " + message, true);
        }

        /// <summary>
        /// Send a whisper to a designated user
        /// </summary>
        /// <param name="user">Username of recpient</param>
        /// <param name="message">Message to whisper</param>
        internal void SendWhisper(string user, string message)
        {
            this.whisperConnection.IrcClient.SendWhisper(user, message);
            Log.Message(Nutbotty.BOTNAME + " >> " + user + ": " + message, true);
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
        #endregion

    }
}
