using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class AWSResourceUsage
    {
        public String AWSResourceUsageID { get; set; }
        public String CustomerID { get; set; }
        public String EC2InstanceID { get; set; }
        public String EC2InstanceType { get; set; }
        public DateTime UsedFrom { get; set; }
        public DateTime UsedUntil { get; set; }
        public TimeSpan activeTime
        {
            get
            {
                return UsedUntil - UsedFrom;
            }
        }

        override
            public String ToString()
        {
            return $"{AWSResourceUsageID} {CustomerID} {EC2InstanceID} {EC2InstanceType} {UsedFrom} {UsedUntil} {activeTime}";
        }
    }
}
