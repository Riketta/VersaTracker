using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace VersaTracker
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static bool terminate = false;

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            LogManager.SetupLogger();
            logger.Info("VersaTracker ver. {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
            logger.Info("Author Riketta. Feedback: riketta@outlook.com / https://github.com/riketta");

            logger.Info("Parsing arguments");
            Arguments arguments = null;
            var result = Parser.Default.ParseArguments<Arguments>(args).WithParsed(opts => arguments = opts);

            logger.Info("Creating new Warcraft API instance");
            WarcraftAPI api = new WarcraftAPI(arguments.Region, arguments.ClientID, arguments.ClientSecret);

            logger.Info("Updating connected realms data");
            ConnectedRealms.Update(api);

            logger.Info("Creating auction data processor");
            AucDataProcessor aucDataProcessor = new AucDataProcessor(new PostgreSqlDatabase("192.168.1.35", "postgres", "_password_", "versatracker")); // TODO: change database

            //logger.Info("Creating battle pet analyzer");
            //PetAnalyzer analyzer = new PetAnalyzer("petdb.txt");

            logger.Info("Creating AH trackers for realms");
            foreach (var realmSlug in arguments.Realms.ToList())
            {
                int realmId = ConnectedRealms.GetRealmIdBySlug(realmSlug);
                logger.Info($"Creating tracker for \"{realmSlug}\" ({realmId})");
                AucTracker tracker = new AucTracker(api, realmId, realmSlug);
                aucDataProcessor.AddTracker(tracker);
            }
            aucDataProcessor.Start();
            logger.Info("All jobs started");


            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                terminate = true;
            };

            logger.Info("Main thread idle");
            while (!terminate) {  }

            logger.Info("Finishing jobs");
            aucDataProcessor.Stop();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            logger.Info("All jobs done");
            //Console.ReadLine();
        }
    }
}
