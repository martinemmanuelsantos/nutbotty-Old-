using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Entities;

namespace TwitchBot
{
    class TwitchChatBotDB
    {
        // Connection string for the database file (Right-click database file in Server Explorer --> Connection --> Connection String)
        public const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
                    D:\Cloud\Dropbox\Projects\Microsoft Visual Studio\TwitchBot\TwitchBot\TwitchBot\TwitchChatBotDB.mdf;
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
        public static List<TwitchChannel> GetChannels()
        {
            List<TwitchChannel> channels = new List<TwitchChannel>();
            SqlConnection connection = GetConnection();
            string selectStatement = @"SELECT * FROM CHANNELS ORDER BY [ChannelName]";
            SqlCommand selectCommand = new SqlCommand(selectStatement, connection);
            try
            {
                connection.Open();
                SqlDataReader reader = selectCommand.ExecuteReader();
                while (reader.Read())
                {
                    TwitchChannel channel = new TwitchChannel(reader["ChannelName"].ToString());
                    channels.Add(channel);
                }
                reader.Close();
            }
            catch (SqlException e) { Log.Message(e.Message, true); }
            finally { connection.Close(); }

            return channels;
        }

        /// <summary>
        /// Add a TwitchChannel entity to the CHANNELS table
        /// </summary>
        /// <param name="channel"></param>
        public static void AddChannel(TwitchChannel channel)
        {
            string insertStatement = @"INSERT INTO CHANNELS (ChannelName) VALUES (@channelName)";
            SqlConnection connection = GetConnection();
            SqlCommand insertCommand = new SqlCommand(insertStatement, connection);
            insertCommand.Parameters.AddWithValue("@channelName", channel.ChannelName);
            try { connection.Open(); insertCommand.ExecuteNonQuery(); }
            catch (SqlException e) { Log.Message(e.Message, true); }
        }

        /// <summary>
        /// Remove all rows in CHANNELS table where the ChannelName is channel
        /// </summary>
        /// <param name="channel"></param>
        public static void RemoveChannel(string channel)
        {
            string deleteStatement = @"DELETE FROM CHANNELS WHERE ChannelName = '" + channel + "'";
            SqlConnection connection = GetConnection();
            SqlCommand deleteCommand = new SqlCommand(deleteStatement, connection);
            try { connection.Open(); deleteCommand.ExecuteNonQuery(); }
            catch (SqlException e) { Log.Message(e.Message, true); }
        }

        /// <summary>
        /// Check if a row exists with the given channel name
        /// </summary>
        /// <param name="channel"></param>
        public static bool ChannelExists(string channel)
        {
            string existsStatement = @"SELECT COUNT(*) FROM CHANNELS WHERE ChannelName = '" + channel + "'";
            SqlConnection connection = GetConnection();
            SqlCommand existsCommand = new SqlCommand(existsStatement, connection);
            try
            {
                connection.Open();
                int count = (int)existsCommand.ExecuteScalar();
                Console.WriteLine(existsCommand.ExecuteScalar());
                if (count > 0)
                {
                    return true;
                }
            }
            catch (SqlException e) { Log.Message(e.Message, true); }
            return false;
        }
        #endregion


    }
}
