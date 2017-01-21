using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Entities
{
    class BlockedPhrase
    {
        #region Methods
        public static void BlockPhrase (string phrase)
        {
            AddPhraseToDB(phrase);
        }

        public static void UnblockPhrase(string phrase)
        {
            DeletePhraseFromDB(phrase);
        }
        #endregion

        #region Database Methods
        public static void AddPhraseToDB(string phrase)
        {
            string tableName = "BLOCKED_PHRASES";
            string columnNames = "Phrase";
            string parameterNames = @"@phrase";

            Action<SqlCommand> addParameters = (insertCommand) =>
            {
                insertCommand.Parameters.AddWithValue("@phrase", phrase);
            };

            Database.InsertEntry(tableName, columnNames, parameterNames, addParameters);
        }

        public static void DeletePhraseFromDB(string quote)
        {
            Database.DeleteEntry("BLOCKED_PHRASES", "Phrase", quote);
        }

        public static bool PhraseExistsInDB(string quote)
        {
            return Database.EntryExists("BLOCKED_PHRASES", "Phrase", quote);
        }

        public static int PhraseCountInDB()
        {
            return Database.CountRows("BLOCKED_PHRASES");
        }

        public static string GetPhraseFromDBAtRow(int rowNumber)
        {
            Func<SqlDataReader, string> getQuoteParameters = (reader) =>
            {
                return reader["Phrase"].ToString();
            };
            return Database.GetEntryAt("BLOCKED_PHRASES", rowNumber, getQuoteParameters);
        }
        #endregion
    }
}
