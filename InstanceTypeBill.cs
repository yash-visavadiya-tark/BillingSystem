using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class InstanceTypeBill
    {
        public String ResourceType { get; set; } = "";
        public int TotalResources { get; set; }
        public TimeSpan TotalUsedTime { get; set; }
        public double RatePerHour { get; set; }
        public double TotalAmount { get; set; }

        override
        public String ToString()
        {
            return $"{ResourceType},{TotalResources},{BillingTotalUsedTime(TotalUsedTime)},{BillingTotalBilledTime(TotalUsedTime)},{RatePerHour},{TotalAmount:0.####}";
        }

        public String BillingTotalUsedTime(TimeSpan time)
        {
            return $"{Math.Floor(time.TotalHours)}:{time.Minutes}:{time.Seconds}";
        }

        public String BillingTotalBilledTime(TimeSpan time)
        {
            return $"{Math.Ceiling(time.TotalHours)}:{"00"}:{"00"}";
        }
    }
}
