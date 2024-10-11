using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.ViewModels
{
    public class UserFoodItemViewModel
    {
        public List<DateOnly>? ExpirationDate { get; set; }
        public required string FoodItem { get; set; }
        public required string Category { get; set; }
    }
}