using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class InstanceTypeBill
    {
        private string Region { get; }
        private string ResourceType { get; }
        private int TotalResources { get; }
        private TimeSpan TotalUsedTime { get; }
        public ChargeDetails Charge { get; }

        public InstanceTypeBill(string region, string resourceType, int totalResources, TimeSpan totalUsedTime, ChargeDetails charge)
        {
            Region = region;
            ResourceType = resourceType;
            TotalResources = totalResources;
            TotalUsedTime = totalUsedTime;
            Charge = charge;
        }

        public override string ToString()
        {
            return $"{Region}, {ResourceType}, {TotalResources}, {BillingTotalUsedTime(TotalUsedTime)}, {BillingTotalBilledTime(TotalUsedTime)}, ${Charge.TotalAmount:0.0000}, ${Charge.TotalDiscount:0.0000}, ${Charge.TotalAmount - Charge.TotalDiscount:0.0000}";
        }

        private string BillingTotalUsedTime(TimeSpan time)
        {
            return $"{Math.Floor(time.TotalHours)}:{time.Minutes}:{time.Seconds}";
        }

        private string BillingTotalBilledTime(TimeSpan time)
        {
            return $"{Math.Ceiling(time.TotalHours)}:{"0"}:{"0"}";
        }
    }
}
