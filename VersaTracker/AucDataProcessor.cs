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

        private Database db = null;
        public List<AucTracker> Trackers { get; } = new List<AucTracker>();

        public AucDataProcessor(Database database)
        {
            db = database;
        }

        public void Start()
        {
            db.Connect();

            foreach (var tracker in Trackers) // TODO: add smart queue with autobalance threads
            {
                db.CreateTable(tracker.RealmId);
                tracker.Start();
            }
        }

        public void Stop()
        {
            foreach (var tracker in Trackers)
                tracker.Stop();

            db.Disconnect();
        }

        public void AddTracker(AucTracker tracker)
        {
            logger.Info($"Trying to add tracker of realm \"{tracker.RealmSlug}\" ({tracker.RealmId})");
            if (!Trackers.Exists(t => t.RealmId == tracker.RealmId))
            {
                Trackers.Add(tracker);
                tracker.AuctionNewReportEvent += AuctionNewReportHandler;
            }
            else logger.Warn("Realm already in tracking");
        }

        void AuctionNewReportHandler(object sender, AuctionReportEventArgs e)
        {
            WarcraftAPI.AuctionApiResponse report = e.Report;
            
            string realms = "";
            foreach (var realm in ConnectedRealms.GetRealmSlugsById(report.realmId))
                realms += $@"""{realm}"" ";
            realms = realms.Trim();

            logger.Info($"Inserting new data into database for realm(s): {realms}");
            DateTime starttime = DateTime.UtcNow;
            db.InsertReport(report);
            logger.Info($"Insertion done for realm(s) {realms} in {DateTime.UtcNow.Subtract(starttime)}");
        }
    }
}
