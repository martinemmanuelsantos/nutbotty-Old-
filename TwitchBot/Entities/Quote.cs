using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        #region Database Methods
        public static void AddQuoteToDB(Quote quote)
        {
            string tableName = "QUOTES";
            string columnNames = "QuoteText, ChannelName, AddedBy, DateAdded";
            string parameterNames = @"@quoteText, @channelName, @addedBy, @dateAdded";

            Action<SqlCommand> addParameters = (insertCommand) =>
            {
                insertCommand.Parameters.AddWithValue("@quoteText", quote.QuoteText);
                insertCommand.Parameters.AddWithValue("@channelName", quote.Channel);
                insertCommand.Parameters.AddWithValue("@addedBy", quote.AddedBy);
                insertCommand.Parameters.AddWithValue("@dateAdded", quote.DateAdded);
            };

            Database.InsertEntry(tableName, columnNames, parameterNames, addParameters);
        }

        public static void DeleteQuoteFromDB(string quote)
        {
            Database.DeleteEntry("QUOTES", "QuoteText", quote);
        }

        public static bool QuoteExistsInDB(string quote)
        {
            return Database.EntryExists("QUOTES", "QuoteText", quote);
        }

        public static int QuoteCountInDB()
        {
            return Database.CountRows("QUOTES");
        }

        public static Quote GetQuoteFromDBAtRow(int rowNumber)
        {
            Func<SqlDataReader, Quote> getQuoteParameters = (reader) =>
            {
                Quote quote = new Quote(reader["QuoteText"].ToString(),
                    reader["ChannelName"].ToString(),
                    reader["AddedBy"].ToString(),
                    (DateTime)reader["DateAdded"]);
                return quote;
            };
            return Database.GetEntryAt("QUOTES", rowNumber, getQuoteParameters);
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
