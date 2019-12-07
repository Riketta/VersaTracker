using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTrackerBot
{
    class Utils
    {
        public static DateTime GetDateTime(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToUniversalTime().DateTime;
        }
    }
}
