using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTrackerBot
{
    class Database
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static SQLiteConnection connection = null;

        static Database()
        {
            connection = new SQLiteConnection("Data Source = database.db; Version = 3; Read Only = True;");
        }

        ~Database()
        {
            Disconnect();
        }

        static string EscapeTable(string table)
        {
            return table.Replace("-", "");
        }

        public static SQLiteConnection Connect()
        {
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to connect to database: {0}", ex.ToString());
            }
            return connection;
        }

        public static void Disconnect()
        {
            logger.Info("Closing database \"{0}\"", connection.FileName);
            connection.Close();
            SQLiteConnection.ClearAllPools();
        }

        public static long GetLastTimestamp(string table)
        {
            table = EscapeTable(table);
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT * FROM {table} ORDER BY timestamp DESC LIMIT 1;";

            return (long)sqlite_cmd.ExecuteScalar();
        }

        public static long GetRows(string table)
        {
            table = EscapeTable(table);
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT COUNT(*) FROM {table};";

            return (long)sqlite_cmd.ExecuteScalar();
        }

        public static List<string> GetTables()
        {
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = "SELECT name FROM (SELECT * FROM sqlite_master UNION ALL SELECT * FROM sqlite_temp_master) WHERE type = 'table' ORDER BY name;";

            List<string> realms = new List<string>();
            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string data = sqlite_datareader.GetString(0);
                realms.Add(data);
            }

            return realms;
        }

        public static List<Lot> GetLots(string realm, int item)
        {
            realm = EscapeTable(realm);
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT * FROM {realm} WHERE item = {item}";

            List<Lot> lots = new List<Lot>();
            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                Lot lot = new Lot()
                {
                    timestamp = sqlite_datareader.GetInt64(0),
                    auc = sqlite_datareader.GetInt32(1),
                    item = sqlite_datareader.GetInt32(2),
                    bid = sqlite_datareader.GetInt64(3),
                    buyout = sqlite_datareader.GetInt64(4),
                    quantity = sqlite_datareader.GetInt32(5)
                };

                lots.Add(lot);
            }

            return lots;
        }
    }
}