using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BillingSystem.Models;

namespace BillingSystem
{
    public class BillingManager
    {
        private void AddNewRecordsToMakeResourceUsageMonthly(AWSResourceUsage resourceUsage, List<AWSResourceUsage> monthlyRecords)
        {
            DateTime start = resourceUsage.UsedFrom;
            DateTime end = resourceUsage.UsedUntil;

            double totalDiff = (end - start).TotalSeconds;
            DateTime last = new DateTime(start.AddMonths(1).Year, start.AddMonths(1).Month, 1);

            while ((last - start).TotalSeconds < totalDiff)
            {
                resourceUsage.UsedFrom = start;
                resourceUsage.UsedUntil = last;

                AWSResourceUsage newRecord = new AWSResourceUsage(resourceUsage);
                monthlyRecords.Add(newRecord);

                start = last;
                last = start.AddMonths(1);
                totalDiff = (end - start).TotalSeconds;
            }
            resourceUsage.UsedFrom = start;
            resourceUsage.UsedUntil = end;

            monthlyRecords.Add(resourceUsage);
        }

        private List<AWSResourceUsage> GetMonthlyUsagesFromResourceUsage(List<AWSResourceUsage> resourceUsages, List<Customer> customerList)
        {
            var monthlyResourceUsages = new List<AWSResourceUsage>();
            foreach (var item in resourceUsages)
            {
                var customer = customerList.Where(x => x.CustomerID.Equals(item.CustomerID.Substring(0, 4) + "-" + item.CustomerID.Substring(4))).ToList().FirstOrDefault();
                customer.JoinDate = customer.JoinDate.CompareTo(item.UsedFrom) <= 0 ? customer.JoinDate : item.UsedFrom;

                AddNewRecordsToMakeResourceUsageMonthly(item, monthlyResourceUsages);
            }
            return monthlyResourceUsages;
        }
        private List<AWSResourceUsage> GetMonthlyUsages(List<AWSResourceUsage> reservedUsages, List<AWSResourceUsage> OnDemandUsages, List<Customer> customerList)
        {
            var monthlyResourceUsages = new List<AWSResourceUsage>();

            monthlyResourceUsages.AddRange(GetMonthlyUsagesFromResourceUsage(OnDemandUsages, customerList));
            monthlyResourceUsages.AddRange(GetMonthlyUsagesFromResourceUsage(reservedUsages, customerList));

            return monthlyResourceUsages;
        }

        public void AddAllRecordsOfSameInstanceType(ref (int Linux, int Windows) discountBalanceHours,IGrouping<dynamic, AWSResourceUsage> data, ref (double BillAmount, double Discount) total, Dictionary<InstancePriceKey, InstancePriceValue> instanceTypeChargeMap, OutputManager outputManager, List<Customer> customers, Dictionary<string, string> regionFreeTierMap)
        {
            UsageTimeInfo TotalTime = new UsageTimeInfo();
            
            void AddTime(IGrouping<dynamic, AWSResourceUsage> data, UsageTimeInfo TotalTime)
            {
                foreach (var item in data)
                {
                    Console.WriteLine(item);

                    if (item.Category.Equals("On Demand"))
                    {
                        TotalTime.OnDemand += item.activeTime;
                        if (item.OS.Equals("Windows"))
                            TotalTime.Windows += item.activeTime;
                        else
                            TotalTime.Linux += item.activeTime;
                    }
                    else
                    {
                        TotalTime.Reserved += item.activeTime;
                    }
                }
                Console.WriteLine();
            }
            
            void CalculateDiscount()
            {

            }

            AddTime(data, TotalTime);

            String InstanceType = data.FirstOrDefault().EC2InstanceType;
            String Region = data.Key.Region;

            double instanceTypeTotalAmount = 0;
            InstancePriceKey instanceTypeRegion = new InstancePriceKey(InstanceType, Region);
            instanceTypeTotalAmount += Math.Ceiling(TotalTime.OnDemand.TotalHours) * instanceTypeChargeMap[instanceTypeRegion].OnDemandCharge;
            instanceTypeTotalAmount += Math.Ceiling(TotalTime.Reserved.TotalHours) * instanceTypeChargeMap[instanceTypeRegion].ReservedCharge;

            total.BillAmount += instanceTypeTotalAmount;

            // Add A Record
            InstanceTypeBill instanceTypeBill = new InstanceTypeBill();
            instanceTypeBill.Region = Region;
            instanceTypeBill.ResourceType = InstanceType;
            instanceTypeBill.TotalResources = data.DistinctBy(x => x.EC2InstanceID).Count();
            instanceTypeBill.TotalUsedTime = TotalTime.OnDemand + TotalTime.Reserved;
            instanceTypeBill.TotalAmount = instanceTypeTotalAmount;

            string customerID = data.ElementAt(0).CustomerID;
            var currDate = data.ElementAt(0).UsedUntil;
            var JoinDate = customers.Where(x => x.CustomerID.Equals(customerID.Substring(0, 4) + "-" + customerID.Substring(4))).ToList().ElementAt(0).JoinDate;
            JoinDate = new DateTime(JoinDate.Year, JoinDate.Month, 1);

            // Apply Discount
            // Checking if Current Date is Within 1 Year Range of Join Date
            double discount = 0;
            if (JoinDate.CompareTo(currDate) <= 0 && currDate.CompareTo(JoinDate.AddYears(1)) < 0) 
            {
                if (regionFreeTierMap[data.Key.Region].Equals(InstanceType))
                {
                    discount += Math.Min(discountBalanceHours.Windows, Math.Ceiling(TotalTime.Windows.TotalHours)) * instanceTypeChargeMap[instanceTypeRegion].OnDemandCharge;
                    discountBalanceHours.Windows = Math.Max(0, discountBalanceHours.Windows - (int)Math.Ceiling(TotalTime.Windows.TotalHours));
                    discount += Math.Min(discountBalanceHours.Linux, Math.Ceiling(TotalTime.Linux.TotalHours)) * instanceTypeChargeMap[instanceTypeRegion].OnDemandCharge;
                    discountBalanceHours.Linux = Math.Max(0, discountBalanceHours.Linux - (int)Math.Ceiling(TotalTime.Linux.TotalHours));
                }
            }
            total.Discount += discount;
            instanceTypeBill.Discount = discount;

            outputManager.BillByInstanceType.Add(instanceTypeBill);
        }

