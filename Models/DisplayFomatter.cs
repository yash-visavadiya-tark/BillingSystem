using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public class DisplayFomatter
    {
        public string CustomerName { get; set; }
        public DateTime BillingTime { get; set; }
        public ChargeDetails Charge { get; set; }
        public List<InstanceTypeBill> BillByInstanceType { get; set; } = new List<InstanceTypeBill>();

        public void SetDisplayFormater(string customerName, DateTime billingTime, ChargeDetails charge)
        {
            CustomerName = customerName;
            BillingTime = billingTime;
            Charge = charge;
        }

        public string GetBill()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(CustomerName);
            output.AppendLine($"Bill for month of {BillingTime.ToString("MMMM")} {BillingTime.ToString("yyyy")}");
            output.AppendLine($"Total Amount: ${Charge.TotalAmount:0.0000}");
            output.AppendLine($"Discount: ${Charge.TotalDiscount:0.0000}");
            output.AppendLine($"Actual Amount: ${Charge.TotalAmount - Charge.TotalDiscount:0.0000}");
            output.AppendLine("Resource Type, Total Resouorces, Total Used Time (HH:mm:ss), Total Billed Time (HH:mm:ss), Total Amount, Discount, Actual Amount");
            foreach (var bill in BillByInstanceType)
            {
                output.AppendLine(bill.ToString());
            }
            return output.ToString();
        }
    }
}
