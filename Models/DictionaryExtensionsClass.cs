using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingSystem.Models
{
    public static class DictionaryExtensionsClass
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,TKey otherKey)
        {
            foreach (var key in dictionary.Keys)
            {
                if( key.ToString().Equals(otherKey.ToString()) ) 
                    return dictionary[key];            
            }
            return dictionary.GetValueOrDefault(otherKey);
        }
    }
}
