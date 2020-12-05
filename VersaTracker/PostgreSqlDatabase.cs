using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class PostgreSqlDatabase : Database
    {
        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override void CreateTable(int realmId)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void InsertReport(int realmId, DateTime lastModified, WarcraftAPI.AuctionApiResponse.Auction lot)
        {
            throw new NotImplementedException();
        }
    }
}
