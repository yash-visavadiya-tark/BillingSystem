using System.Dynamic;
using System.Text;

namespace BillingSystem
{
    internal class BillingApp
    {
        static void Main(string[] args)
        {
            BillingInputManager inputManager = new BillingInputManager();

            List<AWSResourceTypes> resourceTypes = inputManager.GetAWSResourceTypes();
            Dictionary<String, double> instanceTypeChargeMap = resourceTypes.ToDictionary(key => key.InstanceType, value => value.Charge);

            List<Customer> customerList = inputManager.GetCustomers();
            Dictionary<String, String> customerIdNameMap = customerList.ToDictionary(key => key.CustomerID, value => value.CustomerName);

            List<AWSResourceUsage> resourceUsages = inputManager.GetAWSResourceUsages();

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

            var groupedByTime = from item in monthlyRecords
                                 group item by new
                                 {
                                     item.CustomerID,
                                     item.UsedFrom.Year,
                                     item.UsedFrom.Month,
                                 }
                                 into g
                                 select g;

            foreach (var currentGroupedByTime in groupedByTime)
            {
                //Console.WriteLine($"{currentFilteredByTime.Key}");

                var groupedByType = from item in currentGroupedByTime
                                     group item by item.EC2InstanceType;

                List<String> instanceTypeRecords = new List<String>();
                double totalBillAmount = 0;

                foreach (var data in groupedByType)
                {
                    AddAllRecordsOfSameInstanceType(data, instanceTypeRecords, ref totalBillAmount, instanceTypeChargeMap);
                }

                CreateBillFile(totalBillAmount, instanceTypeRecords, customerIdNameMap, currentGroupedByTime);

                //Console.WriteLine();
            }
        }

        public static void AddNewRecordsToMakeResourceUsageMonthly(AWSResourceUsage resourceUsage, List<AWSResourceUsage> monthlyRecords)
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

        public static void AddAllRecordsOfSameInstanceType(IGrouping<string, AWSResourceUsage> data, List<String> instanceTypeRecords, ref double totalBillAmount, Dictionary<String, double> instanceTypeChargeMap)
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
            instanceTypeRecords.Add($"{InstaceType},{uniqueInstanceSet.Count()},{BillingTotalUsedTime(totalTime)},{BillingTotalBilledTime(totalTime)},${instanceTypeChargeMap[InstaceType]},${instanceTypeTotalAmount:0.0000}");

        }

        public static void CreateBillFile(double totalBillAmount, List<String> instanceTypeRecords, Dictionary<String, String> customerIdNameMap, IGrouping<dynamic, AWSResourceUsage> currentGroupedByTime)
        {
            String CustomerName = customerIdNameMap["CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)];
            DateTime BillDate = currentGroupedByTime.ElementAt(0).UsedUntil;

            StringBuilder bill = new StringBuilder();
            bill.AppendLine(CustomerName);
            bill.AppendLine($"Bill for month of {BillDate.ToString("MMMM")} {BillDate.ToString("yyyy")}");
            bill.AppendLine($"Total Amount: ${totalBillAmount:0.0000}");
            bill.AppendLine("Resource Type,Total Resources,Total Used Time (HH:mm:ss),Total Billed Time (HH:mm:ss),Rate (per hour),Total Amount");
            foreach (var record in instanceTypeRecords)
            {
                bill.AppendLine(record);
            }

            // Generate Bill
            File.WriteAllText($"../../../Output/{"CUST-" + currentGroupedByTime.Key.CustomerID.Substring(4)}_{BillDate.ToString("MMM").ToUpper()}-{currentGroupedByTime.Key.Year}.csv", bill.ToString());
        }

        public static String BillingTotalUsedTime(TimeSpan time)
        {
            return $"{Math.Floor(time.TotalHours)}:{time.Minutes}:{time.Seconds}";
        }

        public static String BillingTotalBilledTime(TimeSpan time)
        {
            return $"{Math.Ceiling(time.TotalHours)}:{"00"}:{"00"}";
        }
    }
}
