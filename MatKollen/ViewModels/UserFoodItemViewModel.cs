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
        public required List<UserFoodDetailsViewModel> UserFoodItems { get; set; }
        public decimal SumOfQuantities 
        { 
            get 
            {
                return UserFoodItems?.Sum(q => q.FoodDetails.Quantity) ?? 0;
            }
        }

        public string StatusLevel()
        {
            int daysinWeek = 7;
            int threeDays = 3;

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            foreach (var item in UserFoodItems)
            {
                DateTime expirationDateTime = item.FoodDetails.ExpirationDate.ToDateTime(TimeOnly.MinValue);
                int daysUntilExpiration = (expirationDateTime - today.ToDateTime(TimeOnly.MinValue)).Days;

                if (item.FoodDetails.Quantity <= 0) return "out";
                if (item.FoodDetails.ExpirationDate == new DateOnly()) return "transparent";

                if (daysUntilExpiration < threeDays) return "critical";
                if (daysUntilExpiration < daysinWeek) return "attention";
            }
            return "default";
        }
    }
}