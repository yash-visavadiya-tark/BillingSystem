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
        public List<AWSResourceUsage> GetAWSOnDemandResourceUsages()
        {
            var allData = File.ReadAllLines("../../../Test cases/input/AWSOnDemandResourceUsage.csv");
            var records = from line in allData
                          select line.Split(',').ToList();

            var allAWSResourceUsages = new List<AWSResourceUsage>();
            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var awsResourceUsage = new AWSResourceUsage();

                awsResourceUsage.AWSResourceUsageID = record.data[0];
                awsResourceUsage.CustomerID = record.data[1];
                awsResourceUsage.EC2InstanceID = record.data[2];
                awsResourceUsage.EC2InstanceType = record.data[3];
                awsResourceUsage.UsedFrom = Convert.ToDateTime(record.data[4]);
                awsResourceUsage.UsedUntil = Convert.ToDateTime(record.data[5]);
                awsResourceUsage.Region = record.data[6];
                awsResourceUsage.OS = record.data[7];
                awsResourceUsage.Category = "On Demand";

                allAWSResourceUsages.Add(awsResourceUsage);
            }
            return allAWSResourceUsages;
        }
        
        public List<AWSResourceTypes> GetAWSResourceTypes()
        {
            var allAWSResourceTypes = new List<AWSResourceTypes>();

            var allData = File.ReadAllLines("../../../Test cases/input/AWSResourceTypes.csv");
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
            var allData = File.ReadAllLines("../../../Test cases/input/Customer.csv");
            var records = from line in allData
                          select line.Split(',').ToList();

            var allCustomers = new List<Customer>();
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
            var allData = File.ReadAllLines("../../../Test cases/input/Region.csv");
            var records = from line in allData
                          select line.Split(',').ToList();

            var regionFreeTierMap = new Dictionary<string, string>();

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
            var allData = File.ReadAllLines("../../../Test cases/input/AWSReservedInstanceUsage.csv");
            var records = from line in allData
                          select line.Split(',').ToList();

            var awsReservedInstanceUsages = new List<AWSResourceUsage>();

            foreach (var record in records.Select((data, ind) => (ind, data)))
            {
                if (record.ind == 0)
                    continue;

                var awsReservedInstanceUsage = new AWSResourceUsage();
                awsReservedInstanceUsage.AWSResourceUsageID = record.data[0];
                awsReservedInstanceUsage.CustomerID = record.data[1];
                awsReservedInstanceUsage.EC2InstanceID = record.data[2];
                awsReservedInstanceUsage.EC2InstanceType= record.data[3];
                awsReservedInstanceUsage.UsedFrom = Convert.ToDateTime(record.data[4]);
                awsReservedInstanceUsage.UsedUntil = Convert.ToDateTime(record.data[5]).AddHours(23).AddMinutes(59).AddSeconds(59);
                awsReservedInstanceUsage.Region = record.data[6];
                awsReservedInstanceUsage.OS = record.data[7];
                awsReservedInstanceUsage.Category = "Reserved";
                
                awsReservedInstanceUsages.Add(awsReservedInstanceUsage);
            }
            return awsReservedInstanceUsages;
        }
    }
}
