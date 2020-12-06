using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static VersaTracker.WarcraftAPI;

namespace VersaTracker
{
    public class AuctionReportEventArgs
    {
        public AuctionReportEventArgs(AuctionApiResponse response) { Report = response; }
        public AuctionApiResponse Report { get; private set; }
    }

    class AucTracker
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static int interval = 900 * 1000;
        Timer timer = null;
        DateTime lastModified = DateTime.MinValue;
        WarcraftAPI api = null;

        public int RealmId { get; internal set; }
        public string RealmSlug { get; internal set; }

        public delegate void AuctionReportEventHandler(object sender, AuctionReportEventArgs e);
        public event AuctionReportEventHandler AuctionNewReportEvent;

        public AucTracker(WarcraftAPI api, int realmId, string slug)
        {
            this.api = api;
            RealmId = realmId;
            RealmSlug = slug;
        }

        ~AucTracker()
        {
            Stop();
        }

        public override string ToString()
        {
            return $"\"{RealmSlug}\" ({RealmId})";
        }

        public void Start()
        {
            logger.Info($"Creating thread for tracking realm {ToString()}");
            if (timer == null)
                timer = new Timer(MainJob, null, 0, interval);
            else
                throw new Exception($"Thread for realm {ToString()} already exists!");
        }

        public void Stop()
        {
            logger.Info($"Trying to terminate thread for realm {ToString()}");
            timer?.Dispose();
        }

        void MainJob(object state)
        {
            try
            {
                logger.Info($"Trying to request AH API for realm {ToString()}");
                var report = api.AuctionApiRequest(RealmId);

                if (report.lastModified > lastModified)
                {
                    logger.Info($"Available newer ({report.lastModified}) data report for {ToString()} realm with {report.auctions.Length} lot(s)");
                    lastModified = report.lastModified;

                    AuctionNewReportEvent?.Invoke(this, new AuctionReportEventArgs(report));

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                else
                    logger.Debug($"No new reports for {ToString()}");

                logger.Debug($"Iteration done for {ToString()}");
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                logger.Error("Exception during main job loop: {0}", ex.ToString());
            }
        }
    }
}
