using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class WarcraftAPI
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        Token token = null;
        string region = "";
        string locale = "";

        public WarcraftAPI(string region, string clientId, string clientSecret)
        {
            this.region = Region.Parse(region);
            this.locale = Locale.Default();

            token = new Token(region, clientId, clientSecret);
            token.GetToken();
        }

        string DownloadPage(string url)
        {
            //logger.Debug($"Downloading URL: {url.Replace(token.GetToken(), "{token}")}");
            string data = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            data = readStream.ReadToEnd();
            receiveStream.Close();
            response.Close();

            return data;
        }

        public class AuctionApiResponse
        {
            public class File
            {
                public string url { get; set; }
                public long lastModified { get; set; }
            }

            public File[] files { get; set; }
        }

        public AuctionApiResponse AuctionApiRequest(string realm)
        {
            //logger.Debug($"{nameof(this.AuctionApiRequest)} request");
            string url = $"https://{region}.api.blizzard.com/wow/auction/data/{realm}?locale={locale}&access_token={token.GetToken()}";
            string data = DownloadPage(url);
            AuctionApiResponse response = JsonConvert.DeserializeObject<AuctionApiResponse>(data);
            
            return response;
        }

        class Region
        {
            const string US = "us";
            const string EU = "eu";
            const string KR = "kr";
            const string TW = "tw";

            public static string Default()
            {
                return EU;
            }

            public static string Parse(string region)
            {
                switch (region)
                {
                    case US: return US;
                    case EU: return EU;
                    case KR: return KR;
                    case TW: return TW;
                    default: throw new ArgumentException("No such region available");
                }
            }
        }

        class Locale
        {
            const string enUS = "en_US";
            const string enGB = "en_GB";

            public static string Default()
            {
                return enGB;
            }
        }
    }
}
