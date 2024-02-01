using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BillingSystem.Models;

namespace BillingSystem
{
    public class BillingInputManager
    {
        private static string _PATH = "../../../Test cases/input/";
        public List<AWSResourceUsage> GetAWSOnDemandResourceUsages()
        {
            var FileName = "AWSOnDemandResourceUsage.csv";
            var Category = "On Demand";
            var allAWSResourceUsages = new List<AWSResourceUsage>();

            var allData = File.ReadAllLines(_PATH + FileName);
            var records = from line in allData
                          select line.Split(',').ToList();

            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var awsResourceUsage = new AWSResourceUsage(awsResourceUsageID: record.data[0], customerID: record.data[1], ec2InstanceID: record.data[2], ec2InstanceType: record.data[3], usedFrom: Convert.ToDateTime(record.data[4]), usedUntil: Convert.ToDateTime(record.data[5]), region: record.data[6], os: record.data[7], category: Category);

                allAWSResourceUsages.Add(awsResourceUsage);
            }
            return allAWSResourceUsages;
        }
        
        public List<AWSResourceTypes> GetAWSResourceTypes()
        {
            var allAWSResourceTypes = new List<AWSResourceTypes>();
            var FileName = "AWSResourceTypes.csv";

            var allData = File.ReadAllLines(_PATH + FileName);
            var records = from line in allData
                          select line.Split(',').ToList();

            foreach (var record in records.Select((data, ind) => new { ind, data }))
            {
                if (record.ind == 0)
                    continue;

                var awsResourceType = new AWSResourceTypes();

                awsResourceType.AWSResourceID = record.data[0];
                awsResourceType.InstanceType = record.data[1];
                awsResourceType.OnDemandCharge = double.Parse(record.data[2].Substring(1, record.data[2].Length - 1));
                awsResourceType.ReservedCharge = double.Parse(record.data[3].Substring(1, record.data[3].Length - 1));
                awsResourceType.Region = record.data[4];

                allAWSResourceTypes.Add(awsResourceType);
            }
            return allAWSResourceTypes;
        }

        public List<Customer> GetCustomers()
        {
            var allCustomers = new List<Customer>();
            var FileName = "Customer.csv";

            var allData = File.ReadAllLines(_PATH + FileName);
            var records = from line in allData
                          select line.Split(',').ToList();

            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var customer = new Customer();

                customer.CustomerID = record.data[1];
                customer.CustomerName = record.data[2];

                allCustomers.Add(customer);
            }
            return allCustomers;
        }

        public Dictionary<string, string> GetRegionFreeTierMap()
        {
            var regionFreeTierMap = new Dictionary<string, string>();
            var FileName = "Region.csv";

            var allData = File.ReadAllLines(_PATH + FileName);
            var records = from line in allData
                          select line.Split(',').ToList();

            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var Region = record.data[0];
                var FreeTier = record.data[1];

                regionFreeTierMap.Add(Region, FreeTier);
            }

            return regionFreeTierMap;
        }

        public List<AWSResourceUsage> GetAWSReservedInstanceUsages()
        {
            var awsReservedInstanceUsages = new List<AWSResourceUsage>();
            var FileName = "AWSReservedInstanceUsage.csv";
            var Category = "Reserved";

            var allData = File.ReadAllLines(_PATH + FileName);
            var records = from line in allData
                          select line.Split(',').ToList();

            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var awsReservedInstanceUsage = new AWSResourceUsage(awsResourceUsageID: record.data[0], customerID: record.data[1], ec2InstanceID: record.data[2], ec2InstanceType: record.data[3], usedFrom: Convert.ToDateTime(record.data[4]), usedUntil: Convert.ToDateTime(record.data[5]).AddDays(1), region: record.data[6], os: record.data[7], category: Category);
                
                awsReservedInstanceUsages.Add(awsReservedInstanceUsage);
            }
            return awsReservedInstanceUsages;
        }
    }
}
