using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class Customer
    {
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime JoinDate { get; set; } = new DateTime(9999, 12, 31);

        public override string ToString()
        {
            return $"{CustomerID} {CustomerName} {JoinDate}";
        }
    }
}
