using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Services
{
    public class ConvertQuantity
    {
        public double ConverFromtLiterOrKg(double quanity, double conversionMultiplier)
        {
            var convertedValue = quanity * conversionMultiplier;
            var convertedValueString = convertedValue.ToString(CultureInfo.InvariantCulture);
            // The method vill return an int if the number after the dot it 0 
            if (convertedValueString.Substring(convertedValueString.IndexOf(".") + 1, 1) == "0")
            {
                return (int)Math.Round(quanity * conversionMultiplier);
            }
            return quanity * conversionMultiplier;
        }

        public double ConverToLiterOrKg(double quanity, double conversionMultiplier)
        {
            return quanity * Math.Round(1 / conversionMultiplier, 3);
        }
    }
}