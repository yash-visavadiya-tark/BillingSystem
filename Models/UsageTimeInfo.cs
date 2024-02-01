using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class UsageTimeInfo
    {
        public TimeSpan OnDemand { get; set; }
        public TimeSpan Reserved { get; set; }
        public TimeSpan Linux { get; set; }
        public TimeSpan Windows { get; set; }

        public UsageTimeInfo()
        {
            
        }

        public override string ToString()
        {
            return $"{OnDemand} {Reserved} {Linux} {Windows}";
        }
    }
}
