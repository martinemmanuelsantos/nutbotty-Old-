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
                // Iterate through table rows in database and check if the trigger text matches the message
                for (int i = 0; i < ChatCommand.CommandCountInDB(); i++)
                {
                    //Retrieve command from the database and replace the appropriate strings
                    ChatCommand command = ChatCommand.GetCommandFromDBAtRow(i);
                    string responseText = command.ResponseText;
                    responseText = responseText.Replace("$channel", channelName);
                    responseText = responseText.Replace("$user", user);

                    // Check if the command needs to be matched exactly or "loosely"
                    if ((command.MustBeExact && message.Equals(command.TriggerText)) || (!command.MustBeExact && message.Contains(command.TriggerText)))
                    {
                        // Check if the command is universal, or if the command is in the correct channel
                        if (command.IsUniversal || channelName.Equals(command.ChannelName))
                        {
                            // Check if the user is the subscriber (iff the command is subscriber only)
                            if ((command.SubscriberOnly && chatData.UserIsSubscriber) || !(command.SubscriberOnly))
                            {
                                // Check if the user is the moderator (iff the command is moderator only)
                                if ((command.ModeratorOnly && chatData.UserIsModerator) || !(command.ModeratorOnly))
                                {
                                    // Check if the user is the broadcaster (iff the command is broadcaster only)
                                    if ((command.BroadcasterOnly && chatData.UserIsBroadcaster) || !(command.BroadcasterOnly))
                                    {
                                        // Check if the command is whisper only
                                        if (command.WhisperResponse) { SendWhisper(user, responseText); }
                                        else { SendChatMessage(responseText); }
                                    }
                                    else
                                    {
                                        SendWhisper(user, command.TriggerText + " is only available to the broadcaster.");
                                    }
                                }
                                else
                                {
                                    SendWhisper(user, command.TriggerText + " is only available to moderators.");
                                }
                            }
                            else
                            {
                                SendWhisper(user, command.TriggerText + " is only available to subscribers.");
                            }
                        }
                    }
                }
                #endregion

                #region QUOTE Commands
                // Pull a random quote from the QUOTES table
                if (message.StartsWith("!quote"))
                {
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

                #region GUESSING Commands
                // CeresBot Guesses
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

                // Phantoon guesses
                if (message.Equals("!phantoon"))
                {
                    int[] rands = new int[3];
                    string label = null;
                    int total = 0;

                    // Calculate prediction
                    for (int i = 0; i < rands.Length; i++)
                    {
                        rands[i] = RNG.Next(1, 4);
                        total += rands[i];
                        if (rands[i] == 1) { label = label + " SLOW"; }
                        else if (rands[i] == 2) { label = label + " MID"; }
                        else { label = label + " FAST"; }
                    }

                    // Send chat message
                    if (total <= 3) { SendChatMessage("predicts " + label + ". THE RNG LORDS ARE WITH US PogChamp"); }
                    else if (total > 3 && total <= 4) { SendChatMessage("predicts " + label + ". Praise Jesus BloodTrail"); }
                    else if (total > 4 && total <= 6) { SendChatMessage("predicts " + label + ". Maybe this won't be a reset after all OMGScoots"); }
                    else if (total > 6 && total <= 8) { SendChatMessage("predicts " + label + ". Phantoon please BibleThump"); }
                    else if (total == 9) { SendChatMessage("predicts " + label + ". You motherfucker. RESET RESET RESET SwiftRage"); }
                }

                // Eyedoor guesses
                if (message.Equals("!eyedoor"))
                {
                    int rand = RNG.Next(0, 5);

                    // Send chat message
                    if (rand == 0) { SendChatMessage("predicts... ZERO beams. THE RNG GODS ARE WITH YOU PogChamp"); }
                    else if (rand == 1) { SendChatMessage("predicts... ONE beam. Allelujah! BloodTrail"); }
                    else if (rand == 2) { SendChatMessage("predicts... TWO beams. You're lucky this time OMGScoots"); }
                    else if (rand == 3) { SendChatMessage("predicts... THREE beams. Come on eye door! DansGame"); }
                    else if (rand == 4) { SendChatMessage("predicts... FOUR beams. DAFUQ BITCH?! SwiftRage"); }
                }
                #endregion

                #region OTHER Commands
                // Show the uptime for the stream. Information is pulled from DecAPI API by Alex Thomassen
                if (message.Equals("!uptime"))
                {
                    string uptime = GetUptime(chatData.Channel);
                    SendChatMessage("@" + user + ": " + uptime);
                    Log.Message(user + " checked the uptime for #" + chatData.Channel + ": " + uptime, true);
                }

                // Check how many points Nutbotty has on ceresbot
                if (message.Contains(Nutbotty.BOTNAME) && message.Contains("how many points") && channelName.Equals("oatsngoats"))
                {
                    SendChatMessageNoAction("!points");
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
