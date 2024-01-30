using System.Dynamic;
using System.Text;
using BillingSystem.Models;

namespace BillingSystem
{
    internal class BillingApp
    {
        static void Main(string[] args)
        {
            BillingInputManager inputManager = new BillingInputManager();
            BillingManager billingManager = new BillingManager();

            List<AWSResourceTypes> resourceTypes = inputManager.GetAWSResourceTypes();
            List<Customer> customerList = inputManager.GetCustomers();
            List<AWSResourceUsage> resourceUsages = inputManager.GetAWSResourceUsages();
            Dictionary<string, string> regionFreeTierMap = inputManager.GetRegionFreeTierMap();
            List<AWSReservedInstanceUsage> reservedInstanceUsages = inputManager.GetAWSReservedInstanceUsages();

            billingManager.GenerateCustomerBillsMonthly(resourceTypes, customerList, resourceUsages, reservedInstanceUsages, regionFreeTierMap);
        }
    }
}
