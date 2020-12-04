using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTrackerBotX
{
    class Analyzer
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class Report
        {
            string realm;
            int item;

            public DateTime from;
            public TimeRange range;
            public TimeInterval interval;
            public List<IntervalData> data = new List<IntervalData>();

            public Report(string realm, int item, DateTime from, TimeRange range, TimeInterval interval)
            {
                this.realm = realm;
                this.item = item;

                this.from = from;
                this.range = range;
                this.interval = interval;
            }

            public class IntervalData
            {
                public int Quantity = 0;
                public decimal Minimum = 0;
                public decimal Average = 0;
                public decimal Median = 0;
                public decimal Median15 = 0;
                public decimal Median30 = 0;
            }
        }


        /// <summary>
        /// Time range to analyse in seconds
        /// </summary>
        public enum TimeRange
        {
            Custom = -1,
            Day = 86400,
            ThreeDays = 3 * Day,
            FiveDays = 5 * Day,
            Week = 7 * Day,
            TwoWeeks = 2 * Week,
            Month = 4 * Week,
            ThreeMonths = 3 * Month
        }

        public enum TimeInterval
        {
            Custom = -1,
            Hour = 3600,
            ThreeHours = 3 * Hour,
            SixHours = 6 * Hour,
            TwelveHours = 12 * Hour,
            Day = 24 * Hour,
            ThreeDays = 3 * Day,
            Week = 7 * Day
        }


        public static Report GetReport(string realm, int item, TimeRange range, TimeInterval interval)
        {
            long upperTimestamp = Database.GetLastTimestamp(realm);
            DateTime upper = Utils.GetDateTime(upperTimestamp);
            Report report = new Report(realm, item, upper, range, interval);

            int steps = (int)range / (int)interval;
            var totalLots = Database.GetLots(realm, item, upperTimestamp, Utils.GetTimestamp(upper.AddSeconds(-(double)interval * steps)));
            for (int i = 0; i < steps; i++)
            {
                long localUpperTimestamp = Utils.GetTimestamp(upper.AddSeconds(-(double)interval * i));
                long localLowerTimestamp = Utils.GetTimestamp(upper.AddSeconds(-(double)interval * (i + 1)));
                var lots = totalLots.Where(lot => lot.timestamp > localLowerTimestamp && lot.timestamp <= localUpperTimestamp).ToList();
                lots.Sort();

                Report.IntervalData data = new Report.IntervalData();
                foreach (var lot in lots)
                    data.Quantity += lot.quantity;

                data.Minimum = decimal.MaxValue;
                decimal totalprice = 0;
                int quantity = 0;
                foreach (var lot in lots)
                {
                    // Preparation
                    quantity += lot.quantity;
                    decimal buyout = (decimal)lot.buyout / lot.quantity / 10000;
                    totalprice += buyout;

                    // Analysis
                    if (lot.buyoutPerItem < data.Minimum)
                        data.Minimum = lot.buyoutPerItem / 10000;

                    if (quantity >= data.Quantity * 0.15 && data.Median15 == 0)
                        data.Median15 = buyout;
                    else if (quantity >= data.Quantity * 0.30 && data.Median30 == 0)
                        data.Median30 = buyout;
                    else if (quantity >= data.Quantity * 0.50 && data.Median == 0)
                        data.Median = buyout;
                }
                if (lots.Count > 0)
                    data.Average = totalprice / lots.Count;

                report.data.Add(data);
            }

            return report;
        }
    }
}
