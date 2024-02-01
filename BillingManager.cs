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

        private Customer GetCustomer(string customerID, List<Customer> customerList)
        {
            return customerList.Where(x => x.CustomerID.Equals(customerID.Substring(0, 4) + "-" + customerID.Substring(4))).ToList().First();
        }

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
                var customer = customerList.Where(x => x.CustomerID.Equals(item.CustomerID.Substring(0, 4) + "-" + item.CustomerID.Substring(4))).ToList().First();
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

        private UsageTimeInfo GetUsageTime(IGrouping<dynamic, AWSResourceUsage> data)
        {
            UsageTimeInfo TotalTime = new UsageTimeInfo();
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
            return TotalTime;
        }

        private double GetTotalAmount(InstancePriceValue instancePrice, UsageTimeInfo TotalTime)
        {
            double totalAmount = 0;
            totalAmount += Math.Ceiling(TotalTime.OnDemand.TotalHours) * instancePrice.OnDemandCharge;
            totalAmount += Math.Ceiling(TotalTime.Reserved.TotalHours) * instancePrice.ReservedCharge;
            return totalAmount;
        }

        // This function gives customer Joined Month first day
        private DateTime GetJoinDateOfCustomer(List<Customer> customerList, string customerID)
        {
            var JoinDate = customerList.Where(x => x.CustomerID.Equals(customerID.Substring(0, 4) + "-" + customerID.Substring(4))).ToList().First().JoinDate;
            return new DateTime(JoinDate.Year, JoinDate.Month, 1);
        }

        private double GetTotalDiscount(UsageTimeInfo TotalTime, DiscountBalance discountBalance, AWSResourceUsage resourceUsage, InstancePriceValue instancePrice, Customer customer, string freeInstanceType)
        {
            double totalDiscount = 0;

            string customerID = resourceUsage.CustomerID;
            var currDate = resourceUsage.UsedUntil;
            var JoinDate = new DateTime(customer.JoinDate.Year, customer.JoinDate.Month, 1); // for discount, count from start of the month

            // Checking if Current Date is Within 1 Year Range of Join Date
            if (JoinDate.CompareTo(currDate) <= 0 && currDate.CompareTo(JoinDate.AddYears(1)) <= 0)
            {
                if (freeInstanceType.Equals(resourceUsage.EC2InstanceType))
                {
                    totalDiscount += Math.Min(discountBalance.Windows, Math.Ceiling(TotalTime.Windows.TotalHours)) * instancePrice.OnDemandCharge;
                    discountBalance.Windows = Math.Max(0, discountBalance.Windows - (int)Math.Ceiling(TotalTime.Windows.TotalHours));

                    totalDiscount += Math.Min(discountBalance.Linux, Math.Ceiling(TotalTime.Linux.TotalHours)) * instancePrice.OnDemandCharge;
                    discountBalance.Linux = Math.Max(0, discountBalance.Linux - (int)Math.Ceiling(TotalTime.Linux.TotalHours));
                }
            }
            return totalDiscount;
        }

        private ChargeDetails GetCharge(AWSResourceUsage resourceUsage, Customer customer, string freeInstanceType, InstancePriceValue instancePrice, UsageTimeInfo totalTime, DiscountBalance discountBalance)
        {
            ChargeDetails charge = new ChargeDetails();
            InstancePriceKey instanceTypeRegion = new InstancePriceKey(instanceType: resourceUsage.EC2InstanceType, region: resourceUsage.Region);

            charge.TotalAmount = GetTotalAmount(instancePrice: instancePrice, totalTime);

            charge.TotalDiscount = GetTotalDiscount(totalTime, discountBalance, resourceUsage: resourceUsage, instancePrice: instancePrice, customer: customer, freeInstanceType: freeInstanceType);

            return charge;
        }

        public void AddAllRecordsOfSameInstanceType(DiscountBalance discountBalance, IGrouping<dynamic, AWSResourceUsage> data, ChargeDetails total, Dictionary<InstancePriceKey, InstancePriceValue> instanceTypeChargeMap, OutputManager outputManager, List<Customer> customers, Dictionary<string, string> regionFreeTierMap)
        {
            UsageTimeInfo TotalTime = GetUsageTime(data);

            AWSResourceUsage resourceUsage = data.First();
            Customer customer = GetCustomer(resourceUsage.CustomerID, customerList: customers);
            InstancePriceValue instancePrice = DictionaryExtensionsClass.GetValueOrDefault(instanceTypeChargeMap, new InstancePriceKey(instanceType: resourceUsage.EC2InstanceType, region: resourceUsage.Region));
            string freeInstanceType = regionFreeTierMap[resourceUsage.Region];

            ChargeDetails charge = GetCharge(resourceUsage: resourceUsage, customer: customer, freeInstanceType: freeInstanceType, instancePrice: instancePrice, totalTime: TotalTime, discountBalance: discountBalance);

            total.TotalAmount += charge.TotalAmount;
            total.TotalDiscount += charge.TotalDiscount;

            Console.WriteLine(total + " " + TotalTime);

            InstanceTypeBill instanceTypeBill = new InstanceTypeBill(region: resourceUsage.Region, resourceType: resourceUsage.EC2InstanceType, totalResources: data.DistinctBy(x => x.EC2InstanceID).Count(), totalUsedTime: TotalTime.OnDemand + TotalTime.Reserved, charge: charge);

            outputManager.BillByInstanceType.Add(instanceTypeBill);
        }

        public void CreateBillFile(ChargeDetails total, Dictionary<String, String> customerIdNameMap, IGrouping<dynamic, AWSResourceUsage> currentGroupedByTime, OutputManager outputManager)
        {
            outputManager.CustomerName = customerIdNameMap["CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)];
            outputManager.BillingTime = currentGroupedByTime.First().UsedFrom;
            outputManager.TotalAmount = total.TotalAmount;
            outputManager.TotalDiscount = total.TotalDiscount;
            outputManager.ActualAmount = total.TotalAmount - total.TotalDiscount;

            // Generate Bill
            String path = $"../../../Enhancement-1/Output/{"CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)}_{outputManager.BillingTime.ToString("MMM").ToUpper()}-{currentGroupedByTime.Key.Year}.csv";
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

                ChargeDetails total = new ChargeDetails();
                DiscountBalance discountBalance = new DiscountBalance(750, 750);
                OutputManager outputManager = new OutputManager();

                foreach (var data in groupedByType)
                {
                    Console.WriteLine(data.Key);
                    AddAllRecordsOfSameInstanceType(discountBalance, data, total, instanceTypeChargeMap, outputManager, customerList, regionFreeTierMap);
                }

                if (total.TotalAmount > 0)
                {
                    CreateBillFile(total, customerIdNameMap, currentGroupedByTime, outputManager);
                }
                Console.WriteLine();
            }
        }
    }
}
