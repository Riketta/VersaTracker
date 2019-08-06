using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WowAucDumper.AucTracker;

using PetSpeciesId = System.Int32;

namespace WowAucDumper
{
    class PetAnalyzer
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class LotPrice
        {
            public long Bid = 0;
            public long Buyout = 0;

            public LotPrice(long bid, long buyout)
            {
                Bid = bid / 10000;
                Buyout = buyout / 10000;
            }
        }

        string reports = "reports";
        List<AuctionData> database = new List<AuctionData>();
        List<AucTracker> trackers = new List<AucTracker>();

        public List<AucTracker> Trackers { get => trackers; }

        public PetAnalyzer(string petDatabasePath)
        {
            logger.Info("Parsing local pet database");
            PetDatabase.Load(petDatabasePath);
        }

        public void AddTracker(AucTracker tracker)
        {
            logger.Info("Trying to add tracker of realm \"{0}\"", tracker.Realm);
            if (trackers.Find(t => t.Realm == tracker.Realm) == null)
            {
                trackers.Add(tracker);
                tracker.NewAuctionDataEvent += NewAuctionDataHandler;
            }
            else logger.Warn("Realm already in tracking");
        }

        void AddData(AuctionData newData)
        {
            var oldData = database.Find(data => data.Equals(newData));
            if (oldData != null)
            {
                logger.Debug("Removing old auction data for \"{0}\" realm", newData.realms[0]);
                database.Remove(oldData);
            }

            database.Add(newData);
        }

        void NewAuctionDataHandler(object sender, AuctionDataEventArgs e)
        {
            string realms = "";
            foreach (var realm in e.auctionData.realms)
                realms = realms + string.Format("\"{0}\" ", realm.slug);
            realms = realms.Trim();
            logger.Info("New data for realm(s) {0} received", realms);

            AddData(e.auctionData);
        }

        public Dictionary<string, List<AuctionData.Auction>> FindPets()
        {
            Dictionary<string, List<AuctionData.Auction>> pets = new Dictionary<string, List<AuctionData.Auction>>();
            foreach (var realmAuctionData in database)
            {
                var realmPets = realmAuctionData.auctions.ToList().FindAll(lot => lot.petLevel > 0);
                pets[realmAuctionData.realms[0].slug] = realmPets;
                logger.Debug("Found {0} pets on \"{1}\" realm", realmPets.Count, realmAuctionData.realms[0].slug);
            }

            return pets;
        }

        public Dictionary<PetSpeciesId, Dictionary<string, List<LotPrice>>> Report()
        {
            var petsOverRealms = FindPets();
            Dictionary<PetSpeciesId, Dictionary<string, List<LotPrice>>> petPricesPerRealm = new Dictionary<PetSpeciesId, Dictionary<string, List<LotPrice>>>();
            
            foreach (var petInfo in PetDatabase.DB)
            {
                PetSpeciesId petSpecieId = petInfo.Key;
                //logger.Debug("Parsing data for pet {0}", petSpecieId);

                petPricesPerRealm[petSpecieId] = new Dictionary<string, List<LotPrice>>();
                foreach (var realm in petsOverRealms)
                {
                    var realmPetPrices = petPricesPerRealm[petSpecieId][realm.Key] = new List<LotPrice>();

                    //logger.Debug("Looking for pet {0} on realm \"{1}\"", petSpecieId, realm.Key);
                    realm.Value.FindAll(lot => lot.petSpeciesId == petInfo.Value.SpeciesId).ForEach(pet => realmPetPrices.Add(new LotPrice(pet.bid, pet.buyout)));
                    //logger.Debug("Found {0} of {1} pet on realm \"{2}\"", realmPetPrices.Count, petSpecieId, realm.Key);
                }
            }

            return petPricesPerRealm;
        }

        public string SavePetData()
        {
            var pets = FindPets();

            if (!Directory.Exists(reports))
                Directory.CreateDirectory(reports);

            string filename = string.Format(Path.Combine(reports, "{0}_{1}.json"), this.GetType().Name, DateTime.Now.ToString("yyyyMMdd.HHmmss"));
            using (StreamWriter writer = new StreamWriter(filename))
                writer.Write(JsonConvert.SerializeObject(pets, Formatting.Indented));

            return filename;
        }
    }
}
