using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class AWSReservedInstanceUsage
    {
        public int AWSReservedInstanceUsageID { get; set; }
        public string CustomerID { get; set; }
        public string EC2InstanceID { get; set; }
        public string EC2InstanceType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Region { get; set; }
        public string OS { get; set; }

        public AWSReservedInstanceUsage()
        {
            AWSReservedInstanceUsageID = 0;
            CustomerID = "";
            EC2InstanceID = "";
            EC2InstanceType = "";
            Region = "";
            OS = "";
        }

        public override string ToString()
        {
            return $"{AWSReservedInstanceUsageID} {CustomerID} {EC2InstanceID} {EC2InstanceType} {StartDate} {EndDate} {Region} {OS}";
        }
    }
}
