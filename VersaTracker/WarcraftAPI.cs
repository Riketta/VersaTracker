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
    public class WarcraftAPI
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

        class Response
        {
            public WebHeaderCollection Headers;
            public DateTime LastModified;
            public string Data;
        }

        Response DownloadURL(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
#if DEBUG
            logger.Debug($"Downloading URL: {url.Replace(token.GetToken(), "{token}")}");
#endif
            Response resp = new Response()
            {
                Headers = response.Headers,
                LastModified = response.LastModified,
                Data = readStream.ReadToEnd(),
            };
            receiveStream.Close();
            response.Close();

            return resp;
        }

        public class AuctionApiResponse
        {
            /// <summary>
            /// Additional field with extra meta data
            /// </summary>
            public int realmId { get; set; }
            /// <summary>
            /// Additional field with extra meta data
            /// </summary>
            public DateTime lastModified { get; set; }


            public _Links _links { get; set; }
            public Connected_Realm connected_realm { get; set; }
            public Auction[] auctions { get; set; }

            public class _Links
            {
                public Self self { get; set; }
            }

            public class Self
            {
                public string href { get; set; }
            }

            public class Connected_Realm
            {
                public string href { get; set; }
            }

            public class Auction
            {
                public int id { get; set; }
                public Item item { get; set; }
                public int quantity { get; set; }
                public long unit_price { get; set; }
                public string time_left { get; set; }
                public long buyout { get; set; }
                public long bid { get; set; }
            }

            public class Item
            {
                public int id { get; set; }
                public int context { get; set; }
                public Modifier[] modifiers { get; set; }
                public int[] bonus_lists { get; set; }
                public int pet_breed_id { get; set; }
                public int pet_level { get; set; }
                public int pet_quality_id { get; set; }
                public int pet_species_id { get; set; }
            }

            public class Modifier
            {
                public int type { get; set; }
                public int value { get; set; }
            }
        }

        public class ConnectedRealmsApiResponse
        {
            public int page { get; set; }
            public int pageSize { get; set; }
            public int maxPageSize { get; set; }
            public int pageCount { get; set; }
            public Result[] results { get; set; }

            public class Result
            {
                public Key key { get; set; }
                public Data data { get; set; }
            }

            public class Key
            {
                public string href { get; set; }
            }

            public class Data
            {
                public Realm[] realms { get; set; }
                public int id { get; set; }
                public bool has_queue { get; set; }
                public Status status { get; set; }
                public Population population { get; set; }
            }

            public class Population
            {
                public PopulationNameLocalized name { get; set; }
                public string type { get; set; }
            }

            public class Status
            {
                public StatusNameLocalized name { get; set; }
                public string type { get; set; }
            }

            public class Realm
            {
                public bool is_tournament { get; set; }
                public string timezone { get; set; }
                public RealmNameLocalized name { get; set; }
                public int id { get; set; }
                public Region region { get; set; }
                public Category category { get; set; }
                public string locale { get; set; }
                public Type type { get; set; }
                public string slug { get; set; }
            }

            public class Region
            {
                public RegionNameLocalized name { get; set; }
                public int id { get; set; }
            }

            public class Type
            {
                public TypeNameLocalized name { get; set; }
                public string type { get; set; }
            }

            public class StatusNameLocalized
            {
                public string it_IT { get; set; }
                public string ru_RU { get; set; }
                public string en_GB { get; set; }
                public string zh_TW { get; set; }
                public string ko_KR { get; set; }
                public string en_US { get; set; }
                public string es_MX { get; set; }
                public string pt_BR { get; set; }
                public string es_ES { get; set; }
                public string zh_CN { get; set; }
                public string fr_FR { get; set; }
                public string de_DE { get; set; }
            }

            public class PopulationNameLocalized
            {
                public string it_IT { get; set; }
                public string ru_RU { get; set; }
                public string en_GB { get; set; }
                public string zh_TW { get; set; }
                public string ko_KR { get; set; }
                public string en_US { get; set; }
                public string es_MX { get; set; }
                public string pt_BR { get; set; }
                public string es_ES { get; set; }
                public string zh_CN { get; set; }
                public string fr_FR { get; set; }
                public string de_DE { get; set; }
            }

            public class RealmNameLocalized
            {
                public string it_IT { get; set; }
                public string ru_RU { get; set; }
                public string en_GB { get; set; }
                public string zh_TW { get; set; }
                public string ko_KR { get; set; }
                public string en_US { get; set; }
                public string es_MX { get; set; }
                public string pt_BR { get; set; }
                public string es_ES { get; set; }
                public string zh_CN { get; set; }
                public string fr_FR { get; set; }
                public string de_DE { get; set; }
            }

            public class RegionNameLocalized
            {
                public string it_IT { get; set; }
                public string ru_RU { get; set; }
                public string en_GB { get; set; }
                public string zh_TW { get; set; }
                public string ko_KR { get; set; }
                public string en_US { get; set; }
                public string es_MX { get; set; }
                public string pt_BR { get; set; }
                public string es_ES { get; set; }
                public string zh_CN { get; set; }
                public string fr_FR { get; set; }
                public string de_DE { get; set; }
            }

            public class TypeNameLocalized
            {
                public string it_IT { get; set; }
                public string ru_RU { get; set; }
                public string en_GB { get; set; }
                public string zh_TW { get; set; }
                public string ko_KR { get; set; }
                public string en_US { get; set; }
                public string es_MX { get; set; }
                public string pt_BR { get; set; }
                public string es_ES { get; set; }
                public string zh_CN { get; set; }
                public string fr_FR { get; set; }
                public string de_DE { get; set; }
            }

            public class Category
            {
                public string it_IT { get; set; }
                public string ru_RU { get; set; }
                public string en_GB { get; set; }
                public string zh_TW { get; set; }
                public string ko_KR { get; set; }
                public string en_US { get; set; }
                public string es_MX { get; set; }
                public string pt_BR { get; set; }
                public string es_ES { get; set; }
                public string zh_CN { get; set; }
                public string fr_FR { get; set; }
                public string de_DE { get; set; }
            }
        }

        public AuctionApiResponse AuctionApiRequest(int realmId)
        {
#if DEBUG
            logger.Debug($"{nameof(this.AuctionApiRequest)} request");
#endif
            string url = $"https://{region}.api.blizzard.com/data/wow/connected-realm/{realmId}/auctions?namespace=dynamic-{region}&locale={locale}&access_token={token.GetToken()}";
            var data = DownloadURL(url);
            AuctionApiResponse response = JsonConvert.DeserializeObject<AuctionApiResponse>(data.Data);
            response.lastModified = data.LastModified;
            response.realmId = realmId;

            return response;
        }

        public ConnectedRealmsApiResponse ConnectedRealmsListApiRequest(int page)
        {
#if DEBUG
            logger.Debug($"{nameof(this.ConnectedRealmsListApiRequest)} request");
#endif
            string url = $"https://{region}.api.blizzard.com/data/wow/search/connected-realm?namespace=dynamic-{region}&locale={locale}&_page={page}&access_token={token.GetToken()}";
            var data = DownloadURL(url);
            ConnectedRealmsApiResponse response = JsonConvert.DeserializeObject<ConnectedRealmsApiResponse>(data.Data);

            return response;
        }

        class Region
        {
            const string US = "us";
            const string EU = "eu";
            const string KR = "kr";
            const string TW = "tw";
            const string CN = "cn";

            public static string Default()
            {
                return EU;
            }

            public static string Parse(string region)
            {
                switch (region.ToLower())
                {
                    case US: return US;
                    case EU: return EU;
                    case KR: return KR;
                    case TW: return TW;
                    //case CN: return CN;
                    default: throw new ArgumentException("No such region available");
                }
            }
        }

        class Locale
        {
            const string enUS = "en_US";
            const string enGB = "en_GB";
            const string ruRU = "ru_RU";

            public static string Default()
            {
                return enGB;
            }
        }
    }
}