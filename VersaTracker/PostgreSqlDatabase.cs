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
                PrepareDatabase();
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

        public override void PrepareDatabase()
        {
            lock (connection)
            {
                string sql = $@"CREATE TABLE IF NOT EXISTS ""time_left""
                                (
                                    index INT PRIMARY KEY,
                                    time_left TEXT
                                );";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                cmd.ExecuteNonQuery();

                foreach (var time in Enum.GetValues(typeof(TimeLeft)))
                {
                    cmd.CommandText = $@"INSERT INTO ""time_left"" VALUES({(int)time}, '{time}') ON CONFLICT (index) DO NOTHING;";
                    cmd.ExecuteNonQuery();
                }

                sql = $@"CREATE TABLE IF NOT EXISTS ""last_modified""
                                (
                                    realm_id INT PRIMARY KEY,
                                    timestamp BIGINT
                                );";
                cmd = new NpgsqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
        }

        public override void CreateRealmTable(int realmId)
        {
            lock (connection)
            {
                string sql = $@"CREATE TABLE IF NOT EXISTS ""{realmId}""
                                (
                                    timestamp BIGINT,
                                    lot_id BIGINT,
                                    item_id INT,
                                    time_left INT REFERENCES time_left (index),
                                    quantity INT,
                                    unit_price BIGINT
                                );";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
        }

        public override void InsertRealmReport(WarcraftAPI.AuctionApiResponse report)
        {
            lock (connection)
            {
                long timestamp = new DateTimeOffset(report.lastModified).ToUnixTimeSeconds();
                long storedTimestamp = long.MinValue;

                string sql = $@"SELECT * FROM ""last_modified"" WHERE realm_id = {report.realmId};";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        storedTimestamp = (long)reader[1];
                
                if (storedTimestamp >= timestamp)
                    return;

                sql = $@"INSERT INTO ""last_modified"" VALUES({report.realmId}, '{timestamp}') ON CONFLICT (realm_id) DO UPDATE SET timestamp = EXCLUDED.timestamp;";
                cmd = new NpgsqlCommand(sql, connection);
                cmd.ExecuteNonQuery();

                foreach (var lot in report.auctions)
                {
                    string values = $@"{timestamp}, {lot.id}, {lot.item.id}, '{(int)Enum.Parse(typeof(TimeLeft), lot.time_left)}', {lot.quantity}, {Math.Max(lot.buyout, lot.unit_price)}";
                    sql = $@"INSERT INTO ""{report.realmId}"" VALUES ({values});";
                    cmd = new NpgsqlCommand(sql, connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}