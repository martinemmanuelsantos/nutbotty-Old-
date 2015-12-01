using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Entities
{
    class TwitchChatCommand
    {
        #region Member Variables
        string triggerText;
        string responseText;
        string channelName;
        bool mustBeExact;
        bool whisperResponse;
        #endregion

        #region Getters
        public string TriggerText
        {
            get
            {
                return triggerText;
            }
        }

        public string ResponseText
        {
            get
            {
                return responseText;
            }
        }

        public string ChannelName
        {
            get
            {
                return channelName;
            }
        }

        public bool MustBeExact
        {
            get
            {
                return mustBeExact;
            }
        }

        public bool WhisperResponse
        {
            get
            {
                return whisperResponse;
            }
        } 
        #endregion
        
    }
}
