using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class UserFoodItemViewModel
    {
        public required UserFoodItem FoodItemDetails { get; set; }
        public List<DateOnly>? ExpirationDate { get; set; }
        public List<float>? Amounts { get; set; }
        public List<string>? Units { get; set; }
        public required string FoodItemName { get; set; }
        public required string CategoryName { get; set; }
    }
}