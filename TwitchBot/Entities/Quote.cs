using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Entities
{
    class Quote
    {
        #region Member Variables
        string quoteText;                       // Contents of the quote
        string channel;                         // Channel that the quote originated from
        string addedBy;                         // Username of the user that added the quote
        DateTime dateAdded;
        #endregion

        #region Constructors
        public Quote(string quoteText, string channel, string addedBy, DateTime dateAdded)
        {
            this.quoteText = quoteText;
            this.channel = channel;
            this.addedBy = addedBy;
            this.dateAdded = dateAdded;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "<" + channel + "> " + quoteText + " | added by " + addedBy + " @" + dateAdded;
        }
        #endregion

        #region Getters
        public string QuoteText
        {
            get
            {
                return quoteText;
            }
        }

        public string Channel
        {
            get
            {
                return channel;
            }
        }

        public string AddedBy
        {
            get
            {
                return addedBy;
            }
        }

        public DateTime DateAdded
        {
            get
            {
                return dateAdded;
            }
        }
        #endregion
    }
}
