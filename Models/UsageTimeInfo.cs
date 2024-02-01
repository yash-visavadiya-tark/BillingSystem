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
        public UsageTimeInfo(TimeSpan OnDemand, TimeSpan Reserved, TimeSpan Linux, TimeSpan Windows)
        {
            this.OnDemand = OnDemand;
            this.Reserved = Reserved;
            this.Linux = Linux;
            this.Windows = Windows;
        }

        public override string ToString()
        {
            return $"{OnDemand} {Reserved} {Linux} {Windows}";
        }
    }
}
