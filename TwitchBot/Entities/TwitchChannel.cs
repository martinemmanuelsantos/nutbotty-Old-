using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Entities
{
    class TwitchChannel
    {
        #region Member Variables
        string channelName;
        #endregion

        #region Constructors
        public TwitchChannel(string channelName)
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
