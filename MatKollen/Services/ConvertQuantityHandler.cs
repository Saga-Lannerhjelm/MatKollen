using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Services
{
    public class ConvertQuantityHandler
    {
        public decimal ConvertFromLiterOrKg(decimal quanity, double conversionMultiplier)
        {
            decimal conversionMultiplierDecimal = Convert.ToDecimal(conversionMultiplier);
            var convertedValue = quanity * conversionMultiplierDecimal;
            var convertedValueString = convertedValue.ToString(CultureInfo.InvariantCulture);
            // The method vill return an int if the number after the dot it 0 
            if (convertedValueString.Substring(convertedValueString.IndexOf(".") + 1, 1) == "0")
            {
                return (int)Math.Round(quanity * conversionMultiplierDecimal);
            }
            return quanity * conversionMultiplierDecimal;
        }

        public decimal ConverToLiterOrKg(decimal quanity, double conversionMultiplier)
        {
            decimal conversionMultiplierDecimal = Convert.ToDecimal(conversionMultiplier);
            return quanity * Math.Round(1 / conversionMultiplierDecimal, 3);
        }
    }
}