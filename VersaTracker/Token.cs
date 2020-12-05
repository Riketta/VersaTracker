using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    class Token
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        class RawToken
        {
            public string access_token;
            public string token_type;
            public int expires_in;
            public string scope;
        }

        RawToken token;
        DateTime expires;
        string tokenURL = "https://{region}.battle.net/oauth/token";

        public string Region { get; private set; }
        public string ClientID { get; private set; }
        public string ClientSecret { get; private set; }

        public Token(string region, string clientId, string clientSecret)
        {
            tokenURL = tokenURL.Replace("{region}", region);
            Region = region;
            ClientID = clientId;
            ClientSecret = clientSecret;
        }
        
        public string GetToken()
        {
            if (expires.Subtract(DateTime.UtcNow).TotalSeconds <= 180)
            {
                logger.Info("Token outdated");
                RequestToken();
            }

            return token.access_token;
        }

        void RequestToken()
        {
            logger.Info("Requesting token");

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), tokenURL))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientID}:{ClientSecret}"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                    var response = httpClient.SendAsync(request);
                    response.Wait();
                    var data = response.Result.Content.ReadAsStringAsync();
                    data.Wait();

                    token = JsonConvert.DeserializeObject<RawToken>(data.Result);
                    expires = DateTime.UtcNow.AddSeconds(token.expires_in);
                }
            }

#if DEBUG
            logger.Debug("[DEBUG] Access token: {0}", token.access_token);
#endif
        }
    }
}
