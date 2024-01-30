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

        override
        public string ToString()
        {
            return $"{CustomerID} {CustomerName}";
        }
    }
}
