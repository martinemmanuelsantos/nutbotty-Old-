using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Entities
{
    class Channel
    {
        #region Member Variables
        string channelName;
        #endregion

        #region Constructors
        public Channel(string channelName)
        {
            this.channelName = channelName;
        }
        #endregion

        public override string ToString()
        {
            return this.ChannelName;
        }

        #region Getters
        public string ChannelName
        {
            get
            {
                return channelName;
            }
        } 
        #endregion
    }
}
