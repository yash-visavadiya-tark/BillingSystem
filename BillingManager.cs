using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
            Dictionary<(string InstanceType, string Region), (double OnDemandCharge, double ReservedCharge)> instanceTypeChargeMap = resourceTypes.ToDictionary(key => (key.InstanceType, key.Region), value => (value.OnDemandCharge, value.ReservedCharge));

            Dictionary<String, String> customerIdNameMap = customerList.ToDictionary(key => key.CustomerID, value => value.CustomerName);
            var monthlyRecords = new List<AWSResourceUsage>();

            foreach (var item in onDemandResourceUsages)
            {
                var customer = customerList.Where(x => x.CustomerID.Equals(item.CustomerID.Substring(0, 4) + "-" + item.CustomerID.Substring(4))).ToList().ElementAt(0);
                customer.JoinDate = customer.JoinDate.CompareTo(item.UsedFrom) <= 0 ? customer.JoinDate : item.UsedFrom;

                AddNewRecordsToMakeResourceUsageMonthly(item, monthlyRecords);
            }

            foreach (var item in reservedInstanceUsages)
            {
                var customer = customerList.Where(x => x.CustomerID.Equals(item.CustomerID.Substring(0, 4) + "-" + item.CustomerID.Substring(4)));
                foreach (var c in customer)
                {
                    c.JoinDate = c.JoinDate.CompareTo(item.UsedFrom) <= 0 ? c.JoinDate : item.UsedFrom;
                }
                AddNewRecordsToMakeResourceUsageMonthly(item, monthlyRecords);
            }
            // Now we use filteredForMonthlyRecords instead of awsResourceUsages.
            // So, Cleared awsResourceUsages.
            onDemandResourceUsages.Clear();
            var groupedByTime = monthlyRecords.GroupBy(x => new { x.CustomerID, x.UsedFrom.Year, x.UsedFrom.Month});

            foreach (var currentGroupedByTime in groupedByTime)
            {
                Console.WriteLine($"{currentGroupedByTime.Key}");

                var groupedByType = currentGroupedByTime.GroupBy(x => new { x.EC2InstanceType, x.Region });

                (double BillAmount, double Discount) total = (0, 0);
                (int Linux, int Windows)discountBalanceHours = (750, 750);
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

        public void AddNewRecordsToMakeResourceUsageMonthly(AWSResourceUsage resourceUsage, List<AWSResourceUsage> monthlyRecords)
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

        public void AddAllRecordsOfSameInstanceType(ref (int Linux, int Windows) discountBalanceHours,IGrouping<dynamic, AWSResourceUsage> data, ref (double BillAmount, double Discount) total, Dictionary<(string InstanceType,string Region),  (double OnDemandCharge, double ReservedCharge)> instanceTypeChargeMap, OutputManager outputManager, List<Customer> customers, Dictionary<string, string> regionFreeTierMap)
        {
            HashSet<String> uniqueInstanceSet = new HashSet<String>();
            (TimeSpan OnDemand, TimeSpan Reserved, TimeSpan Linux, TimeSpan Windows) TotalTime = (new TimeSpan(), new TimeSpan(), new TimeSpan(), new TimeSpan());
            
            foreach (var item in data)
            {
                uniqueInstanceSet.Add(item.EC2InstanceID);
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
            //Console.WriteLine();

            String InstanceType = data.ElementAt(0).EC2InstanceType;
            String Region = data.Key.Region;

            double instanceTypeTotalAmount = Math.Ceiling(TotalTime.OnDemand.TotalHours) * instanceTypeChargeMap[(InstanceType, Region)].OnDemandCharge;
            instanceTypeTotalAmount += Math.Ceiling(TotalTime.Reserved.TotalHours) * instanceTypeChargeMap[(InstanceType, Region)].ReservedCharge;

            total.BillAmount += instanceTypeTotalAmount;

            // Add A Record
            InstanceTypeBill instanceTypeBill = new InstanceTypeBill();
            instanceTypeBill.Region = Region;
            instanceTypeBill.ResourceType = InstanceType;
            instanceTypeBill.TotalResources = uniqueInstanceSet.Count();
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
                    discount += Math.Min(discountBalanceHours.Windows, Math.Ceiling(TotalTime.Windows.TotalHours)) * instanceTypeChargeMap[(InstanceType, Region)].OnDemandCharge;
                    discountBalanceHours.Windows = Math.Max(0, discountBalanceHours.Windows - (int)Math.Ceiling(TotalTime.Windows.TotalHours));
                    discount += Math.Min(discountBalanceHours.Linux, Math.Ceiling(TotalTime.Linux.TotalHours)) * instanceTypeChargeMap[(InstanceType, Region)].OnDemandCharge;
                    discountBalanceHours.Linux = Math.Max(0, discountBalanceHours.Linux - (int)Math.Ceiling(TotalTime.Linux.TotalHours));
                }
            }
            total.Discount += discount;
            instanceTypeBill.Discount = discount;

            Console.WriteLine($"{total.BillAmount : 0.0000} {total.Discount : 0.0000} {TotalTime}");
            Console.WriteLine();


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
    }
}
