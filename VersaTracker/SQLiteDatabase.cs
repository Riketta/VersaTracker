using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class SQLiteDatabase : Database
    {
        SQLiteConnection connection = null;

        public SQLiteDatabase()
        {
            // Cache Size = -4000
            connection = new SQLiteConnection("Data Source = database.db; Version = 3; Compress = True; Synchronous = Off; Journal Mode = Memory; Temp Store = Memory;");
        }

        ~SQLiteDatabase()
        {
            Disconnect();
        }

        public override void Connect()
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
        }

        public override void Disconnect()
        {
            logger.Info($"Closing database \"{connection.FileName}\"");
            connection?.Close();
            SQLiteConnection.ClearAllPools();
        }

        public override void PrepareDatabase()
        {
            throw new NotImplementedException();
        }

        public override void CreateRealmTable(int realmId)
        {
            string sql = $@"CREATE TABLE IF NOT EXISTS ""{realmId}""
                                (
                                    timestamp BIGINT,
                                    lot_id BIGINT,
                                    item_id INT,
                                    time_left TEXT,
                                    quantity INT,
                                    unit_price BIGINT
                                );";
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = sql;
            sqlite_cmd.ExecuteNonQuery();
        }

        public override void InsertRealmReport(WarcraftAPI.AuctionApiResponse report)
        {
            long timestamp = new DateTimeOffset(report.lastModified).ToUnixTimeSeconds();
            foreach (var lot in report.auctions)
            {
                string values = $@"{timestamp}, {lot.id}, {lot.item.id}, '{lot.time_left}', {lot.quantity}, {lot.unit_price}, {lot.buyout}, {lot.bid}";
                string sql = $@"INSERT OR IGNORE INTO ""{report.realmId}"" VALUES ({values});";
                SQLiteCommand sqlite_cmd = connection.CreateCommand();
                sqlite_cmd.CommandText = sql;
                sqlite_cmd.ExecuteNonQuery();
            }
        }
    }
}