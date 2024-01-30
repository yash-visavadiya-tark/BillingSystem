using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BillingSystem.Models;

namespace BillingSystem
{
    public class BillingManager
    {
        public void GenerateCustomerBillsMonthly(List<AWSResourceTypes> resourceTypes, List<Customer> customerList, List<AWSResourceUsage> onDemandResourceUsages, List<AWSResourceUsage> reservedInstanceUsages, Dictionary<string, string> regionFreeTierMap)
        {
            //Dictionary<String, double> instanceTypeChargeMap = resourceTypes.ToDictionary(key => key.InstanceType, value => value.OnDemandCharge);
            Dictionary<(string InstanceType, string Region), (double OnDemandCharge, double ReservedCharge)> instanceTypeChargeMap = resourceTypes.ToDictionary(key => (key.InstanceType, key.Region), value => (value.OnDemandCharge, value.ReservedCharge));

            Dictionary<String, String> customerIdNameMap = customerList.ToDictionary(key => key.CustomerID, value => value.CustomerName);

            var monthlyRecords = new List<AWSResourceUsage>();

            foreach (var item in onDemandResourceUsages)
            {
                var customer = customerList.Where(x => x.CustomerID.Equals(item.CustomerID.Substring(0, 4) + "-" + item.CustomerID.Substring(4)));
                foreach(var c in customer)
                {
                    c.StartDate = c.StartDate.CompareTo(item.UsedFrom) > 0 ? c.StartDate : item.UsedFrom;
                }
                AddNewRecordsToMakeResourceUsageMonthly(item, monthlyRecords);
            }

            foreach (var item in reservedInstanceUsages)
            {
                var customer = customerList.Where(x => x.CustomerID.Equals(item.CustomerID));
                
                AddNewRecordsToMakeResourceUsageMonthly(item, monthlyRecords);
            }

            customerList.ForEach(Console.WriteLine);

            // Now we use filteredForMonthlyRecords instead of awsResourceUsages.
            // So, Cleared awsResourceUsages.
            onDemandResourceUsages.Clear();

            var groupedByTime = monthlyRecords.GroupBy(x => new { x.CustomerID, x.UsedFrom.Year, x.UsedFrom.Month});

            //Extra
            //foreach (var item in groupedByTime)
            //{
            //    Console.WriteLine(item.Key);
            //    foreach (var item2 in item)
            //    {
            //        Console.WriteLine(item2);
            //    }
            //    Console.WriteLine();
            //}

            foreach (var currentGroupedByTime in groupedByTime)
            {
                Console.WriteLine($"{currentGroupedByTime.Key}");

                var groupedByType = currentGroupedByTime.GroupBy(x => new { x.EC2InstanceType, x.Region });

                double totalBillAmount = 0;

                OutputManager outputManager = new OutputManager();

                foreach (var data in groupedByType)
                {
                    Console.WriteLine(data.Key);
                    AddAllRecordsOfSameInstanceType(data, ref totalBillAmount, instanceTypeChargeMap, outputManager);
                }

                if (totalBillAmount > 0)
                {
                    CreateBillFile(totalBillAmount, customerIdNameMap, currentGroupedByTime, outputManager);
                }
                Console.WriteLine();
            }
        }

        public void AddNewRecordsToMakeResourceUsageMonthly(AWSResourceUsage resourceUsage, List<AWSResourceUsage> monthlyRecords)
        {
            DateTime start = resourceUsage.UsedFrom;
            DateTime end = resourceUsage.UsedUntil;

            double totalDiff = (end - start).TotalSeconds;
            DateTime last = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month), 23, 59, 59);

            while ((last - start).TotalSeconds < totalDiff)
            {
                resourceUsage.UsedFrom = start;
                resourceUsage.UsedUntil = last;

                AWSResourceUsage newRecord = new AWSResourceUsage(resourceUsage);
                monthlyRecords.Add(newRecord);

                start = last.AddSeconds(1);
                last = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month), 23, 59, 59);
                totalDiff = (end - start).TotalSeconds;
            }
            resourceUsage.UsedFrom = start;
            resourceUsage.UsedUntil = end;

            // Add The Last One which does resides in range of month or default one if not exceeded months
            monthlyRecords.Add(resourceUsage);
        }

        public void AddAllRecordsOfSameInstanceType(IGrouping<dynamic, AWSResourceUsage> data, ref double totalBillAmount, Dictionary<(string InstanceType,string Region),  (double OnDemandCharge, double ReservedCharge)> instanceTypeChargeMap, OutputManager outputManager)
        {
            HashSet<String> uniqueInstanceSet = new HashSet<String>();
            TimeSpan totalTime = new TimeSpan();

            foreach (var item in data)
            {
                uniqueInstanceSet.Add(item.EC2InstanceID);
                totalTime += item.activeTime;
                Console.WriteLine(item);
            }
            Console.WriteLine();

            String InstanceType = data.ElementAt(0).EC2InstanceType;
            String Region = data.ElementAt(0).Region;

            //double Charge = ;
            double instanceTypeTotalAmount = Math.Ceiling(totalTime.TotalHours) * instanceTypeChargeMap[(InstanceType, Region)].OnDemandCharge;
            totalBillAmount += instanceTypeTotalAmount;

            // Add A Record
            InstanceTypeBill instanceTypeBill = new InstanceTypeBill();
            instanceTypeBill.Region = Region;
            instanceTypeBill.ResourceType = InstanceType;
            instanceTypeBill.TotalResources = uniqueInstanceSet.Count();
            instanceTypeBill.TotalUsedTime = totalTime;
            instanceTypeBill.TotalAmount = instanceTypeTotalAmount;

            

            outputManager.BillByInstanceType.Add(instanceTypeBill);
        }

        public void CreateBillFile(double totalBillAmount, Dictionary<String, String> customerIdNameMap, IGrouping<dynamic, AWSResourceUsage> currentGroupedByTime, OutputManager outputManager)
        {
            outputManager.CustomerName = customerIdNameMap["CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)];
            outputManager.BillingTime = currentGroupedByTime.ElementAt(0).UsedUntil;
            outputManager.TotalAmount = totalBillAmount;

            // Generate Bill
            String path = $"../../../Output/{"CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)}_{outputManager.BillingTime.ToString("MMM").ToUpper()}-{currentGroupedByTime.Key.Year}.csv";
            File.WriteAllText(path, outputManager.GenerateBill());
        }
    }
}
