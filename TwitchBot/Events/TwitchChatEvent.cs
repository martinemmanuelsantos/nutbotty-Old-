using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchBot.Events;

namespace TwitchBot.Events
{
    public abstract class TwitchChatEvent
    {

        internal static TwitchChatEvent parseEvent(string ircString)
        {
            #region Event Regular Expressions
            // PING
            Regex pingRgx = new Regex(@"PING.*$");

            // PRIVMSG (for regular chat messages)
            Regex chatRgx =
                new Regex(@"@color=#[.0-9a-fA-F]{6};display-name=.*;emotes=.*;subscriber=[0-1];turbo=[0-1];user-id=[0-9]*;user-type=.* :.+!.+@.+\.tmi\.twitch\.tv PRIVMSG #.+ :.*$");

            // WHISPER (for whisper messages)
            Regex whisperRgx =
                new Regex(@"@color=#[.0-9a-fA-F]{6};display-name=.*;emotes=.*;message-id=[0-9]*;thread-id=.*;turbo=[0-1];user-id=[0-9]*;user-type=.* :.+!.+@.+\.tmi\.twitch\.tv WHISPER .+ :.*$");

            // Host event
            Regex hostRgx = new Regex(@"jtv!jtv@jtv\.tmi\.twitch\.tv PRIVMSG .+ :.+ is now hosting you\.$");
            #endregion

            // Check what the event type is, then pass the relevant instance
            if (pingRgx.IsMatch(ircString)) { return new TwitchChatPing(); }
            else if (chatRgx.IsMatch(ircString)) { return new TwitchChatMessage(ircString); }
            else if (whisperRgx.IsMatch(ircString)) { return new TwitchChatWhisper(ircString); }
            else if (hostRgx.IsMatch(ircString)) { return new TwitchChatHost(ircString); }
            else { return new TwitchChatUnknownEvent(ircString); }
        }

        /// <summary>
        /// Parses the chat portion given a full IRC string
        /// </summary>
        /// <param name="ircString">The full IRC string</param>
        /// <returns></returns>
        internal static string ParseMessage(string ircString)
        {
            string message = ircString;                                                                         // Begin with the IRC message
            int startIndex = message.LastIndexOf(':');
            message = message.Substring(startIndex + 1);                                                        // Remove everything at and before the last ':'
            message = Regex.Replace(message, "[^!@#$%^&*()a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
            if (message.StartsWith("ACTION ")) { message = message.Remove(0, "ACTION ".Length); }               // If the chat message is an ACTION message, remove "ACTION " text
            return message;
        }

        /// <summary>
        /// Parses the username of the user that sent the chat message
        /// </summary>
        /// <param name="ircString">The full IRC string</param>
        /// <returns></returns>
        internal static string ParseUser(string ircString)
        {
            string user = ircString;                                                                            // Begin with the IRC message
            int startIndex = user.IndexOf(':');
            int endIndex = user.IndexOf('!');
            int length = endIndex - startIndex - 1;
            user = user.Substring(startIndex + 1, length);                                                      // Keep substring between first ':' and first '!'
            return user;
        }

        #region Metatag Parser Methods
        /// <summary>
        /// Get metatags for chat message
        /// </summary>
        /// <param name="ircString">Full IRC string</param>
        /// <returns></returns>
        internal List<string> GetMetatags(string ircString)
        {
            int space = ircString.IndexOf(' ');
            string metatagString = ircString.Substring(0, space);
            List<string> metatags = metatagString.Split(';').ToList();
            return metatags;
        }

        /// <summary>
        /// Find the string data associated with a given metatag, given a list of metatags
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string FindMetatagData(List<string> metatags, string tag)
        {
            string metatagData = "";
            foreach (string metatag in metatags)
            {
                if (metatag.StartsWith(tag))
                {
                    metatagData = metatag.Replace(tag, "");
                    break;
                }
            }
            return metatagData;
        }

        /// <summary>
        /// Parse the user's color as a hexadecimal string
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseUserColor(List<string> metatags)
        {
            return FindMetatagData(metatags, "@color=");
        }

        /// <summary>
        /// Parse the user's display name, including all capitalization
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseDisplayName(List<string> metatags)
        {
            return FindMetatagData(metatags, "display-name=");
        }

        /// <summary>
        /// Parse the emotes used in the string
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseEmotes(List<string> metatags)
        {
            return FindMetatagData(metatags, "emotes=");
        }

        /// <summary>
        /// Parse the Twitch turbo status of the user
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal bool ParseHasTurbo(List<string> metatags)
        {
            string turboData = FindMetatagData(metatags, "turbo=");
            if (turboData.Equals("1")) { return true; }
            else return false;
        }

        /// <summary>
        /// Parse the user ID
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal long ParseUserId(List<string> metatags)
        {
            return Convert.ToInt64(FindMetatagData(metatags, "user-id="));
        }

        /// <summary>
        /// Parse the user type (mod, global-mod, admin, staff)
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseUserType(List<string> metatags)
        {
            return FindMetatagData(metatags, "user-type=");
        }
        #endregion

    }
}
