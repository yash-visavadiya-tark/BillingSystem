using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class InstancePriceKey
    {
        public string InstanceType { get; set; }
        public string Region { get; set; }

        public InstancePriceKey(string instanceType, string region)
        {
            InstanceType = instanceType;
            Region = region;
        }
        public override string ToString()
        {
            return InstanceType + " " + Region;
        }
    }
}
