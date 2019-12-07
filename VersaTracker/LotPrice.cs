using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTracker
{
    public struct LotPrice
    {
        public long Bid;
        public long Buyout;

        public LotPrice(long bid, long buyout)
        {
            Bid = bid / 10000;
            Buyout = buyout / 10000;
        }
    }
}
