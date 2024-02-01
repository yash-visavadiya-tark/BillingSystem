using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace BillingSystem.Models
{
    public class Customer
    {
        [Name("Customer ID")]
        public string CustomerID { get; set; }

        [Name("Customer Name")]
        public string CustomerName { get; set; }
        public DateTime JoinDate { get; set; } = new DateTime(9999, 12, 31);

        public override string ToString()
        {
            return $"{CustomerID} {CustomerName} {JoinDate}";
        }
    }
}