        public void CreateBillFile((double BillAmount, double Discount) total, Dictionary<String, String> customerIdNameMap, IGrouping<dynamic, AWSResourceUsage> currentGroupedByTime, OutputManager outputManager)
        {
            outputManager.CustomerName = customerIdNameMap["CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)];
            outputManager.BillingTime = currentGroupedByTime.ElementAt(0).UsedFrom;
            outputManager.TotalAmount = total.BillAmount;
            outputManager.TotalDiscount = total.Discount;
            outputManager.ActualAmount = total.BillAmount - total.Discount;

            // Generate Bill
            String path = $"../../../Output/{"CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)}_{outputManager.BillingTime.ToString("MMM").ToUpper()}-{currentGroupedByTime.Key.Year}.csv";
            File.WriteAllText(path, outputManager.GenerateBill());
        }

        public void GenerateCustomerBillsMonthly(List<AWSResourceTypes> resourceTypes, List<Customer> customerList, List<AWSResourceUsage> onDemandResourceUsages, List<AWSResourceUsage> reservedInstanceUsages, Dictionary<string, string> regionFreeTierMap)
        {
            var instanceTypeChargeMap = resourceTypes.ToDictionary(key => new InstancePriceKey(key.InstanceType, key.Region), value => new InstancePriceValue(value.OnDemandCharge, value.ReservedCharge));
            var customerIdNameMap = customerList.ToDictionary(key => key.CustomerID, value => value.CustomerName);
            var monthlyRecords = GetMonthlyUsages(reservedInstanceUsages, onDemandResourceUsages, customerList);
            var groupedByTime = monthlyRecords.GroupBy(x => new { x.CustomerID, x.UsedFrom.Year, x.UsedFrom.Month });

            foreach (var currentGroupedByTime in groupedByTime)
            {
                Console.WriteLine($"{currentGroupedByTime.Key}");

                var groupedByType = currentGroupedByTime.GroupBy(x => new { x.EC2InstanceType, x.Region });

                (double BillAmount, double Discount) total = (0, 0);
                (int Linux, int Windows) discountBalanceHours = (750, 750);
                OutputManager outputManager = new OutputManager();

                foreach (var data in groupedByType)
                {
                    Console.WriteLine(data.Key);
                    AddAllRecordsOfSameInstanceType(ref discountBalanceHours, data, ref total, instanceTypeChargeMap, outputManager, customerList, regionFreeTierMap);
                }

                if (total.BillAmount > 0)
                {
                    CreateBillFile(total, customerIdNameMap, currentGroupedByTime, outputManager);
                }
                Console.WriteLine();
            }
        }
    }
}
