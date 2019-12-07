using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VersaTracker
{
    public class AuctionData
    {
        public class Realm
        {
            public string name { get; set; }
            public string slug { get; set; }
        }

        public class Auction
        {
            public int auc { get; set; }
            public int item { get; set; }
            public string owner { get; set; }
            public string ownerRealm { get; set; }
            public long bid { get; set; }
            public long buyout { get; set; }
            public int quantity { get; set; }
            public string timeLeft { get; set; }
            public int rand { get; set; }
            public int seed { get; set; }
            public int context { get; set; }
            public Bonuslist[] bonusLists { get; set; }
            public Modifier[] modifiers { get; set; }
            public int petSpeciesId { get; set; }
            public int petBreedId { get; set; }
            public int petLevel { get; set; }
            public int petQualityId { get; set; }
        }

        public class Bonuslist
        {
            public int bonusListId { get; set; }
        }

        public class Modifier
        {
            public int type { get; set; }
            public int value { get; set; }
        }

        public Realm[] realms { get; set; }
        public Auction[] auctions { get; set; }

        public long timestamp { get; set; }

        public string GetRealmName()
        {
            return realms[0].slug;
        }
    }

    public class AuctionDataEventArgs
    {
        public AuctionDataEventArgs(AuctionData data) { auctionData = data; }
        public AuctionData auctionData { get; }
    }

    class AucTracker
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static int interval = 900 * 1000;
        Timer timer = null;
        long lastModified = 0;
        WarcraftAPI API = null;

        public string Realm { get; }

        public delegate void AuctionDataEventHandler(object sender, AuctionDataEventArgs e);
        public event AuctionDataEventHandler AuctionNewDataEvent;

        public AucTracker(WarcraftAPI api, string realm)
        {
            API = api;
            Realm = realm;
        }

        ~AucTracker()
        {
            Stop();
        }

        public void Start()
        {
            logger.Info($"Creating thread for tracking realm \"{Realm}\"");
            if (timer == null)
            {
                timer = new Timer(MainJob, null, 0, interval);
            }
            else
                throw new Exception($"Thread for realm \"{Realm}\" already exists!");
        }

        public void Stop()
        {
            logger.Info($"Trying to terminate thread for realm \"{Realm}\"");
            if (timer != null)
                timer.Dispose();
        }

        void MainJob(object state)
        {
            try
            {
                logger.Info($"Trying to request AH API for realm \"{Realm}\"");
                var response = API.AuctionApiRequest(Realm);
                logger.Info("Available {0} report(s) for \"{1}\": ", response.files.Length, Realm);

                long newLastModified = -1;
                List<AuctionData> reports = new List<AuctionData>();
                foreach (var report in response.files)
                {
                    if (report.lastModified > lastModified)
                    {
                        //logger.Debug("[{0}] Data: {1}; Updated: {2}", Realm, report.url, report.lastModified);

                        logger.Info("Downloading new report for \"{0}\"", Realm);
                        //byte[] rawdata = new WebClient().DownloadData(report.url);
                        //string json = Encoding.UTF8.GetString(rawdata);
                        //AuctionData data = JsonConvert.DeserializeObject<AuctionData>(json);
                        AuctionData data = JsonConvert.DeserializeObject<AuctionData>(Encoding.UTF8.GetString(new WebClient().DownloadData(report.url)));
                        data.timestamp = report.lastModified;
                        newLastModified = report.lastModified;
                        logger.Info("Available data report for \"{0}\" realm with {1} lots", Realm, data.auctions.Length);

                        reports.Add(data);
                    }
                    else continue;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                foreach (var data in reports)
                    AuctionNewDataEvent?.Invoke(this, new AuctionDataEventArgs(data));

                if (newLastModified == -1)
                    logger.Debug("No new reports for \"{0}\"", Realm);
                else
                    lastModified = newLastModified; // TODO: separate lastModified for each "file"

                if (reports.Count > 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                logger.Debug("Iteration done for \"{0}\"", Realm);
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                logger.Error("Exception during main job loop: {0}", ex.ToString());
            }
        }
    }
}
