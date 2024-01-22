using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem
{
    public class AWSResourceTypes
    {
        public String AWSResourceID { get; set; }
        public String InstanceType { get; set; }
        public double Charge { get; set; }


        override
        public String ToString()
        {
            return $"{AWSResourceID} {InstanceType} {Charge}";
        }
    }



}
