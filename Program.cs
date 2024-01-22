using System.Dynamic;

namespace BillingSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InputManager inputManager = new InputManager();

            List<AWSResourceTypes> awsResourceTypes = inputManager.GetAWSResourceTypes();

            List<Customer> customers = inputManager.GetCustomers();

            List<AWSResourceUsage> awsResourceUsages = inputManager.GetAWSResourceUsages();

            foreach (var item in awsResourceUsages)
            {
                Console.WriteLine(item);
            }

            var filtered = (from resource in awsResourceUsages
                           orderby new
                           {
                               resource.CustomerID,
                               resource.EC2InstanceID,
                               resource.EC2InstanceType
                           }
                           group resource by new
                           {
                               resource.CustomerID,
                               resource.EC2InstanceID,
                               resource.EC2InstanceType
                           } into g
                           select g).ToList();

            foreach (var item in filtered)
            {
                Console.WriteLine(item);
            }
        }

    }
}
