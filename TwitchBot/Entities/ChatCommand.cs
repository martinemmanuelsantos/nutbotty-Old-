using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Entities
{
    class ChatCommand
    {
        #region Member Variables
        string triggerText;
        string responseText;
        string channelName;
        bool isUniversal;
        bool mustBeExact;
        bool whisperResponse;
        bool subscriberOnly;
        bool moderatorOnly;
        bool broadcasterOnly;
        #endregion

        #region Constructors
        public ChatCommand(string triggerText, string responseText, string channelName,
            bool isUniversal, bool mustBeExact, bool whisperResponse,
            bool subscriberOnly, bool moderatorOnly, bool broadcasterOnly)
        {
            this.triggerText = triggerText;
            this.responseText = responseText;
            this.channelName = channelName;
            this.isUniversal = isUniversal;
            this.mustBeExact = mustBeExact;
            this.whisperResponse = whisperResponse;
            this.subscriberOnly = subscriberOnly;
            this.moderatorOnly = moderatorOnly;
            this.broadcasterOnly = broadcasterOnly;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return TriggerText + ": " + ResponseText + " | " + ChannelName;
        }
        #endregion

        #region Database Methods
        public static void AddCommandToDB(ChatCommand command)
        {
            string tableName = "COMMANDS";
            string columnNames = "TriggerText, ResponseText, ChannelName, IsUniversal, MustBeExact, WhisperResponse, SubscriberOnly, ModeratorOnly";
            string parameterNames = @"@triggerText, @responseText, @channelName, @isUniversal, @mustBeExact, @whisperResponse, @subscriberOnly, @moderatorOnly";

            Action<SqlCommand> addParameters = (insertCommand) =>
            {
                insertCommand.Parameters.AddWithValue("@triggerText", command.TriggerText);
                insertCommand.Parameters.AddWithValue("@responseText", command.responseText);
                insertCommand.Parameters.AddWithValue("@channelName", command.ChannelName);
                insertCommand.Parameters.AddWithValue("@isUniversal", command.IsUniversal);
                insertCommand.Parameters.AddWithValue("@mustBeExact", command.MustBeExact);
                insertCommand.Parameters.AddWithValue("@whisperResponse", command.WhisperResponse);
                insertCommand.Parameters.AddWithValue("@subscriberOnly", command.SubscriberOnly);
                insertCommand.Parameters.AddWithValue("@moderatorOnly", command.ModeratorOnly);
                insertCommand.Parameters.AddWithValue("@moderatorOnly", command.BroadcasterOnly);
            };

            Database.InsertEntry(tableName, columnNames, parameterNames, addParameters);
        }

        //public static void DeleteQuoteFromDB(string quote)
        //{
        //    Database.DeleteEntry("QUOTES", "QuoteText", quote);
        //}

        //public static bool QuoteExistsInDB(string quote)
        //{
        //    return Database.EntryExists("QUOTES", "QuoteText", quote);
        //}
        
        public static int CommandCountInDB()
        {
            return Database.CountRows("COMMANDS");
        }

        public static ChatCommand GetCommandFromDBAtRow(int rowNumber)
        {
            Func<SqlDataReader, ChatCommand> getCommandParameters = (reader) =>
            {
                ChatCommand command = new ChatCommand(reader["TriggerText"].ToString(),
                    reader["ResponseText"].ToString(),
                    reader["ChannelName"].ToString(),
                    reader["IsUniversal"] as bool? ?? false,
                    reader["MustBeExact"] as bool? ?? false,
                    reader["WhisperResponse"] as bool? ?? false,
                    reader["SubscriberOnly"] as bool? ?? false,
                    reader["ModeratorOnly"] as bool? ?? false,
                    reader["BroadcasterOnly"] as bool? ?? false);
            return command;
            };
            return Database.GetEntryAt("COMMANDS", rowNumber, getCommandParameters);
        }
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

        public bool IsUniversal
        {
            get
            {
                return isUniversal;
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

        public bool SubscriberOnly
        {
            get
            {
                return subscriberOnly;
            }
        }

        public bool ModeratorOnly
        {
            get
            {
                return moderatorOnly;
            }
        }

        public bool BroadcasterOnly
        {
            get
            {
                return broadcasterOnly;
            }
        }
        #endregion

    }
}
