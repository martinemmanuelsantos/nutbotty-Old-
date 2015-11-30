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
        public static SqlConnection GetConnection()
        {
            string connectionString= @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\Cloud\Dropbox\Projects\Microsoft Visual Studio\TwitchBot\TwitchBot\TwitchBot\TwitchChatBotDB.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }

        public static void AddChannel(TwitchChannel channel)
        {
            string insertStatement = "INSERT INTO CHANNELS (ChannelName) VALUES (@channelName)";
            SqlConnection connection = GetConnection();
            SqlCommand insertCommand = new SqlCommand(insertStatement, connection);
            insertCommand.Parameters.AddWithValue("@channelName", channel.ChannelName);
            try { connection.Open(); insertCommand.ExecuteNonQuery(); }
            catch(SqlException e) { throw e; }
        }

        public static List<TwitchChannel> GetChannels()
        {
            List<TwitchChannel> channels = new List<TwitchChannel>();
            SqlConnection connection = GetConnection();
            string selectStatement = "SELECT * FROM CHANNELS";
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
            catch (SqlException e) { throw e; }
            finally { connection.Close(); }

            return channels;
        }
    }
}
