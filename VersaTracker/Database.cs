using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    abstract class Database
    {
        internal static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public abstract void Connect();
        public abstract void CreateTable(int realmId);
        public abstract void Disconnect();
        public abstract void InsertReport(WarcraftAPI.AuctionApiResponse report);
        
        internal string EscapeString(string text)
        {
            return text.Replace("-", "");
        }
    }
}
