using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class Customer
    {
        public String CustomerID { get; set; }
        public String CustomerName{ get; set; }

        override
        public String ToString()
        {
            return $"{CustomerID} {CustomerName}";
        }
    }
}
