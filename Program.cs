using System.Dynamic;
using System.Text;

namespace BillingSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InputManager inputManager = new InputManager();

            List<AWSResourceTypes> awsResourceTypes = inputManager.GetAWSResourceTypes();
            Dictionary<String, double> resourceTypeMap = awsResourceTypes.ToDictionary(key => key.InstanceType, value => value.Charge);

            List<Customer> customers = inputManager.GetCustomers();
            Dictionary<String, String> customerMap = customers.ToDictionary(key => key.CustomerID, value => value.CustomerName);

            List<AWSResourceUsage> awsResourceUsages = inputManager.GetAWSResourceUsages();

            var filteredForMonthlyRecords = new List<AWSResourceUsage>();

            foreach (var item in awsResourceUsages)
            {
                AddNewRecordsToMakeResourceUsageMonthly(item, filteredForMonthlyRecords);

                // Add The Last One which does resides in range of month or default one if not exceeded months
                filteredForMonthlyRecords.Add(item);
            }

            // Now we use filteredForMonthlyRecords instead of awsResourceUsages.
            // So, Cleared awsResourceUsages.
            awsResourceUsages.Clear();

            var filteredByTime = from item in filteredForMonthlyRecords
                                 group item by new
                                 {
                                     item.CustomerID,
                                     item.UsedFrom.Year,
                                     item.UsedFrom.Month,
                                 }
                                 into g
                                 select g;

            foreach (var currentFilteredByTime in filteredByTime)
            {
                //Console.WriteLine($"{currentFilteredByTime.Key}");

                var filteredByType = from item in currentFilteredByTime
                                     group item by item.EC2InstanceType;

                List<String> records = new List<String>();
                double totalAmount = 0;

                foreach (var data in filteredByType)
                {
                    AddAllRecordsOfSameInstanceType(data, records, ref totalAmount, resourceTypeMap);
                }

                GenerateBill(totalAmount, records, customerMap, currentFilteredByTime);

                //Console.WriteLine();
            }
        }

        public static void AddNewRecordsToMakeResourceUsageMonthly(AWSResourceUsage awsResourceUsage, List<AWSResourceUsage> filteredForMonthlyRecords)
        {
            DateTime start = awsResourceUsage.UsedFrom;
            DateTime end = awsResourceUsage.UsedUntil;

            double totalDiff = (end - start).TotalSeconds;
            DateTime last = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month), 23, 59, 59);

            while ((last - start).TotalSeconds < totalDiff)
            {
                awsResourceUsage.UsedFrom = start;
                awsResourceUsage.UsedUntil = last;

                AWSResourceUsage newRecord = new AWSResourceUsage(awsResourceUsage);
                filteredForMonthlyRecords.Add(newRecord);

                start = last.AddSeconds(1);
                last = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month), 23, 59, 59);
                totalDiff = (end - start).TotalSeconds;
            }
            awsResourceUsage.UsedFrom = start;
            awsResourceUsage.UsedUntil = end;
        }

        public static void AddAllRecordsOfSameInstanceType(IGrouping<string, AWSResourceUsage> data, List<String> records, ref double totalAmount, Dictionary<String, double> resourceTypeMap)
        {
            HashSet<String> UniqueInstances = new HashSet<String>();
            TimeSpan totalTime = new TimeSpan();



            foreach (var item in data)
            {
                UniqueInstances.Add(item.EC2InstanceID);
                totalTime += item.activeTime;
                //Console.WriteLine(item);
            }

            String InstaceType = data.ElementAt(0).EC2InstanceType;
            double totalAmountPerType = Math.Ceiling(totalTime.TotalHours) * resourceTypeMap[InstaceType];

            // Add A Record
            records.Add($"{InstaceType},{UniqueInstances.Count()},{TotalUsedTime(totalTime)},{TotalBilledTime(totalTime)},${resourceTypeMap[InstaceType]},${totalAmountPerType:0.0000}");

            totalAmount += totalAmountPerType;
        }

        public static void GenerateBill(double totalAmount, List<String> records, Dictionary<String, String> customerMap, IGrouping<dynamic, AWSResourceUsage> currentFilteredByTime)
        {
            String CustomerName = customerMap["CUST-" + currentFilteredByTime.Key.CustomerID.Substring(4)];
            DateTime BillDate = currentFilteredByTime.ElementAt(0).UsedUntil;

            StringBuilder bill = new StringBuilder();
            bill.AppendLine(CustomerName);
            bill.AppendLine($"Bill for month of {BillDate.ToString("MMMM")} {BillDate.ToString("yyyy")}");
            bill.AppendLine($"Total Amount: ${totalAmount:0.0000}");
            bill.AppendLine("Resource Type,Total Resources,Total Used Time (HH:mm:ss),Total Billed Time (HH:mm:ss),Rate (per hour),Total Amount");
            foreach (var record in records)
            {
                bill.AppendLine(record);
            }

            // Generate Bill
            File.WriteAllText($"../../../{"CUST-" + currentFilteredByTime.Key.CustomerID.Substring(4)}_{BillDate.ToString("MMM").ToUpper()}-{currentFilteredByTime.Key.Year}.csv", bill.ToString());
        }

        public static String TotalUsedTime(TimeSpan time)
        {
            return $"{Math.Floor(time.TotalHours)}:{time.Minutes}:{time.Seconds}";
        }

        public static String TotalBilledTime(TimeSpan time)
        {
            return $"{Math.Ceiling(time.TotalHours)}:{"00"}:{"00"}";
        }
    }
}
