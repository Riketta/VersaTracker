using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class AucDataProcessor
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        List<AucTracker> trackers = new List<AucTracker>();
        public List<AucTracker> Trackers { get => trackers; }

        public AucDataProcessor()
        {
            Database.Connect();
        }

        public void AddTracker(AucTracker tracker)
        {
            logger.Info("Trying to add tracker of realm \"{0}\"", tracker.Realm);
            if (trackers.Find(t => t.Realm == tracker.Realm) == null) // TODO: validate realm and crossrealms
            {
                trackers.Add(tracker);
                tracker.AuctionNewDataEvent += AuctionNewDataHandler;
                Database.CreateTable(tracker.Realm);
            }
            else logger.Warn("Realm already in tracking");
        }

        void AuctionNewDataHandler(object sender, AuctionDataEventArgs e)
        {
            string realms = "";
            foreach (var realm in e.auctionData.realms)
                realms = realms + string.Format("\"{0}\" ", realm.slug);
            realms = realms.Trim();
            logger.Info("New data for realm(s) {0} received", realms);

            logger.Info("Inserting new data into database for realm(s) {0}", realms);
            DateTime starttime = DateTime.UtcNow;
            foreach (var lot in e.auctionData.auctions)
                Database.InsertData(e.auctionData.GetRealmName(), e.auctionData.timestamp, lot);
            logger.Info("Insertion done for realm(s) {0} in {1}", realms, DateTime.UtcNow.Subtract(starttime));
        }
    }
}
