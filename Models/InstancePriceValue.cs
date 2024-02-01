using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class InstancePriceValue
    {
        public double OnDemandCharge { get; set; }
        public double ReservedCharge { get; set; }

        public InstancePriceValue(double OnDemandCharge, double ReservedCharge)
        {
            this.OnDemandCharge = OnDemandCharge;
            this.ReservedCharge = ReservedCharge;
        }
        public override string ToString()
        {
            return OnDemandCharge + " " + ReservedCharge;
        }
    }
}
