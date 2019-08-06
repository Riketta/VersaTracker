using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace WowAucDumper
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            LogManager.SetupLogger();
            logger.Info("Warcraft Auction House Dumper ver. {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
            logger.Info("Author Riketta. Feedback: rowneg@bk.ru / https://github.com/riketta");

            logger.Info("Parsing arguments");
            Arguments arguments = null;
            var result = Parser.Default.ParseArguments<Arguments>(args).WithParsed(opts => arguments = opts);

            logger.Info("Creating new API instance");
            WarcraftAPI api = new WarcraftAPI(arguments.Region, arguments.ClientID, arguments.ClientSecret);

            logger.Info("Creating battle pet analyzer");
            PetAnalyzer analyzer = new PetAnalyzer("petdb.txt");

            logger.Info("Creating AH trackers for realms");
            foreach (var realm in arguments.Realms.ToList())
            {
                logger.Info($"Creating tracker for \"{realm}\"");
                AucTracker tracker = new AucTracker(api, realm);
                analyzer.AddTracker(tracker);
                tracker.Start();
            }
            logger.Info("All jobs started");

            logger.Info("Main thread idle");
            int[] petSpeciesIds = new int[] { 2718, 2081, 1532, 868, 2766, 338, 1387, 844 };
            while (true)
            {
                Console.ReadLine();
                PrintReport(petSpeciesIds, analyzer);
            }

            logger.Info("All jobs done");
            Console.ReadLine();
        }

        static void PrintReport(int[] petSpeciesId, PetAnalyzer analyzer)
        {
            logger.Info("Requesting report for battle pets");
            var pets = analyzer.Report();
            logger.Info("====== Battle Pets ======");
            foreach (int speciesId in petSpeciesId)
            {
                var petInfo = PetDatabase.DB.First(pet => pet.Value.SpeciesId == speciesId).Value;
                logger.Info("[{0}] SpeciesId {1}; NpcId {2}", petInfo.Name, petInfo.SpeciesId, petInfo.NpcId);

                foreach (var realm in pets[speciesId])
                {
                    if (realm.Value.Count > 0)
                        logger.Info("\t[{0}] Amount: {1}; Avg: {2}; Med: {3}; Min: {4}", realm.Key, realm.Value.Count, 
                            (long)realm.Value.Average(pet => pet.Buyout), (long)Statistics.Median(realm.Value.Select(pet => (double)pet.Buyout)), realm.Value.Min(pet => pet.Buyout));
                    else
                        logger.Info("\t[{0}] No such pet found", realm.Key);
                }
            }
            logger.Info("=========================");
        }

        static void SaveReport(PetAnalyzer analyzer)
        {
            logger.Info("Trying to save report for battle pets");
            string filename = analyzer.SavePetData();
            logger.Info("Report saved as \"{0}\"", filename);
        }
    }
}
