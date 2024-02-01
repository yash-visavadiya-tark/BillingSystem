using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class ChargeDetails
    {
        public double TotalAmount { get; set; }
        public double TotalDiscount { get; set; }

        public ChargeDetails()
        {
            
        }
        public ChargeDetails(double totalAmount, double totalDiscount)
        {
            TotalAmount = totalAmount;
            TotalDiscount = totalDiscount;
        }

        public override string ToString()
        {
            return $"{TotalAmount} {TotalDiscount}";
        }
    }
}
