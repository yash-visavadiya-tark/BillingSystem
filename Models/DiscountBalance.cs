using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class DiscountBalance
    {
        public int Linux { get; set; }
        public int Windows { get; set; }

        public DiscountBalance(int maxLinusBalance, int maxWindowsBalance)
        {
            Linux = maxLinusBalance;
            Windows = maxWindowsBalance;
        }   
    }
}
