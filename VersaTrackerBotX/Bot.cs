using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace VersaTrackerBotX
{
    class Bot
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static DiscordClient _client;
        private static string botChannel = "botzone";

        public static void ConnectDB(string database)
        {
            Database.Connect(database);
        }

        public static void SetClient(DiscordClient client)
        {
            _client = client;
        }

        public static async Task MessageReceivedAsync(MessageCreateEventArgs e)
        {
            if (e.Author.Id == _client.CurrentUser.Id)
                return;
            if (e.Channel.Name != botChannel)
                return;

            if (e.Message.Content.StartsWith("!realms"))
            {
                logger.Info("Generating realms report");
                string msg = "Realms:" + Environment.NewLine;

                var realms = Database.GetTables();
                foreach (var realm in realms)
                {
                    long rows = Database.GetRowsCount(realm);
                    long timestamp = Database.GetLastTimestamp(realm);

                    msg += string.Format("\t{0}: {1} lots; last modified: {2}{3}", realm, rows, Utils.GetDateTime(timestamp).ToString(Utils.GetDateFormatString()), Environment.NewLine);
                }
                msg = string.Format("```{0}{1}All time in UTC.```", msg, Environment.NewLine);

                await e.Channel.SendMessageAsync(msg);
            }
            else if (e.Message.Content.StartsWith("!item")) // !item realm itemID
            {
                logger.Info("Generating item report");
                string[] temp = e.Message.Content.Split(' ');
                string realm = temp[1];
                int item = int.Parse(temp[2]);

                var lots = Database.GetAllLots(realm, item);
                await e.Channel.SendMessageAsync(string.Format("```Found {0} lots for item {1} for {2}```", lots.Count, item, realm));
            }
            else if (e.Message.Content.StartsWith("!price")) // !price realm item range interval
            {
                logger.Info("Generating price report");
                string[] temp = e.Message.Content.Split(' ');
                if (temp.Length == 1)
                {
                    // TODO: print help
                }
                else
                {
                    string realm = temp[1];
                    int item = int.Parse(temp[2]);
                    Analyzer.TimeRange range = Analyzer.TimeRange.Custom;
                    Enum.TryParse(temp[3], out range);
                    Analyzer.TimeInterval interval = Analyzer.TimeInterval.Custom;
                    Enum.TryParse(temp[4], out interval);

                    var report = Analyzer.GetReport(realm, item, range, interval);
                    int step = 0;
                    string histogram = "";
                    foreach (var data in report.data)
                    {
                        DateTime localFrom = report.from.AddSeconds(-(double)interval * step);
                        DateTime localTo = report.from.AddSeconds(-(double)interval * (step + 1));

                        histogram += string.Format("[{0}; {1}): Min: {5:0.##}g; Average: {2:0.##}g; Median15%: {3:0.##}g; Quantity: {4}{6}",
                            localTo.ToString(Utils.GetDateFormatString()), localFrom.ToString(Utils.GetDateFormatString()), data.Average, data.Median15, data.Quantity, data.Minimum, Environment.NewLine);
                        step++;
                    }

                    string sreport = string.Format("```Report for item {0} for {1}:{3}{2}```",
                        item, realm, histogram, Environment.NewLine);
                    await e.Channel.SendMessageAsync(sreport);
                }
            }
        }
    }
}
