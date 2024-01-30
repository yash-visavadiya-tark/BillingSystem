using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class AWSResourceUsage
    {
        public string AWSResourceUsageID { get; set; }
        public string CustomerID { get; set; }
        public string EC2InstanceID { get; set; }
        public string EC2InstanceType { get; set; }
        public DateTime UsedFrom { get; set; }
        public DateTime UsedUntil { get; set; }
        public TimeSpan activeTime
        {
            get
            {
                return UsedUntil - UsedFrom;
            }
        }
        public string Region { get; set; }
        public string OS { get; set; }

        public string Category;

        public AWSResourceUsage()
        {
            AWSResourceUsageID = "";
            CustomerID = "";
            EC2InstanceID = "";
            EC2InstanceType = "";
            Region = "";
            OS = "";
            Category = "";
        }

        public AWSResourceUsage(AWSResourceUsage other)
        {
            AWSResourceUsageID = other.AWSResourceUsageID;
            CustomerID = other.CustomerID;
            EC2InstanceID = other.EC2InstanceID;
            EC2InstanceType = other.EC2InstanceType;
            UsedFrom = other.UsedFrom;
            UsedUntil = other.UsedUntil;
            Region = other.Region;
            OS = other.OS;
            Category = other.Category;
        }

        public override string ToString()
        {
            return $"{AWSResourceUsageID} {CustomerID} {EC2InstanceID} {EC2InstanceType} {UsedFrom} {UsedUntil} {activeTime} {Region} {OS} {Category}";
        }
    }
}
