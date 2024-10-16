using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class UserFoodItemViewModel
    {
        public UserFoodItem? FoodItemDetails { get; set; }
        public List<DateOnly>? ExpirationDate { get; set; }
        public List<double>? Quantities { get; set; }
        public List<string>? Units { get; set; }
        public required string FoodItemName { get; set; }
        public string? CategoryName { get; set; }
    }
}