using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class InstanceTypeBill
    {
        public string Region { get; set; } = "";
        public string ResourceType { get; set; } = "";
        public int TotalResources { get; set; }
        public TimeSpan TotalUsedTime { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }

        public override string ToString()
        {
            return $"{Region},{ResourceType},{TotalResources},{BillingTotalUsedTime(TotalUsedTime)},{BillingTotalBilledTime(TotalUsedTime)},{TotalAmount:0.0000},{Discount:0.0000},{TotalAmount - Discount:0.0000}";
        }

        public string BillingTotalUsedTime(TimeSpan time)
        {
            return $"{Math.Floor(time.TotalHours)}:{time.Minutes}:{time.Seconds}";
        }

        public string BillingTotalBilledTime(TimeSpan time)
        {
            return $"{Math.Ceiling(time.TotalHours)}:{"00"}:{"00"}";
        }
    }
}
