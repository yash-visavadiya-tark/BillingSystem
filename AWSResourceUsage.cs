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

        public AWSResourceUsage() {
            AWSResourceUsageID = "";
            CustomerID = "";
            EC2InstanceID = "";
            EC2InstanceType = "";
        }

        public AWSResourceUsage(AWSResourceUsage other)
        {
            this.AWSResourceUsageID = other.AWSResourceUsageID;
            this.CustomerID = other.CustomerID;
            this.EC2InstanceID = other.EC2InstanceID;
            this.EC2InstanceType = other.EC2InstanceType;
            this.UsedFrom = other.UsedFrom;
            this.UsedUntil = other.UsedUntil;
        }

        override
            public String ToString()
        {
            return $"{AWSResourceUsageID} {CustomerID} {EC2InstanceID} {EC2InstanceType} {UsedFrom} {UsedUntil} {activeTime}";
        }
    }
}
