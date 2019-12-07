using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class Database
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static SQLiteConnection connection = null;

        static Database()
        {
            // Cache Size = -4000
            connection = new SQLiteConnection("Data Source = database.db; Version = 3; Compress = True; Synchronous = Off; Journal Mode = Memory; Temp Store = Memory;");
        }

        ~Database()
        {
            Disconnect();
        }

        public static SQLiteConnection Connect()
        {
            try
            {
                connection.Open();
                //connection.SetExtendedResultCodes(true);
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

        public static void CreateTable(string realm)
        {
            string command = $@"CREATE TABLE IF NOT EXISTS {realm}
                                (
                                    timestamp BIGINT,
                                    auc INT PRIMARY KEY,
                                    item INT,
                                    bid BIGINT,
                                    buyout BIGINT,
                                    quantity INT,
                                    context INT,
                                    petSpeciesId INT,
                                    petBreedId INT,
                                    petLevel INT,
                                    petQualityId INT
                                );";

            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = command;
            sqlite_cmd.ExecuteNonQuery();
        }

        public static void InsertData(string realm, long timestamp, AuctionData.Auction lot)
        {
            //string values = $"{timestamp}, {lot.auc}, {lot.item}, \"{lot.owner}\", \"{lot.ownerRealm}\", {lot.bid}, {lot.buyout}, {lot.quantity}, \"{lot.timeLeft}\", {lot.rand}, {lot.seed}, {lot.context}, 0, 0, {lot.petSpeciesId}, {lot.petBreedId}, {lot.petLevel}, {lot.petQualityId}";
            string values = $"{timestamp}, {lot.auc}, {lot.item}, {lot.bid}, {lot.buyout}, {lot.quantity}, {lot.context}, {lot.petSpeciesId}, {lot.petBreedId}, {lot.petLevel}, {lot.petQualityId}";
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $@"INSERT OR IGNORE INTO {realm} VALUES ({values});";
            sqlite_cmd.ExecuteNonQuery();
        }

        public static bool IsLotExists(string realm, AuctionData.Auction lot)
        {
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT auc FROM {realm} WHERE auc = '{lot.auc}';";


            //SQLiteDataReader sqlite_datareader
            //while (sqlite_datareader.Read())
            //{
            //    string data = sqlite_datareader.GetString(0);
            //    logger.Debug(data);
            //}

            int linesAffected = sqlite_cmd.ExecuteNonQuery();
            if (linesAffected >= 1)
            {
                if (linesAffected > 1)
                    logger.Error("More than one lot found for auc {0} with realm \"{1}\"", lot.auc, realm);

                return true;
            }

            return false;
        }
    }
}