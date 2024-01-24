using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class BillingInputManager
    {
        public List<AWSResourceUsage> GetAWSResourceUsages()
        {
            var allData = File.ReadAllLines("../../../TestCases/Case1/Input/AWSCustomerUsage.csv");
            var records = from line in allData
                          select line.Split(',').ToList();

            var allAWSResourceUsages = new List<AWSResourceUsage>();
            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var awsResourceUsage = new AWSResourceUsage();

                awsResourceUsage.AWSResourceUsageID = record.data[0].Substring(1, record.data[0].Length - 2);
                awsResourceUsage.CustomerID = record.data[1].Substring(1, record.data[1].Length - 2);
                awsResourceUsage.EC2InstanceID = record.data[2].Substring(1, record.data[2].Length - 2);
                awsResourceUsage.EC2InstanceType = record.data[3].Substring(1, record.data[3].Length - 2);
                awsResourceUsage.UsedFrom = Convert.ToDateTime(record.data[4].Substring(1, record.data[4].Length - 2));
                awsResourceUsage.UsedUntil = Convert.ToDateTime(record.data[5].Substring(1, record.data[5].Length - 2));

                allAWSResourceUsages.Add(awsResourceUsage);
            }
            return allAWSResourceUsages;
        }
        
        public List<AWSResourceTypes> GetAWSResourceTypes()
        {
            var allAWSResourceTypes = new List<AWSResourceTypes>();

            var allData = File.ReadAllLines("../../../TestCases/Case1/Input/AWSResourceTypes.csv");
            var records = from line in allData
                          select line.Split(',').ToList();


            foreach (var record in records.Select((data, ind) => new { ind, data }))
            {
                if (record.ind == 0)
                    continue;

                var awsResourceType = new AWSResourceTypes();

                awsResourceType.AWSResourceID = record.data[0].Substring(1, record.data[0].Length - 2);
                awsResourceType.InstanceType = record.data[1].Substring(1, record.data[1].Length - 2);
                awsResourceType.Charge = double.Parse(record.data[2].Substring(2, record.data[2].Length - 3));

                allAWSResourceTypes.Add(awsResourceType);
            }
            return allAWSResourceTypes;
        }

        public List<Customer> GetCustomers()
        {
            var allData = File.ReadAllLines("../../../TestCases/Case1/Input/Customer.csv");
            var records = from line in allData
                          select line.Split(',').ToList();

            var allCustomers = new List<Customer>();
            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var customer = new Customer();

                customer.CustomerID = record.data[1].Substring(1, record.data[1].Length - 2);
                customer.CustomerName = record.data[2].Substring(1, record.data[2].Length - 2);

                allCustomers.Add(customer);
            }
            return allCustomers;
        }
    }
}
