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

        public enum TimeLeft
        {
            SHORT,
            MEDIUM,
            LONG,
            VERY_LONG,
        }

        public abstract void Connect();
        public abstract void Disconnect();
        public abstract void PrepareDatabase();
        public abstract void CreateRealmTable(int realmId);
        public abstract void InsertRealmReport(WarcraftAPI.AuctionApiResponse report);
        
        internal string EscapeString(string text)
        {
            return text.Replace("-", "");
        }
    }
}
