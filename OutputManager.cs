using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class OutputManager
    {
        public String CustomerName { get; set; }
        public DateTime BillingTime { get; set; }
        public double TotalBillingAmount { get; set; }
        public List<InstanceTypeBill> BillByInstanceType { get; set; } = new List<InstanceTypeBill>();

        public String GenerateBill()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(CustomerName);
            output.AppendLine($"Bill for month of {BillingTime.ToString("MMMM")} {BillingTime.ToString("yyyy")}");
            output.AppendLine($"Total Amount: ${TotalBillingAmount:0.####}");
            output.AppendLine("Resource Type,Total Resources,Total Used Time (HH:mm:ss),Total Billed Time (HH:mm:ss),Rate (per hour),Total Amount");
            foreach(var bill in BillByInstanceType)
            {
                output.AppendLine(bill.ToString());
            }
            return output.ToString();
        }
    }
}
