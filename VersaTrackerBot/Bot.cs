using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace VersaTrackerBot
{
    class Bot
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static DiscordSocketClient _client;
        private static string MainChannel = "botzone";

        public static void ConnectDB()
        {
            Database.Connect();
        }

        public static void SetClient(DiscordSocketClient client)
        {
            _client = client;
        }

        public static async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;
            if (message.Channel.Name != MainChannel)
                return;

            if (message.Content.StartsWith("!realms"))
            {
                string msg = "Realms:" + Environment.NewLine;

                var realms = Database.GetTables();
                foreach (var realm in realms)
                {
                    long rows = Database.GetRows(realm);
                    long timestamp = Database.GetLastTimestamp(realm);

                    msg += string.Format("\t{0}: {1} lots; last modified: {2}{3}", realm, rows, Utils.GetDateTime(timestamp).ToString(), Environment.NewLine);
                }

                await message.Channel.SendMessageAsync(msg);
            }
            else if (message.Content.StartsWith("!item"))
            {
                string[] temp = message.Content.Split(' ');
                string realm = temp[1];
                int item = int.Parse(temp[2]);

                var lots = Database.GetLots(realm, item);
                await message.Channel.SendMessageAsync(string.Format("Found {0} lots for item {1} for {2}", lots.Count, item, realm));
            }
        }
    }
}
