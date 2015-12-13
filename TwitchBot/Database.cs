using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Entities;

namespace TwitchBot
{
    class Database
    {
        // Connection string for the database file (Right-click database file in Server Explorer --> Connection --> Connection String)
        public const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
                        D:\Cloud\Dropbox\Projects\Microsoft Visual Studio\TwitchBot\TwitchBot\TwitchBot\NutbottyDB.mdf;
                        Integrated Security=True";

        /// <summary>
        /// Connect to the SQL database
        /// </summary>
        /// <returns>The connection</returns>
        public static SqlConnection GetConnection()
        {            
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }

        #region CHANNEL Table
        /// <summary>
        /// Retrive all TwitchChannel entities and populate a list
        /// </summary>
        /// <returns>List of TwitchChannel entities</returns>
        public static List<Channel> GetChannelList()
        {
            List<Channel> channels = new List<Channel>();
            SqlConnection connection = GetConnection();
            string selectStatement = @"SELECT * FROM CHANNELS ORDER BY [ChannelName]";
            SqlCommand selectCommand = new SqlCommand(selectStatement, connection);
            try
            {
                connection.Open();
                SqlDataReader reader = selectCommand.ExecuteReader();
                while (reader.Read())
                {
                    Channel channel = new Channel(reader["ChannelName"].ToString());
                    channels.Add(channel);
                }
                reader.Close();
            }
            catch (SqlException e) { Log.Message(e.Message, true); }
            finally { connection.Close(); }

            return channels;
        }
        #endregion

        #region QUOTES Table
        /// <summary>
        /// Retrive all TwitchChannel entities and populate a list
        /// </summary>
        /// <returns>List of TwitchChannel entities</returns>
        public static List<Quote> GetQuotesList()
        {
            List<Quote> quotes = new List<Quote>();
            SqlConnection connection = GetConnection();
            string selectStatement = @"SELECT * FROM QUOTES ORDER BY [QuoteID]";
            SqlCommand selectCommand = new SqlCommand(selectStatement, connection);
            try
            {
                connection.Open();
                SqlDataReader reader = selectCommand.ExecuteReader();
                while (reader.Read())
                {
                    Quote quote = new Quote(reader["QuoteText"].ToString(), 
                        reader["ChannelName"].ToString(), 
                        reader["AddedBy"].ToString(), 
                        (DateTime)reader["DateAdded"]);
                    quotes.Add(quote);
                }
                reader.Close();
            }
            catch (SqlException e) { Log.Message(e.Message, true); }
            finally { connection.Close(); }

            return quotes;
        }
        #endregion

        #region Test
        /// <summary>
        /// Add an entry to a specified table
        /// </summary>
        /// <param name="tableName">Name of the table that entry will be inserted into</param>
        /// <param name="columnNames">Name of the columns to write data into</param>
        /// <param name="parameterNames">Parameter names for insert statement</param>
        /// <param name="addParameters">Method for adding parameters to insert statement</param>
        public static void InsertEntry(string tableName, string columnNames, string parameterNames, Action<SqlCommand> addParameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                // Build up the SQL command
                string insertStatement = @"INSERT INTO " + tableName + " (" + columnNames + ") VALUES ("+parameterNames+")";
                SqlCommand insertCommand = new SqlCommand(insertStatement, connection);

                // Add the parameters of the object to be saved into database
                addParameters(insertCommand);

                // Execute insert command
                try { connection.Open(); insertCommand.ExecuteNonQuery(); }
                catch (SqlException e) { Log.Message(e.Message, true); }
            }
        }

        /// <summary>
        /// Searches for an entry from a specified table, where the parameter in the specified column matches "parameterString",
        /// then deletes the entry
        /// </summary>
        /// <param name="tableName">Name of the table that entry will be deleted from</param>
        /// <param name="columnName">Name of the column where parameter will be matched</param>
        /// <param name="parameterString">Parameter string to match</param>
        public static void DeleteEntry(string tableName, string columnName, string parameterString)
        {
            using (SqlConnection connection = GetConnection())
            {
                // Build up the SQL command
                string parameter = parameterString.Replace("'", "''");                   // Escape ' characters with ''
                string deleteStatement = @"DELETE FROM " + tableName + " WHERE " + columnName + " = '" + parameter + "'";

                // Add the parameters of the object to be saved into database
                SqlCommand deleteCommand = new SqlCommand(deleteStatement, connection);

                // Execute delete command
                try { connection.Open(); deleteCommand.ExecuteNonQuery(); }
                catch (SqlException e) { Log.Message(e.Message, true); }
            }
        }

        /// <summary>
        /// Searches for an entry from a specified table, where the parameter in the specified column matches parameterString,
        /// then returns true if an entry was found
        /// </summary> 
        /// <param name="tableName">Name of the table that entry will be searched from</param>
        /// <param name="columnName">Name of the column where parameter will be matched</param>
        /// <param name="parameterString">Parameter string to match</param>
        /// <returns>True if parameterString was found in columnName from tableName</returns>
        public static bool EntryExists(string tableName, string columnName, string parameterString)
        {
            string parameter = parameterString.Replace("'", "''");                   // Escape ' characters with ''
            string existsStatement = @"SELECT COUNT(*) FROM " + tableName + " WHERE " + columnName + " = '" + parameter + "'";
            SqlConnection connection = GetConnection();
            SqlCommand existsCommand = new SqlCommand(existsStatement, connection);
            try
            {
                connection.Open();
                int count = (int)existsCommand.ExecuteScalar();
                if (count > 0)
                {
                    return true;
                }
            }
            catch (SqlException e) { Log.Message(e.Message, true); }
            return false;
        }

        /// <summary>
        /// Returns the number of rows in a specified table
        /// </summary>
        /// <param name="tableName">Name of the table that will be counted</param>
        /// <returns></returns>
        public static int CountRows(string tableName)
        {
            string countStatement = @"SELECT COUNT(*) FROM " + tableName;
            SqlConnection connection = GetConnection();
            SqlCommand existsCommand = new SqlCommand(countStatement, connection);
            try
            {
                connection.Open();
                int count = (int)existsCommand.ExecuteScalar();
                return count;
            }
            catch (SqlException e) { Log.Message(e.Message, true); }
            return 0;
        }

        /// <summary>
        /// Retrive all TwitchChannel entities and populate a list
        /// </summary>
        /// <returns>List of TwitchChannel entities</returns>
        public static T GetEntryAt<T>(string tableName, int rowNumber, Func<SqlDataReader, T> getEntityData)
        {

            using (SqlConnection connection = GetConnection())
            {
                string selectStatement = @"SELECT * FROM " + tableName;
                SqlCommand selectCommand = new SqlCommand(selectStatement, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = selectCommand.ExecuteReader();
                    for (int i = 0; reader.Read(); i++)
                    {
                        if (i == rowNumber)
                        {
                            return getEntityData(reader);
                        }
                    }
                    reader.Close();
                }
                catch (SqlException e) { Log.Message(e.Message, true); }
                finally { connection.Close(); }
            }

            return default(T);
        }
        #endregion

    }
}
