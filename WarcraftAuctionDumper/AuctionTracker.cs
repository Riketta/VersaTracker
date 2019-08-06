using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WowAucDumper
{
    class AucTracker
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static int interval = 900 * 1000;
        Thread thread = null;
        long lastModified = 0;
        WarcraftAPI API = null;

        public string Realm { get; }

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

            public override bool Equals(object obj)
            {
                if (!(obj is AuctionData))
                    return false;

                foreach (var aRealm in realms)
                    foreach (var bRealm in ((AuctionData)obj).realms)
                        if (aRealm.slug == bRealm.slug)
                            return true;

                return false;
            }
        }

        public class AuctionDataEventArgs
        {
            public AuctionDataEventArgs(AuctionData data) { auctionData = data; }
            public AuctionData auctionData { get; }
        }

        public delegate void AuctionDataEventHandler(object sender, AuctionDataEventArgs e);
        public event AuctionDataEventHandler NewAuctionDataEvent;


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
            logger.Info($"Creating new thread tracking realm \"{Realm}\"");
            if (thread == null)
            {
                thread = new Thread(MainJob);
                thread.Start();
            }
            else
                throw new Exception($"Thread for realm \"{Realm}\" already started!");
        }

        public void Stop()
        {
            logger.Info($"Trying to terminate thread for realm \"{Realm}\"");
            if (thread != null)
                thread.Abort();
        }

        void MainJob()
        {
            while (true)
            {
                logger.Info($"Trying to request AH API for realm \"{Realm}\"");
                var response = API.AuctionApiRequest(Realm);
                logger.Info("Available {0} report(s) for \"{1}\": ", response.files.Length, Realm);

                long lastMod = 0;
                foreach (var report in response.files)
                {
                    if (report.lastModified > lastModified)
                    {
                        logger.Debug("[{0}] Data: {1}; Updated: {2}", Realm, report.url, report.lastModified);

                        logger.Info("Downloading new report for \"{0}\"", Realm);
                        byte[] rawdata = new WebClient().DownloadData(report.url);
                        string json = Encoding.UTF8.GetString(rawdata);

                        AuctionData data = JsonConvert.DeserializeObject<AuctionData>(json);
                        logger.Info("Available data report for \"{0}\" realm with {1} lots", Realm, data.auctions.Length);

                        lastMod = report.lastModified;
                        NewAuctionDataEvent?.Invoke(this, new AuctionDataEventArgs(data));
                    }
                    else continue;
                }

                if (lastMod == 0)
                    logger.Debug("No new reports for \"{0}\"", Realm);
                else
                    lastModified = lastMod; // TODO: separate lastModified for each "file"
                logger.Debug("Iteration done for \"{0}\"", Realm);
                Thread.Sleep(interval);
            }
        }
    }
}
