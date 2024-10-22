using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class UserFoodItemViewModel
    {
        public required string FoodItemName { get; set; }
        public string? CategoryName { get; set; }
        public List<(double ConvertedQuantity, UserFoodItem FoodDetails, MeasurementUnit UnitInfo)>? UserFoodItems { get; set; }
        public double SumOfQuantities 
        { 
            get 
            {
                return UserFoodItems?.Sum(q => q.FoodDetails.Quantity) ?? 0;
            }
        }

        public string CriticalLevel()
        {
            int daysinWeek = 7;
            int threeDays = 3;

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            foreach (var item in UserFoodItems)
            {
                DateTime expirationDateTime = item.FoodDetails.ExpirationDate.ToDateTime(TimeOnly.MinValue);
                int daysUntilExpiration = (expirationDateTime - today.ToDateTime(TimeOnly.MinValue)).Days;

                if (item.FoodDetails.Quantity <= 0) return "out";

                if (daysUntilExpiration < threeDays) return "critical";
                if (daysUntilExpiration < daysinWeek) return "attention";
            }
            return "default";
        }
    }
}




// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using MatKollen.Models;

// namespace MatKollen.ViewModels
// {
//     public class UserFoodItemViewModel
//     {
//         public UserFoodItem? FoodItemDetails { get; set; }
//         public List<DateOnly>? ExpirationDate { get; set; }
//         public List<(double ConvertedQuantity, double Quantity)> Quantities { get; set; }
//         public double SumOfQuantities { get; set;}
//         public List<MeasurementUnit>? Units { get; set; }
//         public required string FoodItemName { get; set; }
//         public string? CategoryName { get; set; }

//         public string CriticalLevel()
//         {
//             // Handle cases where the expiration date list is null or empty
//             if (ExpirationDate == null || !ExpirationDate.Any())
//             {
//                 return "default"; // Or any default value you prefer
//             }

//             int daysinWeek = 7;
//             int threeDays = 3;

//             DateOnly today = DateOnly.FromDateTime(DateTime.Now);


//             foreach (var date in ExpirationDate)
//             {
//                 DateTime expirationDateTime = date.ToDateTime(TimeOnly.MinValue);
//                 int daysUntilExpiration = (expirationDateTime - today.ToDateTime(TimeOnly.MinValue)).Days;

//                 if (daysUntilExpiration < threeDays) return "critical";
//                 if (daysUntilExpiration < daysinWeek) return "attention";
//             }
//             return "default";
//         }
//     }
// }