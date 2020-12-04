using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaTrackerBotX
{
    class Lot : IComparable<Lot>
    {
        public long timestamp;
        public int auc;
        public int item;
        public long bid;
        public long buyout;
        public int quantity;
        /// <summary>
        /// Buyout price per each item in stack
        /// </summary>
        public decimal buyoutPerItem;

        public int CompareTo(Lot compareLot)
        {
            if (compareLot == null)
                return 1;
            else
                return this.buyoutPerItem.CompareTo(compareLot.buyoutPerItem);
        }

        public override bool Equals(object obj)
        {
            return obj is Lot lot &&
                   auc == lot.auc &&
                   item == lot.item;
        }

        public override int GetHashCode()
        {
            return auc;
        }
    }
}
