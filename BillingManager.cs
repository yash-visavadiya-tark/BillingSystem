using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class BillingManager
    {
        public void GenerateCustomerBillsMonthly(List<AWSResourceTypes> resourceTypes, List<Customer> customerList, List<AWSResourceUsage> resourceUsages)
        {
            Dictionary<String, double> instanceTypeChargeMap = resourceTypes.ToDictionary(key => key.InstanceType, value => value.Charge);
            Dictionary<String, String> customerIdNameMap = customerList.ToDictionary(key => key.CustomerID, value => value.CustomerName);

            var monthlyRecords = new List<AWSResourceUsage>();

            foreach (var item in resourceUsages)
            {
                AddNewRecordsToMakeResourceUsageMonthly(item, monthlyRecords);

                // Add The Last One which does resides in range of month or default one if not exceeded months
                monthlyRecords.Add(item);
            }

            // Now we use filteredForMonthlyRecords instead of awsResourceUsages.
            // So, Cleared awsResourceUsages.
            resourceUsages.Clear();

            var groupedByTime = monthlyRecords.GroupBy(x => new { x.CustomerID, x.UsedFrom.Year, x.UsedFrom.Month}).Select(x => x);

            foreach (var currentGroupedByTime in groupedByTime)
            {
                //Console.WriteLine($"{currentFilteredByTime.Key}");

                var groupedByType = from item in currentGroupedByTime
                                    group item by item.EC2InstanceType;

                double totalBillAmount = 0;

                OutputManager outputManager = new OutputManager();

                foreach (var data in groupedByType)
                {
                    AddAllRecordsOfSameInstanceType(data, ref totalBillAmount, instanceTypeChargeMap, outputManager);
                }

                if (totalBillAmount > 0)
                {
                    CreateBillFile(totalBillAmount, customerIdNameMap, currentGroupedByTime, outputManager);
                }
                //Console.WriteLine();
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
        }

        public void AddAllRecordsOfSameInstanceType(IGrouping<string, AWSResourceUsage> data, ref double totalBillAmount, Dictionary<String, double> instanceTypeChargeMap, OutputManager outputManager)
        {
            HashSet<String> uniqueInstanceSet = new HashSet<String>();
            TimeSpan totalTime = new TimeSpan();

            foreach (var item in data)
            {
                uniqueInstanceSet.Add(item.EC2InstanceID);
                totalTime += item.activeTime;
                //Console.WriteLine(item);
            }

            String InstaceType = data.ElementAt(0).EC2InstanceType;
            double instanceTypeTotalAmount = Math.Ceiling(totalTime.TotalHours) * instanceTypeChargeMap[InstaceType];
            totalBillAmount += instanceTypeTotalAmount;

            // Add A Record
            InstanceTypeBill instanceTypeBill = new InstanceTypeBill();
            instanceTypeBill.ResourceType = InstaceType;
            instanceTypeBill.TotalResources = uniqueInstanceSet.Count();
            instanceTypeBill.TotalUsedTime = totalTime;
            instanceTypeBill.RatePerHour = instanceTypeChargeMap[InstaceType];
            instanceTypeBill.TotalAmount = instanceTypeTotalAmount;

            outputManager.BillByInstanceType.Add(instanceTypeBill);
        }

        public void CreateBillFile(double totalBillAmount, Dictionary<String, String> customerIdNameMap, IGrouping<dynamic, AWSResourceUsage> currentGroupedByTime, OutputManager outputManager)
        {
            outputManager.CustomerName = customerIdNameMap["CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)];
            outputManager.BillingTime = currentGroupedByTime.ElementAt(0).UsedUntil;
            outputManager.TotalBillingAmount = totalBillAmount;

            // Generate Bill
            String path = $"../../../Output/{"CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)}_{outputManager.BillingTime.ToString("MMM").ToUpper()}-{currentGroupedByTime.Key.Year}.csv";
            File.WriteAllText(path, outputManager.GenerateBill());
        }
    }
}
