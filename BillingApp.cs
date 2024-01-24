using System.Dynamic;
using System.Text;

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

            billingManager.GenerateCustomerBillsMonthly(resourceTypes, customerList, resourceUsages);
        }
    }
}
