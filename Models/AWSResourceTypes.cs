using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class AWSResourceTypes
    {
        public string AWSResourceID { get; set; }
        public string InstanceType { get; set; }
        public double OnDemandCharge { get; set; }
        public double ReservedCharge { get; set; }
        public string Region { get; set; }

        public AWSResourceTypes()
        {
            AWSResourceID = "";
            InstanceType = "";
            OnDemandCharge = 0;
            ReservedCharge = 0;
            Region = "";
        }

        override
        public string ToString()
        {
            return $"{AWSResourceID} {InstanceType} {OnDemandCharge} {ReservedCharge} {Region}";
        }
    }



}
