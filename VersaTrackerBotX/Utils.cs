using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTrackerBotX
{
    class Utils
    {
        public static DateTime GetDateTime(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        }

        public static long GetTimestamp(DateTime datetime)
        {
            DateTimeOffset offset = new DateTimeOffset(datetime);
            return offset.ToUnixTimeMilliseconds();
        }

        public static string GetDateFormatString()
        {
            return "(ddd) dd.MM HH:mm";
        }
    }
}
