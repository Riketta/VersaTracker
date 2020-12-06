using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class PostgreSqlDatabase : Database
    {
        NpgsqlConnection connection;

        public PostgreSqlDatabase(string host, string username, string password, string database)
        {
            connection = new NpgsqlConnection($"Host = {host}; Username = {username}; Password = {password}; Database = {database}");
        }

        ~PostgreSqlDatabase()
        {
            Disconnect();
        }

        public override void Connect()
        {
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to connect to database: {0}", ex.ToString());
            }
        }

        public override void Disconnect()
        {
            connection.Close();
        }

        public override void CreateTable(int realmId)
        {
            lock (connection)
            {
                string sql = $@"CREATE TABLE IF NOT EXISTS ""{realmId}""
                                (
                                    timestamp BIGINT,
                                    lot_id BIGINT PRIMARY KEY,
                                    item_id INT,
                                    time_left TEXT,
                                    quantity INT,
                                    unit_price BIGINT,
                                    buyout BIGINT,
                                    bid BIGINT
                                );";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
        }

        public override void InsertReport(WarcraftAPI.AuctionApiResponse report)
        {
            lock (connection)
            {
                long timestamp = new DateTimeOffset(report.lastModified).ToUnixTimeSeconds();
                foreach (var lot in report.auctions)
                {
                    string values = $@"{timestamp}, {lot.id}, {lot.item.id}, '{lot.time_left}', {lot.quantity}, {lot.unit_price}, {lot.buyout}, {lot.bid}";
                    string sql = $@"INSERT INTO ""{report.realmId}"" VALUES ({values}) ON CONFLICT (lot_id) DO UPDATE SET time_left = EXCLUDED.time_left, quantity = EXCLUDED.quantity;";
                    NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}