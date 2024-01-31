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

        public InstancePriceKey(string InstanceType, string Region)
        {
            this.InstanceType = InstanceType;
            this.Region = Region;
        }
    }
}
