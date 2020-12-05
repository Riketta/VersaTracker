﻿using System;
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

        public override void CreateTable(int realmId)
        {
            string command = $@"CREATE TABLE IF NOT EXISTS {realmId}
                                (
                                    timestamp BIGINT,
                                    lot_id BIGINT PRIMARY KEY,
                                    item_id INT,
                                    time_left TEXT,
                                    quantity INT,
                                    unit_price BIGINT,
                                    buyout BIGINT,
                                    bid BIGINT,
                                );";

            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = command;
            sqlite_cmd.ExecuteNonQuery();
        }

        public override void InsertReport(int realmId, DateTime lastModified, WarcraftAPI.AuctionApiResponse.Auction lot)
        {
            string values = $"{new DateTimeOffset(lastModified).ToUnixTimeSeconds()}, {lot.id}, {lot.item.id}, {lot.time_left}, {lot.quantity}, {lot.unit_price}, {lot.buyout}, {lot.bid}";
            SQLiteCommand sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $@"INSERT OR IGNORE INTO {realmId} VALUES ({values});";
            sqlite_cmd.ExecuteNonQuery();
        }
    }
}